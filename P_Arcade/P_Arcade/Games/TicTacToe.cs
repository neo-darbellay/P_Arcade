using System;
using System.Linq;

namespace P_Arcade
{
    internal class TicTacToe : Game
    {
        // The current player (true = 1, false = 0)
        static bool blnCurrentPlayer = true;

        // The player's position
        static sbyte bytRow = 0;
        static sbyte bytCol = 0;

        static sbyte bytPrevRow = 0;
        static sbyte bytPrevCol = 0;

        public TicTacToe() : base("Tic Tac Toe", false) { }

        public override void Start()
        {
            // Create the grid
            byte[,] bytGrid = new byte[3, 3];

            bool blnWon = false;
            bool blnFull = false;

            byte bytWinner = 0;

            blnCurrentPlayer = true;
            bytRow = 0;
            bytCol = 0;

            // Clear the screen and add the title back
            Arcade.ShowTitle(Name);
            DrawGrid(bytGrid);

            // Start the game up
            Console.CursorVisible = false;
            do
            {
                MoveCursor(bytGrid);

                ConsoleKey keyPressed = Console.ReadKey(true).Key;

                ConsoleKey[] tab_MovementKeys =
                {
                    ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.W, ConsoleKey.A, ConsoleKey.S, ConsoleKey.D
                };

                // Check to see if the user pressed a valid key
                if (tab_MovementKeys.Contains(keyPressed))
                {
                    // Determine the movement direction
                    switch (keyPressed)
                    {
                        case ConsoleKey.LeftArrow: case ConsoleKey.A: bytCol -= 1; break;
                        case ConsoleKey.RightArrow: case ConsoleKey.D: bytCol += 1; break;
                        case ConsoleKey.UpArrow: case ConsoleKey.W: bytRow -= 1; break;
                        case ConsoleKey.DownArrow: case ConsoleKey.S: bytRow += 1; break;
                    }
                }
                else if (keyPressed == ConsoleKey.Enter || keyPressed == ConsoleKey.Spacebar)
                {
                    // Start the next player's turn if the move is valid
                    if (SendPiece(bytGrid))
                    {
                        blnCurrentPlayer = !blnCurrentPlayer;
                        DrawTurnText();
                    }
                }
                else if (keyPressed == ConsoleKey.Escape || keyPressed == ConsoleKey.R)
                    break;

                (blnWon, bytWinner) = CheckWin(bytGrid);
                blnFull = FullGrid(bytGrid);
            } while (!blnWon && !blnFull);

            // Place the cursor at the right spot
            Console.SetCursorPosition(0, 13);

            // If the user won, then show the victory message
            if (blnWon)
            {
                Console.Write($"\n   Player {bytWinner} won! Press any key to restart.");
                Console.ReadKey(true);
            }

            // If the grid is full and the player hasn't won, show the draw message
            if (!blnWon && blnFull)
            {
                Console.Write($"\n   It's a draw! Press any key to restart.");
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Move the cursor to the correct position
        /// </summary>
        private static void MoveCursor(byte[,] bytGrid)
        {
            // Keep the cursor inside bounds (0–2)
            if (bytRow < 0) bytRow = 0;
            if (bytRow > 2) bytRow = 2;
            if (bytCol < 0) bytCol = 0;
            if (bytCol > 2) bytCol = 2;

            byte bytPrevConsoleRow = (byte)(6 + (bytPrevRow * 2));
            byte bytPrevConsoleCol = (byte)(4 + (bytPrevCol * 4) + 1);

            byte bytPrevCel = bytGrid[bytPrevRow, bytPrevCol];

            Console.SetCursorPosition(bytPrevConsoleCol, bytPrevConsoleRow);

            if (bytPrevCel == 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write('X');
            }
            else if (bytPrevCel == 2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write('O');
            }
            else
            {
                Console.Write(' ');
            }

            Console.ResetColor();

            byte bytConsoleRow = (byte)(6 + (bytRow * 2));
            byte bytConsoleCol = (byte)(4 + (bytCol * 4) + 1);

            byte bytCurrentCel = bytGrid[bytRow, bytCol];

            Console.SetCursorPosition(bytConsoleCol, bytConsoleRow);
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(bytCurrentCel == 0 ? ' ' : bytCurrentCel == 1 ? 'X' : 'O');
            Console.ResetColor();

            bytPrevRow = bytRow;
            bytPrevCol = bytCol;
        }

        /// <summary>
        /// Put a piece down if nothing is there already
        /// </summary>
        /// <param name="bytGrid"></param>
        /// <returns></returns>
        private static bool SendPiece(byte[,] bytGrid)
        {
            if (bytGrid[bytRow, bytCol] == 0)
            {
                bytGrid[bytRow, bytCol] = (byte)(blnCurrentPlayer ? 1 : 2);

                byte consoleRow = (byte)(6 + (bytRow * 2));
                byte consoleCol = (byte)(4 + (bytCol * 4) + 1);

                Console.SetCursorPosition(consoleCol, consoleRow);
                Console.ForegroundColor = blnCurrentPlayer ? ConsoleColor.Red : ConsoleColor.Yellow;
                Console.Write(blnCurrentPlayer ? 'X' : 'O');

                Console.ResetColor();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Draw the Player's turn text
        /// </summary>
        private static void DrawTurnText()
        {
            Console.SetCursorPosition(24, 6);
            Console.Write("Player ");
            Console.ForegroundColor = blnCurrentPlayer ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.Write(blnCurrentPlayer ? 1 : 2);
            Console.ResetColor();
            Console.Write("'s turn");
        }

        /// <summary>
        /// Checks whether or not the current grid is full
        /// </summary>
        /// <param name="bytGrid"></param>
        /// <returns></returns>
        private static bool FullGrid(byte[,] bytGrid)
        {
            for (byte x = 0; x < bytGrid.GetLength(0); x++)
                for (byte y = 0; y < bytGrid.GetLength(1); y++)
                    if (bytGrid[x, y] == 0)
                        return false;
            return true;
        }

        /// <summary>
        /// Check whether the user has won (if there is 3 of a character in a row)
        /// </summary>
        /// <param name="bytGrid"></param>
        /// <returns></returns>
        private static (bool blnWon, byte bytWinner) CheckWin(byte[,] bytGrid)
        {
            byte bytRepeated;

            // Check rows
            for (int row = 0; row < 3; row++)
            {
                bytRepeated = bytGrid[row, 0];

                if (bytRepeated != 0 &&
                    bytRepeated == bytGrid[row, 1] &&
                    bytRepeated == bytGrid[row, 2])
                {
                    return (true, bytRepeated);
                }
            }

            // Check columns
            for (int col = 0; col < 3; col++)
            {
                bytRepeated = bytGrid[0, col];

                if (bytRepeated != 0 &&
                    bytRepeated == bytGrid[1, col] &&
                    bytRepeated == bytGrid[2, col])
                {
                    return (true, bytRepeated);
                }
            }

            // Check diagonal (top-left to bottom-right)
            bytRepeated = bytGrid[0, 0];
            if (bytRepeated != 0 &&
                bytRepeated == bytGrid[1, 1] &&
                bytRepeated == bytGrid[2, 2])
            {
                return (true, bytRepeated);
            }

            // Check diagonal (top-right to bottom-left)
            bytRepeated = bytGrid[0, 2];
            if (bytRepeated != 0 &&
                bytRepeated == bytGrid[1, 1] &&
                bytRepeated == bytGrid[2, 0])
            {
                return (true, bytRepeated);
            }

            return (false, 0);
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
                "Player " + (blnCurrentPlayer ? 1 : 2) + "'s turn" ,
                "Use arrow keys to move",
                "Press enter or space to send your " + (blnCurrentPlayer ? 'X' : 'O') + " symbol",
                "Press ESC to quit the game",
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
                        Console.Write("   ║");
                    }
                    else
                    {
                        Console.Write(" " + (bytValue == 1 ? 'X' : 'O') + " ║");
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
    }
}
