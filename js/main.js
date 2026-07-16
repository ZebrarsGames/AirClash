// Игровой интерактив на главной странице
document.addEventListener("DOMContentLoaded", () => {
    const mallet = document.getElementById("interactive-mallet");
    const puck = document.getElementById("interactive-puck");
    const arena = document.querySelector(".neon-arena-preview");

    if (mallet && puck && arena) {
        // Простая физика движения шайбы за битой при движении мыши по арене
        arena.addEventListener("mousemove", (e) => {
            const rect = arena.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            // Ограничение движения биты левой половиной стола
            const limitX = Math.max(20, Math.min(x, rect.width / 2 - 20));
            const limitY = Math.max(20, Math.min(y, rect.height - 20));

            mallet.style.left = `${limitX - 16}px`;
            mallet.style.top = `${limitY - 16}px`;

            // Вычисление коллизии шайбы
            const malletCenterX = limitX;
            const malletCenterY = limitY;
            const puckCenterX = puck.offsetLeft + 10;
            const puckCenterY = puck.offsetTop + 10;

            const dx = puckCenterX - malletCenterX;
            const dy = puckCenterY - malletCenterY;
            const distance = Math.sqrt(dx * dx + dy * dy);

            if (distance < 26) {
                // Отскок
                const angle = Math.atan2(dy, dx);
                const pushForce = 40;
                let newPuckX = malletCenterX + Math.cos(angle) * (26 + pushForce) - 10;
                let newPuckY = malletCenterY + Math.sin(angle) * (26 + pushForce) - 10;

                // Ограничения стола для шайбы
                newPuckX = Math.max(10, Math.min(newPuckX, rect.width - 30));
                newPuckY = Math.max(10, Math.min(newPuckY, rect.height - 30));

                puck.style.left = `${newPuckX}px`;
                puck.style.top = `${newPuckY}px`;
            }
        });
        
        // Возвращение шайбы в центр при уходе мыши
        arena.addEventListener("mouseleave", () => {
            puck.style.left = "48%";
            puck.style.top = "45%";
            mallet.style.left = "20%";
            mallet.style.top = "40%";
        });
    }
});

// Симуляция скачивания
function triggerDownload(platformName) {
    if (platformName == 'PC Client') {
        alert(`К сожалению, игра пока что не доступна на ПК`);
    }
    else if (platformName == 'Android APK') {
        window.open('https://github.com/ZebrarsGames/AirClash/releases', '_blank');
    }
}

// Отправка формы обратной связи AirClash
async function handleContactSubmit(event) {
    event.preventDefault(); // Блокируем стандартную перезагрузку страницы
    
    const form = event.target;
    const button = form.querySelector('button[type="submit"]');
    
    // Блокируем кнопку и меняем текст на время отправки
    const originalButtonText = button.textContent;
    button.textContent = "Отправка сигнала...";
    button.disabled = true;

    // Собираем данные из полей ввода
    const formData = new FormData(form);
    
    // Автоматически добавляем ваш ключ активации Web3Forms в запрос
    formData.append("access_key", "fcbe9b27-2403-431d-a8b0-ef4804fcf167".trim());

    try {
        // Отправляем данные на правильный рабочий сервер API
        const response = await fetch("https://api.web3forms.com/submit", {
            method: "POST",
            body: formData
        });

        const result = await response.json();

        // Проверяем успешность ответа от сервера
        if (response.ok && result.success) {
            alert("Сообщение успешно отправлено в центр управления AirClash! Мы ответим вам в ближайшее время.");
            form.reset(); // Очищаем поля формы только при успехе
        } else {
            alert("Ошибка сервера: " + (result.message || "Неизвестная ошибка"));
        }
    } catch (error) {
        console.error("Ошибка сети:", error);
        alert("Не удалось отправить сигнал. Проверьте интернет-соединение.");
    } finally {
        // В любом случае возвращаем кнопку в исходное рабочее состояние
        button.textContent = originalButtonText;
        button.disabled = false;
    }
}