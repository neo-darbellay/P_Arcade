using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace P_Arcade
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
        const byte FIRST_TILE_X = 5;
        const byte FIRST_TILE_Y = 6;

        /// <summary>
        /// A counter used to keep track of how many pieces have been placed during the game
        /// </summary>
        static byte bytCounter = 0;

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
            GetInputInBounds(out bytRow, VAL_MIN_ROWS, VAL_MAX_ROWS);


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
            GetInputInBounds(out bytColumn, VAL_MIN_COLUMNS, VAL_MAX_COLUMNS);


            // Ask the user if they want to play with two players
            bool blnVerification = false;

            while (!blnVerification)
            {
                Console.WriteLine("\n   Would you like to play with a second player? (Y / N)");

                Console.Write("   Your input: ");

                char chrAnswer = 'X';

                char.TryParse(Console.ReadLine(), out chrAnswer);


                switch (char.ToUpper(chrAnswer))
                {
                    case 'y':
                    case 'Y':
                        blnTwoPlayers = true;
                        blnVerification = true;
                        break;

                    case 'n':
                    case 'N':
                        blnTwoPlayers = false;
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

                    string strConsoleLine = Console.ReadLine();

                    if (byte.TryParse(strConsoleLine, out bytBotSmartness))
                    {
                        if (Enumerable.Range(1, 10).Contains(bytBotSmartness))
                        {
                            blnVerification = true;
                        }
                    }

                    Arcade.Windows11TerminalFix();
                }
            }
        }


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public override void Start()
        {
            // Full screen the app
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, 3);


            GetUserInput();

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

            // The 2D grid used for the game's logic
            byte[,] GameGrid = new byte[bytRow, bytColumn];

            // Put the cursor inside of the navigation grid
            Console.SetCursorPosition(FIRST_TILE_X, FIRST_TILE_Y);
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
                            Console.WriteLine("   Press any key to continue.");
                            Console.ReadKey(true);
                            return;

                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            // If the cursor isn't all the way on the left
                            if (bytCursorPosX > 0)
                            {
                                // Erase previous piece
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("  ");

                                // Add new piece to the left
                                bytCursorPosX--;
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            else
                            {
                                // Move the piece all the way to the right
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("  ");
                                bytCursorPosX = (byte)(bytColumn - 1);
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            break;

                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            // If the cursor isn't all the way on the right
                            if (bytCursorPosX < bytColumn - 1)
                            {
                                // Erase previous piece
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("  ");

                                // Add new piece to the right
                                bytCursorPosX++;
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            else
                            {
                                // Move the piece all the way to the left
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("  ");
                                bytCursorPosX = 0;
                                Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                                Console.Write("█");
                            }
                            break;

                        // Piece throwing system
                        case ConsoleKey.Spacebar:
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                        case ConsoleKey.Enter:
                            // Drop the piece in the first empty line
                            for (int i = bytRow - 1; i >= 0; i--)
                            {
                                if (GameGrid[i, bytCursorPosX] == 0)
                                {
                                    GameGrid[i, bytCursorPosX] = bytPlayer;
                                    Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, 10 + (i * 2));
                                    Console.Write("█");

                                    blnPiecePlaced = true;
                                    break;
                                }
                            }
                            break;

                    }

                    // Victory check
                    if (blnPiecePlaced)
                    {
                        // Increment the piece counter
                        bytCounter++;
                        if (Check_Victory(GameGrid, bytPlayer))
                        {
                            Console.SetCursorPosition(0, bytLastRow);
                            Console.Write("\n\n\n\n");
                            Console.ResetColor();

                            Console.Write("   Congratulations,  ");

                            Console.ForegroundColor = (bytPlayer == 1 ? ConsoleColor.Red : ConsoleColor.Yellow);
                            Console.Write("Player " + bytPlayer);
                            Console.ResetColor();

                            Console.Write(" ! You have won in " + bytCounter + " turns!\n");
                            Console.ResetColor();
                            Console.WriteLine("   Press any key to continue.");
                            Console.ReadKey(true);
                            return;
                        }

                        if (Grid_Full(GameGrid))
                        {
                            Console.SetCursorPosition(0, bytLastRow);
                            Console.WriteLine("\n\n\n");
                            Console.ResetColor();

                            Console.Write("   It's a tie! The game grid is full.\n\n");
                            Console.ResetColor();
                            Console.WriteLine("   Press any key to continue.");
                            Console.ReadKey(true);
                            return;
                        }

                        // Alternate between player 1 and 2
                        bytPlayer = (byte)(bytPlayer == 1 ? 2 : 1);

                        // Update the piece's color
                        Console.ForegroundColor = (bytPlayer == 1 ? ConsoleColor.Red : blnTwoPlayers ? ConsoleColor.Yellow : ConsoleColor.Cyan);
                        Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                        Console.Write("█");
                    }
                }
                // Bot's turn

                else
                {
                    int chosenCol = GetBestMove(GameGrid);

                    for (int i = bytRow - 1; i >= 0; i--)
                    {
                        if (GameGrid[i, chosenCol] == 0)
                        {
                            GameGrid[i, chosenCol] = 2;

                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.SetCursorPosition(FIRST_TILE_X + chosenCol * 4, 10 + (i * 2));
                            Console.Write("█");

                            break;
                        }
                    }

                    bytCounter++;

                    if (Check_Victory(GameGrid, 2))
                    {
                        Console.SetCursorPosition(0, bytLastRow);
                        Console.Write("\n\n\n\n");
                        Console.ResetColor();

                        Console.Write("   Computer won in " + bytCounter + " turns!\n");
                        Console.WriteLine("   Press any key to continue.");
                        Console.ReadKey(true);
                        return;
                    }

                    if (Grid_Full(GameGrid))
                    {
                        Console.WriteLine("\n\n   It's a tie!");
                        Console.ReadKey(true);
                        return;
                    }

                    bytPlayer = 1;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(FIRST_TILE_X + bytCursorPosX * 4, FIRST_TILE_Y);
                    Console.Write("█");
                }
            }
        }

        /// <summary>
        /// Get the user's input, and verify that it is in bound
        /// </summary>
        /// <param name="bytAnswer">The variable that will get changed</param>
        /// <param name="MIN_VALUE">Minimal value</param>
        /// <param name="MAX_VALUE">Maximum value</param>
        private void GetInputInBounds(out byte bytAnswer, byte MIN_VALUE, byte MAX_VALUE)
        {
            bool blnVerification = false;

            do
            {
                Console.Write("   Your input: ");

                bool blnResult = byte.TryParse(Console.ReadLine(), out bytAnswer);
                bool blnResultInBound = blnResult && (bytAnswer > MIN_VALUE) && (bytAnswer < MAX_VALUE);

                // Check if the value is correct
                if (blnResultInBound)
                {
                    blnVerification = true;
                }
                else if (blnResult)
                {
                    Console.Write("\n   Your value isn't between {0} and {1}, please retry.\n\n", MIN_VALUE, MAX_VALUE);
                }
                else
                {
                    Console.Write("\n   Your value isn't a number, please retry.\n\n");
                }

                Arcade.Windows11TerminalFix();
            }
            while (!blnVerification);
        }

        /// <summary>
        /// Check to see if either player has won
        /// </summary>
        /// <param name="GameGrid">The bidirectionnal array that stores all the pieces</param>
        /// <returns>Whether or not this player has won</returns>
        private bool Check_Victory(byte[,] GameGrid, byte bytPlayer)
        {
            // Check to see if the current grid size allows a victory
            if (bytRow < 4 || bytColumn < 4)
                return false;



            // Horizontal verification
            for (byte bytLines = 0; bytLines < bytRow; bytLines++)
            {
                for (byte bytColumns = 0; bytColumns <= bytColumn - 4; bytColumns++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                    {
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (GameGrid[bytLines, bytColumns + x] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }
                    }

                    if (blnVictory) return true;
                }
            }

            // Vertical verification
            for (byte bytColumns = 0; bytColumns < bytColumn; bytColumns++)
            {
                for (byte bytLines = 0; bytLines <= bytRow - 4; bytLines++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                    {
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (GameGrid[bytLines + x, bytColumns] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }
                    }

                    if (blnVictory) return true;
                }
            }

            // Diagional verification (\)
            for (byte bytLines = 0; bytLines <= bytRow - 4; bytLines++)
            {
                for (byte bytColumns = 0; bytColumns <= bytColumn - 4; bytColumns++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                    {
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (GameGrid[bytLines + x, bytColumns + x] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }
                    }

                    if (blnVictory) return true;
                }
            }

            // Diagonal verification (/)
            for (byte bytLines = 0; bytLines <= bytRow - 4; bytLines++)
            {
                for (byte bytColumns = 3; bytColumns < bytColumn; bytColumns++)
                {
                    bool blnVictory = true;

                    for (int x = 0; x < 4; x++)
                    {
                        // If a piece isn't the current player's, then that means the line has stopped
                        if (GameGrid[bytLines + x, bytColumns - x] != bytPlayer)
                        {
                            blnVictory = false;
                            break;
                        }
                    }

                    if (blnVictory) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check to see whether or not the grid is full
        /// </summary>
        /// <param name="GameGrid">The bidirectionnal array that stores all the pieces</param>
        /// <returns>Whether or not the grid is full</returns>
        private bool Grid_Full(byte[,] GameGrid)
        {
            for (byte bytLines = 0; bytLines < bytRow; bytLines++)
            {
                for (byte bytColumns = 0; bytColumns < bytColumn; bytColumns++)
                {
                    // Empty case = not full
                    if (GameGrid[bytLines, bytColumns] == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Clones the grid for the bot
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private byte[,] CloneGrid(byte[,] grid)
        {
            byte[,] newGrid = new byte[bytRow, bytColumn];
            Array.Copy(grid, newGrid, grid.Length);
            return newGrid;
        }

        /// <summary>
        /// Check for legal moves for the bot
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private bool ColumnAvailable(byte[,] grid, int col)
        {
            return grid[0, col] == 0;
        }

        /// <summary>
        /// Drop a piece for the bot
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="col"></param>
        /// <param name="player"></param>
        private void DropPiece(byte[,] grid, int col, byte player)
        {
            for (int row = bytRow - 1; row >= 0; row--)
            {
                if (grid[row, col] == 0)
                {
                    grid[row, col] = player;
                    return;
                }
            }
        }

        /// <summary>
        /// Evaluate the best move
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private int EvaluatePosition(byte[,] grid)
        {
            int score = 0;

            // Helper to evaluate 4-cell windows
            int EvalWindow(byte[] window)
            {
                int botPieces = window.Count(x => x == 2);
                int playerPieces = window.Count(x => x == 1);
                int empty = window.Count(x => x == 0);

                if (botPieces == 4) return 10000;
                if (botPieces == 3 && empty == 1) return 50;
                if (botPieces == 2 && empty == 2) return 10;

                // block player, aggressively
                if (playerPieces == 3 && empty == 1) return -80;

                return 0;
            }

            // Horizontal
            for (int r = 0; r < bytRow; r++)
            {
                for (int c = 0; c < bytColumn - 3; c++)
                {
                    byte[] window = { grid[r, c], grid[r, c + 1], grid[r, c + 2], grid[r, c + 3] };
                    score += EvalWindow(window);
                }
            }

            // Vertical
            for (int c = 0; c < bytColumn; c++)
            {
                for (int r = 0; r < bytRow - 3; r++)
                {
                    byte[] window = { grid[r, c], grid[r + 1, c], grid[r + 2, c], grid[r + 3, c] };
                    score += EvalWindow(window);
                }
            }

            // Diagonal \
            for (int r = 0; r < bytRow - 3; r++)
            {
                for (int c = 0; c < bytColumn - 3; c++)
                {
                    byte[] window = { grid[r, c], grid[r + 1, c + 1], grid[r + 2, c + 2], grid[r + 3, c + 3] };
                    score += EvalWindow(window);
                }
            }

            // Diagonal /
            for (int r = 0; r < bytRow - 3; r++)
            {
                for (int c = 3; c < bytColumn; c++)
                {
                    byte[] window = { grid[r, c], grid[r + 1, c - 1], grid[r + 2, c - 2], grid[r + 3, c - 3] };
                    score += EvalWindow(window);
                }
            }

            return score;
        }



        private int MinMax(byte[,] grid, int depth, int alpha, int beta, bool maximizing)
        {
            byte bytMaxMoves = 7;

            // Base cases

            if (depth == 0 ||
                Check_Victory(grid, 1) ||
                Check_Victory(grid, 2) ||
                Grid_Full(grid))
            {
                return EvaluatePosition(grid);
            }


            if (maximizing)
            {
                int maxEval = int.MinValue;


                List<int> moves = Enumerable.Range(0, bytColumn)
                                             .Where(c => ColumnAvailable(grid, c))
                                             .OrderBy(c => Math.Abs(c - bytColumn / 2))
                                             .Take(bytMaxMoves)
                                             .ToList();


                foreach (int col in moves)
                {
                    if (ColumnAvailable(grid, col))
                    {
                        var clone = CloneGrid(grid);
                        DropPiece(clone, col, 2); // bot plays as player 2
                        int eval = MinMax(clone, depth - 1, alpha, beta, false);
                        maxEval = Math.Max(maxEval, eval);
                        alpha = Math.Max(alpha, eval);
                        if (beta <= alpha) break;
                    }
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                for (int col = 0; col < bytColumn; col++)
                {
                    if (ColumnAvailable(grid, col))
                    {
                        var clone = CloneGrid(grid);
                        DropPiece(clone, col, 1); // human
                        int eval = MinMax(clone, depth - 1, alpha, beta, true);
                        minEval = Math.Min(minEval, eval);
                        beta = Math.Min(beta, eval);
                        if (beta <= alpha) break;
                    }
                }
                return minEval;
            }
        }

        /// <summary>
        /// Get the best move possible
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private int GetBestMove(byte[,] grid)
        {
            int bestScore = int.MinValue;
            int bestMove = 0;

            int depth =
                bytColumn > 8 ? 3 :
                bytBotSmartness <= 3 ? 2 :
                bytBotSmartness <= 6 ? 4 :
                                       6;

            for (int col = 0; col < bytColumn; col++)
            {
                if (ColumnAvailable(grid, col))
                {
                    var clone = CloneGrid(grid);
                    DropPiece(clone, col, 2);

                    int score = MinMax(clone, depth, int.MinValue, int.MaxValue, false);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = col;
                    }
                }
            }

            return bestMove;
        }
    }
}
