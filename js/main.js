// --- Тосты (лёгкая замена alert()) ---
function ensureToastStack() {
    let stack = document.querySelector(".toast-stack");
    if (!stack) {
        stack = document.createElement("div");
        stack.className = "toast-stack";
        stack.setAttribute("aria-live", "polite");
        stack.setAttribute("aria-relevant", "additions");
        document.body.appendChild(stack);
    }
    return stack;
}

function showToast(message, type = "blue", duration = 4200) {
    const stack = ensureToastStack();
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.setAttribute("role", "status");
    toast.textContent = message;
    stack.appendChild(toast);
    setTimeout(() => {
        toast.style.opacity = "0";
        toast.style.transition = "opacity 0.3s ease";
        setTimeout(() => toast.remove(), 300);
    }, duration);
}
window.showToast = showToast;

// --- Игровой интерактив на главной странице (превью в hero) ---
document.addEventListener("DOMContentLoaded", () => {
    let ambientFrame = 0;
    window.addEventListener("pointermove", (event) => {
        if (ambientFrame) return;
        ambientFrame = requestAnimationFrame(() => {
            ambientFrame = 0;
            document.body.style.setProperty("--ambient-x", `${event.clientX}px`);
            document.body.style.setProperty("--ambient-y", `${event.clientY}px`);
            document.body.classList.add("has-ambient-pointer");
        });
    }, { passive: true });

    window.addEventListener("pointerleave", () => {
        document.body.classList.remove("has-ambient-pointer");
    }, { passive: true });

    const mallet = document.getElementById("interactive-mallet");
    const puck = document.getElementById("interactive-puck");
    const redMallet = document.querySelector(".mallet-red");
    const arena = document.querySelector(".neon-arena-preview");

    if (mallet && puck && arena) {
        let rect = arena.getBoundingClientRect();
        let width = rect.width || arena.clientWidth || 400;
        let height = rect.height || arena.clientHeight || 250;

        // Начальные координаты
        let puckX = width / 2;
        let puckY = height / 2;
        let puckVx = 0;
        let puckVy = 0;

        let malletX = width * 0.25;
        let malletY = height * 0.5;
        let targetMalletX = malletX;
        let targetMalletY = malletY;
        let prevMalletX = malletX;
        let prevMalletY = malletY;

        let redX = width * 0.75;
        let redY = height * 0.5;

        const malletRadius = 18;
        const puckRadius = 14;
        const friction = 0.94; // Плавное замедление (инерция)

        let isInside = false;
        let pointerX = malletX;
        let pointerY = malletY;

        arena.addEventListener("pointermove", (event) => {
            isInside = true;
            const r = arena.getBoundingClientRect();
            pointerX = event.clientX - r.left;
            pointerY = event.clientY - r.top;

            targetMalletX = Math.max(malletRadius, Math.min(pointerX, r.width / 2 - malletRadius));
            targetMalletY = Math.max(malletRadius, Math.min(pointerY, r.height - malletRadius));
        });

        arena.addEventListener("pointerleave", () => {
            isInside = false;
        });

        function animate() {
            const r = arena.getBoundingClientRect();
            const w = r.width || 400;
            const h = r.height || 250;

            prevMalletX = malletX;
            prevMalletY = malletY;

            if (isInside) {
                // Плавное следование за курсором
                malletX += (targetMalletX - malletX) * 0.35;
                malletY += (targetMalletY - malletY) * 0.35;

                // Движение шайбы с трением (инерция)
                puckX += puckVx;
                puckY += puckVy;
                puckVx *= friction;
                puckVy *= friction;

                if (Math.abs(puckVx) < 0.05) puckVx = 0;
                if (Math.abs(puckVy) < 0.05) puckVy = 0;
            } else {
                // Плавный возврат на исходные позиции при уходе курсора
                malletX += (w * 0.25 - malletX) * 0.1;
                malletY += (h * 0.5 - malletY) * 0.1;

                puckX += (w / 2 - puckX) * 0.1;
                puckY += (h / 2 - puckY) * 0.1;
                puckVx *= 0.8;
                puckVy *= 0.8;
            }

            const malletVx = malletX - prevMalletX;
            const malletVy = malletY - prevMalletY;

            // Обработка границ арены и отскоков
            if (puckX - puckRadius < 0) {
                puckX = puckRadius;
                puckVx = -puckVx * 0.8;
            } else if (puckX + puckRadius > w) {
                puckX = w - puckRadius;
                puckVx = -puckVx * 0.8;
            }

            if (puckY - puckRadius < 0) {
                puckY = puckRadius;
                puckVy = -puckVy * 0.8;
            } else if (puckY + puckRadius > h) {
                puckY = h - puckRadius;
                puckVy = -puckVy * 0.8;
            }

            // Столкновение синей биты с шайбой
            const dx = puckX - malletX;
            const dy = puckY - malletY;
            const dist = Math.sqrt(dx * dx + dy * dy);
            const minDist = malletRadius + puckRadius;

            if (dist < minDist && dist > 0) {
                const overlap = minDist - dist;
                const nx = dx / dist;
                const ny = dy / dist;

                puckX += nx * overlap;
                puckY += ny * overlap;

                const hitSpeed = Math.sqrt(malletVx * malletVx + malletVy * malletVy);
                const pushStrength = Math.max(hitSpeed * 1.3, 5);
                puckVx = nx * pushStrength + malletVx * 0.5;
                puckVy = ny * pushStrength + malletVy * 0.5;
            }

            // Бот (красная бита) на правой половине
            if (redMallet) {
                const targetRedX = w * 0.75 + (isInside ? Math.sin(Date.now() * 0.003) * 15 : 0);
                const targetRedY = isInside ? puckY : h * 0.5;

                redX += (targetRedX - redX) * 0.1;
                redY += (Math.max(malletRadius, Math.min(targetRedY, h - malletRadius)) - redY) * 0.1;

                const dxRed = puckX - redX;
                const dyRed = puckY - redY;
                const distRed = Math.sqrt(dxRed * dxRed + dyRed * dyRed);
                if (distRed < minDist && distRed > 0) {
                    const overlap = minDist - distRed;
                    const nx = dxRed / distRed;
                    const ny = dyRed / distRed;
                    puckX += nx * overlap;
                    puckY += ny * overlap;
                    puckVx = nx * 6 + (Math.random() - 0.5) * 2;
                    puckVy = ny * 6 + (Math.random() - 0.5) * 2;
                }

                redMallet.style.left = `${redX}px`;
                redMallet.style.top = `${redY}px`;
                redMallet.style.transform = "translate(-50%, -50%)";
            }

            // Применение позиций строго по центру через transform
            mallet.style.position = "absolute";
            mallet.style.left = `${malletX}px`;
            mallet.style.top = `${malletY}px`;
            mallet.style.transform = "translate(-50%, -50%)";

            puck.style.position = "absolute";
            puck.style.left = `${puckX}px`;
            puck.style.top = `${puckY}px`;
            puck.style.transform = "translate(-50%, -50%)";

            requestAnimationFrame(animate);
        }

        requestAnimationFrame(animate);
    }

    // Живой счётчик XP/уровня-демо на главной
    const xpFill = document.getElementById("xp-demo-fill");
    if (xpFill) {
        requestAnimationFrame(() => {
            xpFill.style.width = xpFill.dataset.progress || "0%";
        });
    }

    const daysCount = document.getElementById("days-count");
    const daysLabel = document.getElementById("days-label");
    if (daysCount && daysLabel) {
        const today = new Date();
        const todayUtc = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
        const startUtc = Date.UTC(2026, 2, 21);
        const days = Math.max(0, Math.floor((todayUtc - startUtc) / 86400000));
        const lastDigit = days % 10;
        const lastTwoDigits = days % 100;

        daysCount.value = String(days);
        daysCount.textContent = String(days);
        daysLabel.textContent = lastDigit === 1 && lastTwoDigits !== 11
            ? "день"
            : [2, 3, 4].includes(lastDigit) && ![12, 13, 14].includes(lastTwoDigits)
                ? "дня"
                : "дней";
    }
});

// --- Отправка формы обратной связи AirClash ---
async function handleContactSubmit(event) {
    event.preventDefault();

    const form = event.target;
    const button = form.querySelector('button[type="submit"]');

    const originalButtonText = button.textContent;
    button.textContent = "Отправка сигнала...";
    button.disabled = true;

    const usernameInput = form.querySelector('input[type="text"]');
    const emailInput = form.querySelector('input[type="email"]');
    const messageInput = form.querySelector("textarea");

    const payload = {
        access_key: "fcbe9b27-2403-431d-a8b0-ef4804fcf167",
        username: usernameInput ? usernameInput.value : "",
        email: emailInput ? emailInput.value : "",
        message: messageInput ? messageInput.value : "",
    };

    try {
        const response = await fetch("https://api.web3forms.com/submit", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Accept: "application/json",
            },
            body: JSON.stringify(payload),
        });

        const result = await response.json();

        if (response.ok && result.success) {
            showToast("Сообщение успешно отправлено в центр управления AirClash! Мы ответим вам в ближайшее время.", "blue");
            form.reset();
        } else {
            showToast("Ошибка сервера: " + (result.message || "Неверный ключ"), "pink");
        }
    } catch (error) {
        console.error("Ошибка сети:", error);
        showToast("Не удалось отправить сигнал. Проверьте интернет-соединение.", "pink");
    } finally {
        button.textContent = originalButtonText;
        button.disabled = false;
    }
}

// --- Логика страницы дней в разработке ---
document.addEventListener("DOMContentLoaded", () => {
    const daysCountEl = document.getElementById("daysCount");
    const daysWordEl = document.getElementById("daysWord");
    const dateTextEl = document.getElementById("dateText");
    const liveTimerEl = document.getElementById("liveTimer");

    if (!daysCountEl || !daysWordEl || !dateTextEl || !liveTimerEl) return;

    const startDate = new Date(2026, 2, 21);
    startDate.setHours(0, 0, 0, 0);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const days = Math.max(0, Math.floor((today - startDate) / 86400000));
    const lastDigit = days % 10;
    const lastTwoDigits = days % 100;

    daysCountEl.textContent = String(days);
    daysWordEl.textContent = lastDigit === 1 && lastTwoDigits !== 11
        ? "день"
        : [2, 3, 4].includes(lastDigit) && ![12, 13, 14].includes(lastTwoDigits)
            ? "дня"
            : "дней";
    daysCountEl.setAttribute("aria-label", `${days} ${daysWordEl.textContent}. Нажми, чтобы увидеть пасхалку.`);

    let timerState;

    function updateTimeDisplay(value) {
        if (!timerState) {
            const container = document.createElement("span");
            container.className = "timer-container";
            container.setAttribute("aria-hidden", "true");
            const columns = [];

            for (const char of value) {
                if (char >= "0" && char <= "9") {
                    const column = document.createElement("span");
                    column.className = "timer-digit-column";
                    const track = document.createElement("span");
                    track.className = "timer-digit-track";
                    const digits = [];

                    for (let digit = 0; digit <= 9; digit += 1) {
                        const item = document.createElement("span");
                        item.className = `timer-digit${digit === 0 ? " is-active" : ""}`;
                        item.textContent = String(digit);
                        track.appendChild(item);
                        digits.push(item);
                    }

                    column.appendChild(track);
                    container.appendChild(column);
                    columns.push({ track, digits, current: -1 });
                } else {
                    const separator = document.createElement("span");
                    separator.className = "timer-separator";
                    separator.textContent = char;
                    container.appendChild(separator);
                    columns.push({ separator, current: char });
                }
            }

            liveTimerEl.replaceChildren(container);
            timerState = { columns };
        }

        for (let index = 0; index < value.length; index += 1) {
            const column = timerState.columns[index];
            const char = value[index];
            if (!column || column.current === char) continue;

            if (column.digits) {
                const digit = Number(char);
                const previousDigit = Number(column.current);
                if (previousDigit >= 0) column.digits[previousDigit].classList.remove("is-active");
                column.current = char;
                column.digits[digit].classList.add("is-active");
                column.track.style.transform = `translateY(-${digit * 1.2}em)`;
            } else {
                column.current = char;
                column.separator.textContent = char;
            }
        }
    }

    function updateTimer() {
        const elapsed = Math.max(0, Date.now() - startDate.getTime());
        const hours = Math.floor((elapsed / 3600000) % 24);
        const minutes = Math.floor((elapsed / 60000) % 60);
        const seconds = Math.floor((elapsed / 1000) % 60);
        const time = `${String(hours).padStart(2, "0")} : ${String(minutes).padStart(2, "0")} : ${String(seconds).padStart(2, "0")}`;
        liveTimerEl.setAttribute("aria-label", `Время сегодня: ${time}`);
        updateTimeDisplay(time);
    }

    updateTimer();
    window.setInterval(updateTimer, 1000);

    const easterEggs = [
        "Код работает, никто не знает почему", "Багов нет, это фичи", "Каждая строка приближает релиз",
        "Разработка идёт по плану", "Пора пить чай", "Физика шайбы под контролем", "Коммит ради коммита",
        "Строк кода всё больше", "Работает — не трогай", "Сломалось? Откати", "Бесконечный цикл оптимизации",
        "Ошибки делают нас сильнее", "Где моя документация", "Снова рефакторинг", "Игра почти готова",
        "План был надёжным, как швейцарские часы", "Сборка прошла успешно. Наверное", "Глюк или фича?",
        "Почти киберспорт", "Полёт нормальный", "Патчноут пишется сам", "Клавиатура выдержит",
        "Мысль ускользнула", "Баг зафиксирован. Он не согласен", "Релиз не за горами",
        "Таймер считает вечно", "Холст рисует сам", "Память утекает красиво", "Скрипт шуршит",
        "Мысли о билде", "Тесты прошли мимо", "Кофе закончился", "Снова ночь на дворе",
        "Экран моргает от багов", "Разгон процессора", "Кадр выпал из кадра",
        "Случайный баг в подарок", "Логи чисты как слеза", "Идея пришла во сне",
        "Проект мечты", "Ждём фидбек от стенки", "Шайба летит в угол", "Борьба с физикой",
        "Это не дедлайн, а точка сохранения", "Ещё один маленький фикс", "Где-то здесь есть пасхалка",
        "Шайба знает короткий путь", "Нажми ещё раз — вдруг поможет", "Сначала кофе, потом оптимизация",
        "В этой строке точно нет бага", "Секундочку, компилируется", "404: сон не найден",
        "Бот тоже ждёт релиз", "Не баг, а неожиданный геймплей", "Пиксель к пикселю — и готово",
        "Секретный режим пока секретный", "Проверено на одном разработчике", "Стабильность — это привычка",
        "Шайба просила передать привет", "Всё под контролем. Почти", "Осталось только отполировать",
        "Ctrl+S — лучший звук", "Здесь могла быть ещё одна фича", "Слишком много вкладок",
        "Главное — не открыть старый билд", "Ещё чуть-чуть и точно готово", "Релизный поезд уже прогревается",
        "Случайность — тоже часть дизайна", "Баг ушёл думать над поведением", "Чемодан, код, релиз",
        "Кажется, мы что-то забыли", "Вжух — и ещё одна строчка кода", "Пауза на проверку шайбы",
        "Всё выглядит подозрительно стабильно", "Этот клик засчитан за тестирование",
        "Новый день — новый коммит", "Анимация плавная, дедлайн резкий", "Спасибо, что заглянул!"
    ];
    const defaultDateText = "В разработке с 21 марта 2026 г.";
    let messageTimeout;
    let fadeTimeout;

    function showDateMessage(message) {
        window.clearTimeout(fadeTimeout);
        dateTextEl.style.opacity = "0";
        dateTextEl.style.transform = "translateY(4px)";
        fadeTimeout = window.setTimeout(() => {
            dateTextEl.textContent = message;
            dateTextEl.style.opacity = "1";
            dateTextEl.style.transform = "translateY(0)";
        }, 150);
    }

    daysCountEl.addEventListener("click", () => {
        window.clearTimeout(messageTimeout);
        showDateMessage(easterEggs[Math.floor(Math.random() * easterEggs.length)]);
        messageTimeout = window.setTimeout(() => showDateMessage(defaultDateText), 4000);
    });
});