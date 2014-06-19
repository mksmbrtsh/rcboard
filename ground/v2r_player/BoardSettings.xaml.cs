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

    public partial class BoardSettings : Window
    {
        public BoardSettings()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Network.SendChangeBitrate(Convert.ToInt32(bitrate.Text));
            this.Close();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Network.SendChangeMTU(Convert.ToInt32(mtu.Text));            
            this.Close();
        }
    }
}
