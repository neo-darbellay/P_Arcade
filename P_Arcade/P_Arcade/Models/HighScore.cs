using System;

namespace P_Arcade.Models
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

        public HighScore(int intScore, string strName)
        {
            Score = intScore;

            string strTrimmed = (strName ?? "Tmp").Trim().ToUpper();

            if (strTrimmed.Length >= 3)
                Initials = strTrimmed.Substring(0, 3);
            else
                Initials = strTrimmed.PadLeft(3, ' ');
        }
    }
}
