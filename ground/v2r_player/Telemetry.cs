using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;

namespace v2r_player
{
    static class Telemetry
    {

        private static Label voltagelabel, connectlabel;
        private static string connectparams;
        private static float voltage;
        private static String client_ip;

        /* timers */
        private static DispatcherTimer aliveTimer;
        private static long aliveTimestamp;

        private static long lag;

        public static void Start()
        {
            aliveTimestamp = 0;

            aliveTimer = new DispatcherTimer();
            aliveTimer.Tick += new EventHandler(aliveTimer_Tick);
            aliveTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            aliveTimer.Start();

        }

        public static void ParseCommand(byte[] message)
        {

            String messagestring = String.Empty;
            string[] elements;

            switch (message[0])
            {

                case 0:

                    // alive
                    TimeSpan t = (DateTime.Now - new DateTime(1970, 1, 1));
                    long currentTimestamp = (long)t.TotalMilliseconds;

                    aliveTimestamp = currentTimestamp;

                    Network.connected = true;

                    break;

                case 1:
                    // voltage

                    float vol_int = message[1];
                    float vol_dec = (float)message[2];
                    voltage = vol_int + (vol_dec / 100);
                    break;

                case 2:
                    // Wi-Fi or 3G/4G  signal level

                    
                    switch (message[1])
                    {
                        case 1:
                            /* Wi-Fi */

                            for (int i = 3; i < message.Length; i++)
                                if (message[i]!=0) messagestring += ((char)message[i]).ToString();

                            elements = messagestring.Split(';');

                            connectparams = "Wi-Fi " + elements[0] + " " + elements[1] + " " + elements[2];
                            break;

                        case 2:
                            /* Yota */

                            for (int i = 3; i < message.Length; i++)
                                if (message[i] != 0) messagestring += ((char)message[i]).ToString();

                            //elements = messagestring.Split(';');

                            connectparams = "Yota " + messagestring;
                            break;

                        case 3:
                            /* LAN */

                            for (int i = 3; i < message.Length; i++)
                                if (message[i] != 0) messagestring += ((char)message[i]).ToString();

                            connectparams = "LAN ";
                            break;

                        case 4:
                            /* Ubiquity */

                            for (int i = 3; i < message.Length; i++)
                                if (message[i] != 0) messagestring += ((char)message[i]).ToString();

                            connectparams = "Ubiquity ";
                            break;
                    }

                    break;

            }

        }

        public static void ShowStatus()
        {
            voltagelabel.Content = voltage.ToString() + " В";
            
            connectlabel.Content = Network.connected ? "подключено" : "отключено";

            if (!Network.connected) {
                client_ip = "";
                connectparams = "";
            }
            
            connectlabel.Content += "  ( " + lag.ToString() + " мс )   " + connectparams + "   " + client_ip;
        }


        public static void SetNetworkInfo(String info) 
        {
            client_ip = info;
        }
        

        private static void aliveTimer_Tick(object sender, EventArgs e)
        {
            byte[] msg;
            msg = new byte[3];
            msg[0] = 0; // alive cmd id
            msg[1] = 1; // client can show videostream
            msg[2] = (byte) (msg[0] ^ msg[1]); // checksum
            Network.Send(msg, msg.Length);

            if (aliveTimestamp > 0)
            {

                TimeSpan t = (DateTime.Now - new DateTime(1970, 1, 1));
                long currentTimestamp = (long)t.TotalMilliseconds;
                long delta = currentTimestamp - aliveTimestamp;
                lag = delta;

                if (delta > Status.nettimeout)
                {
                    Network.connected = false;
                }
            }

            ShowStatus();

        }


        public static void SetControls(Label forvoltage, Label forconnection)
        {
            voltagelabel = forvoltage;
            connectlabel = forconnection;
        }


    }
}
