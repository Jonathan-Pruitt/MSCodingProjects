//#region FIREWORK CLASS

/*LEGEND:
    FW  - FIREWORK
    SP  - SPARKS
    BG  - BACKGROUND
    S   - SIZE
    C   - COLOR
    SP  - SPEED
    AG  - ANGLE
    i   - initial
    d   - decay
    b   - base
    r   - range
*/
const FW_TOTAL      = 5;
const FW_RATE       = 3;
const FW_TAIL       = 50; 
const FW_GROW       = 0.5;
const FW_Smin       = 2;
const FW_Smax       = 65;
const FW_Sb         = 15;
const FW_Sr         = 15;
const FW_SPbi       = 7;
const FW_SPri       = 4;
const FW_SPdb       = 2;
const FW_SPdr       = 4;
const FW_X_AGr      = 10;
const FW_Cb_RED     = 220;  //(RANGE FROM 0 - 255)
const FW_Cb_GREEN   = 100;  //(RANGE FROM 0 - 255)
const FW_Cb_BLUE    = 85;  //(RANGE FROM 0 - 255)
const FW_OPACITYb   = 200; //(RANGE FROM 0 - 255)
const BG_Cmin_RED   = 10; //BEST LOOK IF MIN IS 20 - 50;
const BG_Cmin_GREEN = 10; //BEST LOOK IF MIN IS 20 - 50;
const BG_Cmin_BLUE  = 10; //BEST LOOK IF MIN IS 20 - 50;
const BG_C_ARRAY    = [BG_Cmin_RED, BG_Cmin_GREEN, BG_Cmin_BLUE];
const PUSH_RADIUS   = 100;




class Firework {
    //CONSTRUCTOR    
    constructor() {
        //CREATE SNOW PROPERTIES
        let colors = RandomColor();

        this.color = colors.main;
        this.tailColor = colors.tail;
        this.size = Math.random() * FW_Sb + FW_Sr;
        this.initSize = this.size;
        this.growth = (Math.random() - 0.75) * FW_GROW;
        this.tail = [];        
        
        let fifthWindow = storedWindowWidth * 0.2;
        let buffer = fifthWindow * 2;
        this.x = (Math.random() * fifthWindow) + buffer;
        this.y = storedWindowHeight + this.size * 3;
        
        this.dy = ((Math.random() * FW_SPri) + FW_SPbi) * -1;        
        this.dx = ((Math.random() - 0.5 ) * FW_X_AGr);
        
        this.decay = ((Math.random() * FW_SPdr) + FW_SPdb) / 50;
        this.endSpeed = this.dy * (Math.random() - .5);

    }//END CONSTRUCTOR

    //DRAWS THE SNOW TO CANVAS
    Draw = function () {            
        screen.beginPath();
        screen.fillStyle = this.color; //set circle color
        screen.fillRect(this.x, this.y, this.size, this.size);        
        for (let i = 0; i < this.tail.length; i++) {
            screen.fillStyle = this.tailColor;
            let size = this.size * ((i + 0.5) / this.tail.length);
            screen.fillRect(this.tail[i].x, this.tail[i].y, size, size);
        }                           
    }
    Destroy = function () {
        //CREATE SHAPE
        popHolder.push(new Pop(this.x, this.y, this.initSize, this.color));
    }

}//END CLASS

class Pop {
    constructor(xI, yI, size, color) {
        this.color = color;
        this.x = xI;
        this.y = yI;        
        this.radius = size;
        this.maxSize = (Math.random() * 100) + this.radius * this.radius;
        let upTime = Math.random() * 500 + 200;
        setTimeout(this.Destroy, upTime);

    }//END CONSTRUCTOR

    DrawPop = function () {
        screen.beginPath();
        screen.fillStyle = this.color;
        screen.arc(this.x, this.y, this.radius, 0, 2 * Math.PI);
        screen.fill();
        
    }//END FUNCTION

    Grow = function () {        
        if (this.radius >= this.maxSize) {
            return this.maxSize;
        }
        return this.radius *= 1.75;
    }//END FUNCTION

    Fade = function () {
        let color = this.color;
        let newColor = color.slice(0, 7);
        color = this.color;
        let newOpacity = parseInt(color.slice(7), 16);
        // newOpacity -= 0x1f;
        newOpacity -= 0xc;
        newOpacity = newOpacity < 0 ? 0 : newOpacity;
        newOpacity = newOpacity.toString(16).padStart(2, 0);
        newColor += newOpacity;

        return newColor;
    }

    Destroy = function () {
        let pop = popHolder[0];
        //DESTROY INTO SMALLER SPARKS
        let sparks = [];
        for (let i = 1; i < 4; i++) {
            let spX = (Math.random() - 0.5) * 100 + pop.x;
            let spY = (Math.random() - 0.5) * 100 + pop.y;
            sparks.push(new Sparks(spX, spY, pop.x, pop.y))
        }
        sparksHolder[sparksHolder.length] = sparks;
        setTimeout(KillSpark, 1000);
        popHolder.shift();
    }
}//END CLASS

class Sparks {
    constructor(xI, yI, parentX, parentY) {
        this.x = xI;
        this.y = yI;
        let colorOps = ["#FFFF00", "#FFA500", "#FF5500", "#FF2200"]
        this.color = colorOps[Math.floor(Math.random() * colorOps.length)];
        this.dimmer = 0;        

        this.dx = this.x > parentX ? Math.random() * 2 : Math.random() * -2;
        this.dy = this.y > parentY ? Math.random() * 2 : Math.random() * -2;
    }

    Draw = function () {
        for (let i = 0; i < 10; i++) {
            let display = this.color;            
            let opacity = Math.floor(Math.random() * 255) - this.dimmer;
            let randX = Math.random() * 360;
            let randY = Math.random() * 360;
            let xDist = (Math.random() - 0.5) * 200;
            let yDist = (Math.random() - 0.5) * 200;
            let xPos = Math.sin(randX) * xDist + this.x 
            let yPos = Math.sin(randY) * yDist + this.y 
            display += opacity.toString(16);
            screen.beginPath();
            screen.fillStyle = display;
            screen.fillRect(xPos, yPos, 5, 5);
        }
    }
}

//#endregion

//#region BEGIN MAIN

//ASSIGNS CANVAS ELEMENT TO VARIABLE
const screenElement = document.getElementById("canvasElement");

//CREATES SCREEN OBJECT
const screen = screenElement.getContext("2d");

//SETS VALUES OF BACKGROUND COLORS
const rgbCol = [BG_Cmin_RED, BG_Cmin_GREEN, BG_Cmin_BLUE];

//EMPTY ARRAY FOR SNOW
const fireworkHolder = [];
const popHolder = [];
const sparksHolder = [];

//STORE VALUE OF ENTIRE VIEWPORT
let storedWindowWidth = window.innerWidth;
let storedWindowHeight = window.innerHeight;

//SET SCREEN ELEMENT TO FULL WINDOW
screenElement.width = storedWindowWidth;
screenElement.height = storedWindowHeight;

UpdateScreenColor(rgbCol[0],rgbCol[1],rgbCol[2]);

//setInterval(CreateFirework, 1500);

for (let i = 0; i < FW_TOTAL; i++) {
    let rate = Math.random() * 1000 + (FW_RATE * 500 * i);
    setTimeout(CreateFirework, rate);
}


//UPDATES ANIMATION
UpdateAnimation();

//CHECKS FOR MOUSE-CLICK -- RUNS PUSHSNOW
addEventListener("mousedown", PopFirework);

//#endregion END MAIN


//#region HELPER FUNCTIONS

function CreateFirework() {
    if (fireworkHolder.length >= FW_TOTAL) {
        return;
    }
    fireworkHolder.push(new Firework());
}

//SHOULD SHOOT FIREWORK AT TARGET LOCATION
function PopFirework(data) {    
    //STORE MOUSE CLICK POINTS
    let x = data.clientX;
    let y = data.clientY;    

    //PUSHING SNOWFLAKE BASED ON LOCATION TO MOUSE-CLICK
    for (let i = 0; i < fireworkHolder.length; i++) {
        let destroyed = CheckPush(fireworkHolder[i], x, y);
        if (destroyed) {
            fireworkHolder.splice(i, 1);
            let rate = Math.random() * 1000 + (FW_RATE * 500)
            setTimeout(CreateFirework, rate);
            Flash();
        }
    }
}//END FUNCTION

function CheckPush(firework, x, y) {
    //HOW FAR SNOW WILL BE PUSHED AWAY ???
    let pushRadius = PUSH_RADIUS;
    
    //SET VARIABLE FOR X/Y ABSOLUTE DITANCE FROM CLICK
    let xProx = Math.abs(firework.x - x);
    let yProx = Math.abs(firework.y - y);

    //CONDITION IS TRUE IF SNOWFLAKE IS WITHIN PUSH RADIUS
    if (xProx < pushRadius && yProx < pushRadius) {
        
        firework.Destroy();
        return true;
    }
}//END FUNCTION

//CREATE COLOR
function RandomColor() {    

    let mainColor = "#";
    mainColor += (Math.floor((Math.random() * (255 - FW_Cb_RED)) + FW_Cb_RED)).toString(16);
    mainColor += (Math.floor((Math.random() * (255 - FW_Cb_GREEN)) + FW_Cb_GREEN)).toString(16);
    mainColor += (Math.floor((Math.random() * (255 - FW_Cb_BLUE)) + FW_Cb_BLUE)).toString(16);
    let opacity = (Math.floor((Math.random() * (255 - FW_OPACITYb)) + FW_OPACITYb));

    let subOpacity = "19";
    let subColor = mainColor;

    mainColor += opacity.toString(16);
    subColor += subOpacity.toString(16);

    return({main : mainColor, tail : subColor})
}//END FUNCTION

//REDRAW EACH FLAKE AT NEW POSITION ??
function UpdateFirework() {
    for (let i = 0; i < fireworkHolder.length; i++) {
        let firework = fireworkHolder[i];
        
        firework.Draw();
                
        firework.y += firework.dy;
        firework.x += firework.dx;
        firework.dy += firework.decay;

        if (firework.size > 4 && firework.size < 20) {
            firework.size += firework.growth;
        }

        if (firework.tail.length == FW_TAIL) {
            firework.tail.shift();
        }
        if (firework.tail.length < FW_TAIL) {
            firework.tail.push({x : firework.x, y : firework.y})
        }  


        //CHECK FOR TERMINAL SPEED
        if (firework.dy > firework.endSpeed) {
            //CREATE DESTRUCTION FUNCTION
            firework.Destroy();


            fireworkHolder.splice(i, 1);
            let rate = Math.random() * 1000 + (FW_RATE * 500)
            setTimeout(CreateFirework, rate);
            Flash();
            return;
        }        
        
        if ((firework.y + firework.size < 40) || (firework.x >= storedWindowWidth - 40) || (firework.x <= 40)) {
            firework.Destroy();
            
            fireworkHolder.splice(i, 1);
            let rate = Math.random() * 1000 + (FW_RATE * 500)
            setTimeout(CreateFirework, rate);                       
            Flash();
        }
    }    
}//END FUNCTION

function UpdatePop() {
    for (let i = 0; i < popHolder.length; i++) {
        let pop = popHolder[i];
        pop.DrawPop();
        pop.radius = pop.Grow();
        pop.color = pop.Fade();

        pop.y += 1;
    }
}

function UpdateSparks() {
    if (sparksHolder.length === 0) {return;}
    for (let i = 0; i < sparksHolder.length; i++) {
        let container = sparksHolder[i];
        for (let s = 0; s < container.length; s++) {
            let spark = container[s]
            spark.Draw();

            spark.x += spark.dx;
            spark.y += spark.dy;
            spark.dimmer += 4;
            if (spark.dimmer > 250) {
                spark.dimmer = 250;
            }
        }
    }
}

function Flash() {
    for (let i = 0; i < rgbCol.length; i++) {
        let value = Math.random() * 100 + 40;
        rgbCol[i] += value;
        if (rgbCol[i] > 255) {
            rgbCol[i] = 225;
        }
    }
}

function KillSpark() {
    sparksHolder.shift();
}

function UpdateScreenColor() {
    screen.clearRect(0,0, screenElement.width, screenElement.height);
    screen.fillStyle=`rgba(${rgbCol[0]},${rgbCol[1]},${rgbCol[2]}, 1)`;
    screen.fillRect(0,0,screenElement.width, screenElement.height); 
    
    for (let i = 0; i < rgbCol.length; i++) {
        if (rgbCol[i] > BG_C_ARRAY[i]) {
            rgbCol[i] -= 7;
            if (rgbCol[i] < 0) {
                rgbCol[i] = BG_C_ARRAY[i];
            }
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
    UpdateFirework();
    UpdatePop();
    UpdateSparks();
    requestAnimationFrame(UpdateAnimation);
}//END FUNCTION

//#endregion