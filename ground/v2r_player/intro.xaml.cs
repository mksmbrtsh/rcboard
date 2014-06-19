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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace v2r_player
{

    public partial class intro : Window
    {

        DispatcherTimer timer = new DispatcherTimer();

        public intro()
        {
            InitializeComponent();

            if (Status.LoadConfig() > 0)
            {
                MessageBox.Show("Не могу прочитать настройки из файла");
                this.Close();
            }

            /* check local GStreamer (in current directory) */
            string AppDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Status.gstfound = true;

            if (System.IO.File.Exists(AppDir + @"\gstreamer\lib\gstreamer-0.10\libgstrtp.dll"))
            {
                System.Environment.SetEnvironmentVariable("GST_PLUGIN_PATH", AppDir + @"\gstreamer\lib\gstreamer-0.10");
                System.Environment.SetEnvironmentVariable("GST_PLUGIN_SYSTEM_PATH", AppDir + @"\gstreamer\bin");

                string newenv = AppDir + @"\gstreamer\bin;" + AppDir + @"\gstreamer\lib\gstreamer-0.10";
                System.Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + newenv);
            }
            else
            {
                if (!System.IO.File.Exists(Environment.GetEnvironmentVariable("GST_PLUGIN_PATH") + @"\libgstrtp.dll"))
                {
                    MessageBox.Show("GStreamer не найден, видео будет недоступно");
                    Status.gstfound = false;
                }
            }

            /* must we write gstreamer log? */
            if (Status.gstlog)
            {
                System.Environment.SetEnvironmentVariable("GST_DEBUG", "*:" + Status.gstloglevel.ToString());
                System.Environment.SetEnvironmentVariable("GST_DEBUG_FILE", "gstreamer.log");
            }

            /* init selected joystick */

            try
            {
                Joystick.Init(Status.joystickname);
            }
            catch (Exception err)
            {
                //MessageBox.Show("Не могу инициализировать джойстик");
            }
            
            

            /* init keyboard */
            Keyboard.LoadConfig(Status.keyboardkeys, Status.keyboardchannels);

            Gst.Application.Init();

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            loadstatus.Text += Joystick.GetList(Status.joystickname);
            loadstatus.Text += DiscoverGstPlugins();

            /* run timer for delayed intro window closing */
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Start();

        }

        public void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            /* open main window */
            MainWindow mainwindow = new MainWindow();
            mainwindow.ShowInTaskbar = true;
            mainwindow.Show();

            /* close intro window */
            this.Close();
            
        }

        private String DiscoverGstPlugins()
        {
            String result = "";
            result += "размещение плагинов:\n" + Environment.GetEnvironmentVariable("GST_PLUGIN_PATH") + "\n";
            result += "размещение библиотек:\n" + Environment.GetEnvironmentVariable("GST_PLUGIN_SYSTEM_PATH") + "\n";

            if (Status.gstlog)
                result += "пишу лог (уровень " + Status.gstloglevel + ")\n";

            return result;
        }

    }
}
