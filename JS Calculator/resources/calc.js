
//DEFINE CLASS FOR CALCULATOR
    class Calculator {
        //FIELDS
        displayValue = "";

        constructor() {
            this.displayValue = "0";
        }//end constructor;
    }//end class


//CREATE INSTANCE OF CALCULATOR CLASS
    document.addEventListener("keydown", KeyInputDetected);
    const webCalculator = new Calculator();
    var operatorsArray = document.getElementsByClassName("operator");
    var numbersArray = document.getElementsByClassName("numbers");
    var allClearButton = document.getElementsByClassName("all-clear");
    var equalSignButton = document.getElementsByClassName("equal-sign");
    var historyTab = document.getElementById("history");
    let isOdd = true;
    let equalsJustPressed = false;

    for (var i = 0; i < operatorsArray.length; i++) {        
        operatorsArray[i].addEventListener("click", InputOperator);
    }
    for (var i = 0; i < numbersArray.length; i++) {
        numbersArray[i].addEventListener("click", InputDigit);
    }
    allClearButton[0].addEventListener("click", ResetCalculator)
    equalSignButton[0].addEventListener("click", ProcessInput)


function InputDigit(digit) {    
    if (equalsJustPressed) {
        webCalculator.displayValue = "";
        equalsJustPressed = false;
    }
    if (webCalculator.displayValue != "0" || digit.currentTarget.value == ".") {
        webCalculator.displayValue += digit.currentTarget.value;        
    } else {
        webCalculator.displayValue = digit.currentTarget.value;
    }
    UpdateDisplay(); 
}//end function

function InputOperator(operator) {
    var last = webCalculator.displayValue.length - 1;
    if (webCalculator.displayValue[last] != "+" && webCalculator.displayValue[last] != "*" && webCalculator.displayValue[last] != "/" && webCalculator.displayValue[last] != "-") {
        webCalculator.displayValue += operator.currentTarget.value;        
    } else {
        let replacement = "";
        for (var i = 0; i < last; i++) {            
            replacement += webCalculator.displayValue[i];
        }
        replacement += operator.currentTarget.value;
        webCalculator.displayValue = replacement;
    }   
    equalsJustPressed = false;
    UpdateDisplay(); 
}//end function

function ResetCalculator() {
    webCalculator.displayValue = "0";
    equalsJustPressed = false;
    UpdateDisplay();
}//end function

function UpdateDisplay() {
    var display = document.getElementById("display");
    display.value = webCalculator.displayValue;
}//end function

function ProcessInput() {
    var equation = webCalculator.displayValue;
    var result = eval(equation);
    let li = document.createElement("li");
    let node = document.createTextNode(equation + " = " + result);
    if (!isOdd) {
        li.classList.toggle("clicked");
    } else {
        li.classList.toggle("unclicked");
    }
    li.appendChild(node);
    historyTab.appendChild(li);
    webCalculator.displayValue = result;    
    isOdd = !isOdd;
    equalsJustPressed = true;
    UpdateDisplay();
}//end function

function KeyInputDetected(data) {        
    if ((data.key >= 0 && data.key <= 9) || data.key == ".") {
        if (equalsJustPressed) {
            webCalculator.displayValue = "";
            equalsJustPressed = false;
        }
        if (webCalculator.displayValue != "0" || data.key == ".") {
            webCalculator.displayValue += data.key;
        } else {
            webCalculator.displayValue = data.key;
        }
        UpdateDisplay();         
    } else if (data.key == "+" || data.key == "-" || data.key == "/" || data.key == "*") {
        var last = webCalculator.displayValue.length - 1;
        if (webCalculator.displayValue[last] != "+" && webCalculator.displayValue[last] != "*" && webCalculator.displayValue[last] != "/" && webCalculator.displayValue[last] != "-") {
            webCalculator.displayValue += data.key;
        } else {
            let replacement = "";
            for (var i = 0; i < last; i++) {            
                replacement += webCalculator.displayValue[i];
            }
            replacement += data.key;
            webCalculator.displayValue = replacement;
        }   
        equalsJustPressed = false;
        UpdateDisplay(); 
    } else if (data.key == "Enter") {
        ProcessInput();
    } else if (data.key == "Delete" || data.key == "Backspace") {
        ResetCalculator();
    }
}