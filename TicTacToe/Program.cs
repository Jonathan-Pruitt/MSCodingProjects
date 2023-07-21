using Microsoft.VisualBasic;
using System.Drawing;

namespace TicTacToe {
    internal class Program {
        static void Main(string[] args) {

            //SET UP PLAYER ARRAYS (SYMBOLS, NAMES, COLORS)
            string[] playerNames    = new string[2];
            string[] symbols        = {"X", "O"};
            ConsoleColor[] colors   = {ConsoleColor.Red, ConsoleColor.Blue};

            //INTRODUCE GAME AND GATHER PLAYER NAMES
            NamePlayers(playerNames);

            //SET PERMANENT GAME-STATE LOOP
            while (true) {

                //INITIALIZE VARIABLES
                bool gameFinished = false;
                int playerTurn = 0;
                string[,] board = {{ "*", "*", "*"}, { "*", "*", "*"}, { "*", "*", "*" } };
                int winState = -1;
                string temporaryName = "";

                //SET INDIVIDUAL GAME LOOP
                while (!gameFinished) {

                    //RESET ANY BACKGROUND COLOR CHANGES
                    Console.BackgroundColor = ConsoleColor.Black;

                    //RUN BOARD DISPLAY FUNCTION TO SHOW CURRENT BOARD STATE
                    DisplayGame(board);

                    //UPDATE BOARD WITH MARKER
                    PlaceMarker(board, playerTurn, playerNames, symbols);

                    //CHECK FOR GAME COMPLETION STATE
                    //WINSTATE = CHECKFORWIN(BOARD, PLAYERTURN);
                    winState = CheckForWin(board, symbols[playerTurn]);
                    gameFinished = winState >= 0;

                    if (gameFinished) { //GAME COMPLETED
                        Console.Clear();
                        DisplayGame(board);

                        //DRAW
                        if (winState == 0) {
                            Console.WriteLine("\n\n\tTHE GAME IS A DRAW.");
                        
                        //PLAYER VICTORY
                        } else {
                            
                            temporaryName = playerNames[playerTurn];
                            temporaryName = temporaryName.ToUpper();
                            Console.ForegroundColor = colors[playerTurn];
                            Console.WriteLine($"\n\n\t{temporaryName} HAS WON THE GAME!");
                        }
                        Console.WriteLine("\t\tPRESS 'ENTER' TO PLAY AGAIN");
                        Console.ReadLine();

                    }// END GAME COMPLETED

                    //UPDATE PLAYER TURN
                    playerTurn++;
                    if (playerTurn > 1) {
                        playerTurn = 0;
                    }

                    Thread.Sleep(200); // <<<-- PURELY FOR AESTHETIC PURPOSES.

                    //CLEAR CONSOLE DISPLAY FOR CLEANER INTERFACE FOR USER.
                    Console.Clear();

                }//END TURN/GAME LOOP
            }//END PERMANENT GAME-STATE LOOP
        }//END MAIN

        static void DisplayGame(string[,] boardState) { 

            //BEGIN ROW LOOP
            for (int y = 0; y < boardState.GetLength(1); y++) {

                //BEGIN COLUMN LOOP
                for (int x = 0; x < boardState.GetLength(0); x++) {
                    //SET COLOR OF MARKERS FOR EASIER BOARD INTERPRETATION
                    if (boardState[x, y] == "X") {
                        Console.ForegroundColor = ConsoleColor.Red;
                    } else if (boardState[x, y] == "O") {
                        Console.ForegroundColor = ConsoleColor.Blue;
                    }
                    
                    //DISPLAY ALL MARKERS ON GAMEBOARD IN THE CHOSEN COLOR.
                    if (x != 2) { 
                        if (boardState[x, y] == "*") {
                            Console.Write(" ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" | ");
                        } else {
                            Console.Write(boardState[x, y]);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" | ");
                        }
                    } else {
                        if (boardState[x, y] == "*") {
                            Console.Write(" \n");
                        } else {
                            Console.Write($"{boardState[x, y]}\n");
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }//END COLUMN FOR LOOP

                //CREATE ROW SEPARATING LINE FOR AESTHETICS.
                if (y != 2) {
                    Console.WriteLine($"--+---+--");
                }
            }//END ROW FOR LOOP

        }//END DISPLAYGAME

        static void PlaceMarker(string[,] board, int playerTurn, string[] playerNames, string[] symbols) {
            int column_location;
            int row_location;
            bool openSpace = false;

            do { // VALIDATION LOOP TO COLLECT COLUMN/ROW AND VERIFY CHOSEN LOCATION IS AVAILABLE.
                column_location = TryPromptInt($"\n\n{playerNames[playerTurn]}:\nOn which row would you like to place your mark? ");
                while (column_location < 1 || column_location > 3) {
                    Console.WriteLine("Sorry, you must select row 1, 2, or 3.");
                    column_location = TryPromptInt($"Please enter a valid row for your mark? ");
                }

                row_location = TryPromptInt($"In which column would you like to place your mark? ");
                while (row_location < 1 || row_location > 3) {
                    Console.WriteLine("Sorry, you must select column 1, 2, or 3.");
                    row_location = TryPromptInt($"Please enter a valid column for your mark? ");
                }
                //CHECK IF CHOSEN LOCATION IS AVAILABLE WITH BOOL OPENSPACE.
                if (row_location > 0 && row_location < 4 && column_location > 0 && column_location < 4) {
                    openSpace = board[row_location - 1, column_location - 1] == "*";
                }
                if (!openSpace) {
                    Console.WriteLine("Sorry, you have selected a location that is already taken. Please select again.");
                }
            } while (!openSpace);

            //PLACE THE PLAYER'S SYMBOL AT THE CHOSEN LOCATION
            board[row_location - 1, column_location - 1] = symbols[playerTurn];

        }//END PLACEMARKER

        static void NamePlayers(string[] playerNames) {
            //INTRODUCE GAME
            Console.WriteLine("\n*************  WELCOME TO TIC-TAC-TOE!!!  *************\n\n");

            //ACQUIRE THE NAME OF PLAYER 1 AND INFORM THEM OF THEIR PLAYER SYMBOL
            playerNames[0] = PromptString("Please enter a name for Player 1: ");
            Console.WriteLine($"\t\t{playerNames[0]}, you will be 'X's.\n");

            //ACQUIRE THE NAME OF PLAYER 2 AND INFORM THEM OF THEIR PLAYER SYMBOL
            playerNames[1] = PromptString("Please enter a name for Player 2: ");
            Console.WriteLine($"\t\t{playerNames[1]}, you will be 'O's.\n");
            PromptString("\n\nPress ENTER to continue...");

            //CLEAR DISPLAY FOR CLEANER GAME PRESENTATION <<-- AESTHETIC PURPOSES ONLY
            Console.Clear();
        }

        #region WIN checking functions
        static bool ScanRow(string[,] board, int row, string symbol) {            
            return board[0, row] == symbol && board[1, row] == symbol && board[2, row] == symbol;
        }//End ScanRow
        static bool ScanColumn(string[,] board, int column, string symbol) {
            return board[column, 0] == symbol && board[column, 1] == symbol && board[column, 2] == symbol;
        }//End ScanColumn

        static bool ScanDiagonals(string[,] board, string symbol) {
            // check for descending diagonal win.
            if (board[0, 0] == symbol && board[1, 1] == symbol && board[2, 2] == symbol) {
                return true;
            }

            // check for ascending diagonal win.
            if (board[0, 2] == symbol && board[1, 1] == symbol && board[2, 0] == symbol) {
                return true;
            }

            //neither diagonals contained wins. 
            return false;
        }//End ScanDiagonals

        static int CheckForWin(string[,] board, string symbol) {
            bool winDetected = false;
            int winState = -1;
            int indexer = 0;

            //WIN = 1; DRAW = 0; GAME CONTINUES = -1

            //Check for a ROW win
            while (indexer < 3 && !winDetected) {
                winDetected = ScanRow(board, indexer, symbol);
                indexer++;
            }

            //RESET indexer and check for COLUMN win
            indexer = 0;
            while (indexer < 3 && !winDetected) {
                winDetected = ScanColumn(board, indexer, symbol);
                indexer++;
            }

            //Check for DIAGONALS win
            if (!winDetected) {
                winDetected = ScanDiagonals(board, symbol);
            }

            //RETURN A WIN IS DETECTED
            if (winDetected) {
                winState = 1;
                return winState;

            //Check for the ABSENCE of the DRAW state
            } else if (!winDetected) {
                for (int y = 0; y < board.GetLength(1); y++) {
                    for (int x = 0; x < board.GetLength(0); x++) {

                        //No Draw Detected.
                        if (board[x, y] == "*") {
                            winState = -1;
                            return winState;
                        }
                    }
                }
            }
            //No asterisks detected means a draw.
            winState = 0;
            return winState;


        }//End CheckForWin        
        
        #endregion

        #region Prompt Functions
        static double PromptDouble(string message) {
            Console.Write(message);
            return double.Parse(Console.ReadLine());
        }//End PromptDouble

        static string PromptString(string message) {
            Console.Write(message);
            return Console.ReadLine();
        }//End PromptString

        static int PromptInt(string message) {
            Console.Write(message);
            return int.Parse(Console.ReadLine());
        }//End PromptInt

        static int TryPromptInt(string message) {
            bool validInt = false;
            int parsedValue = 0;
            Console.Write(message);
            validInt = int.TryParse(Console.ReadLine(), out parsedValue);

            while (!validInt) {
                Console.WriteLine($"There has been an error. You must enter a valid whole number.\n{message}");
                validInt = int.TryParse(Console.ReadLine(), out parsedValue);
            }

            return parsedValue;

        }//end TryPromptInt

        static double TryPromptDouble(string message) {
            bool validDouble = false;
            double parsedValue = 0;
            Console.Write(message);
            validDouble = double.TryParse(Console.ReadLine(), out parsedValue);

            while (!validDouble) {
                Console.WriteLine($"There has been an error. You must enter a valid number.\n{message}");
                validDouble = double.TryParse(Console.ReadLine(), out parsedValue);
            }

            return parsedValue;

        }//end TryPromptInt
        #endregion
    }
}