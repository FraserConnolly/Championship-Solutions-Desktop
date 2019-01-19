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

using ChampionshipSolutions.DM;
using System.IO;
using Microsoft.Win32;
using static ChampionshipSolutions.FileIO.FConnFile;

namespace ChampionshipSolutions.Scanning
{
    /// <summary>
    /// Interaction logic for ImageDisplay.xaml
    /// </summary>
    public partial class ImageDisplay : Window
    {
        private FileStorage File;

        public ImageDisplay()
        {
            InitializeComponent();
        }

        public ImageDisplay(FileStorage fs) : this()
        {
            File = fs;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to permanently delete this image?", "Delete?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.File.Event.RemoveFile ( File );

                this.Close();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
         SaveFileDialog sfd = new SaveFileDialog();
   
            sfd.FileName = File.Name + "." + File.Extension;

            Nullable<bool> result = sfd.ShowDialog();

            if ( result == true )
            {
                // save the file.
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                fs.Write(File.FileData,0,File.FileData.Count());
                fs.Close();
            }

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (File == null) this.Close();

            if (File.Extension == "bmp")
            {
                // display the file.

                MemoryStream memory = new MemoryStream(File.FileData);

                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                imgHolder.Source = bitmapImage;

            }
        }

        private void imgHolder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var st = (ScaleTransform)((TransformGroup)imgHolder.RenderTransform).Children.First(tr => tr is ScaleTransform);

            double zoom = e.Delta > 0 ? .2 : -.2;

            st.ScaleX += zoom;
            st.ScaleY += zoom;
        }

        Point start;
        Point origin;

        private void imgHolder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            imgHolder.CaptureMouse();

            var tt = (TranslateTransform)((TransformGroup)imgHolder.RenderTransform).Children.First(tr => tr is TranslateTransform);

            start = e.GetPosition(this.mainGrid);

            origin = new Point(tt.X, tt.Y);
        }

        private void imgHolder_MouseMove(object sender, MouseEventArgs e)
        {
            if (imgHolder.IsMouseCaptured)
            {
                var tt = (TranslateTransform)((TransformGroup)imgHolder.RenderTransform).Children.First(tr => tr is TranslateTransform);

                Vector v = start - e.GetPosition(this.mainGrid);

                tt.X = origin.X - v.X;
                tt.Y = origin.Y - v.Y;
            }
        }

        private void imgHolder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imgHolder.ReleaseMouseCapture();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
