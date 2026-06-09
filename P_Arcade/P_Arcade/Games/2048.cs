using P_Arcade.Models;
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

        private const byte VAL_BOARD_WIDTH = 4;
        private const byte VAL_BOARD_HEIGHT = 4;

        private const byte VAL_CELL_HEIGHT = 5;
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
                "TODO"
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

                ConsoleKey[] tab_validPresses =
                {
                    ConsoleKey.W,
                    ConsoleKey.UpArrow,
                    ConsoleKey.A,
                    ConsoleKey.LeftArrow,
                    ConsoleKey.D,
                    ConsoleKey.RightArrow,
                    ConsoleKey.S,
                    ConsoleKey.DownArrow
                };

                // Validate that the key pressed is valid
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

                string name = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                    name = "Tmp";

                HighScores.Add(new HighScore(CurrentScore, name));

                Arcade.SetHighScoresToFile(this);
            }
        }

        /// <summary>
        /// Gets a tile's color
        /// </summary>
        /// <param name="intValue"></param>
        /// <returns></returns>
        private static ConsoleColor GetTileColor(int intValue)
        {
            if (intValue < 2)
                return ConsoleColor.DarkGray;

            int index = (int)Math.Log(intValue, 2) - 1;

            if (index < 0)
                return ConsoleColor.DarkGray;

            if (index >= tab_numberColors.Length)
                return tab_numberColors[tab_numberColors.Length - 1];

            return tab_numberColors[index];
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

            string[] lines = new string[VAL_CELL_HEIGHT];

            for (int y = 0; y < VAL_CELL_HEIGHT; y++)
            {
                char[] line = new char[VAL_CELL_WIDTH];

                for (int x = 0; x < VAL_CELL_WIDTH; x++)
                {
                    line[x] = ' ';

                    if (y == intCenterY && strText.Length > 0)
                    {
                        int start = intCenterX - (strText.Length / 2);

                        if (x >= start && x < start + strText.Length)
                        {
                            line[x] = strText[x - start];
                        }
                    }
                }

                lines[y] = new string(line);
            }

            return lines;
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
            {
                for (int intCol = 0; intCol < intCols; intCol++)
                {
                    tab_renderedCells[intRow * intCols + intCol] = GetCellLines(tab_gridValues[intRow, intCol]);
                }
            }

            // Draw row by row
            for (int intRow = 0; intRow < intRows; intRow++)
            {
                for (int intLine = 0; intLine < VAL_CELL_HEIGHT; intLine++)
                {
                    Console.Write("   ");

                    for (int intColumn = 0; intColumn < intCols; intColumn++)
                    {
                        int intIndex = intRow * intCols + intColumn;
                        ConsoleColor backgroundColor = GetTileColor(tab_gridValues[intRow, intColumn]);

                        Console.BackgroundColor = backgroundColor;
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
        /// <param name="tab_gridValues"></param>
        /// <returns></returns>
        private int CalculateGridScore(int[,] tab_gridValues)
        {
            int score = 0;
            for (int r = 0; r < VAL_BOARD_HEIGHT; r++)
            {
                for (int c = 0; c < VAL_BOARD_WIDTH; c++)
                {
                    score += tab_gridValues[r, c];
                }
            }
            return score;
        }

        /// <summary>
        /// Check whether or not a grid is impossible
        /// </summary>
        /// <param name="tab_gridValues">The grid itself</param>
        /// <returns></returns>
        private static bool GridImpossible(int[,] tab_gridValues)
        {
            int rows = tab_gridValues.GetLength(0);
            int cols = tab_gridValues.GetLength(1);

            // 1. Check for empty cells
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (tab_gridValues[r, c] == 0)
                        return false;
                }
            }

            // 2. Check for mergeable neighbors
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int value = tab_gridValues[r, c];

                    // Right neighbor
                    if (c < cols - 1 && tab_gridValues[r, c + 1] == value)
                        return false;

                    // Down neighbor
                    if (r < rows - 1 && tab_gridValues[r + 1, c] == value)
                        return false;
                }
            }

            // 3. No moves left
            return true;
        }

        /// <summary>
        /// Checks whether or not grid A is equal to grid B
        /// </summary>
        /// <param name="tab_gridA"></param>
        /// <param name="tab_gridB"></param>
        /// <returns></returns>
        private static bool GridsEqual(int[,] tab_gridA, int[,] tab_gridB)
        {
            for (int intRow = 0; intRow < tab_gridA.GetLength(0); intRow++)
            {
                for (int intCol = 0; intCol < tab_gridA.GetLength(1); intCol++)
                {
                    if (tab_gridA[intRow, intCol] != tab_gridB[intRow, intCol])
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Move everything in a grid in the given direction
        /// </summary>
        /// <param name="tab_gridValues">The grid itself</param>
        /// <param name="strDirection">The direction itself</param>
        /// <returns></returns>
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
                List<int> values = intLine.Where(x => x != 0).ToList();

                // Merge equal neighbours
                for (int j = 0; j < values.Count - 1; j++)
                {
                    if (values[j] == values[j + 1])
                    {
                        values[j] *= 2;

                        values.RemoveAt(j + 1);
                    }
                }

                // Pad with zeroes
                while (values.Count < 4)
                    values.Add(0);

                intLine = values.ToArray();

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
        /// <returns></returns>
        private static int[,] CreateNewTile(int[,] tab_gridValues)
        {
            // Collect all empty cells
            List<(int, int)> emptyCells = new List<(int, int)>();
            for (int r = 0; r < VAL_BOARD_HEIGHT; r++)
            {
                for (int c = 0; c < VAL_BOARD_WIDTH; c++)
                {
                    if (tab_gridValues[r, c] == 0)
                        emptyCells.Add((r, c));
                }
            }

            // no space to spawn a new tile
            if (emptyCells.Count == 0)
                return null;

            // Pick a random empty cell
            (int intRow, int intCol) = emptyCells[rnd.Next(emptyCells.Count)];

            // Spawn a 2 (90%) or 4 (10%) randomly
            int newValue = rnd.Next(0, 10) == 0 ? 4 : 2;

            tab_gridValues[intRow, intCol] = newValue;

            return tab_gridValues;
        }

        /// <summary>
        /// Generates the game's grid with two new random tiles inside of it
        /// </summary>
        /// <returns></returns>
        private static int[,] GenerateGrid()
        {
            int[,] tab_gridValues = new int[VAL_BOARD_HEIGHT, VAL_BOARD_WIDTH];

            tab_gridValues = CreateNewTile(tab_gridValues);
            tab_gridValues = CreateNewTile(tab_gridValues);

            return tab_gridValues;
        }
    }
}