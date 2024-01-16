using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CatanTracker {
    internal class Player {
        private string _name;
        private bool _hasLargestArmy = false;
        private bool _hasLongestRoad = false;
        private bool _isWinner = false;
        private int _turnInGame;
        private int _longestRoadLength = 1;
        private int _roadCount = 2;
        private int _shipCount = 0;
        private int _shipsMoved = 0;
        private int _stolenFromCount = 0;
        private int _knightCount = 0;
        private int _whackedCount = 0;
        private int _totalWood = 0;
        private int _totalBrick = 0;
        private int _totalGold = 0;
        private int _totalOre = 0;
        private int _totalSheep = 0;
        private int _totalWheat = 0;
        private int _timesBlocked = 0;
        private int _victoryPoints = 0;
        private int _chitPoints = 0;
        private int _devCardsBought = 0;
        private string _robberActivity = ""; //"Miles stole from Jackie\n" (if a card wasn't stolen, have a void and say "Miles blocked Jackie with robber")
        private string _knightActivity = ""; //"Miles knighted Jackie\n" (if a card wasn't stolen, have a void and say "Miles blocked Jackie with knight")
        private TimeSpan[] _turnTimes;   
        private Settlement[] _settlementObjects;
        private Settlement[] _openingSettlements;
        private City[] _cityObjects;

#region CONSTRUCTORS

        public Player(string name, int turn) {
            _name = name;
            _turnInGame = turn;
        }
#endregion

#region NEW BUILD METHODS
//ADD A DURATION OF TIME TO THE _TURNTIMES ARRAY TO SHOW HOW LONG A TURN TOOK
        
        public void AddTurnTime(TimeSpan turnTime) {
            //CHECK IF THE _TURNTIMES ARRAY IS CURRENTLY EMPTY
            if (_turnTimes != null) {
                TimeSpan[] updatedTurnTimes = new TimeSpan[_turnTimes.Length + 1];
                int index = 0;

                //ITERATE TO END OF TURNTIME ARRAY AND APPEND THE NEWEST TIME TO THE END OF ARRAY
                while (index < _turnTimes.Length) {
                    updatedTurnTimes[index] = _turnTimes[index++];
                }//END LOOP

                //SAVE NEW ARRAY AND OVERWRITE PREVIOUS _TURNTIMES ARRAY
                updatedTurnTimes[index] = turnTime;
                _turnTimes = updatedTurnTimes;

            //IF _TURNTIMES ARRAY IS EMPTY, CREATE ARRAY AND ADD TIME
            } else {
                _turnTimes = new TimeSpan[1] {turnTime };
            }
        }//END METHOD


        public void AddSettlement(Settlement newSettlement) {
            if (_settlementObjects != null) {
                Settlement[] updatedSettlements = new Settlement[_settlementObjects.Length + 1];
                int index = 0;
                while (index < _settlementObjects.Length) {
                    updatedSettlements[index] = _settlementObjects[index++];
                }//END LOOP
                updatedSettlements[index] = newSettlement;
                _settlementObjects = updatedSettlements;
            } else {
                _settlementObjects = new Settlement[1] {newSettlement };
            }
        }//END METHOD

        public void ReduceSettlements() {
            int index = 0;
            Settlement[] newSettles;
            if (_settlementObjects == null) {return;}

            for (int i = 0; i < _settlementObjects.Length; i++) {
                if (_settlementObjects[i] != null) {
                    index++;
                }
            }
            newSettles = new Settlement[index];
            index = 0;
            for (int i = 0; i < _settlementObjects.Length; i++) {
                if (_settlementObjects[i] != null) {
                    newSettles[index++] = _settlementObjects[i];
                }
            }
            _settlementObjects = newSettles;
        }
        public void AddCity(City newCity) {
            if (_cityObjects != null) {
                City[] updatedCity = new City[_cityObjects.Length + 1];
                int index = 0;
                while (index < _cityObjects.Length) {
                    updatedCity[index] = _cityObjects[index++];
                }//END LOOP
                updatedCity[index] = newCity;
                _cityObjects = updatedCity;
            } else {
                _cityObjects = new City[1] {newCity};
            }
            ReduceSettlements();
        }//END METHOD

        public bool IsOnLocation(Location target) {
            if (_settlementObjects != null) {
                foreach (Settlement settlement in _settlementObjects) {
                    if (settlement != null) {
                        if (settlement.HasLocation(target)) return true;
                    }
                }
            }
            if (_cityObjects != null) {
                foreach (City city in _cityObjects) {
                    if (city.HasLocation(target)) return true;
                }
            }
            return false;
        }

        public string UpdateResourceCount(Location target, GameBoard game) {
            int initBrick = _totalBrick;
            int initGold = _totalGold;
            int initOre = _totalOre;
            int initSheep = _totalSheep;
            int initWheat = _totalWheat;
            int initWood = _totalWood;
            int initBlocked = _timesBlocked;
            string blocked = "";

            if (_settlementObjects != null) {
                for (int i = 0; i < _settlementObjects.Length; i++) {
                    if (_settlementObjects[i] != null) {
                        if (_settlementObjects[i].HasLocation(target) && target.Equals(game.Blocked)) {
                            _timesBlocked += 1;
                            blocked = game.Blocked.Resource;
                        } else if (_settlementObjects[i].HasLocation(target)) {
                            if (target.Resource == "BRICK") {  _totalBrick += 1;}
                            if (target.Resource == "GOLD") { _totalGold += 1;}
                            if (target.Resource == "ORE") { _totalOre += 1;}
                            if (target.Resource == "SHEEP") { _totalSheep += 1;}
                            if (target.Resource == "WHEAT") { _totalWheat += 1;}
                            if (target.Resource == "WOOD") { _totalWood += 1;}
                        }                
                    }
                }
            }
            if (_cityObjects != null) {
                for (int i = 0; i < _cityObjects.Length; i++) {
                    if (_cityObjects[i].HasLocation(target) && target.Equals(game.Blocked)) {
                        _timesBlocked += 2;
                        blocked = game.Blocked.Resource;
                    } else if (_cityObjects[i].HasLocation(target)) {
                        if (target.Resource == "BRICK") {  _totalBrick += 2;}
                        if (target.Resource == "GOLD") { _totalGold += 2;}
                        if (target.Resource == "ORE") { _totalOre += 2;}
                        if (target.Resource == "SHEEP") { _totalSheep += 2;}
                        if (target.Resource == "WHEAT") { _totalWheat += 2;}
                        if (target.Resource == "WOOD") { _totalWood += 2;}
                    }
                }
            }//END CITY IF
            string output = "";
            if (_totalBrick > initBrick) {output += $"{_totalBrick - initBrick} BRICK: ";}
            if (_totalGold > initGold) {output += $"{_totalGold - initGold} GOLD: ";}
            if (_totalOre > initOre) {output += $"{_totalOre - initOre} ORE: "; }
            if (_totalSheep > initSheep) {output += $"{_totalSheep - initSheep} SHEEP: "; }
            if (_totalWheat > initWheat) {output += $"{_totalWheat - initWheat} WHEAT: ";}
            if (_totalWood > initWood) {output += $"{_totalWood - initWood} WOOD: ";}
            if (_timesBlocked > initBlocked) {output += $"({_timesBlocked - initBlocked} {blocked} BLOCKED): ";}

            return output;
        }//END METHOD

        public Settlement[] GetSettlements() {
            if (_settlementObjects == null) {return null; }

            Settlement[] settlements = new Settlement[_settlementObjects.Length];
            for (int i = 0; i < settlements.Length; i++) {
                settlements[i] = _settlementObjects[i];
            }
            return settlements;
        }//END METHOD

        public string[] GetSettlementsAsString() {
            if (_settlementObjects == null) {return null; }

            string[] settlementData = new string[_settlementObjects.Length];
            Settlement[] spots = GetSettlements();
            for (int i = 0; i < settlementData.Length; i++) {
                settlementData[i] = spots[i].GetLocationsToString();
            }
            return settlementData;
        }//END METHOD

        public City[] GetCities() {
            if (_cityObjects == null) {return null;}

            City[] cities = new City[_cityObjects.Length];
            for (int i = 0; i < cities.Length; i++) {
                cities[i] = _cityObjects[i];
            }
            return cities;
        }//END METHOD

        public string[] GetCitiesAsString() {
            if (_cityObjects == null) {return null; }

            string[] cityNames = new string[_cityObjects.Length];
            City[] spots = GetCities();
            for (int i = 0; i < cityNames.Length; i++) {
                cityNames[i] = spots[i].GetLocationsToString();
            }
            return cityNames;
        }//END METHOD

        public void ConvertSettlementToCity(Settlement settlement) {
            if (_settlementObjects == null) {return;}

            if (_settlementObjects.Length > 1) {
                Settlement[] newArray = new Settlement[_settlementObjects.Length - 1];
                int index = 0;
                for (int i = 0; i < _settlementObjects.Length; i++) {
                    if (_settlementObjects[i] != settlement) {
                        newArray[index++] = _settlementObjects[i];
                    }
                }
                City newCity = new City(settlement);
                _settlementObjects = newArray;
                AddCity(newCity);
            } else {                
                City city = new City(settlement);
                _settlementObjects = null;
                AddCity(city);
                return;
            }
        }//END METHOD

        public void EndStats(Grid grid, int index) {
            TextBlock[] statInfo = new TextBlock[grid.ColumnDefinitions.Count];
            for (int i = 0; i < statInfo.Length; i++) {
                statInfo[i] = new TextBlock();
                statInfo[i].HorizontalAlignment = HorizontalAlignment.Center;
                statInfo[i].VerticalAlignment = VerticalAlignment.Center;
                statInfo[i].FontWeight = FontWeights.Bold;
            }
            //FIND LONGEST TURN
            TimeSpan longturn = GetLongestTurn();

            statInfo[0].Text = Name;
            statInfo[1].Text = _whackedCount.ToString();
            statInfo[2].Text = longturn.ToString(@"hh\:mm\:ss");
            statInfo[3].Text = _knightCount.ToString();
            statInfo[4].Text = _stolenFromCount.ToString();
            statInfo[5].Text = _timesBlocked.ToString();
            statInfo[6].Text = AllResources.ToString();
            
            for (int i = 0; i < statInfo.Length; i++) {
                Grid.SetRow(statInfo[i], index);
                Grid.SetColumn(statInfo[i], i);
                grid.Children.Add(statInfo[i]);
            }
        }//END METHOD

        public string GetGameStats(){     

            string longOrLarge = "";
            if (HasLongestRoad) {longOrLarge += "Longest Road\n"; }
            if (HasLargestArmy) {longOrLarge += "Largest Army\n"; }
            string[] settlements = GetSettlementsAsString();
            string settles = "";
            if (settlements != null) {
                foreach (string item in settlements) {
                    settles += item + "\n";
                }
            }

            string[] cityArray = GetCitiesAsString();
            string cities = "";
            if (cityArray != null) {
                foreach (string item in cityArray) {
                    cities += item + "\n";
                }
            }
            string settlementIntro = "Settlements:\n";
            string citiesIntro = "Cities:\n";
            if (settles == "") {settlementIntro = "";}
            if (cities == "") {citiesIntro = "";}

            string final = $"Victory Points: {VictoryPoints}\n{longOrLarge}{settlementIntro}{settles}{citiesIntro}{cities}";
            
            return final ;

        }//END METHOD

        public TimeSpan GetLongestTurn() {
            TimeSpan longestTime = new TimeSpan();
            for (int i = 0; i < _turnTimes.Length; i++) {
                if (_turnTimes[i] > longestTime) {
                    longestTime = _turnTimes[i];
                }
            }
            return longestTime;
        }//END METHOD

        public TimeSpan GetShortestTurn() {
            TimeSpan shortestTime = new TimeSpan(9, 59, 59);
            for (int i = 0; i < _turnTimes.Length; i++) {
                if (_turnTimes[i] < shortestTime) {
                    shortestTime = _turnTimes[i];
                }
            }
            return shortestTime;
        }//END METHOD

        public TimeSpan GetAverageTurn() {
            TimeSpan averageTime = new TimeSpan();
            for (int i = 0; i < _turnTimes.Length; i++) {
                averageTime += _turnTimes[i];
            }
            averageTime /= _turnTimes.Length;
            return averageTime;
        }//END METHOD

        public string GetAllPlayerDataToString(bool gameIsSeafarers) {
            string playerData = "";
            string seafarers = "";
            if (gameIsSeafarers) {
                seafarers = $"{_shipCount},{_shipsMoved},{_chitPoints},";
            }
            playerData += $"{Name}, {_turnInGame}, {_longestRoadLength}, {_knightCount}, {_stolenFromCount}, {_whackedCount}, {_totalWood}, {_totalBrick}, {_totalOre}, {_totalSheep}, {_totalWheat}, {_totalGold}, {AllResources}, {_timesBlocked}, {VictoryPoints},{seafarers} {_devCardsBought}, {GetLongestTurn().ToString(@"hh\:mm\:ss")}, {GetShortestTurn().ToString(@"hh\:mm\:ss")}, {GetAverageTurn().ToString(@"hh\:mm\:ss")}, {_robberActivity}, {_knightActivity}\n";
            return playerData;
        }

#endregion

#region NEW BUILD PROPERTIES

        public bool IsWinner {
            get {return _isWinner; } set { _isWinner = value; }
        }

        public Settlement[] OpeningSettlements {
            get {return _openingSettlements; } set { _openingSettlements = value; }
        }

        public int AllResources {
            get {
                int fullCount = _totalWood;
                fullCount += _totalBrick;
                fullCount += _totalGold;
                fullCount += _totalOre;
                fullCount += _totalSheep;
                fullCount += _totalWheat;
                return fullCount;
            }                   
        }//END PROPERTY

#endregion
        
                                 
#region PROPERTIES                                 
        public string Name {
            get {return _name;}
        }

        public int Knight {
            get {return _knightCount;} set { _knightCount = value; }
        }

        public int LongestRoad {
            get {return _longestRoadLength;} set { _longestRoadLength = value; }
        }

        public bool HasLargestArmy {
            get {return _hasLargestArmy;} set { _hasLargestArmy = value; }
        }

        public bool HasLongestRoad {
            get {return _hasLongestRoad;} set { _hasLongestRoad = value; }
        }

        public string KnightActivity {
            get {return _knightActivity; } 
        }

        public string RobberActivity {
            get {return _robberActivity; }
        }

        public int WhackedCount {
            get {return _whackedCount; } set { _whackedCount = value; }
        }

        public int DevCardsBought {
            get {return _devCardsBought;} set { _devCardsBought = value; }
        }

        public int ChitPoints {
            get {return _chitPoints;} set { _chitPoints = value;}
        }

        public int RoadCount {
            get {return _roadCount;} set { _roadCount = value; }
        }
        
        public int ShipCount {
            get {return _shipCount;} set { _shipCount = value; }
        }

        public int MovedShips {
            get {return _shipsMoved; } set { _shipsMoved = value; }
        }

        public int VictoryPoints {
            get {
                int addons = 0;
                int settlements = 0;
                int cities = 0;
                settlements = _settlementObjects == null ? 0 : _settlementObjects.Length;
                cities = _cityObjects == null ? 0 : _cityObjects.Length;
                addons += HasLongestRoad ? 2 : 0;
                addons += HasLargestArmy ? 2 : 0;
                return _victoryPoints + settlements + (cities * 2) + addons;}
            set {_victoryPoints = value; }
        }


#endregion        

#region  METHODS

        public void SetKnightActivity(string opponent, bool isRobber, bool hadCards) {
            string updated = "";
            if (opponent == "No One") {
                return;
            }
            updated = Name;
            if (isRobber) {
                if (hadCards) {
                    updated += " knighted ";
                } else {
                    updated += " blocked ";
                }
            } else {
                _stolenFromCount += 1;
                if (hadCards) {
                    updated += " was knighted by ";
                } else {
                    updated += " was blocked by ";
                }
            }
            updated += $"{opponent} :: ";
            _knightActivity += updated;
        }//END METHOD

        public void SetRobberActivity(string opponent, bool isRobber, bool hadCards) {
            string updated = "";
            if (opponent == "No One") {
                return;
            }
            updated = Name;
            if (isRobber) {
                if (hadCards) {
                    updated += " robbed ";
                } else {
                    updated += " blocked ";
                }
            } else {
                _stolenFromCount += 1;
                if (hadCards) {
                    updated += " was robbed by ";
                } else {
                    updated += " was blocked by ";
                }
            }
            updated += $"{opponent} :: ";
            _robberActivity += updated;
        }//END METHOD

        public void AddVictoryPoint() {
            _victoryPoints++;
        }//END METHOD

#endregion
    }//END CLASS
}
