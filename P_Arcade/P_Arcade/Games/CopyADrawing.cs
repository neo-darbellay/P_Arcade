using System;
using System.Linq;

using P_Arcade.Models;

namespace P_Arcade.Games
{
    internal class CopyADrawing : Game
    {
        /// <summary>
        /// Copy A Drawing constructor
        /// </summary>
        public CopyADrawing() : base("Copy A Drawing", false) { }

        private const byte VAL_DRAWING_WIDTH = 11;
        private const byte VAL_DRAWING_HEIGHT = 11;

        private static readonly Random rnd = new Random();

        private static readonly (int X, int Y) Origin = (VAL_DRAWING_HEIGHT / 2, VAL_DRAWING_WIDTH / 2);

        private static (int X, int Y) Cursor;

        private static bool[,] tab_blnCurrentDrawing;
        private static bool[,] tab_blnGoalDrawing;

        public override string[] About()
        {
            string[] tab_strAbout =
            {
                "Copy the drawing shown on the right side",

                "",

                "Move using arrow keys or WASD",
                "Entering a new cell toggles its state",

                "",

                "Match the drawing exactly to win",

                "",

                "Press Home or End to generate a new drawing",
                "Press ESC to quit"
            };

            return tab_strAbout;
        }

        public override void Start()
        {
            bool blnRestart;

            do
            {
                blnRestart = false;

                InitializeGame();

                Arcade.ShowTitle(Name);

                Console.CursorVisible = false;

                do
                {
                    Console.SetCursorPosition(0, 5);

                    DrawGrid();

                    ConsoleKey movementKeyPressed = Console.ReadKey(true).Key;

                    ConsoleKey[] tab_MovementKeys =
                    {
                        ConsoleKey.UpArrow,
                        ConsoleKey.DownArrow,
                        ConsoleKey.LeftArrow,
                        ConsoleKey.RightArrow,

                        ConsoleKey.W,
                        ConsoleKey.A,
                        ConsoleKey.S,
                        ConsoleKey.D
                    };

                    if (tab_MovementKeys.Contains(movementKeyPressed))
                    {
                        MoveCursor(movementKeyPressed);
                    }
                    else if (movementKeyPressed == ConsoleKey.Spacebar)
                    {
                        ToggleCell();
                    }
                    else if (movementKeyPressed == ConsoleKey.R)
                    {
                        blnRestart = true;
                        break;
                    }
                    else if (movementKeyPressed == ConsoleKey.Escape)
                    {
                        return;
                    }

                } while (!DrawingsMatch());

                if (blnRestart)
                    continue;

                Arcade.ShowTitle(Name);
                DrawGrid();

                Console.WriteLine("\n   You matched the drawings!");

                Console.ReadKey(true);
                blnRestart = true;

            } while (blnRestart);
        }

        /// <summary>
        /// Reset the game state
        /// </summary>
        private static void InitializeGame()
        {
            Cursor = Origin;

            tab_blnCurrentDrawing = new bool[VAL_DRAWING_HEIGHT, VAL_DRAWING_WIDTH];

            do
            {
                tab_blnGoalDrawing = GenerateRandomDrawing();
            }
            while (DrawingsMatch());
        }
        private static void ToggleCell()
        {
            int x = Cursor.X;
            int y = Cursor.Y;

            tab_blnCurrentDrawing[x, y] = !tab_blnCurrentDrawing[x, y];
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        /// <summary>
        /// Move around the drawing grid
        /// </summary>
        private static void MoveCursor(ConsoleKey keyPressed)
        {
            switch (keyPressed)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    Cursor = (Cursor.X - 1, Cursor.Y);
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    Cursor = (Cursor.X + 1, Cursor.Y);
                    break;

                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    Cursor = (Cursor.X, Cursor.Y - 1);
                    break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    Cursor = (Cursor.X, Cursor.Y + 1);
                    break;
            }

            int x = Clamp(Cursor.X, 0, VAL_DRAWING_HEIGHT - 1);
            int y = Clamp(Cursor.Y, 0, VAL_DRAWING_WIDTH - 1);

            Cursor = (x, y);
        }
        /// <summary>
        /// Check if the drawings match
        /// </summary>
        private static bool DrawingsMatch()
        {
            for (byte bytRow = 0; bytRow < VAL_DRAWING_HEIGHT; bytRow++)
            {
                for (byte bytCol = 0; bytCol < VAL_DRAWING_WIDTH; bytCol++)
                {
                    if (tab_blnCurrentDrawing[bytRow, bytCol] != tab_blnGoalDrawing[bytRow, bytCol])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void DrawCell(bool blnFilled, int intRow, int intCol, bool blnSelected, bool blnCursorGrid)
        {
            bool isCheckerDark = (intRow + intCol) % 2 == 0;

            char chrToDraw;

            if (blnFilled)
            {
                chrToDraw = '█';
            }
            else
            {
                chrToDraw = isCheckerDark ? '░' : '▒';
            }

            ConsoleColor fg = ConsoleColor.Gray;
            ConsoleColor bg = ConsoleColor.Black;

            if (blnSelected && blnCursorGrid)
            {
                bg = ConsoleColor.DarkCyan;

                if (blnFilled)
                {
                    fg = ConsoleColor.White;
                }
                else
                {
                    fg = ConsoleColor.White;
                    chrToDraw = isCheckerDark ? '▒' : '░';
                }
            }

            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;

            Console.Write(chrToDraw);
            Console.ResetColor();
        }

        /// <summary>
        /// Draw both drawings and instructions
        /// </summary>
        private static void DrawGrid()
        {
            string strHorizontal = new string('═', VAL_DRAWING_WIDTH / 2);

            string[] tab_strInstructions =
            {
                "Instructions:",
                "Move with Arrow Keys or WASD",
                "Press Space or Enter to fill in a tile",
                "Match the right drawing",
                "Press R for a new drawing",
                "Press ESC to quit"
            };

            Console.WriteLine();

            Console.WriteLine("  ╔" + strHorizontal + "╬" + strHorizontal + "╗ " + "╔" + strHorizontal + "╬" + strHorizontal + "╗");

            for (byte bytRow = 0; bytRow < VAL_DRAWING_HEIGHT; bytRow++)
            {
                Console.Write(bytRow == VAL_DRAWING_HEIGHT / 2 ? "  ╬" : "  ║");

                for (byte bytCol = 0; bytCol < VAL_DRAWING_WIDTH; bytCol++)
                {
                    bool blnSelected = bytRow == Cursor.X && bytCol == Cursor.Y;
                    bool blnPainted = tab_blnCurrentDrawing[bytRow, bytCol];

                    DrawCell(blnPainted, bytRow, bytCol, blnSelected, true);
                }

                Console.Write(bytRow == VAL_DRAWING_HEIGHT / 2 ? "╬ ╬" : "║ ║");

                for (byte bytCol = 0; bytCol < VAL_DRAWING_WIDTH; bytCol++)
                {
                    bool blnGoalFilled = tab_blnGoalDrawing[bytRow, bytCol];

                    DrawCell(blnGoalFilled, bytRow, bytCol, false, false);
                }

                Console.Write(bytRow == VAL_DRAWING_HEIGHT / 2 ? "╬" : "║");

                if (bytRow < tab_strInstructions.Length)
                    Console.Write("\t" + tab_strInstructions[bytRow]);

                Console.WriteLine();
            }

            Console.WriteLine("  ╚" + strHorizontal + "╬" + strHorizontal + "╝ ╚" + strHorizontal + "╬" + strHorizontal + "╝");
        }

        /// <summary>
        /// Generate a random target drawing
        /// </summary>
        private static bool[,] GenerateRandomDrawing()
        {
            bool[,] tab_blnDrawing = new bool[VAL_DRAWING_HEIGHT, VAL_DRAWING_WIDTH];

            int intPoints = rnd.Next(3, 12);

            (int X, int Y) posA = Origin;

            for (int i = 0; i < intPoints; i++)
            {
                (int X, int Y) posB = (rnd.Next(VAL_DRAWING_HEIGHT), rnd.Next(VAL_DRAWING_WIDTH));

                DrawLine(tab_blnDrawing, posA, posB);

                tab_blnDrawing[posB.X, posB.Y] = false;

                posA = posB;
            }

            DrawLine(tab_blnDrawing, posA, Origin);

            return tab_blnDrawing;
        }

        /// <summary>
        /// Draw a line between two points
        /// </summary>
        private static void DrawLine(bool[,] tab_blnDrawing, (int X, int Y) posA, (int X, int Y) posB)
        {
            while (posA != posB)
            {
                if (Math.Abs(posA.X - posB.X) > Math.Abs(posA.Y - posB.Y))
                    posA.X = posA.X > posB.X ? posA.X - 1 : posA.X + 1;
                else
                    posA.Y = posA.Y > posB.Y ? posA.Y - 1 : posA.Y + 1;

                tab_blnDrawing[posA.X, posA.Y] = !tab_blnDrawing[posA.X, posA.Y];
            }
        }
    }
}