using System;
using System.IO;

namespace CMD_Music_Player
{
    class Program
    {
        public static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer(); //the namesake of this program
        public static void error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            for (; ; )
            {
                Console.Write(">");
                string command = Console.ReadLine();
                string commandupper = command.ToUpper();

                if (commandupper.StartsWith("HELP")) { Console.WriteLine("List of commands:\n\tls\n\tplay\n\tstop\n\tpause\n\texit"); }
                else if (commandupper.StartsWith("LS"))
                {
                    foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                        Console.WriteLine("\t" + Path.GetFileName(file));
                }
                else if (commandupper == "PLAY") { functions.resume(player); }
                else if (commandupper.StartsWith("PLAY "))
                {
                    string path = command.Substring(5);
                    if (File.Exists(path))
                    {
                        Console.WriteLine("Playing " + path + "...");
                        functions.play(player, path);
                    }
                    else { error("Error playing file: File doesn't exist."); }
                }
                else if (commandupper.StartsWith("STOP")) { functions.stop(player); }
                else if (commandupper.StartsWith("PAUSE")) { functions.pause(player); }
                else if (commandupper.StartsWith("EXIT")) { Environment.Exit(0); }
                else { error("Command unknown. Type 'help' for a list of commands."); }
            }
        }
        class functions
        {
            public static void play(WMPLib.WindowsMediaPlayer player, string @filepath)
            {
                try
                {
                    player.URL = filepath;
                    player.controls.play();
                }
                catch (Exception err) { error("Error playing file: " + err.Message); }
            }
            public static void resume(WMPLib.WindowsMediaPlayer player) { player.controls.play(); }
            public static void stop(WMPLib.WindowsMediaPlayer player){ player.controls.stop(); }
            public static void pause(WMPLib.WindowsMediaPlayer player){ player.controls.pause(); }
            public string readPosition(WMPLib.WindowsMediaPlayer player){ return player.controls.currentPositionString; }
        }
    }
}