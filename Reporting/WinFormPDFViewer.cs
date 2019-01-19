using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChampionshipSolutions.ControlRoom
{
    internal partial class WinFormPDFViewer : Form
    {
        public WinFormPDFViewer()
        {
            InitializeComponent();
        }

        public WinFormPDFViewer(byte[] memory) : this ()
        {
            LoadPDF( memory );
        }

        public WinFormPDFViewer ( string filePath ) : this( )
        {
            LoadPDF( filePath );
        }

        public WinFormPDFViewer ( Stream memory ) : this( )
        {
            LoadPDF( memory );
        }

        public void LoadPDF(byte[] memory)
        {
            pdfViewer1.Document?.Dispose ( );

            try
            {
                pdfViewer1.Document = PdfDocument.Load ( new MemoryStream ( memory ) );
                Show( );
            }
            catch ( Exception ex)
            {
                MessageBox.Show ( ex.Message );
            }

        }

        public void LoadPDF(string filePath)
        {
            if ( File.Exists(filePath))
            {
                pdfViewer1.Document?.Dispose( );

                try
                {
                    pdfViewer1.Document = PdfDocument.Load( filePath );
                    Show( );
                }
                catch
                {
                    MessageBox.Show( "Failed to load PDF document" );
                }
            }
        }

        public void LoadPDF (Stream document)
        {
            if ( document != null )
            {
                pdfViewer1.Document?.Dispose( );

                try
                {
                    pdfViewer1.Document = PdfDocument.Load( document );
                    Show( );
                }
                catch 
                {
                    MessageBox.Show( "Failed to load PDF document" );
                }
            }
        }

    }
}
