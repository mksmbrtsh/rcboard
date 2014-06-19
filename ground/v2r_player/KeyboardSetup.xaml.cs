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

namespace v2r_player
{
    public partial class KeyboardSetup : Window
    {

        int currentelement;

        String[] keys = new String[Keyboard.TOTAL_KEYS];
        int[] channels = new int[Keyboard.TOTAL_CHANNELS];

        public KeyboardSetup()
        {
            InitializeComponent();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentelement == Keyboard.EMPTY) 
                return;
            
        }


        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

            if (currentelement == Keyboard.EMPTY)
                return;
            
            keys[currentelement] = e.Key.ToString();

            if (currentelement == Keyboard.UP)
            {
                button_up.Content = e.Key.ToString();
                keys[Keyboard.UP] = e.Key.ToString();
            }

            if (currentelement == Keyboard.LEFT)
            {
                button_left.Content = e.Key.ToString();
                keys[Keyboard.LEFT] = e.Key.ToString();
            }
            if (currentelement == Keyboard.RIGHT)
            {
                button_right.Content = e.Key.ToString();
                keys[Keyboard.RIGHT] = e.Key.ToString();
            }
            if (currentelement == Keyboard.DOWN)
            {
                button_down.Content = e.Key.ToString();
                keys[Keyboard.DOWN] = e.Key.ToString();
            }
            if (currentelement == Keyboard.FORSAGE)
            {
                button_forsage.Content = e.Key.ToString();
                keys[Keyboard.FORSAGE] = e.Key.ToString();
            }
            if (currentelement == Keyboard.SPEEDUP)
            {
                button_speedup.Content = e.Key.ToString();
                keys[Keyboard.SPEEDUP] = e.Key.ToString();
            }
            if (currentelement == Keyboard.SPEEDDOWN)
            {
                button_speeddown.Content = e.Key.ToString();
                keys[Keyboard.SPEEDDOWN] = e.Key.ToString();
            }
            if (currentelement == Keyboard.ADD1)
            {
                button_add1.Content = e.Key.ToString();
                keys[Keyboard.ADD1] = e.Key.ToString();
            }
            if (currentelement == Keyboard.ADD2)
            {
                button_add2.Content = e.Key.ToString();
                keys[Keyboard.ADD2] = e.Key.ToString();
            }
            if (currentelement == Keyboard.ADD3)
            {
                button_add3.Content = e.Key.ToString();
                keys[Keyboard.ADD3] = e.Key.ToString();
            }
            if (currentelement == Keyboard.ADD4)
            {
                button_add4.Content = e.Key.ToString();
                keys[Keyboard.ADD4] = e.Key.ToString();
            }

            currentelement = Keyboard.EMPTY;

            ShowMessage(false);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.UP;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.LEFT;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.RIGHT;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.DOWN;
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.SPEEDDOWN;
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.SPEEDUP;
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.ADD1;
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.ADD2;
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.ADD3;
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.ADD4;
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage(true);
            currentelement = Keyboard.FORSAGE;
        }


        private void ShowMessage(bool show)
        {
            message.Content = show ? "Нажмите любую клавишу на клавиатуре" : "";
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.keys.CopyTo(keys, 0);
            Keyboard.channels.CopyTo(channels, 0);

            ctrltype_car.IsChecked = (bool)!Status.keyboardctrltype;
            ctrltype_tank.IsChecked = (bool)Status.keyboardctrltype;

            button_up.Content = keys[Keyboard.UP];
            button_down.Content = keys[Keyboard.DOWN];
            button_left.Content = keys[Keyboard.LEFT];
            button_right.Content = keys[Keyboard.RIGHT];
            button_forsage.Content = keys[Keyboard.FORSAGE];
            button_speedup.Content = keys[Keyboard.SPEEDUP];
            button_speeddown.Content = keys[Keyboard.SPEEDDOWN];
            button_add1.Content = keys[Keyboard.ADD1];
            button_add2.Content = keys[Keyboard.ADD2];
            button_add3.Content = keys[Keyboard.ADD3];
            button_add4.Content = keys[Keyboard.ADD4];

            channel0.Text = channels[0].ToString();
            channel1.Text = channels[1].ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            channels[0] = Convert.ToInt32(channel0.Text);
            channels[1] = Convert.ToInt32(channel1.Text);

            keys.CopyTo(Keyboard.keys, 0);
            channels.CopyTo(Keyboard.channels, 0);

            Status.keyboardctrltype = (bool) ctrltype_tank.IsChecked; ;

            Keyboard.SaveConfig();
            this.Close();
        }

    }
}
