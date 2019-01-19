using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.ExcelToPdfConverter;
using System.Drawing.Imaging;

namespace ChampionshipSolutions
{
    public static class SFExToPDF
    {
        public static bool ConvertExcelToPDF ( string ExcelFileName , string PDFFileName = null )
        {
            if ( PDFFileName == null )
                PDFFileName = ExcelFileName + ".pdf";

            if ( !System.IO.File.Exists ( ExcelFileName ) )
                return false;



            ExcelToPdfConverter converter = new ExcelToPdfConverter(ExcelFileName);
            //Initialize the PdfDocument Class
            PdfDocument pdfDoc = new PdfDocument();

            //Initialize the ExcelToPdfConverterSettings class
            ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings()
            {
                LayoutOptions = LayoutOptions.FitSheetOnOnePage,
                TemplateDocument = pdfDoc,
                DisplayGridLines = GridLinesDisplayStyle.Invisible,
                EmbedFonts = true,
                ExportQualityImage = true
            };

            //Convert the Excel document to PDf
            pdfDoc = converter.Convert ( settings );
            try
            {
                pdfDoc.Save ( PDFFileName );

                return true;
            }
            catch ( Exception )
            {
                return false;
            }
        }

        public static bool ConvertExcelToImage ( string ExcelFileName , string ExportFileName = null )
        {
            if ( ExportFileName == null )
                ExportFileName = ExcelFileName + ".png";

            if ( !System.IO.File.Exists ( ExcelFileName ) )
                return false;

            try
            {
                ExcelEngine ee = new ExcelEngine();

                IWorkbook book = ee.Excel.Workbooks.OpenReadOnly ( ExcelFileName );

                IWorksheet sheet = book.ActiveSheet;
                sheet.UsedRangeIncludesFormatting = false;
                int lastRow = sheet.UsedRange.LastRow + 1;
                int lastColumn = sheet.UsedRange.LastColumn + 1;
                System.Drawing.Image img = sheet.ConvertToImage(1, 1, lastRow, lastColumn, ImageType.Bitmap, null);
                img.Save ( ExportFileName , ImageFormat.Png );

                return true;
            }
            catch ( Exception )
            {
                return false;
            }
        }

    }
}
