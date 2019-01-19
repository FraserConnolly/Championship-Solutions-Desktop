using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.Reporting
{
    public class Table
    {
        private readonly Color defaultBorderColour = Colors.Black;
        
        public Table()
        {
            BorderColour = defaultBorderColour;
        }

        public void setBorderColour(string KnownColour)
        {
            Color col = Color.Parse(KnownColour);

            if (col != null)
            {
                BorderColour = Color.FromCmyk(col.C, col.M, col.Y, col.K);
                KnownBorderColour = KnownColour;
            }
            else
                BorderColour = Colors.Black;
        }

        public string Name { get; set; }
        public decimal BorderThickness { get; set; }
        public Color BorderColour { get; set; }
        public XPoint Position { get; set; }
        public decimal HeaderHeight { get; set; }
        public decimal DataRowHeight { get; set; }
        //public decimal SampleRowHeight { get; set; }
        //public decimal SampleRowRepeats { get; set; }
        public string KnownBorderColour { get; private set; }

        public List<Cell> HeaderCells = new List<Cell>();
        //public List<Cell> SampleRow = new List<Cell>();
        public List<Cell> Cells = new List<Cell>();

        public List<TableRow> FixedRows = new List<TableRow>();

        public double Width()
        {
            decimal d = 0;

            HeaderCells.ForEach(x => d += x.Width);

            return (double)d;
        }
    }

    public class TableRow
    {
        public string Name { get; set; }
        public decimal Height { get; set; }
    
        public List<Cell> Cells = new List<Cell>();
    }
}
