using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Threading;

using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

using Newtonsoft.Json.Linq;

namespace v2r_player
{

    /* initial discover joysticks and make name list */
    static class Joystick
    {
        static public Device device;
        static List<string> devicelist = new List<string>();
        static JObject config;

        public static Overlay overlay;
        
        static private DispatcherTimer timer = new DispatcherTimer();

        static EventHandler oldhandler;

        /* CONSTANTS FOR JOYSTICK */

        static public int TOTAL_CHANNELS = 8;
        static public int TOTAL_AXIS = 28;
        static public int TOTAL_COMPONENTS = 2;

        static public int
            EMPTY = 0,
            X = 1,
            Y = 2,
            Z = 3,
            Rx = 4,
            Ry = 5,
            Rz = 6,
            VX = 7,
            VY = 8,
            VZ = 9,
            VRx = 10,
            VRy = 11,
            VRz = 12,
            AX = 13,
            AY = 14,
            AZ = 15,
            ARx = 16,
            ARy = 17,
            ARz = 18,
            FX = 19,
            FY = 20,
            FZ = 21,
            FRx = 22,
            FRy = 23,
            FRz = 24,
            SL0 = 25,
            SL1 = 26,
            SUM0 = 27;


        /* GLOBAL VARS FOR JOYSTICK */
        static public int[] axis = new int[TOTAL_AXIS];
        static public int[] channels = new int[TOTAL_CHANNELS];
        static public int[] finalchannels = new int[TOTAL_CHANNELS];
        static public int[] finalchannels_old = new int[TOTAL_CHANNELS];
        static public int[] inverses = new int[TOTAL_CHANNELS];
        static public int[] components = new int[TOTAL_COMPONENTS];
        static public bool ctrltype;

        static public int axis_min, axis_max, axis_middle, period;

        
        /* load joystick config from JSON file */
        static public void LoadConfig(String selecteddevicename)
        {

            StreamReader sr = null;
            String configfilename;
            String configfilecontent = "";
            try
            {
                configfilename = selecteddevicename.Replace(' ', '_') + ".json";
                sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + configfilename);
                configfilecontent = sr.ReadToEnd();
                sr.Close();
                config = JObject.Parse(configfilecontent);
            }
            catch (Exception err)
            {
                LoadDefaultValues();
            }

            
            /* fill values from config  */
            axis_min = Convert.ToInt32(config["axis_min"].ToString());
            axis_max = Convert.ToInt32(config["axis_max"].ToString());
            axis_middle = (axis_max - axis_min) / 2;
            period = Convert.ToInt32(config["period"].ToString());
            ctrltype = (Convert.ToInt32(config["type"].ToString()) > 0)? true : false;

            try
            {
                channels = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(config["channels"].ToString().Replace("\"", ""));
                inverses = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(config["inverses"].ToString().Replace("\"", ""));
                components = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(config["components"].ToString().Replace("\"", ""));
            }
            catch (Exception err)
            {

            }

        }

        static private void LoadDefaultValues()
        {
            config = JObject.Parse("{\"axis_max\":255,\"axis_min\":0,\"period\":20,\"type\":0,\"channels\":\"[25,1,0,0,0,0,0,0]\",\"inverses\":\"[0,0,0,0,0,0,0,0]\",\"components\":\"[0,0]\"}");
        }

        
        /* save current joystick configuration into JSON file */
        static public void SaveConfig()
        {

            string ch_json = Newtonsoft.Json.JsonConvert.SerializeObject(channels);
            string inv_json = Newtonsoft.Json.JsonConvert.SerializeObject(inverses);
            string comp_json = Newtonsoft.Json.JsonConvert.SerializeObject(components);
            config["type"] = ctrltype ? 1 : 0;
            config["channels"] = ch_json;
            config["inverses"] = inv_json;
            config["components"] = comp_json;

            /* save settings into file */
            String configfilename = "";
            try
            {
                configfilename = Status.joystickname.Replace(' ', '_') + ".json";
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + configfilename);
                sw.Write(config);
                sw.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show("Не могу сохранить настройки джойстика в файл " + configfilename);
            }


        }


        /* init selected joystick */
        static public void Init(String selecteddevicename)
        {
            LoadConfig(selecteddevicename);

            devicelist.Clear();

            foreach (DeviceInstance instance in Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
            {

                /* save all joysticks names into array */
                devicelist.Add(instance.ProductName.ToString());

                /* but init only one selected in config*/
                if (selecteddevicename != "")
                {
                    if (instance.ProductName != selecteddevicename) continue;
                }
                else
                {
                    /* if joystick name not set in config - use any present */
                    selecteddevicename = instance.ProductName;
                    Status.joystickname = instance.ProductName;
                }

                /* create joystick device */
                device = new Device(instance.InstanceGuid);

                device.SetCooperativeLevel(null, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);

                foreach (DeviceObjectInstance doi in device.Objects)
                {
                    if ((doi.ObjectId & (int)DeviceObjectTypeFlags.Axis) != 0)
                    {
                        device.Properties.SetRange(ParameterHow.ById, doi.ObjectId, new InputRange(axis_min, axis_max));
                    }

                }

                device.Acquire();

                SetDefaultListener();
                timer.Interval = new TimeSpan(0, 0, 0, 0, period);
            }

        }

        static public void SetDefaultListener()
        {
            SetListener(new EventHandler(timer_Tick));
        }

        /* set joystick timer listener */
        static public void SetListener(EventHandler handler)
        {
            /* remove old handler */
            if (oldhandler != null)
                timer.Tick -= oldhandler;
            
            /* save new handler */
            oldhandler = handler;

            /* set new handler */
            timer.Tick += handler;            
        }

        /* start joystick read timer */
        static public void Start()
        {
            timer.Start();
        }

        /* stop joystick read timer */
        static public void Stop()
        {
            timer.Stop();
        }


        /* check joystick device */
        static public Boolean isReady()
        {
            return device == null  ? false : true;
        }


        /* check joystick ready */
        /* return false if any joysticks absent and true - if present */
        static public Boolean isPresent()
        {
            return devicelist.Count > 0;
        }

        /* joystick read timer function */
        static private void timer_Tick(object sender, EventArgs e)
        {
            if (!Status.joystickactive) return;
            UpdateJoystickState();
        }

        
        /* read joystick state */
        static private void UpdateJoystickState()
        {
            if (device == null) return;

            JoystickState j = device.CurrentJoystickState;

            /* set two sliders fields */
            int[] extraAxis = j.GetSlider();
            axis[SL0] = extraAxis[0];
            axis[SL1] = extraAxis[1];

            /* fill axis and sliders array */
            axis[X] = j.X;
            axis[Y] = j.Y;
            axis[Z] = j.Z;
            axis[Rx] = j.Rx;
            axis[Ry] = j.Ry;
            axis[Rz] = j.Rz;
            axis[VX] = j.VX;
            axis[VY] = j.VY;
            axis[VZ] = j.VZ;
            axis[VRx] = j.VRx;
            axis[VRy] = j.VRy;
            axis[VRz] = j.VRz;
            axis[AX] = j.AX;
            axis[AY] = j.AY;
            axis[AZ] = j.AZ;
            axis[ARx] = j.ARx;
            axis[ARy] = j.ARy;
            axis[ARz] = j.ARz;
            axis[FX] = j.FX;
            axis[FY] = j.FY;
            axis[FZ] = j.FZ;
            axis[FRx] = j.FRx;
            axis[FRy] = j.FRy;
            axis[FRz] = j.FRz;


            /* set buttons */
            string buttonstate = "";
            byte[] buttons = j.GetButtons();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != 0)
                {
                    buttonstate += i + " ";
                }
            }


            /* calculate components sum */
            axis[Joystick.SUM0] = (Joystick.axis_max - Joystick.axis_min) / 2 + axis[components[0]] / 2 - axis[components[1]] / 2;


            /* check changed axis values*/
            for (int i = 0; i < TOTAL_CHANNELS; i++)
            {
                finalchannels[i] = (inverses[i] == 0) ? axis[channels[i]] : (axis_max - axis[channels[i]]);
                if (finalchannels[i] != finalchannels_old[i])
                {
                    finalchannels_old[i] = finalchannels[i];

                    /* some fun */
                    if (i == 0) overlay.ShowArrow((axis_max - axis_min) / 2 - finalchannels[0]);
                    if (i == 1) overlay.ShowSpeed((double) Math.Abs(Joystick.axis_middle - finalchannels[1]) / Joystick.axis_middle);


                    /* send to socket */
                    SendAxis(i, finalchannels[i], Joystick.ctrltype);

                }
            }


        }


        /* send axis values via network */
        public static void SendAxis(int channel, int value, bool type)
        {

            byte[] msg;
            msg = new byte[5];

            /* check ctrl type */
            msg[0] = type ? (byte) 2 : (byte) 1; // command "axis" type

            msg[1] = (byte)(channel & 0xFF); // command channel
            msg[2] = (byte)(value & 0xFF); // value low byte
            msg[3] = (byte)((value >> 8) & 0xFF); // // value high byte (for future use)
            msg[4] = (byte)(msg[0] ^ msg[1] ^ msg[2] ^ msg[3]); // XOR checksum (hot' chto-to)

            Network.Send(msg, msg.Length);
        }

        /* send buttons via network */
        public static void SendButtons(int value)
        {
            byte[] msg;
            msg = new byte[6];
            msg[0] = 2; // command "buttons" type
            msg[1] = (byte)(value & 0xFF); // value low byte
            msg[2] = (byte)((value >> 8) & 0xFF); // // value low byte (for future use)
            msg[3] = (byte)((value >> 16) & 0xFF); // // value high byte (for future use)
            msg[4] = (byte)((value >> 24) & 0xFF); // // value high byte (for future use)
            msg[5] = (byte)(msg[0] ^ msg[1] ^ msg[2] ^ msg[3] ^ msg[4]); // XOR checksum (hot' chto-to)
            Network.Send(msg, msg.Length);
        }


        /* open joystick settings dialog */
        static public void Setup()
        {
            /* open setup window */
            JoystickSetup setupwindow = new JoystickSetup();
            setupwindow.ShowInTaskbar = false;
            setupwindow.ShowDialog();
        }

        
        /* get all joysticks list in one string (for loader window) */
        static public List<string> GetList()
        {
            return devicelist;
        }


        /* get all joysticks list in one string (for loader window) */
        static public String GetList (String selecteddevicename) {

            String result = "";
            foreach (String devicename in devicelist)
            {
                if (devicename == "") continue;
                
                /* if it is a selected device */
                if (devicename == selecteddevicename)
                {
                    result += "* " + devicename + "\n";
                }
                else
                {
                    result += devicename + "\n";
                }

            }

            if (result == "") 
                result = "джойстики не обнаружены\n";
            else
                result = "обнаружены джойстики:\n" + result + "\n";

            return result;
        }
 
    }
}
