getLocation();

var periods = document.getElementsByClassName("periods");
var dayNames= document.getElementsByClassName("dayNames");
var hiLows  = document.getElementsByClassName("hi-low");
var days    = document.getElementsByClassName("daysForecasts");
var nights  = document.getElementsByClassName("nightsForecasts");
var periodsData = [];
var highAndLowObject = [];
var grid_X = 0;
var grid_Y = 0;
var officeName = "";
var largestCellY = 0;
var largestCellX = 0;
var firstPeriodIsDay = false;



function getLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition);
    } else {
        console.log("Geolocation is not supported by this browser.");
    }//end if
}//end function

function showPosition(position) {
    var latitude  = position.coords.latitude;
    var longitude = position.coords.longitude;    

    getGridRequest(latitude,longitude);
}//end function

function getGridRequest(latitude,longitude) {
    //VARIABLES
    var ajaxRequest = new XMLHttpRequest;
    var url = `https://api.weather.gov/points/${latitude},${longitude}`
    var runAsyncronously = true;

    //SETUP REQUEST
    ajaxRequest.open('GET', url, runAsyncronously);

    //WHICH FUNCTION TO RUN WHEN THE REQUEST RETURNS
    ajaxRequest.onreadystatechange = checkStatus;

    //ACTUALLY SEND THE REQUEST AND WAIT FOR RESPONSE
    ajaxRequest.send();
}//end function

function checkStatus() {
    //MAKE CERTAIN RESPONSE IS OK AND READY 
    if (this.status === 200 && this.readyState === 4) {

        //PARSE THE STRING BACK INTO AN OBJECT
        var data = JSON.parse(this.responseText);

        var office = data.properties.gridId;
        var gridX  = data.properties.gridX;
        var gridY  = data.properties.gridY;
        officeName = data.properties.gridId;
        grid_X      = data.properties.gridX;
        grid_Y      = data.properties.gridY;

        //getHighAndLow(office,gridX,gridY);
        getForecast(office,gridX,gridY);
    }//end if
}//end function

function getHighAndLow (office, gridX, gridY) {
    //VARIABLES
    var ajaxRequest = new XMLHttpRequest;
    var url = `https://api.weather.gov/gridpoints/${office}/${gridX},${gridY}`;
    var runAsyncronously = true;

    //SETUP REQUEST
    ajaxRequest.open('GET', url, runAsyncronously);

    //WHICH FUNCTION TO RUN WHEN THE REQUEST RETURNS
    ajaxRequest.onreadystatechange = checkHighLowForecast;

    //ACTUALLY SEND THE REQUEST AND WAIT FOR RESPONSE
    ajaxRequest.send();

}//end function

function checkHighLowForecast(office, gridX, gridY) {
    //MAKE CERTAIN RESPONSE IS OK AND READY 
    if (this.status === 200 && this.readyState === 4) {

        //PARSE THE STRING BACK INTO AN OBJECT
        var highLowForecast = JSON.parse(this.responseText);        

        highAndLowObject = highLowForecast; 
        
        PopulateHighsLows(highAndLowObject);
    }//end if
}

function getForecast(office,gridX,gridY) {
   /* "gridId": "LIX",
        "gridX": 111,
            "gridY": 110,*/

    //VARIABLES
    var ajaxRequest = new XMLHttpRequest;
    var url = `https://api.weather.gov/gridpoints/${office}/${gridX},${gridY}/forecast`;
    var runAsyncronously = true;

    //SETUP REQUEST
    ajaxRequest.open('GET', url, runAsyncronously);

    //WHICH FUNCTION TO RUN WHEN THE REQUEST RETURNS
    ajaxRequest.onreadystatechange = checkForcastStatus;

    //ACTUALLY SEND THE REQUEST AND WAIT FOR RESPONSE
    ajaxRequest.send();
}//end function

function checkForcastStatus() {
    //MAKE CERTAIN RESPONSE IS OK AND READY 
    if (this.status === 200 && this.readyState === 4) {

        //PARSE THE STRING BACK INTO AN OBJECT
        var data = JSON.parse(this.responseText);

        PopulateForecastData(data.properties.periods);
    }//end if
}//end function

function PopulateForecastData(forecast) {
    let dayIndex = 0;
    let dayIterator = 0;
    let dayNameIndex = 0;
    let nightIndex = 0;
    let periodsIndex = 0;    
    
    if (!forecast[0].isDaytime) {
        periodsIndex += 1;
        dayIndex += 1;
        dayIterator += 1;
        periods[0].classList.toggle("empty");
    } else {
        firstPeriodIsDay = true;
    }

    for (var i = 0; i < 14; i++) {
        let rain = "N/A";
        
        if (i % 2 == periodsIndex) {
            dayNames[dayNameIndex++].innerText = forecast[i].name;
        }
        if (forecast[i].probabilityOfPrecipitation.value != null) {
            rain = forecast[i].probabilityOfPrecipitation.value;
        }
        if (forecast[i].isDaytime) {
            if (dayIndex < 7) {
                days[dayIndex++].innerHTML = `<b>${forecast[i].temperature}&deg;</b><br>Precip %<br><strong>${rain}</strong><br>Humidity<br><strong>${forecast[i].relativeHumidity.value}%</strong>`;                
            }
        } else {
            nights[nightIndex++].innerHTML = `<b>${forecast[i].temperature}&deg;</b><br>Precip %<br><strong>${rain}</strong><br>Humidity<br><strong>${forecast[i].relativeHumidity.value}%</strong>`;                            
        }
        if (periods[i].clientHeight > largestCellY || periods[i].clientWidth > largestCellX) {
            largestCellY = periods[i].clientHeight;
            largestCellX = periods[i].clientWidth;
        }
        periods[i].addEventListener("click", PeriodClicked);
    }
    for (var i = periodsIndex; i < 14; i++) {
        periodsData[i] = forecast[i-periodsIndex];
    }//end for

    dayIndex = dayIterator;
    nightIndex = 0;

    for (var i = 0; i < 14; i++) {        
        if (forecast[i].isDaytime) {
            if (dayIndex < 7) {
                SetImage(forecast[i], "day", days[dayIndex++], largestCellY);
            }
        } else {            
            SetImage(forecast[i], "night", nights[nightIndex++], largestCellY);
        }
    }   
    getHighAndLow(officeName,grid_X,grid_Y);
}

function SetImage(periodData, time, gridInfo, largestCellY) {
    //CHECK FOR FORECASTED WEATHER CONDITIONS AND UPDATE CSS IMAGE
    //RAIN = "rainy" CLOUDS = "cloudy" SUN = "sunny" CLEAR = "clear" UNKNOWN = "none"
    //DAY = "day" NIGHT = "night"
    //EG OF CSS CLASS -- "day-cloudy"
    let text = periodData.shortForecast.toUpperCase();
    let sizeX = gridInfo.clientWidth;
    let sizeY = largestCellY;
    let className = `${time}-`;

    if (text.includes("THUNDER" || "THUNDERSTORM" || "LIGHTNING")) {
        className += "storm";
        gridInfo.classList.toggle(className);
    } else if (text.includes("RAIN" || "RAINY" || "SHOWERS" || "STORM" || "STORMY")) {
        className += "rainy";
        gridInfo.classList.toggle(className);
    } else if (text.includes("PARTLY CLOUDY" || "MOSTLY CLOUDY")) {
        className += "partlycloudy";
        gridInfo.classList.toggle(className);
    } else if (text.includes("CLOUD" || "CLOUDY" || "CLOUDS" || "OVERCAST")) {
        className += "cloudy";
        gridInfo.classList.toggle(className);
    } else if (text.includes("SUN" || "SUNNY" || "SUNSHINE")) {
        className += "sunny";
        gridInfo.classList.toggle(className);
    } else if (text.includes("CLEAR")) {
        className += "clear";
        gridInfo.classList.toggle(className);
    } else {
        className += "none";
        gridInfo.classList.toggle(className);
    }

    gridInfo.style.backgroundSize = `${sizeX}px ${sizeY}px`;
}

function PopulateHighsLows(highAndLowObject) {
    let maxTempsCel = highAndLowObject.properties.maxTemperature.values;
    //let minTempsCel = highAndLowObject.properties.minTemperature.values;
    let minTempsCel = [];
    for (var i = 0; i < periodsData.length; i++) {
        if (!periodsData[i].isDaytime) {
            minTempsCel.push(periodsData[i]);
        }
    }
    let isDay = 0;

    if (firstPeriodIsDay) {
        isDay = 1;
    }

    for (var i = 0; i < 7; i++) {
        let max = (maxTempsCel[i].value * 9/5) + 32;
        //let min = (minTempsCel[i + isDay].value * 9/5) + 32;
        let min = minTempsCel[i].temperature;
        hiLows[i].innerHTML = `High <strong>${max}&deg;</strong><br>Low &nbsp;<strong>${min}&deg;</strong>`
    }
}

function UpdateImageSizes() {
    largestCellY = periods[0].clientHeight;
    largestCellX = periods[0].clientWidth;

    for (var i = 0; i < periods.length; i++) {
        periods[i].style.backgroundSize = `${largestCellX}px ${largestCellY}px`;
    }
}

function PeriodClicked() {
    let display = document.getElementById("specific");
    let clickedCell = GetValue(this.outerHTML);
    let forecastData = periodsData[clickedCell];
    let forecast = forecastData.detailedForecast;
    let image   = forecastData.icon;
    let alt     = forecastData.shortForecast;
    let temp    = forecastData.temperature;
    let humidity= forecastData.relativeHumidity.value;

    display.innerHTML = `<h1>Detailed Forecast</h1><br><p>${forecast}</p><img src=${image} alt=${alt}><span><strong>${temp}&deg;F</strong> and ${humidity}% humidity</span>`;
}

function GetValue(text){
    let value = "";
    let searching = true;
    let detectedBSlash = false;
    let index = 0;
    while (searching && index < text.length) {
        if (text[index] == '\"' && detectedBSlash == false) { 
            detectedBSlash = true;
        } else if (detectedBSlash) {
            if (text[index] != '\"') {
                value += text[index];
            } else {
                searching = false;
            }
        }
        index++;
    }
    return parseInt(value);
}