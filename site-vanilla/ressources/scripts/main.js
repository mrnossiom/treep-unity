console.log("Script loaded")

// Random image
var imageNumber = 3;
var numRand = Math.floor(Math.random() * imageNumber);

var imagePath = `ressources/media/diapo/IMG_${numRand}.png`;
var bannerElement = document.getElementsByClassName("banner");
bannerElement[0].style.background = `url(${imagePath}) center/cover no-repeat`;
