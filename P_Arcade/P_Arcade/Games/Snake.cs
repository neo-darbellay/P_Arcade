using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace P_Arcade
{
    /// <summary>
    /// A part of the snake
    /// </summary>
    public class SnakePart
    {
        /// <summary>
        /// This part's position
        /// </summary>
        public (byte X, byte Y) Position { get; set; }

        public SnakePart((byte X, byte Y) position) { Position = position; }
    }

    /// <summary>
    /// The snake
    /// </summary>
    public class Snake
    {
        /// <summary>
        /// An Enum used for the snake's next direction
        /// </summary>
        public enum Direction { Up, Down, Left, Right }

        /// <summary>
        /// The snake's current direction
        /// </summary>
        public Direction? currentDirection = null;

        /// <summary>
        /// The snake's previous direction
        /// </summary>
        private Direction previousDirection = Direction.Up;

        /// <summary>
        /// The ASCII character that the snake will use as a head
        /// </summary>
        public char HeadSymbol { get; set; }

        /// <summary>
        /// The ASCII character that the snake will use as a body
        /// </summary>
        public char BodySymbol { get; set; }

        /// <summary>
        /// An array of snake parts, used as the snake's body
        /// </summary>
        private readonly List<SnakePart> _body;

        /// <summary>
        /// The snake's head
        /// </summary>
        private SnakePart Head => _body.First();

        private readonly ConsoleColor PrimaryColor = ConsoleColor.DarkGreen;
        private readonly ConsoleColor SecondaryColor = ConsoleColor.Green;

        public Snake((byte, byte) startingPoint)
        {
            HeadSymbol = '█';
            BodySymbol = '█';

            _body = new List<SnakePart> { new SnakePart(startingPoint), new SnakePart(startingPoint) };

            ClearFromGrid();
            WriteToGrid();
        }

        /// <summary>
        /// Clear the snake from the logic grid
        /// This method should be called before doing any snake movement
        /// </summary>
        private void ClearFromGrid()
        {
            foreach (SnakePart part in _body)
            {
                SnakeGame.GameGrid[part.Position.Y, part.Position.X] = 0;
            }
        }

        /// <summary>
        /// Add the snake to the logic grid
        /// </summary>
        private void WriteToGrid()
        {
            // Body
            foreach (SnakePart part in _body.Skip(1))
            {
                SnakeGame.GameGrid[part.Position.Y, part.Position.X] = 2;
            }

            SnakeGame.GameGrid[Head.Position.Y, Head.Position.X] = 1;
        }

        /// <summary>
        /// Draw the snake
        /// </summary>
        public void Draw()
        {
            foreach (SnakePart snakePart in _body)
            {
                if (snakePart == Head)
                    continue;

                SnakeGame.DrawTile(snakePart.Position.X, snakePart.Position.Y, BodySymbol, SecondaryColor, false);
            }

            SnakeGame.DrawTile(Head.Position.X, Head.Position.Y, HeadSymbol, PrimaryColor, false);
        }

        /// <summary>
        /// Move the snake
        /// </summary>
        /// <param name="direction">Which direction to go in</param>
        /// <returns>true if the movement worked, false if the movement failed, meaning the snake died</returns>
        public bool Move(Direction direction, out bool blnAteApple)
        {
            blnAteApple = false;

            int intNewX = Head.Position.X;
            int intNewY = Head.Position.Y;

            switch (direction)
            {
                case Direction.Up: intNewY--; break;
                case Direction.Down: intNewY++; break;
                case Direction.Left: intNewX--; break;
                case Direction.Right: intNewX++; break;
            }

            int intMaxGridX = SnakeGame.GameGrid.GetLength(1);
            int intMaxGridY = SnakeGame.GameGrid.GetLength(0);

            // Check for border collisions
            bool blnBorderCollision = intNewX < 0 || intNewY < 0 || intNewX >= intMaxGridX || intNewY >= intMaxGridY;

            if (blnBorderCollision)
            {
                if (SnakeGame.DoesBorderKill)
                {
                    return false;
                }
                else
                {
                    // Teleport the snake to the other side
                    if (intNewX < 0) intNewX = (byte)(intMaxGridX - 1);
                    else if (intNewX >= intMaxGridX) intNewX = 0;

                    if (intNewY < 0) intNewY = (byte)(intMaxGridY - 1);
                    else if (intNewY >= intMaxGridY) intNewY = 0;
                }
            }

            (byte X, byte Y) newPos = ((byte)intNewX, (byte)intNewY);

            // If there is a self collision with the first body part (other than the head), move using the previous position
            if (_body.Count >= 2 && _body[1].Position == newPos)
                return Move(previousDirection, out blnAteApple);

            // Check for self collisions
            bool blnSelfCollision = _body.Any(p => p.Position.Equals(newPos));

            if (blnSelfCollision)
            {
                return false;
            }

            blnAteApple = SnakeGame.GameGrid[newPos.Y, newPos.X] == 255;

            ClearFromGrid();

            // Insert new head
            _body.Insert(0, new SnakePart(newPos));

            if (!blnAteApple)
            {
                // Remove tail ONLY if no apple was eaten
                SnakePart tail = _body.Last();
                SnakeGame.DrawTile(tail.Position.X, tail.Position.Y, BodySymbol, ConsoleColor.Black, true);
                _body.RemoveAt(_body.Count - 1);
            }
            else
            {
                SnakeGame.GenerateApple();
            }

            WriteToGrid();
            Draw();

            previousDirection = direction;

            return true;
        }
    }

    /// <summary>
    /// The Snake game, ported to C# console
    /// </summary>
    internal class SnakeGame : Game
    {
        /// <summary>
        /// The Snake game's constructor
        /// </summary>
        public SnakeGame() : base("Snake", true) { }

        // Constants used for min/max of length/width/apples/speed
        const byte VAL_MIN_LENGTH = 6 * 2;
        const byte VAL_MIN_WIDTH = 6 * 4;
        const byte VAL_MIN_APPLES = 1;
        const byte VAL_MIN_SPEED = 1;
        const byte VAL_MAX_LENGTH = 25 * 2;
        const byte VAL_MAX_WIDTH = 25 * 4;
        const byte VAL_MAX_APPLES = 10;
        const byte VAL_MAX_SPEED = 4;

        // User input for length and width
        static byte bytLength = 0;
        static byte bytWidth = 0;

        // The first tile's X and Y position
        const byte FIRST_TILE_X = 4;
        const byte FIRST_TILE_Y = 6;

        // How many apples need to be on screen at once
        public static byte TotalApples;

        // Whether or not the borders kill you
        public static bool DoesBorderKill;

        // The game's current speed
        public static byte bytGameSpeed;

        // The game's grid
        public static byte[,] GameGrid;

        static readonly Random rng = new Random();

        private static readonly ConsoleColor PrimaryBackgroundColor = ConsoleColor.Gray;
        private static readonly ConsoleColor SecondaryBackgroundColor = ConsoleColor.White;


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public override void Start()
        {
            // Full screen the app
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, 3);

            CurrentScore = 0;

            // Get user-related values
            GetUserInput();

            // Clear the screen and add the title back
            Arcade.ShowTitle(Name);

            Console.CursorVisible = false;

            // Draw the game board
            for (int x = 0; x <= bytLength + 1; x++)
            {
                for (int y = 0; y <= bytWidth + 1; y++)
                {
                    // Top row
                    if (x == 0)
                    {
                        if (y == 0)
                            Console.Write("   ╔");

                        else if (y == bytWidth + 1)
                            Console.Write("╗");
                        else
                            Console.Write("═");
                    }
                    // Bottom row
                    else if (x == bytLength + 1)
                    {
                        if (y == 0)
                            Console.Write("   ╚");
                        else if (y == bytWidth + 1)
                            Console.Write("╝");
                        else
                            Console.Write("═");
                    }
                    // Middle rows
                    else
                    {
                        if (y == 0)
                            Console.Write("   ");

                        if (y == 0 || y == bytWidth + 1)
                            Console.Write("║");
                        else
                        {
                            Console.ResetColor();
                            int gridX = x - 1; // subtract left border
                            int gridY = y - 1; // subtract top border

                            Console.BackgroundColor = ((gridX / 2) + (gridY / 2)) % 2 == 0 ? PrimaryBackgroundColor : SecondaryBackgroundColor;
                            Console.Write(" ");
                        }
                    }

                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            // Create the game grid
            GameGrid = new byte[bytLength, bytWidth];

            for (int i = 0; i < TotalApples; i++)
            {
                GenerateApple();
            }

            // Create the player and put him in the middle
            Snake player = new Snake(((byte)(bytWidth / 2), (byte)(bytLength / 2)));
            player.Draw();

            // Create a stopwatch for time-based movement
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Handle player movement
            bool blnContinue = true;
            while (blnContinue)
            {
                // Handle input
                if (Console.KeyAvailable)
                {
                    ConsoleKey playerInputKey = Console.ReadKey(true).Key;

                    switch (playerInputKey)
                    {
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            if (player.currentDirection != Snake.Direction.Down)
                                player.currentDirection = Snake.Direction.Up;
                            break;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            if (player.currentDirection != Snake.Direction.Up)
                                player.currentDirection = Snake.Direction.Down;
                            break;
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            if (player.currentDirection != Snake.Direction.Right)
                                player.currentDirection = Snake.Direction.Left;
                            break;
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            if (player.currentDirection != Snake.Direction.Left)
                                player.currentDirection = Snake.Direction.Right;
                            break;
                        case ConsoleKey.Q:
                        case ConsoleKey.Escape:
                            blnContinue = false;
                            break;
                    }
                }

                // Move snake at fixed interval
                byte bytAmountOfWait = 100;

                if (player.currentDirection == Snake.Direction.Up || player.currentDirection == Snake.Direction.Down)
                    bytAmountOfWait += bytAmountOfWait;

                if (player.currentDirection.HasValue && stopwatch.ElapsedMilliseconds >= (bytAmountOfWait / bytGameSpeed))
                {
                    blnContinue = player.Move(player.currentDirection.Value, out bool blnAteApple);
                    if (blnAteApple) CurrentScore++;
                    stopwatch.Restart();
                }
            }

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
        /// Getting the user input
        /// </summary>
        private void GetUserInput()
        {
            Arcade.ShowTitle(Name);

            // Ask the user for the number of rows they want
            Console.Write("   Please enter the length of the area that you want.\n   The value needs to be greater than ");

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
            Console.Write("\n   Please enter the width of the area that you want.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_WIDTH);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_WIDTH);
            Console.ResetColor();

            // Get the correct input
            GetInputInBounds(out bytWidth, VAL_MIN_WIDTH, VAL_MAX_WIDTH);


            // Ask the user for the number of apples they want
            Console.Write("\n   Please enter the amount of apples that you want on screen at once.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_APPLES);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_APPLES);
            Console.ResetColor();

            // Get the correct input
            GetInputInBounds(out TotalApples, VAL_MIN_APPLES, VAL_MAX_APPLES);


            // Ask the user for the game's speed
            Console.Write("\n   Please enter how fast you want the game to be.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_SPEED);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_SPEED);
            Console.ResetColor();

            // Get the correct input
            GetInputInBounds(out bytGameSpeed, VAL_MIN_SPEED, VAL_MAX_SPEED);


            // Ask the user whether or not they want the borders to kill the snake or not
            Console.Write("\n   Should the grid borders kill the snake, or teleport it to the other side?\n   Write ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(0);
            Console.ResetColor();

            Console.Write(" if it should kill, and ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(1);
            Console.ResetColor();

            Console.WriteLine(" if it should teleport the snake to the other side");

            GetInputInBounds(out byte bytBorderKills, 0, 1);

            DoesBorderKill = bytBorderKills == 0;
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

        /// <summary>
        /// Draws a sprite at the given x and y coordinate
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="chrSprite">The char to draw</param>
        /// <param name="ccrSpriteColor">The ConsoleColor of the sprite</param>
        /// <param name="blnErase">Whether or not it should draw or erase at the given position</param>
        public static void DrawTile(int left, int top, char chrSprite, ConsoleColor ccrSpriteColor, bool blnErase)
        {
            (int X, int Y) intStart = (FIRST_TILE_X + left, FIRST_TILE_Y + top);

            Console.BackgroundColor = ((intStart.X / 2) + (intStart.Y / 2)) % 2 != 0 ? PrimaryBackgroundColor : SecondaryBackgroundColor;
            Console.ForegroundColor = ccrSpriteColor;

            Console.SetCursorPosition(intStart.X, intStart.Y);
            Console.Write(blnErase ? ' ' : chrSprite);

            Console.ResetColor();
        }

        /// <summary>
        /// Generates an apple at a random place on the map
        /// </summary>
        public static bool GenerateApple()
        {
            List<(int x, int y)> emptyCells = new List<(int x, int y)>();

            // Collect all the empty cells
            for (int y = 0; y < bytLength; y++)
            {
                for (int x = 0; x < bytWidth; x++)
                {
                    if (GameGrid[y, x] == 0)
                    {
                        emptyCells.Add((x, y));
                    }
                }
            }

            // If there are no empty cells, we can't place an apple
            if (emptyCells.Count == 0)
                return false;

            // Pick a random empty cell
            (int intAppleX, int intAppleY) = emptyCells[rng.Next(emptyCells.Count)];

            // Place the apple on the grid
            GameGrid[intAppleY, intAppleX] = 255;

            // Draw it
            DrawTile(intAppleX, intAppleY, '█', ConsoleColor.DarkRed, false);

            return true;
        }
    }
}