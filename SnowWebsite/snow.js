//#region SNOW CLASS

///CHANGE CONSTANTS TO EFFECT SNOW WEBSITE
const SNOW_SIZE_BASE = 2;
const SNOW_SIZE_RANGE = 7;
const SNOW_BASE_RED_VAL = 255; //VALUE SHOULD BE NO GREATER THAN 255
const SNOW_BASE_GREEN_VAL = 0; //VALUE SHOULD BE NO GREATER THAN 255
const SNOW_BASE_BLUE_VAL = 100; //VALUE SHOULD BE NO GREATER THAN 255
const SNOW_BASE_OPACITY_VAL = 3; //VALUE SHOULD BE BETWEEN 0 - 1

const TOTAL_SNOWFLAKES = 1000;
const BACKGROUND_MIN_VALUE = 0; //MIN VALUE SHOULD BE GREATER THAN 0
const BACKGROUND_MAX_VALUE = 100; //MAX VALUE SHOULD NOT EXCEED 255
//BACKGROUND START VALUE SHOULD BE BETWEEN BACKGROUND_MAX/MIN VALUES
const BACKGROUND_START_VAL_RED = 5;
const BACKGROUND_START_VAL_GREEN = 5;
const BACKGROUND_START_VAL_BLUE = 5;
const BACKGROUND_DELTA_RED = .6;
const BACKGROUND_DELTA_GREEN = 0;
const BACKGROUND_DELTA_BLUE = .3;
const BACKGROUND_COLOR_DELTA_ARRAY = [BACKGROUND_DELTA_RED, BACKGROUND_DELTA_GREEN, BACKGROUND_DELTA_BLUE];
const PUSH_RADIUS = 150;
const KNOCKBACK_MULTIPLIER = 2.5;
const KNOCKBACK_DECAY = .9;
const SNOW_STUN_TIME_VECTOR = 100;



class Snow {
    //CONSTRUCTOR    
    constructor(location = null) {
        //CREATE SNOW PROPERTIES
        this.color = RandomColor();
        this.radius = Math.random() * SNOW_SIZE_RANGE + SNOW_SIZE_BASE;        
        
        this.x = Math.random() * storedWindowWidth;

        if (location == null) {
            this.y = ((Math.random() * storedWindowHeight) - this.radius);
        } else {
            this.y = (location - this.radius);
        }

        //SIZE OF SNOW ??? MAYBE DEPTH ???
        this.dy = Math.random();        
        this.dx = ((Math.random() - 0.5 )* 0.25);
        //CONTROLS INTENSITY OF MOUSE DOWN ???
        this.pushdx;
        this.pushdy;
        //CONTROLS SPEED ???
        this.swayForce = (Math.random() - .5) / 30;
        this.sway = 0;
        this.pushTime = 0;
        this.intensity;
    }//END CONSTRUCTOR

    //DRAWS THE SNOW TO CANVAS
    Draw = function () {
        //STANDARD SNOW DRAWING

        screen.beginPath();
        screen.fillStyle = this.color; //set circle color
        screen.fillRect(this.x, this.y, this.radius, this.radius);                    

        ///FOR VALENTINE'S DAY ONLY
        /* 
        let x = this.x;
        let y = this.y;
        let rad = this.radius;
        let hRad = rad * 0.5;
        let qRad = rad * 0.25;
        let eRad = rad * 0.125;
        let col = this.color;
        screen.beginPath();
        
        //LEFT HALF OF HEART
        screen.moveTo(x, y - hRad);          
        screen.bezierCurveTo(x, y - hRad - qRad, x - eRad, y - rad, x - hRad, y - rad);
        screen.bezierCurveTo(x - qRad - hRad, y - rad, x - rad, y - hRad - qRad, x - rad, y - hRad);
        screen.bezierCurveTo(x - rad, y + qRad, x - eRad, y + hRad + qRad, x, y + rad);        

        //RIGHT HALF OF HEART
        screen.moveTo(x, y - hRad);  
        screen.bezierCurveTo(x, y - hRad - qRad, x + eRad, y - rad, x + hRad, y - rad);
        screen.bezierCurveTo(x + qRad + hRad, y - rad, x + rad, y - hRad - qRad, x + rad, y - hRad);
        screen.bezierCurveTo(x + rad, y + qRad, x + eRad, y + hRad + qRad, x, y + rad);        
        
        screen.fillStyle = col;
        screen.fill();
        */
    }            
}//END CLASS

//#endregion

//#region BEGIN MAIN

const fullPage = document.getElementById("page");

//ASSIGNS CANVAS ELEMENT TO VARIABLE
const screenElement = document.getElementById("snowCan");

//CREATES SCREEN OBJECT
const screen = screenElement.getContext("2d");

//SETS BACKGROUND (ARRAY HOLDING OBJECTS - BOOL-RISE, INT-VAL)
const rgbCol = [{rise : true, val : BACKGROUND_START_VAL_RED}, {rise : true, val : BACKGROUND_START_VAL_GREEN}, {rise : true, val : BACKGROUND_START_VAL_BLUE}];

//EMPTY ARRAY FOR SNOW
const snowHolder = [];

//STORE VALUE OF ENTIRE VIEWPORT
let storedWindowWidth = window.innerWidth;
let storedWindowHeight = window.innerHeight;

//SET SCREEN ELEMENT TO FULL WINDOW
screenElement.width = storedWindowWidth;
screenElement.height = storedWindowHeight;
fullPage.width = storedWindowWidth;
fullPage.height = storedWindowHeight;

//GENERATE BACKGROUND COLORS ???
UpdateScreenColor(rgbCol[0],rgbCol[1],rgbCol[2]);

//CREATING 1000 SNOW OBJECTS
for (let i = 0; i<TOTAL_SNOWFLAKES; i++) {
    snowHolder.push(new Snow());
}//END FOR LOOP

//UPDATES ANIMATION
UpdateAnimation();

//CHECKS FOR MOUSE-CLICK -- RUNS PUSHSNOW
addEventListener("mousedown", PushSnow);

//#endregion END MAIN


//#region HELPER FUNCTIONS

//PUSHES SNOW AWAY FROM MOUSE
function PushSnow(data) {    
    //STORE MOUSE CLICK POINTS
    let x = data.clientX;
    let y = data.clientY;    

    //PUSHING SNOWFLAKE BASED ON LOCATION TO MOUSE-CLICK
    for (let i = 0; i < snowHolder.length; i++) {
        CheckPush(snowHolder[i], x, y);
    }
}//END FUNCTION

function CheckPush(flake, x, y) {
    //HOW FAR SNOW WILL BE PUSHED AWAY ???
    let pushRadius = PUSH_RADIUS;
    
    //SET VARIABLE FOR X/Y ABSOLUTE DITANCE FROM CLICK
    let xProx = Math.abs(flake.x - x);
    let yProx = Math.abs(flake.y - y);

    //CONDITION IS TRUE IF SNOWFLAKE IS WITHIN PUSH RADIUS
    if (xProx < pushRadius && yProx < pushRadius) {
        
        //AFFECTING THE FORCE OF THE PUSHBACK
        flake.intensity = (1 - (xProx + yProx) / (pushRadius * 2));
        
        //LENGTH OF TIME THE SNOWFLAKES ARE IN HIT-STUN ???
        //HOW MANY POSITIONAL POINTS THE SNOWFLAKES WILL BE PUSHED BEFORE NORMAL MOVEMENT ???
        flake.pushTime = (Math.random() * 10) + SNOW_STUN_TIME_VECTOR;                                                              
        
        //CONTROLS DIRECTION OF PUSH x-for x y for y
        flake.pushdx = (flake.x - x) * flake.intensity * KNOCKBACK_MULTIPLIER;        
        flake.pushdy = (flake.y - y) * flake.intensity * KNOCKBACK_MULTIPLIER;
    }
}//END FUNCTION

//CREATE COLOR FOR SNOW
function RandomColor() {    
    return (
        "rgba(" +
        (Math.floor((Math.random() * (255 - SNOW_BASE_RED_VAL)) + SNOW_BASE_RED_VAL)) +
        ", " +
        (Math.floor((Math.random() * (255 - SNOW_BASE_GREEN_VAL)) + SNOW_BASE_GREEN_VAL)) + ", " +
        (Math.floor((Math.random() * (255 - SNOW_BASE_BLUE_VAL)) + SNOW_BASE_BLUE_VAL)) +
        ", " +
        Math.floor((Math.random() * (9 - SNOW_BASE_OPACITY_VAL)) +SNOW_BASE_OPACITY_VAL)/ 10 +
        ")"
    );
}//END FUNCTION

//REDRAW EACH FLAKE AT NEW POSITION ??
function UpdateSnow() {
    for (let i = 0; i < snowHolder.length; i++) {
        let currentSnow = snowHolder[i];
        
        currentSnow.Draw();
        currentSnow.sway += currentSnow.swayForce;
                
        if (currentSnow.pushTime <= 0) {
            currentSnow.y += currentSnow.dy;
            currentSnow.x += Math.sin(currentSnow.sway) + currentSnow.dx;
        } else {
            currentSnow.pushTime--;
            currentSnow.y += currentSnow.dy * .60;
            currentSnow.x += (Math.sin(currentSnow.sway) + currentSnow.dx) * .60;
            
            currentSnow.y += currentSnow.pushdy;
            currentSnow.x += currentSnow.pushdx;

            currentSnow.pushdy *= currentSnow.intensity;
            currentSnow.pushdx *= currentSnow.intensity;
            currentSnow.intensity *= KNOCKBACK_DECAY;
        }

        
        if ((currentSnow.y - currentSnow.radius >= storedWindowHeight) || (currentSnow.x >= storedWindowWidth + currentSnow.radius) || (currentSnow.x <= 0 - currentSnow.radius)) {
            snowHolder.splice(i, 1);
            snowHolder.push(new Snow(0));            
        }
    }    
}//END FUNCTION

function UpdateScreenColor() {
    screen.clearRect(0,0, screenElement.width, screenElement.height);
    screen.fillStyle=`rgba(${rgbCol[0].val},${rgbCol[1].val},${rgbCol[2].val}, 1)`;
    screen.fillRect(0,0,screenElement.width, screenElement.height);    
    
    for (let i = 0; i < rgbCol.length; i++) {
        rgbCol[i].val += rgbCol[i].rise ? BACKGROUND_COLOR_DELTA_ARRAY[i] : -BACKGROUND_COLOR_DELTA_ARRAY[i];
        if (rgbCol[i].val > BACKGROUND_MAX_VALUE || rgbCol[i].val < BACKGROUND_MIN_VALUE) {
            rgbCol[i].rise = !rgbCol[i].rise;
        }        
    }        
}//END FUNCTION

function UpdateAnimation() {
    if (storedWindowWidth != window.innerWidth || storedWindowHeight != window.innerHeight) {
        storedWindowHeight = window.innerHeight;
        storedWindowWidth = window.innerWidth;
        
        screenElement.width = storedWindowWidth;
        screenElement.height = storedWindowHeight;
        fullPage.width = storedWindowWidth;
        fullPage.height = storedWindowHeight;
    }
    UpdateScreenColor();
    UpdateSnow();
    requestAnimationFrame(UpdateAnimation);
}//END FUNCTION


function Debug(text) {
    console.log(text);
}//END FUNCTION

//#endregion