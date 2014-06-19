using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

using System.Configuration;
using System.Collections.Specialized;

namespace v2r_player
{
    static class Status
    {
        static public Configuration config;

        static public bool gstfound;

        static public int blocksize;
        static public int port;
        static public int udp_buffer_size;
        static public bool do_timestamp;
        static public uint latency;
        static public bool drop;
        static public int mode;

        static public bool gstlog;
        static public int gstloglevel;

        static public String effect;
        static public String savefilename;

        static public bool topmost;
        static public bool forceaspectratio;

        static public double window_top, window_left, window_width, window_height;

        static public bool overlay;

        static public String joystickname;
        static public bool joystickactive;

        static public String keyboardkeys;
        static public String keyboardchannels;
        static public Boolean keyboardctrltype;

        static public String nethost;
        static public int netport;
        static public bool netrole;
        static public int nettimeout;
        static public bool netautoconnect;
		static public String netforwardhost;

        static public int LoadConfig() {

            try
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (ConfigurationErrorsException)
            {
                /* error reading config */
                return 1;
            }


            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.blocksize"))
                blocksize = Convert.ToInt32(config.AppSettings.Settings["stream.blocksize"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.port"))
                port = Convert.ToInt32(config.AppSettings.Settings["stream.port"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.udp-buffer-size"))
                udp_buffer_size = Convert.ToInt32(config.AppSettings.Settings["stream.udp-buffer-size"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.latency"))
                latency = Convert.ToUInt32(config.AppSettings.Settings["stream.latency"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.do-timestamp"))
                do_timestamp = Convert.ToBoolean(config.AppSettings.Settings["stream.do-timestamp"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.drop"))
                drop = Convert.ToBoolean(config.AppSettings.Settings["stream.drop"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.mode"))
                mode = Convert.ToInt32(config.AppSettings.Settings["stream.mode"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.forceaspectratio"))
                forceaspectratio = Convert.ToBoolean(config.AppSettings.Settings["stream.forceaspectratio"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.effect"))
                effect = config.AppSettings.Settings["stream.effect"].Value;

            if (ConfigurationManager.AppSettings.AllKeys.Contains("stream.savefilename"))
                savefilename = config.AppSettings.Settings["stream.savefilename"].Value;

            if (ConfigurationManager.AppSettings.AllKeys.Contains("gst. g"))
                gstlog = Convert.ToBoolean(config.AppSettings.Settings["gst.log"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("gst.loglevel"))
                gstloglevel = Convert.ToInt32(config.AppSettings.Settings["gst.loglevel"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("window.topmost"))
                topmost = Convert.ToBoolean(config.AppSettings.Settings["window.topmost"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("window.top"))
                window_top = Convert.ToDouble(config.AppSettings.Settings["window.top"].Value);
                
            if (ConfigurationManager.AppSettings.AllKeys.Contains("window.left"))
                window_left = Convert.ToDouble(config.AppSettings.Settings["window.left"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("window.width"))
                window_width = Convert.ToDouble(config.AppSettings.Settings["window.width"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("window.height"))
                window_height = Convert.ToDouble(config.AppSettings.Settings["window.height"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("window.overlay"))
                overlay = Convert.ToBoolean(config.AppSettings.Settings["window.overlay"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("joystick.name"))
                joystickname = config.AppSettings.Settings["joystick.name"].Value;

            if (ConfigurationManager.AppSettings.AllKeys.Contains("joystick.active"))
                joystickactive = Convert.ToBoolean(config.AppSettings.Settings["joystick.active"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("keyboard.keys"))
                keyboardkeys = config.AppSettings.Settings["keyboard.keys"].Value;

            if (ConfigurationManager.AppSettings.AllKeys.Contains("keyboard.channels"))
                keyboardchannels = config.AppSettings.Settings["keyboard.channels"].Value;

            if (ConfigurationManager.AppSettings.AllKeys.Contains("keyboard.ctrltype"))
                keyboardctrltype = Convert.ToBoolean(config.AppSettings.Settings["keyboard.ctrltype"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("network.host"))
                nethost = config.AppSettings.Settings["network.host"].Value;

            if (ConfigurationManager.AppSettings.AllKeys.Contains("network.port"))
                netport = Convert.ToInt32(config.AppSettings.Settings["network.port"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("network.role"))
                netrole = Convert.ToBoolean(config.AppSettings.Settings["network.role"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("network.autoconnect"))
                netautoconnect = Convert.ToBoolean(config.AppSettings.Settings["network.autoconnect"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("network.timeout"))
                nettimeout = Convert.ToInt32(config.AppSettings.Settings["network.timeout"].Value);

            if (ConfigurationManager.AppSettings.AllKeys.Contains("network.fwdhost"))
                netforwardhost = config.AppSettings.Settings["network.fwdhost"].Value;


            /* set default values if config empty */
            if (port == 0) 
                port = 3000;

            if (savefilename == "")
                savefilename = AppDomain.CurrentDomain.BaseDirectory + "stream.mkv";


            return 0;

        }

        public static void SaveConfig()
        {
            /* create empty fields */
            config.AppSettings.Settings.Add("stream.blocksize", "");
            config.AppSettings.Settings.Add("stream.port", "");
            config.AppSettings.Settings.Add("stream.udp-buffer-size", "");
            config.AppSettings.Settings.Add("stream.latency", "");
            config.AppSettings.Settings.Add("stream.do-timestamp", "");
            config.AppSettings.Settings.Add("stream.drop", "");
            config.AppSettings.Settings.Add("stream.mode", "");
            config.AppSettings.Settings.Add("stream.effect", "");
            config.AppSettings.Settings.Add("stream.savefilename", "");
            config.AppSettings.Settings.Add("gst.log", "");
            config.AppSettings.Settings.Add("gst.loglevel", "");
            config.AppSettings.Settings.Add("joystick.name", "");
            config.AppSettings.Settings.Add("network.host", "");
            config.AppSettings.Settings.Add("network.port", "");
            config.AppSettings.Settings.Add("network.role", "");
            config.AppSettings.Settings.Add("network.autoconnect", "");
            config.AppSettings.Settings.Add("network.timeout", "");
            config.AppSettings.Settings.Add("network.fwdhost", "");

            /* set config fields */
            config.AppSettings.Settings["stream.blocksize"].Value = blocksize.ToString();
            config.AppSettings.Settings["stream.port"].Value = port.ToString();
            config.AppSettings.Settings["stream.udp-buffer-size"].Value = udp_buffer_size.ToString();
            config.AppSettings.Settings["stream.latency"].Value = latency.ToString();
            config.AppSettings.Settings["stream.do-timestamp"].Value = do_timestamp.ToString();
            config.AppSettings.Settings["stream.drop"].Value = drop.ToString();
            config.AppSettings.Settings["stream.mode"].Value = mode.ToString();
            config.AppSettings.Settings["stream.effect"].Value = effect;
            config.AppSettings.Settings["stream.savefilename"].Value = savefilename;
            config.AppSettings.Settings["gst.log"].Value = gstlog.ToString();
            config.AppSettings.Settings["gst.loglevel"].Value = gstloglevel.ToString();
            config.AppSettings.Settings["joystick.name"].Value = joystickname;
            config.AppSettings.Settings["network.host"].Value = nethost;
            config.AppSettings.Settings["network.port"].Value = netport.ToString();
            config.AppSettings.Settings["network.role"].Value = netrole.ToString();
            config.AppSettings.Settings["network.autoconnect"].Value = netautoconnect.ToString();
            config.AppSettings.Settings["network.timeout"].Value = nettimeout.ToString();
            config.AppSettings.Settings["network.fwdhost"].Value = netforwardhost;

            /* saving config */
            try
            {
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Не могу сохранить настройки в файл");
            }

        }


        public static void SaveOperatingConfig()
        {
            /* create empty fields */
            config.AppSettings.Settings.Add("stream.forceaspectratio", "");
            config.AppSettings.Settings.Add("window.topmost", "");
            config.AppSettings.Settings.Add("window.top", "");
            config.AppSettings.Settings.Add("window.left", "");
            config.AppSettings.Settings.Add("window.width", "");
            config.AppSettings.Settings.Add("window.height", "");
            config.AppSettings.Settings.Add("window.overlay", "");
            config.AppSettings.Settings.Add("joystick.active", "");
            config.AppSettings.Settings.Add("keyboard.keys", "");
            config.AppSettings.Settings.Add("keyboard.channels", "");
            config.AppSettings.Settings.Add("keyboard.ctrltype", "");

            /* set config fields, only operating values (not from config panel) */
            config.AppSettings.Settings["stream.forceaspectratio"].Value = forceaspectratio.ToString();
            config.AppSettings.Settings["window.topmost"].Value = topmost.ToString();
            config.AppSettings.Settings["window.top"].Value = window_top.ToString();
            config.AppSettings.Settings["window.left"].Value = window_left.ToString();
            config.AppSettings.Settings["window.width"].Value = window_width.ToString();
            config.AppSettings.Settings["window.height"].Value = window_height.ToString();
            config.AppSettings.Settings["window.overlay"].Value = overlay.ToString();
            config.AppSettings.Settings["joystick.active"].Value = joystickactive.ToString();
            config.AppSettings.Settings["keyboard.keys"].Value = keyboardkeys;
            config.AppSettings.Settings["keyboard.channels"].Value = keyboardchannels;
            config.AppSettings.Settings["keyboard.ctrltype"].Value = keyboardctrltype.ToString();

            /* saving config */
            try
            {
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Не могу сохранить настройки в файл");
            }

        }


    }


    public class FileParts
    {
        String fullfilename;

        public FileParts(String path)
        {
            fullfilename = path;
        }

        public String GetFullName()
        {

            String[] pathparts = fullfilename.Split('\\');
            if (pathparts.Length > 0)
            {
                return pathparts[pathparts.Length - 1];
            }

            return "";
        }

        public String GetName()
        {
            String fullname = GetFullName();

            String[] filenameparts = fullname.Split('.');
            return filenameparts[0];

        }


        public String GetExtension()
        {
            String fullname = GetFullName();

            String[] filenameparts = fullname.Split('.');
            String realfilename = filenameparts[0];
            String restfilename = "";
            if (filenameparts.Length > 1)
            {
                for (int i = 1; i < filenameparts.Length; i++)
                    restfilename += '.' + filenameparts[i];

                return restfilename;
            }

            return "";
        }

        public String GetPath()
        {
            String[] pathparts = fullfilename.Split('\\');
            if (pathparts.Length > 0)
            {
                string completefilename = pathparts[pathparts.Length - 1];
                return fullfilename.Replace(completefilename, "");
            }

            return "";

        }

    }

}
    