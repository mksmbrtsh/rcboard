using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

namespace v2r_player
{

    public partial class JoystickSetup : Window
    {

        Boolean ready = false;
        String savejoystick;


        /* LOCAL VARS FOR SETUP JOYSTICK */
        int[] axis = new int[Joystick.TOTAL_AXIS];
        int[] channels = new int[Joystick.TOTAL_CHANNELS];
        int[] inverses = new int[Joystick.TOTAL_CHANNELS];
        int[] components = new int[Joystick.TOTAL_COMPONENTS];


        int currentElement;

        public JoystickSetup()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            /* save current joystick name */
            savejoystick = Status.joystickname;

            /* load joysticks list */
            deviceList.ItemsSource = Joystick.GetList();
            deviceList.SelectedIndex = deviceList.Items.IndexOf(Status.joystickname);

            ready = true;

            if (!Joystick.isReady()) return;

            /* set up our new joystick timer handler */
            Joystick.SetListener(new EventHandler(timer_Tick));

            /* load channels from Joystick class */
            Joystick.channels.CopyTo(channels, 0);
            Joystick.inverses.CopyTo(inverses, 0);
            Joystick.components.CopyTo(components, 0);


            /* fill inverses */
            channel0_inv.IsChecked = inverses[0] > 0 ? true : false;
            channel1_inv.IsChecked = inverses[1] > 0 ? true : false;
            channel2_inv.IsChecked = inverses[2] > 0 ? true : false;
            channel3_inv.IsChecked = inverses[3] > 0 ? true : false;
            channel4_inv.IsChecked = inverses[4] > 0 ? true : false;
            channel5_inv.IsChecked = inverses[5] > 0 ? true : false;
            channel6_inv.IsChecked = inverses[6] > 0 ? true : false;
            channel7_inv.IsChecked = inverses[7] > 0 ? true : false;

            /* set ctrl type */
            ctrltype_car.IsChecked = (bool)!Joystick.ctrltype;
            ctrltype_tank.IsChecked = (bool)Joystick.ctrltype;

        }


        private void timer_Tick(object sender, EventArgs e)
        {
            UpdateJoystickState();
        }


        /* read joystick state */
        private void UpdateJoystickState()
        {
            if (!Joystick.isReady()) return;

            JoystickState j = Joystick.device.CurrentJoystickState;

            /* set axis values */
            field_X.Content = j.X.ToString();
            field_Y.Content = j.Y.ToString();
            field_Z.Content = j.Z.ToString();
            field_Rx.Content = j.Rx.ToString();
            field_Ry.Content = j.Ry.ToString();
            field_Rz.Content = j.Rz.ToString();
            field_VX.Content = j.VX.ToString();
            field_VY.Content = j.VY.ToString();
            field_VZ.Content = j.VZ.ToString();
            field_VRx.Content = j.VRx.ToString();
            field_VRy.Content = j.VRy.ToString();
            field_AX.Content = j.AX.ToString();
            field_AY.Content = j.AY.ToString();
            field_AZ.Content = j.AZ.ToString();
            field_ARx.Content = j.ARx.ToString();
            field_ARy.Content = j.ARy.ToString();
            field_ARz.Content = j.ARz.ToString();
            field_FX.Content = j.FX.ToString();
            field_FY.Content = j.FY.ToString();
            field_FZ.Content = j.FZ.ToString();
            field_FRx.Content = j.FRx.ToString();
            field_FRy.Content = j.FRy.ToString();
            field_FRz.Content = j.FRz.ToString();
            field_SL0.Content = j.Y.ToString();

            /* set two sliders fields */
            int[] extraAxis = j.GetSlider();
            axis[Joystick.SL0] = extraAxis[0];
            axis[Joystick.SL1] = extraAxis[1];
            field_SL0.Content = extraAxis[0].ToString();
            field_SL1.Content = extraAxis[1].ToString();

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
            buttonvalues.Content = buttonstate;

            /* fill axis and sliders array */
            axis[Joystick.X] = j.X;
            axis[Joystick.Y] = j.Y;
            axis[Joystick.Z] = j.Z;
            axis[Joystick.Rx] = j.Rx;
            axis[Joystick.Ry] = j.Ry;
            axis[Joystick.Rz] = j.Rz;
            axis[Joystick.VX] = j.VX;
            axis[Joystick.VY] = j.VY;
            axis[Joystick.VZ] = j.VZ;
            axis[Joystick.VRx] = j.VRx;
            axis[Joystick.VRy] = j.VRy;
            axis[Joystick.VRz] = j.VRz;
            axis[Joystick.AX] = j.AX;
            axis[Joystick.AY] = j.AY;
            axis[Joystick.AZ] = j.AZ;
            axis[Joystick.ARx] = j.ARx;
            axis[Joystick.ARy] = j.ARy;
            axis[Joystick.ARz] = j.ARz;
            axis[Joystick.FX] = j.FX;
            axis[Joystick.FY] = j.FY;
            axis[Joystick.FZ] = j.FZ;
            axis[Joystick.FRx] = j.FRx;
            axis[Joystick.FRy] = j.FRy;
            axis[Joystick.FRz] = j.FRz;

            /* calculate components sum */
            field_CMP0.Content = axis[components[0]];
            field_CMP1.Content = axis[components[1]];
            axis[Joystick.SUM0] = (Joystick.axis_max - Joystick.axis_min) / 2 + axis[components[0]] / 2 - axis[components[1]] / 2;
            field_SUM0.Content = axis[Joystick.SUM0];


            /* check axis array */
            int value;
            for (int i = 0; i < 8; i++)
            {
                value = 0;
                if (channels[i] != Joystick.EMPTY)
                {
                    value = (inverses[i] == 0) ? axis[channels[i]] : (Joystick.axis_max - axis[channels[i]]);

                    switch (i)
                    {
                        case 0:
                            channel0.Content = value.ToString();
                            break;
                        case 1:
                            channel1.Content = value.ToString();
                            break;
                        case 2:
                            channel2.Content = value.ToString();
                            break;
                        case 3:
                            channel3.Content = value.ToString();
                            break;
                        case 4:
                            channel4.Content = value.ToString();
                            break;
                        case 5:
                            channel5.Content = value.ToString();
                            break;
                        case 6:
                            channel6.Content = value.ToString();
                            break;
                        case 7:
                            channel7.Content = value.ToString();
                            break;
                    }
                    
                }
                                
            }


        }


        /* show axis menu */
        private void ShowMenu(int element_id) 
        {
            currentElement = element_id;
            wfHost.ContextMenu.PlacementTarget = wfHost;
            wfHost.ContextMenu.IsOpen = true;
        }                    

        
        private void Window_Closed(object sender, EventArgs e)
        {
            Joystick.Init(savejoystick);
        }


        /* commit changes and save config */
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Status.joystickname = savejoystick = deviceList.SelectedValue.ToString();

            channels.CopyTo(Joystick.channels, 0);
            inverses.CopyTo(Joystick.inverses, 0);
            components.CopyTo(Joystick.components, 0);

            /* set ctrl type */
            Joystick.ctrltype = (bool) ctrltype_tank.IsChecked;

            Joystick.SaveConfig();
            Status.SaveConfig();

            this.Close();
        }


        private void field_X_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.X);
        }

        private void field_Y_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.Y);
        }

        private void field_Z_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.Z);
        }

        private void field_Rx_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.Rx);
        }

        private void field_Ry_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.Ry);
        }

        private void field_Rz_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.Rz);
        }

        private void field_VX_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.VX);
        }

        private void field_VY_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.VY);
        }

        private void field_VZ_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.VZ);
        }

        private void field_VRx_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.VRx);
        }

        private void field_VRy_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.VRy);
        }

        private void field_VRz_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.VRz);
        }

        private void field_AX_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.AX);
        }

        private void field_AY_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.AY);
        }

        private void field_AZ_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.AZ);
        }

        private void field_ARx_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.ARx);
        }

        private void field_ARy_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.ARy);
        }

        private void field_ARz_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.ARz);
        }

        private void field_FX_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.FX);
        }

        private void field_FY_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.FY);
        }

        private void field_FZ_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.FZ);
        }

        private void field_FRx_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.FRx);
        }

        private void field_FRy_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.FRy);
        }

        private void field_FRz_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.FRz);
        }

        private void field_SL0_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.SL0);
        }

        private void field_SL1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.SL1);
        }

        private void field_SUM0_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu(Joystick.SUM0);
        }

        private void menuitem0_Click(object sender, RoutedEventArgs e)
        {
            channels[0] = currentElement;
        }
        private void menuitem1_Click(object sender, RoutedEventArgs e)
        {
            channels[1] = currentElement;
        }
        private void menuitem2_Click(object sender, RoutedEventArgs e)
        {
            channels[2] = currentElement;
        }
        private void menuitem3_Click(object sender, RoutedEventArgs e)
        {
            channels[3] = currentElement;
        }
        private void menuitem4_Click(object sender, RoutedEventArgs e)
        {
            channels[4] = currentElement;
        }
        private void menuitem5_Click(object sender, RoutedEventArgs e)
        {
            channels[5] = currentElement;
        }
        private void menuitem6_Click(object sender, RoutedEventArgs e)
        {
            channels[6] = currentElement;
        }
        private void menuitem7_Click(object sender, RoutedEventArgs e)
        {
            channels[7] = currentElement;
        }
        private void menuitem8_Click(object sender, RoutedEventArgs e)
        {
            components[0] = currentElement;
        }
        private void menuitem9_Click(object sender, RoutedEventArgs e)
        {
            components[1] = currentElement;
        }

        private void channel0_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[0] = channel0_inv.IsChecked == true ? 1 : 0;
        }
        private void channel1_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[1] = channel1_inv.IsChecked == true ? 1 : 0;
        }
        private void channel2_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[2] = channel2_inv.IsChecked == true ? 1 : 0;
        }
        private void channel3_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[3] = channel3_inv.IsChecked == true ? 1 : 0;
        }
        private void channel4_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[4] = channel4_inv.IsChecked == true ? 1 : 0;
        }
        private void channel5_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[5] = channel5_inv.IsChecked == true ? 1 : 0;
        }
        private void channel6_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[6] = channel6_inv.IsChecked == true ? 1 : 0;
        }
        private void channel7_inv_Click(object sender, RoutedEventArgs e)
        {
            inverses[7] = channel7_inv.IsChecked == true ? 1 : 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Joystick.TOTAL_CHANNELS; i++)
            {
                channels[i] = Joystick.EMPTY;
                inverses[i] = 0;
            }

            channel0_inv.IsChecked = false;
            channel1_inv.IsChecked = false;
            channel2_inv.IsChecked = false;
            channel3_inv.IsChecked = false;
            channel4_inv.IsChecked = false;
            channel5_inv.IsChecked = false;
            channel6_inv.IsChecked = false;
            channel7_inv.IsChecked = false;

            channel0.Content = "0";
            channel1.Content = "0";
            channel2.Content = "0";
            channel3.Content = "0";
            channel4.Content = "0";
            channel5.Content = "0";
            channel6.Content = "0";
            channel7.Content = "0";

        }

        
        private void DeviceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ready)
                return;

            /* skip empty */
            if (deviceList.SelectedValue == null)
                return;

            Joystick.Init(deviceList.SelectedValue.ToString());
            Joystick.Stop();
            Joystick.SetListener(new EventHandler(timer_Tick));
            Joystick.Start();

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }        


    }
}
