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
    public partial class Overlay : Window
    {

        public MainWindow mainwindow;

        public Overlay()
        {
            InitializeComponent();
            speedbox.Width = 0;

        }

		public void SetPosition(double top, double left, double width, double height)
		{
			this.Top = top;
			this.Left = left;
			this.Width = width;
			this.Height = height;
			this.Topmost = true;


            /* scale bottom panel */
            panel1.Margin = new Thickness(20, height - 50, 20, 0);
            panel1.Width = width - 40;

            /* scale arrow image */
            keyarrow.Margin = new Thickness(width - 60, 30, 0, 0);
        
            /* scale speed box */
            speedbox.Margin = new Thickness(width - 60, 64, 0, 0);

            /* scale jitterbuffer */
            jitterbuffer.Margin = new Thickness(width - 70, height - 60, 0, 0);
        }


        public void ShowArrow(double angle)
        {
            RotateTransform rt = new RotateTransform(angle, keyarrow.Width / 2, keyarrow.Height / 2);
            keyarrow.RenderTransform = rt;
            rt = null;
        }

        public void ShowSpeed(double value)
        {
            speedbox.Width = value * keyarrow.Width;
        }

        public void ShowRec(bool state, String filename)
        {
            mainwindow.recordfilename.Content = filename;
            recordlabel.Visibility = state ? Visibility.Visible : Visibility.Hidden;
        }

        public void ShowInfo(String info)
        {
            infolabel.Content = info;
        }

        public void ShowBuffer(int value)
        {
            jitterbuffer.Value = value;
        }

    }

}
