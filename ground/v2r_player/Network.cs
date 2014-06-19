using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;

using UPnP;


namespace v2r_player
{
    static class Network
    {
        

        private static bool role = false;

        /* timers */
        private static DispatcherTimer ReceiveTimer;


        /* UDP client */
        private static UdpClient udpClient = new UdpClient();
        private static IPEndPoint remoteEP;

        /* UDP server */
        private static IPEndPoint remoteIPEndPoint;
        private static Socket SrvSock;

        private static IPEndPoint sender;
        private static EndPoint Remote;

        public static bool connected;

        static ThreadStart upnp_proccess;
        static Thread childThread;


        /* open network settings window */
        static public void Setup()
        {
            NetworkSetup networkwindow = new NetworkSetup();
            networkwindow.ShowInTaskbar = false;
            networkwindow.ShowDialog();
        }

        static public void End()
        {
            /* remove firewall rules for video and commands stream */

            if (childThread != null)
                childThread.Abort();

            if (Status.netforwardhost == "")
                return;

            UPnP.NAT.Discover();

            /* list all local IP addresses */
            System.Net.IPAddress[] arr = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < arr.Length; i++)
            {
                if (IPAddress.Parse(arr[i].ToString()).AddressFamily == AddressFamily.InterNetwork)
                {
                    if (Status.netforwardhost != arr[i].ToString())
                        continue;

                    /* remove videostream rule */
                    UPnP.NAT.DeleteForwardingRule(arr[i].ToString(), Status.port, ProtocolType.Udp);

                    /* remove forward UPnP rule for incoming commands stream */
                    /* only in server mode */
                    if (role)
                    {
                        UPnP.NAT.DeleteForwardingRule(arr[i].ToString(), Status.netport, ProtocolType.Udp);
                    }

                }
            }

        }


        public static void Start(String host, int port, bool netrole)
        {
            role = netrole;

            /* check UPnP and add rules in new thread */
            if (Status.netforwardhost != "")
            {
                try
                {
                    upnp_proccess = new ThreadStart(CallUPnPStart);
                    childThread = new Thread(upnp_proccess);
                    childThread.Start();
                }
                catch (Exception err)
                {
                }
            }

            if (role)
            {
                /* start as server */
                StartServer(port);
            }
            else
            {
                /* start as client */
                StartClient(host, port);
            }


            ReceiveTimer = new DispatcherTimer();
            ReceiveTimer.Tick += new EventHandler(receiveTimer_Tick);
            ReceiveTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            ReceiveTimer.Start();

        }


        public static void StartClient(String host, int port)
        {
            IPAddress ipAddress;

            try
            {

                // Establish the remote endpoint for the socket
                ipAddress = IPAddress.Parse(host);
                remoteEP = new IPEndPoint(ipAddress, port);

                // init UDP connect
                udpClient.Connect(ipAddress, port);

            }
            catch (Exception e)
            {
            }

        }


        public static void CallUPnPStart()
        {
            /* check UPnP */
            UPnP.NAT.Discover();

            /* list all local IP addresses */
            System.Net.IPAddress[] arr = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < arr.Length; i++)
            {
                if (IPAddress.Parse(arr[i].ToString()).AddressFamily == AddressFamily.InterNetwork)
                {

                    if (Status.netforwardhost != arr[i].ToString())
                        continue;

                    /* add forward UPnP rule for incoming video stream */
                    UPnP.NAT.ForwardPort(arr[i].ToString(), Status.port, ProtocolType.Udp, "virt2real player stream");

                    /* add forward UPnP rule for incoming commands stream */
                    /* only in server mode */
                    if (role)
                    {
                        UPnP.NAT.ForwardPort(arr[i].ToString(), Status.netport, ProtocolType.Udp, "virt2real player commands ");
                    }

                }
            }
        }



        public static void StartServer(int port)
        {

            /* prevent multiple creations */
            if (SrvSock != null)
                return;

            remoteIPEndPoint = new IPEndPoint(IPAddress.Any, port);
            SrvSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            SrvSock.Bind(remoteIPEndPoint);

            sender = new IPEndPoint(IPAddress.Any, 0);
            Remote = (EndPoint)(sender);
                    
        }


        private static void receiveTimer_Tick(object sender, EventArgs e)
        {
            ReceiveMessage();
        }


        public static void ReceiveMessage()
        {
            try
            {

                if (role)
                {
                    // server

                    if (SrvSock.Available > 0)
                    {
                        byte[] content = new byte[1024];

                        int len = SrvSock.ReceiveFrom(content, ref Remote);
                        
                        if (len > 0)
                        {
                            Telemetry.ParseCommand(content);
                            Telemetry.SetNetworkInfo(Remote.ToString());
                        }
                    }

                }
                else
                {
                    // client

                    if (udpClient.Available > 0)
                    {
                        byte[] content = udpClient.Receive(ref remoteEP);

                        if (content.Length > 0)
                        {
                            Telemetry.ParseCommand(content);
                            Telemetry.SetNetworkInfo(remoteEP.ToString());
                        }
                    }
                
                }

            }
            catch (Exception e) { }

        }


        public static void Send(byte[] msg, int len)
        {
            try
            {
                if (role)
                {
                    // server
                    try
                    {
                        SrvSock.SendTo(msg, len, SocketFlags.None, Remote);
                    }
                    catch (Exception err) { }                        
                
                }
                else
                {
                    // client
                    try
                    {
                        udpClient.Send(msg, len);
                    }
                    catch (Exception err) { }
                    
                }

            } 
            catch (Exception e) { }

        }


        public static void SendCmdServerRestart()
        {
            byte[] msg;
            msg = new byte[5];
            msg[0] = (byte)5; // cmd id - system command 
            msg[1] = (byte)1; // value restart cmd server
            msg[2] = (byte)(msg[0] ^ msg[1]); // XOR checksum (hot' chto-to)
            Send(msg, msg.Length);
        }

        public static void SendChangeBitrate(long bitrate)
        {
            byte[] msg;
            msg = new byte[7];
            msg[0] = (byte)5; // cmd id - system command 
            msg[1] = (byte)2; // change bitrate command
            msg[2] = (byte)(bitrate & 0xff);
            msg[3] = (byte)((bitrate >> 8) & 0xff);
            msg[4] = (byte)((bitrate >> 16) & 0xff);
            msg[5] = (byte)((bitrate >> 24) & 0xff);
            msg[6] = (byte)(msg[0] ^ msg[1] ^ msg[2] ^ msg[3] ^ msg[4] ^ msg[5]); // XOR checksum (hot' chto-to)
            Send(msg, msg.Length);
        }

        public static void SendChangeMTU(long mtu)
        {
            byte[] msg;
            msg = new byte[7];
            msg[0] = (byte)5; // cmd id - system command 
            msg[1] = (byte)3; // change mtu command
            msg[2] = (byte)(mtu & 0xff);
            msg[3] = (byte)((mtu >> 8) & 0xff);
            msg[4] = (byte)((mtu >> 16) & 0xff);
            msg[5] = (byte)((mtu >> 24) & 0xff);
            msg[6] = (byte)(msg[0] ^ msg[1] ^ msg[2] ^ msg[3] ^ msg[4] ^ msg[5]); // XOR checksum (hot' chto-to)
            Send(msg, msg.Length);
        }

    }
}
