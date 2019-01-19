using Syncfusion.Windows.PdfViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for PDFViewer.xaml
    /// </summary>
    public partial class PDFViewer : Window
    {
        /// <summary>
        /// Opens a new preview window
        /// </summary>
        /// <param name="File">
        /// Supports PDFs in these formats 
        /// string - file path
        /// string[] - multiple file paths
        /// stream - single file stream
        /// stream[] - multiple file stream
        /// byte[] - single file as a byte array
        ///</param>
        public static void OpenOnSTAThread ( object FilePath )
        {
            ( (App)App.Current ).EventPage.Dispatcher.Invoke ( delegate ( )
            {
                new PDFViewer ( FilePath ).Show ( );
            } );
        }


        private object data;
        private PDFViewer ( )
        {
            InitializeComponent ( );
        }

        /// <summary>
        /// Opens a new preview window
        /// </summary>
        /// <param name="File">
        /// Supports PDFs in these formats 
        /// string - file path
        /// string[] - multiple file paths
        /// stream - single file stream
        /// stream[] - multiple file stream
        /// byte[] - single file as a byte array
        ///</param>
        private PDFViewer ( object File ) : this ()
        {
            data = File;
        }


        private void Window_Loaded ( object sender , RoutedEventArgs e )
        {
            if ( data == null )
                return;

            try
            {
                if ( data is string )
                {
                    pdfviewer1.Load( (string) data );
                }
                else if ( data is Stream )
                {
                    pdfviewer1.Load( (Stream) data );
                }
                else if ( data is byte [ ] )
                {
                    pdfviewer1.Load( new MemoryStream( (byte [ ]) data ) );
                }
                else if ( data is string [ ] )
                {
                    string [] files = (string[])data;

                    if ( files.Count( ) == 0 )
                        return;
                    else if ( files.Count( ) == 1 )
                        pdfviewer1.Load( files [ 0 ] );
                    else
                    {
                        pdfviewer1.Load( files [ 0 ] );
                        Tab1.Header = "PDF 1";

                        for ( int i = 1 ; i < files.Count( ) ; i++ )
                        {
                            TabItem ti = new TabItem ( ) { Header = "PDF " + ( i + 1 ) };
                            tabControl.Items.Add( ti );
                            PdfViewerControl pdfControl = new PdfViewerControl ( );
                            pdfControl.Load( files [ i ] );
                            ti.Content = pdfControl;
                        }
                    }
                }
                else if ( data is Stream [ ] )
                {
                    Stream [] files = (Stream[])data;

                    pdfviewer1.Load( files [ 0 ] );
                    Tab1.Header = "PDF 1";

                    for ( int i = 1 ; i < files.Count( ) ; i++ )
                    {
                        TabItem ti = new TabItem ( ) { Header = "PDF " + ( i + 1 ) };
                        tabControl.Items.Add( ti );
                        PdfViewerControl pdfControl = new PdfViewerControl ( );
                        pdfControl.Load( files [ i ] );
                        ti.Content = pdfControl;
                    }
                }
                else
                {
                    throw new ArgumentException( "Unknown format" );
                }
            }
            catch
            {
                MessageBox.Show( "Failed to load PDF, unknown data type." );
            }
        }
    }
}
