using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using P_Arcade.Models;
using P_Arcade.Services;

namespace P_Arcade.Games
{
    internal class MineSweeper : Game
    {
        /// <summary>
        /// The MineSweeper game's constructor
        /// </summary>
        public MineSweeper() : base("MineSweeper", false) { }

        // Constants used for min/max of the board's size and the difficulty (which is used to determine the number of mines)
        const byte VAL_MIN_LENGTH = 6;
        const byte VAL_MIN_WIDTH = 6;
        const byte VAL_MIN_DIFFICULTY = 1;
        const byte VAL_MAX_LENGTH = 15;
        const byte VAL_MAX_DIFFICULTY = 10;
        const byte VAL_MAX_WIDTH = 15;

        // Sprites used for the grid
        const char HIDDEN = '▓';
        const char MINE = '☼';
        const char FLAG = 'P';

        static bool blnWon;
        static bool blnGameOver;

        static Stopwatch Timer = new Stopwatch();

        // The currently selected tile
        private static (byte row, byte col) currentTile;

        // User input for length, width, and difficulty
        static byte bytLength = 0;
        static byte bytWidth = 0;
        static byte bytDifficulty = 0;

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public override void Start()
        {
            // Full screen the app
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, 3);

            // Get user-related values
            GetUserInput();

            // Generate the grid
            sbyte[,] bytGridValues;
            bool[,] blnRevealed;
            bool[,] blnFlagged;

            (bytGridValues, blnRevealed, blnFlagged) = GenerateGrid();

            blnWon = false;
            blnGameOver = false;

            // Clear the screen and add the title back
            Arcade.ShowTitle(Name);

            // Start the game up
            Console.CursorVisible = false;
            Timer.Restart();
            do
            {
                Console.SetCursorPosition(0, 5);
                DrawGrid(bytGridValues, blnRevealed, blnFlagged);

                // Check to see if the user pressed a valid key
                ConsoleKey keyPressed = Console.ReadKey(true).Key;

                ConsoleKey[] tab_MovementKeys =
                {
                    ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.W, ConsoleKey.A, ConsoleKey.S, ConsoleKey.D
                };

                ConsoleKey[] tab_RevealKeys =
                {
                    ConsoleKey.R, ConsoleKey.Enter
                };

                ConsoleKey[] tab_FlagKeys =
                {
                    ConsoleKey.F, ConsoleKey.Spacebar
                };

                if (tab_MovementKeys.Contains(keyPressed))
                {
                    // Determine the movement direction
                    (sbyte bytRow, sbyte bytCol) = (0, 0);

                    switch (keyPressed)
                    {
                        case ConsoleKey.LeftArrow: case ConsoleKey.A: bytCol = -1; break;
                        case ConsoleKey.RightArrow: case ConsoleKey.D: bytCol = 1; break;
                        case ConsoleKey.UpArrow: case ConsoleKey.W: bytRow = -1; break;
                        case ConsoleKey.DownArrow: case ConsoleKey.S: bytRow = 1; break;
                    }

                    MoveInGrid(bytGridValues, bytRow, bytCol);
                }
                else if (tab_RevealKeys.Contains(keyPressed) && !blnFlagged[currentTile.row, currentTile.col])
                    // Reveal the current tile
                    RevealTile(bytGridValues, blnRevealed, blnFlagged, currentTile.row, currentTile.col);
                else if (tab_FlagKeys.Contains(keyPressed) && !blnRevealed[currentTile.row, currentTile.col])
                {
                    bool blnHasRevealedNeighbour = false;

                    for (int dx = -1; dx <= 1 && !blnHasRevealedNeighbour; dx++)
                    {
                        for (int dy = -1; dy <= 1 && !blnHasRevealedNeighbour; dy++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            int nx = currentTile.row + dx;
                            int ny = currentTile.col + dy;

                            if (nx >= 0 && nx < blnRevealed.GetLength(0) &&
                                ny >= 0 && ny < blnRevealed.GetLength(1) &&
                                blnRevealed[nx, ny])
                            {
                                blnHasRevealedNeighbour = true;
                            }
                        }
                    }

                    if (blnHasRevealedNeighbour)
                        blnFlagged[currentTile.row, currentTile.col] = !blnFlagged[currentTile.row, currentTile.col];
                }
                else if (keyPressed == ConsoleKey.Escape)
                    break;

                CheckWin(bytGridValues, blnRevealed, blnFlagged);
            } while (!blnWon && !blnGameOver);

            Timer.Stop();

            // Display the corresponding win message
            if (blnGameOver)
            {
                // Clear the screen and add the title back
                Arcade.ShowTitle(Name);
                DrawGrid(bytGridValues, blnRevealed, blnFlagged);

                Console.WriteLine("\n   Game Over!");
                Console.ReadKey(true);
            }
            else if (blnWon)
            {
                // Clear the screen and add the title back
                Arcade.ShowTitle(Name);
                DrawGrid(bytGridValues, blnRevealed, blnFlagged);

                // Format the time
                long lngElapsedMiliseconds = Timer.ElapsedMilliseconds;

                string strTime = FormatMilliseconds(lngElapsedMiliseconds);

                Console.WriteLine($"\n   You solved a {bytLength} by {bytWidth} grid on difficulty {bytDifficulty} in {strTime}!\n   Press any key to continue");
                Console.ReadKey(true);
            }
        }
        
        /// <summary>
        /// Converts milliseconds into a readable duration string
        /// </summary>
        /// <param name="lngMiliseconds">The duration in milliseconds</param>
        /// <returns></returns>
        public static string FormatMilliseconds(long lngMiliseconds)
        {
            if (lngMiliseconds < 0)
                return "0ms";

            TimeSpan timeSpan = TimeSpan.FromMilliseconds(lngMiliseconds);

            // Build a readable string, skipping zero units
            string strResult = string.Format("{0}{1}{2}{3}",
                timeSpan.Days > 0 ? $"{timeSpan.Days} day " : "",
                timeSpan.Hours > 0 ? $"{timeSpan.Hours} hour " : "",
                timeSpan.Minutes > 0 ? $"{timeSpan.Minutes} minutes " : "",
                timeSpan.Seconds > 0 ? $"{timeSpan.Seconds} seconds " : ""
            ).Trim();

            // If everything is zero, return "0ms"
            return string.IsNullOrEmpty(strResult) ? "0 second" : strResult;
        }

        /// <summary>
        /// Check whether the user has won (all flags are placed properly, or all safe tiles are revealed)
        /// </summary>
        /// <param name="bytGridValue">The grid itself, containing every value</param>
        /// <param name="blnRevealed">The grid used to know whether or not a tile has been revealed</param>
        /// <param name="blnFlagged"> The grid used to know whether or not a tile has been flagged</param>
        /// <returns></returns>
        private static void CheckWin(sbyte[,] bytGridValue, bool[,] blnRevealed, bool[,] blnFlagged)
        {
            blnWon = false;

            byte bytRows = (byte)bytGridValue.GetLength(0);
            byte bytCols = (byte)bytGridValue.GetLength(1);

            // Check if every mine is flagged and every flag is on a mine
            bool blnFlagsCorrect = true;

            for (byte bytRow = 0; bytRow < bytRows; bytRow++)
            {
                for (byte bytCol = 0; bytCol < bytCols; bytCol++)
                {
                    bool blnIsMine = bytGridValue[bytRow, bytCol] == 9;

                    if (blnIsMine != blnFlagged[bytRow, bytCol])
                    {
                        blnFlagsCorrect = false;
                        break;
                    }
                }

                if (!blnFlagsCorrect)
                    break;
            }

            if (blnFlagsCorrect)
            {
                blnWon = true;
                return;
            }

            // Check if every safe tile is revealed
            bool blnAllSafeTilesRevealed = true;

            for (byte bytRow = 0; bytRow < bytRows; bytRow++)
            {
                for (byte bytCol = 0; bytCol < bytCols; bytCol++)
                {
                    bool blnIsMine = bytGridValue[bytRow, bytCol] == 9;

                    if (!blnIsMine && !blnRevealed[bytRow, bytCol])
                    {
                        blnAllSafeTilesRevealed = false;
                        break;
                    }
                }

                if (!blnAllSafeTilesRevealed)
                    break;
            }

            blnWon = blnAllSafeTilesRevealed;
        }

        /// <summary>
        /// Move the cursor around in the grid
        /// </summary>
        /// <param name="bytGridValue">The grid itself, containing every value</param>
        /// <param name="bytRowOffset">How much to move on the X axis</param>
        /// <param name="bytColOffset">How much to move on the Y axis</param>
        private static void MoveInGrid(sbyte[,] bytGridValue, sbyte bytRowOffset, sbyte bytColOffset)
        {
            int bytNewRow = currentTile.row + bytRowOffset;
            int bytNewCol = currentTile.col + bytColOffset;

            // Teleport across edges
            if (bytNewRow < 0)
                bytNewRow = bytGridValue.GetLength(0) - 1;
            else if (bytNewRow >= bytGridValue.GetLength(0))
                bytNewRow = 0;

            if (bytNewCol < 0)
                bytNewCol = bytGridValue.GetLength(1) - 1;
            else if (bytNewCol >= bytGridValue.GetLength(1))
                bytNewCol = 0;

            currentTile = ((byte)bytNewRow, (byte)bytNewCol);
        }

        /// <summary>
        /// Reveal every mine, triggers when the game ends
        /// </summary>
        /// <param name="bytGridValue">The grid itself, containing every value</param>
        /// <param name="blnRevealed">The grid used to know whether or not a tile has been revealed</param>
        /// <param name="blnFlagged"> The grid used to know whether or not a tile has been flagged</param>
        private static void RevealMines(sbyte[,] bytGridValue, bool[,] blnRevealed, bool[,] blnFlagged)
        {
            // Reveal all mines
            for (sbyte x = 0; x < bytGridValue.GetLength(0); x++)
            {
                for (sbyte y = 0; y < bytGridValue.GetLength(1); y++)
                {
                    sbyte bytValue = bytGridValue[x, y];

                    if (bytValue == 9)
                    {
                        blnRevealed[x, y] = true;
                        blnFlagged[x, y] = false;
                    }
                }
            }

            blnGameOver = true;
        }

        /// <summary>
        /// Reveal the current tile, and if it's a zero, reveal the area around it as well
        /// </summary>
        /// <param name="bytGridValue">The grid itself, containing every value</param>
        /// <param name="blnRevealed">The grid used to know whether or not a tile has been revealed</param>
        /// <param name="blnFlagged"> The grid used to know whether or not a tile has been flagged</param>
        /// <param name="bytRow">The X coordinate</param>
        /// <param name="bytCol">The Y coordinate</param>
        private static void RevealArea(sbyte[,] bytGridValue, bool[,] blnRevealed, bool[,] blnFlagged, byte bytRow, byte bytCol)
        {
            // Out of bounds
            if (bytRow >= bytGridValue.GetLength(0) || bytCol >= bytGridValue.GetLength(1))
                return;

            // Already revealed or flagged
            if (blnRevealed[bytRow, bytCol] || blnFlagged[bytRow, bytCol])
                return;

            // Reveal current tile
            blnRevealed[bytRow, bytCol] = true;

            // Stop if this is a numbered tile
            if (bytGridValue[bytRow, bytCol] != 0)
                return;

            // Reveal all neighbours
            for (sbyte bytRowOffset = -1; bytRowOffset <= 1; bytRowOffset++)
            {
                for (sbyte bytColOffset = -1; bytColOffset <= 1; bytColOffset++)
                {
                    if (bytRowOffset == 0 && bytColOffset == 0)
                        continue;

                    byte bytNeighbourRow = (byte)(bytRow + bytRowOffset);
                    byte bytNeighbourCol = (byte)(bytCol + bytColOffset);

                    if (bytNeighbourRow < bytGridValue.GetLength(0) &&
                        bytNeighbourCol < bytGridValue.GetLength(1))
                    {
                        RevealArea(bytGridValue, blnRevealed, blnFlagged, bytNeighbourRow, bytNeighbourCol);
                    }
                }
            }
        }

        /// <summary>
        /// Reveals a tile and its neighbors if that tile is a 0
        /// </summary>
        /// <param name="bytGridValue">The grid itself, containing every value</param>
        /// <param name="blnRevealed">The grid used to know whether or not a tile has been revealed</param>
        /// <param name="blnFlagged"> The grid used to know whether or not a tile has been flagged</param>
        /// <param name="bytRow">The X coordinate</param>
        /// <param name="bytCol">The Y coordinate</param>
        private static void RevealTile(sbyte[,] bytGridValue, bool[,] blnRevealed, bool[,] blnFlagged, byte bytRow, byte bytCol)
        {
            if (bytGridValue[bytRow, bytCol] == 9)
            {
                RevealMines(bytGridValue, blnRevealed, blnFlagged);
            }
            else if (bytGridValue[bytRow, bytCol] == 0)
            {
                RevealArea(bytGridValue, blnRevealed, blnFlagged, bytRow, bytCol);
            }
            else
            {
                blnRevealed[bytRow, bytCol] = true;
            }
        }

        /// <summary>
        /// Generate the grid by placing mines randomly, then computing adjacency counts for all other tiles.
        /// </summary>
        /// <returns></returns>
        private static (sbyte[,] bytGrid, bool[,] blnRevealed, bool[,] blnFlagged) GenerateGrid()
        {
            Random rnd = new Random();

            sbyte[,] bytGrid = new sbyte[bytLength, bytWidth];
            bool[,] blnRevealed = new bool[bytLength, bytWidth];
            bool[,] blnFlagged = new bool[bytLength, bytWidth];

            // The mine count is determined by the size of the grid and the difficulty, with a minimum of 2 mines
            int intMineCount = Math.Max(2, (bytLength * bytWidth * bytDifficulty) / 30);

            // Scatter mines
            int intMinesPlaced = 0;
            while (intMinesPlaced < intMineCount)
            {
                byte x = (byte)rnd.Next(0, bytLength);
                byte y = (byte)rnd.Next(0, bytWidth);

                if (bytGrid[x, y] != 9)
                {
                    bytGrid[x, y] = 9;
                    intMinesPlaced++;
                }
            }

            // Count adjacent mines for every non-mine tile
            for (byte x = 0; x < bytLength; x++)
            {
                for (byte y = 0; y < bytWidth; y++)
                {
                    if (bytGrid[x, y] == 9)
                        continue;

                    sbyte bytCount = 0;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < bytLength && ny >= 0 && ny < bytWidth && bytGrid[nx, ny] == 9)
                                bytCount++;
                        }
                    }

                    bytGrid[x, y] = bytCount;
                }
            }

            // Reveal a safe area at the start of the game
            bool blnFound = false;

            for (byte x = 0; x < bytLength && !blnFound; x++)
            {
                for (byte y = 0; y < bytWidth && !blnFound; y++)
                {
                    if (bytGrid[x, y] == 0)
                    {
                        RevealArea(bytGrid, blnRevealed, blnFlagged, x, y);

                        // Move the currently selected tile to it
                        currentTile = (x, y);
                        
                        blnFound = true;
                    }
                }
            }

            return (bytGrid, blnRevealed, blnFlagged);
        }

        /// <summary>
        /// Get the symbol to display for a tile based on its value and whether it's revealed or flagged
        /// </summary>
        /// <param name="bytValue">The current number logically on the tile</param>
        /// <param name="blnRevealed">Whether or not the tile has been revealed</param>
        /// <param name="blnFlagged">Whether or not the tile has a flag</param>
        /// <returns></returns>
        private static string GetTileSymbol(sbyte bytValue, bool blnRevealed, bool blnFlagged)
        {
            if (blnFlagged)
                return FLAG.ToString();

            if (!blnRevealed)
                return HIDDEN.ToString();

            if (bytValue == 9)
                return MINE.ToString();

            if (bytValue == 0)
                return " ";

            return bytValue.ToString();
        }

        /// <summary>
        /// Draw the minesweeper grid to the console, using the values in the grid and whether each tile is revealed or flagged to determine what to display for each tile
        /// </summary>
        /// <param name="bytGridValue">The grid itself, containing every value</param>
        /// <param name="blnRevealed">The grid used to know whether or not a tile has been revealed</param>
        /// <param name="blnFlagged"> The grid used to know whether or not a tile has been flagged</param>
        private static void DrawGrid(sbyte[,] bytGridValue, bool[,] blnRevealed, bool[,] blnFlagged)
        {
            byte bytHeight = (byte)bytGridValue.GetLength(0);
            byte bytWidth = (byte)bytGridValue.GetLength(1);

            string[] tab_strInstructions = new string[]
            {
                "Instructions:",
                "Use arrow keys or WASD to move around",
                "press R or Enter to Reveal a tile",
                "press F or Space to Flag a tile, if its nearby revealed ones",
                "Press ESC to quit the game",
                "-----------------------------",
                "Difficulty " + bytDifficulty
            };

            for (byte x = 0; x < bytHeight; x++)
            {
                if (x == 0)
                {
                    Console.Write("   ╔");
                    for (int y = 0; y < bytWidth; y++)
                        Console.Write("═════" + (y == bytWidth - 1 ? "╗\n" : "╦"));
                }

                // TOP INSIDE LINE
                Console.Write("   ║");
                for (byte y = 0; y < bytWidth; y++)
                {
                    bool blnSelected = x == currentTile.row && y == currentTile.col;

                    if (!blnRevealed[x, y])
                        if (blnSelected)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("█▓▓▓█");
                            Console.ResetColor();
                        }
                        else
                            Console.Write("▓▓▓▓▓");
                    else if (blnRevealed[x, y] && bytGridValue[x, y] == 9 && !blnFlagged[x, y])
                    {
                        Console.BackgroundColor = ConsoleColor.Red;

                        if (blnSelected)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("█   █");
                        }
                        else
                            Console.Write("     ");

                        Console.ResetColor();
                    }
                    else if (blnSelected)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("█   █");
                        Console.ResetColor();
                    }
                    else
                        Console.Write("     ");
                    
                    Console.Write("║");
                }
                Console.WriteLine();

                // MIDDLE INSIDE LINE
                Console.Write("   ║");
                for (byte y = 0; y < bytWidth; y++)
                {
                    bool blnSelected = x == currentTile.row && y == currentTile.col;

                    sbyte strValue = bytGridValue[x, y];
                    string strSymbol = GetTileSymbol(strValue, blnRevealed[x, y], blnFlagged[x, y]);

                    if (!blnRevealed[x, y])
                    {
                        char chrCurrent = blnFlagged[x, y] ? FLAG : '▓';

                        if (blnSelected)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("▓▓");
                            Console.ResetColor();

                            Console.BackgroundColor = blnFlagged[x, y] ? ConsoleColor.White : ConsoleColor.Black;
                            Console.ForegroundColor = blnFlagged[x, y] ? ConsoleColor.Red : ConsoleColor.White;
                            Console.Write(chrCurrent);
                            Console.ResetColor();

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("▓▓");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write("▓▓");

                            Console.BackgroundColor = blnFlagged[x, y] ? ConsoleColor.White : ConsoleColor.Black;
                            Console.ForegroundColor = blnFlagged[x, y] ? ConsoleColor.Red : ConsoleColor.Gray;
                            Console.Write(chrCurrent);
                            Console.ResetColor();

                            Console.Write("▓▓");
                        }
                    }
                    else if (blnRevealed[x, y] && strValue == 9  && !blnFlagged[x, y])
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write($"  {strSymbol}  ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("  ");

                        if (blnFlagged[x, y])
                            Console.ForegroundColor = ConsoleColor.Red;
                        else if (strValue > 0 && strValue < 9)
                        {
                            ConsoleColor[] colors =
                            {
                                ConsoleColor.Blue,
                                ConsoleColor.Green,
                                ConsoleColor.Red,
                                ConsoleColor.DarkBlue,
                                ConsoleColor.DarkRed,
                                ConsoleColor.DarkCyan,
                                ConsoleColor.DarkGray,
                                ConsoleColor.Gray
                            };

                            Console.ForegroundColor = colors[strValue - 1];
                        }

                        Console.Write(strSymbol);
                        Console.ResetColor();
                        Console.Write("  ");
                    }

                    Console.Write("║");
                }

                if (x < tab_strInstructions.Length)
                    Console.Write("\t" + tab_strInstructions[x]);

                Console.WriteLine();

                // BOTTOM INSIDE LINE
                Console.Write("   ║");
                for (byte y = 0; y < bytWidth; y++)
                {
                    bool blnSelected = x == currentTile.row && y == currentTile.col;

                    if (!blnRevealed[x, y])
                        if (blnSelected)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("█▓▓▓█");
                            Console.ResetColor();
                        }
                        else
                            Console.Write("▓▓▓▓▓");
                    else if (blnRevealed[x, y] && bytGridValue[x, y] == 9 && !blnFlagged[x, y])
                    {
                        Console.BackgroundColor = ConsoleColor.Red;

                        if (blnSelected)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("█   █");
                        }
                        else
                            Console.Write("     ");

                        Console.ResetColor();
                    }
                    else
                        if (blnSelected)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("█   █");
                            Console.ResetColor();
                        }
                        else
                            Console.Write("     ");

                    Console.Write("║");
                }
                Console.WriteLine();

                if (x < bytHeight - 1)
                {
                    Console.Write("   ╠");
                    for (int y = 0; y < bytWidth; y++)
                        Console.Write("═════" + (y == bytWidth - 1 ? "╣\n" : "╬"));
                }
                else
                {
                    Console.Write("   ╚");
                    for (int y = 0; y < bytWidth; y++)
                        Console.Write("═════" + (y == bytWidth - 1 ? "╝\n" : "╩"));
                }
            }

            for (int i = bytHeight; i < tab_strInstructions.Length; i++)
            {
                Console.Write("    ");

                for (int y = 0; y < bytWidth; y++)
                    Console.Write("      ");

                Console.WriteLine("\t" + tab_strInstructions[i]);
            }
        }

        /// <summary>
        /// Getting the user input
        /// </summary>
        private void GetUserInput()
        {
            Arcade.ShowTitle(Name);

            // Ask the user for the number of rows they want
            Console.Write("   Please enter the length of the board that you want.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_LENGTH);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_LENGTH);
            Console.ResetColor();

            // Get the correct input
            InputService.GetInputInBoundaries(out bytLength, VAL_MIN_LENGTH, VAL_MAX_LENGTH);


            // Ask the user for the number of columns they want
            Console.Write("\n   Please enter the width of the board that you want.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_WIDTH);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_WIDTH);
            Console.ResetColor();

            // Get the correct input
            InputService.GetInputInBoundaries(out bytWidth, VAL_MIN_WIDTH, VAL_MAX_WIDTH);


            // Ask the user for the difficulty they want
            Console.Write("\n   Please enter the difficulty you want.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_DIFFICULTY);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_DIFFICULTY);
            Console.ResetColor();

            // Get the correct input
            InputService.GetInputInBoundaries(out bytDifficulty, VAL_MIN_DIFFICULTY, VAL_MAX_DIFFICULTY);
        }
    }
}
