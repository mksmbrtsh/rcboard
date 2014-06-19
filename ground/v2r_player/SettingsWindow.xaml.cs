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
    
    public partial class SettingsWindow : Window
    {

        Videostream stream;

        public SettingsWindow(Videostream currentstream)
        {
            InitializeComponent();

            stream = currentstream;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

            blocksize.Text = Status.blocksize.ToString();
            port.Text = stream.GetCurrentPort().ToString();
            udp_buffer_size.Text = stream.GetCurrentBuffer().ToString();
            latency.Text = stream.GetCurrentLatency().ToString();
            dropframes.IsChecked = stream.GetCurrentDropFrames();
            dotimestamp.IsChecked = stream.GetCurrentDoTimestamp();
            gstlog.IsChecked = Status.gstlog;
            gstloglevel.Text = Status.gstloglevel.ToString();
            gstloglevel.IsEnabled = Status.gstlog;


            /* search item for default selection in effects field */
            /* list of effects need to be get from gstreamer inspect */

            string[] effectList = {
                            "",
                            "agingtv",
                            "quarktv",
                            "rippletv",
                        };

            foreach (string val in effectList)
            {
                effect.Items.Add(val);
            }

            for (int i = 0; i < effect.Items.Count; i++)
            {
                if (effect.Items.GetItemAt(i).ToString() == Status.effect)
                {
                    effect.SelectedIndex = i;
                    break;
                }
            }


            /* make items for rtpjitterbuffer selector */
            string[] modeList = {
                            "none",
                            "slave",
                            "buffer",
                        };

            foreach (string val in modeList)
            {
                jittermode.Items.Add(val);
            }

            jittermode.SelectedIndex = Status.mode;


            FileParts file = new FileParts(Status.savefilename);
            save_filepath.Text = file.GetPath();
            save_filename.Text = file.GetFullName();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            SaveSettings();
            this.Close();
        }

        private void ApplySettings()
        {
            stream.SetCurrentPort(Convert.ToInt32(port.Text));
            stream.SetCurrentBuffer(Convert.ToInt32(udp_buffer_size.Text));
            stream.SetCurrentLatency(Convert.ToUInt32(latency.Text));
            stream.SetCurrentDropFrames((bool)dropframes.IsChecked);
            stream.SetCurrentDropFrames((bool)dropframes.IsChecked);
            stream.SetJitterbufferMode(jittermode.SelectedIndex);
            stream.SetBlocksize(Convert.ToUInt32(blocksize.Text));

            Status.blocksize = Convert.ToInt32(blocksize.Text);
            Status.port = Convert.ToInt32(port.Text);
            Status.udp_buffer_size = Convert.ToInt32(udp_buffer_size.Text);
            Status.latency = Convert.ToUInt32(latency.Text);
            Status.drop = (bool)dropframes.IsChecked;
            Status.mode = jittermode.SelectedIndex;
            Status.effect = effect.SelectionBoxItem.ToString();
            Status.savefilename = save_filepath.Text + save_filename.Text;
            Status.gstlog = (bool)gstlog.IsChecked;
            Status.gstloglevel = Convert.ToInt32(gstloglevel.Text);
            Status.do_timestamp = (bool)dotimestamp.IsChecked;
        }

        private void SaveSettings()
        {

            Status.SaveConfig();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FileParts deffile = new FileParts(Status.savefilename);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = deffile.GetPath();
            dlg.FileName = deffile.GetName();
            dlg.DefaultExt = ".mkv";
            dlg.Filter = "Matroska (.mkv)|*.mkv";

            /* Show save file dialog box */
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                FileParts file = new FileParts(dlg.FileName);
                save_filepath.Text = file.GetPath();
                save_filename.Text = file.GetFullName();

            }
        }

        private void gstlog_Click(object sender, RoutedEventArgs e)
        {
            gstloglevel.IsEnabled = (bool)gstlog.IsChecked;
        }

    }
}
