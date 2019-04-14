using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

/* This is designed for a 20x4 character LCD screen. Here is the layout with border to denote screen edges:
    +--------------------+
    |[SONG TITLE]        |
    |[SONG ARTIST]       |
    |      6:9/4:20      |
    |[======>           ]|
    +--------------------+


    -=List of accepted commands by Arduino=-
    Commands that take parameters will accept them on newlines.

   title: Sets the title of the song
   artist: Sets the artist of the song
   d: Sets the duration of the song.
   p: Sets the current position into a song
   u: Redraws LCD screen with set information.
*/
namespace CMD_Music_Player
{
    class ArduinoCharScreenInterface
    {
        private static readonly HelperFunctions functions = new HelperFunctions();

        private readonly SerialPort serial = new SerialPort();
        private Thread ScreenTicker = null;
        public int SerialPort = 0; //ID of COM port that Arduino uses
        public bool Enabled = false; //allows the thread to be turned on and off without actually closing it

        public void Activate()
        {
            //check if the screen is already activated.
            if (serial.IsOpen)
            {
                functions.Error("Screen is already activated.");
                return;
            }

            //setup serial port
            serial.PortName = "COM" + SerialPort; //e.g. COM4
            serial.BaudRate = 115200; //ensure correct baud rate
            //disable hardware handshaking
            serial.RtsEnable = false;
            serial.DtrEnable = false;

            try { serial.Open(); /*start communication*/}
            catch (IOException)
            {
                functions.Error("Serial port " + serial.PortName + " does not exist.\nAborting initialisation of Arduino interface.");
                return;
            }
            writeline("ver"); //retrieve the software version to check the Arduino is running what we expect
            if (!serial.ReadLine().Contains("Music Readout"))
            {
                functions.Error("Arduino is not running the expected sketch!\nPlease flash the Arduino with the provided sketch then retry.\nAborting Arduino interface.");
                return;
            }

            writeline("c"); //command to clear the screen of whatever was once on it

            ScreenTicker = new Thread(ScreenTick);
            ScreenTicker.Start();
            Enabled = true;
            functions.Success("Screen is successfully activated!");
        }

        public void UpdateTrackData()
        {
            if (Core.player.currentMedia == null)
            {
                functions.Error("Unable to update screen track data: no media is selected.\nIf you are seeing this: oops.");
                return;
            }

            writeline("title"); //command to update song title
            var value = Core.player.currentMedia.getItemInfo("title");
            writeline(value);

            writeline("artist"); //command to update song artist
            value = Core.player.currentMedia.getItemInfo("Author");
            writeline(value);

            string[] strtimeargs = Core.player.currentMedia.durationString.Split(char.Parse(":")); //retrieve and unpack durationString
            send(strtimeargs, "d"); //command 'd' for duration
        }

        private void ScreenTick()
        {
            for (; ; )
            {
                if (Enabled)
                {
                    if (Core.player.currentMedia == null) continue; //do NOT proceed if no media is selected

                    string[] strtimeargs = Core.player.controls.currentPositionString.Split(char.Parse(":")); //retrieve and unpack durationString
                    if (strtimeargs.Length == 0) strtimeargs = new string[] {"0", "0", "0"};
                    send(strtimeargs, "p"); //command 'p' for position
                }
                Thread.Sleep(300); //delay between signals to update
            }
        }
        private void send(string[] input, string command)
        {
            List<double> timeargs;
            timeargs = functions.ConvertTimeargsToInt(input);

            //pad any missing arguments with 0's
            for (int i = timeargs.Count; i < 3; i++) timeargs.Add(0);
            timeargs.Reverse(); //reverse it to the hh:mm:ss format

            //send data to the Arduino

            writeline(command);
            foreach (double i in timeargs) writeline(i.ToString());
        }
        private void writeline(string data)
        {
            try { serial.WriteLine(data); }
            catch (IOException)
            {
                functions.Error("Unable to write data to serial.");
                Enabled = false;
            }
        }
    }
}