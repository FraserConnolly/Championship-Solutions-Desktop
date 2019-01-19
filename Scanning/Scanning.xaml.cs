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
using System.Drawing;
using ChampionshipSolutions.ControlRoom.Scanning;
using System.IO;
using System.Collections;
using System.Drawing.Imaging;
using System.ComponentModel;
using ChampionshipSolutions.DM;
using static ChampionshipSolutions.FileIO.FConnFile;

namespace ChampionshipSolutions.Scanning
{
    /// <summary>
    /// Interaction logic for Scanning.xaml
    /// </summary>
    public partial class Scanning : Window
    {
        public Scanning()
        {
            InitializeComponent();
        }

        List<System.Drawing.Image> scannedImgs;
        FileStorage file;

        public void StartScanTheading()
        {

            if (cbxScanners.SelectedItem != null)
            {

                this.btnStartScan.IsEnabled = false;
                var bw = new BackgroundWorker();

                bw.DoWork += (sender, args) =>
                {
                    scannedImgs = WIAScanning.Scan((string)args.Argument);
                };

                bw.RunWorkerCompleted += bw_RunWorkerCompleted;
                bw.RunWorkerAsync(cbxScanners.SelectedItem.ToString());

            }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (scannedImgs != null)
            {
                if (scannedImgs.Count == 1)
                {
                    this.Dispatcher.Invoke( scanningFinished );
                }
            }

            this.btnStartScan.IsEnabled = true;
        }

        private ImageCodecInfo GetEncoder ( ImageFormat format )
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach ( ImageCodecInfo codec in codecs )
            {
                if ( codec.FormatID == format.Guid )
                {
                    return codec;
                }
            }
            return null;
        }

        private void scanningFinished()
        {

            MemoryStream memoryHigh = new MemoryStream();
            MemoryStream memoryLow = new MemoryStream();
            
            // rotate image
            scannedImgs.First().RotateFlip(RotateFlipType.Rotate90FlipNone);
            
            // convert to System.Media.Image
            scannedImgs.First().Save(memoryHigh, ImageFormat.Bmp);

            // URGENT To do compress to JPEG

            memoryHigh.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryHigh;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);


            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            Bitmap bit = new Bitmap(bitmapImage.StreamSource);

            bit.Save ( memoryLow , jgpEncoder , myEncoderParameters );

            imgHolder.Source = bitmapImage;

            ArrayList barcodes;
            System.Drawing.Bitmap bmp = new Bitmap(scannedImgs.First());
            barcodes = BarcodeScanning.ScanImgForBarcode(bmp);
            Championship Championship;
            AEvent Event;

            if (barcodes.Count == 1)
            {
                // successfully found a barcode
                // MessageBox.Show("Barcode found" + barcodes[0].ToString());
            

                if (parseBarcode(barcodes[0].ToString(), out Championship, out Event))
                {
                    file = new FileStorage(
                        Event.ShortName + " Result Card", 
                        "bmp",
                        memoryLow.ToArray());

                    //if (Event.Files == null)
                    //    Event.Files = new List<FileStorage>();

                    Event.AddFile(file);



                    //SaveChanges();
                    //MainWindow.SQLiteSubmit();
                    //MainWindow.Context.SaveChanges();

                    MessageBox.Show("Result card for " + Event.Name + " has been successfully stored");
                }
                else
                {
                    MessageBox.Show("Sorry, I could not understand the barcode on this sheet.\nPlease select an even from the next screen.");

                    SelectEvent se = new SelectEvent();

                    //se.cbxSelectEvent.ItemsSource = MainWindow.CurrentChampionship.listAllEvents();
                    se.cbxSelectEvent.ItemsSource = ((App)Application.Current).CurrentChampionship.Championship.Events;

                    if (se.ShowDialog() == true)
                    {
                        Event = se.SelectedEvent;

                        if (Event != null)
                        {
                            file = new FileStorage(
                                Event.ShortName + " Result Card",
                                "bmp",
                                memoryHigh.ToArray());

                            Event.AddFile(file);

                            //SaveChanges();
                            MessageBox.Show("Result card for " + Event.Name + " has been successfully stored");
                        }
                        else
                            MessageBox.Show("This scan has not been saved!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                        MessageBox.Show("This scan has not been saved!","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                }

            }
            else
            {
                MessageBox.Show("Two Barcodes were detected, please select an event from the next screen.");
                SelectEvent se = new SelectEvent();

                //se.cbxSelectEvent.ItemsSource = MainWindow.CurrentChampionship.listAllEvents();
                se.cbxSelectEvent.ItemsSource = ((App)Application.Current).CurrentChampionship.Events;

                if (se.ShowDialog() == true)
                {
                    Event = se.SelectedEvent;

                    if (Event != null)
                    {
                        //if (Event.Files == null)
                        //    Event.Files = new List<FileStorage>();

                        Event.AddFile(file);

                        //MainWindow.SQLiteSubmit();
                        //SaveChanges();
                        MessageBox.Show("Result card for " + Event.Name + " has been successfully stored");
                    }
                    else
                        MessageBox.Show("This scan has not been saved!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                    MessageBox.Show("This scan has not been saved!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private bool parseBarcode ( string Barcode, out Championship Championship , out AEvent Event)
        {

            Championship = null;
            Event = null;

            if (Barcode.Contains("C-") && Barcode.Contains("%E-"))
            {
                string champStr = Barcode.Split('%')[0];
                string eventStr = Barcode.Split('%')[1];

                champStr = champStr.TrimStart(new char[] { 'C', '-' });
                eventStr = eventStr.TrimStart(new char[] { 'E', '-' });

                Championship = GetFileDetails().IO.GetAll<Championship>().ToArray().Where(c => c.ShortName.ToUpper().Trim() == champStr.ToUpper().Trim()).FirstOrDefault();

                if (Championship != null)
                {
                    Event = Championship.Events.Where(e => e.ShortName.ToUpper().Trim() == eventStr).FirstOrDefault();
                    if (Event == null) return false;
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }

        }

        private void btnStartScan_Click_1(object sender, RoutedEventArgs e)
        {
            StartScanTheading();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            cbxScanners.ItemsSource = WIAScanning.GetDevices();

            if (cbxScanners.Items.Count == 1)
                cbxScanners.SelectedItem = cbxScanners.Items[0];
        }

        protected override void OnClosing ( System.ComponentModel.CancelEventArgs e )
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
