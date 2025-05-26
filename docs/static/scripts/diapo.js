const randomNumber = (min, max) => Math.floor(Math.random() * (max - min + 1) + min);

const bannerElement = document.querySelector(".blured-image");
bannerElement.src = `static/media/diapo/IMG_${randomNumber(1, 4)}.png`;
