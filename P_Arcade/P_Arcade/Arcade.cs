using P_Arcade.Games;
using P_Arcade.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace P_Arcade
{
    internal class Arcade
    {
        /*----------------------------------------------------------------------------

                  /$$$$$$                                      /$$          
                 /$$__  $$                                    | $$          
                | $$  \ $$  /$$$$$$   /$$$$$$$  /$$$$$$   /$$$$$$$  /$$$$$$ 
                | $$$$$$$$ /$$__  $$ /$$_____/ |____  $$ /$$__  $$ /$$__  $$
                | $$__  $$| $$  \__/| $$        /$$$$$$$| $$  | $$| $$$$$$$$
                | $$  | $$| $$      | $$       /$$__  $$| $$  | $$| $$_____/
                | $$  | $$| $$      |  $$$$$$$|  $$$$$$$|  $$$$$$$|  $$$$$$$
                |__/  |__/|__/       \_______/ \_______/ \_______/ \_______/

        ----------------------------------------------------------------------------*/

        static List<Game> AvailableGames;


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main()
        {
            AvailableGames = new List<Game>
            {
                // Initialize games
                new Connect4(),
                new CopyADrawing(),
                new MineSweeper(),
                new SimonGame(),
                new SlidingPuzzle(),
                new SnakeGame(),
                new TicTacToe(),
                new Yahtzee()
            };

            // Set up their scores and a GameNames table
            List<string> GameNames = new List<string>();

            foreach (Game game in AvailableGames)
            {
                GameNames.Add((AvailableGames.IndexOf(game) + 1) + ". " + game.Name);

                if (!game.SupportsHighscore) continue;
            }

            // Add one option, which is to exit the program
            GameNames.Add($"{GameNames.Count + 1}. Exit");

            // Show every available games and handle the user's choice
            do
            {
                int intUserChoice = DisplaySelectMenu("P_Arcade", "Select the game you want to play, or exit the app", GameNames, 5);

                if (intUserChoice == GameNames.Count)
                {
                    Console.WriteLine("\n\n");
                    Environment.Exit(418);
                }
                else if (intUserChoice < GameNames.Count && intUserChoice > 0)
                {
                    ShowGameOptions(AvailableGames[intUserChoice - 1]);
                }
            } while (true);
        }

        /// <summary>
        /// Shows a title. Either a game's, or the program's
        /// </summary>
        /// <param name="title">The current title</param>
        public static void ShowTitle(string title)
        {
            // Full screen the app just in case it got resized
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, 3);

            Console.Clear();
            Console.ResetColor();

            string byLine = "By Néo Darbellay";

            // Determine the inner width of the box
            int innerWidth = Math.Max(title.Length, byLine.Length) + 4; // padding

            string top = $"╔════{new string('═', innerWidth)}════╗";
            string bottom = $"╚════{new string('═', innerWidth)}════╝";

            Console.WriteLine("   " + top);
            Console.WriteLine("   ║    " + CenterText(title, innerWidth) + "    ║");
            Console.WriteLine("   ║    " + CenterText(byLine, innerWidth) + "    ║");
            Console.WriteLine("   " + bottom + "\n");
        }

        /// <summary>
        /// Helper function to center text with a specific width
        /// </summary>
        /// <param name="text">The text to center</param>
        /// <param name="width">The total width</param>
        /// <returns></returns>
        static string CenterText(string text, int width)
        {
            int padding = width - text.Length;
            int padLeft = padding / 2;
            int padRight = padding - padLeft;

            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        /// <summary>
        /// Diplays an interactive menu
        /// </summary>
        /// <param name="strTitle">The current title</param>
        /// <param name="strSubTitle">The subtitle to appear above the list of choices</param>
        /// <param name="lst_strChoices">A list of strings that are the choices names</param>
        /// <param name="intTopLine">The line used to start the list</param>
        /// <returns>A number corresponding to the user's choice</returns>
        static int DisplaySelectMenu(string strTitle, string strSubTitle, List<string> lst_strChoices, int intTopLine)
        {
            ShowTitle(strTitle);
            int userChoice = 1;
            ConsoleKeyInfo userKey;
            Console.CursorVisible = false;
            Console.CursorTop = intTopLine;

            Console.WriteLine("   " + strSubTitle);
            for (int i = 0; i < lst_strChoices.Count; i++)
                Console.WriteLine("      " + lst_strChoices[i].PadRight(64));

            do
            {
                // Points the current choice with an arrow
                Console.SetCursorPosition(3, intTopLine + userChoice);
                Console.Write("->");

                // Waits for the user to press a key
                userKey = Console.ReadKey(true);

                // Deletes the previous arrow
                Console.SetCursorPosition(3, intTopLine + userChoice);
                Console.Write("  ");

                if (userKey.Key == ConsoleKey.DownArrow)
                {
                    userChoice++;
                    // If exceed the number of choices, reset to the min choice
                    if (userChoice > lst_strChoices.Count)
                        userChoice = 1;
                }
                else if (userKey.Key == ConsoleKey.UpArrow)
                {
                    userChoice--;
                    // If is less than 1, set the choosed option to the last option
                    if (userChoice < 1)
                        userChoice = lst_strChoices.Count;
                }
                else if (char.IsDigit(userKey.KeyChar))
                {
                    int intChoice = userKey.KeyChar - '0';

                    if (intChoice >= 1 && intChoice <= lst_strChoices.Count)
                    {
                        return intChoice;
                    }
                }

            } while (userKey.Key != ConsoleKey.Enter && userKey.Key != ConsoleKey.Spacebar);

            return userChoice;
        }

        /// <summary>
        /// Displays the options (play, about, high scores and exit) of the current game
        /// </summary>
        /// <param name="game">The game to show the options in</param>
        static bool ShowGameOptions(Game game)
        {
            int intIndex = 1;

            List<string> lst_strOptions = new List<string>
            {
                $"{intIndex++}. Start"
            };

            string[] tab_strAbout = game.About();

            bool blnHasAbout = tab_strAbout.Any(strAbout => !string.IsNullOrWhiteSpace(strAbout));
            bool blnHasHighscore = game.SupportsHighscore;

            int intAboutOption = -1;
            int intHighscoreOption = -1;

            if (blnHasAbout)
            {
                intAboutOption = intIndex;
                lst_strOptions.Add($"{intIndex++}. About");
            }

            if (blnHasHighscore)
            {
                intHighscoreOption = intIndex;
                lst_strOptions.Add($"{intIndex++}. Show high scores");
            }

            int intBackOption = intIndex;
            lst_strOptions.Add($"{intIndex}. Back to the arcade");

            // We let the user choose what he wants
            do
            {
                int intChoice = DisplaySelectMenu(game.Name, "Select an option:", lst_strOptions, 5);

                // Start the game
                if (intChoice == 1)
                {
                    game.Start();
                }
                // Show the about section only if the game has one
                else if (intChoice == intAboutOption)
                {
                    Console.Clear();
                    ShowTitle(game.Name.ToUpper());

                    tab_strAbout.ToList().ForEach(strAbout => Console.WriteLine("   " + strAbout));

                    Console.WriteLine("\n   -> Back");

                    Console.ReadKey(true);
                }
                // Only show high scores if the game supports them
                else if (intChoice == intHighscoreOption)
                {
                    DisplayHighestScores(game, 5);
                }
                // Quit the game and return to the arcade
                else if (intChoice == intBackOption)
                {
                    return true;
                }
            } while (true);
        }

        /// <summary>
        /// Displays the given game's high score
        /// </summary>
        /// <param name="game">The given game</param>
        /// <param name="intEntryAmount">The number of entry</param>
        static void DisplayHighestScores(Game game, int intEntryAmount)
        {
            ShowTitle(game.Name);

            Console.WriteLine("   " + game.Name + "'s top " + intEntryAmount + " highest scores: ");

            foreach (HighScore score in game.HighScores.OrderByDescending(score => score.Score).Take(intEntryAmount))
            {
                Console.WriteLine("      " + score.Initials + ": " + score.Score);
            }

            Console.WriteLine("\n   -> Back");

            Console.ReadKey(true);
        }


        /// <summary>
        /// Gets the high score for a game
        /// Note: This was made using "Icemanind"'s answer over at https://stackoverflow.com/questions/19456408/add-a-highscore-system-thats-saves-the-data
        /// </summary>
        /// <param name="game">The game to </param>
        /// <returns>A new list of highscores if the file hasn't been found, or the high scores list</returns>
        public static List<HighScore> GetHighScoresFromFile(Game game)
        {
            string path = game.Name + "_highscores.xml";

            if (!File.Exists(path))
                return new List<HighScore>();

            XmlSerializer serializer =
                new XmlSerializer(typeof(List<HighScore>));

            using (StreamReader reader = new StreamReader(path))
            {
                return (List<HighScore>)serializer.Deserialize(reader);
            }
        }


        public static void SetHighScoresToFile(Game game)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<HighScore>));

            using (StreamWriter writer = new StreamWriter(game.Name + "_highscores.xml", false))
            {
                serializer.Serialize(writer, game.HighScores);
            }
        }

        /// <summary>
        /// Small fix for windows 11's terminal
        /// </summary>
        public static void Windows11TerminalFix()
        {
            if (Console.CursorTop == Console.BufferHeight)
            {
                Console.SetBufferSize(Console.BufferWidth, Console.BufferHeight + 3);
            }
            else if (Console.CursorTop == Console.BufferHeight - 1)
            {
                Console.SetBufferSize(Console.BufferWidth, Console.BufferHeight + 2);
            }
            else if (Console.CursorTop == Console.BufferHeight - 2)
            {
                Console.SetBufferSize(Console.BufferWidth, Console.BufferHeight + 1);
            }
        }
    }
}
