using System;
using System.Collections.Generic;
using System.Threading;

using P_Arcade.Models;
using P_Arcade.Services;

namespace P_Arcade.Games
{
    /// <summary>
    /// One of the four colored panels on the Simon board
    /// </summary>
    public class SimonButton
    {
        /// <summary>
        /// The color of the button when it is lit
        /// </summary>
        public ConsoleColor ActiveColor { get; }

        /// <summary>
        /// The color of the button when it is unlit
        /// </summary>
        public ConsoleColor InactiveColor { get; }

        /// <summary>
        /// The keys the player presses to activate this button
        /// </summary>
        public ConsoleKey[] Keys { get; }

        /// <summary>
        /// The button's beep frequency, used for Console.Beep()
        /// </summary>
        public int BeepFrequency { get; }

        public SimonButton(ConsoleColor activeColor, ConsoleColor inactiveColor, ConsoleKey[] keys, int intBeepFrequency)
        {
            ActiveColor = activeColor;
            InactiveColor = inactiveColor;
            Keys = keys;
            BeepFrequency = intBeepFrequency;
        }
    }

    /// <summary>
    /// The Simon Says memory game, ported to C# console
    /// </summary>
    internal class SimonGame : Game
    {
        /// <summary>
        /// The Simon Game's constructor
        /// </summary>
        public SimonGame() : base("Simon", true) { }

        // Constants used for min/max speed
        const byte VAL_MIN_SPEED = 1;
        const byte VAL_MAX_SPEED = 4;

        // The game's current speed
        private byte bytGameSpeed = 2;

        // Whether or not the user wanted to exit the game
        static bool blnExitRequested;

        private readonly Random rng = new Random();

        // The current sequence of button indices the player must repeat
        private List<byte> lst_bytSequence;

        // The Simon buttons
        private static readonly SimonButton[] Buttons = new SimonButton[]
        {
            new SimonButton(ConsoleColor.Green,     ConsoleColor.DarkGreen,     new ConsoleKey[] { ConsoleKey.W, ConsoleKey.UpArrow     }, 415),
            new SimonButton(ConsoleColor.Magenta,   ConsoleColor.DarkMagenta,   new ConsoleKey[] { ConsoleKey.D, ConsoleKey.RightArrow  }, 310),
            new SimonButton(ConsoleColor.Yellow,    ConsoleColor.DarkYellow,    new ConsoleKey[] { ConsoleKey.S, ConsoleKey.DownArrow   }, 252),
            new SimonButton(ConsoleColor.Blue,      ConsoleColor.DarkBlue,      new ConsoleKey[] { ConsoleKey.A, ConsoleKey.LeftArrow   }, 209),

            // This button is used to show whether or not the player got the answer right
            new SimonButton(ConsoleColor.Green,     ConsoleColor.DarkGray,      new ConsoleKey[] { ConsoleKey.A, ConsoleKey.LeftArrow   }, 0),
        };

        // Board layout
        private const int VAL_BOARD_X = 3;
        private const int VAL_BOARD_Y = 7;
        private const int VAL_BUTTON_WIDTH = 14;
        private const int VAL_BUTTON_HEIGHT = 6;
        private const int VAL_BUTTON_COLUMN_GAP = 2;
        private const int VAL_BUTTON_ROW_GAP = 2;
        private const int VAL_STATUS_ROW = VAL_BOARD_Y + (VAL_BUTTON_HEIGHT + VAL_BUTTON_ROW_GAP) * 3;

        public override string[] About()
        {
            string[] tab_strAbout = new string[]
            {
                "Simon is a classic memory game that tests your memory and concentration",
                "Watch the sequence of flashing buttons and repeat it exactly",

                "",

                "Each round adds one new step to the sequence",
                "The sequence becomes longer and harder to remember as you progress",

                "",

                "Use W, A, S and D or the Arrow Keys to repeat the pattern",
                "Pressing a wrong button ends the game immediately",

                "",

                "Your score equals the number of sequences you successfully complete",
                "Higher game speeds make the flashes appear more quickly",
            };

            return tab_strAbout;
        }

        public override  void Start()
        {
            blnExitRequested = false;
            CurrentScore = 0;
            lst_bytSequence = new List<byte>();

            // Get user-related values
            GetUserInput();

            if (blnExitRequested) return;

            // Clear the screen and add the title back
            Console.Clear();
            Arcade.ShowTitle(Name);
            Console.CursorVisible = false;

            DrawBoard();
            SetStatus("Get ready...");
            Thread.Sleep(1000);

            bool blnContinue = true;

            do
            {
                // Extend the sequence by one random step
                lst_bytSequence.Add((byte)rng.Next(4));

                SetStatus($"Round {lst_bytSequence.Count} - Watch carefully!");
                Thread.Sleep(600);

                // Show the sequence
                PlaySequence();

                // Discard any keys pressed during playback
                while (Console.KeyAvailable)
                    Console.ReadKey(true);

                // Prompt the player
                SetStatus($"Your turn! Repeat the sequence ({lst_bytSequence.Count} step{(lst_bytSequence.Count != 1 ? "s" : "")}).");

                bool blnCorrect = GetPlayerInput(out bool blnQuit);

                if (blnQuit)
                    return;

                if (!blnCorrect)
                {
                    FlashButton(ConsoleColor.DarkRed, 3);
                    SetStatus("Wrong! Game over.");
                    Thread.Sleep(1500);

                    blnContinue = false;
                }
                else
                {
                    CurrentScore++;

                    SetStatus($"Correct!  Score: {CurrentScore}");
                    FlashButton(ConsoleColor.Green, 1);
                    Thread.Sleep(600);
                }
            }
            while (blnContinue);

            if (SupportsHighscore)
            {
                Console.Clear();
                Arcade.ShowTitle(Name);
                Console.CursorVisible = true;

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
        /// Draws all four Simon buttons in their inactive state, plus the control panel on the right
        /// </summary>
        private void DrawBoard()
        {
            for (byte i = 0; i < 5; i++)
                DrawButton(i, false);

            Console.ResetColor();
            Console.SetCursorPosition(VAL_BOARD_X, VAL_STATUS_ROW + 2);

            int intX = VAL_BOARD_X + (VAL_BUTTON_WIDTH + VAL_BUTTON_COLUMN_GAP) * 3 + 4;
            int intY = VAL_BOARD_Y;

            Console.SetCursorPosition(intX, intY);
            Console.Write("   Controls");
            Console.SetCursorPosition(intX, intY + 1);
            Console.Write("   --------");

            Console.SetCursorPosition(intX, intY + 3);
            Console.Write("   Move:");
            Console.SetCursorPosition(intX, intY + 4);
            Console.Write("   WASD / Arrow keys");

            Console.SetCursorPosition(intX, intY + 6);
            Console.Write("   Repeat the sequence");

            Console.SetCursorPosition(intX, intY + 8);
            Console.Write("   Quit: Q / Escape");
        }

        /// <summary>
        /// Returns the position of the button with the given ID's top-left corner, based on the board layout
        /// </summary>
        private static (int X, int Y) GetButtonOrigin(int intIndex)
        {
            int intCenterX = VAL_BOARD_X + VAL_BUTTON_WIDTH + VAL_BUTTON_COLUMN_GAP;
            int intCenterY = VAL_BOARD_Y + VAL_BUTTON_HEIGHT + VAL_BUTTON_ROW_GAP;

            switch (intIndex)
            {
                // Green
                case 0:
                    return (intCenterX, intCenterY - (VAL_BUTTON_HEIGHT + VAL_BUTTON_ROW_GAP));

                // Red
                case 1:
                    return (intCenterX + VAL_BUTTON_WIDTH + VAL_BUTTON_COLUMN_GAP, intCenterY);

                // Yellow
                case 2:
                    return (intCenterX, intCenterY + VAL_BUTTON_HEIGHT + VAL_BUTTON_ROW_GAP);

                // Blue
                case 3:
                    return (intCenterX - VAL_BUTTON_WIDTH - VAL_BUTTON_COLUMN_GAP, intCenterY);

                // Indicator to show whether the player got the answer right or wrong
                case 4:
                    return (intCenterX, intCenterY);

                default:
                    return (intCenterX, intCenterY);
            }
        }

        /// <summary>
        /// Draws a single Simon button
        /// </summary>
        /// <param name="bytIndex">Index of the button (0-3)</param>
        /// <param name="blnActive">Whether the button should appear lit up</param>
        /// <param name="colorOverride">Optional color that overrides the button's own colors</param>
        private static void DrawButton(byte bytIndex, bool blnActive, ConsoleColor? colorOverride = null)
        {
            SimonButton button = Buttons[bytIndex];

            ConsoleColor backgroundColor = colorOverride ?? (blnActive ? button.ActiveColor : button.InactiveColor);
            ConsoleColor foregroundColor = blnActive ? ConsoleColor.White : backgroundColor;

            (int intX, int intY) = GetButtonOrigin(bytIndex);

            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;

            for (int intRow = 0; intRow < VAL_BUTTON_HEIGHT; intRow++)
            {
                Console.SetCursorPosition(intX, intY + intRow);
                Console.Write(new string(' ', VAL_BUTTON_WIDTH));
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Writes a status message on the line below the board
        /// </summary>
        private static void SetStatus(string strMessage)
        {
            Console.ResetColor();
            Console.SetCursorPosition(VAL_BOARD_X, VAL_STATUS_ROW);

            // Add some padding to the right to clear the console of leftover text from previous messages
            Console.Write(strMessage.PadRight(64));
        }

        /// <summary>
        /// Lights up each button in the current sequence in order, then returns
        /// </summary>
        private void PlaySequence()
        {
            // Scale flash duration to game speed
            int intFlashMs = Math.Max(100, 550 - (bytGameSpeed - 1) * 150);
            int intGapMs = Math.Max(40, intFlashMs / 3);

            foreach (byte bytStep in lst_bytSequence)
            {
                DrawButton(bytStep, true);
                Console.Beep(Buttons[bytStep].BeepFrequency, intFlashMs);
                DrawButton(bytStep, false);
                Thread.Sleep(intGapMs);
            }
        }

        /// <summary>
        /// Reads the player's button presses and validates them against the current sequence
        /// </summary>
        /// <param name="blnQuit">Set to true if the player pressed Escape</param>
        /// <returns>true if the player matched the full sequence correctly</returns>
        private bool GetPlayerInput(out bool blnQuit)
        {
            blnQuit = false;

            for (int intSequenceIndex = 0; intSequenceIndex < lst_bytSequence.Count; intSequenceIndex++)
            {
                // Wait for a valid key press
                while (true)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;

                    if (key == ConsoleKey.Escape)
                    {
                        blnQuit = true;
                        return false;
                    }

                    // Map the key to a button index
                    byte? bytPressedIndex = null;
                    for (byte bytIndex = 0; bytIndex < Buttons.Length; bytIndex++)
                    {
                        if (Array.IndexOf(Buttons[bytIndex].Keys, key) >= 0)
                        {
                            bytPressedIndex = bytIndex;
                            break;
                        }
                    }

                    // Ignore unrelated keys
                    if (bytPressedIndex == null)
                        continue;

                    // Flash the pressed button
                    DrawButton(bytPressedIndex.Value, true);
                    Console.Beep(Buttons[bytPressedIndex.Value].BeepFrequency, 150);
                    DrawButton(bytPressedIndex.Value, false);

                    if (bytPressedIndex.Value != lst_bytSequence[intSequenceIndex])
                        return false;

                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// Flashes all four buttons a given color a set number of times
        /// </summary>
        /// <param name="color">The color to flash</param>
        /// <param name="intFlashAmount">How many flashes</param>
        private static void FlashButton(ConsoleColor color, int intFlashAmount)
        {
            for (int intIteration = 0; intIteration < intFlashAmount; intIteration++)
            {
                DrawButton(4, true, color);

                Thread.Sleep(200);

                DrawButton(4, false);

                Thread.Sleep(150);
            }
        }

        /// <summary>
        /// Prompts the player for the game speed
        /// </summary>
        private void GetUserInput()
        {
            Arcade.ShowTitle(Name);

            Console.Write("\n   Please enter how fast you want the game to be.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_SPEED);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_SPEED);
            Console.ResetColor();

            blnExitRequested = !InputService.GetInputInBoundaries(out bytGameSpeed, VAL_MIN_SPEED, VAL_MAX_SPEED);
        }
    }
}