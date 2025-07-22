using System.Diagnostics;

/*
  Author: Atharv Kulkarni and Joshua Schelin
  Class for the ChessGame object
*/

namespace ChessBrowser
{
    internal class ChessGame
    {
        public string round;
        public string result;
        public string moves;
        public string blackName;
        public string blackElo;
        public string whiteName;
        public string whiteElo;
        public string eventName;
        public string eventSite;
        public string eventDate;

        /// <summary>
        /// Constructor for a new ChessGame object.
        /// </summary>
        /// <param name="round"></param>
        /// <param name="result"></param>
        /// <param name="moves"></param>
        /// <param name="blackName"></param>
        /// <param name="blackElo"></param>
        /// <param name="whiteName"></param>
        /// <param name="whiteElo"></param>
        /// <param name="eventName"></param>
        /// <param name="eventSite"></param>
        /// <param name="eventDate"></param>
        ChessGame(string round, string result, string moves, string blackName, string blackElo, string whiteName, string whiteElo, string eventName, string eventSite, string eventDate)
        {
            this.round = round;
            this.result = result;
            this.moves = moves;
            this.blackName = blackName;
            this.blackElo = blackElo;
            this.whiteName = whiteName;
            this.whiteElo = whiteElo;
            this.eventName = eventName;
            this.eventSite = eventSite;
            this.eventDate = eventDate;
        }

        /// <summary>
        /// Method that creates a list of ChessGame objects by parsing a PGN file.
        /// </summary>
        /// <param name="PGNfilename">The path for the PGN file</param>
        /// <returns>The list of ChessGame objects</returns>
        static public List<ChessGame> GamesFromPGNFile(string PGNfilename)
        {
            List<ChessGame> games = new List<ChessGame>();

            String[] pgnLines = File.ReadAllLines(PGNfilename);

            string state = "tags";

            // initialize variables to keep track of data
            string round = "";
            string result = "";
            string moves = "";
            string blackName = "";
            string blackElo = "";
            string whiteName = "";
            string whiteElo = "";
            string eventName = "";
            string eventSite = "";
            string eventDate = "";

            // loop through each line in the PGN file
            foreach (string pgnLine in pgnLines)
            {
                if (String.IsNullOrEmpty(pgnLine))
                {
                    // if the line is blank move to the next state
                    if (state == "tags")
                    {
                        state = "moves";
                    }
                    else if (state == "moves")
                    {
                        // before moving to the next game, add a new game based on the previous data
                        games.Add(new ChessGame(round, result, moves, blackName, blackElo, whiteName, whiteElo, eventName, eventSite, eventDate));

                        moves = "";

                        state = "tags";
                    }
                }
                else
                {
                    if (state == "tags")
                    {
                        // first try and get the string encased in brackets
                        int startIndex = pgnLine.IndexOf('[');
                        int length = pgnLine.IndexOf(']') - startIndex;
                        if (startIndex != -1 && length >= 0)
                        {
                            string trimLine = pgnLine.Substring(startIndex + 1, length - 1);

                            // check if the tag is one of the fields we need and save it if it is
                            if (trimLine.StartsWith("Event "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                eventName = trimLine;
                            }
                            else if (trimLine.StartsWith("Site "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                eventSite = trimLine;
                            }
                            else if (trimLine.StartsWith("Round "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                round = trimLine;
                            }
                            else if (trimLine.StartsWith("White "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                whiteName = trimLine;
                            }
                            else if (trimLine.StartsWith("Black "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                blackName = trimLine;
                            }
                            else if (trimLine.StartsWith("Result "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                string resultString = trimLine;
                                if (resultString == "1-0")
                                {
                                    result = "W";
                                }
                                else if (resultString == "0-1")
                                {
                                    result = "B";
                                }
                                else
                                {
                                    result = "D";
                                }
                            }
                            else if (trimLine.StartsWith("WhiteElo "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                whiteElo = trimLine;
                            }
                            else if (trimLine.StartsWith("BlackElo "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                blackElo = trimLine;
                            }
                            else if (trimLine.StartsWith("EventDate "))
                            {
                                startIndex = trimLine.IndexOf('"');
                                length = trimLine.LastIndexOf('"') - startIndex;
                                trimLine = trimLine.Substring(startIndex + 1, length - 1);
                                eventDate = trimLine;
                            }
                        }
                    }
                    else if (state == "moves")
                    {
                        // add the line to the string representing the moves
                        moves += pgnLine;
                    }
                }
            }

            return games;
        }
    }
}
