using MySqlConnector;

/*
  Author: Daniel Kopta and Atharv Kulkarni and Joshua Schelin
  Chess browser backend 
*/

namespace ChessBrowser
{
    internal class Queries
    {

        /// <summary>
        /// This function runs when the upload button is pressed.
        /// Given a filename, parses the PGN file, and uploads
        /// each chess game to the user's database.
        /// </summary>
        /// <param name="PGNfilename">The path to the PGN file</param>
        internal static async Task InsertGameData(string PGNfilename, MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            // Create a list of chess games from the PGN file
            List<ChessGame> games = ChessGame.GamesFromPGNFile(PGNfilename);

            mainPage.SetNumWorkItems(games.Count);

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    MySqlCommand addPlayersAndEvent = conn.CreateCommand();
                    addPlayersAndEvent.CommandText = "insert into Players (Name, Elo) values (@whiteName, @whiteElo) on duplicate key update Elo = if(@whiteElo > Elo, @whiteElo, Elo);" +
                        "insert into Players (Name, Elo) values (@blackName, @blackElo) on duplicate key update Elo = if(@blackElo > Elo, @blackElo, Elo);" +
                        "insert ignore into Events (Name, Site, Date) values (@eventName, @eventSite, @eventDate);";
                    addPlayersAndEvent.Prepare();

                    MySqlCommand addGame = conn.CreateCommand();
                    addGame.CommandText = "insert ignore into Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID) values (@round, @result, @moves, " +
                        "(select pID from Players where Name = @blackName), (select pID from Players where Name = @whiteName), (select eID from Events where Name = @eventName));";
                    addGame.Prepare();

                    foreach (ChessGame game in games)
                    {
                        // First attempt to add the corresponding Players and Event for the current Game
                        addPlayersAndEvent.Parameters.AddWithValue("@whiteName", game.whiteName);
                        addPlayersAndEvent.Parameters.AddWithValue("@whiteElo", game.whiteElo);
                        addPlayersAndEvent.Parameters.AddWithValue("@blackName", game.blackName);
                        addPlayersAndEvent.Parameters.AddWithValue("@blackElo", game.blackElo);
                        addPlayersAndEvent.Parameters.AddWithValue("@eventName", game.eventName);
                        addPlayersAndEvent.Parameters.AddWithValue("@eventSite", game.eventSite);
                        addPlayersAndEvent.Parameters.AddWithValue("@eventDate", game.eventDate);
                        addPlayersAndEvent.ExecuteNonQuery();
                        addPlayersAndEvent.Parameters.Clear();
                        
                        // Then attempt to add the Game
                        addGame.Parameters.AddWithValue("@round", game.round);
                        addGame.Parameters.AddWithValue("@result", game.result);
                        addGame.Parameters.AddWithValue("@moves", game.moves);
                        addGame.Parameters.AddWithValue("@blackName", game.blackName);
                        addGame.Parameters.AddWithValue("@whiteName", game.whiteName);
                        addGame.Parameters.AddWithValue("@eventName", game.eventName);
                        addGame.ExecuteNonQuery();
                        addGame.Parameters.Clear();

                        await mainPage.NotifyWorkItemCompleted();
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
            

        }


        /// <summary>
        /// Queries the database for games that match all the given filters.
        /// The filters are taken from the various controls in the GUI.
        /// </summary>
        /// <param name="white">The white player, or null if none</param>
        /// <param name="black">The black player, or null if none</param>
        /// <param name="opening">The first move, e.g. "1.e4", or null if none</param>
        /// <param name="winner">The winner as "W", "B", "D", or null if none</param>
        /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
        /// <param name="start">The start of the date range</param>
        /// <param name="end">The end of the date range</param>
        /// <param name="showMoves">True if the returned data should include the PGN moves</param>
        /// <returns>A string separated by newlines containing the filtered games</returns>
        internal static string PerformQuery(string white, string black, string opening,
          string winner, bool useDate, DateTime start, DateTime end, bool showMoves,
          MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            // Build up this string containing the results from your query
            string parsedResult = "";

            // Use this to count the number of rows returned by your query
            // (see below return statement)
            int numRows = 0;

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    string query = "select Round, Result, Moves, Event.Name as Event, Site, Date, Black.Name as Black, Black.Elo as BlackElo, " +
                        "White.Name as White, White.Elo as WhiteElo from Games natural join Events as Event join Players as Black join Players as White " +
                        "where BlackPlayer = Black.pID && WhitePlayer = White.pID";
                    // Append any additional conditions to the query based on the specifications
                    if (white != null)
                    {
                        query += $" && White.Name = \"{white}\"";
                    }
                    if (black != null)
                    {
                        query += $" && Black.Name = \"{black}\"";
                    }
                    if (opening != null)
                    {
                        query += $" && Moves like \"{opening}%\"";
                    }
                    if (winner != null)
                    {
                        query += $" && Result = \"{winner}\"";
                    }
                    if (useDate)
                    {
                        query += $" && Date between \"{start.ToString("yyyyMMddHHmmss")}\" and \"{end.ToString("yyyyMMddHHmmss")}\"";
                    }

                    // Finish creating the query command
                    MySqlCommand selectGames = conn.CreateCommand();
                    selectGames.CommandText = query + ";";

                    using (MySqlDataReader reader = selectGames.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Add the data for each Game that was returned to the output string
                            numRows++;
                            parsedResult += "\nEvent: " + reader["Event"];
                            parsedResult += "\nSite: " + reader["Site"];
                            parsedResult += "\nDate: " + reader["Date"];
                            parsedResult += "\nWhite: " + reader["White"] + " (" + reader["WhiteElo"] + ")";
                            parsedResult += "\nBlack: " + reader["Black"] + " (" + reader["BlackElo"] + ")";
                            parsedResult += "\nResult: " + reader["Result"];
                            if (showMoves)
                            {
                                parsedResult += "\nMoves: " + reader["Moves"];
                            }
                            parsedResult += "\n";
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return numRows + " results\n" + parsedResult;
        }

    }
}
