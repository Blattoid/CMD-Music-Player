using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CMD_Music_Player
{
    static class Program
    {
        public static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer(); //the heart of this program

        static bool bypassfilecheck = false;
        public static void error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static bool IsMediaSelected()
        {
            bool answer;
            try
            {
                string x = player.currentMedia.durationString;
                answer = true;
            }
            catch { answer = false; }
            return answer;
        }
        static string CreateBarFromMediaInfo()
        {
            //very simple; just returns a progress bar in a certain layout with the media information.
            //this function only exists to prevent having to enter this very long command to get a standardised progress bar.
            return ProgressBar.GenerateBar(player.controls.currentPosition,
                                           player.currentMedia.duration,
                                           barwidth: Console.WindowWidth-2,
                                           barsuffix: "] " + player.controls.currentPositionString + "/" + player.currentMedia.durationString);
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
                    path = path.Replace(Convert.ToChar("\""), Convert.ToChar(" "));
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
                    if (IsMediaSelected()) { Console.WriteLine(CreateBarFromMediaInfo()); }
                    else { error("No media is loaded."); }
                    
                }
                else if (commandupper.Replace(" ", "") == "GOTO") { error("Syntax: mm:ss"); }
                else if (commandupper.StartsWith("GOTO"))
                {
                    if (!IsMediaSelected()) { error("No media is selected!"); continue; }

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

                    Console.WriteLine(CreateBarFromMediaInfo());
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

    static class ProgressBar
    {
        static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        
        //this was converted by hand from Python to C#.
        public static string GenerateBar(double value, double maxvalue,
            string barprefix = "[", string filledchar = "=", string pointerchar = ">",
            string emptychar = " ", string barsuffix = "]", int barwidth = 50)
        {
            //sanity checks to make sure nothing is going to break anything
            if (value > maxvalue)
            {
                Program.error("Current value is bigger than max value!");
                return "";
            }
            if (value < 0)
            {
                Program.error("Current value is smaller than 0!");
                return "";
            }

            decimal percentage = (decimal)value / (decimal)maxvalue * 100; //calculate the percentage of progress
                                                           //calculate how much space the non-changing parts of the bar will take up
            int length = (
                barprefix.Length +
                pointerchar.Length +
                barsuffix.Length 
            );

            //If that length is smaller than the barwidth, then there will literally be no space left for the bar to move. Oh well.
            //adjust the bar to fit the space it has, using the length we just calculated
            value = Convert.ToInt32(Map((decimal)value, 0, (decimal)maxvalue, 0, barwidth - length));
            maxvalue = barwidth - length;

            //construct the bar as a string
            string bar = barprefix; //start with the bar prefix
            bar += String.Concat(Enumerable.Repeat(filledchar, (int)value)); //the currently filled portion of the bar
            bar += pointerchar;
            bar += String.Concat(Enumerable.Repeat(emptychar, (int)maxvalue - (int)value)); //the empty portion of the bar
            bar += barsuffix + " "; //separator

            return bar; //send the finished thing back

        }
    }
}