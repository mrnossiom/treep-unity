console.log("Script loaded")

// Random image
var imageNumber = 1;
var numRand = Math.floor(Math.random() * imageNumber);

var imagePath = `ressources/media/diapo/IMG_${numRand}.png`;
var bannerElement = document.getElementsByClassName("banner");
bannerElement[0].style.background = `url(${imagePath}) center/cover no-repeat`;

// OS Detection for download
function detectOS() {
  const userAgent = navigator.userAgent;
  let os = "Unknown OS";

  if (/Windows NT 10.0/.test(userAgent)) os = "Windows 10";
  else if (/Windows NT 6.3/.test(userAgent)) os = "Windows 8.1";
  else if (/Windows NT 6.2/.test(userAgent)) os = "Windows 8";
  else if (/Windows NT 6.1/.test(userAgent)) os = "Windows 7";
  else if (/Macintosh/.test(userAgent)) os = "macOS";
  else if (/Linux/.test(userAgent)) os = "Linux";
  else if (/Android/.test(userAgent)) os = "Android";
  else if (/iPhone|iPad|iPod/.test(userAgent)) os = "iOS";

  return os;
}
