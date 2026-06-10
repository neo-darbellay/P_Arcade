using P_Arcade.Games;
using P_Arcade.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using static P_Arcade.Services.ConsoleModifications;

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

        const int VAL_PAGE_SIZE = 5;

        static void Main()
        {
            AvailableGames = new List<Game>
            {
                // Initialize games
                new TwoThousandFourtyEight(),
                new Connect4(),
                new CopyADrawing(),
                new MineSweeper(),
                new SimonGame(),
                new SlidingPuzzle(),
                new SnakeGame(),
                new TicTacToe(),
                new Yahtzee()
            };

            int intPage = 0;
            int intTotalGames = AvailableGames.Count;
            bool blnMultiPage = intTotalGames > VAL_PAGE_SIZE;
            int intTotalPages = (intTotalGames + VAL_PAGE_SIZE - 1) / VAL_PAGE_SIZE;

            // Show every available game and handle the user's choice
            do
            {
                int intStart = intPage * VAL_PAGE_SIZE;
                int intEnd = Math.Min(intStart + VAL_PAGE_SIZE, intTotalGames);

                // Set up the game names for the current page
                List<string> lst_strOptions = new List<string>();
                int intSelectableCount = 0;

                // Add Previous/Next buttons if there are more games than fit on one page

                // Separation line
                lst_strOptions.Add(null);

                int intPreviousIndex = -1;
                int intNextIndex = -1;

                if (blnMultiPage)
                {
                    intPreviousIndex = ++intSelectableCount;
                    lst_strOptions.Add($"{intPreviousIndex}. Previous page");

                    intNextIndex = ++intSelectableCount;
                    lst_strOptions.Add($"{intNextIndex}. Next page");
                }

                // Separation line
                lst_strOptions.Add(null);

                // Track which selectable index the games start at
                int intFirstGameIndex = intSelectableCount + 1;

                for (int i = intStart; i < intEnd; i++)
                    lst_strOptions.Add($"{++intSelectableCount}. {AvailableGames[i].Name}");

                int intGamesOnPage = intSelectableCount;

                string strSubTitle = "Select the game you want to play, or exit the app by pressing escape";

                if (blnMultiPage)
                    strSubTitle += $" (Page {intPage + 1}/{intTotalPages})";

                int intUserChoice = DisplaySelectMenu("P_Arcade", strSubTitle, lst_strOptions, 5);

                if (intUserChoice == -1)
                {
                    Console.SetCursorPosition(3, 5 + lst_strOptions.Count + 3);
                    Environment.Exit(418);
                }
                else if (blnMultiPage && intUserChoice == intPreviousIndex)
                {
                    // Wrap around to the last page if on the first
                    intPage = (intPage - 1 + intTotalPages) % intTotalPages;
                }
                else if (blnMultiPage && intUserChoice == intNextIndex)
                {
                    // Wrap around to the first page if on the last
                    intPage = (intPage + 1) % intTotalPages;
                }
                else if (intUserChoice >= intFirstGameIndex && intUserChoice <= intGamesOnPage)
                    ShowGameOptions(AvailableGames[intStart + intUserChoice - intFirstGameIndex]);
            } while (true);
        }

        /// <summary>
        /// Shows a title. Either a game's, or the program's
        /// </summary>
        /// <param name="strTitle">The current title</param>
        public static void ShowTitle(string strTitle)
        {
            // Fullscreen the app
            Fullscreen();

            Console.Clear();
            Console.ResetColor();

            string strByLine = "By Néo Darbellay";

            // Determine the inner width of the box
            int intInnerWidth = Math.Max(strTitle.Length, strByLine.Length) + 4; // padding

            string strTop = $"╔════{new string('═', intInnerWidth)}════╗";
            string strBottom = $"╚════{new string('═', intInnerWidth)}════╝";

            Console.WriteLine("   " + strTop);
            Console.WriteLine("   ║    " + CenterText(strTitle, intInnerWidth) + "    ║");
            Console.WriteLine("   ║    " + CenterText(strByLine, intInnerWidth) + "    ║");
            Console.WriteLine("   " + strBottom + "\n");

            // Change the console's title as well
            Console.Title = strTitle;
        }

        /// <summary>
        /// Helper function to center text with a specific width
        /// </summary>
        /// <param name="strText">The text to center</param>
        /// <param name="intWidth">The total width</param>
        static string CenterText(string strText, int intWidth)
        {
            int intPadding = intWidth - strText.Length;
            int intPadLeft = intPadding / 2;
            int intPadRight = intPadding - intPadLeft;

            return new string(' ', intPadLeft) + strText + new string(' ', intPadRight);
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

            ConsoleKeyInfo keyPressedInfo;

            Console.CursorVisible = false;
            Console.CursorTop = intTopLine;

            Console.WriteLine("   " + strSubTitle);

            // Build a line map for selectable items, rendering null entries as blank separator lines
            List<int> lst_intChoiceLines = new List<int>();
            int intCurrentLine = intTopLine;

            for (int i = 0; i < lst_strChoices.Count; i++)
            {
                intCurrentLine++;

                if (lst_strChoices[i] == null)
                    Console.WriteLine();
                else
                {
                    lst_intChoiceLines.Add(intCurrentLine);
                    Console.WriteLine("      " + lst_strChoices[i].PadRight(64));
                }
            }

            int intSelectableCount = lst_intChoiceLines.Count;
            int intUserChoice = 1;

            do
            {
                // Points the current choice with an arrow
                Console.SetCursorPosition(3, lst_intChoiceLines[intUserChoice - 1]);
                Console.Write("->");

                // Waits for the user to press a key
                keyPressedInfo = Console.ReadKey(true);

                // Deletes the previous arrow
                Console.SetCursorPosition(3, lst_intChoiceLines[intUserChoice - 1]);
                Console.Write("  ");

                if (keyPressedInfo.Key == ConsoleKey.Escape)
                    return -1;
                else if (keyPressedInfo.Key == ConsoleKey.DownArrow || keyPressedInfo.Key == ConsoleKey.S)
                {
                    intUserChoice++;
                    // If exceed the number of choices, reset to the min choice
                    if (intUserChoice > intSelectableCount)
                        intUserChoice = 1;
                }
                else if (keyPressedInfo.Key == ConsoleKey.UpArrow || keyPressedInfo.Key == ConsoleKey.W)
                {
                    intUserChoice--;
                    // If is less than 1, set the choosed option to the last option
                    if (intUserChoice < 1)
                        intUserChoice = intSelectableCount;
                }
                else if (char.IsDigit(keyPressedInfo.KeyChar))
                {
                    int intChoice = keyPressedInfo.KeyChar - '0';

                    if (intChoice >= 1 && intChoice <= intSelectableCount)
                        return intChoice;
                }
            } while (keyPressedInfo.Key != ConsoleKey.Enter && keyPressedInfo.Key != ConsoleKey.Spacebar);

            return intUserChoice;
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

            int intAboutOption = -2;
            int intHighscoreOption = -2;

            string[] tab_strAbout = game.About() ?? new string[0];

            bool blnAboutSection = tab_strAbout.Any(strAbout => !string.IsNullOrWhiteSpace(strAbout));
            if (blnAboutSection)
            {
                intAboutOption = intIndex;
                lst_strOptions.Add($"{intIndex++}. About");
            }

            if (game.SupportsHighscore)
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
                else if (intChoice == intBackOption || intChoice == -1)
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
            string strPath = game.Name + "_highscores.xml";

            if (!File.Exists(strPath))
                return new List<HighScore>();

            XmlSerializer serializer = new XmlSerializer(typeof(List<HighScore>));

            using (StreamReader reader = new StreamReader(strPath))
                return ((List<HighScore>)serializer.Deserialize(reader)).OrderByDescending(score => score.Score).ToList();
        }


        public static void SetHighScoresToFile(Game game)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<HighScore>));

            using (StreamWriter writer = new StreamWriter(game.Name + "_highscores.xml", false))
                serializer.Serialize(writer, game.HighScores);
        }

        /// <summary>
        /// Small fix for windows 11's terminal to make sure the buffer is never too small
        /// </summary>
        public static void Windows11TerminalFix() => Console.SetBufferSize(Console.BufferWidth, Math.Max(Console.BufferHeight, Console.CursorTop + 3));
    }
}
