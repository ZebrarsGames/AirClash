// Мини-версия AirClash: играбельный аэрохоккей против ИИ прямо в браузере.
// Демонстрирует систему сложности ботов, описанную в самой игре.
(function () {
    const canvas = document.getElementById("game-canvas");
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) {
        console.error("AirClash mini-match requires a 2D canvas context.");
        return;
    }

    // Логические размеры поля (не зависят от CSS-масштаба канваса)
    const W = 700;
    const H = 400;
    let pixelRatio = 0;
    let arenaCanvas;

    const GOAL_HALF = 60;
    const MALLET_R = 22;
    const PUCK_R = 12;
    const WIN_SCORE = 7;

    // Параметры сложности — аналог PlayerPrefs "Difficulty" / "BotOffsetX" / "BotOffsetY"
    // из оригинального BotsAI.cs. moveSpeed — базовая скорость (px/кадр),
    // offsetX/offsetY — насколько неточно бот занимает защитную позицию,
    // puckKoof — угол атаки при подъезде к шайбе (аналог PuckKoof.y).
    const DIFFICULTIES = {
        easy: { label: "Лёгкий", moveSpeed: 3.4, offsetX: 48, offsetY: 60, puckKoof: 28 },
        medium: { label: "Средний", moveSpeed: 4.6, offsetX: 34, offsetY: 44, puckKoof: 20 },
        hard: { label: "Сложный", moveSpeed: 6.4, offsetX: 20, offsetY: 26, puckKoof: 11 },
        extreme: { label: "Экстрим", moveSpeed: 8.6, offsetX: 9, offsetY: 11, puckKoof: 4 },
    };

    let currentDifficulty = "medium";

    const BOT_START = { x: W * 0.82, y: H * 0.5 };
    const restartBtn = document.getElementById("restart-match");
    const timerOverlay = document.getElementById("timer-overlay");
    const timerText = document.getElementById("timer-overlay-text");
    const scoreYou = document.getElementById("score-you");
    const scoreBot = document.getElementById("score-bot");
    const matchStatus = document.getElementById("match-status");
    const difficultyButtons = document.querySelectorAll(".diff-btn");
    const soundToggle = document.getElementById("sound-toggle");
    const winsOutput = document.getElementById("wins-count");
    const lossesOutput = document.getElementById("losses-count");
    const streakOutput = document.getElementById("win-streak");
    const STATS_KEY = "airclash-mini-match-stats";
    const SOUND_KEY = "airclash-mini-match-muted";
    const pressedKeys = new Set();
    let isMuted = false;

    function readStoredValue(key, fallback) {
        try {
            const value = window.localStorage.getItem(key);
            return value === null ? fallback : value;
        } catch (error) {
            console.error(`AirClash could not read local setting "${key}".`, error);
            return fallback;
        }
    }

    function writeStoredValue(key, value) {
        try {
            window.localStorage.setItem(key, value);
        } catch (error) {
            console.error(`AirClash could not save local setting "${key}".`, error);
        }
    }

    function readStats() {
        try {
            const storedStats = window.localStorage.getItem(STATS_KEY);
            const stats = storedStats ? JSON.parse(storedStats) : {};
            return {
                wins: Number.isSafeInteger(stats.wins) && stats.wins >= 0 ? stats.wins : 0,
                losses: Number.isSafeInteger(stats.losses) && stats.losses >= 0 ? stats.losses : 0,
                streak: Number.isSafeInteger(stats.streak) && stats.streak >= 0 ? stats.streak : 0,
            };
        } catch (error) {
            console.error("AirClash could not read mini-match statistics.", error);
            return { wins: 0, losses: 0, streak: 0 };
        }
    }

    const stats = readStats();

    function updateStatsDisplay() {
        if (winsOutput) winsOutput.textContent = String(stats.wins);
        if (lossesOutput) lossesOutput.textContent = String(stats.losses);
        if (streakOutput) streakOutput.textContent = String(stats.streak);
    }

    function recordMatchResult(won) {
        if (won) {
            stats.wins += 1;
            stats.streak += 1;
        } else {
            stats.losses += 1;
            stats.streak = 0;
        }
        writeStoredValue(STATS_KEY, JSON.stringify(stats));
        updateStatsDisplay();
    }

    function updateSoundToggle() {
        if (!soundToggle) return;
        soundToggle.textContent = isMuted ? "Звук: выключен" : "Звук: включён";
        soundToggle.setAttribute("aria-pressed", String(isMuted));
    }

    isMuted = readStoredValue(SOUND_KEY, "false") === "true";
    updateStatsDisplay();
    updateSoundToggle();

    const state = {
        player: { x: W * 0.18, y: H * 0.5, vx: 0, vy: 0 },
        bot: { x: W * 0.82, y: H * 0.5, vx: 0, vy: 0 },
        puck: { x: W * 0.5, y: H * 0.5, vx: 0, vy: 0 },
        scoreYou: 0,
        scoreBot: 0,
        running: true,
        pointerActive: false,
        playerMoved: false,
        gameOver: false,
        botSpeedFactor: 1, // аналог UpdateBotSpeed() — камбэк-механика по разнице счёта
        timerOn: false, // аналог TimerScr.TimerOn — блокирует ИИ и физику на время таймера
    };

    let countdownHandle = null;
    let animationFrameId = null;
    let lastFrameTime = 0;

    function resizeCanvas() {
        const nextPixelRatio = Math.min(window.devicePixelRatio || 1, 2);
        if (nextPixelRatio === pixelRatio) return;

        pixelRatio = nextPixelRatio;
        canvas.width = W * pixelRatio;
        canvas.height = H * pixelRatio;
        ctx.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);

        arenaCanvas = document.createElement("canvas");
        arenaCanvas.width = canvas.width;
        arenaCanvas.height = canvas.height;
        const arenaContext = arenaCanvas.getContext("2d");
        if (!arenaContext) {
            console.error("AirClash mini-match could not create its arena canvas.");
            return;
        }
        arenaContext.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
        drawArena(arenaContext);
    }

    // --- Таймер гола / старта (порт TimerScr.cs) ---
    // Простые синтезированные звуки через Web Audio — заменяют
    // AudioClip[] goalSounds / AudioClip timerSound, для которых нет файлов.
    function playBeep(freq, duration, type, volume) {
        try {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            if (isMuted || !AudioCtx) return;
            if (!window.__airclashAudioCtx) window.__airclashAudioCtx = new AudioCtx();
            const audioCtx = window.__airclashAudioCtx;
            const play = () => {
                const osc = audioCtx.createOscillator();
                const gain = audioCtx.createGain();
                osc.type = type;
                osc.frequency.value = freq;
                gain.gain.value = volume;
                osc.connect(gain).connect(audioCtx.destination);
                osc.start();
                gain.gain.exponentialRampToValueAtTime(0.0001, audioCtx.currentTime + duration);
                osc.stop(audioCtx.currentTime + duration);
            };
            if (audioCtx.state === "suspended") {
                audioCtx.resume().then(play).catch((error) => {
                    console.warn("AirClash audio could not be resumed.", error);
                });
                return;
            }
            play();
        } catch (error) {
            console.warn("AirClash audio is unavailable.", error);
        }
    }

    function showTimerOverlay(text) {
        if (!timerOverlay || !timerText) return;
        timerText.textContent = text;
        timerOverlay.classList.add("is-active");
    }

    function hideTimerOverlay() {
        if (!timerOverlay) return;
        timerOverlay.classList.remove("is-active");
    }

    function setRestartInteractable(interactable) {
        if (restartBtn) restartBtn.disabled = !interactable;
    }

    // Аналог TimerScr.Goal(): показываем "GOAL", блокируем рестарт и игру,
    // проигрываем гол-звук, через 1 секунду запускаем обратный отсчёт.
    function goalOverlay() {
        clearTimeout(countdownHandle);
        clearInterval(countdownHandle);
        state.timerOn = true;
        setRestartInteractable(false);
        showTimerOverlay("GOAL");
        playBeep(200, 0.4, "sawtooth", 0.07);
        state.puck.x = W / 2;
        state.puck.y = H / 2;
        state.puck.vx = 0;
        state.puck.vy = 0;
        countdownHandle = setTimeout(timerStart, 1000);
    }

    // Аналог TimerScr.TimerStart() + Timer(): показываем "4","3","2","1" с интервалом
    // в секунду, затем скрываем таймер, разблокируем рестарт и возобновляем игру.
    function timerStart() {
        let timeLeft = 4;
        state.timerOn = true;
        state.player.vx = 0;
        state.player.vy = 0;
        state.playerMoved = false;
        state.bot.vx = 0;
        state.bot.vy = 0;
        pressedKeys.clear();
        setRestartInteractable(false);
        showTimerOverlay(String(timeLeft));
        playBeep(440, 0.12, "square", 0.05);

        countdownHandle = setInterval(() => {
            timeLeft -= 1;

            if (timeLeft <= 0) {
                clearInterval(countdownHandle);
                countdownHandle = null;
                hideTimerOverlay();
                state.player.vx = 0;
                state.player.vy = 0;
                state.playerMoved = false;
                state.bot.x = BOT_START.x;
                state.bot.y = BOT_START.y;
                state.bot.vx = 0;
                state.bot.vy = 0;
                state.puck.x = W / 2;
                state.puck.y = H / 2;
                state.puck.vx = 0;
                state.puck.vy = 0;
                pressedKeys.clear();
                state.timerOn = false;
                setRestartInteractable(true);
            } else {
                showTimerOverlay(String(timeLeft));
                playBeep(440, 0.12, "square", 0.05);
            }
        }, 1000);
    }

    function resetMatch() {
        clearTimeout(countdownHandle);
        clearInterval(countdownHandle);
        countdownHandle = null;
        state.scoreYou = 0;
        state.scoreBot = 0;
        state.gameOver = false;
        state.botSpeedFactor = 1;
        state.player.x = W * 0.18;
        state.player.y = H * 0.5;
        state.bot.x = W * 0.82;
        state.bot.y = H * 0.5;
        state.puck.x = W / 2;
        state.puck.y = H / 2;
        state.puck.vx = 0;
        state.puck.vy = 0;
        updateScoreboard();
        setStatus(`Матч начат. Уровень бота: <strong>${DIFFICULTIES[currentDifficulty].label}</strong>. Первый до ${WIN_SCORE}. Если начнёшь сильно вести — бот прибавит скорость.`);
        timerStart();
    }

    // Аналог UpdateBotSpeed(score1, score2) из оригинала: бот "злится" и ускоряется,
    // когда сильно проигрывает, и сбавляет обороты, когда сильно выигрывает.
    function updateBotSpeedFactor() {
        const diff = state.scoreBot - state.scoreYou;
        let factor = 1;

        if (diff >= 10) factor = 1 / 3;
        else if (diff >= 7) factor = 1 / 2.5;
        else if (diff >= 5) factor = 1 / 2;
        else if (diff >= 3) factor = 1 / 1.5;
        else if (diff <= -10) factor = 2;
        else if (diff <= -7) factor = 1.7;
        else if (diff <= -5) factor = 1.5;
        else if (diff <= -3) factor = 1.2;

        const prevFactor = state.botSpeedFactor;
        state.botSpeedFactor = factor;

        if (factor !== prevFactor && window.showToast) {
            if (factor > 1) {
                window.showToast("Бот отстаёт и включает камбэк-режим — стал быстрее!", "pink");
            } else if (factor < 1) {
                window.showToast("Бот сильно ведёт и сбавляет скорость, давая тебе шанс.", "gold");
            }
        }
    }

    function updateScoreboard() {
        if (scoreYou) scoreYou.textContent = state.scoreYou;
        if (scoreBot) scoreBot.textContent = state.scoreBot;
    }

    function setStatus(html) {
        if (matchStatus) matchStatus.innerHTML = html;
    }

    // --- Управление игроком (мышь / тач) ---
    function pointerToCanvas(clientX, clientY) {
        const rect = canvas.getBoundingClientRect();
        const scaleX = W / rect.width;
        const scaleY = H / rect.height;
        return {
            x: (clientX - rect.left) * scaleX,
            y: (clientY - rect.top) * scaleY,
        };
    }

    function movePlayerTo(x, y) {
        if (state.timerOn || state.gameOver) return;
        const clampedX = Math.max(MALLET_R, Math.min(x, W / 2 - MALLET_R));
        const clampedY = Math.max(MALLET_R, Math.min(y, H - MALLET_R));
        state.player.vx = clampedX - state.player.x;
        state.player.vy = clampedY - state.player.y;
        state.player.x = clampedX;
        state.player.y = clampedY;
        state.playerMoved = state.player.vx !== 0 || state.player.vy !== 0;
    }

    canvas.addEventListener("pointerdown", (event) => {
        if (!event.isPrimary || state.timerOn || state.gameOver) return;
        canvas.focus({ preventScroll: true });
        canvas.setPointerCapture(event.pointerId);
        state.pointerActive = true;
        const point = pointerToCanvas(event.clientX, event.clientY);
        movePlayerTo(point.x, point.y);
    });

    canvas.addEventListener("pointermove", (event) => {
        if (event.pointerType !== "mouse" && !state.pointerActive) return;
        const point = pointerToCanvas(event.clientX, event.clientY);
        movePlayerTo(point.x, point.y);
    });

    function releasePointer(event) {
        if (!state.pointerActive || !canvas.hasPointerCapture(event.pointerId)) return;
        state.pointerActive = false;
        canvas.releasePointerCapture(event.pointerId);
    }

    canvas.addEventListener("pointerup", releasePointer);
    canvas.addEventListener("pointercancel", releasePointer);
    canvas.addEventListener("lostpointercapture", () => {
        state.pointerActive = false;
    });

    const movementKeys = new Set(["w", "a", "s", "d", "arrowup", "arrowleft", "arrowdown", "arrowright"]);
    canvas.addEventListener("keydown", (event) => {
        const key = event.key.toLowerCase();
        if (!movementKeys.has(key)) return;
        event.preventDefault();
        if (state.timerOn || state.gameOver) return;
        pressedKeys.add(key);
    });
    canvas.addEventListener("keyup", (event) => {
        pressedKeys.delete(event.key.toLowerCase());
    });
    canvas.addEventListener("blur", () => pressedKeys.clear());

    if (soundToggle) {
        soundToggle.addEventListener("click", () => {
            isMuted = !isMuted;
            writeStoredValue(SOUND_KEY, String(isMuted));
            updateSoundToggle();
        });
    }

    // --- ИИ бота (порт логики BotsAI.cs) ---
    // Масштаб: оригинальная арена Unity ~16 юнитов в ширину / ~9 в высоту,
    // наш канвас 700×400, поэтому 1 юнит ≈ 43.75px по X и ≈ 44.4px по Y.
    const KX = (W / 2) / 8;
    const KY = (H / 2) / 4.5;
    const ZONE_SHALLOW = 2 * KX; // puck.position.x > 0 && < 2
    const ZONE_FOUR = 4 * KX; // puck.position.x < -4
    const ZONE_SIX = 6 * KX; // puck.position.x < -6
    const Y_FOUR = 4 * KY; // puck.y > 4 || puck.y < -4
    const Y_THREE_FIVE = 3.5 * KY; // puck.y > 3.5 || puck.y < -3.5

    function moveTowards(obj, targetX, targetY, maxDelta) {
        const dx = targetX - obj.x;
        const dy = targetY - obj.y;
        const distanceSquared = dx * dx + dy * dy;
        if (distanceSquared <= maxDelta * maxDelta || distanceSquared === 0) {
            obj.x = targetX;
            obj.y = targetY;
        } else {
            const dist = Math.sqrt(distanceSquared);
            obj.x += (dx / dist) * maxDelta;
            obj.y += (dy / dist) * maxDelta;
        }
    }

    function updateBot(frameScale) {
        const cfg = DIFFICULTIES[currentDifficulty];
        const moveSpeed = cfg.moveSpeed * state.botSpeedFactor * frameScale;

        // relX > 0 — шайба на нашей половине (у ворот бота), relX < 0 — у игрока.
        // Это зеркало условий оригинала, где x < 0 означало "своя половина".
        const relX = state.puck.x - W / 2;
        const dy = state.puck.y - H / 2;

        let targetX;
        let targetY;
        let speed;

        if (relX < 0 && relX > -ZONE_SHALLOW) {
            // a) шайба едва перешла к игроку — не бежим за ней, а перестраиваемся
            // в защитную позицию (зеркальное отражение + отступ назад + разброс по Y)
            const randomSign = Math.random() > 0.5 ? 1 : -1;
            targetX = W - state.puck.x + cfg.offsetX;
            targetY = state.puck.y + randomSign * cfg.offsetY;
            speed = moveSpeed / 4;
        } else if (relX <= -ZONE_SHALLOW) {
            // b) шайба глубоко на половине игрока — спешить некуда, едем в старт
            targetX = BOT_START.x;
            targetY = BOT_START.y;
            speed = moveSpeed / 3;
        } else if (relX > 0 && Math.abs(dy) > Y_FOUR) {
            // c) шайба у нас, но далеко от центра по вертикали — откатываемся в старт
            targetX = BOT_START.x;
            targetY = BOT_START.y;
            speed = moveSpeed / 4;
        } else if (relX > ZONE_SIX && Math.abs(dy) > Y_THREE_FIVE) {
            // d) то же самое у самых наших ворот
            targetX = BOT_START.x;
            targetY = BOT_START.y;
            speed = moveSpeed / 4;
        } else if (relX > ZONE_SIX) {
            // e) шайба почти у наших ворот — летим напрямую, без хитростей
            targetX = state.puck.x;
            targetY = state.puck.y;
            speed = moveSpeed * 1.5;
        } else if (relX > ZONE_FOUR) {
            // f) шайба приближается — подъезжаем под углом, а не в лоб,
            // чтобы отбить её к центру, а не просто остановить
            targetX = state.puck.x;
            targetY = state.puck.y - Math.sign(dy || 1) * cfg.puckKoof;
            speed = moveSpeed;
        } else {
            // g) шайба в средней зоне нашей половины — та же логика подъезда под углом
            targetX = state.puck.x;
            targetY = state.puck.y - Math.sign(dy || 1) * cfg.puckKoof;
            speed = moveSpeed;
        }

        targetX = Math.max(W / 2 + MALLET_R, Math.min(targetX, W - MALLET_R));
        targetY = Math.max(MALLET_R, Math.min(targetY, H - MALLET_R));

        const prevX = state.bot.x;
        const prevY = state.bot.y;

        moveTowards(state.bot, targetX, targetY, speed);

        state.bot.x = Math.max(W / 2 + MALLET_R, Math.min(state.bot.x, W - MALLET_R));
        state.bot.y = Math.max(MALLET_R, Math.min(state.bot.y, H - MALLET_R));

        state.bot.vx = state.bot.x - prevX;
        state.bot.vy = state.bot.y - prevY;
    }

    // --- Физика шайбы ---
    function resolveCollision(mallet) {
        const dx = state.puck.x - mallet.x;
        const dy = state.puck.y - mallet.y;
        const distanceSquared = dx * dx + dy * dy;
        const minDist = MALLET_R + PUCK_R;

        if (distanceSquared < minDist * minDist) {
            const dist = Math.sqrt(distanceSquared);
            const nx = dist > 0 ? dx / dist : 1;
            const ny = dist > 0 ? dy / dist : 0;

            state.puck.x = mallet.x + nx * minDist;
            state.puck.y = mallet.y + ny * minDist;

            const relVx = state.puck.vx - mallet.vx;
            const relVy = state.puck.vy - mallet.vy;
            const dot = relVx * nx + relVy * ny;

            if (dot < 0) {
                state.puck.vx -= 2 * dot * nx;
                state.puck.vy -= 2 * dot * ny;
            }

            state.puck.vx += mallet.vx * 0.5;
            state.puck.vy += mallet.vy * 0.5;

            const speedSquared = state.puck.vx ** 2 + state.puck.vy ** 2;
            const maxSpeed = 14;
            if (speedSquared > maxSpeed * maxSpeed) {
                const speed = Math.sqrt(speedSquared);
                state.puck.vx = (state.puck.vx / speed) * maxSpeed;
                state.puck.vy = (state.puck.vy / speed) * maxSpeed;
            }
        }
    }

    function step(frameScale) {
        if (!state.running || state.gameOver || state.timerOn) {
            state.playerMoved = false;
            return;
        }

        const horizontal = Number(pressedKeys.has("d") || pressedKeys.has("arrowright"))
            - Number(pressedKeys.has("a") || pressedKeys.has("arrowleft"));
        const vertical = Number(pressedKeys.has("s") || pressedKeys.has("arrowdown"))
            - Number(pressedKeys.has("w") || pressedKeys.has("arrowup"));
        if (horizontal || vertical) {
            const length = Math.hypot(horizontal, vertical);
            const speed = 7 * frameScale;
            movePlayerTo(
                state.player.x + (horizontal / length) * speed,
                state.player.y + (vertical / length) * speed
            );
        } else if (!state.playerMoved) {
            state.player.vx = 0;
            state.player.vy = 0;
        }
        updateBot(frameScale);

        state.puck.x += state.puck.vx * frameScale;
        state.puck.y += state.puck.vy * frameScale;
        const friction = Math.pow(0.995, frameScale);
        state.puck.vx *= friction;
        state.puck.vy *= friction;

        // Верх / низ
        if (state.puck.y - PUCK_R < 0) {
            state.puck.y = PUCK_R;
            state.puck.vy *= -1;
        } else if (state.puck.y + PUCK_R > H) {
            state.puck.y = H - PUCK_R;
            state.puck.vy *= -1;
        }

        const goalTop = H / 2 - GOAL_HALF;
        const goalBottom = H / 2 + GOAL_HALF;
        const inGoalMouth = state.puck.y > goalTop && state.puck.y < goalBottom;

        // Левая стена / ворота
        if (state.puck.x - PUCK_R < 0) {
            if (inGoalMouth) {
                state.scoreBot += 1;
                afterGoal();
                return;
            } else {
                state.puck.x = PUCK_R;
                state.puck.vx *= -1;
            }
        }

        // Правая стена / ворота
        if (state.puck.x + PUCK_R > W) {
            if (inGoalMouth) {
                state.scoreYou += 1;
                afterGoal();
                return;
            } else {
                state.puck.x = W - PUCK_R;
                state.puck.vx *= -1;
            }
        }

        resolveCollision(state.player);
        resolveCollision(state.bot);
        state.playerMoved = false;
    }

    function afterGoal() {
        updateScoreboard();
        updateBotSpeedFactor();
        if (state.scoreYou >= WIN_SCORE || state.scoreBot >= WIN_SCORE) {
            state.gameOver = true;
            const youWin = state.scoreYou > state.scoreBot;
            recordMatchResult(youWin);
            setStatus(
                youWin
                    ? `🏆 Победа! Ты обыграл бота уровня <strong>${DIFFICULTIES[currentDifficulty].label}</strong> ${state.scoreYou}:${state.scoreBot}.`
                    : `Бот оказался сильнее — ${state.scoreBot}:${state.scoreYou}. Попробуй ещё раз или выбери уровень полегче.`
            );
            if (window.showToast) {
                window.showToast(
                    youWin ? "Матч выигран! Отличная игра." : "Матч проигран. Попробуй ещё раз!",
                    youWin ? "gold" : "pink"
                );
            }
            return;
        }
        goalOverlay();
    }

    function drawArena(context) {
        context.fillStyle = "#0a1c44";
        context.fillRect(0, 0, W, H);
        // Центральная линия
        context.strokeStyle = "rgba(165, 210, 255, 0.28)";
        context.lineWidth = 2;
        context.setLineDash([10, 10]);
        context.beginPath();
        context.moveTo(W / 2, 0);
        context.lineTo(W / 2, H);
        context.stroke();
        context.setLineDash([]);

        // Центральный круг
        context.strokeStyle = "rgba(92, 200, 255, 0.3)";
        context.beginPath();
        context.arc(W / 2, H / 2, 45, 0, Math.PI * 2);
        context.stroke();

        // Ворота
        context.fillStyle = "#ff5c9a";
        context.shadowColor = "#ff5c9a";
        context.shadowBlur = 12;
        context.fillRect(0, H / 2 - GOAL_HALF, 6, GOAL_HALF * 2);
        context.fillRect(W - 6, H / 2 - GOAL_HALF, 6, GOAL_HALF * 2);
        context.shadowBlur = 0;
    }

    function draw() {
        ctx.drawImage(arenaCanvas, 0, 0, W, H);

        // Биты
        drawCircle(state.player.x, state.player.y, MALLET_R, "#5cc8ff");
        drawCircle(state.bot.x, state.bot.y, MALLET_R, "#ff5c9a");

        // Шайба
        drawCircle(state.puck.x, state.puck.y, PUCK_R, "#ffffff");
    }

    function drawCircle(x, y, r, color) {
        ctx.beginPath();
        ctx.arc(x, y, r, 0, Math.PI * 2);
        ctx.fillStyle = color;
        ctx.shadowColor = color;
        ctx.shadowBlur = 16;
        ctx.fill();
        ctx.strokeStyle = "#fff";
        ctx.lineWidth = 2;
        ctx.stroke();
        ctx.shadowBlur = 0;
    }

    function loop(timestamp) {
        if (document.hidden) {
            animationFrameId = null;
            return;
        }
        const frameScale = lastFrameTime
            ? Math.min((timestamp - lastFrameTime) / (1000 / 60), 2)
            : 1;
        lastFrameTime = timestamp;
        step(frameScale);
        draw();
        animationFrameId = requestAnimationFrame(loop);
    }

    function startLoop() {
        if (animationFrameId !== null) return;
        lastFrameTime = 0;
        animationFrameId = requestAnimationFrame(loop);
    }

    // --- UI: выбор сложности ---
    difficultyButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            const nextDifficulty = btn.dataset.difficulty;
            if (!DIFFICULTIES[nextDifficulty] || nextDifficulty === currentDifficulty) return;
            difficultyButtons.forEach((button) => {
                const isActive = button === btn;
                button.classList.toggle("active", isActive);
                button.setAttribute("aria-pressed", String(isActive));
            });
            currentDifficulty = nextDifficulty;
            resetMatch();
        });
    });

    if (restartBtn) {
        restartBtn.addEventListener("click", resetMatch);
    }

    resizeCanvas();
    window.addEventListener("resize", resizeCanvas, { passive: true });
    document.addEventListener("visibilitychange", () => {
        if (document.hidden) {
            if (animationFrameId !== null) cancelAnimationFrame(animationFrameId);
            animationFrameId = null;
            return;
        }
        startLoop();
    });
    resetMatch();
    startLoop();
})();
