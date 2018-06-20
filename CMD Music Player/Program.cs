using System;
using System.IO;

namespace CMD_Music_Player
{
    class Program
    {
        public WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer(); //the namesake of this program
        public static void error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    class functions
    {
        void play(WMPLib.WindowsMediaPlayer player, string @filepath)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    player.URL = filepath;
                    player.controls.play();
                }
                catch (Exception err) { Program.error("Error playing file: " + err.Message); }
            }
        }
    }
}