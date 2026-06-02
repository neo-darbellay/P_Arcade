using System;

namespace P_Arcade
{
    [Serializable()]
    public class HighScore
    {
        public int Score { get; set; }
        public string Initials { get; set; }

        public HighScore()
        {
            Score = 0;
            Initials = "TMP";
        }

        public HighScore(int score, string name)
        {
            Score = score;

            string strTrimmed = (name ?? "Tmp").Trim().ToUpper();

            if (strTrimmed.Length >= 3)
                Initials = strTrimmed.Substring(0, 3);
            else
                Initials = strTrimmed.PadLeft(3, ' ');
        }
    }
}
