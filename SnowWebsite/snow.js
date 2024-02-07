//#region SNOW CLASS

class Snow {
    //CONSTRUCTOR    
    constructor(location = null) {
        //CREATE SNOW PROPERTIES
        this.color = RandomColor();
        this.radius = Math.random() * 5 + 2;        
        
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
        screen.beginPath();
        screen.fillStyle = this.color; //set circle color
        screen.fillRect(this.x, this.y, this.radius, this.radius);                    
    }            
}//END CLASS

//#endregion

//#region BEGIN MAIN

//ASSIGNS CANVAS ELEMENT TO VARIABLE
const screenElement = document.getElementById("snowCan");

//CREATES SCREEN OBJECT
const screen = screenElement.getContext("2d");

//SETS BACKGROUND (ARRAY HOLDING OBJECTS - BOOL-RISE, INT-VAL)
const rgbCol = [{rise : true, val : 33}, {rise : true, val : 66}, {rise : true, val : 99}];

//EMPTY ARRAY FOR SNOW
const snowHolder = [];

//STORE VALUE OF ENTIRE VIEWPORT
let storedWindowWidth = window.innerWidth;
let storedWindowHeight = window.innerHeight;

//SET SCREEN ELEMENT TO FULL WINDOW
screenElement.width = storedWindowWidth;
screenElement.height = storedWindowHeight;

//GENERATE BACKGROUND COLORS ???
UpdateScreenColor(rgbCol[0],rgbCol[1],rgbCol[2]);

//CREATING 1000 SNOW OBJECTS
for (let i = 0; i<1000; i++) {
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
    let pushRadius = 100;
    
    //SET VARIABLE FOR X/Y ABSOLUTE DITANCE FROM CLICK
    let xProx = Math.abs(flake.x - x);
    let yProx = Math.abs(flake.y - y);

    //CONDITION IS TRUE IF SNOWFLAKE IS WITHIN PUSH RADIUS
    if (xProx < pushRadius && yProx < pushRadius) {
        
        //AFFECTING THE FORCE OF THE PUSHBACK
        flake.intensity = (1 - (xProx + yProx) / (pushRadius * 2));
        
        //LENGTH OF TIME THE SNOWFLAKES ARE IN HIT-STUN ???
        //HOW MANY POSITIONAL POINTS THE SNOWFLAKES WILL BE PUSHED BEFORE NORMAL MOVEMENT ???
        flake.pushTime = (Math.random() * 20) + (flake.intensity * 3);                           
        // flake.pushTime = 5000;                           
        
        //CONTROLS DIRECTION OF PUSH x-for x y for y
        flake.pushdx = (flake.x - x) * flake.intensity;        
        flake.pushdy = (flake.y - y) * flake.intensity;
    }
}//END FUNCTION

//CREATE COLOR FOR SNOW
function RandomColor() {    
    return (
        "rgba(" +
        (Math.floor((Math.random() * 55) + 200)) +
        ", " +
        (Math.floor((Math.random() * 55) + 200)) + ", " +
        (Math.floor((Math.random() * 55) + 200)) +
        ", " +
        Math.floor((Math.random() * 5) + 3)/ 10 +
        ")"
    );
}//END FUNCTION

//REDRAW EACH FLAKE AT NEW POSITION ??
function UpdateSnow() {
    for (let i = 0; i < snowHolder.length; i++) {
        let currentSnow = snowHolder[i];
        
        currentSnow.Draw();
        
        //CHECK FOR NEG/POS AND ASSIGN L/R DIRECTION ACCORDINGLY ???
        if (currentSnow.pushTime <= 0) {
            currentSnow.y += currentSnow.dy;

            currentSnow.sway += currentSnow.swayForce;
            currentSnow.x += Math.sin(currentSnow.sway) + currentSnow.dx;
        } else {
            currentSnow.pushTime--;
            currentSnow.y += currentSnow.pushdy;
            currentSnow.y += currentSnow.dy;
            currentSnow.x += currentSnow.pushdx;

            currentSnow.pushdy *= currentSnow.intensity;
            currentSnow.pushdx *= currentSnow.intensity;
            currentSnow.intensity *= .99;
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
        rgbCol[i].val += rgbCol[i].rise ? (i+1)*0.5 : (i+1)*-0.5;
        if (rgbCol[i].val > 100 || rgbCol[i].val < 10) {
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
    }
    UpdateScreenColor();
    UpdateSnow();
    requestAnimationFrame(UpdateAnimation);
}//END FUNCTION


function Debug(text) {
    console.log(text);
}//END FUNCTION

//#endregion