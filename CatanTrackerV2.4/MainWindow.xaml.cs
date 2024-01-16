using CatanTrackV2;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace CatanTracker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow:Window {

        //UPDATE VERSION INFORMATION HERE
        string version = "v2.4";

        //////NEEDS THOROUGH TESTING, BUT I THINK THIS VERSION NOW DISCERNS BETWEEN TOKEN/RESOURCE DUPES
        
        #region CLASS FIELDS
        bool isCatan = false;
        bool isSeafarers = false;

        Player[] players;
        GameBoard mainBoard;
        Button[] rollButtons = new Button[11];
        Button[] whackedButtons = new Button[6];
        Player selectedPlayer = null;
        int turnIndex = 0;
        int pipIndex = 0;
        int[] pipViewArray;
        int selectedRoll = -1;
        int buyPhase     = 0;
        int robberStage = 0; // 0 - ASK WHACK ; 1 - CHECK RESPONSE ; 2 - REQUEST TOKEN MOVE ; 3 - REQUEST RESOURCE HIT ; 4 - REQUEST PLAYER HIT ; 5 - RESOLVE HIT
        bool reverse = false;
        bool knightActive = false;
        bool knightExpended = false;
        bool robberActive = false;
        bool buyingSettlement = false;
        bool interruptingRoad = false;
        bool buyingCity = false;
        bool buyingRoad = false;
        bool hasRolled = false;
        SolidColorBrush buttonUnclicked = new SolidColorBrush();
        SolidColorBrush buttonClicked = new SolidColorBrush();

        Location[] tempArray;
        DispatcherTimer _displayTimer;

        #endregion
        
        public MainWindow() {
            
            
            InitializeComponent();
            
            //DISPLAY VERSION IN WINDOW
            this.Title = $"Catan Tracker {version}";
            
            Color buttonColor = new Color();
            buttonColor.A = 0xFF;
            buttonColor.R = buttonColor.G = buttonColor.B = 0xDD;
            buttonUnclicked.Color = buttonColor;
            buttonColor.R = 0x77;
            buttonColor.B = 0xFF;
            buttonColor.G = 0x00;
            buttonClicked.Color = buttonColor;
            rollButtons = GetRollButtons();
            whackedButtons = GetWhackedButtons();

            viewGameSelect.Visibility = Visibility.Visible;
            viewBoardSetup.Visibility = Visibility.Collapsed;
            viewPlayerStart.Visibility = Visibility.Collapsed;
            viewMainLoop.Visibility = Visibility.Collapsed;
            viewEndGame.Visibility = Visibility.Collapsed;

        }//END MAIN

        private Location ConvertIntToLocation(int token, string resource) {
            Location hex = new Location(token, resource);
            return hex;
        }//END METHOD

        private void playerCount_ValueChanged(object sender,RoutedPropertyChangedEventArgs<double> e) {
            string valueText = "Number of Players ";
            double value = playerCount.Value;
            valueText += value;
            playerLabel.Content = valueText;
        }//END METHOD

        private void confirmPlayerCount_Click(object sender,RoutedEventArgs e) {
            int numberOfPlayers = (int)playerCount.Value;

            players = new Player[numberOfPlayers];
            
            MessageBoxButton confirmation = MessageBoxButton.OKCancel;
            MessageBoxResult result = MessageBox.Show($"{numberOfPlayers} Players", "Do you wish to proceed?", confirmation);
            if (result == MessageBoxResult.OK) {
                playerLabel.Content = $"What is the name of Player 1?";
                nameInput.Visibility = Visibility.Visible;
                playerCount.Visibility = Visibility.Collapsed;
                confirmPlayerCount.Visibility = Visibility.Collapsed;            
                nameInput.Visibility = Visibility.Visible;
                settlementLabel.Visibility = Visibility.Visible;
                settlementCount.Visibility = Visibility.Visible;
                confirmOwnedNumbers.Visibility = Visibility.Visible;
            }//END IF
        }//END METHOD

        private void confirmOwnedNumbers_Click(object sender,RoutedEventArgs e) {
            if (nameInput.Text.Length < 3 || settlementCount.Text.Length < 1) {
                return;
            }            
            string playerName = nameInput.Text;
            int[] ownArray = ParseStringToIntArray(settlementCount.Text);

            tempArray = new Location[3];
            players[turnIndex] = new Player(playerName, turnIndex + 1);

            pipViewArray = ownArray;

            viewPlayerStartSub.Visibility = Visibility.Visible;
            lblHexQuestion.Content = $"What resource is related to your settlement with the {pipViewArray[pipIndex]} pip?";
        }

        private void confirmOwnedNumbersReverse_Click(object sender,RoutedEventArgs e) {
            if (settlementCount.Text.Length < 1) {
                return;
            }
            int[] ownArray = ParseStringToIntArray(settlementCount.Text);
            
            tempArray = new Location[3];
            
            pipViewArray = ownArray;
            viewPlayerStartSub.Visibility = Visibility.Visible;
            lblHexQuestion.Content = $"What resource is related to your settlement with the {pipViewArray[pipIndex]} pip?";           
        }
        
        private void UpdateInfoGatherScreen(int currentTurn) {
            nameInput.Text = "";
            settlementCount.Text = "";
            settlementLabel.Content = $"List the numbers Player {currentTurn + 1} has settlements on (separate each value with a SPACE)";
            playerLabel.Content = $"What is the name of Player {currentTurn + 1}?";
            if (players[currentTurn] != null) {
                nameInput.Text = players[currentTurn].Name;
                nameInput.IsReadOnly = true;
                nameInput.Visibility = Visibility.Collapsed;
                settlementLabel.Content = $"List the numbers {players[currentTurn].Name} has settlements on (separate each value with a SPACE)";
                playerLabel.Content = players[currentTurn].Name;
            }
        }//END METHOD

        private void setupBtn_Click(object sender,RoutedEventArgs e) {
            MessageBoxResult result = PauseAndAsk("Are the numbers entered accurate?", "Verify Inputs");
            if (result == MessageBoxResult.No) {
                return;
            }
            int[] brickArray = ParseStringToIntArray(setupBrick.Text);
            int[] goldArray = ParseStringToIntArray(setupGold.Text);
            int[] oreArray = ParseStringToIntArray(setupOre.Text);
            int[] sheepArray = ParseStringToIntArray(setupSheep.Text);
            int[] wheatArray = ParseStringToIntArray(setupWheat.Text);
            int[] woodArray = ParseStringToIntArray(setupWood.Text);


            int totalCount = brickArray.Length + goldArray.Length + oreArray.Length + sheepArray.Length + wheatArray.Length + woodArray.Length;
            Location[] allLocations = new Location[totalCount];            
            for (int i = 0; i < brickArray.Length; i++) {
                allLocations[i] = ConvertIntToLocation(brickArray[i], "BRICK");
            }
            for (int i = 0; i < goldArray.Length; i++) {
                allLocations[i + brickArray.Length] = ConvertIntToLocation(goldArray[i],"GOLD");
            }
            for (int i = 0; i < oreArray.Length; i++) {
                allLocations[i + brickArray.Length + goldArray.Length] = ConvertIntToLocation(oreArray[i], "ORE");
            }
            for (int i = 0; i < sheepArray.Length; i++) {
                allLocations[i + brickArray.Length + goldArray.Length + oreArray.Length] = ConvertIntToLocation(sheepArray[i], "SHEEP"); 
            }
            for (int i = 0; i < wheatArray.Length; i++) {
                allLocations[i + brickArray.Length + goldArray.Length + oreArray.Length + sheepArray.Length] = ConvertIntToLocation(wheatArray[i], "WHEAT");
            }
            for (int i = 0; i < woodArray.Length; i++) {
                allLocations[i + brickArray.Length + goldArray.Length + oreArray.Length + sheepArray.Length + wheatArray.Length] = ConvertIntToLocation(woodArray[i], "WOOD");
            }
            
            mainBoard = new GameBoard(allLocations);            
            
            viewBoardSetup.Visibility = Visibility.Collapsed;
            viewPlayerStart.Visibility = Visibility.Visible;
        }//END METHOD

        private int[] ParseStringToIntArray(string text) {
            string initialText = text;
            string validText = "";
            for (int i = 0; i < initialText.Length; i++) {
                if ((initialText[i] >= 48 && initialText[i] <= 57) || initialText[i] == 32) {
                    validText += initialText[i];
                }
            }
            string[] numbersArray = validText.Split(' ');
            int[] ownArray = new int[numbersArray.Length];
            int finalArraySize = 0;
            for (int i = 0; i < numbersArray.Length; i++) {     
                if (numbersArray[i] != "") {
                    ownArray[i] = int.Parse(numbersArray[i]);
                    finalArraySize++;
                }
            }
            int[] numArray = new int[finalArraySize];
            int index = 0;
            for (int i = 0; i < numbersArray.Length; i++) {     
                if (numbersArray[i] != "") {
                    numArray[index++] = int.Parse(numbersArray[i]);
                }
            }
            return numArray;
        }//END METHOD

        private void btnResource_Click(object sender,RoutedEventArgs e) {
            Button resource = sender as Button;
            string text = resource.Content.ToString();
            
            if (mainBoard.Exists(pipViewArray[pipIndex], text)) {
                
                if (mainBoard.GetLocation(pipViewArray[pipIndex], text).IsDuplicate) {
                    Location[] check = mainBoard.GetDuplicatesByToken(pipViewArray[pipIndex]);
                    int index = 0;
                    bool found = false;                     
                    while (!found) {
                        MessageBoxResult result = PauseAndAsk($"Is this settlement on {check[index].LocationToString()}?", "Duplicate Resource/Token Combo Detected!");
                        if (result == MessageBoxResult.Yes) {
                            found = true;
                            tempArray[pipIndex] = check[index];
                        } else {
                            index++;
                        }
                        index = index == check.Length ? 0 : index;
                    }//END LOOP
                } else {
                    tempArray[pipIndex] = mainBoard.GetLocation(pipViewArray[pipIndex], text); 
                }
                
            } else {
                SetError("This token does not correspond to the given resource");
                return;
            }

            ClearError();
            pipIndex++;
            if (pipIndex < pipViewArray.Length) {
                lblHexQuestion.Content = $"What resource is related to your settlement with the {pipViewArray[pipIndex]} pip?";
                return;
            }

            Settlement settlement = new Settlement(tempArray[0], tempArray[1], tempArray[2]);
            tempArray = null;
            pipViewArray = null;
            players[turnIndex].AddSettlement(settlement);
            pipIndex = 0;

            if (!reverse) {
                turnIndex++;
                if (turnIndex > players.Length - 1) {
                    //MOVE TO NEXT GAME PHASE
                    turnIndex--;
                    reverse = true;
                    confirmOwnedNumbersReverse.Visibility = Visibility.Visible;
                    confirmOwnedNumbers.Visibility = Visibility.Collapsed;
                    UpdateInfoGatherScreen(turnIndex);
                } else {
                    UpdateInfoGatherScreen(turnIndex);
                }
            } else {
                turnIndex--;
                if (turnIndex < 0) {
                    //MOVE TO NEXT GAME PHASE
                    viewPlayerStart.Visibility = Visibility.Collapsed;
                    viewMainLoop.Visibility = Visibility.Visible;
                    foreach (Player player in players) {
                        player.OpeningSettlements = player.GetSettlements();
                    }
                    InstantiateDispatchTimer();
                    mainBoard.StartGameTimer();
                    pipIndex = 0;
                    turnIndex = 0;
                    turnName.Content = $"{players[turnIndex].Name}'s Turn {mainBoard.Round}";
                    lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";
                    btnTurn.IsEnabled = false;
                    btnGameOver.IsEnabled = false;
                } else {
                    UpdateInfoGatherScreen(turnIndex);
                }
            }
            viewPlayerStartSub.Visibility = Visibility.Collapsed;
        }//END METHOD

        #region VERIFIED GOOD METHODS
        private void SetError(string error) {
            lblErrorText.Content = error;
            lblErrorText.Visibility = Visibility.Visible;
        }//END METHOD

        private void ClearError() {
            lblErrorText.Visibility = Visibility.Collapsed;
            lblErrorText.Content = "";
        }

        private void btnRoll_click(object sender,RoutedEventArgs e) {
            Button resource = sender as Button;
            int roll = int.Parse(resource.Content.ToString());
            selectedRoll = roll;
            SetButtonsUnclicked(rollButtons);
            resource.Background = buttonClicked;
            confirmRoll.Visibility = Visibility.Visible;            
        }//END METHOD

        private Label GetLabel(int rollNumber) {
            Label label = null;
            switch(rollNumber) {
                case 2: label = lbl2; break;
                case 3: label = lbl3; break;
                case 4: label = lbl4; break;
                case 5: label = lbl5; break;
                case 6: label = lbl6; break;
                case 7: label = lbl7; break;
                case 8: label = lbl8; break;
                case 9: label = lbl9; break;
                case 10: label = lbl10; break;
                case 11: label = lbl11; break;
                case 12: label = lbl12; break;
                default: return null;
            }
            return label;
        }//END METHOD

        private Button[] GetRollButtons() {
            Button[] buttons = new Button[11];
            buttons[0] = btn2;
            buttons[1] = btn3;
            buttons[2] = btn4;
            buttons[3] = btn5;
            buttons[4] = btn6;
            buttons[5] = btn7;
            buttons[6] = btn8;
            buttons[7] = btn9;
            buttons[8] = btn10;
            buttons[9] = btn11;
            buttons[10] = btn12;
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].Background = buttonUnclicked;
            }
            return buttons;
        }//END METHOD

        private Button[] GetWhackedButtons() {
            Button[] buttons = new Button[6];
            buttons[0] = p1;
            buttons[1] = p2;
            buttons[2] = p3;
            buttons[3] = p4;
            buttons[4] = p5;
            buttons[5] = p6;
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].Background = buttonUnclicked;
            }
            return buttons;
        }//END METHOD

        private void WhackedButton_Click(object sender,RoutedEventArgs e) {
            Button resource = sender as Button;
            if (resource.Background == buttonUnclicked) {
                resource.Background = buttonClicked;
            } else {
                resource.Background = buttonUnclicked;
            }
        }//END METHOD

        private void SetButtonsUnclicked(Button[] buttons) {
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].Background = buttonUnclicked;
            }
        }//END METHOD

        private void DisableRollButtons() {
            for (int i = 0; i < rollButtons.Length; i++) {
                rollButtons[i].IsEnabled = false;
            }
        }//END METHOD

        private void EnableRollButtons() {
            for (int i = 0; i < rollButtons.Length; i++) {
                rollButtons[i].IsEnabled = true;
            }
        }//END METHOD


        #endregion

        private void confirmRoll_Click(object sender,RoutedEventArgs e) {
            Label label = null;
            if (selectedRoll != -1) {
                label = GetLabel(selectedRoll);
            }
            if (label != null) {
                mainBoard.AddRoll(selectedRoll);
                label.Content = mainBoard.GetRollCount(selectedRoll);
            }
            SetButtonsUnclicked(rollButtons);
            confirmRoll.Visibility = Visibility.Collapsed;
            DisableRollButtons();
            if (selectedRoll == 7) { 
                viewLstBxSelection.Visibility = Visibility.Visible;
                btnKnight.IsEnabled = false;
                btnVicPoint.IsEnabled = false;
                robberActive = true;
                MoveRobber(robberStage);
            } else {
                viewMainLoopPostRollOptions.Visibility = Visibility.Visible;
                lblRobber.Content = UpdatePlayerResources(selectedRoll);
                lblRobber.Visibility = Visibility.Visible;
            }
            selectedRoll = -1;
            btnTurn.IsEnabled = true;
            hasRolled = true;
        }//END METHOD


        private void btnTurn_Click(object sender,RoutedEventArgs e) {
            MessageBoxResult result = PauseAndAsk($"Is {players[turnIndex].Name}'s turn finished?", "Confirm Turn End");
            if (result == MessageBoxResult.No) {
                return;
            }
            //LOG PLAYER'S TURN TIME
            TimeSpan turn = mainBoard.EndTurnTimer();
            players[turnIndex].AddTurnTime(turn);
            
            //UPDATE TURN
            turnIndex++;
            if (turnIndex >= players.Length) {
                mainBoard.Round++;
                turnIndex = 0;
                mainBoard.AddRound(players, mainBoard.GetGameTime());
            }//END IF
            turnName.Content = $"{players[turnIndex].Name}'s Turn {mainBoard.Round}";
            string longestOrLargest = "";
            if (players[turnIndex].HasLongestRoad) {longestOrLargest += "LongRoad ";}
            if (players[turnIndex].HasLargestArmy) {longestOrLargest += "LargeArmy";}
            lblBonus.Content = longestOrLargest;

            //REFRESH GUI VIEW OF OPTIONS
            EnableRollButtons();
            viewMainLoopPostRollOptions.Visibility = Visibility.Collapsed;
            btnKnight.IsEnabled = true;
            robberStage = 0;
            lblRobber.Visibility = Visibility.Collapsed;
            knightExpended = false;
            hasRolled = false;
            lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";
            EnableOptions();
            btnTurn.IsEnabled = false;
            
        }//END METHOD

        private void btnKnight_Click(object sender,RoutedEventArgs e) {
            viewLstBxSelection.Visibility = Visibility.Visible;
            lstBxSelection.Visibility = Visibility.Collapsed;
            players[turnIndex].Knight += 1;
            int armyCheck = 2;
            int largestNow = mainBoard.LargestArmy == null ? 0 : mainBoard.LargestArmy.Knight;
            for (int i = 0; i < players.Length; i++) {
                players[i].HasLargestArmy = false;
                if (players[i].Knight > armyCheck) {
                    if (players[i].Knight > largestNow) {
                        mainBoard.LargestArmy = players[i];
                        players[i].HasLargestArmy = true;
                    }
                }
            }
            if (mainBoard.LargestArmy != null) {
                mainBoard.LargestArmy.HasLargestArmy = true;
            }
            btnKnight.IsEnabled = false;
            knightActive = true;
            knightExpended = true;
            viewMainLoopPostRollOptions.Visibility = Visibility.Collapsed;
            MoveRobber(robberStage = 2);
            string longestOrLargest = "";
            if (players[turnIndex].HasLongestRoad) {longestOrLargest += "LongRoad ";}
            if (players[turnIndex].HasLargestArmy) {longestOrLargest += "LargeArmy";}
            lblBonus.Content = longestOrLargest;
            lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";
            DisableRollButtons();
        }//END METHOD

        private void MoveRobber(int robStage) {
            lblRobber.Visibility = Visibility.Visible;
            DisableOptions();
            //ASK IF ANYONE GOT WHACKED
            if (robStage == 0) {
                lblRobber.Content = "Select everyone who got whacked.";
                for (int i = 0; i < players.Length; i++) {
                    whackedButtons[i].Content = players[i].Name;
                    whackedButtons[i].Visibility = Visibility.Visible;
                }
            }
            
            //RETURN WHACKED RESPONSE
            if (robStage == 1) {
                for (int i = 0; i < players.Length; i++) {
                    if (whackedButtons[i].Background == buttonClicked) {
                        players[i].WhackedCount += 1;
                    }
                }
                SetButtonsUnclicked(whackedButtons);
                for (int i = 0; i < players.Length; i++) {
                    whackedButtons[i].Visibility = Visibility.Collapsed;
                }
                robStage = 2;
                robberStage = 2;
            }
            
            //SELECT TOKEN TO MOVE ROBBER ONTO
            if (robStage == 2) {                
                txtPipSelect.Visibility = Visibility.Visible;
                lblRobber.Content = "Enter the token number you blocked. (Enter 0 to BLOCK desert)";
                if (isSeafarers) {
                    lblRobber.Content += "\nOR\t[Enter 'P' to move Pirate]";
                }
            }

            //SELECT TYPE OF RESOURCE BEING BLOCKED
            if (robStage == 3) {
                string response = txtPipSelect.Text.ToUpper();
                if (response == "0") {
                    knightActive = false;
                    robberActive = false;
                    viewLstBxSelection.Visibility = Visibility.Collapsed;
                    lblRobber.Content = $"{players[turnIndex].Name} was super nice and blocked the desert.";
                    mainBoard.Blocked = null;
                    robberStage = 0;
                    if (!hasRolled) {
                        EnableRollButtons();
                    } else {
                        viewMainLoopPostRollOptions.Visibility = Visibility.Visible;
                    }
                    EnableOptions();
                    return;
                } else if (response.Contains("P")) {

                        //PLAYER IS MOVING PIRATE
                    lblRobber.Content = $"Which player (if any) are you stealing from?";
                    Player[] stealable = new Player[players.Length];
                    for (int i = 0; i < players.Length; i++) {
                        if (players[i] == players[turnIndex]) {
                            stealable[i] = null;
                        } else {
                            stealable[i] = players[i];
                        }
                    }
                    PopulateListBox(stealable, true);
                    lstBxSelection.Items.Add("No One");
                    txtPipSelect.Visibility = Visibility.Collapsed;
                    lstBxSelection.Visibility = Visibility.Visible;
                    txtPipSelect.Text = "";
                    robberStage = 4;
                    return;
                }
                string validText = "";
                for (int i = 0; i < response.Length; i++) {
                    if ((response[i] >= 48 && response[i] <= 57) || response[i] == 32) {
                        validText += response[i];
                    }
                }
                if (validText == "") {
                    robberStage = 2;
                    MoveRobber(2);
                    return;
                }
                int blockedToken = int.Parse(validText);
                if (blockedToken < 2 || blockedToken > 12 || blockedToken == 7) {
                    MoveRobber(2);
                    robberStage = 2;
                    return;
                }
                mainBoard.BlockedNumber = blockedToken;
                txtPipSelect.Visibility = Visibility.Collapsed;
                txtPipSelect.Text = "";
                //lblRobber.Content = "Select the resource you blocked";
                
                lstBxSelection.Visibility = Visibility.Visible;

                mainBoard.BlockedNumber = blockedToken;

                lblRobber.Content = "Select the location you blocked";
                Location[] options = mainBoard.GetLocationsByToken(blockedToken);
                string[] optStrings = new string[options.Length];
                    for (int i = 0; i < options.Length; i++) {
                        optStrings[i] = options[i].LocationToString();
                    }
                PopulateListBox(optStrings);


                /*PopulateListBox(mainBoard.GetResourcesByToken(blockedToken));
                if (lstBxSelection.Items.IsEmpty) {
                    lstBxSelection.Items.Add("Unknown");
                }
                */

            }

            //SELECT PLAYERS BEING STOLEN FROM
            if (robStage == 4) {

                if (lstBxSelection.SelectedIndex == -1) {
                    robberStage = 3;
                    return;
                }

                Location[] options = mainBoard.GetLocationsByToken(mainBoard.BlockedNumber);
                mainBoard.BlockLocation(options[lstBxSelection.SelectedIndex]);

                /*
                mainBoard.BlockedResource = lstBxSelection.SelectedItem.ToString();
                mainBoard.BlockLocation(mainBoard.BlockedNumber, mainBoard.BlockedResource);
                */

                mainBoard.BlockedNumber = 0;
                mainBoard.BlockedResource = "";
                lstBxSelection.Items.Clear();
                lblRobber.Content = "Select the player you stole from.";

                Player[] stealable = GetPlayersByLocation(mainBoard.Blocked);
                for (int i = 0; i < stealable.Length; i++) {

                    if (stealable[i] == players[turnIndex]) { stealable[i] = null; }
                }
                
                PopulateListBox(stealable, true);
            }
            
            //RESOLVE OUTCOME OF STEALING
            if (robStage == 5) {
                if (lstBxSelection.SelectedIndex == -1) {
                    robberStage = 4;
                    return;
                }
                string selectedPlayer = lstBxSelection.SelectedItem.ToString();
                MessageBoxResult result = MessageBoxResult.No;
                if (selectedPlayer != "No One") {
                    MessageBoxButton confirmation = MessageBoxButton.YesNo;
                    result = MessageBox.Show($"Did {selectedPlayer} have any cards?", "Cards Acquired?", confirmation);
                }
                for (int i = 0; i < players.Length; i++) {
                    if (selectedPlayer == players[i].Name) {
                        if (knightActive) {
                            players[turnIndex].SetKnightActivity(selectedPlayer, true, result == MessageBoxResult.Yes);
                            players[i].SetKnightActivity(players[turnIndex].Name, false, result == MessageBoxResult.Yes);
                        } else {
                            players[turnIndex].SetRobberActivity(selectedPlayer, true, result == MessageBoxResult.Yes);
                            players[i].SetRobberActivity(players[turnIndex].Name, false, result == MessageBoxResult.Yes);
                        }
                    } else if (selectedPlayer == "No One") {
                        players[turnIndex].SetKnightActivity(selectedPlayer, true, false);
                    }
                }
                lstBxSelection.Items.Clear();
                btnVicPoint.IsEnabled = true;
                if (!knightActive) {
                    btnKnight.IsEnabled = true;
                } else if (!hasRolled) {
                    EnableRollButtons();                    
                }
                if (hasRolled) {
                    viewMainLoopPostRollOptions.Visibility = Visibility.Visible;
                }
                knightActive = false;
                robberActive = false;
                robberStage = 0;
                viewLstBxSelection.Visibility = Visibility.Collapsed;
                lblRobber.Content = $"{players[turnIndex].Name} bopped {selectedPlayer}";                  
                EnableOptions();
            }
        }//END METHOD

        private void PopulateListBox(object[] array, int restriction = -1) {
            for (int i = 0; i < array.Length; i++) {
                if (i != restriction && (array[i] != "" || array[i] != null)) {
                    lstBxSelection.Items.Add(array[i]);
                }
            }
        }//END METHOD

        private void PopulateListBox(Player[] array, bool getName) {
            bool empty = true;
            for (int i = 0; i < array.Length; i++) {
                if (array[i] != null) {
                    lstBxSelection.Items.Add(array[i].Name);
                    empty = false;
                }
            }
            if (empty) {
                lstBxSelection.Items.Add("No One");
            }
        }//END METHOD

        
        private Player[] GetPlayersByLocation(Location target) {
            int index = 0;
            //Player[] tempPlayers = new Player[testPlayers.Length];
            //foreach (Player player in testPlayers) {
            Player[] tempPlayers = new Player[players.Length];
            foreach (Player player in players) {
                if (player.IsOnLocation(target)) { tempPlayers[index++] = player; }
            }
            Player[] foundPlayers = new Player[index];
            for (int i = 0; i < foundPlayers.Length; i++) {
                foundPlayers[i] = tempPlayers[i];
            }
            return foundPlayers;
        }
        
        
        private void btnVicPoint_Click(object sender,RoutedEventArgs e) {
            players[turnIndex].AddVictoryPoint();
            lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";
            lblRobber.Visibility = Visibility.Visible;
            lblRobber.Content = $"{players[turnIndex].Name} played a victory point from their Development Cards";
        }//END METHOD

        private void btnChit_Click(object sender,RoutedEventArgs e) {
            players[turnIndex].ChitPoints++;
            players[turnIndex].AddVictoryPoint();
            lblRobber.Visibility = Visibility.Visible;
            lblRobber.Content = $"{players[turnIndex].Name} received a chit victory point.";
            lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";
        }//END METHOD

        private void btnGameOver_Click(object sender,RoutedEventArgs e) {
            MessageBoxResult result = PauseAndAsk("Has the game concluded?", "Confirm Game Over");
            if (result == MessageBoxResult.No) {
                return;
            }
            viewMainLoop.Visibility = Visibility.Collapsed;
            viewEndGame.Visibility = Visibility.Visible;
            mainBoard.StopGameTimer();
            TimeSpan turn = mainBoard.GetTurnTime();
            players[turnIndex].AddTurnTime(turn);
            mainBoard.AddRound(players, mainBoard.GetGameTime());

            //ASK WHO WON IN A MESSAGE BOX OR SOMETHING
            bool winnerFound = false;
            int index = 0;
            while (!winnerFound) {
                MessageBoxResult winner = PauseAndAsk($"Did {players[index].Name} win?", "SELECT WINNER");
                if (winner == MessageBoxResult.Yes) {
                    mainBoard.Winner = players[index];
                    players[index].IsWinner = true;
                    winnerFound = true;
                } else {
                    index += 1;
                }
                index = players.Length == index ? 0 : index;
            }//END WINNER FINDING LOOP

            RowDefinition[] playerStats = new RowDefinition[players.Length];
            for (int i = 0; i < playerStats.Length; i++) {
                playerStats[i] = new RowDefinition();
                grdPlayerResults.RowDefinitions.Add(playerStats[i]);
                players[i].EndStats(grdPlayerResults, i + 1);
            }//END LOOP

            SetEndGameStats(mainBoard);
        }//END METHOD

        private void SetEndGameStats(GameBoard game) {
            TextBlock[] rollInfo = new TextBlock[11];
            for (int i = 0; i < game.RollCounter.Length; i++) {
                rollInfo[i] = new TextBlock();
                rollInfo[i].HorizontalAlignment = HorizontalAlignment.Center;
                rollInfo[i].FontWeight = FontWeights.Bold;
                rollInfo[i].FontSize = 15;
                rollInfo[i].Text = game.RollCounter[i].ToString();
                Grid.SetRow(rollInfo[i], 1);
                Grid.SetColumn(rollInfo[i], i);
                grdRollStats.Children.Add(rollInfo[i]);
            }//END LOOP

            TextBlock[] mainStats = new TextBlock[3];
            for (int i = 0; i < mainStats.Length; i++) {
                mainStats[i] = new TextBlock();
                mainStats[i].FontSize = 15;
                mainStats[i].HorizontalAlignment = HorizontalAlignment.Center;
                mainStats[i].VerticalAlignment = VerticalAlignment.Center;
                mainStats[i].FontWeight = FontWeights.Bold;
                Grid.SetRow(mainStats[i], 0);
                Grid.SetColumn(mainStats[i], i);
            }
            mainStats[0].Text = game.GetGameTime().ToString(@"hh\:mm\:ss");
            mainStats[1].Text = $"Total Rounds: {game.Round}";
            mainStats[2].Text = $"Winner: {game.Winner.Name}";
            grdGameResults.Children.Add(mainStats[0]);
            grdGameResults.Children.Add(mainStats[1]);
            grdGameResults.Children.Add(mainStats[2]);
        }//END METHOD

        private void btnDevCard_Click(object sender,RoutedEventArgs e) {
            players[turnIndex].DevCardsBought++;
            lblRobber.Visibility = Visibility.Visible;
            lblRobber.Content = $"{players[turnIndex].Name} bought a Development Card";
        }//END METHOD

        private void btnRoad_Click(object sender,RoutedEventArgs e) {
            Button selection = sender as Button;
            string action = "bought";
            string item = "road";
            MessageBoxResult isLonger = PauseAndAsk("Will this action affect the length of your LONGEST ROAD?", "Trade Route Action");
            if (selection.Name == "btnRoad") {
                players[turnIndex].RoadCount++;
                lblRobber.Visibility = Visibility.Visible;
                lstBxSelection.Visibility = Visibility.Collapsed;
            } else if (selection.Name == "btnShip") {
                players[turnIndex].ShipCount++;
                lblRobber.Visibility = Visibility.Visible;
                lstBxSelection.Visibility = Visibility.Collapsed;
                item = "ship";
            } else if (selection.Name == "btnMove") {
                players[turnIndex].MovedShips++;
                lblRobber.Visibility = Visibility.Visible;
                lstBxSelection.Visibility = Visibility.Collapsed;
                action = "moved";
                item = "ship";
            }
            
            if (isLonger == MessageBoxResult.Yes) {
                buyingRoad = true;
                lblRobber.Content = $"What is {players[turnIndex].Name.ToUpper()}'s longest Road now?";
                viewLstBxSelection.Visibility = Visibility.Visible;
                txtPipSelect.Visibility = Visibility.Visible;
                DisableOptions();
            } else {
                lblRobber.Content = $"{players[turnIndex].Name} {action} a {item}.";
            }
        }//END METHOD

        private void btnSettlement_Click(object sender,RoutedEventArgs e) {
            lstBxSelection.Items.Clear();
            lstBxSelection.Visibility = Visibility.Collapsed;
            buyingSettlement = true;
            lblRobber.Visibility = Visibility.Visible;
            lblRobber.Content = $"What are the number tokens adjacent to your settlement?\n\t[Enter 0 for desert]\t(Separate with SPACE)";
            viewLstBxSelection.Visibility = Visibility.Visible;
            txtPipSelect.Visibility = Visibility.Visible;
            txtPipSelect.MaxLength = 15;
            MessageBoxResult result = PauseAndAsk("Did your settlement shorten any other player's longest road?", "Check Point");
            if (result == MessageBoxResult.Yes) {
                interruptingRoad = true;
                bool foundTarget = false;
                int index = 0;
                while (!foundTarget) {
                    if (index != turnIndex) {
                        MessageBoxResult playerFound = PauseAndAsk($"Was {players[index].Name}'s road affected?", "Which Player's Road was Shortened?");
                        if (playerFound == MessageBoxResult.Yes) {
                            foundTarget = true;
                        } else {
                            index++;
                        }
                    } else {
                        index++;
                    }
                    index = index == players.Length ? 0 : index;
                }//END WHILE
                selectedPlayer = players[index];
                lblRobber.Content = $"What is {selectedPlayer.Name.ToUpper()}'s longest Road now?";
            }//END IF (CHECK FOR A ROAD-CUT)
            pipIndex = 0;
            DisableOptions();
        }//END METHOD

        private void btnCity_Click(object sender,RoutedEventArgs e) {
            lstBxSelection.Items.Clear();
            buyingCity = true;
            lblRobber.Visibility = Visibility.Visible;
            lblRobber.Content = $"Which Settlement are you upgrading?";
            viewLstBxSelection.Visibility = Visibility.Visible;
            txtPipSelect.Visibility = Visibility.Collapsed;
            //PopulateListBox with settlement options.
            lstBxSelection.Visibility = Visibility.Visible;
            
            
            string[] settlementOptions = players[turnIndex].GetSettlementsAsString();
            PopulateListBox(settlementOptions);
            //PopulateListBox(players[turnIndex].GetSettlementsAsString());
            DisableOptions();
        }
        private void btnOK_Click(object sender,RoutedEventArgs e) {
            if (interruptingRoad) {
                LoseRoad(selectedPlayer);
            //} else if (buyingSettlement || buyingCity) {
            } else if (buyingSettlement) {
                BuySettlementOrCity(buyingSettlement, buyPhase);
            } else if  (buyingCity) {
                if (lstBxSelection != null) {
                    BuyCity(lstBxSelection.SelectedIndex);
                }
            } else if (buyingRoad) {
                BuyRoad();
            } else {
                robberStage++;
                MoveRobber(robberStage);
            }
            txtPipSelect.Text = "";
        }//END METHOD

        private void BuySettlementOrCity(bool settlement, int phase) {            
            if (phase == 0) {
                pipViewArray = ParseStringToIntArray(txtPipSelect.Text);
                tempArray = new Location[3];
                
                if (pipViewArray.Length < 1) {return;}

                for (int i = 0; i < pipViewArray.Length; i++) {
                    if ((pipViewArray[i] < 2 || pipViewArray[i] > 12 || pipViewArray[i] == 7) && pipViewArray[i] != 0) {                        
                        return;
                    }
                }
                
                buyPhase = 1;
                phase = 1;
            }
            if (phase == 1) {
                string text = "";
                if (lstBxSelection.SelectedItem != null) {

                    Location[] options = mainBoard.GetLocationsByToken(pipViewArray[pipIndex]);
                    tempArray[pipIndex++] = options[lstBxSelection.SelectedIndex];
                    /*
                    text = lstBxSelection.SelectedItem.ToString();

                    tempArray[pipIndex] = mainBoard.GetLocation(pipViewArray[pipIndex++], text);
                    */
                    lstBxSelection.Items.Clear();
                }

                if (pipIndex == pipViewArray.Length) {
                    buyPhase = 2;
                    phase = 2;
                }

                if (phase == 1) {       
                    /*
                    lblRobber.Content = $"Which resource is associated with token {pipViewArray[pipIndex]}?";
                    lstBxSelection.Items.Clear();
                    string[] resources = mainBoard.GetResourcesByToken(pipViewArray[pipIndex]);
                    PopulateListBox(resources);
                    lstBxSelection.Visibility = Visibility.Visible;
                    txtPipSelect.Visibility = Visibility.Collapsed; */

                    //change text to "what select a valid location" +++ Create a holding LOCATION[] and use it to show "ToString"
                    lblRobber.Content = $"Select a valid location for token-{pipViewArray[pipIndex]}";
                    lstBxSelection.Items.Clear();
                    Location[] options = mainBoard.GetLocationsByToken(pipViewArray[pipIndex]);
                    string[] optStrings = new string[options.Length];
                    for (int i = 0; i < options.Length; i++) {
                        optStrings[i] = options[i].LocationToString();
                    }
                    PopulateListBox(optStrings);
                    lstBxSelection.Visibility = Visibility.Visible;
                    txtPipSelect.Visibility = Visibility.Collapsed;
                }

            } 
            if (phase == 2) {
                if (settlement) {

                    Settlement newSettlement = new Settlement(tempArray[0], tempArray[1], tempArray[2]);
                    players[turnIndex].AddSettlement(newSettlement);

                    lblRobber.Content = $"{players[turnIndex].Name} bought a Settlement";
                    buyingSettlement = false;
                } else {
                    lblRobber.Content = $"{players[turnIndex].Name} bought a City";
                    buyingCity = false;
                }
                lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";
                viewLstBxSelection.Visibility = Visibility.Collapsed;
                txtPipSelect.MaxLength = 2;
                buyPhase = 0;
                pipIndex = 0;
                pipViewArray = null;
                tempArray = null;
                EnableOptions();
            }
        }//END METHOD

        public void BuyCity(int buyIndex) {
            if (buyIndex == -1) {return; }
            Settlement chosen = players[turnIndex].GetSettlements()[buyIndex];
            players[turnIndex].ConvertSettlementToCity(chosen);
            viewLstBxSelection.Visibility = Visibility.Collapsed;
            lstBxSelection.Items.Clear();
            lblRobber.Content = $"{players[turnIndex].Name} bought a City";
            buyingCity = false;
            lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";            
            EnableOptions();
        }

        private void BuyRoad() {
            string response = txtPipSelect.Text;
            string validText = "";
            for (int i = 0; i < response.Length; i++) {
                if (response[i] >= 48 && response[i] <= 57) {
                    validText += response[i];
                }
            }
            if (validText == "") {return;}

            int roadLength = int.Parse(validText);
            players[turnIndex].LongestRoad = roadLength;
            int roadCheck = 4;
            int longestNow = mainBoard.LongestRoad == null ? 0 : mainBoard.LongestRoad.LongestRoad;
            for (int i = 0; i < players.Length; i++) {
                players[i].HasLongestRoad = false;
                if (players[i].LongestRoad > roadCheck) {
                    if (players[i].LongestRoad > longestNow) {
                        mainBoard.LongestRoad = players[i];
                        players[i].HasLongestRoad = true;
                    }
                }
            }
            if (mainBoard.LongestRoad != null) {
                mainBoard.LongestRoad.HasLongestRoad = true;
            }
            lblRobber.Content = $"{players[turnIndex].Name} extended their Trade Route.";
            lblVicPoints.Content = $"Victory Points: {players[turnIndex].VictoryPoints}";
            
            string longestOrLargest = "";
            if (players[turnIndex].HasLongestRoad) {longestOrLargest += "LongRoad ";}
            if (players[turnIndex].HasLargestArmy) {longestOrLargest += "LargeArmy";}
            lblBonus.Content = longestOrLargest;

            buyingRoad = false;
            viewLstBxSelection.Visibility = Visibility.Collapsed;
            EnableOptions();
        }//END METHOD

        private void LoseRoad(Player victim) {

            string response = txtPipSelect.Text;
            string validText = "";
            for (int i = 0; i < response.Length; i++) {
                if (response[i] >= 48 && response[i] <= 57) {
                    validText += response[i];
                }
            }

            if (validText == "") {return;}

            int roadLength = int.Parse(validText);
            victim.LongestRoad = roadLength;
            int roadCheck = 4;
            int longestNow = mainBoard.LongestRoad == null ? 0 : mainBoard.LongestRoad.LongestRoad;
            for (int i = 0; i < players.Length; i++) {
                players[i].HasLongestRoad = false;
                if (players[i].LongestRoad > roadCheck) {
                    if (players[i].LongestRoad > longestNow) {
                        mainBoard.LongestRoad = players[i];
                        players[i].HasLongestRoad = true;
                    }
                }
            }
            if (mainBoard.LongestRoad != null) {
                mainBoard.LongestRoad.HasLongestRoad = true;
            }
            buyingRoad = false;
            interruptingRoad = false;
            selectedPlayer = null;
            lblRobber.Content = $"What are the number tokens adjacent to your settlement? (Separate with SPACE)";

            string longestOrLargest = "";
            if (players[turnIndex].HasLongestRoad) {longestOrLargest += "LongRoad ";}
            if (players[turnIndex].HasLargestArmy) {longestOrLargest += "LargeArmy";}
            lblBonus.Content = longestOrLargest;

            buyPhase = 0;
            
        }//END METHOD

        private MessageBoxResult PauseAndAsk(string question, string windowTitle) {
            MessageBoxButton confirmation = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show(question, windowTitle, confirmation);
            return result;
        }//END METHOD

        private string UpdatePlayerResources(int token) {
            string output = "";
            Location[] rolledSpot = mainBoard.GetLocationsByToken(token);
            for (int pl = 0; pl < players.Length; pl++) {
                string holder = $"{players[pl].Name}-";
                for (int loc = 0; loc < rolledSpot.Length; loc++) {
                    if (players[pl].IsOnLocation(rolledSpot[loc])) {
                        holder += $"{players[pl].UpdateResourceCount(rolledSpot[loc], mainBoard)}";
                    }
                }
                if (holder == $"{players[pl].Name}-") {
                    holder = "";
                }
                output += holder + "  ";
            }
            return output;
        }//END METHOD

        public void InstantiateDispatchTimer() {
            _displayTimer = new DispatcherTimer(new TimeSpan(0,0, 0, 0, 100), DispatcherPriority.Background, time_Tick, Dispatcher.CurrentDispatcher);
            _displayTimer.IsEnabled = true;
        }
        private void time_Tick(object sender, EventArgs e) {
            lblTimer.Content = mainBoard.GetGameTime().ToString(@"hh\:mm\:ss");
        }

        private void btnTimeControl_Click(object sender,RoutedEventArgs e) {
            if (mainBoard.GameClock.IsRunning) {
                mainBoard.StopGameTimer();
                btnTimeControl.Content = "RESUME";
                DisableOptions();
                DisableRollButtons();
            } else {
                mainBoard.StartGameTimer();
                btnTimeControl.Content = "PAUSE";
                EnableOptions();
                if (!hasRolled) {
                    EnableRollButtons();
                }
            }
        }

        private void DisableOptions() {
            btnSettlement.IsEnabled = false;
            btnCity.IsEnabled = false;
            btnKnight.IsEnabled = false;
            btnVicPoint.IsEnabled = false;
            btnDevCard.IsEnabled = false;
            btnRoad.IsEnabled = false;
            btnGameOver.IsEnabled = false;
            btnTurn.IsEnabled = false;
            btnChit.IsEnabled = false;
            btnShip.IsEnabled = false;
            btnMove.IsEnabled = false;
        }//END METHOD

        private void EnableOptions() {
            if (knightActive || buyingCity || buyingRoad || buyingSettlement || robberActive) {return; }

            btnSettlement.IsEnabled = true;
            btnCity.IsEnabled = true;
            btnVicPoint.IsEnabled = true;
            btnDevCard.IsEnabled = true;
            btnRoad.IsEnabled = true;
            btnGameOver.IsEnabled = true;
            btnChit.IsEnabled = true;
            btnShip.IsEnabled = true;
            btnMove.IsEnabled = true;

            if (!knightExpended) {
                btnKnight.IsEnabled = true;
            }
            if (hasRolled) {
                btnTurn.IsEnabled = true;
            }
        }//END METHOD

        private void btnExport_Click(object sender,RoutedEventArgs e) {
            StreamWriter outfile;
            SaveFileDialog saveFile = new SaveFileDialog();

            saveFile.Filter = "csv files (*.csv)|*.csv";
            saveFile.FilterIndex = 2 ;
            saveFile.RestoreDirectory = true ;

            if (saveFile.ShowDialog() == true) {
                string path = saveFile.FileName;
                outfile = new StreamWriter(path);
                string allData = GetAllGameData();
                for (int i = 0; i < allData.Length; i++) {
                    outfile.Write(allData[i]);
                }
                outfile.Close();
            }
        }//END METHOD

        private string GetAllGameData() {
            string data = "";
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            TimeOnly startTime = TimeOnly.FromDateTime(DateTime.Now - mainBoard.GetGameTime());
            string gameName = isSeafarers ? "SEAFARERS" : "STANDARD CATAN";
            data += $"GAME STATS, {gameName}, {today}, {startTime}\nTOTAL GAME TIME, TOTAL ROUNDS, WINNER\n";
            data += $"{mainBoard.GetGameTime().ToString(@"hh\:mm\:ss")}, {mainBoard.Round}, {mainBoard.Winner.Name}\n\n";
            data += "ROLL DATA\n";
            data += $"2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12\n";
            for (int i = 0; i < mainBoard.RollCounter.Length; i++) {
                data += $"{mainBoard.RollCounter[i]}, ";
            }
            data += "\b\b\n\n";
            data += "PLAYER DATA\n";            

            //INPUT SEAFARER CHECK AND STRING ADDENDUM HERE.. SHIPCOUNT SHIPSMOVED CHITS
            string seafarerTitles = "";
            if (isSeafarers) {
                seafarerTitles = "Ships Bought, Ships Moved, Chits Earned,";
            }
            data += $"Name, Turn Order, Longest Road, Largest Army, Times Robbed, Whacked Count, Wood Earned, Bricks Earned, Ore Earned, Sheep Earned, Wheat Earned, Gold Earned, Total Resources Earned, Times Blocked, Victory Points,{seafarerTitles} Development Cards Bought, Longest Turn, Shortest Turn, Average Turn, Robber Activity, Knight Activity\n";
            for (int i = 0; i < players.Length; i++) {
                data += players[i].GetAllPlayerDataToString(isSeafarers);
            }

            //go NAME, Opening Settlement Full Data, Opening PipCount, Opening Resources
            data += "\n\nOPENING DATA, Opening Settlement 1, Opening Settlement 2, Total 'Pips', Opening Resources\n";

            for (int i = 0; i < players.Length; i++) {
                Settlement[] first = players[i].OpeningSettlements;
                string resourcePartA = first[0].GetUniqueResourcesToString();
                string resourcePartB = first[1].GetUniqueResourcesToString();
                resourcePartA += resourcePartB;
                string[] resourceList = resourcePartA.Split(' ');
                string resources = "";
                foreach (string resource in resourceList) {
                    if (!resources.Contains(resource)) {
                        resources += resource + " + ";
                    }
                }//END FOR LOOP
                char[] resourcesBrokenUp = resources.ToCharArray();
                resources = "";
                for (int chrctr = 0; chrctr < resourcesBrokenUp.Length - 3; chrctr++) {
                    resources += resourcesBrokenUp[chrctr];
                }//END LOOP

                data += $"{players[i].Name},{first[0].GetLocationsToStringSeparatedByPlus()}, {first[1].GetLocationsToStringSeparatedByPlus()}, {first[0].GetPipValue() + first[1].GetPipValue()}, {resources}\n";
            }

            Round[] rnd = mainBoard.Rounds;
            data += "\n\nROUNDS,";
            for (int i = 0; i < mainBoard.RoundsCount; i++) {
                if (i < mainBoard.RoundsCount - 1) {
                    data += $"Round {i + 1},";
                } else {
                    data += $"Final Round\n";
                }//END IF
            }//END FOR

            data += "Round Duration:,";
            for (int i = 0; i < rnd.Length; i++) {
                data += $"{rnd[i].RoundTime.ToString(@"mm\:ss")},";
            }//END FOR

            data += "\nElapsed Game Time:,";
            for (int i = 0; i < rnd.Length; i++) {
                data += $"{rnd[i].GameTime.ToString(@"mm\:ss")},";
            }//END FOR

            data += "\nLongest Road(s),";
            for (int i = 0; i < rnd.Length; i++) {
                if (rnd[i].LongestRoad.Length > 1) {
                    for (int p = 0; p < rnd[i].LongestRoad.Length; p++) {
                        data += p == rnd[i].LongestRoad.Length - 1 ? $"{rnd[i].LongestRoad[p].Name} - {rnd[i].TopRoad}," : $"{rnd[i].LongestRoad[p].Name} + ";
                    }
                } else {
                    data += $"{rnd[i].LongestRoad[0].Name} - {rnd[i].TopRoad},";
                }
            }//END FOR

            data += "\nLargest Army(s),";
            for (int i = 0; i < rnd.Length; i++) {
                if (rnd[i].LargestArmy.Length > 1) {
                    for (int p = 0; p < rnd[i].LargestArmy.Length; p++) {
                        data += p == rnd[i].LargestArmy.Length - 1 ? $"{rnd[i].LargestArmy[p].Name} - {rnd[i].TopKnight}," : $"{rnd[i].LargestArmy[p].Name} + ";
                    }
                } else {
                    data += $"{rnd[i].LargestArmy[0].Name} - {rnd[i].TopKnight},";
                }
            }//END FOR

            data += "\nMost Victory Points,";
            for (int i = 0; i < rnd.Length; i++) {
                if (rnd[i].PointLeader.Length > 1) {
                    for (int p = 0; p < rnd[i].PointLeader.Length; p++) {
                        data += p == rnd[i].PointLeader.Length - 1 ? $"{rnd[i].PointLeader[p].Name} - {rnd[i].TopVicPoint}," : $"{rnd[i].PointLeader[p].Name} + ";
                    }
                } else {
                    data += $"{rnd[i].PointLeader[0].Name} - {rnd[i].TopVicPoint},";
                }
            }//END FOR


            return data;
        }//END METHOD

        private void Expander_Expanded(object sender,RoutedEventArgs e) {
            //expPlayers.Height = mainWindow.Height - 30;
            expPlayers.Height = double.NaN;
            grdGameStats.Height = expPlayers.Height;

            if (grdGameStats.RowDefinitions.Count < 1) {
                RowDefinition[] playerStats = new RowDefinition[players.Length * 2];
                for (int i = 0; i < playerStats.Length; i++) {
                    playerStats[i] = new RowDefinition();
                    if (i % 2 == 0) {playerStats[i].Height = new GridLength(25);}
                    grdGameStats.RowDefinitions.Add(playerStats[i]);
                }
            }
            
            TextBlock[] statInfo = new TextBlock[players.Length * 2];            

            for (int i = 0; i < statInfo.Length; i++) {
                statInfo[i] = new TextBlock();
                if (i % 2 == 0) {
                    statInfo[i].HorizontalAlignment = HorizontalAlignment.Center;
                    statInfo[i].VerticalAlignment = VerticalAlignment.Bottom;
                    statInfo[i].Height = 20;
                    statInfo[i].FontWeight = FontWeights.Bold;
                    statInfo[i].FontSize = 15;
                } else {
                    statInfo[i].HorizontalAlignment = HorizontalAlignment.Left;
                    statInfo[i].VerticalAlignment = VerticalAlignment.Top;
                }
            }
            for (int i = 0; i < players.Length; i++) {
                statInfo[i * 2].Text = players[i].Name;
                statInfo[(i * 2) + 1].Text = players[i].GetGameStats();
            }
            for (int i = 0; i < statInfo.Length; i++) {
                Grid.SetRow(statInfo[i], i);
                Grid.SetColumn(statInfo[i], 1);
                grdGameStats.Children.Add(statInfo[i]);
            }
        }//END METHOD

        private void expPlayers_Collapsed(object sender,RoutedEventArgs e) {
            expPlayers.Height = Double.NaN;
            grdGameStats.Children.Clear();
        }//END METHOD

        private void btnGameSelect_Click(object sender,RoutedEventArgs e) {
            //SELECT WHICH VERSION OF CATAN IS BEING PLAYED
            Button selection = sender as Button;
            if (selection.Name == "btnCatan") {
                //STANDARD CATAN IS CHOSEN
                isCatan = true;
            } else if (selection.Name == "btnSeafarers") {
                //SEAFARERS IS CHOSEN
                isSeafarers = true;
            } else {
                return;
            }
            viewGameSelect.Visibility = Visibility.Collapsed;
            viewBoardSetup.Visibility = Visibility.Visible;

            if (isCatan) {
                imgGold.Visibility = Visibility.Collapsed;
                setupGold.Visibility = Visibility.Collapsed;
                btnGold.Visibility = Visibility.Collapsed;
                btnShip.Visibility = Visibility.Collapsed;
                btnMove.Visibility = Visibility.Collapsed;
                btnChit.Visibility = Visibility.Collapsed;
                btnSettlement.Margin = new Thickness(402, 122, 0, 0);
                btnDevCard.Margin = new Thickness(402, 172, 0, 0);
                btnRoad.Margin = new Thickness(532, 122, 0, 0);
                btnCity.Margin = new Thickness(532, 172, 0, 0);
                btnKnight.Margin = new Thickness(402, 238,0,0);
                btnVicPoint.Margin = new Thickness(532, 238, 0, 0);
            }
        }//END METHOD

    }//END WINDOW/PROGRAM CLASS
}
