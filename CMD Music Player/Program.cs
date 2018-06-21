using System;
using System.IO;

namespace CMD_Music_Player
{
    class Program
    {
        public static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer(); //the heart of this program
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

                if (commandupper.StartsWith("HELP")) { Console.WriteLine("List of commands:\n\tls\n\tplay\n\tstop\n\tpause\n\tpos\n\texit"); }
                else if (commandupper.StartsWith("LS"))
                {
                    foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                        Console.WriteLine("\t" + Path.GetFileName(file));
                }
                else if (commandupper == "PLAY") { player.controls.play(); }
                else if (commandupper.StartsWith("PLAY "))
                {
                    string path = command.Substring(5);
                    if (File.Exists(path))
                    {
                        Console.WriteLine("Playing " + path + "...");
                        try
                        {
                            player.URL = path;
                            player.controls.play();
                        }
                        catch (Exception err) { error("Error playing file: " + err.Message); }
                    }
                    else { error("Error playing file: File doesn't exist."); }
                }
                else if (commandupper.StartsWith("STOP")) { player.controls.stop(); }
                else if (commandupper.StartsWith("PAUSE")) { player.controls.pause(); }
                else if (commandupper.StartsWith("POS"))
                {
                    string pos = player.controls.currentPositionString;
                    if (pos == "") { error("No file playing."); }
                    else { Console.WriteLine(pos); }
                }
                else if (commandupper.StartsWith("EXIT")) { Environment.Exit(0); }
                else { error("Command unknown. Type 'help' for a list of commands."); }
            }
        }
    }
}