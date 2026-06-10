using System;

namespace P_Arcade.Services
{
    internal class InputService
    {
        /// <summary>
        /// Reads a line of input and returns null if the escape key gets pressed
        /// This is a recreation of Console.ReadLine(), but with the ability to stop input by pressing escape
        /// </summary>
        public static string ReadLineOrEscape()
        {
            string strInput = "";

            while (true)
            {
                ConsoleKeyInfo keyPressedInfo = Console.ReadKey(true);

                if (keyPressedInfo.Key == ConsoleKey.Escape)
                    return null;

                if (keyPressedInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return strInput;
                }

                if (keyPressedInfo.Key == ConsoleKey.Backspace && strInput.Length > 0)
                {
                    strInput = strInput.Remove(strInput.Length - 1, 1);

                    // Brings the cursor back and erases the previous character
                    Console.Write("\b \b");
                }

                if (!char.IsControl(keyPressedInfo.KeyChar))
                {
                    strInput += keyPressedInfo.KeyChar;
                    Console.Write(keyPressedInfo.KeyChar);
                }
            }
        }



        /// <summary>
        /// Get the user's input, and verify that it is in bound.
        /// Returns false if the user presses Escape.
        /// </summary>
        /// <param name="bytAnswer">The variable that will get changed</param>
        /// <param name="MIN_VALUE">Minimal value</param>
        /// <param name="MAX_VALUE">Maximum value</param>
        public static bool GetInputInBoundaries(out byte bytAnswer, byte MIN_VALUE, byte MAX_VALUE)
        {
            bool blnVerification = false;
            do
            {
                Console.Write("   Your input: ");

                string strLine = ReadLineOrEscape();

                if (strLine == null)
                {
                    bytAnswer = 255;
                    return false;
                }

                bool blnResult = byte.TryParse(strLine, out bytAnswer);
                bool blnResultInBound = blnResult && (bytAnswer >= MIN_VALUE) && (bytAnswer <= MAX_VALUE);

                if (blnResultInBound)
                    blnVerification = true;
                else if (blnResult)
                    Console.Write("\n   Your value isn't between {0} and {1}, please retry.\n\n", MIN_VALUE, MAX_VALUE);
                else
                    Console.Write("\n   Your value isn't a number, please retry.\n\n");

                Arcade.Windows11TerminalFix();
            }
            while (!blnVerification);

            return true;
        }
    }
}
