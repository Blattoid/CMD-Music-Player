using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}