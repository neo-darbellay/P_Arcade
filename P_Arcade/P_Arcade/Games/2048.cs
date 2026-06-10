using P_Arcade.Models;
using P_Arcade.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace P_Arcade.Games
{
    internal class TwoThousandFourtyEight : Game
    {
        /// <summary>
        /// The 2048 game's constructor
        /// </summary>
        public TwoThousandFourtyEight() : base("2048", true) { }

        // Dimension-related constants
        private const byte VAL_BOARD_HEIGHT = 4;
        private const byte VAL_CELL_HEIGHT = 5;
        private const byte VAL_BOARD_WIDTH = 4;
        private const byte VAL_CELL_WIDTH = 11;

        private static readonly Random rnd = new Random();

        private static readonly ConsoleColor[] tab_numberColors =
        {
            ConsoleColor.Gray,          // 2
            ConsoleColor.White,         // 4
            ConsoleColor.Yellow,        // 8
            ConsoleColor.DarkYellow,    // 16
            ConsoleColor.Green,         // 32
            ConsoleColor.Cyan,          // 64
            ConsoleColor.Blue,          // 128
            ConsoleColor.DarkBlue,      // 256
            ConsoleColor.Magenta,       // 512
            ConsoleColor.DarkMagenta,   // 1024
            ConsoleColor.Red,           // 2048
            ConsoleColor.DarkRed        // 4096+
        };

        public override string[] About()
        {
            string[] tab_strAbout =
            {
                "2048 is a sliding tile puzzle game where the goal is to combine tiles until you reach the 2048 tile",
                "Each move slides all tiles on the board in a chosen direction at once",
                "When two tiles with the same number collide, they merge into a single tile with their combined value",

                "",

                "After every valid move, a new tile worth either 2 or 4 is placed on a random empty cell",
                "The 4 tiles have a 10% chance of spawning",
                "Planning your moves carefully is important, as the board fills up quickly",

                "",

                "The game ends when no empty cells remain and no adjacent tiles can be merged",
                "You win by creating a tile with the value 2048, but you can keep playing beyond that for a higher score",

                "",

                "Use the arrow keys or WASD to slide all tiles in the chosen direction",
                "Press Escape to quit to the menu"
            };

            return tab_strAbout;
        }

        public override void Start()
        {
            CurrentScore = 0;

            int[,] tab_gridValues = GenerateGrid();

            // Clear the screen and add the title back
            Arcade.ShowTitle(Name);

            // Start the game up
            Console.CursorVisible = false;
            bool blnGameOver = false;
            do
            {
                Console.SetCursorPosition(0, 5);

                DrawGrid(tab_gridValues);

                ConsoleKey keyPressed = Console.ReadKey(true).Key;

                ConsoleKey[] tab_validPresses = { ConsoleKey.W, ConsoleKey.UpArrow, ConsoleKey.A, ConsoleKey.LeftArrow, ConsoleKey.D, ConsoleKey.RightArrow, ConsoleKey.S, ConsoleKey.DownArrow };

                // Check that the key pressed is valid
                if (keyPressed == ConsoleKey.Escape)
                    return;
                else if (tab_validPresses.Contains(keyPressed))
                {
                    // Determine the direction
                    string strDirection;
                    if (keyPressed == ConsoleKey.W || keyPressed == ConsoleKey.UpArrow)
                        strDirection = "UP";
                    else if (keyPressed == ConsoleKey.A || keyPressed == ConsoleKey.LeftArrow)
                        strDirection = "LEFT";
                    else if (keyPressed == ConsoleKey.D || keyPressed == ConsoleKey.RightArrow)
                        strDirection = "RIGHT";
                    else
                        strDirection = "DOWN";

                    int[,] oldGrid = (int[,])tab_gridValues.Clone();

                    tab_gridValues = MoveGrid(tab_gridValues, strDirection);

                    // Make sure the new grid isn't equal to the previous one before adding something new to it
                    if (!GridsEqual(oldGrid, tab_gridValues))
                        tab_gridValues = CreateNewTile(tab_gridValues);

                    // Update score based on the total grid
                    CurrentScore = CalculateGridScore(tab_gridValues);

                    // We know that the game is over when the grid becomes null
                    if (GridImpossible(tab_gridValues))
                        blnGameOver = true;
                }
            } while (!blnGameOver);


            if (SupportsHighscore)
            {
                Console.Clear();
                Arcade.ShowTitle(Name);

                Console.WriteLine("   Game Over!");
                Console.WriteLine($"   Final Score: {CurrentScore}");
                Console.Write("\n   Enter your name: ");

                string strName = InputService.ReadLineOrEscape();

                if (string.IsNullOrWhiteSpace(strName))
                    strName = "Tmp";

                HighScores.Add(new HighScore(CurrentScore, strName));

                Arcade.SetHighScoresToFile(this);
            }
        }

        /// <summary>
        /// Gets a tile's color
        /// </summary>
        /// <param name="intValue"></param>
        private static ConsoleColor GetTileColor(int intValue)
        {
            if (intValue < 2)
                return ConsoleColor.DarkGray;

            int intIndex = (int)Math.Log(intValue, 2) - 1;

            if (intIndex < 0)
                return ConsoleColor.DarkGray;

            if (intIndex >= tab_numberColors.Length)
                return tab_numberColors[tab_numberColors.Length - 1];

            return tab_numberColors[intIndex];
        }

        /// <summary>
        /// Returns a single cell in the grid as strings
        /// </summary>
        /// <param name="intValue">The cell's number value</param>
        private static string[] GetCellLines(int intValue)
        {
            string strText = intValue == 0 ? "" : intValue.ToString();

            int intCenterX = VAL_CELL_WIDTH / 2;
            int intCenterY = VAL_CELL_HEIGHT / 2;

            string[] tab_strLines = new string[VAL_CELL_HEIGHT];

            for (int y = 0; y < VAL_CELL_HEIGHT; y++)
            {
                char[] tab_chrLine = new char[VAL_CELL_WIDTH];

                for (int x = 0; x < VAL_CELL_WIDTH; x++)
                {
                    tab_chrLine[x] = ' ';

                    if (y == intCenterY && strText.Length > 0)
                    {
                        int intStart = intCenterX - (strText.Length / 2);

                        if (x >= intStart && x < intStart + strText.Length)
                        {
                            tab_chrLine[x] = strText[x - intStart];
                        }
                    }
                }

                tab_strLines[y] = new string(tab_chrLine);
            }

            return tab_strLines;
        }

        /// <summary>
        /// Draw an entire grid to the screen
        /// </summary>
        /// <param name="tab_gridValues">The grid itself</param>
        private static void DrawGrid(int[,] tab_gridValues)
        {
            int intRows = tab_gridValues.GetLength(0);
            int intCols = tab_gridValues.GetLength(1);

            string[][] tab_renderedCells = new string[intRows * intCols][];

            // Get every cell
            for (int intRow = 0; intRow < intRows; intRow++)
                for (int intCol = 0; intCol < intCols; intCol++)
                    tab_renderedCells[intRow * intCols + intCol] = GetCellLines(tab_gridValues[intRow, intCol]);

            // Draw row by row
            for (int intRow = 0; intRow < intRows; intRow++)
            {
                for (int intLine = 0; intLine < VAL_CELL_HEIGHT; intLine++)
                {
                    Console.Write("   ");

                    for (int intColumn = 0; intColumn < intCols; intColumn++)
                    {
                        int intIndex = intRow * intCols + intColumn;

                        Console.BackgroundColor = GetTileColor(tab_gridValues[intRow, intColumn]);
                        Console.ForegroundColor = ConsoleColor.Black;

                        Console.Write(tab_renderedCells[intIndex][intLine]);

                        Console.ResetColor();

                        // Spacing between cells
                        Console.Write("  ");
                    }

                    Console.WriteLine();
                }

                // Spacing between rows
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Calculate the grid's score
        /// </summary>
        /// <param name="tab_gridValues">The grid itself</param>
        private int CalculateGridScore(int[,] tab_gridValues)
        {
            int intScore = 0;

            for (int r = 0; r < VAL_BOARD_HEIGHT; r++)
                for (int c = 0; c < VAL_BOARD_WIDTH; c++)
                    intScore += tab_gridValues[r, c];

            return intScore;
        }

        /// <summary>
        /// Check whether or not a grid is impossible
        /// </summary>
        /// <param name="tab_gridValues">The grid itself</param>
        private static bool GridImpossible(int[,] tab_gridValues)
        {
            int intRows = tab_gridValues.GetLength(0);
            int intCols = tab_gridValues.GetLength(1);

            // Check for empty cells
            for (int intRow = 0; intRow < intRows; intRow++)
                for (int intCol = 0; intCol < intCols; intCol++)
                    if (tab_gridValues[intRow, intCol] == 0)
                        return false;

            // Check for mergeable neighbors
            for (int intRow = 0; intRow < intRows; intRow++)
                for (int intCol = 0; intCol < intCols; intCol++)
                {
                    int intValue = tab_gridValues[intRow, intCol];

                    // Right neighbor
                    if (intCol < intCols - 1 && tab_gridValues[intRow, intCol + 1] == intValue)
                        return false;

                    // Down neighbor
                    if (intRow < intRows - 1 && tab_gridValues[intRow + 1, intCol] == intValue)
                        return false;
                }

            // No moves left
            return true;
        }

        /// <summary>
        /// Checks whether or not grid A is equal to grid B
        /// </summary>
        /// <param name="tab_gridA"></param>
        /// <param name="tab_gridB"></param>
        private static bool GridsEqual(int[,] tab_gridA, int[,] tab_gridB)
        {
            // If anything is different, the grids arent equal
            for (int intRow = 0; intRow < tab_gridA.GetLength(0); intRow++)
                for (int intCol = 0; intCol < tab_gridA.GetLength(1); intCol++)
                    if (tab_gridA[intRow, intCol] != tab_gridB[intRow, intCol])
                        return false;

            return true;
        }

        /// <summary>
        /// Move everything in a grid in the given direction
        /// </summary>
        /// <param name="tab_gridValues">The grid itself</param>
        /// <param name="strDirection">The direction itself</param>
        private int[,] MoveGrid(int[,] tab_gridValues, string strDirection)
        {
            int intRows = tab_gridValues.GetLength(0);
            int intCols = tab_gridValues.GetLength(1);

            for (int i = 0; i < (strDirection == "LEFT" || strDirection == "RIGHT" ? intRows : intCols); i++)
            {
                int[] intLine = new int[4];

                // Extract row/column
                for (int j = 0; j < 4; j++)
                {
                    switch (strDirection)
                    {
                        case "LEFT":
                            intLine[j] = tab_gridValues[i, j];
                            break;

                        case "RIGHT":
                            intLine[j] = tab_gridValues[i, intCols - 1 - j];
                            break;

                        case "UP":
                            intLine[j] = tab_gridValues[j, i];
                            break;

                        case "DOWN":
                            intLine[j] = tab_gridValues[intRows - 1 - j, i];
                            break;
                    }
                }

                // Remove zeroes
                List<int> lst_intValues = intLine.Where(x => x != 0).ToList();

                // Merge equal neighbours
                for (int j = 0; j < lst_intValues.Count - 1; j++)
                {
                    if (lst_intValues[j] == lst_intValues[j + 1])
                    {
                        lst_intValues[j] *= 2;

                        lst_intValues.RemoveAt(j + 1);
                    }
                }

                // Pad with zeroes
                while (lst_intValues.Count < 4)
                    lst_intValues.Add(0);

                intLine = lst_intValues.ToArray();

                // Write back
                for (int j = 0; j < 4; j++)
                {
                    switch (strDirection)
                    {
                        case "LEFT":
                            tab_gridValues[i, j] = intLine[j];
                            break;

                        case "RIGHT":
                            tab_gridValues[i, intCols - 1 - j] = intLine[j];
                            break;

                        case "UP":
                            tab_gridValues[j, i] = intLine[j];
                            break;

                        case "DOWN":
                            tab_gridValues[intRows - 1 - j, i] = intLine[j];
                            break;
                    }
                }
            }

            return tab_gridValues;
        }

        /// <summary>
        /// Create a new tile in a random empty space, inside of a grid
        /// </summary>
        /// <param name="tab_gridValues">The grid itself</param>
        private static int[,] CreateNewTile(int[,] tab_gridValues)
        {
            // Collect all empty cells
            List<(int, int)> emptyCells = new List<(int, int)>();
            for (int intRow = 0; intRow < VAL_BOARD_HEIGHT; intRow++)
                for (int intCol = 0; intCol < VAL_BOARD_WIDTH; intCol++)
                    if (tab_gridValues[intRow, intCol] == 0)
                        emptyCells.Add((intRow, intCol));

            // no space to spawn a new tile
            if (emptyCells.Count == 0)
                return null;

            // Pick a random empty cell
            (int intEmptyRow, int intEmptyCol) = emptyCells[rnd.Next(emptyCells.Count)];

            // Spawn a 2 (90%) or 4 (10%) randomly
            int intNewValue = rnd.Next(0, 10) == 0 ? 4 : 2;

            tab_gridValues[intEmptyRow, intEmptyCol] = intNewValue;

            return tab_gridValues;
        }

        /// <summary>
        /// Generates the game's grid with two new random tiles inside of it
        /// </summary>
        private static int[,] GenerateGrid()
        {
            int[,] tab_gridValues = new int[VAL_BOARD_HEIGHT, VAL_BOARD_WIDTH];

            tab_gridValues = CreateNewTile(tab_gridValues);
            tab_gridValues = CreateNewTile(tab_gridValues);

            return tab_gridValues;
        }
    }
}