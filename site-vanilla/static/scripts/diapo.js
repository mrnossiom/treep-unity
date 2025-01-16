const randomNumber = (min, max) => Math.floor(Math.random() * (max - min + 1) + min);

const imagePath = `/static/media/diapo/IMG_${randomNumber(1, 2)}.png`;
const bannerElement = document.getElementsByClassName("banner");
bannerElement[0].style.background = `url(${imagePath}) center/cover no-repeat`;
