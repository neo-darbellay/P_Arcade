using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        public SimonButton(ConsoleColor activeColor, ConsoleColor inactiveColor, ConsoleKey[] keys)
        {
            ActiveColor = activeColor;
            InactiveColor = inactiveColor;
            Keys = keys;
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
        private byte _bytGameSpeed = 2;

        private readonly Random _rng = new Random();

        // The current sequence of button indices the player must repeat
        private List<byte> _lst_sequence;

        // The four Simon buttons
        private static readonly SimonButton[] Buttons = new SimonButton[]
        {
            new SimonButton(ConsoleColor.Green, ConsoleColor.DarkGreen, new ConsoleKey[] {ConsoleKey.W, ConsoleKey.UpArrow}),
            new SimonButton(ConsoleColor.Magenta, ConsoleColor.DarkMagenta, new ConsoleKey[] { ConsoleKey.D, ConsoleKey.RightArrow }),
            new SimonButton(ConsoleColor.Yellow, ConsoleColor.DarkYellow, new ConsoleKey[] { ConsoleKey.S, ConsoleKey.DownArrow }),
            new SimonButton(ConsoleColor.Blue, ConsoleColor.DarkBlue, new ConsoleKey[] { ConsoleKey.A, ConsoleKey.LeftArrow }),

            // This button is used to show whether or not the player got the answer right
            new SimonButton(ConsoleColor.Green, ConsoleColor.DarkGray, new ConsoleKey[] { ConsoleKey.A, ConsoleKey.LeftArrow }),
        };

        // Board layout
        private const int BOARD_X = 3;
        private const int BOARD_Y = 7;
        private const int BUTTON_WIDTH = 14;
        private const int BUTTON_HEIGHT = 6;
        private const int BUTTON_COLUMN_GAP = 2;
        private const int BUTTON_ROW_GAP = 2;
        private const int STATUS_ROW = BOARD_Y + (BUTTON_HEIGHT + BUTTON_ROW_GAP) * 3;

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

                "",

                "Controls",
                "  W / Up Arrow    - Top button",
                "  D / Right Arrow - Right button",
                "  S / Down Arrow  - Bottom button",
                "  A / Left Arrow  - Left button",

                "",

                "Press Q or Escape at any time to quit the current game."
            };

            return tab_strAbout;
        }

        public override  void Start()
        {
            CurrentScore = 0;
            _lst_sequence = new List<byte>();

            // Get user-related values
            GetUserInput();

            // Clear the screen and add the title back
            Console.Clear();
            Arcade.ShowTitle(Name);
            Console.CursorVisible = false;

            DrawBoard();
            SetStatus("Get ready...");
            Thread.Sleep(1000);

            MainLoop();
        }

        private void MainLoop()
        {
            bool blnContinue = true;

            while (blnContinue)
            {
                // Extend the sequence by one random step
                _lst_sequence.Add((byte)_rng.Next(4));

                SetStatus($"Round {_lst_sequence.Count} - Watch carefully!");
                Thread.Sleep(600);

                // Show the sequence
                PlaySequence();

                // Discard any keys pressed during playback
                while (Console.KeyAvailable)
                    Console.ReadKey(true);

                // Prompt the player
                SetStatus($"Your turn! Repeat the sequence ({_lst_sequence.Count} step{(_lst_sequence.Count != 1 ? "s" : "")}).");

                bool blnCorrect = GetPlayerInput(out bool blnQuit);

                if (blnQuit)
                    break;

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

            if (SupportsHighscore)
            {
                Console.Clear();
                Arcade.ShowTitle(Name);
                Console.CursorVisible = true;

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
        /// Draws all four Simon buttons in their inactive state, plus the control panel on the right
        /// </summary>
        private void DrawBoard()
        {
            for (byte i = 0; i < 5; i++)
                DrawButton(i, false);

            Console.ResetColor();
            Console.SetCursorPosition(BOARD_X, STATUS_ROW + 2);

            int intX = BOARD_X + (BUTTON_WIDTH + BUTTON_COLUMN_GAP) * 3 + 4;
            int intY = BOARD_Y;

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
        /// Returns the position of the given button ID's top-left corner, based on the board layout
        /// </summary>
        private static (int X, int Y) GetButtonOrigin(int intIndex)
        {
            int intCenterX = BOARD_X + BUTTON_WIDTH + BUTTON_COLUMN_GAP;
            int intCenterY = BOARD_Y + BUTTON_HEIGHT + BUTTON_ROW_GAP;

            switch (intIndex)
            {
                // Green
                case 0:
                    return (intCenterX, intCenterY - (BUTTON_HEIGHT + BUTTON_ROW_GAP));

                // Red
                case 1:
                    return (intCenterX + BUTTON_WIDTH + BUTTON_COLUMN_GAP, intCenterY);

                // Yellow
                case 2:
                    return (intCenterX, intCenterY + BUTTON_HEIGHT + BUTTON_ROW_GAP);

                // Blue
                case 3:
                    return (intCenterX - BUTTON_WIDTH - BUTTON_COLUMN_GAP, intCenterY);

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

            for (int intRow = 0; intRow < BUTTON_HEIGHT; intRow++)
            {
                Console.SetCursorPosition(intX, intY + intRow);
                Console.Write(new string(' ', BUTTON_WIDTH));
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Writes a status message on the line below the board
        /// </summary>
        private static void SetStatus(string strMessage)
        {
            Console.ResetColor();
            Console.SetCursorPosition(BOARD_X, STATUS_ROW);

            // Add some padding to the right to clear the console of leftover text from previous messages
            Console.Write(strMessage.PadRight(64));
        }

        /// <summary>
        /// Lights up each button in the current sequence in order, then returns
        /// </summary>
        private void PlaySequence()
        {
            // Scale flash duration to game speed
            int intFlashMs = Math.Max(100, 550 - (_bytGameSpeed - 1) * 150);
            int intGapMs = Math.Max(40, intFlashMs / 3);

            foreach (byte bytStep in _lst_sequence)
            {
                DrawButton(bytStep, true);
                Thread.Sleep(intFlashMs);
                DrawButton(bytStep, false);
                Thread.Sleep(intGapMs);
            }
        }

        /// <summary>
        /// Reads the player's button presses and validates them against the current sequence
        /// </summary>
        /// <param name="blnQuit">Set to true if the player pressed Q or Escape</param>
        /// <returns>true if the player matched the full sequence correctly</returns>
        private bool GetPlayerInput(out bool blnQuit)
        {
            blnQuit = false;

            for (int intSequenceIndex = 0; intSequenceIndex < _lst_sequence.Count; intSequenceIndex++)
            {
                // Wait for a valid key press
                while (true)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;

                    if (key == ConsoleKey.Q || key == ConsoleKey.Escape)
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
                    Thread.Sleep(150);
                    DrawButton(bytPressedIndex.Value, false);

                    if (bytPressedIndex.Value != _lst_sequence[intSequenceIndex])
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
            for (int t = 0; t < intFlashAmount; t++)
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

            InputService.GetInputInBoundaries(out _bytGameSpeed, VAL_MIN_SPEED, VAL_MAX_SPEED);
        }
    }
}