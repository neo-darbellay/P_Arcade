using System;
using System.Collections.Generic;
using System.Linq;

using P_Arcade.Models;
using P_Arcade.Services;

namespace P_Arcade.Games
{
    /// <summary>
    /// The classic Connect 4 game, ported to C# Console.
    /// This was originally made a year ago for the I319 module, but modified this year
    /// </summary>
    internal class Connect4 : Game
    {
        /// <summary>
        /// The Connect 4 game's constructor
        /// </summary>
        public Connect4() : base("Connect 4", false) { }

        /// <summary>
        /// The current player
        /// </summary>
        static byte bytPlayer = 1;

        // Constants used for min/max of rows/columns
        const byte VAL_MIN_ROWS = 5;
        const byte VAL_MAX_ROWS = 13;
        const byte VAL_MIN_COLUMNS = 6;
        const byte VAL_MAX_COLUMNS = 16;

        // User input for rows and columns
        static byte bytRow = 0;
        static byte bytColumn = 0;

        /// <summary>
        /// Variables used as a fix to windows 11's terminal not having infinite scroll
        /// </summary>
        static byte bytLastRow = 0;

        /// <summary>
        /// Whether or not there is a second player
        /// </summary>
        static bool blnTwoPlayers = true;

        /// <summary>
        /// The second player (bot)'s level of thinking
        /// </summary>
        static byte bytBotSmartness = 1;

        // The game piece's X position
        static byte bytCursorPosX = 0;

        // The first tile's X and Y position
        const byte VAL_FIRST_TILE_X = 5;
        const byte VAL_FIRST_TILE_Y = 6;

        // Whether or not the player wants to quit the game
        static bool blnExitRequested;

        /// <summary>
        /// A counter used to keep track of how many pieces have been placed during the game
        /// </summary>
        static byte bytCounter = 0;

        public override string[] About()
        {
            string[] tab_strAbout = new string[]
            {
                "Connect 4 is a game about connecting 4 pieces in a row",
                "Players take turns dropping pieces into one of the columns",
                "A line of 4 pieces can be horizontal, vertical, or diagonal",
                "The first player to connect 4 pieces wins the game",
                "If the board fills up and nobody connects 4, the game ends in a draw",

                "",

                "This version of the game contains a Bot mode, which makes you go against a computer and try to win",
                "Bot mode has 10 levels of difficulty, which you can choose from before starting the game. The higher the level, the smarter the bot will be",
                "Player 1 is always red, and Player 2 is either yellow or cyan depending on whether or not it's a second player or a bot"
            };

            return tab_strAbout;
        }

        public override void Start()
        {
            blnExitRequested = false;

            GetUserInput();

            if (blnExitRequested) return;

            // Clear the screen and add the title back
            Arcade.ShowTitle(Name);

            // Display the navigation grid (where the game piece moves before dropping)
            {
                // Initialize the last row with 5, as that's the number of free space the title gives us
                bytLastRow = 5;

                // (1/3)
                Console.Write("   ╔");
                for (byte x = 1; x < bytColumn; x++)
                {
                    Console.Write("═══╦");
                }
                Console.Write("═══╗\t    User guide");

                bytLastRow++;

                // (2/3)
                Console.Write("\n   ║");
                for (byte x = 1; x < bytColumn; x++)
                {
                    Console.Write("   ║");
                }
                Console.Write("   ║\t    -------------------");

                bytLastRow++;

                // (3/3)
                Console.Write("\n   ╚");
                for (byte x = 1; x < bytColumn; x++)
                {
                    Console.Write("═══╩");
                }
                Console.Write("═══╝\t        Movement\tDirectional keys");

                bytLastRow++;

                // Space used to for the user guide
                Console.Write("\n    ");
                for (byte x = 1; x < bytColumn; x++)
                {
                    Console.Write("    ");
                }
                Console.Write("    \t        Shoot\t\tSpacebar or Enter\n");

                bytLastRow++;

            }


            // First line of the grid
            {
                Console.Write("   ╔");
                for (byte x = 1; x < bytColumn; x++)
                {
                    Console.Write("═══╦");
                }
                Console.Write("═══╗\t        Quit\t\tEscape");

                bytLastRow++;
            }


            // Middle part of the grid
            for (byte x = 1; x < byte.MaxValue; x++)
            {
                if (x < bytRow)
                {
                    bytLastRow++;

                    Console.Write("\n   ║");
                    for (byte y = 0; y < bytColumn; y++)
                    {
                        Console.Write("   ║");
                    }

                    Console.Write("\n   ");
                    bytLastRow++;

                    Console.Write("╠");
                    for (byte y = 1; y < bytColumn; y++)
                    {
                        Console.Write("═══╬");
                    }
                    Console.Write("═══╣");

                }
                else if (x < 10)
                {

                    Console.Write("\n    ");
                    for (byte y = 0; y < bytColumn; y++)
                    {
                        Console.Write("    ");
                    }

                    Console.Write("\n\t ");
                    for (byte y = 1; y < bytColumn; y++)
                    {
                        Console.Write("    ");
                    }
                    Console.Write("    ");
                }

                // Continue the user guide on specific rows
                switch (x)
                {
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("\t        Player 1: █");

                        Console.ForegroundColor = blnTwoPlayers ? ConsoleColor.Yellow : ConsoleColor.Cyan;
                        Console.Write("\tPlayer 2: █");

                        Console.ResetColor();
                        break;

                    case 3:
                        if (!blnTwoPlayers)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("\t\t\t\tComputer level");

                            Console.ResetColor();
                        }
                        break;

                    case 4:
                        if (!blnTwoPlayers)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("\t\t\t\t----------------------");

                            Console.ResetColor();
                        }
                        break;

                    case 5:
                        if (!blnTwoPlayers)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("\t\t\t\tEasy (1-3)");

                            Console.ResetColor();
                        }
                        break;

                    case 6:
                        if (!blnTwoPlayers)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("\t\t\t\tMedium (4-6)");

                            Console.ResetColor();
                        }
                        break;

                    case 7:
                        if (!blnTwoPlayers)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("\t\t\t\tHard (7+)");

                            Console.ResetColor();
                        }
                        break;

                    case 9:
                        if (!blnTwoPlayers)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("\t\t\t\tCurrent computer level : {0}", bytBotSmartness);

                            Console.ResetColor();
                        }

                        Console.WriteLine("\n");

                        Console.SetCursorPosition(0, bytLastRow - 1);
                        break;

                    default:
                        break;
                }
            }

            // Last line of the grid
            {
                bytLastRow++;

                Console.Write("\n   ║");
                for (byte x = 0; x < bytColumn; x++)
                {
                    Console.Write("   ║");
                }

                Console.Write("\n   ╚═══");
                for (byte x = 1; x < bytColumn; x++)
                {
                    Console.Write("╩═══");
                }
                Console.Write("╝");
            }

            // The bidirectional grid used for the game's logic
            byte[,] tab_gridValues = new byte[bytRow, bytColumn];

            // Put the cursor inside of the navigation grid
            Console.SetCursorPosition(VAL_FIRST_TILE_X, VAL_FIRST_TILE_Y);
            Console.CursorVisible = false;

            // Reset variables used for the game
            Console.ForegroundColor = (bytPlayer == 1 ? ConsoleColor.Red : ConsoleColor.Yellow);

            bytPlayer = 1;
            bytCursorPosX = 0;
            bytCounter = 0;

            // Insert the piece
            Console.Write("█");

            // Game loop
            while (true)
            {
                // Check if there's two players, or if its player 1's turn
                if (blnTwoPlayers || bytPlayer == 1)
                {
                    ConsoleKey keyPressed = Console.ReadKey(true).Key;

                    bool blnPiecePlaced = false;

                    // Movement handling
                    switch (keyPressed)
                    {
                        case ConsoleKey.Backspace:
                        case ConsoleKey.Escape:
                            Console.SetCursorPosition(0, bytLastRow);
                            Console.Write("\n\n\n\n   ");

                            Console.ResetColor();
                            return;

                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            // If the cursor isn't all the way on the left
                            if (bytCursorPosX > 0)
                            {
                                // Erase previous piece
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("  ");

                                // Add new piece to the left
                                bytCursorPosX--;
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            else
                            {
                                // Move the piece all the way to the right
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("  ");
                                bytCursorPosX = (byte)(bytColumn - 1);
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            break;

                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            // If the cursor isn't all the way on the right
                            if (bytCursorPosX < bytColumn - 1)
                            {
                                // Erase previous piece
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("  ");

                                // Add new piece to the right
                                bytCursorPosX++;
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            else
                            {
                                // Move the piece all the way to the left
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("  ");

                                bytCursorPosX = 0;
                                Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            break;

                        case ConsoleKey.Spacebar:
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                        case ConsoleKey.Enter:
                            // Drop the piece in the first empty line
                            for (int i = bytRow - 1; i >= 0; i--)
                                if (tab_gridValues[i, bytCursorPosX] == 0)
                                {
                                    tab_gridValues[i, bytCursorPosX] = bytPlayer;

                                    Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, 10 + (i * 2));
                                    Console.Write("█");

                                    blnPiecePlaced = true;
                                    break;
                                }

                            break;
                    }

                    // Victory check
                    if (blnPiecePlaced)
                    {
                        // Increment the piece counter
                        bytCounter++;
                        if (Check_Victory(tab_gridValues, bytPlayer))
                        {
                            Console.SetCursorPosition(0, bytLastRow);
                            Console.WriteLine("\n\n\n");
                            Console.ResetColor();

                            Console.Write("   Congratulations,  ");

                            Console.ForegroundColor = (bytPlayer == 1 ? ConsoleColor.Red : ConsoleColor.Yellow);
                            Console.Write("Player " + bytPlayer);
                            Console.ResetColor();

                            Console.Write(" ! You have won in " + bytCounter + " turns!\n");
                            Console.ResetColor();

                            Console.ReadKey(true);
                            return;
                        }

                        if (Grid_Full(tab_gridValues))
                        {
                            Console.SetCursorPosition(0, bytLastRow);
                            Console.WriteLine("\n\n\n");
                            Console.ResetColor();

                            Console.Write("   It's a tie! The game grid is full.\n\n");
                            Console.ResetColor();

                            Console.ReadKey(true);
                            return;
                        }

                        // Alternate between player 1 and 2
                        bytPlayer = (byte)(bytPlayer == 1 ? 2 : 1);

                        // Update the piece's color
                        Console.ForegroundColor = (bytPlayer == 1 ? ConsoleColor.Red : blnTwoPlayers ? ConsoleColor.Yellow : ConsoleColor.Cyan);
                        
                        Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
                        
                        Console.Write("█");
                    }
                }
                // Bot's turn
                else
                {
                    int intChosenCol = GetBestMove(tab_gridValues);

                    // Drop a piece in the best option
                    for (int i = bytRow - 1; i >= 0; i--)
                        if (tab_gridValues[i, intChosenCol] == 0)
                        {
                            tab_gridValues[i, intChosenCol] = 2;

                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.SetCursorPosition(VAL_FIRST_TILE_X + intChosenCol * 4, 10 + (i * 2));
                            Console.Write("█");

                            break;
                        }

                    bytCounter++;

                    if (Check_Victory(tab_gridValues, 2))
                    {
                        Console.SetCursorPosition(0, bytLastRow);
                        Console.Write("\n\n\n\n");
                        Console.ResetColor();

                        Console.Write("   Computer won in " + bytCounter + " turns!\n");

                        Console.ReadKey(true);
                        return;
                    }

                    if (Grid_Full(tab_gridValues))
                    {
                        Console.WriteLine("\n\n   It's a tie!");

                        Console.ReadKey(true);
                        return;
                    }

                    bytPlayer = 1;

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.SetCursorPosition(VAL_FIRST_TILE_X + bytCursorPosX * 4, VAL_FIRST_TILE_Y);
 
                    Console.Write("█");
                }
            }
        }

        /// <summary>
        /// Check to see if either player has won
        /// </summary>
        /// <param name="tab_gridValues">The bidirectionnal array that stores all the pieces</param>
        /// <returns>Whether or not this player has won</returns>
        private bool Check_Victory(byte[,] tab_gridValues, byte bytPlayer)
        {
            // Check to see if the current grid size allows a victory
            if (bytRow < 4 || bytColumn < 4)
                return false;

            // Horizontal verification
            for (byte bytLines = 0; bytLines < bytRow; bytLines++)
                for (byte bytColumns = 0; bytColumns <= bytColumn - 4; bytColumns++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (tab_gridValues[bytLines, bytColumns + x] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }

                    if (blnVictory) return true;
                }

            // Vertical verification
            for (byte bytColumns = 0; bytColumns < bytColumn; bytColumns++)
                for (byte bytLines = 0; bytLines <= bytRow - 4; bytLines++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (tab_gridValues[bytLines + x, bytColumns] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }

                    if (blnVictory) return true;
                }

            // Diagional verification (\)
            for (byte bytLines = 0; bytLines <= bytRow - 4; bytLines++)
                for (byte bytColumns = 0; bytColumns <= bytColumn - 4; bytColumns++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (tab_gridValues[bytLines + x, bytColumns + x] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }

                    if (blnVictory) return true;
                }

            // Diagonal verification (/)
            for (byte bytLines = 0; bytLines <= bytRow - 4; bytLines++)
                for (byte bytColumns = 3; bytColumns < bytColumn; bytColumns++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (tab_gridValues[bytLines + x, bytColumns - x] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }

                    if (blnVictory) return true;
                }

            // No victory found for the current player
            return false;
        }

        /// <summary>
        /// Check to see whether or not the grid is full
        /// </summary>
        /// <param name="tab_gridValues">The bidirectionnal array that stores all the pieces</param>
        /// <returns>Whether or not the grid is full</returns>
        private bool Grid_Full(byte[,] tab_gridValues)
        {
            for (byte bytLines = 0; bytLines < bytRow; bytLines++)
                for (byte bytColumns = 0; bytColumns < bytColumn; bytColumns++)
                    // Empty case = not full
                    if (tab_gridValues[bytLines, bytColumns] == 0)
                        return false;

            return true;
        }

        /// <summary>
        /// Clones the grid for the bot
        /// </summary>
        /// <param name="tab_gridValues"></param>
        private byte[,] CloneGrid(byte[,] tab_gridValues)
        {
            byte[,] newGrid = new byte[bytRow, bytColumn];
            Array.Copy(tab_gridValues, newGrid, tab_gridValues.Length);
            return newGrid;
        }

        /// <summary>
        /// Check for legal moves for the bot
        /// </summary>
        /// <param name="tab_gridValues">The bidirectionnal array that stores all the pieces</param>
        /// <param name="intCol">The wanted column</param>
        private bool ColumnAvailable(byte[,] tab_gridValues, int intCol) => tab_gridValues[0, intCol] == 0;

        /// <summary>
        /// Drop a piece for the bot
        /// </summary>
        /// <param name="tab_gridValues">The bidirectionnal array that stores all the pieces</param>
        /// <param name="intCol"></param>
        /// <param name="bytPlayer"></param>
        private void DropPiece(byte[,] tab_gridValues, int intCol, byte bytPlayer)
        {
            for (int row = bytRow - 1; row >= 0; row--)
                if (tab_gridValues[row, intCol] == 0)
                {
                    tab_gridValues[row, intCol] = bytPlayer;
                    return;
                }
        }

        /// <summary>
        /// Evaluate the best move
        /// </summary>
        /// <param name="tab_gridValues">The bidirectionnal array that stores all the pieces</param>
        private int EvaluatePosition(byte[,] tab_gridValues)
        {
            int intScore = 0;

            // Helper to evaluate 4-cell windows
            int EvalWindow(byte[] tab_bytWindow)
            {
                int botPieces = tab_bytWindow.Count(x => x == 2);
                int playerPieces = tab_bytWindow.Count(x => x == 1);
                int empty = tab_bytWindow.Count(x => x == 0);

                if (botPieces == 4) return 10000;
                if (botPieces == 3 && empty == 1) return 50;
                if (botPieces == 2 && empty == 2) return 10;

                // block player, aggressively
                if (playerPieces == 3 && empty == 1) return -80;

                return 0;
            }

            // Horizontal
            for (int intRow = 0; intRow < bytRow; intRow++)
                for (int intCol = 0; intCol < bytColumn - 3; intCol++)
                {
                    byte[] tab_bytWindow = { tab_gridValues[intRow, intCol], tab_gridValues[intRow, intCol + 1], tab_gridValues[intRow, intCol + 2], tab_gridValues[intRow, intCol + 3] };
                    intScore += EvalWindow(tab_bytWindow);
                }

            // Vertical
            for (int intCol = 0; intCol < bytColumn; intCol++)
                for (int intRow = 0; intRow < bytRow - 3; intRow++)
                {
                    byte[] tab_bytWindow = { tab_gridValues[intRow, intCol], tab_gridValues[intRow + 1, intCol], tab_gridValues[intRow + 2, intCol], tab_gridValues[intRow + 3, intCol] };
                    intScore += EvalWindow(tab_bytWindow);
                }

            // Diagonal (\)
            for (int intRow = 0; intRow < bytRow - 3; intRow++)
                for (int c = 0; c < bytColumn - 3; c++)
                {
                    byte[] window = { tab_gridValues[intRow, c], tab_gridValues[intRow + 1, c + 1], tab_gridValues[intRow + 2, c + 2], tab_gridValues[intRow + 3, c + 3] };
                    intScore += EvalWindow(window);
                }

            // Diagonal (/)
            for (int intRow = 0; intRow < bytRow - 3; intRow++)
                for (int intCol = 3; intCol < bytColumn; intCol++)
                {
                    byte[] window = { tab_gridValues[intRow, intCol], tab_gridValues[intRow + 1, intCol - 1], tab_gridValues[intRow + 2, intCol - 2], tab_gridValues[intRow + 3, intCol - 3] };
                    intScore += EvalWindow(window);
                }

            return intScore;
        }

        /// <summary>
        /// The MinMax algorythm, used by the bot
        /// </summary>
        /// <param name="tab_gridValues">The bidirectionnal array that stores all the pieces</param>
        /// <param name="intDepth">The number of moves remaining to search ahead</param>
        /// <param name="intAlpha">The best score found so far for the maximizing player</param>
        /// <param name="intBeta">The best score found so far for the minimizing player</param>
        /// <param name="blnMaximizing">Indicates whether the current player is maximizing the score</param>
        private int MinMax(byte[,] tab_gridValues, int intDepth, int intAlpha, int intBeta, bool blnMaximizing)
        {
            byte bytMaxMoves = 7;

            // Base cases

            if (intDepth == 0 || Check_Victory(tab_gridValues, 1) || Check_Victory(tab_gridValues, 2) || Grid_Full(tab_gridValues))
                return EvaluatePosition(tab_gridValues);

            if (blnMaximizing)
            {
                int intMaxEval = int.MinValue;

                List<int> lst_intMoves = Enumerable.Range(0, bytColumn).Where(c => ColumnAvailable(tab_gridValues, c)).OrderBy(c => Math.Abs(c - bytColumn / 2)).Take(bytMaxMoves).ToList();

                foreach (int col in lst_intMoves)
                    if (ColumnAvailable(tab_gridValues, col))
                    {
                        byte[,] tab_gridClone = CloneGrid(tab_gridValues);

                        DropPiece(tab_gridClone, col, 2);

                        int intEval = MinMax(tab_gridClone, intDepth - 1, intAlpha, intBeta, false);

                        intMaxEval = Math.Max(intMaxEval, intEval);
                        intAlpha = Math.Max(intAlpha, intEval);

                        if (intBeta <= intAlpha) break;
                    }

                return intMaxEval;
            }
            else
            {
                int intMinEval = int.MaxValue;

                for (int col = 0; col < bytColumn; col++)
                {
                    if (ColumnAvailable(tab_gridValues, col))
                    {
                        byte[,] tab_gridClone = CloneGrid(tab_gridValues);

                        DropPiece(tab_gridClone, col, 1);

                        int eval = MinMax(tab_gridClone, intDepth - 1, intAlpha, intBeta, true);

                        intMinEval = Math.Min(intMinEval, eval);
                        intBeta = Math.Min(intBeta, eval);

                        if (intBeta <= intAlpha) break;
                    }
                }
                return intMinEval;
            }
        }

        /// <summary>
        /// Get the best move possible
        /// </summary>
        /// <param name="tab_gridValues">The bidirectionnal array that stores all the pieces</param>
        private int GetBestMove(byte[,] tab_gridValues)
        {
            int intBestScore = int.MinValue;
            int intBestMove = 0;

            int intDepth = bytColumn > 8 ? 3 : bytBotSmartness <= 3 ? 2 : bytBotSmartness <= 6 ? 4 : 6;

            for (int intCol = 0; intCol < bytColumn; intCol++)
                if (ColumnAvailable(tab_gridValues, intCol))
                {
                    byte[,] tab_gridClone = CloneGrid(tab_gridValues);

                    DropPiece(tab_gridClone, intCol, 2);

                    int intScore = MinMax(tab_gridClone, intDepth, int.MinValue, int.MaxValue, false);

                    if (intScore > intBestScore)
                    {
                        intBestScore = intScore;
                        intBestMove = intCol;
                    }
                }

            return intBestMove;
        }

        /// <summary>
        /// Getting the user input
        /// </summary>
        private void GetUserInput()
        {
            Arcade.ShowTitle(Name);

            // Ask the user for the number of rows they want
            Console.Write("   Please enter the number of rows you want.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_ROWS);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_ROWS);
            Console.ResetColor();

            // Get the correct input
            blnExitRequested = !InputService.GetInputInBoundaries(out bytRow, VAL_MIN_ROWS, VAL_MAX_ROWS);

            if (blnExitRequested)
                return;


            // Ask the user for the number of columns they want
            Console.Write("\n   Please enter the number of columns you want.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_COLUMNS);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_COLUMNS);
            Console.ResetColor();

            // Get the correct input
            blnExitRequested = !InputService.GetInputInBoundaries(out bytColumn, VAL_MIN_COLUMNS, VAL_MAX_COLUMNS);

            if (blnExitRequested)
                return;

            // Ask the user if they want to play with two players
            bool blnVerification = false;

            while (!blnVerification)
            {
                Console.WriteLine("\n   Would you like to play with a second player? (Y / N)");

                Console.Write("   Your input: ");

                string strAnswer = InputService.ReadLineOrEscape();

                if (strAnswer == null)
                { 
                    blnExitRequested = true;
                    return;
                }

                switch (char.ToUpper(strAnswer.Length == 1 ? strAnswer[0] : '\0'))
                {
                    case 'Y':
                        blnTwoPlayers = true;
                        blnVerification = true;
                        break;

                    case 'N':
                        blnTwoPlayers = false;

                        // Ensure player 1 will be the next player
                        bytPlayer = 1;

                        blnVerification = true;
                        break;

                    default:
                        Console.WriteLine("\n   Wrong character found. Please type Y for yes, or N for no.");
                        break;
                }

                Arcade.Windows11TerminalFix();
            }

            // Ask the user for the level of difficulty if they want to play against a bot
            if (!blnTwoPlayers)
            {
                Console.Write("\n   What level do you want the computer to be at?");

                blnVerification = false;

                while (!blnVerification)
                {
                    Console.WriteLine("\n   Please enter a number between 1 and 10, where 1 is the easiest level, and 10 is the hardest");

                    Console.Write("   Your input : ");

                    string strConsoleLine = InputService.ReadLineOrEscape();

                    if (strConsoleLine == null)
                    {
                        blnExitRequested = true;
                        return;
                    }

                    if (byte.TryParse(strConsoleLine, out bytBotSmartness) && Enumerable.Range(1, 10).Contains(bytBotSmartness))
                        blnVerification = true;

                    Arcade.Windows11TerminalFix();
                }
            }
        }
    }
}
