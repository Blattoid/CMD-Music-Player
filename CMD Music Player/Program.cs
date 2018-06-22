using System;
using System.Collections.Generic;
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

                if (commandupper.StartsWith("HELP"))
                {
                    Console.WriteLine("List of commands:\n" +
"\tls\n" +
"\tplay\n" +
"\tstop\n" +
"\tpause\n" +
"\tpos\n" +
"\tgoto\n" +
"\texit");
                }
                else if (commandupper.StartsWith("LS"))
                {
                    foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                        Console.WriteLine("\t" + Path.GetFileName(file));
                }

                else if (commandupper.Replace(" ", "") == "PLAY") { player.controls.play(); }
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
                    try
                    {
                        string pos = player.controls.currentPositionString;
                        string totallength = player.currentMedia.durationString;
                        if (pos == "") { Console.WriteLine("At beginning of media."); }//at beginning of track
                        else { Console.WriteLine(pos + "/" + totallength); }
                    }
                    catch (Exception) { error("No media playing."); }
                }

                else if (commandupper.Replace(" ", "") == "GOTO") { error("Syntax: mm:ss"); }
                else if (commandupper.StartsWith("GOTO"))
                {
                    //player.controls.currentPosition is a double representing the number of seconds the track has been playing for.
                    command = command.Substring(5); //trim command + space
                    string[] strtimeargs = command.Split(Char.Parse(":"));
                    List<int> timeargs = new List<int>();

                    try
                    {
                        foreach (string item in strtimeargs)
                        {
                            if (item == "") { error("Syntax: [hh:][mm:]ss"); continue; }
                            timeargs.Add(Convert.ToInt32(item)); //int32 works just fine since it represents seconds, and 2147483647 seconds is 68.04 years.
                        }
                    }
                    catch (Exception err) { error("Invalid time code: " + err.Message); continue; }
                    int seconds = 0;
                    if (timeargs.Count == 1){ seconds = timeargs[0]; } //ss
                    if (timeargs.Count == 2){ seconds = (timeargs[0] * 60) + timeargs[1]; } //mm:ss
                    if (timeargs.Count == 3){ seconds = (timeargs[0] * 60 * 60) + (timeargs[1] * 60) + timeargs[2]; } //hh:mm:ss
                    player.controls.currentPosition = seconds;
                }
                else if (commandupper.StartsWith("EXIT")) { Environment.Exit(0); }
                else if (command == "") { } //blank commands are ignored.
                else { error("Command unknown. Type 'help' for a list of commands."); }
            }
        }
    }
}