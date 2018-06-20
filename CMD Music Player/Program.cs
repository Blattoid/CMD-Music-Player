using System;

namespace CMD_Music_Player
{
    class Program
    {
        public static WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer(); //the namesake of this program
        static void Main(string[] args)
        {
            string filepath = @"C:\Users\USERNAME\Music\eyyy it works.mp3";
            try
            {
                Console.WriteLine("Starting playback of " + filepath);
                wplayer.URL = filepath;
                wplayer.controls.play();
                Console.ReadLine();
            }
            catch (Exception err) { Console.WriteLine("Error playing file: " + err.Message); }
        }
    }
}
