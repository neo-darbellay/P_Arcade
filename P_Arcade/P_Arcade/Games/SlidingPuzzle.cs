using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace P_Arcade
{
    internal class SlidingPuzzle : Game
    {
        public SlidingPuzzle() : base("Sliding Puzzle", false) { }

        // Constants used for min/max of the board's size
        const byte VAL_MIN_LENGTH = 3;
        const byte VAL_MIN_WIDTH = 3;
        const byte VAL_MAX_LENGTH = 15;
        const byte VAL_MAX_WIDTH = 15;

        // User input for length and width
        static byte bytLength = 0;
        static byte bytWidth = 0;

        private static int intMoves = 0;

        private static (byte row, byte col) emptyTile;

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public override void Start()
        {
            // Full screen the app
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, 3);

            intMoves = 0;

            // Get user-related values
            GetUserInput();

            // Generate the grid
            byte[,] bytGrid = CreateAndFillGrid();

            bool blnWon = false;

            // Clear the screen and add the title back
            Arcade.ShowTitle(Name);

            // Start the game up
            Console.CursorVisible = false;
            do
            {
                Console.SetCursorPosition(0, 5);
                DrawGrid(bytGrid);

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

                    bool blnResult;

                    blnResult = SwapTiles(bytGrid, bytRow, bytCol);

                    if (blnResult) intMoves++;
                }
                else if (keyPressed == ConsoleKey.Escape || keyPressed == ConsoleKey.R)
                    break;

                blnWon = CheckWin(bytGrid);
            } while (!blnWon);

            // If the user won, then show the victory message
            if (blnWon)
            {
                // Clear the screen and add the title back
                Arcade.ShowTitle(Name);
                DrawGrid(bytGrid);

                Console.WriteLine($"\n   You won a {bytLength} by {bytWidth} grid in {intMoves} moves!\n   Press any key to continue");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Check whether the user has won (if the grid is organized)
        /// </summary>
        /// <param name="bytGrid"></param>
        /// <returns></returns>
        private static bool CheckWin(byte[,] bytGrid)
        {
            byte bytExpectedNumber = 1;
            int total = bytGrid.Length;

            foreach (byte bytCurrentNumber in bytGrid)
            {
                if (bytExpectedNumber < total && bytCurrentNumber != bytExpectedNumber)
                    return false;

                bytExpectedNumber++;
            }

            return true;
        }

        /// <summary>
        /// Swap two tiles (with one being empty)
        /// </summary>
        /// <param name="bytGrid"></param>
        /// <param name="emptyTile"></param>
        /// <param name="bytRow">How much to swap (X)</param>
        /// <param name="bytCol">How much to swap (Y)</param>
        /// <returns>The empty tile's position</returns>
        private static bool SwapTiles(byte[,] bytGrid, sbyte bytRow, sbyte bytCol)
        {
            bool blnResult = false;

            // Calculate the new position of the empty tile
            byte bytNewRow = (byte)(emptyTile.row + bytRow);
            byte bytNewCol = (byte)(emptyTile.col + bytCol);

            // Check if the new position is within the grid boundaries
            if (bytNewRow >= 0 && bytNewRow < bytWidth &&
                bytNewCol >= 0 && bytNewCol < bytLength)
            {
                // Swap the empty tile with the adjacent number
                bytGrid[emptyTile.row, emptyTile.col] = bytGrid[bytNewRow, bytNewCol];
                bytGrid[bytNewRow, bytNewCol] = 0;

                // Update the empty tile position
                emptyTile = (bytNewRow, bytNewCol);

                blnResult = true;
            }

            return blnResult;
        }

        /// <summary>
        /// Create the randomized grid
        /// </summary>
        /// <returns></returns>
        private static byte[,] CreateAndFillGrid()
        {
            // Initialize the grid with sequential numbers
            byte[,] bytGrid = new byte[bytWidth, bytLength];
            byte bytCounter = 1;

            for (byte row = 0; row < bytWidth; row++)
            {
                for (byte col = 0; col < bytLength; col++)
                {
                    bytGrid[row, col] = bytCounter;
                    bytCounter++;
                }
            }

            // Set the last cell as empty
            bytGrid[bytWidth - 1, bytLength - 1] = 0;

            // Initialize random number generator
            Random random = new Random();

            // Track the position of the empty tile
            emptyTile = ((byte)(bytWidth - 1), (byte)(bytLength - 1));

            // Shuffle the grid by making random valid moves
            for (int intMove = 0; intMove < bytWidth * bytLength * 50; intMove++)
            {
                // Determine the movement direction
                (sbyte bytRow, sbyte bytCol) = (0, 0);

                switch (random.Next(4))
                {
                    case 0: bytRow = -1; break;
                    case 1: bytRow = 1; break;
                    case 2: bytCol = -1; break;
                    case 3: bytCol = 1; break;
                }

                _ = SwapTiles(bytGrid, bytRow, bytCol);
            }

            return bytGrid;
        }

        /// <summary>
        /// Draw the grid
        /// </summary>
        /// <param name="bytGrid"></param>
        private static void DrawGrid(byte[,] bytGrid)
        {
            byte bytHeight = (byte)bytGrid.GetLength(0);
            byte bytWidth = (byte)bytGrid.GetLength(1);

            string[] tab_strInstructions = new string[]
            {
                "Instructions:",
                "Use arrow keys to move the empty (white) tile",
                "Press ESC to quit the game",
                "-----------------------------",
                "Moves made: " + intMoves
            };

            for (byte x = 0; x < bytHeight; x++)
            {
                // Draw top border for first row
                if (x == 0)
                    for (int y = 0; y < bytWidth; y++)
                        Console.Write((y == 0 ? "   ╔" : "") + "═══" + (y == bytWidth - 1 ? "╗\n" : "╦"));

                // Draw row content
                Console.Write("   ║");
                for (byte y = 0; y < bytWidth; y++)
                {
                    byte bytValue = bytGrid[x, y];

                    if (bytValue == 0)
                    {
                        Console.Write(" ");
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write(" ");
                        Console.ResetColor();
                        Console.Write(" ║");
                    }
                    else if (bytValue < 10)
                    {
                        Console.Write(" " + bytValue + " ║");
                    }
                    else if (bytValue <= 99)
                    {
                        Console.Write(bytValue + " ║");
                    }
                    else
                    {
                        Console.Write(bytValue + "║");
                    }
                }

                // Draw instruction if available
                if (x < tab_strInstructions.Length)
                {
                    Console.WriteLine("\t" + tab_strInstructions[x]);
                }
                else
                {
                    Console.WriteLine();
                }

                // Draw separation line or bottom border
                if (x < bytHeight - 1)
                    for (int y = 0; y < bytWidth; y++)
                        Console.Write((y == 0 ? "   ╠" : "") + "═══" + (y == bytWidth - 1 ? "╣\n" : "╬"));
                else
                    for (int y = 0; y < bytWidth; y++)
                        Console.Write((y == 0 ? "   ╚" : "") + "═══" + (y == bytWidth - 1 ? "╝\n" : "╩"));
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
            GetInputInBounds(out bytLength, VAL_MIN_LENGTH, VAL_MAX_LENGTH);


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
            GetInputInBounds(out bytWidth, VAL_MIN_WIDTH, VAL_MAX_WIDTH);
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
                bool blnResultInBound = blnResult && (bytAnswer >= MIN_VALUE) && (bytAnswer <= MAX_VALUE);

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
    }
}
