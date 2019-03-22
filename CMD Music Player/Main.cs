using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace CMD_Music_Player
{
    public static class Core
    {
        public static WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer(); //the heart of this program
        //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.textinfo.totitlecase?redirectedfrom=MSDN&view=netframework-4.7.2#System_Globalization_TextInfo_ToTitleCase_System_String_
        static TextInfo TextCaseConverter = new CultureInfo("en-GB", false).TextInfo;

        public static bool bypassfilecheck = false;
        public static void error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
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
                                           barwidth: Console.WindowWidth - 2,
                                           barsuffix: "] " + player.controls.currentPositionString + "/" + player.currentMedia.durationString);
        }
        static void SaveConfiguration()
        {
            Properties.Settings.Default.Save();
            success("Saved configuration!");
        }

        static StringCollection discoveredfiles = new StringCollection();
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            //attempt to read the list of registered folders from the stored configuration.
            try { int x = Properties.Settings.Default.RegisteredFolders.Count; }
            catch { Properties.Settings.Default.RegisteredFolders = new StringCollection(); } //error handling if the list is empty

            discoveredfiles = FileSearch.PerformScan(); //perform inital scan for media

            for (; ; )
            {
                Console.Write(">");
                string[] command = CommandLineToArgs(Console.ReadLine());
                if (command.Length == 0) continue;
                string commandupper = command[0].ToUpper();

                if (commandupper == "HELP")
                {
                    foreach (string line in new string[] { "List of commands:" ,
                                             "\tplay / pl",
                                             "\tstop / s",
                                             "\tpause / pa",
                                             "",
                                             "\tpos / ?",
                                             "\tgoto / g",
                                             "",
                                             "\tlist / ls",
                                             "\tmanage_folders / mf",
                                             "\thelp / h",
                                             "\tquit / q"})
                    { Console.WriteLine(line); }
                }
                else if (commandupper == "LIST" || commandupper == "LS")
                {
                    if (discoveredfiles.Count == 0)
                    {
                        error("No music is in your library. :(\nType \"manage_folders\" to add a folder containing music.");
                        continue;
                    }
                    Console.Write("Filename");
                    Console.Write(String.Concat(Enumerable.Repeat(" ", 50 - 8)));
                    Console.WriteLine("Artist name");
                    foreach (string file in discoveredfiles)
                    {
                        var media = player.newMedia(file);
                        string filename = TextCaseConverter.ToTitleCase(Path.GetFileName(file));

                        string line = "";
                        for (int i = 0; i <= 50; i++)
                        {
                            //print the first 20 characters of the song name and pad any extra space
                            try { line += filename[i]; }
                            catch { line += " "; }
                        }
                        line += "   "; //padding between title and authour
                        line += media.getItemInfo("Author");
                        Console.WriteLine(line);
                    }
                }
                else if (commandupper == "PLAY" || commandupper == "PL")
                {
                    if (command.Length == 1) { player.controls.play(); }
                    else
                    {
                        string medianame = command[1]; //get media name parameter
                        if (FileSearch.IsElementInList(medianame, discoveredfiles)) //check if the file exists
                        {
                            string filepath = FileSearch.FindMediaPath(medianame, discoveredfiles);
                            Console.WriteLine("Playing " + filepath + "...");
                            try
                            {
                                player.URL = filepath;
                                player.controls.play();
                            }
                            catch (Exception err) { error("Error playing file: " + err.Message); }
                        }
                        else { error("Error playing file: File doesn't exist."); }
                    }
                }
                else if (commandupper == "STOP" || commandupper == "S") { player.controls.stop(); }
                else if (commandupper == "PAUSE" || commandupper == "PA") { player.controls.pause(); }

                else if (commandupper == "POS" || commandupper == "?")
                {
                    if (IsMediaSelected()) Console.WriteLine(CreateBarFromMediaInfo());
                    else error("No media is loaded.");


                }
                else if (commandupper == "INFO" || commandupper == "I")
                {
                    if (player.currentMedia == null)
                    {
                        error("No media is loaded.");
                        continue;
                    }
                    for (int i = 0; i < player.currentMedia.attributeCount; i++)
                    {
                        var name = player.currentMedia.getAttributeName(i);
                        var value = player.currentMedia.getItemInfo(name);
                        if (value.Length == 0) continue;
                        Console.WriteLine(name + ": " + value);
                    }
                }
                else if (commandupper == "GOTO" || commandupper == "G")
                {
                    string syntax = "Syntax: [hh:][mm:]ss";
                    if (!IsMediaSelected())
                    {
                        error("No media is loaded.");
                        continue;
                    }
                    if (command.Length < 2)
                    {
                        error(syntax);
                        continue;
                    }

                    string[] strtimeargs = command[1].Split(Char.Parse(":"));
                    if (strtimeargs.Length < 1 || strtimeargs.Length > 3) error(syntax);
                    List<double> timeargs = new List<double>();

                    try
                    {
                        for (int i = strtimeargs.Length; i-- > 0;) //reverse it so it is now in the ss:mm:hh format. This makes it easier to process.
                        {
                            string item = strtimeargs[i];
                            if (item == "") { error(syntax); continue; }
                            timeargs.Add(Convert.ToDouble(item)); //double is bulletproof since it is a 64 bit number, so it will overflow after 5.38 millenia.
                        }
                    }
                    catch (Exception err) { error("Invalid time code: " + err.Message); continue; }
                    //parse into seconds
                    float seconds = 0;
                    int multiplier = 1; //multiply the item by this
                    foreach (int unit in timeargs)
                    {
                        seconds += unit * multiplier;
                        multiplier *= 60; //multiple the multipler by 60 (sequence is 1,60,3600)
                    }
                    //is this longer than the length of the song?
                    if (seconds > player.currentMedia.duration)
                    {
                        error("Timecode exceeds song length.");
                        continue;
                    }
                    //or is it smaller than 0 ? 
                    if (seconds < 0)
                    {
                        error("Timecode is below 0 seconds.");
                        continue;
                    }

                    //go to the position and draw a progress bar.
                    player.controls.currentPosition = seconds;
                    Console.WriteLine(CreateBarFromMediaInfo());
                }

                else if (commandupper == "MANAGE_FOLDERS" || commandupper == "MF")
                {
                    Console.WriteLine("Entering folder manager...\nType help for list of commands.");
                    for (; ; )
                    {
                        Console.Write(":");
                        command = CommandLineToArgs(Console.ReadLine());
                        if (command.Length == 0) continue;
                        commandupper = command[0].ToUpper();

                        if (commandupper == "HELP" || commandupper == "H")
                        {
                            foreach (string line in new string[] { "List of commands:" ,
                                             "\tlist / ls",
                                             "\tadd / a",
                                             "\tdel / d",
                                             "",
                                             "\thelp / h",
                                             "\texit / e",
                                             "Changes are saved upon executing the \"exit\" command."})
                            { Console.WriteLine(line); }
                        }
                        else if (commandupper == "LIST" || commandupper == "LS")
                        {
                            Console.WriteLine("List of registered folders:");
                            int folderid = 0;
                            foreach (string path in Properties.Settings.Default.RegisteredFolders)
                            {
                                Console.WriteLine("[" + folderid + "] " + path);
                                folderid++;
                            }
                            if (Properties.Settings.Default.RegisterAppFolder) Console.WriteLine("* " + AppDomain.CurrentDomain.BaseDirectory);
                        }
                        else if (commandupper == "ADD" || commandupper == "A")
                        {
                            //query for folderpath to add
                            Console.Write("Enter the full filepath to the folder below.\nMake sure to include the drive letter.\n?");
                            string folderpath = Console.ReadLine();
                            if (!(Directory.Exists(folderpath)))
                            {
                                error("Folder doesn't exist.");
                                continue;
                            }
                            //add the path if it doesn't already exist.
                            try
                            {
                                if (Properties.Settings.Default.RegisteredFolders.Contains(folderpath)) error("Folder already registered.");
                                else
                                {
                                    Properties.Settings.Default.RegisteredFolders.Add(folderpath);
                                    success("Successfully added folder!");
                                }
                            }
                            catch { error("Internal error."); }
                        }
                        else if (commandupper == "DEL" || commandupper == "D")
                        {
                            string userinput;
                            int folderid;
                            int i = 0;
                            //has the user already specified the folder id as an argument? if not, ask them for the id.
                            if (!int.TryParse(command[1], out int n))
                            {
                                Console.WriteLine("List of registered folders:");
                                foreach (string path in Properties.Settings.Default.RegisteredFolders)
                                {
                                    Console.WriteLine("[" + i + "] " + path);
                                    i++;
                                }
                                Console.Write("Enter folder id to de-register: ");
                                userinput = Console.ReadLine();
                            }
                            else userinput = command[1];
                            try { folderid = Convert.ToInt16(userinput); }
                            catch
                            {
                                error("Invalid input: Must be a number.");
                                continue;
                            }

                            if (folderid > Properties.Settings.Default.RegisteredFolders.Count - 1) error("Folder ID exceeds maximum ID.");
                            Properties.Settings.Default.RegisteredFolders.RemoveAt(folderid);
                            success("De-registered folder #" + folderid);
                        }
                        else if (commandupper == "EXIT" || commandupper == "E") break;
                        else { error("Command unknown. Type 'help' for a list of commands."); }
                    }
                    SaveConfiguration();
                    Console.WriteLine("Rescanning for media...");
                    discoveredfiles = FileSearch.PerformScan(); //perform another scan for media
                }
                else if (commandupper == "TOGGLE_FILECHECK" || commandupper == "TF")
                {
                    //just for fun: a hidden command to disable checking of existence of a file.
                    //this could allow for lots of possible input, such as http addresses pointing to media.

                    bypassfilecheck = !bypassfilecheck; //invert state
                    if (bypassfilecheck) warning("Warning: Filechecks have been disabled. This can result in unexpected behavior."); //obligatory warning message
                    else success("Filechecks enabled.");
                }
                else if (commandupper == "QUIT" || commandupper == "Q") { Environment.Exit(0); }
                else { error("Command unknown. Type 'help' for a list of commands."); }
            }
        }


        //https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs
        );
        public static string[] CommandLineToArgs(string commandLine)
        {
            var argv = CommandLineToArgvW(commandLine, out int argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}