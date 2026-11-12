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
    const arena = document.querySelector(".neon-arena-preview");

    if (mallet && puck && arena) {
        let framePending = false;
        let previewFrame = 0;
        let pointerX = 0;
        let pointerY = 0;

        arena.addEventListener("pointermove", (event) => {
            pointerX = event.clientX;
            pointerY = event.clientY;
            if (framePending) return;

            framePending = true;
            previewFrame = requestAnimationFrame(() => {
                previewFrame = 0;
                framePending = false;
                const rect = arena.getBoundingClientRect();
                const malletCenterX = Math.max(20, Math.min(pointerX - rect.left, rect.width / 2 - 20));
                const malletCenterY = Math.max(20, Math.min(pointerY - rect.top, rect.height - 20));
                mallet.style.left = `${malletCenterX - 16}px`;
                mallet.style.top = `${malletCenterY - 16}px`;

                const dx = puck.offsetLeft + 10 - malletCenterX;
                const dy = puck.offsetTop + 10 - malletCenterY;
                const distanceSquared = dx * dx + dy * dy;

                if (distanceSquared < 26 * 26 && distanceSquared > 0) {
                    const distance = Math.sqrt(distanceSquared);
                    const pushDistance = 66;
                    const newPuckX = malletCenterX + (dx / distance) * pushDistance - 10;
                    const newPuckY = malletCenterY + (dy / distance) * pushDistance - 10;
                    puck.style.left = `${Math.max(10, Math.min(newPuckX, rect.width - 30))}px`;
                    puck.style.top = `${Math.max(10, Math.min(newPuckY, rect.height - 30))}px`;
                }
            });
        });

        arena.addEventListener("pointerleave", () => {
            if (previewFrame) cancelAnimationFrame(previewFrame);
            previewFrame = 0;
            framePending = false;
            puck.style.left = "48%";
            puck.style.top = "45%";
            mallet.style.left = "20%";
            mallet.style.top = "40%";
        });
    }

    // Живой счётчик XP/уровня-демо на главной (чисто декоративный, без ложных данных)
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
