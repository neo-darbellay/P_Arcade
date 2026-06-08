using System;
using System.Linq;
using System.Runtime.InteropServices;

using P_Arcade.Models;
using P_Arcade.Services;

namespace P_Arcade.Games
{
    /// <summary>
    /// Represents a single die.
    /// </summary>
    internal class Die
    {
        private readonly Random _random;
        private const byte _VAL_MIN = 1;
        private readonly byte _bytValMax;

        public byte Value { get; private set; }

        public Die(Random random, byte maxValue)
        {
            _random = random;
            _bytValMax = (byte)(maxValue + 1);
            Roll();
        }

        /// <summary>
        /// Rolls the die, assigning it a new random value.
        /// </summary>
        public void Roll()
        {
            Value = (byte)_random.Next(_VAL_MIN, _bytValMax);
        }

        /// <summary>
        /// Draws one horizontal line of the die's box art.
        /// </summary>
        /// <param name="line">0 = top border, 1 = value row, 2 = bottom border</param>
        public void Display(byte line)
        {
            switch (line)
            {
                case 0: Console.Write("╔═══╗ "); break;
                case 1: Console.Write($"║ {Value} ║ "); break;
                case 2: Console.Write("╚═══╝ "); break;
            }
        }
    }

    internal class Yahtzee : Game
    {
        /// <summary>
        /// The Yahtzee game's constructor
        /// </summary>
        public Yahtzee() : base("Yahtzee", false) { }

        // Constants used for min/max of the amount of players
        const byte VAL_MIN_PLAYERS = 2;
        const byte VAL_MAX_PLAYERS = 4;

        private enum Category
        {
            Ones,
            Twos,
            Threes,
            Fours,
            Fives,
            Sixes,

            UpperTotal,
            Bonus,

            ThreeOfAKind,
            FourOfAKind,
            FullHouse,
            SmallStraight,
            LargeStraight,
            Yahtzee,
            Chance,

            LowerTotal,
            GrandTotal
        }

        private static readonly Category[] TBL_PLAYABLE_CATEGORIES =
        {
            Category.Ones,
            Category.Twos,
            Category.Threes,
            Category.Fours,
            Category.Fives,
            Category.Sixes,
            Category.ThreeOfAKind,
            Category.FourOfAKind,
            Category.FullHouse,
            Category.SmallStraight,
            Category.LargeStraight,
            Category.Yahtzee,
            Category.Chance
        };

        // Category names
        static readonly string[] TBL_CATEGORIES = {
            "Ones (1)",
            "Twos (2)",
            "Threes (3)",
            "Fours (4)",
            "Fives (5)",
            "Sixes (6)",

            "Upper Total",
            "Bonus (+25 if total >= 15)",

            "Three of a Kind",
            "Four of a Kind",
            "Full House (3+2)",
            "Small Straight (4 dice)",
            "Large Straight (5 dice)",
            "Yahtzee (5 of a kind)",
            "Chance (free total)",

            "Lower Total",
            "Grand Total"
        };

        // Player states
        static int[,] tbl_intScores;
        static byte bytPlayers;
        static readonly Die[] tbl_Dice = new Die[5];
        static readonly Random random = new Random();

        public override string[] About()
        {
            string[] tab_strAbout = new string[]
            {
                "Yahtzee is a dice-based strategy game where players aim to score the highest total points",
                "It is played over 13 rounds using five dice per player",

                "",

                "Each turn allows up to three rolls to build the best possible combination",
                "Players may choose which dice to keep and which to reroll between attempts",

                "",

                "After rolling, the player must assign their result to one unused scoring category",
                "Each category can only be used once per player during the game",

                "",

                "Upper section categories score based on individual dice values (ones through sixes)",
                "Bonus points are awarded if the upper section total reaches a required threshold",

                "",

                "Lower section categories include combinations such as three of a kind, full house, and straights",
                "Each combination has specific scoring rules and conditions",

                "",

                "The game ends after all categories are filled",
                "The player with the highest total score wins",

                "",

                "Use number input to hold dice during rolls (1 to keep, 0 to reroll)",
                "Select a category number to assign your final roll score",
                "Press Enter after input to confirm actions"
            };

            return tab_strAbout;
        }

        public override void Start()
        {
            GetUserInput();

            tbl_intScores = new int[TBL_CATEGORIES.Length, bytPlayers];

            // Initialise every cell to -1 (distinguishes "not yet scored" from a genuine 0)
            for (int i = 0; i < TBL_CATEGORIES.Length; i++)
                for (int j = 0; j < bytPlayers; j++)
                    tbl_intScores[i, j] = -1;

            // 13 rounds, each player takes one turn per round
            for (int intRound = 0; intRound < 13; intRound++)
                for (int intPlayer = 0; intPlayer < bytPlayers; intPlayer++)
                    PlayTurn(intPlayer, intRound);

            // Final scoring and display
            CalculateFinalScores();

            Arcade.ShowTitle(Name);
            Console.WriteLine("\n   Final Results:\n");
            ShowGrid();
            DisplayWinner();

            Console.ReadKey(true);
        }

        /// <summary>
        /// Renders the full score grid for all players.
        /// </summary>
        private static void ShowGrid()
        {
            // Top border
            string strTopLine = "╔═════════════════════════════";
            for (int i = 0; i < bytPlayers; i++)
                strTopLine += "╦═════════════";
            Console.WriteLine("   " + strTopLine + "╗");

            // Header row
            string strHeader = "║          Category           ║";
            for (int i = 0; i < bytPlayers; i++)
                strHeader += $" {"Player " + (i + 1),-11} ║";
            Console.WriteLine("   " + strHeader);

            // Separator
            string strSepLine = "╠═════════════════════════════";
            for (int i = 0; i < bytPlayers; i++)
                strSepLine += "╬═════════════";

            Console.WriteLine("   " + strSepLine + "╣");

            // Category rows
            for (int i = 0; i < TBL_CATEGORIES.Length; i++)
            {
                // Add an extra line if we need it
                if (i == (int)Category.UpperTotal ||
                    i == (int)Category.ThreeOfAKind ||
                    i == (int)Category.LowerTotal ||
                    i == (int)Category.GrandTotal)
                {
                    Console.WriteLine("   " + strSepLine + "╣");
                }
                string strRow = $"║ {TBL_CATEGORIES[i], -27} ║";

                for (int j = 0; j < bytPlayers; j++)
                {
                    string strScore;
                    if (i == (int)Category.Bonus)
                        strScore = tbl_intScores[i, j] == -1 ? "" : "+ " + tbl_intScores[i, j] + " pts";
                    else
                        strScore = tbl_intScores[i, j] == -1 ? "" : tbl_intScores[i, j] + " pts";

                    strRow += $" {strScore, -11} ║";
                }

                Console.WriteLine("   " + strRow);
            }

            // Bottom border
            string strBottomLine = "╚═════════════════════════════";
            for (int i = 0; i < bytPlayers; i++)
                strBottomLine += "╩═════════════";
            Console.WriteLine("   " + strBottomLine + "╝");
        }

        /// <summary>
        /// Renders all five dice as ASCII box art
        /// </summary>
        private static void ShowDice()
        {
            for (byte line = 0; line < 3; line++)
            {
                Console.Write("   ");
                for (int i = 0; i < 5; i++)
                    tbl_Dice[i].Display(line);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Runs a single player's turn
        /// </summary>
        private static void PlayTurn(int intPlayer, int intRound)
        {
            Arcade.ShowTitle("Yahtzee");
            ShowGrid();

            Console.WriteLine($"\n   Player {intPlayer + 1}'s turn — Round {intRound + 1}");

            // Fresh dice each turn
            for (int i = 0; i < 5; i++)
                tbl_Dice[i] = new Die(random, 6);

            int intRolls = 0;
            bool[] tbl_blnKeep = new bool[5];

            while (intRolls < 3)
            {
                // Skip remaining rolls if all dice are held
                if (tbl_blnKeep.All(k => k)) break;

                Console.WriteLine($"\n   Roll #{intRolls + 1}");

                // Roll only unheld dice
                for (int i = 0; i < 5; i++)
                    if (!tbl_blnKeep[i])
                        tbl_Dice[i].Roll();

                ShowDice();

                // No hold prompt after the last roll
                if (intRolls == 2) break;

                // Ask which dice to hold
                string strInput;

                do
                {
                    Console.Write("\n   Enter 5 digits (1 = keep, 0 = reroll), e.g. 10101\n   ");
                    strInput = Console.ReadLine() ?? "";
                    Console.WriteLine();
                }
                while (strInput.Length != 5);

                for (int i = 0; i < 5; i++)
                    tbl_blnKeep[i] = strInput[i] == '1';

                intRolls++;
            }

            ChooseCategory(intPlayer);
        }

        /// <summary>
        /// Prompts the current player to pick an available scoring category.
        /// </summary>
        private static void ChooseCategory(int intPlayer)
        {
            int intChosenCategory;

            do
            {
                Console.WriteLine("\n   Available categories:");

                for (int i = 0; i < TBL_PLAYABLE_CATEGORIES.Length; i++)
                {
                    Category selectedCategory = TBL_PLAYABLE_CATEGORIES[i];

                    int intScore = tbl_intScores[(int)selectedCategory, intPlayer];
                    string strDisplayScore = intScore != -1 ? $"({intScore})" : "";

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"   {i + 1 + ".",-4}");
                    Console.ResetColor();

                    Console.WriteLine(
                        string.Format(
                            "{0,-24}{1,4}",
                            TBL_CATEGORIES[(int)selectedCategory],
                            strDisplayScore));
                }

                Console.Write("\n   Category number: ");

                bool blnValid = int.TryParse(Console.ReadLine(), out intChosenCategory);

                if (!blnValid)
                {
                    Console.WriteLine("   Invalid input. Try again.");
                    intChosenCategory = -1;
                    continue;
                }

                if (intChosenCategory < 1 ||
                    intChosenCategory > TBL_PLAYABLE_CATEGORIES.Length)
                {
                    Console.WriteLine("   That category does not exist. Try again.");
                    intChosenCategory = -1;
                    continue;
                }

                Category category = TBL_PLAYABLE_CATEGORIES[intChosenCategory - 1];

                if (tbl_intScores[(int)category, intPlayer] != -1)
                {
                    Console.WriteLine("   That category is already filled. Try again.");
                    intChosenCategory = -1;
                }
            }
            while (intChosenCategory == -1);

            Category chosenCategory = TBL_PLAYABLE_CATEGORIES[intChosenCategory - 1];

            tbl_intScores[(int)chosenCategory, intPlayer] =
                CalculateScore((int)chosenCategory);
        }

        /// <summary>
        /// Returns true if the given dice values contain a small straight (any 4 consecutive values)
        /// </summary>
        private static bool HasSmallStraight(int[] diceValues)
        {
            string strValues = string.Join("", diceValues.Distinct().OrderBy(x => x));
            return strValues.Contains("1234") || strValues.Contains("2345") || strValues.Contains("3456");
        }

        /// <summary>
        /// Returns true if the given dice values form a large straight (any 5 consecutive values)
        /// </summary>
        private static bool HasLargeStraight(int[] diceValues)
        {
            string strValues = string.Join("", diceValues.Distinct().OrderBy(x => x));
            return strValues.Contains("12345") || strValues.Contains("23456");
        }

        /// <summary>
        /// Calculates the score for the given category index based on the current dice
        /// </summary>
        private static int CalculateScore(int intCategory)
        {
            int[] tbl_intDiceValues = tbl_Dice.Select(d => (int)d.Value).ToArray();
            int intSum = tbl_intDiceValues.Sum();
            var groups = tbl_intDiceValues.GroupBy(x => x).OrderByDescending(g => g.Count()).ToArray();

            switch (intCategory)
            {
                case 0: return tbl_intDiceValues.Where(d => d == 1).Sum();
                case 1: return tbl_intDiceValues.Where(d => d == 2).Sum();
                case 2: return tbl_intDiceValues.Where(d => d == 3).Sum();
                case 3: return tbl_intDiceValues.Where(d => d == 4).Sum();
                case 4: return tbl_intDiceValues.Where(d => d == 5).Sum();
                case 5: return tbl_intDiceValues.Where(d => d == 6).Sum();
                case 8: return groups.Any(g => g.Count() >= 3) ? intSum : 0;                                        // Three of a kind
                case 9: return groups.Any(g => g.Count() >= 4) ? intSum : 0;                                        // Four of a kind
                case 10: return (groups.Length == 2 && groups[0].Count() == 3 && groups[1].Count() == 2) ? 15 : 0;  // Full house
                case 11: return HasSmallStraight(tbl_intDiceValues) ? 20 : 0;                                       // Small straight
                case 12: return HasLargeStraight(tbl_intDiceValues) ? 30 : 0;                                       // Large straight
                case 13: return groups.Any(g => g.Count() == 5) ? 40 : 0;                                           // Yahtzee
                case 14: return intSum;                                                                             // Chance
                default: return 0;
            }
        }

        /// <summary>
        /// Fills in all auto-calculated totals and bonus rows for every player
        /// </summary>
        private static void CalculateFinalScores()
        {
            for (int intPlayer = 0; intPlayer < bytPlayers; intPlayer++)
            {
                int intUpperTotal = 0;
                int intLowerTotal = 0;

                for (int i = 0; i <= 5; i++)
                    intUpperTotal += tbl_intScores[i, intPlayer];

                tbl_intScores[6, intPlayer] = intUpperTotal;
                tbl_intScores[7, intPlayer] = intUpperTotal >= 15 ? 25 : 0;

                for (int i = 8; i <= 14; i++)
                    intLowerTotal += tbl_intScores[i, intPlayer];

                tbl_intScores[15, intPlayer] = intLowerTotal;
                tbl_intScores[16, intPlayer] = intUpperTotal + tbl_intScores[7, intPlayer] + intLowerTotal;
            }
        }

        /// <summary>
        /// Prints the winner (or announces a tie) after all rounds are complete
        /// </summary>
        private static void DisplayWinner()
        {
            int intBestPlayer = 0;
            int intBestScore = 0;
            bool blnTie = false;

            for (int intPlayer = 0; intPlayer < bytPlayers; intPlayer++)
            {
                int intScore = tbl_intScores[16, intPlayer];

                if (intScore > intBestScore)
                {
                    intBestPlayer = intPlayer;
                    intBestScore = intScore;
                    blnTie = false;
                }
                else if (intScore == intBestScore)
                {
                    blnTie = true;
                }
            }

            if (!blnTie)
                Console.WriteLine($"\n\n   Player {intBestPlayer + 1} wins with {intBestScore} points!");
            else
                Console.WriteLine("\n\n   It's a tie! No winner this time.");

            Console.WriteLine("\n   Press any key to continue");
        }


        /// <summary>
        /// Asks the user how many players will participate
        /// </summary>
        private void GetUserInput()
        {
            Arcade.ShowTitle(Name);

            Console.Write("   Please enter the number of players.\n   The value needs to be greater than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(VAL_MIN_PLAYERS);
            Console.ResetColor();

            Console.Write(" and smaller than ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(VAL_MAX_PLAYERS);
            Console.ResetColor();

            InputService.GetInputInBoundaries(out bytPlayers, VAL_MIN_PLAYERS, VAL_MAX_PLAYERS);
        }

    }
}