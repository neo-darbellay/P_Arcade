using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        // Constants used for min/max of the board's size
        const byte VAL_MIN_LENGTH = 3;
        const byte VAL_MIN_WIDTH = 3;
        const byte VAL_MAX_LENGTH = 15;
        const byte VAL_MAX_WIDTH = 15;

        // Sprites used for the grid
        const char HIDDEN = '▓';
        const char MINE = '☼';
        const char FLAG = '⚑';

        private static (byte row, byte col) currentTile;

        // User input for length and width
        static byte bytLength = 0;
        static byte bytWidth = 0;

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



            bool blnWon = false;

            // Clear the screen and add the title back
            Arcade.ShowTitle(Name);

            // Start the game up
            Console.CursorVisible = false;
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

                    //MoveInGrid(bytGridValues, bytRow, bytCol);
                }
                else if (keyPressed == ConsoleKey.Q)
                {
                    // Reveal the tile
                }
                else if (keyPressed == ConsoleKey.E)
                {
                    // Place a flag
                }
                else if (keyPressed == ConsoleKey.Escape || keyPressed == ConsoleKey.R)
                    break;

                blnWon = CheckWin(bytGridValues, blnRevealed, blnFlagged);
            } while (!blnWon);

            // If the user won, then show the victory message
            if (blnWon)
            {
                // Clear the screen and add the title back
                Arcade.ShowTitle(Name);
                DrawGrid(bytGridValues, blnRevealed, blnFlagged);

                Console.WriteLine($"\n   You solved a {bytLength} by {bytWidth} grid!\n   Press any key to continue");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Check whether the user has won (all flags are placed properly, or all safe tiles are revealed)
        /// </summary>
        /// <param name="bytGrid"></param>
        /// <returns></returns>
        private static bool CheckWin(sbyte[,] bytGrid, bool[,] blnRevealed, bool[,] blnFlagged)
        {
            return false;
        }

        /// <summary>
        /// Generate the grid
        /// </summary>
        /// <returns></returns>
        private static (sbyte[,] grid, bool[,] revealed, bool[,] flagged) GenerateGrid()
        {
            Random rnd = new Random();

            sbyte[,] grid = new sbyte[bytLength, bytWidth];
            bool[,] revealed = new bool[bytLength, bytWidth];
            bool[,] flagged = new bool[bytLength, bytWidth];

            for (byte x = 0; x < bytLength; x++)
            {
                for (byte y = 0; y < bytWidth; y++)
                {
                    // Random value between 0 and 9
                    grid[x, y] = (sbyte)rnd.Next(0, 10);

                    // Randomly reveal some tiles for testing
                    revealed[x, y] = rnd.Next(0, 2) == 1;

                    // Randomly place some flags for testing
                    flagged[x, y] = rnd.Next(0, 10) == 0;
                }
            }

            return (grid, revealed, flagged);
        }

        /// <summary>
        /// Get the symbol to display for a tile based on its value and whether it's revealed or flagged
        /// </summary>
        /// <param name="bytValue"></param>
        /// <param name="blnRevealed"></param>
        /// <param name="blnFlagged"></param>
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
            byte bytHeight = (byte)bytGrid.GetLength(0);
            byte bytWidth = (byte)bytGrid.GetLength(1);

            string[] tab_strInstructions = new string[]
            {
                "Instructions:",
                "Use arrow keys to move around",
                "press Q to reveal a tile",
                "press E to place a flag",
                "Press ESC to quit the game",
            };

            for (byte x = 0; x < bytHeight; x++)
            {
                // top border
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
                    if (!blnRevealed[x, y] && !blnFlagged[x, y])
                        Console.Write("▓▓▓▓▓");
                    else
                        Console.Write("     ");

                    Console.Write("║");
                }

                Console.WriteLine();

                // MIDDLE INSIDE LINE
                Console.Write("   ║");

                for (byte y = 0; y < bytWidth; y++)
                {
                    sbyte value = bytGrid[x, y];

                    string symbol = GetTileSymbol(
                        value,
                        blnRevealed[x, y],
                        blnFlagged[x, y]);

                    if (!blnRevealed[x, y] && !blnFlagged[x, y])
                    {
                        Console.Write("▓▓▓▓▓");
                    }
                    else
                    {
                        Console.Write("  ");

                        if (blnFlagged[x, y])
                            Console.ForegroundColor = ConsoleColor.Red;
                        else if (value == 9)
                            Console.ForegroundColor = ConsoleColor.Black;
                        else if (value > 0 && value < 9)
                        {
                            ConsoleColor[] colors =
                            {
                                ConsoleColor.Blue,
                                ConsoleColor.Green,
                                ConsoleColor.Red,
                                ConsoleColor.DarkBlue,
                                ConsoleColor.DarkRed,
                                ConsoleColor.DarkCyan,
                                ConsoleColor.Black,
                                ConsoleColor.Gray
                            };

                            Console.ForegroundColor = colors[value - 1];
                        }

                        Console.Write(symbol);

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
                    if (!blnRevealed[x, y] && !blnFlagged[x, y])
                        Console.Write("▓▓▓▓▓");
                    else
                        Console.Write("     ");

                    Console.Write("║");
                }

                Console.WriteLine();

                // separator
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

            // If grid is smaller than instruction count, print remaining instructions below
            for (int i = bytHeight; i < tab_strInstructions.Length; i++)
                for (int y = 0; y < bytWidth; y++)
                    Console.Write((y == 0 ? "    " : "") + "   " + (y == bytWidth - 1 ? (" \t" + tab_strInstructions[i] + "\n") : " "));
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
        }
    }
}
