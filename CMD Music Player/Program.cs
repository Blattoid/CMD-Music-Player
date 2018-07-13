using System;
using System.Collections.Generic;
using System.IO;

namespace CMD_Music_Player
{
    static class Program
    {
        public static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer(); //the heart of this program

        static bool bypassfilecheck = false;
        static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        public static void generateProgressBar()
        {
            //CURRENTLY DOESNT WORK
            try
            {
                int pos = Convert.ToInt32(player.controls.currentPosition);
                int totallength = Convert.ToInt32(player.currentMedia.duration);
                if (pos == 0) { Console.WriteLine("At beginning of media."); } //at beginning of track
                else
                {
                    //draw the progress bar
                    Console.Write("[");
                    int progress = Convert.ToInt32(Map(pos, 0, totallength, 0, Console.WindowWidth- (player.controls.currentPositionString.Length + player.currentMedia.durationString.Length))); //calculate progress
                    for (int i = 0; i <= progress-2; i++) { Console.Write("="); }
                    Console.Write(">"); //arrow
                    for (int i = 0; i <= Console.WindowWidth - progress - (player.controls.currentPositionString.Length+ player.currentMedia.durationString.Length)-6; i++) { Console.Write("."); } //2 for the [], 1 for the >, and however much else for the current/total time
                    Console.Write("] ");
                    //write the current and total time.
                    Console.WriteLine(player.controls.currentPositionString + "/" + player.currentMedia.durationString);
                }
            }
            catch (Exception) { error("No media playing."); }
        }
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
                                            "\tplay\n" +
                                            "\tstop\n" +
                                            "\tpause\n\n" +
                                            "\tpos\n" +
                                            "\tgoto\n\n" +
                                            "\tls\n" +
                                            "\texit"
                    );  // don't be sad ;-;
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
                    //remove quotation marks, as they interfere with the media player.
                    path = path.Replace(Convert.ToChar("\""), Convert.ToChar(""));
                    if (File.Exists(path) || bypassfilecheck) //check if either the file exists, or if said check is disabled.
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
                    try { Console.WriteLine(player.controls.currentPositionString + "/" + player.currentMedia.durationString); }
                    catch (Exception err) { error("No media is loaded."); }
                    //generateProgressBar();
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
                    if (timeargs.Count == 1) { seconds = timeargs[0]; } //ss
                    if (timeargs.Count == 2) { seconds = (timeargs[0] * 60) + timeargs[1]; } //mm:ss
                    if (timeargs.Count == 3) { seconds = (timeargs[0] * 60 * 60) + (timeargs[1] * 60) + timeargs[2]; } //hh:mm:ss
                    player.controls.currentPosition = seconds;
                    //generateProgressBar();
                }

                else if (commandupper.StartsWith("TOGGLE_FILECHECK"))
                {
                    //just for fun: disables checking of existence of a file. this could allow for lots of possible input, such as http addresses pointing to media
                    bypassfilecheck = !bypassfilecheck;
                    Console.WriteLine(!bypassfilecheck);
                }
                else if (commandupper.StartsWith("EXIT")) { Environment.Exit(0); }
                else if (command == "") { } //blank commands are ignored.
                else { error("Command unknown. Type 'help' for a list of commands."); }
            }
        }
    }
}