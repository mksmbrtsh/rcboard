using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Newtonsoft.Json.Linq;

namespace v2r_player
{
    static class Keyboard
    {

        public static int TOTAL_KEYS = 12;
        public static int TOTAL_CHANNELS = 2;

        public static int
            EMPTY = 0,
            UP = 1,
            DOWN = 2,
            LEFT = 3,
            RIGHT = 4,
            FORSAGE = 5,
            SPEEDUP = 6,
            SPEEDDOWN = 7,
            ADD1 = 8,
            ADD2 = 9,
            ADD3 = 10,
            ADD4 = 11;

        public static String[] keys = new String[TOTAL_KEYS];
        public static int[] channels = new int[TOTAL_CHANNELS];

        private static int keystate, buttonstate;
        private static int old_keystate, old_buttonstate;

        private static int currentspeed;
        private static int forsage_speed = Joystick.axis_middle;
        private static int save_forsage_speed;

        public static Overlay overlay;


        /* open keyboard settings window */
        public static void Setup()
        {
            KeyboardSetup keyboardwindow = new KeyboardSetup();
            keyboardwindow.ShowInTaskbar = false;
            keyboardwindow.ShowDialog();

        }


        public static void LoadConfig(String keystring, String keychannels)
        {

            /* load keys config */
            if (keystring == "") keystring = LoadDefaultKeysValues();

            String[] loadedkeys = keystring.Split(',');

            for (int i = 0; i < loadedkeys.Length; i++)
            {
                if (loadedkeys[i] == "")
                    continue;
                keys[i + 1] = loadedkeys[i];
            }

            /* load channels config */
            if (keychannels == "") keychannels = LoadDefaultChannelsValues();

            String[] loadedchannels = keychannels.Split(',');

            for (int i = 0; i < loadedchannels.Length; i++)
            {
                if (loadedchannels[i] == "")
                    continue;
                channels[i] = Convert.ToInt32(loadedchannels[i]);
            }

            /* set default speed for channel 1 (acelerate) */
            currentspeed = Joystick.axis_middle / 2;
            
        }


        private static String LoadDefaultKeysValues()
        {
            return "Up,Down,Left,Right,LeftCtrl,S,A,Z,X,C,V";
        }


        private static String LoadDefaultChannelsValues()
        {
            return "1,2";
        }


        public static void SaveConfig()
        {

            /* for keys */
            String tmp = "";
            for (int i = 1; i < keys.Length; i++)
            {
                if (keys[i] == "")
                    continue;
                tmp += keys[i] + ",";
            }

            /* for channels */
            String tmp1 = "";
            for (int i = 0; i < channels.Length; i++)
            {
                tmp1 += channels[i].ToString() + ",";
            }

            Status.keyboardkeys = tmp;
            Status.keyboardchannels = tmp1;
            Status.SaveOperatingConfig();

        }


        public static void ParseKeys(KeyEventArgs e, bool state)
        {

            if (e.Key.ToString() == keys[Keyboard.UP])
                keystate = state ? keystate |= 1 << Keyboard.UP : keystate &= ~(1 << Keyboard.UP);
            if (e.Key.ToString() == keys[Keyboard.DOWN])
                keystate = state ? keystate |= 1 << Keyboard.DOWN : keystate &= ~(1 << Keyboard.DOWN);
            if (e.Key.ToString() == keys[Keyboard.LEFT])
                keystate = state ? keystate |= 1 << Keyboard.LEFT : keystate &= ~(1 << Keyboard.LEFT);
            if (e.Key.ToString() == keys[Keyboard.RIGHT])
                keystate = state ? keystate |= 1 << Keyboard.RIGHT : keystate &= ~(1 << Keyboard.RIGHT);


            /* add this keys into key byte and check speed setting change */
            if (e.Key.ToString() == keys[Keyboard.SPEEDUP]) {
                keystate = state ? keystate |= 1 << Keyboard.SPEEDUP : keystate &= ~(1 << Keyboard.SPEEDUP);
                currentspeed += 5;
                if (currentspeed >= Joystick.axis_middle)
                    currentspeed = Joystick.axis_middle;
            } else 
            if (e.Key.ToString() == keys[Keyboard.SPEEDDOWN])
            {
                keystate = state ? keystate |= 1 << Keyboard.SPEEDDOWN : keystate &= ~(1 << Keyboard.SPEEDDOWN);
                currentspeed -= 5;
                if (currentspeed < Joystick.axis_min)
                    currentspeed = Joystick.axis_min;
            }


            if (e.Key.ToString() == keys[Keyboard.FORSAGE])
            {
                keystate = state ? keystate |= 1 << Keyboard.FORSAGE : keystate &= ~(1 << Keyboard.FORSAGE);

                if (state)
                {
                    save_forsage_speed = currentspeed;
                    currentspeed = forsage_speed;
                }
                else
                {
                    currentspeed = save_forsage_speed;
                }
            }

            
            /* make buttons channel byte */
            if (e.Key.ToString() == keys[Keyboard.ADD1])
                buttonstate = state ? keystate |= 1 << Keyboard.ADD1 : keystate &= ~(1 << Keyboard.ADD1);
            if (e.Key.ToString() == keys[Keyboard.ADD2])
                buttonstate = state ? keystate |= 1 << Keyboard.ADD2 : keystate &= ~(1 << Keyboard.ADD2);
            if (e.Key.ToString() == keys[Keyboard.ADD3])
                buttonstate = state ? keystate |= 1 << Keyboard.ADD3 : keystate &= ~(1 << Keyboard.ADD3);
            if (e.Key.ToString() == keys[Keyboard.ADD4])
                buttonstate = state ? keystate |= 1 << Keyboard.ADD4 : keystate &= ~(1 << Keyboard.ADD4);


            /* send the same command only once */
            if (keystate != old_keystate)
            {

                /* process axe 1 (wheel) */
                if ((keystate & (1 << Keyboard.LEFT)) > 0)
                    Joystick.SendAxis(channels[0] - 1, (keystate & (1 << Keyboard.LEFT)) > 0 ? Joystick.axis_middle + currentspeed  : Joystick.axis_middle, Status.keyboardctrltype);
                else
                if ((keystate & (1 << Keyboard.RIGHT)) > 0)
                    Joystick.SendAxis(channels[0] - 1, (keystate & (1 << Keyboard.RIGHT)) > 0 ? Joystick.axis_middle - currentspeed : Joystick.axis_middle, Status.keyboardctrltype);
                else
                    Joystick.SendAxis(channels[0] - 1, Joystick.axis_middle, Status.keyboardctrltype);


                /* process axe 2 (accelerate) */
                if ((keystate & (1 << Keyboard.UP)) > 0)
                    Joystick.SendAxis(channels[1] - 1, (keystate & (1 << Keyboard.UP)) > 0 ? Joystick.axis_middle - currentspeed : Joystick.axis_middle, Status.keyboardctrltype);
                else
                if ((keystate & (1 << Keyboard.DOWN)) > 0)
                    Joystick.SendAxis(channels[1] - 1, (keystate & (1 << Keyboard.DOWN)) > 0 ? Joystick.axis_middle + currentspeed : Joystick.axis_middle, Status.keyboardctrltype);
                else
                    Joystick.SendAxis(channels[1] - 1, Joystick.axis_middle, Status.keyboardctrltype);

                MakeArrow(keystate);
                overlay.ShowSpeed((double) currentspeed / Joystick.axis_middle);

                old_keystate = keystate;
            }


            /* send buttons channel command */
            if (buttonstate != old_buttonstate)
            {
                Joystick.SendButtons(buttonstate);
                old_buttonstate = buttonstate;
            }


        }

        
        private static void MakeArrow(int state)
        {
            switch (state)
            {
                default:
                case 0: 	// stop
                    overlay.ShowArrow(0);
                    break;
                case 2: 	// up
                    overlay.ShowArrow(0);
                    break;
                case 4: 	// down
                    overlay.ShowArrow(180);
                    break;
                case 8: 	// left
                    overlay.ShowArrow(-90);
                    break;
                case 16: 	// right
                    overlay.ShowArrow(90);
                    break;
                case 10: 	// up-left
                    overlay.ShowArrow(-45);
                    break;
                case 18: 	// up-right
                    overlay.ShowArrow(45);
                    break;
                case 12: 	// down-left
                    overlay.ShowArrow(-135);
                    break;
                case 20: 	// down-right
                    overlay.ShowArrow(135);
                    break;
            }

        }

    }
}
