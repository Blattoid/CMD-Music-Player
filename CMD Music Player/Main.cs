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
        static readonly TextInfo TextCaseConverter = new CultureInfo("en-GB", false).TextInfo;

        public static bool bypassfilecheck = false;
        static bool LoopEnabled = false;
        static StringCollection discoveredfiles = new StringCollection();
        private readonly static FileSearch fileSearch = new FileSearch();
        private readonly static HelperFunctions functions = new HelperFunctions();
        private readonly static ArduinoCharScreenInterface screen = new ArduinoCharScreenInterface();

        static void Main()
        {
            //add play state change event handler
            player.PlayStateChange += player_PlayStateChange;

            //attempt to read the list of registered folders from the stored configuration.
            try { int x = Properties.Settings.Default.RegisteredFolders.Count; }
            catch { Properties.Settings.Default.RegisteredFolders = new StringCollection(); } //error handling if the list is empty

            discoveredfiles = fileSearch.PerformScan(); //perform inital scan for media
            if (Properties.Settings.Default.ArduinoAutostart)  screen.Activate(); //autostart the screen if we need to
            for (; ; )
            {
                Console.Write(">");
                string[] command = CommandLineToArgs(Console.ReadLine());
                if (command.Length == 0) continue;
                string commandupper = command[0].ToUpper();

                if (commandupper == "HELP" || commandupper == "H")
                {
                    foreach (string line in new string[] { "List of commands:" ,
                                             "\tplay / pl",
                                             "\tstop / s",
                                             "\tpause / pa",
 "\tloop / l",
                                             "",
                                             "\tpos / ?",
                                             "\tgoto / g",
                                             "",
                                             "\tlist / ls",
                                             "\tmanage_folders / mf",
                                             "\tsettings / se",
                                             "\thelp / h",
                                             "\tquit / q"})
                    { Console.WriteLine(line); }
                }
                else if (commandupper == "LIST" || commandupper == "LS")
                {
                    if (discoveredfiles.Count == 0)
                    {
                        functions.Error("No music is in your library. :(\nType \"manage_folders\" to add a folder containing music.");
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
                        if (fileSearch.IsElementInList(medianame, discoveredfiles)) //check if the file exists
                        {
                            string filepath = fileSearch.FindMediaPath(medianame, discoveredfiles);
                            Console.WriteLine("Playing " + filepath + "...");
                            try
                            {
                                player.URL = filepath;
                                player.controls.play();
                            }
                            catch (Exception err) { functions.Error("Error playing file: " + err.Message); }
                        }
                        else { functions.Error("Error playing file: File doesn't exist."); }
                    }
                }
                else if (commandupper == "STOP" || commandupper == "S") { player.controls.stop(); }
                else if (commandupper == "PAUSE" || commandupper == "PA") { player.controls.pause(); }
                else if (commandupper == "LOOP" || commandupper == "L")
                {
                    LoopEnabled = !LoopEnabled; //toggle LoopEnabled.
                    player.settings.setMode("loop", LoopEnabled); //tell the player to loop the media.

                    //output result
                    Console.Write("Looping is ");
                    if (LoopEnabled) { Console.WriteLine("ENABLED."); }
                    else { Console.WriteLine("DISABLED."); }
                }

                else if (commandupper == "POS" || commandupper == "?")
                {
                    if (functions.IsMediaSelected()) Console.WriteLine(functions.CreateBarFromMediaInfo());
                    else functions.Error("No media is loaded.");
                }
                else if (commandupper == "INFO" || commandupper == "I")
                {
                    if (player.currentMedia == null)
                    {
                        functions.Error("No media is loaded.");
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
                    if (!functions.IsMediaSelected())
                    {
                        functions.Error("No media is loaded.");
                        continue;
                    }
                    if (command.Length < 2)
                    {
                        functions.Error(syntax);
                        continue;
                    }

                    string[] strtimeargs = command[1].Split(Char.Parse(":"));
                    if (strtimeargs.Length < 1 || strtimeargs.Length > 3)
                    {
                        functions.Error(syntax);
                        continue;
                    }

                    List<double> timeargs = new List<double>();
                    timeargs = functions.ConvertTimeargsToInt(strtimeargs);

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
                        functions.Error("Timecode exceeds song length.");
                        continue;
                    }
                    //or is it smaller than 0 ? 
                    if (seconds < 0)
                    {
                        functions.Error("Timecode is below 0 seconds.");
                        continue;
                    }

                    //go to the position and draw a progress bar.
                    player.controls.currentPosition = seconds;
                    Console.WriteLine(functions.CreateBarFromMediaInfo());
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
                                             "\tlist / l / ls",
                                             "\tadd \"[folderpath]\" / a \"[folderpath]\"",
                                             "\tdel [folderid] / d [folderid]",
                                             "\treg_app_dir / x",
                                             "",
                                             "\thelp / h",
                                             "\texit / e / q",
                                             "**Changes are saved upon executing the \"exit\" command**"})
                            { Console.WriteLine(line); }
                        }
                        else if (commandupper == "LIST" || commandupper == "L" || commandupper == "LS")
                        {
                            Console.WriteLine("List of registered folders:");
                            ListRegisteredFolders();
                            if (Properties.Settings.Default.RegisterAppFolder) Console.WriteLine("* " + AppDomain.CurrentDomain.BaseDirectory);
                        }
                        else if (commandupper == "ADD" || commandupper == "A")
                        {
                            string folderpath = "";
                            //check if a folder parameter was specified
                            if (command.Length > 1)
                            {
                                //there is - attempt to add it.
                                folderpath = command[1];
                                Console.WriteLine("Adding '" + folderpath + "'");
                            }
                            else
                            {
                                //no parameter was specified; query for folderpath to add
                                Console.Write("Enter the full filepath to the folder below.\nMake sure to include the drive letter.\n?");
                                folderpath = Console.ReadLine();
                            }

                            if (!(Directory.Exists(folderpath)))
                            {
                                functions.Error("Folder doesn't exist.");
                                continue;
                            }
                            //add the path if it doesn't already exist.
                            try
                            {
                                if (Properties.Settings.Default.RegisteredFolders.Contains(folderpath)) functions.Error("Folder already registered.");
                                else
                                {
                                    Properties.Settings.Default.RegisteredFolders.Add(folderpath);
                                    functions.Success("Successfully registered folder!");
                                }
                            }
                            catch { functions.Error("Internal error."); }
                        }
                        else if (commandupper == "DEL" || commandupper == "D")
                        {
                            string userinput = "";
                            int folderid;
                            //has the user already specified the folder id as an argument? if not, ask them for the id.
                            if (command.Length == 1)
                            {
                                ListRegisteredFolders();
                                Console.Write("Enter folder id to de-register: ");
                                userinput = Console.ReadLine();
                            }
                            else userinput = command[1];
                            try { folderid = Convert.ToInt16(userinput); }
                            catch
                            {
                                functions.Error("Invalid input: Must be a number.");
                                continue;
                            }

                            if (folderid > Properties.Settings.Default.RegisteredFolders.Count - 1) functions.Error("Folder ID exceeds maximum ID.");
                            Properties.Settings.Default.RegisteredFolders.RemoveAt(folderid);
                            functions.Success("De-registered folder #" + folderid);
                        }
                        else if (commandupper == "REG_APP_DIR" || commandupper == "X")
                        {
                            Console.Write("Use of the app directory for music is now ");
                            bool enabled = !Properties.Settings.Default.RegisterAppFolder;
                            Properties.Settings.Default.RegisterAppFolder = enabled;
                            if (enabled) functions.Success("enabled.");
                            else functions.Error("disabled.");
                        }

                        else if (commandupper == "EXIT" || commandupper == "E" || commandupper == "Q") break;
                        else { functions.Error("Command unknown. Type 'help' for a list of commands."); }
                    }
                    functions.SaveConfiguration();
                    Console.WriteLine("Rescanning for media...");
                    discoveredfiles = fileSearch.PerformScan(); //perform another scan for media
                }
                else if (commandupper == "SETTINGS" || commandupper == "SE")
                {
                    Console.WriteLine("Entering settings menu...\nType help for list of commands.");
                    for (; ; )
                    {
                        Console.Write(":");
                        command = CommandLineToArgs(Console.ReadLine());
                        if (command.Length == 0) continue;
                        commandupper = command[0].ToUpper();

                        if (commandupper == "HELP" || commandupper == "H")
                        {
                            foreach (string line in new string[] { "List of commands:" ,
                                             "\tset_serial_port / com",
                                             "\ttoggle_arduino / ta",
                                             "\tautostart_arduino / as",
                                             "\tset_back_colour / bc",
                                             "\tset_text_colour / tc",
                                             "",
                                             "\thelp / h",
                                             "\texit / e / q",
                                             "**Changes are saved upon executing the \"exit\" command**"})
                            { Console.WriteLine(line); }
                        }
                        else if (commandupper == "SET_SERIAL_PORT" || commandupper == "COM")
                        {
                            //for every com port, add it to the list of options.
                            Console.WriteLine("List of COM ports:");
                            List<int> comPorts = new List<int>();
                            foreach (string item in System.IO.Ports.SerialPort.GetPortNames())
                            {
                                comPorts.Add(Convert.ToInt16(item.Substring(3)));
                            }
                            comPorts.Sort();
                            foreach (int portid in comPorts)
                            {
                                Console.WriteLine("\tCOM" + portid);
                            }

                            Console.Write("Please select a COM port from the provided list: ");
                            try { Properties.Settings.Default.ArduinoPort = Convert.ToInt16(Console.ReadLine().Replace("COM","")); }
                            catch (Exception err)
                            {
                                functions.Error("Invalid input. " + err.Message);
                                continue;
                            }

                            //Properties.Settings.Default.ArduinoPort = ;

                        }
                        else if (commandupper == "TOGGLE_ARDUINO" || commandupper == "TA")
                        {
                            if (screen.serial.IsOpen)
                            {
                                screen.Enabled = !screen.Enabled; //invert state
                            }
                            else screen.Activate(); //initial activation.

                            //output change
                            Console.Write("Arduino interface is ");
                            if (screen.Enabled) functions.Success("enabled.");
                            else functions.Error("disabled.");
                        }
                        else if (commandupper == "AUTOSTART_ARDUINO" || commandupper == "AS")
                        {
                            Properties.Settings.Default.ArduinoAutostart = !Properties.Settings.Default.ArduinoAutostart; //invert state

                            //output change
                            Console.Write("Autostart of Arduino interface is ");
                            if (Properties.Settings.Default.ArduinoAutostart) functions.Success("enabled.");
                            else functions.Error("disabled.");
                        }
                        else if (commandupper == "SET_BACK_COLOUR" || commandupper == "BC") { }
                        else if (commandupper == "SET_BACK_COLOUR" || commandupper == "TC") { }

                        else if (commandupper == "EXIT" || commandupper == "E" || commandupper == "Q") break;
                        else { functions.Error("Command unknown. Type 'help' for a list of commands."); }
                    }
                }
                else if (commandupper == "TOGGLE_FILECHECK" || commandupper == "TF")
                {
                    //just for fun: a hidden command to disable checking of existence of a file.
                    //this could allow for lots of possible input, such as http addresses pointing to media.

                    bypassfilecheck = !bypassfilecheck; //invert state
                    if (bypassfilecheck) functions.Warning("Warning: Filechecks have been disabled. This can result in unexpected behavior."); //obligatory warning message
                    else functions.Success("Filechecks enabled.");
                }
                else if (commandupper == "QUIT" || commandupper == "Q" || commandupper == "E") { Environment.Exit(0); }
                else { functions.Error("Command unknown. Type 'help' for a list of commands."); }
            }
        }

        static void player_PlayStateChange(int newstate)
        {
            if (Core.player.currentMedia == null) return; //do NOT proceed if no media is selected

            /*  List of state codes and their meanings:
             *  0: Undefined
             *  1: Stopped
             *  2: Paused
             *  3: Playing
             *  4: ScanForward
             *  5: ScanReverse
             *  6: Buffering
             *  7: Waiting
             *  8: MediaEnded
             *  9: Transitioning
             *  10: Ready
             *  11: Reconnecting
             *  12: Last
             */

            //If we need to, inform the Arduino Interface to update track information.
            if (screen.Enabled) screen.UpdateTrackData();
        }
        private static void ListRegisteredFolders()
        {
            int folderid = 0;
            foreach (string path in Properties.Settings.Default.RegisteredFolders)
            {
                Console.WriteLine("[" + folderid + "] " + path);
                folderid++;
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