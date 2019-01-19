//#define Print_Using_Adobe_Reader

using GhostscriptSharp;
using GhostscriptSharp.Settings;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Printing;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
//using System.Windows.Forms;
using Syncfusion.Windows.PdfViewer;
using Syncfusion.Windows.Shared;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;
using System.Printing;
using System.Windows.Media.Imaging;

namespace ChampionshipSolutions
{
    public static class Printing
    {
        private static string printerName;

        public static string savePDFToTemp(PdfDocument pdfFile)
        {
            MemoryStream ms = new MemoryStream();
            byte[] b;
            if (pdfFile.Pages.Count != 0)
            {
                pdfFile.Save(ms);
                b = ms.ToArray();
            }
            else
            {
                b = null;
            }

            if (b == null)
                throw new ArgumentException("Could not generate PDF");


            return saveArrayToTemp(b);
        }

        public static string saveArrayToTemp(byte[] pdfFile, string extentions = null)
        {
            string FilePath = System.IO.Path.GetTempFileName();

            if (!string.IsNullOrWhiteSpace(extentions))
            {
                FilePath += "." + extentions;
            }

            if (pdfFile != null)
            {
                try
                {
                    FileStream fs = new FileStream(FilePath, FileMode.Create);
                    MemoryStream ms = new MemoryStream(pdfFile);
                    ms.WriteTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }


            return FilePath;
        }

        public static void GetPdfThumbnail(string sourcePdfFilePath, string destinationPngFilePath, int StartPage = 1, int EndPage = 1, GhostscriptPageSizes PageSize = GhostscriptPageSizes.a4)
        {
            // Use GhostscriptSharp to convert the pdf to a png
            GhostscriptWrapper.GenerateOutput(sourcePdfFilePath, destinationPngFilePath,
                new GhostscriptSettings
                {
                    Device = GhostscriptDevices.pngalpha,
                    Page = new GhostscriptPages
                    {
                        // Only make a thumbnail of the first page
                        Start = StartPage,
                        End = EndPage,
                        AllPages = false
                    },
                    Resolution = new Size
                    {
                        // Render at 72x72 dpi
                        Height = 300,
                        Width = 300
                    },
                    Size = new GhostscriptPageSize
                    {
                        // The dimensions of the incoming PDF must be
                        // specified. The example PDF is US Letter sized.
                        Native = PageSize
                    }
                }
            );
        }

        public static void PrintPDF(string filePath, string Printer = null)
        {

#if ( Print_Using_Adobe_Reader )
            // Define or otherwise determine the path of the Adobe reader
            PdfFilePrinter.AdobeReaderPath = @Properties.Settings.Default.AdobeReader;

            if (string.IsNullOrWhiteSpace(printerName))
            {

                // Present a Printer settings dialogue to the user so that they may select the printer
                // to use.
                PrinterSettings settings = new PrinterSettings();
                settings.Collate = false;

                PrintDialog printerDialog = new PrintDialog();
                printerDialog.AllowSomePages = false;
                printerDialog.ShowHelp = false;
                printerDialog.PrinterSettings = settings;
                printerDialog.AllowPrintToFile = true;
                printerDialog.PrinterSettings.PrintToFile = true;

                DialogResult result = printerDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    printerName = settings.PrinterName;
                }
                else
                {
                    return;
                }

            }

            // Print the document on the selected printer (We are ignoring all other print
            // options here

            PdfFilePrinter printer = new PdfFilePrinter(filePath, printerName);

            try
            {
                printer.Print();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
#else
            if ( !File.Exists ( filePath ) ) return;
            // This is a massive bodge!
            ( (App)App.Current ).EventPage.Dispatcher.Invoke ( delegate ( )
               {

                   PdfDocumentView pdfViewer1 =  new PdfDocumentView();


                   pdfViewer1.Load ( filePath );

                   //if ( !string.IsNullOrWhiteSpace ( Printer ) )
                   //    pdfViewer1.Print ( Printer );
                   //else
                   //    pdfViewer1.Print ( );

                   PrintDialog print = new PrintDialog();

                   if ( !string.IsNullOrWhiteSpace ( Printer ) )
                   {
                       PrintQueue t = print.PrintQueue;
                       try
                       {
                           PrintServer server = new PrintServer();
                           print.PrintQueue = server.GetPrintQueue ( Printer );
                       }
                       catch ( Exception )
                       {
                           System.Diagnostics.Debug.WriteLine ( "Failed to find printer" );
                           print.PrintQueue = t;
                       }
                   }

                   //BitmapSource img = pdfViewer1.ExportAsImage ( 1 );


                   print.PrintDocument ( pdfViewer1.PrintDocument.DocumentPaginator , "Championship Solutions" );


               } );
#endif
        }

        public static void PrintPDF(PdfDocument pdfDoc)
        {
            MemoryStream ms = new MemoryStream();
            byte[] b;
            if (pdfDoc.Pages.Count != 0)
            {
                pdfDoc.Save(ms);
                b = ms.ToArray();
            }
            else
            {
                b = null;
            }

            if (b == null)
                throw new ArgumentException("Could not generate PDF");

            PrintPDF(b);
        }
        
        public static void PrintPDF(byte[] pdfDoc)
        {

            //if (string.IsNullOrWhiteSpace(printerName))
            //{

            //    // Present a Printer settings dialogue to the user so that they may select the printer
            //    // to use.
            //    PrinterSettings settings = new PrinterSettings();
            //    settings.Collate = false;

            //    PrintDialog printerDialog = new PrintDialog();
            //    printerDialog.AllowSomePages = false;
            //    printerDialog.ShowHelp = false;
            //    printerDialog.PrinterSettings = settings;
            //    printerDialog.AllowPrintToFile = true;
            //    printerDialog.PrinterSettings.PrintToFile = true;

            //    DialogResult result = printerDialog.ShowDialog();

            //    if (result == DialogResult.OK)
            //    {
            //        printerName = settings.PrinterName;
            //    }
            //    else
            //    {
            //        return;
            //    }

            //}

            // Print the document on the selected printer (We are ignoring all other print
            // options here

            PrintPDF(saveArrayToTemp(pdfDoc, "pdf"));

            //PdfFilePrinter printer = new PdfFilePrinter(saveArrayToTemp(pdfDoc,"pdf"), printerName);

            //try
            //{
            //    printer.Print();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error: " + ex.Message);
            //}

        
        }
    }
}
