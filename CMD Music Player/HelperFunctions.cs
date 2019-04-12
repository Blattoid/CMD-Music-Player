using System;
using System.Collections.Generic;

namespace CMD_Music_Player
{
    class HelperFunctions
    {
        private ProgressBar progressBar = new ProgressBar();

        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public bool IsMediaSelected()
        {
            bool answer;
            try
            {
                string x = Core.player.currentMedia.durationString;
                answer = true;
            }
            catch { answer = false; }
            return answer;
        }
        public string CreateBarFromMediaInfo()
        {
            //very simple; just returns a progress bar in a certain layout with the media information.
            //this function only exists to prevent having to enter this very long command to get a standardised progress bar.
            return progressBar.GenerateBar(Core.player.controls.currentPosition,
                                           Core.player.currentMedia.duration,
                                           barwidth: Console.WindowWidth - 2,
                                           barsuffix: "] " + Core.player.controls.currentPositionString + "/" + Core.player.currentMedia.durationString);
        }

        public void SaveConfiguration()
        {
            Properties.Settings.Default.Save();
            Success("Saved configuration!");
        }

        public List<double> ConvertTimeargsToInt(string[] inputtimeargs)
        {
            List<double> timeargs = new List<double>();
            try
            {
                for (int i = inputtimeargs.Length; i-- > 0;) //reverse it so it is now in the ss:mm:hh format. This makes it easier to process.
                {
                    string item = inputtimeargs[i];
                    if (item == "") timeargs.Add(0); //sanity checking to make sure we actually have a number and not a blank string.
                    else timeargs.Add(Convert.ToDouble(item)); //double is bulletproof since it is a 64 bit number, so it will overflow after 5.38 millenia.
                }
            }
            catch (Exception err) { Error("Invalid time code: " + err.Message); }
            return timeargs;
        }
    }
}