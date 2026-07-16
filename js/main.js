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

// Отправка формы обратной связи
function handleContactSubmit(event) {
    event.preventDefault();
    alert("Сообщение успешно отправлено в центр управления AirClash! Мы ответим вам в ближайшее время.");
    event.target.reset();
}
