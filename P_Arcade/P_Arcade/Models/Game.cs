using System;
using System.Collections.Generic;

namespace P_Arcade
{
    /// <summary>
    /// The base class for any game
    /// </summary>
    internal class Game
    {
        /// <summary>
        /// The current game's name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The current game's high scores (if there are any)
        /// </summary>
        public List<HighScore> HighScores { get; set; }

        /// <summary>
        /// The active game's current score
        /// </summary>
        public int CurrentScore { get; protected set; }

        /// <summary>
        /// Whether or not the current game supports high scores
        /// </summary>
        public bool SupportsHighscore { get; protected set; }

        public Game(string name, bool blnSupportsHighscore)
        {
            Name = name;

            SupportsHighscore = blnSupportsHighscore;

            if (SupportsHighscore)
            {
                HighScores = Arcade.GetHighScoresFromFile(this);
                Arcade.SetHighScoresToFile(this);
                CurrentScore = 0;
            }
        }

        /// <summary>
        /// Starts the game logic
        /// </summary>
        public virtual void Start()
        {
            Arcade.ShowTitle(Name);

            if (SupportsHighscore)
                CurrentScore = 0;

            Console.WriteLine("   Hello, World!");
            Console.ReadKey(true);
        }
    }
}