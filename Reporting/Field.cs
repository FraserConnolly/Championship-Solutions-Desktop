using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using MigraDoc.DocumentObjectModel;
using System.Data;

namespace ChampionshipSolutions.Reporting
{
    public enum ReportFields
    {
        Field,
        DataField,
        RectangleField,
        DataRectangleField,
        Cell,
        DataCell,
        SampleCell
    }

    public interface IDataField
    {
        string DataColumn { get; set; }
    }

    public class Field
    {
        public string Name { get; set; }

        public virtual ReportFields fieldType()
        {
            return ReportFields.Field;
        }

        public Field()
        {
            _Font = defaultFont;
            //Brush = defaultBrush;
            colour = defaultColour;
            xcolour = defaultXColour;
            Alignment = defaultAlignment;
        }
        private readonly XFont  defaultFont = new XFont("Calibri", 11, XFontStyle.Regular, new XPdfFontOptions(PdfSharp.Pdf.PdfFontEmbedding.Always));
        private readonly XBrush defaultBrush   = XBrushes.Black;
        private readonly XColor defaultXColour = XColors.Black;
        private readonly Color  defaultColour  =  Colors.Black;
        private  Color  colour;
        private XColor xcolour;

        //private read-only XStringFormat defaultFormat = XStringFormats.TopLeft;
        private readonly PdfSharp.Drawing.Layout.XParagraphAlignment defaultAlignment = XParagraphAlignment.Left;


        private XFont _Font;
        public XFont Font { get { return _Font; } private set { _Font = value; } }

        public void setBrush(string KnownColour)
        {
            XColor xcol = XColor.FromKnownColor((XKnownColor)Enum.Parse(typeof(XKnownColor), KnownColour));
            Color col = Color.Parse(KnownColour);

            if (xcol != null && col != null)
            {
                xcolour = xcol;
                colour = col;
            }
            else
            {
                xcolour = defaultXColour;
                colour = defaultColour;
            }
        }

        public void setTextFormat(string format)
        {
            switch (format)
            {
                case "Centre":
                    Alignment = XParagraphAlignment.Center;
                    return;
                case "Center":
                    Alignment = XParagraphAlignment.Center;
                    return;
                case "Left":
                    Alignment = XParagraphAlignment.Left;
                    return;
                case "Right":
                    Alignment = XParagraphAlignment.Right;
                    return;
                case "Justified":
                    Alignment = XParagraphAlignment.Justify;
                    return;
                default:
                    Alignment = XParagraphAlignment.Default;
                    return;
            }
        }

        public XBrush Brush
        {
            get
            {
                if(colour != null)
                    return new XSolidBrush(xcolour);
                return defaultBrush;
            }
        }

        public string FontName
        {
            get { return _Font.Name; }
            set
            {
                try
                {
                    _Font = new XFont( value , _Font.Size , _Font.Style , new XPdfFontOptions( PdfSharp.Pdf.PdfFontEmbedding.Always ) );
                }
                catch ( Exception )
                {
                    _Font = defaultFont;
                }
            }
        }
        public double FontSize
        {
            get { return _Font.Size; }

            set
            {
                string fontName = _Font.Name;
                _Font = new XFont(fontName, value, _Font.Style, new XPdfFontOptions(PdfSharp.Pdf.PdfFontEmbedding.Always));
            }
        }

        public string FontStyle
        {
            get
            {
                return _Font.Style.ToString();
            }
            set
            {
                setFontStyle(value);
            }
        }
        public void setFontStyle(string style)
        {
            XFontStyle fontStyle;

            switch (style)
            {
                case "Bold":
                    fontStyle = XFontStyle.Bold;
                    break;
                case "BoldItalic":
                    fontStyle = XFontStyle.BoldItalic;
                    return;
                case "Italic":
                    fontStyle = XFontStyle.Italic;
                    break;
                case "Strikeout":
                    fontStyle = XFontStyle.Strikeout;
                    break;
                case "Underline":
                    fontStyle = XFontStyle.Underline;
                    break;
                default:
                    fontStyle = XFontStyle.Regular;
                    break;
            }
            _Font = new XFont(_Font.Name, _Font.Size, fontStyle, new XPdfFontOptions(PdfSharp.Pdf.PdfFontEmbedding.Always));
    }

        private PdfSharp.Drawing.Layout.XParagraphAlignment _Alignment;
        public PdfSharp.Drawing.Layout.XParagraphAlignment Alignment
        {
            get
            {
                return _Alignment;
            }
            set
            {
                _Alignment = value;
            }
        }

        private string Orientation;

        public MigraDoc.DocumentObjectModel.ParagraphAlignment getAlignment()
        {
            switch (Alignment)
            {
                case XParagraphAlignment.Center:
                    return MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                
                case XParagraphAlignment.Justify:
                    return MigraDoc.DocumentObjectModel.ParagraphAlignment.Justify;
                
                case XParagraphAlignment.Left:
                    return MigraDoc.DocumentObjectModel.ParagraphAlignment.Left;
                
                case XParagraphAlignment.Right:
                    return MigraDoc.DocumentObjectModel.ParagraphAlignment.Right;
                
                default:
                    return MigraDoc.DocumentObjectModel.ParagraphAlignment.Left;
            }
        }

        public MigraDoc.DocumentObjectModel.Color getFontColour()
        {
            //return MigraDoc.DocumentObjectModel.Color.FromCmyk(colour.C,colour.M,colour.Y,colour.K);
            return colour;
        }

        internal bool isItalic()
        {
            return this.FontStyle.Contains("Italic");
        }

        internal bool isBold()
        {
            return this.FontStyle.Contains("Bold");
        }

        internal MigraDoc.DocumentObjectModel.Underline isUnderlined()
        {
            if (this.FontStyle.Contains("Underline"))
                return MigraDoc.DocumentObjectModel.Underline.Single;
            else
                return MigraDoc.DocumentObjectModel.Underline.None;
        }

        private string _DefaultText;
        public string DefualtText { get { if (_DefaultText == null) return string.Empty; return _DefaultText; } set { _DefaultText = value; } }

        public void setOrientation(string orientation)
        {
            Orientation = orientation;
        }

        internal MigraDoc.DocumentObjectModel.Shapes.TextOrientation getTextOrientation()
        {

            switch (Orientation)
            {
                case "Downward":
                    return MigraDoc.DocumentObjectModel.Shapes.TextOrientation.Downward;
                case "Horizontal":
                    return MigraDoc.DocumentObjectModel.Shapes.TextOrientation.Horizontal;
                case "HorizontalRotatedFarEast":
                    return MigraDoc.DocumentObjectModel.Shapes.TextOrientation.HorizontalRotatedFarEast;
                case "Upward":
                    return MigraDoc.DocumentObjectModel.Shapes.TextOrientation.Upward;
                case "Vertical":
                    return MigraDoc.DocumentObjectModel.Shapes.TextOrientation.Vertical;
                case "VerticalFarEast":
                    return MigraDoc.DocumentObjectModel.Shapes.TextOrientation.VerticalFarEast;
                default:
                    return MigraDoc.DocumentObjectModel.Shapes.TextOrientation.Horizontal;
            }


        }

    }

    public class DataField : Field, IDataField
    {
        public override ReportFields fieldType()
        {
            return ReportFields.DataField;
        }

        public string DataColumn { get; set; }
    }

    public class RectangleField : Field
    {
        public override ReportFields fieldType()
        {
            return ReportFields.RectangleField;
        }

        public XRect XPosition { get; set; }
        public System.Drawing.Rectangle Rectangle
        {
            get
            {
                return new System.Drawing.Rectangle(Convert.ToInt32(XPosition.X), Convert.ToInt32(XPosition.Y), Convert.ToInt32(XPosition.Width), Convert.ToInt32(XPosition.Height));
            }
            set
            {
                XPosition = new XRect(value.X, value.Y, value.Width, value.Height);
            }
        }
    }

    public class DataRectangleField : RectangleField, IDataField
    {
        public override ReportFields fieldType()
        {
            return ReportFields.DataRectangleField;
        }

        public string DataColumn {get; set;}
    }

    public class Cell : Field
    {
        public override ReportFields fieldType()
        {
            return ReportFields.Cell;
        }

        public string VAlignment;

        public void setVerticalAlignment(string VA)
        {
            VAlignment = VA;
        }

        public decimal Width { get; set; }

        public int MergeRight { get; set; }

        public int MergeDown { get; set; }

        public bool CanShade { get; set; }

        public virtual string PrintText(DataRow dr)
        {
            return this.DefualtText;
        }

        internal MigraDoc.DocumentObjectModel.Tables.VerticalAlignment getVerticalAlignment()
        {
            switch (VAlignment)
            {
                case "Bottom":
                    return MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Bottom;
                case "Centre":
                    return MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                case "Center":
                    return MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                case "Top":
                    return MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
                default:
                    return MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Top;
            }
        }
    }

    public class DataCell : Cell, IDataField
    {
        public override ReportFields fieldType()
        {
            return ReportFields.DataCell;
        }

        public string DataColumn { get; set; }

        public override string PrintText(DataRow dr)
        {
            if (dr != null)
                if (dr[DataColumn] != null) 
                    return dr[DataColumn].ToString();
            
            return base.PrintText(dr);
        }
    }

    public class SampleCell : DataCell
    {
        public override ReportFields fieldType ( ) { return ReportFields.SampleCell; }

        public override string PrintText ( DataRow dr )
        {
            return this.DefualtText;
        }

    }
}
