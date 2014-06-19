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
using System.Net;
using System.Net.Sockets;

namespace v2r_player
{

    public partial class NetworkSetup : Window
    {
        public NetworkSetup()
        {
            InitializeComponent();
        }

        private void socketrole_client_Click(object sender, RoutedEventArgs e)
        {
            host.IsEnabled = (bool) netrole_client.IsChecked;
        }

        private void socketrole_server_Click(object sender, RoutedEventArgs e)
        {
            host.IsEnabled = (bool) !netrole_server.IsChecked;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            host.Text = Status.nethost;
            port.Text = Status.netport.ToString();
            timeout.Text = Status.nettimeout.ToString();

            autoconnect.IsChecked = Status.netautoconnect;
            netrole_client.IsChecked = !Status.netrole;
            netrole_server.IsChecked = Status.netrole;

            host.IsEnabled = (bool)netrole_client.IsChecked;

            fwdlocaladdress.Items.Add("");

            /* list all local IP addresses */
            System.Net.IPAddress[] arr = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < arr.Length; i++)
            {
                if (IPAddress.Parse(arr[i].ToString()).AddressFamily == AddressFamily.InterNetwork)
                {
                    fwdlocaladdress.Items.Add(arr[i].ToString());
                }
            }

            fwdlocaladdress.SelectedIndex = fwdlocaladdress.Items.IndexOf(Status.netforwardhost);

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Status.nethost = host.Text;
            Status.netport = Convert.ToInt32(port.Text);
            Status.nettimeout = Convert.ToInt32(timeout.Text);
            Status.netautoconnect = (bool) autoconnect.IsChecked;
            Status.netrole = (bool) netrole_server.IsChecked;

            if (fwdlocaladdress.SelectedValue != null)
                Status.netforwardhost = fwdlocaladdress.SelectedValue.ToString();
            else
                Status.netforwardhost = "";

            Status.SaveConfig();
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Network.Start(host.Text, Convert.ToInt32(port.Text), (bool) netrole_server.IsChecked);
        }

        private void fwdlocaladdress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Status.netforwardhost = fwdlocaladdress.SelectedValue.ToString();
        }
    }
}
