using CatanTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatanTrackV2 {
    internal class Round {
        //LOG EACH PLAYERS VIC POINTS PER ROUND, WHO IS LONGEST/LARGEST, TOTAL RESOURCES EARNED

        /* CONSIDER STORING THE ENTIRE PLAYER AT THIS POINT IF FUTURE ADDITIONAL DATA IS DESIRED */
        /////////////////////
        private TimeSpan _currentGameTime;
        private TimeSpan _roundTime;
        private Player _longestPlayer;
        private Player[] _tiedLongest;
        private Player _largestPlayer;
        private Player[] _tiedLargest;
        private Player _vicPointLeader;
        private Player[] _tiedPointLead;
        private int _longRoadCount;
        private int _largeArmyCount;
        private int _vicPountCount;

        public Round(Player[] players, TimeSpan gameTime, TimeSpan roundTime) {
            _currentGameTime = gameTime;
            _roundTime = roundTime;

            //CODE HERE TO STRAIN OUT SPECIFIC/LEADERSHIP DATA
            SetLongest(players);
            SetLargest(players);
            SetLeader(players);

        }//END CONSTRUCTOR

        #region PROPERTIES [GAMETIME, ROUNDTIME, LONGESTROAD, LARGESTARMY, POINTLEADER]
        public TimeSpan GameTime {
            get { return _currentGameTime; }
        }

        //TO UPDATE THE ROUND TIME, IN GAMEBOARD CHECK IF THE 'ROUND' ARRAY IS EMPTY, IF YES - ROUNDTIME = GAMETIME; IF NO, ROUNDTIME = (GAMETIME) - LAST ROUND IN ARRAY.GAMETIME
        public TimeSpan RoundTime {
            get { return _roundTime; } 
        }

        public Player[] LongestRoad {
            get { 
                return _longestPlayer != null ? new Player[]{_longestPlayer } : _tiedLongest;
            }
        }

        public Player[] LargestArmy {
            get { 
                return _largestPlayer != null ? new Player[]{_largestPlayer } : _tiedLargest;
            }
        }

        public Player[] PointLeader {
            get { 
                return _vicPointLeader != null ? new Player[]{_vicPointLeader } : _tiedPointLead;
            }
        }

        public int TopRoad {
            get {return _longRoadCount;}
        }

        public int TopKnight {
            get {return _largeArmyCount;}
        }

        public int TopVicPoint {
            get {return _vicPountCount;}
        }

        #endregion

        private void SetLongest(Player[] _players) {
            int longestFound = 0;
            Player longestPlayer = CheckLongest(_players, out longestFound);
            _longRoadCount = longestFound;

            if (longestPlayer != null) {
                _longestPlayer = longestPlayer;
            } else {
                int index = 0;
                for (int i = 0; i < _players.Length; i++) {
                    if (_players[i].LongestRoad == longestFound) {
                        index++;
                    }
                }//END LOOP
                
                Player[] longBoys = new Player[index];
                index = 0;
                for (int i = 0; i < _players.Length;i++) {
                    if (_players[i].LongestRoad == longestFound) {
                        longBoys[index++] = _players[i];
                    }
                }//END LOOP
                _tiedLongest = longBoys;
            }//END IF
        }//END METHOD
        private Player CheckLongest(Player[] _players, out int longest) {
            longest = 0;
            foreach (Player player in _players) {
                if (player.HasLongestRoad) {
                    return player;
                }
            }//END LOOP
            bool isTied = false;
            
            Player p = null;
            for (int i = 0; i < _players.Length; i++) {
                if (_players[i].LongestRoad > longest) {
                    isTied = false;
                    longest = _players[i].LongestRoad;
                    p = _players[i];
                } else if (_players[i].LongestRoad == longest) {
                    isTied = true;
                }
            }//END LOOP
            return isTied ? null : p;
        }//END METHOD

        private void SetLargest(Player[] _players) {
            int largestFound;
            Player largestPlayer = CheckLargest(_players, out largestFound);
            _largeArmyCount = largestFound;

            if (largestPlayer != null) {
                _largestPlayer = largestPlayer;
            } else {
                int index = 0;
                for (int i = 0; i < _players.Length; i++) {
                    if (_players[i].Knight == largestFound) {
                        index++;
                    }
                }//END LOOP
                
                Player[] largeBoys = new Player[index];
                index = 0;
                for (int i = 0; i < _players.Length;i++) {
                    if (_players[i].Knight == largestFound) {
                        largeBoys[index++] = _players[i];
                    }
                }//END LOOP
                _tiedLargest = largeBoys;
            }//END IF
        }//END METHOD
        private Player CheckLargest(Player[] _players, out int largest) {
            largest = 0;
            foreach (Player player in _players) {
                if (player.HasLargestArmy) {
                    return player;
                }
            }//END LOOP
            bool isTied = false;
            
            Player p = null;
            for (int i = 0; i < _players.Length; i++) {
                if (_players[i].Knight > largest) {
                    isTied = false;
                    largest = _players[i].Knight;
                    p = _players[i];
                } else if (_players[i].Knight == largest) {
                    isTied = true;
                }
            }//END LOOP
            return isTied ? null : p;
        }//END METHOD

        private void SetLeader(Player[] _players) {
            int topVP;
            Player leader = CheckLeader(_players, out topVP);
            _vicPountCount = topVP;

            if (leader != null) {
                _vicPointLeader = leader;
            } else {
                int index = 0;
                for (int i = 0; i < _players.Length; i++) {
                    if (_players[i].VictoryPoints == topVP) {
                        index++;
                    }
                }//END LOOP
                Player[] leadBoys = new Player[index];
                index = 0;
                for (int i = 0; i < _players.Length;i++) {
                    if (_players[i].VictoryPoints == topVP) {
                        leadBoys[index++] = _players[i];
                    }
                }//END LOOP
                _tiedPointLead = leadBoys;
            }//END IF
        }//END METHOD
        private Player CheckLeader(Player[] _players, out int highest) {
            highest = 0;
            
            bool isTied = false;
            
            Player p = null;
            for (int i = 0; i < _players.Length; i++) {
                if (_players[i].VictoryPoints > highest) {
                    isTied = false;
                    highest = _players[i].VictoryPoints;
                    p = _players[i];
                } else if (_players[i].VictoryPoints == highest) {
                    isTied = true;
                }
            }//END LOOP
            return isTied ? null : p;
        }//END METHOD

    }//END CLASS
}
