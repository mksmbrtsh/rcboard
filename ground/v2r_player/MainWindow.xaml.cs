using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Gst;

namespace v2r_player
{

    public partial class MainWindow : Window
    {

        Gst.GLib.MainLoop m_GLibMainLoop;
        System.Threading.Thread m_GLibThread;
        Videostream stream;
        Overlay overlay;

        /* for window sizing */
        double window_top;
        double window_left;
        double window_width;
        double window_height;


        public MainWindow()
        {

            InitializeComponent();

            if (Status.gstfound)
            {

                /* Create a main loop for GLib, run it in a separate thread */
                m_GLibMainLoop = new Gst.GLib.MainLoop();
                m_GLibThread = new System.Threading.Thread(m_GLibMainLoop.Run);
                m_GLibThread.Name = "GLibMainLoop";
                m_GLibThread.Start();

                System.Threading.Thread.CurrentThread.Name = "virt2real player";

                /* create stream */
                stream = new Videostream(Status.port);

                stream.CreatePipeline();

                if (stream.ready)
                {
                    wfHost.Visibility = Visibility.Visible;
                    videoPanel.Visible = true;
                }

                /* end create stream */

            }

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            /* create overlay window */
            overlay = new Overlay();
            overlay.mainwindow = this;

            if (stream.ready)
            {
                stream.overlay = overlay;
                stream.SetOutput(videoPanel.Handle);
                stream.SetState(State.Playing);
            }

            menuitem9.IsChecked = Status.joystickactive;
            menuitem1.IsChecked = Status.topmost;
            this.Topmost = Status.topmost;
            menuitem4.IsChecked = Status.forceaspectratio;

            /* set new window width and height */
            if ((Status.window_width > 0) && (Status.window_height > 0))
            {
                this.Width = Status.window_width;
                this.Height = Status.window_height;
            }

            /* set new window position*/
            if (Status.window_top != -1) 
                this.Top = Status.window_top;
            if (Status.window_left != -1) 
                this.Left = Status.window_left;

            this.MouseDown += delegate
            {
                try
                {
                    WindowMove();
                }
                catch (Exception err) { }
            };

            /* set up and open overlay window */
            if (Status.overlay)
            {
                overlay.ShowInTaskbar = false;
                overlay.Show();
                menuitem6.IsChecked = Status.overlay;
            }

            overlay.SetPosition(this.Top, this.Left, this.Width, this.Height);

            /* make menu item inactive if joysticks are absent */
            menuitem7.IsEnabled = Joystick.isPresent();

            /* start joystick read */
            Joystick.overlay = overlay;
            Joystick.Start();


            /* fill controls for telemetry */
            Telemetry.SetControls(overlay.voltage, overlay.connectstatus);
            Telemetry.Start();


            /* start network */
            if (Status.netautoconnect)
            {
                Network.Start(Status.nethost, Status.netport, Status.netrole);
            }

            Keyboard.overlay = overlay;


        }


        private void videoPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (e.Y < 30)
                wfHost.Margin = new Thickness(0, 30, 0, 0);
            else
                wfHost.Margin = new Thickness(0, 0, 0, 0);

        }


        private void videoPanel_Click(object sender, EventArgs e)
        {
            ShowMenu();
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowMenu();
        }


        /* =============  main menu functions ========================================================== */

        private void ShowMenu()
        {
            wfHost.ContextMenu.PlacementTarget = wfHost;
            wfHost.ContextMenu.IsOpen = true;
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = menuitem1.IsChecked == true ? true : false;
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            stream.SetRecord(!stream.record_state);
        }

        private void MenuItem3_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem4_Click(object sender, RoutedEventArgs e)
        {
            stream.SetForceSeAspectRatio(menuitem4.IsChecked == true ? true : false);
        }

        private void MenuItem5_Click(object sender, RoutedEventArgs e)
        {
            /* open settings window */
            SettingsWindow settingswindow = new SettingsWindow(stream);
            settingswindow.Owner = this;
            settingswindow.ShowInTaskbar = false;
            settingswindow.ShowDialog();
        }

        private void MenuItem6_Click(object sender, RoutedEventArgs e)
        {
            if (menuitem6.IsChecked)
                overlay.Show();
            else
                overlay.Hide();
        }

        private void MenuItem7_Click(object sender, RoutedEventArgs e)
        {
            Joystick.Setup();
        }

        private void MenuItem9_Click(object sender, RoutedEventArgs e)
        {
            Status.joystickactive = menuitem9.IsChecked;
        }

        private void MenuItem8_Click(object sender, RoutedEventArgs e)
        {
            
            if (stream.GetState() == State.Playing)
                stream.SetState(State.Paused);
            else if (stream.GetState() == State.Paused)
                stream.SetState(State.Playing);
        }

        private void MenuItem10_Click(object sender, RoutedEventArgs e)
        {

            Keyboard.Setup();
        }

        private void MenuItem11_Click(object sender, RoutedEventArgs e)
        {

            Network.Setup();
        }

        private void MenuItem12_Click(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("Вы правда хотите перезапустить сервер команд?", "Подтверждение действия", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Network.SendCmdServerRestart();
            }

        }

        private void MenuItem13_Click(object sender, RoutedEventArgs e)
        {
            BoardSettings boardsettings = new BoardSettings();
            boardsettings.Owner = this;
            boardsettings.ShowInTaskbar = false;
            boardsettings.ShowDialog();
        }


        /* =============  main window move and resize events functions ========================================================== */

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            /* toggle windowed fullscreen mode */
            if (this.Width == SystemParameters.PrimaryScreenWidth)
            {
                this.Top = window_top;
                this.Left = window_left;
                this.Width = window_width;
                this.Height = window_height;
            }
            else
            {
                window_top = this.Top;
                window_left = this.Left;
                window_width = this.Width;
                window_height = this.Height;

                this.Top = 0;
                this.Left = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;
            }

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /* resize overlay window */
            if (overlay != null)
            {
                overlay.SetPosition(this.Top, this.Left, this.Width, this.Height);
            }
        }

        private void WindowMove(){
            
            DragMove();
            
            /* move overlay window */
            overlay.SetPosition(this.Top, this.Left, this.Width, this.Height);

        }

        /* =============  end main window move and resize functions ========================================================== */

        /* =============  main window key pressing functions ========================================================== */

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Keyboard.ParseKeys(e, true);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            Keyboard.ParseKeys(e, false);
        }

        /* =============  end main window key pressing functions ========================================================== */

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            overlay.Hide();
            wfHost.ContextMenu.IsOpen = false;
            this.Hide();

            /* stopping gstreamer */
            if (m_GLibMainLoop != null)
                m_GLibMainLoop.Quit();

            /* delete stream object */
            stream = null;

            /* close overlay */
            if (overlay != null) 
                overlay.Close();

            /* save main window size and position */
            Status.window_top = this.Top;
            Status.window_left = this.Left;
            Status.window_width = this.Width;
            Status.window_height = this.Height;

            Status.topmost = menuitem1.IsChecked == true ? true : false;
            Status.forceaspectratio = menuitem4.IsChecked == true ? true : false;
            Status.overlay = menuitem6.IsChecked == true ? true : false;
            Status.joystickactive = menuitem9.IsChecked == true ? true : false;

            /* save operational config at quit */
            Status.SaveOperatingConfig();

        }


        private void Window_Closed(object sender, EventArgs e)
        {

            Network.End();

        }


    }
}



