using System;
using System.IO.Ports;
using System.Threading;

/* This is designed for a 20x4 character LCD screen. Here is the layout with border to denote screen edges:
    +--------------------+
    | [SCROLLING TITLE]  |
    |--------------------|
    |      6:9/4:20      |
    |[======>           ]|
    +--------------------+
*/
namespace CMD_Music_Player
{
    class ArduinoCharScreenInterface
    {
        private static HelperFunctions functions = new HelperFunctions();

        private SerialPort serial = new SerialPort();
        private Thread ScreenTicker = null;
        public int SerialPort = 0; //ID of COM port that Arduino uses
        public bool Enabled = true; //allows the thread to be turned on and off without actually closing it

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
            serial.BaudRate = 9600; //ensure correct baud rate
            //disable hardware handshaking
            serial.RtsEnable = false;
            serial.DtrEnable = false;
            serial.Open(); //start communication

            ScreenTicker = new Thread(ScreenTick);
            ScreenTicker.Start();
        }
        private void ScreenTick()
        {
            for (; ; )
            {
                if (Enabled)
                {
                    serial.WriteLine("Data to send to Arduino goes here! :D");
                }
                Thread.Sleep(500); //delay between signals to update
            }
        }
    }
}