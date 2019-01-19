using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Data;
using System.Xml;
using System.IO;
using PdfSharp.Drawing.Layout;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using Ionic.Zip;

namespace ChampionshipSolutions.Reporting
{

    public interface IScriptApplication
    {

    }

    public interface IFilter
    {

    }

    public interface IReport
    {
        string Name { get; }
        //string[] getScriptNames();

        bool isCompatibleWith(string Script, object Host);
        //int countPages(string Script, object Host, IScriptApplication Application = null, IFilter Filter = null);
        int countPage(DataSet Data);
        int countPage(DataTable Data);

        //byte[] Generate ( string Script , object Host , IScriptApplication Application = null , IFilter Filter = null );
        byte[] Generate ( DataSet Data );
        byte[] Generate ( DataTable Data );
        byte[] GenerateSample ( );

        byte[] GetTemplate ( );
        void SetTemplate ( byte[] Template );

        bool isDocument();
    }

    public enum ReportType
    {
        PDF, 
        EXCEL,
        CSV,
        XML
    }

    public abstract class AReport : IReport
    {
        public const string PAGEFIELDS = "PageFields";

        //private static Stream getFile(string fileName, ZipFile zip)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    ICollection<string> files = zip.EntryFileNames;

        //    if (files.Contains(fileName))
        //        zip[fileName].Extract(ms);

        //    return ms;
        //}

        static public AReport LoadTemplate ( string Instructions , byte[] template = null )
        {

            XmlDocument instructions = new XmlDocument();

            instructions.LoadXml ( Instructions );

            return AReport.LoadTemplate ( instructions , template );
        }

        static public AReport LoadTemplate (XmlDocument XMLInstructions, byte[] template = null)
        {
            AReport newReport;

            XmlNode FConnTemplate = XMLInstructions.DocumentElement.SelectSingleNode("/FConnTemplate");

            XmlNode ReportInstructions = XMLInstructions.DocumentElement.SelectSingleNode("/FConnTemplate/Report");

            XmlNode ScriptInstructions = XMLInstructions.DocumentElement.SelectSingleNode("/FConnTemplate/Report/Script");

            ReportType type = (ReportType)Enum.Parse(typeof(ReportType), ReportInstructions.Attributes["Type"].Value.ToString());

            switch (type)
            {
                case ReportType.PDF:
                    newReport = new PDFReport();

                    string templateFileName = "";

                    if (ReportInstructions.SelectSingleNode("TemplateFileName") != null)
                        templateFileName = ReportInstructions.SelectSingleNode("TemplateFileName").FirstChild.Value.Trim();

                    if ( template != null )
                        ( (PDFReport)newReport ).setTemplate ( template , templateFileName );

                    break;
                case ReportType.EXCEL:
                    newReport = new ExcelReport();
                    break;
                case ReportType.CSV:
                    newReport = new CSVReport();
                    break;
                case ReportType.XML:
                    newReport = new XMLReport();
                    break;
                default:
                    newReport = null;
                    break;
            }

            if (newReport == null) throw new ArgumentException("Could not establish the report type");

            // commented out in Championship Solutions V3-0 - FC 2016-04-24
            //newReport.rawXML = XMLInstructions.OuterXml;

            newReport.Name = ReportInstructions.Attributes["Name"].Value;

            //newReport.Championship = ReportInstructions.Attributes["Championship"].Value.ToString();

            #region Load fields

            if (ReportInstructions.SelectSingleNode("Fields") != null)
            {
                newReport.loadFields(ReportInstructions.SelectSingleNode("Fields").OuterXml);
                //    // for each field in the report
                //    foreach (XmlNode f in ReportInstructions.SelectSingleNode("Fields").ChildNodes)
                //    {
                //        newReport.fields.Add(loadField(f));
                //    }
            }


            #endregion

            #region Load Tables

            if (ReportInstructions.SelectSingleNode("Tables") != null)
            {
                newReport.loadTables(ReportInstructions.SelectSingleNode("Tables").OuterXml);
            }
            #endregion

            #region Load Sample Data

            if ( ReportInstructions.SelectSingleNode ( "SampleData" ) != null )
            {
                newReport.loadSampleData ( ReportInstructions.SelectSingleNode ( "SampleData" ).OuterXml );
            }


            #endregion

            #region Load Script

            if ( ReportInstructions.SelectSingleNode("Script") != null)
            {
                switch (ReportInstructions.SelectSingleNode("Script").Attributes["Language"].Value)
                {
                    case "CSharp":
                        newReport.Script = new CSharpScripting();
                        newReport.Script.HeaderCode = ScriptInstructions.SelectSingleNode("/FConnTemplate/Report/Script/Header").InnerText;

                        // for each template name in the report
                        foreach (XmlNode f in ScriptInstructions.SelectNodes("/FConnTemplate/Report/Script/Template"))
                            newReport.Script.AddTemplate(
                                f.Attributes["Name"].Value,
                                f.Attributes["Host"].Value,
                                f.InnerText);

                        break;

                    case "Python":
                        newReport.Script = new PythonScripting();
                        // Note HeaderCode is ignored for Python
                        //newReport.Script.HeaderCode = ScriptInstructions.SelectSingleNode("/FConnTemplate/Report/Script/Header").InnerText;

                        // for each template name in the report
                        foreach (XmlNode f in ScriptInstructions.SelectNodes("/FConnTemplate/Report/Script/Template"))
                            newReport.Script.AddTemplate(
                                f.Attributes["Name"].Value,
                                f.Attributes["Host"].Value,
                                PythonScripting.CleanPythonTabs(f.InnerText));

                        break;
                    default:
                        newReport.Script = null;
                        break;
                }

                if (newReport.Script != null)
                {

                    newReport.Script.UsingStatements = ScriptInstructions.SelectSingleNode("/FConnTemplate/Report/Script/UsingStatements").InnerText;
                    newReport.Script.AdditionalMethods = ScriptInstructions.SelectSingleNode("/FConnTemplate/Report/Script/AdditionalMethods").InnerText;

                }


            }
            #endregion

            return newReport;

        }

        ///// <param name="TemplatePath">Path to the directory containing Instruction.xml.
        ///// Path should not end with '\'</param>
        //static public AReport LoadXMLTemplate (string TemplatePath, string TemplateFile)
        //{
        //    if (!System.IO.File.Exists(TemplatePath)) throw new ArgumentException("Template not found");

        //    File.ReadAllText(TemplatePath);

        //    if (TemplateFile == null)
        //    {
        //        return AReport.LoadTemplate(File.ReadAllText(TemplatePath), null);
        //    }
        //    else
        //    {
        //        return AReport.LoadTemplate(File.ReadAllText(TemplatePath),
        //            File.ReadAllBytes(TemplatePath));
        //    }
        //}

        public abstract void saveTemplate(string filePath);

        public abstract void saveTemplate(out string instructions, out byte[] templateFile);

        protected abstract string makeRawXML ( );

        public string rawXML { get { return makeRawXML ( ); } }

        #region Loading

        internal void loadFields(string fieldsXML)
        {
            this.fields.Clear();

            if (string.IsNullOrWhiteSpace(fieldsXML)) return;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fieldsXML);

            // for each field in the report
            foreach (XmlNode f in doc.SelectSingleNode("Fields").ChildNodes)
                if (! string.IsNullOrWhiteSpace ( f.InnerXml ) )
                fields.Add(loadField(f));
        }

        internal void loadTables ( string tablesXML )
        {
            this.tables.Clear ( );

            if ( string.IsNullOrWhiteSpace ( tablesXML ) ) return;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml ( tablesXML );

            // for each table in the report
            foreach ( XmlNode t in doc.SelectSingleNode ( "Tables" ).ChildNodes )
            {
                if ( string.IsNullOrWhiteSpace ( t.InnerXml ) ) continue;

                    Table newTable = new Table();

                newTable.Position = new XPoint (
                    Convert.ToInt32 ( t.Attributes["X"].Value ) ,
                    Convert.ToInt32 ( t.Attributes["Y"].Value ) );

                newTable.setBorderColour ( t.Attributes["BorderColour"].Value );

                newTable.BorderThickness = Convert.ToDecimal ( t.Attributes["BorderThickness"].Value );

                newTable.Name = t.Attributes["Name"].Value;

                if ( t.SelectSingleNode ( "Header" ) != null )
                {
                    newTable.HeaderHeight = Convert.ToDecimal ( t.SelectSingleNode ( "Header" ).Attributes["Height"].Value );
                    foreach ( XmlNode hc in t.SelectSingleNode ( "Header" ).ChildNodes )
                    {
                        if ( string.IsNullOrWhiteSpace ( hc.InnerXml ) ) continue;
                        newTable.HeaderCells.Add ( loadCell ( hc ) );
                    }
                }

                if ( t.SelectSingleNode ( "DataRow" ) != null )
                {
                    newTable.DataRowHeight = Convert.ToDecimal ( t.SelectSingleNode ( "DataRow" ).Attributes["Height"].Value );

                    foreach ( XmlNode hc in t.SelectSingleNode ( "DataRow" ).ChildNodes )
                    {
                        if ( string.IsNullOrWhiteSpace ( hc.InnerXml ) ) continue;
                        newTable.Cells.Add ( loadCell ( hc ) );
                    }
                }

                //if ( t.SelectSingleNode ( "SampleRow" ) != null )
                //{
                //    newTable.SampleRowHeight  = Convert.ToDecimal ( t.SelectSingleNode ( "SampleRow" ).Attributes["Height"]  .Value );
                //    newTable.SampleRowRepeats = Convert.ToInt32   ( t.SelectSingleNode ( "SampleRow" ).Attributes["Repeated"].Value );

                //    foreach ( XmlNode hc in t.SelectSingleNode ( "DataRow" ).ChildNodes )
                //    {
                //        newTable.SampleRow.Add ( loadCell ( hc ) );
                //    }
                //}

                if ( t.SelectSingleNode ( "FixedRows" ) != null )
                {
                    foreach ( XmlNode fixedRow in t.SelectSingleNode ( "FixedRows" ).ChildNodes )
                    {
                        if ( string.IsNullOrWhiteSpace ( fixedRow.InnerXml ) ) continue;
                        TableRow r = new TableRow();

                        r.Height = Convert.ToDecimal ( fixedRow.Attributes["Height"].Value );

                        foreach ( XmlNode frCell in fixedRow )
                        {
                            if ( string.IsNullOrWhiteSpace ( frCell.InnerXml ) ) continue;
                            r.Cells.Add ( loadCell ( frCell ) );
                        }

                        newTable.FixedRows.Add ( r );
                    }
                }

                this.tables.Add ( newTable );

            } // end for each table
        }

        internal void loadSampleData ( string sampleXml )
        {
            this.SampleDataSet = new DataSet ( );

            if ( string.IsNullOrWhiteSpace ( sampleXml ) ) return;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml ( sampleXml );

            // for each table in the report
            foreach ( XmlNode t in doc.SelectSingleNode ( "SampleData" ).ChildNodes )
            {
                if ( string.IsNullOrWhiteSpace ( t.InnerXml ) ) continue;
                DataTable newTable = new DataTable();

                newTable.TableName = t.Attributes["Name"].Value;

                if ( t.SelectSingleNode ( "Columns" ) != null )
                {
                    foreach ( XmlNode column in t.SelectSingleNode ( "Columns" ).ChildNodes )
                    {
                        if ( string.IsNullOrWhiteSpace ( column.OuterXml ) ) continue;
                        newTable.Columns.Add (
                                column.Attributes["Name"].Value ,
                                System.Type.GetType ( column.Attributes["Type"].Value )
                            );
                    }
                }
                else
                {
                    continue;
                }

                if ( t.SelectSingleNode ( "SampleRows" ) != null )
                {
                    foreach ( XmlNode sampleRow in t.SelectSingleNode ( "SampleRows" ).ChildNodes )
                    {
                        if ( string.IsNullOrWhiteSpace ( t.InnerXml ) ) continue;
                        DataRow dr = newTable.NewRow();
                        foreach ( XmlNode sampleCell in sampleRow.ChildNodes )
                            if ( !string.IsNullOrWhiteSpace ( t.InnerXml ) )
                                if ( sampleCell.Name == "SampleCell" )
                                {
                                    object value = sampleCell.Attributes["Value"].Value;

                                    if ( sampleCell.Attributes["Type"] != null )
                                    {
                                        Type type = System.Type.GetType(sampleCell.Attributes["Type"].Value);

                                        if ( type.ToString ( ) == "System.DBNull" )
                                        {
                                            dr[sampleCell.Attributes["Name"].Value] = System.DBNull.Value;
                                        }
                                        else
                                        {
                                            dr[sampleCell.Attributes["Name"].Value] = Convert.ChangeType ( value , type );
                                        }
                                    }
                                    else // assume type is System.String
                                    {
                                        dr[sampleCell.Attributes["Name"].Value] = sampleCell.Attributes["Value"].Value.ToString ( );
                                    }
                                }
                        newTable.Rows.Add ( dr );
                    }
                }

                SampleDataSet.Tables.Add ( newTable );
            } // end for each table
        }

        private static Field loadField(XmlNode node)
        {
            Field newField = null;
            #region Field Factory
            switch (node.Name)
            {
                case "Field":
                    newField = new Field ( );
                    break;

                case "DataField":
                    newField = new DataField ( );
                    break;

                case "RectangleField":
                    newField = new RectangleField ( );
                    break;

                case "DataRectangleField":
                    newField = new DataRectangleField ( );
                    break;

                case "Cell":
                    newField = new Cell ( );
                    break;

                case "DataCell":
                    newField = new DataCell ( );
                    break;

                case "SampleCell":
                    newField = new SampleCell ( );
                    break;

                default:
                    newField = new Field ( );
                    break;
            }
            #endregion

            newField.Name = node.Attributes["Name"].Value;

            if (node.Attributes["DefaultText"] != null)
                newField.DefualtText = node.Attributes["DefaultText"].Value.Replace("\\n","\n").Replace("\\t", "\t") ;

            if (node.Attributes["TextAlign"] != null)
                newField.setTextFormat(node.Attributes["TextAlign"].Value);

            if (newField is IDataField)
                ((IDataField)newField).DataColumn = node.Attributes["Column"].Value;

            if (newField is RectangleField)
                ((RectangleField)newField).Rectangle = new Rectangle(
                Convert.ToInt32(node.Attributes["X"].Value),
                Convert.ToInt32(node.Attributes["Y"].Value),
                Convert.ToInt32(node.Attributes["Width"].Value),
                Convert.ToInt32(node.Attributes["Height"].Value));

            if (newField is Cell)
            {
                if ( node.Attributes["Width"] != null)
                    ((Cell)newField).Width = Convert.ToDecimal(node.Attributes["Width"].Value);
                if (node.Attributes["Orientation"] != null)
                    ((Cell)newField).setOrientation(node.Attributes["Orientation"].Value);
                if (node.Attributes["MergeDown"] != null)
                    ((Cell)newField).MergeDown = Convert.ToInt32(node.Attributes["MergeDown"].Value);
                if (node.Attributes["MergeRight"] != null)
                    ((Cell)newField).MergeRight = Convert.ToInt32(node.Attributes["MergeRight"].Value);
                if (node.Attributes["VerticalAlignment"] != null)
                    ((Cell)newField).setVerticalAlignment ( node.Attributes["VerticalAlignment"].Value ) ;
                if (node.Attributes["ShadeOnDefault"] != null)
                    ( (Cell)newField ).CanShade = bool.Parse( node.Attributes["ShadeOnDefault"].Value );

            }

            if (node.SelectSingleNode("Font") != null)
            {
                XmlNode font = node.SelectSingleNode("Font");


                if (font.Attributes["Name"] != null)
                    newField.FontName = font.Attributes["Name"].Value;

                if (font.Attributes["Size"] != null)
                    newField.FontSize = Convert.ToInt32(font.Attributes["Size"].Value);

                if (font.Attributes["Colour"] != null)
                    newField.setBrush(font.Attributes["Colour"].Value);

                if (font.Attributes["Style"] != null)
                    newField.setFontStyle(font.Attributes["Style"].Value);
            }


            return newField;
        }

        private static Cell loadCell(XmlNode node)
        {
            Field newField = loadField(node);

            if (newField is Cell)
            {
                return (Cell)newField;
            }

            throw new ArgumentException("The loaded field was not a cell");
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public ReportType Type { get { return getReportType(); } }

        public AScripting Script { get; set; }

        public DataSet SampleDataSet { get; set; }

        //public string ExportPath { get; set; }

        ///// <summary>
        ///// Used to store Query information but is no longer execute with Generate().
        ///// </summary>
        //public string Query { get; set; }

        #endregion

        #region Abstract Methods

        protected abstract ReportType getReportType();

        public abstract bool setTemplate ( string path );
        public abstract bool setTemplate ( byte[] templateData , string fileName );

        /// <summary>
        /// Can be used for fields only
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public abstract byte[] Generate(DataTable dataTable);
        
        /// <summary>
        /// Can be used for fields and tables
        /// </summary>
        /// <param name="dataSet">Must contain a table called PageFields</param>
        public abstract byte[] Generate(DataSet dataSet);

        //public abstract byte[] Generate(string Script, object Host, IScriptApplication Application = null, IFilter Filter = null);

        public abstract byte[] GenerateSample ( );

        #endregion

        //private string _TemplatePath;
        //public string TemplatePath
        //{
        //    get
        //    { return _TemplatePath; }
        //    set
        //    {
        //        if (setTemplate(value))
        //            _TemplatePath = value;
        //    }
        //}


        public List<Field> fields = new List<Field>();
        public List<Table> tables = new List<Table>();

        public virtual void SaveGeneratedFile(string FilePath = null)
        {
            //if (string.IsNullOrEmpty(FilePath) && string.IsNullOrEmpty(ExportPath))
            if ( string.IsNullOrEmpty(FilePath) )
                throw new ArgumentNullException("FilePath", "FilePath and ExportPath can not both be null");

                //if (FilePath == null)
                //FilePath = ExportPath;

            if (memoryStorage != null)
            {
                try
                {
                    FileStream fs = new FileStream(FilePath, FileMode.Create);
                    MemoryStream ms = new MemoryStream(memoryStorage);
                    ms.WriteTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    // can't throw exception here as we are not on the UI thread.
                    throw new ApplicationException("Failed to save file \n" + FilePath);
                }
            }
        }

        protected byte[] memoryStorage;

        protected byte[] TemplateData;


        //public string[] getScriptNames()
        //{
        //    if (Script == null) return null;

        //    return Script.getTemplates();
        //}

        public bool isCompatibleWith(string Script, object Host)
        {
            if (this.Script == null) return false;

            return this.Script.hasTemplate(Script);
        }

        //abstract public int countPages(string Script, object Host, IScriptApplication Application = null, IFilter Filter = null);
        abstract public int countPage(DataSet Data);
        abstract public int countPage(DataTable Data);

        abstract public bool isDocument();

        public byte[] GetTemplate ( )
        {
            return TemplateData;
        }

        public void SetTemplate ( byte[] Template )
        {
            TemplateData = Template;
        }
    }

    public class PDFReport : AReport
    {

        public override void saveTemplate(string filePath)
        {
            string reportDirectory = filePath + "\\" + Name;

            if (!Directory.Exists(reportDirectory))
                System.IO.Directory.CreateDirectory(filePath + "\\" + Name);

            if (!File.Exists(reportDirectory + "\\Instruction.xml"))
                File.Delete(reportDirectory + "\\Instruction.xml");

            StreamWriter fs = new StreamWriter (reportDirectory);

            fs.Write(makeXMLTemplate());

            fs.Close();
        }

        protected override string makeRawXML ( )
        {
            return makeXMLTemplate ( );
        }

        private string makeXMLTemplate()
        {
            StringBuilder sb = new StringBuilder();

            //XmlWriter xml = XmlWriter.Create(reportDirectory + "\\Instruction.xml");
            XmlWriter xml = XmlWriter.Create(sb);
            xml.WriteStartDocument();

            xml.WriteStartElement("FConnTemplate");
            xml.WriteStartElement("Report");
            xml.WriteAttributeString("Name", Name);
            xml.WriteAttributeString("Type", Type.ToString());

            if (Script != null)
            {
                xml.WriteStartElement("Script");
                xml.WriteAttributeString("Language", Script.Language);

                if ( Script is CSharpScripting cScript )
                {
                    xml.WriteElementString( "UsingStatements" , cScript.UsingStatements );
                    xml.WriteElementString( "Header" , cScript.HeaderCode );
                    xml.WriteElementString( "AdditionalMethods" , cScript.AdditionalMethods );
                }

                foreach (string t in Script.getTemplates())
                {
                    xml.WriteStartElement("Template");
                    xml.WriteAttributeString("Name", t);
                    xml.WriteAttributeString("Host", Script.getHostObject(t));
                    xml.WriteValue(Script.getTemplateCode(t));
                    xml.WriteEndElement();// end of Template
                }

                xml.WriteEndElement();// end of Script
            }

            // Fields

            if (this.fields.Count > 0)
            {
                xml.WriteStartElement("Fields");

                foreach (Field f in fields)
                {
                    xml.WriteStartElement(f.fieldType().ToString());
                    xml.WriteAttributeString("Name", f.Name);
                    xml.WriteAttributeString("TextAlign", f.Alignment.ToString());
                    xml.WriteAttributeString ( "DefaultText" , f.DefualtText );

                    if (f is IDataField)
                        xml.WriteAttributeString("Column", ((IDataField)f).DataColumn);

                    if (f is RectangleField)
                    {
                        xml.WriteAttributeString("X", ((RectangleField)f).XPosition.X.ToString());
                        xml.WriteAttributeString("Y", ((RectangleField)f).XPosition.Y.ToString());
                        xml.WriteAttributeString("Width", ((RectangleField)f).XPosition.Width.ToString());
                        xml.WriteAttributeString("Height", ((RectangleField)f).XPosition.Height.ToString());
                    }

                    // Font

                    xml.WriteStartElement("Font");

                    xml.WriteAttributeString("Name", f.FontName);
                    xml.WriteAttributeString("Size", f.FontSize.ToString());
                    xml.WriteAttributeString("Colour", f.getFontColour().ToString());
                    xml.WriteAttributeString("Style", f.FontStyle.ToString());

                    xml.WriteEndElement();// end of Font

                    xml.WriteEndElement();// end of Field
                }



                xml.WriteEndElement();// end of Fields
            }

            // Tables

            if (this.tables.Count > 0)
            {
                xml.WriteStartElement("Tables");

                foreach (Table t in this.tables)
                {
                    xml.WriteStartElement("Tables");
                    xml.WriteAttributeString("Name", t.Name);
                    xml.WriteAttributeString("BorderColour", t.KnownBorderColour.ToString());
                    xml.WriteAttributeString("BorderThickness", t.BorderThickness.ToString());
                    xml.WriteAttributeString("X", t.Position.X.ToString());
                    xml.WriteAttributeString("Y", t.Position.Y.ToString());

                    xml.WriteStartElement("Header");
                    xml.WriteAttributeString("Height", t.HeaderHeight.ToString());

                    foreach (Cell c in t.HeaderCells)
                    {
                        xml.WriteStartElement(c.fieldType().ToString());
                        xml.WriteAttributeString("Width", c.Width.ToString());
                        xml.WriteAttributeString("Name", c.Name);
                        xml.WriteAttributeString("DefaultText", c.DefualtText);
                        xml.WriteAttributeString("TextAlign", c.Alignment.ToString());
                        xml.WriteAttributeString ( "MergeRight" , c.MergeRight.ToString ( ) );
                        xml.WriteAttributeString ( "MergeDown" , c.MergeDown.ToString ( ) );
                        xml.WriteAttributeString("VerticalAlignment", c.getVerticalAlignment().ToString());
                        if (c is IDataField)
                            xml.WriteAttributeString("Column", ((IDataField)c).DataColumn);

                        xml.WriteStartElement("Font");

                        xml.WriteAttributeString("Name", c.FontName);
                        xml.WriteAttributeString("Size", c.FontSize.ToString());
                        xml.WriteAttributeString("Colour", c.getFontColour().ToString());
                        xml.WriteAttributeString("Style", c.FontStyle.ToString());

                        xml.WriteEndElement();// end of Font


                        xml.WriteEndElement();// end of Cell

                    }

                    xml.WriteEndElement();// end of Header

                    xml.WriteStartElement("DataRow");
                    xml.WriteAttributeString("Height", t.DataRowHeight.ToString());

                    foreach (Cell c in t.Cells)
                    {
                        xml.WriteStartElement(c.fieldType().ToString());
                        xml.WriteAttributeString( "Width", c.Width.ToString());
                        xml.WriteAttributeString( "Name", c.Name);
                        xml.WriteAttributeString( "DefaultText", c.DefualtText);
                        xml.WriteAttributeString( "TextAlign", c.Alignment.ToString());
                        xml.WriteAttributeString ( "MergeRight" , c.MergeRight.ToString ( ) );
                        xml.WriteAttributeString ( "MergeDown" , c.MergeDown.ToString ( ) );
                        xml.WriteAttributeString( "VerticalAlignment", c.getVerticalAlignment().ToString());
                        xml.WriteAttributeString ( "ShadeOnDefault" , c.CanShade.ToString() );
                        if (c is IDataField)
                            xml.WriteAttributeString("Column", ((IDataField)c).DataColumn);


                        xml.WriteStartElement("Font");

                        xml.WriteAttributeString("Name", c.FontName);
                        xml.WriteAttributeString("Size", c.FontSize.ToString());
                        xml.WriteAttributeString("Colour", c.getFontColour().ToString());
                        xml.WriteAttributeString("Style", c.FontStyle.ToString());

                        xml.WriteEndElement();// end of Font
                        xml.WriteEndElement();// end of Cell

                    }


                    xml.WriteEndElement();// end of DataRow

                    if (t.FixedRows.Count > 0)
                    {

                        xml.WriteStartElement("FixedRows");

                        foreach (TableRow tr in t.FixedRows)
                        {
                            xml.WriteStartElement("FixedRow");
                            xml.WriteAttributeString("Height", tr.Height.ToString());

                            foreach (Cell c in tr.Cells)
                            {
                                xml.WriteStartElement(c.fieldType().ToString());
                                xml.WriteAttributeString("Width", c.Width.ToString());
                                xml.WriteAttributeString("Name", c.Name);
                                xml.WriteAttributeString("DefaultText", c.DefualtText);
                                xml.WriteAttributeString("TextAlign", c.Alignment.ToString());
                                xml.WriteAttributeString ( "MergeRight" , c.MergeRight.ToString ( ));
                                xml.WriteAttributeString ( "MergeDown" , c.MergeDown.ToString ( ));
                                xml.WriteAttributeString("VerticalAlignment", c.getVerticalAlignment().ToString());
                                if (c is IDataField)
                                    xml.WriteAttributeString("Column", ((IDataField)c).DataColumn);


                                xml.WriteStartElement("Font");

                                xml.WriteAttributeString("Name", c.FontName);
                                xml.WriteAttributeString("Size", c.FontSize.ToString());
                                xml.WriteAttributeString("Colour", c.getFontColour().ToString());
                                xml.WriteAttributeString("Style", c.FontStyle.ToString());

                                xml.WriteEndElement();// end of Font

                                xml.WriteEndElement();// end of Cell

                            }
                            xml.WriteEndElement();// end of FixedRow
                        }
                        xml.WriteEndElement();// end of FixedRows


                    }

                    xml.WriteEndElement();// end of Table

                }


                xml.WriteEndElement();// end of Tables
            }

            // Sample Data

            if ( this.SampleDataSet != null )
            {
                xml.WriteStartElement ( "SampleData" );

                foreach ( DataTable table in SampleDataSet.Tables )
                {
                    xml.WriteStartElement ( "SampleTable" );
                    xml.WriteAttributeString ( "Name" , table.TableName );

                    xml.WriteStartElement ( "Columns" );

                    foreach ( DataColumn column in table.Columns)
                    {
                        xml.WriteStartElement ( "Column" );
                        xml.WriteAttributeString ( "Name" , column.ColumnName );
                        xml.WriteAttributeString ( "Type" , column.DataType.ToString() );
                        xml.WriteEndElement ( );
                    }

                    xml.WriteEndElement ( ); // end Columns

                    xml.WriteStartElement ( "SampleRows" );

                    foreach ( DataRow dr in table.Rows ) 
                    {
                        xml.WriteStartElement ( "SampleRow" );

                        for ( int c = 0 ; c < dr.ItemArray.Count ( ) ; c++ ) 
                        {
                            if ( dr[c].GetType().ToString ( ) == "System.DBNull" )
                                continue;
                            xml.WriteStartElement ( "SampleCell" );
                            xml.WriteAttributeString ( "Name" , table.Columns[c].ColumnName );
                            xml.WriteAttributeString ( "Value" , dr[c].ToString ( ) ); 
                            xml.WriteAttributeString ( "Type" , dr[c].GetType().ToString() );
                            xml.WriteEndElement ( ); // end SampleCell
                        }
                        xml.WriteEndElement ( ); // end SampleRow

                    }

                    xml.WriteEndElement ( ); // end SampleRows


                    xml.WriteEndElement ( );// end of SampleTable
                }



                xml.WriteEndElement ( );//  end of SampleData
            }


            //if ( !string.IsNullOrWhiteSpace(TemplatePath))
            //    xml.WriteElementString("TemplateFileName", TemplatePath);

            xml.WriteEndElement();// end of Report
            xml.WriteEndElement();// end of FConnTemplate
            xml.Flush();

            return sb.ToString();
        }

        public override void saveTemplate(out string instructions, out byte[] templateFile)
        {

            instructions = makeXMLTemplate();

            //if (Template != null)
            //{
                //MemoryStream templateStream = new MemoryStream();
                //FileStream fs = new FileStream(TemplateFileName, FileMode.Open);

                //fs.CopyTo(templateStream);

                //templateFile = templateStream.ToArray();
            //}
            if(GetTemplate() != null)
            {
                templateFile = GetTemplate ( );
            }
            else
            {
                templateFile = null;
            }


            //ZipFile zip = new ZipFile();

            //zip.AddEntry("Instructions.xml", makeXMLTemplate());

            //if (Template != null)
            //{
            //    Stream templateStream = new MemoryStream();
            //    Template.Save(templateStream);
            //    zip.AddEntry("Template.pdf", templateStream);
            //}

            //Stream outputZip = new MemoryStream();

            //zip.Save(outputZip);

            //return outputZip;
        }

        public PdfSharp.Drawing.XRect PageSize { get; private set; }

        public override bool setTemplate(byte[] templateData, string fileName)
        {
            if (templateData == null)
            {
                return false;
            }

            try
            {
                MemoryStream ms = new MemoryStream(templateData );


                Template = PdfReader.Open(ms, PdfDocumentOpenMode.Import);

                if (Template.Pages.Count < 1)
                    throw new ArgumentException("Template does not have any pages");

                TemplatePage = Template.Pages[0];
                PageSize = new XRect() { Height = TemplatePage.Height, Width = TemplatePage.Width };
                TemplateFileName = fileName;


                //templateData.CopyTo ( ms );

                SetTemplate ( templateData );
                return true;
            }
            catch (Exception)
            {
                Template = null;
                return false;
                //throw new ApplicationException("Failed to open template");
            }
        }

        public override bool setTemplate(string value)
        {
            if (value == string.Empty)
            {
                return false;
            }

            if (System.IO.File.Exists(value))
            {
                try
                {
                    FileStream fileIO = System.IO.File.OpenRead ( value );

                    MemoryStream ms = new MemoryStream();

                    fileIO.CopyTo ( ms );


                    return setTemplate ( ms.ToArray() , "" );

                    //MemoryStream ms = new MemoryStream();


                    //Template = PdfReader.Open(value, PdfDocumentOpenMode.Modify);

                    //if (Template.Pages.Count < 1)
                        //throw new ArgumentException("Template does not have any pages");

                    //TemplatePage = Template.Pages[0];
                    //PageSize = new XRect() { Height = TemplatePage.Height, Width = TemplatePage.Width };
                    //TemplateFileName = value;


                    //Template.Save ( ms );

                    //SetTemplate ( ms.ToArray ( ) );

                    //return true;
                }
                catch (Exception)
                {
                    Template = null;
                    return false;
                    //throw new ApplicationException("Failed to open template");
                }
            }
            else
            {
                //throw new ArgumentException("File does not exist");
                return false;
            }
        }

        private string TemplateFileName;
        private PdfDocument Template;
        private PdfPage TemplatePage;

        // added for Championship Solutions V3-0 2016-04-26
        public override byte[] GenerateSample ( )
        {
            if (SampleDataSet != null)
                return Generate ( SampleDataSet );

            if ( GetTemplate() != null )
                return GetTemplate();

            return null;
        }

        public override byte[] Generate(DataTable dt)
        {
            PdfDocument output = new PdfDocument();

            foreach (DataRow row in dt.Rows)
            {
                PdfPage page;

                if (Template != null)
                {
                    page = output.AddPage(TemplatePage);
                }
                else
                {
                    page = output.AddPage();
                }

                XGraphics gfx = XGraphics.FromPdfPage(page);
                XTextFormatter tf = new XTextFormatter(gfx);

                gfx.MFEH = PdfFontEmbedding.Always;

                foreach (DataRectangleField field in fields)
                {
                    if (row[field.DataColumn] != null)
                    {
                        //gfx.DrawString(row[field.Column].ToString(), field.Font, field.Brush, field.XPosition, field.TextFormat);
                        tf.Alignment = field.Alignment;
                        tf.Font = field.Font;
                        tf.DrawString(row[field.DataColumn].ToString(), field.Font, field.Brush, field.XPosition, XStringFormats.TopLeft);
                        

                        //gfx.DrawRectangle(XPens.DarkCyan, field.XPosition);
                    }
                }

            }

            MemoryStream ms = new MemoryStream();

            if (output.Pages.Count != 0)
            {
                output.Save(ms);
                memoryStorage = ms.ToArray();
            }
            else
            {
                memoryStorage = null;
            }

            return memoryStorage;
            //if (output.Pages.Count!=0)
            //    output.Save(ExportPath);

        }

        /// <summary>
        /// Uses MigraDocs
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public override byte[] Generate(DataSet ds)
        {
            if(ds.Tables.Count == 0)
                throw new ArgumentException("DataSet must have at least 1 table");

            if (!ds.Tables.Contains(PAGEFIELDS))
            {
                if ( ds.Tables.Count == 1)
                {
                    return Generate(ds.Tables[0]);
                }
                else
                {
                    throw new ArgumentException("DataSet must have PageFields");
                }
            }

            PdfDocument pdfDoc = new PdfDocument();

            // Iterate through each page
            foreach (DataRow row in ds.Tables[PAGEFIELDS].Rows)
            {
                // Prepare PDFSharp with template


                PdfPage page;

                if ( Template == null )
                    if (GetTemplate () != null )
                        setTemplate ( GetTemplate ( ) , "" );

                if ( Template != null )
                    page = pdfDoc.AddPage ( TemplatePage );
                else
                    page = pdfDoc.AddPage ( );

                PdfSharp.Drawing.XGraphics gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
                XTextFormatter tf = new XTextFormatter(gfx);

                gfx.MFEH = PdfFontEmbedding.Always;


                foreach (Field field in fields)
                {
                    if (field is DataRectangleField)
                    {
                        DataRectangleField df = (DataRectangleField)field;

                        tf.Alignment = field.Alignment;
                        tf.Font = field.Font;

                        if ( ds.Tables [ PAGEFIELDS ].Columns.Contains( df.DataColumn ) )
                        {

                            tf.DrawString(row[df.DataColumn].ToString(), field.Font, field.Brush, df.XPosition, XStringFormats.TopLeft);

                        }
                        else
                        {
                            tf.DrawString( df.DefualtText , field.Font , field.Brush , df.XPosition , XStringFormats.TopLeft );
                        }
                    }
                    else if (field is RectangleField)
                    {
                        RectangleField df = (RectangleField)field;

                        tf.Alignment = field.Alignment;
                        tf.Font = field.Font;
                        tf.DrawString(df.DefualtText.ToString(), field.Font, field.Brush, df.XPosition, XStringFormats.TopLeft);
                    }

                }

                int PageIndex = (int)row["Index"];

                // Goes through each table that is in the template file
                foreach (Table table in tables)
                {
                    // If the data set does not contain a Table for this templates table then skip
                    if (!ds.Tables.Contains(table.Name)) continue;

                    // Prepare MigraDocs
                    Document document = new Document();
                    Section section = document.AddSection();

                    MigraDoc.DocumentObjectModel.Tables.Table docTable = section.AddTable();

                    docTable.Borders.Width = new Unit( (double) table.BorderThickness );
                    docTable.Borders.Color = table.BorderColour;

                    // Add the columns
                    foreach (Cell cell in table.HeaderCells)
                        docTable.AddColumn(Unit.FromPoint((double)cell.Width));

                    // populate the header row
                    int i = 0;
                    Row headerRow = docTable.AddRow();
                    headerRow.Height = Unit.FromPoint((double)table.HeaderHeight);
                    headerRow.HeightRule = RowHeightRule.Exactly;

                    foreach (Cell cell in table.HeaderCells)
                    {
                        //headerRow[i].Add(CreateTextFrame(cell, cell.DefualtText));
                        //headerRow[i].Add(CreateTextFrame(cell, row, docTable.Columns[i].Width, headerRow.Height));
                        headerRow[i].Add(CreateParagraph(cell, cell.PrintText(row)));
                        headerRow[i].MergeDown = cell.MergeDown;
                        headerRow[i].MergeRight = cell.MergeRight;
                        headerRow[i].VerticalAlignment = cell.getVerticalAlignment();

                        i++;
                    }

                    foreach (TableRow tr in table.FixedRows)
                    {
                        Row fixRow = docTable.AddRow();
                        i = 0;

                        foreach (Cell cell in tr.Cells)
                        {

                            fixRow[i].Add(CreateParagraph(cell, cell.PrintText(row)));
                            fixRow[i].MergeDown = cell.MergeDown;
                            fixRow[i].MergeRight = cell.MergeRight;
                            fixRow[i].VerticalAlignment = cell.getVerticalAlignment();

                            i++;
                        
                        }
                    }



                    DataView dv = ds.Tables[table.Name].DefaultView;

                    // Can we order this table?
                    if (ds.Tables[table.Name].Columns.Contains("TableIndex"))
                    {
                        dv.Sort = "TableIndex";
                    }

                    // populate the data rows
                    foreach (DataRow tRow in dv.ToTable().Rows ) // ds.Tables[table.Name].Rows)
                    {
                        if ( tRow["PageIndex"] == DBNull.Value ) continue;

                        // Check that this row is for this page
                        if ((int)tRow["PageIndex"] != PageIndex) continue;
                        
                        Row dRow = docTable.AddRow();
                        dRow.Height = Unit.FromPoint((double)table.DataRowHeight);
                        dRow.HeightRule = RowHeightRule.Exactly;

                        i = 0;
                        Double rowHeight = dRow.Height.Value;

                        foreach (Cell cell in table.Cells)
                        {
                            if (cell is IDataField)
                            {
                                // cell.Name == String is a bodge related to the TF 2016 Athlete Report - String indicates that we want to text wrap on a table cell.
                                // to do unbodge this TF specific wrapping bodge
                                dRow[i].Add(AddTextToCell(gfx, cell, cell.PrintText(tRow), docTable.Columns[i].Width, ref rowHeight , cell.Name == "String" ) );
                                dRow[i].MergeDown = cell.MergeDown;
                                dRow[i].MergeRight = cell.MergeRight;
                                dRow[i].VerticalAlignment = cell.getVerticalAlignment();

                                // to do add functionality to add shading for athletes who have not been selected for the final.
                                if ( cell.CanShade )
                                    if ( cell.PrintText ( tRow ) == cell.PrintText ( null ) )
                                        dRow[i].Shading.Color = Colors.LightGray;

                            }
                            else
                            {
                                //dRow[i].Add(CreateTextFrame(cell, null, docTable.Columns[i].Width, dRow.Height));
                                //dRow[i].Add(CreateParagraph(cell, cell.PrintText(tRow)));
                                dRow[i].Add(AddTextToCell(gfx, cell, cell.PrintText(tRow), docTable.Columns[i].Width, ref rowHeight ) );
                                dRow[i].MergeDown = cell.MergeDown;
                                dRow[i].MergeRight = cell.MergeRight;
                                dRow[i].VerticalAlignment = cell.getVerticalAlignment();
                            }
                            i++;
                            dRow.Height = rowHeight;
                        }
                    }

                    // Create a renderer and prepare (=layout) the document
                    MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(document);
                    docRenderer.PrepareDocument();
                    // Render the paragraph. You can render tables or shapes the same way.
                    docRenderer.RenderObject(gfx, XUnit.FromPoint(table.Position.X), XUnit.FromPoint(table.Position.Y), XUnit.FromPoint(table.Width()), docTable);

                }

            }

            MemoryStream ms = new MemoryStream();

            if (pdfDoc.Pages.Count != 0)
            {
                pdfDoc.Save(ms);

                memoryStorage = ms.ToArray();
            }
            else
            {
                memoryStorage = null;
            }

            return memoryStorage;

        }

        // replaced by CreateTextFrame which has orientation functionality
        private Paragraph CreateParagraph(Field field, string text)
        {
            Paragraph p = new Paragraph();

            p.AddText(text);
            p.Format.Font.Name = field.FontName;
            p.Format.Font.Color = field.getFontColour();
            p.Format.Font.Bold = field.isBold();
            p.Format.Font.Italic = field.isItalic();
            p.Format.Font.Underline = field.isUnderlined();
            p.Format.Alignment = field.getAlignment();

            return p;
        }

        private Paragraph AddTextToCell(XGraphics oGFX, Field field, string text, Unit Width, ref double Height, bool CanWrap = false)
        {
            //XGraphics oGFX = XGraphics.FromPdfPage(pg);
            Unit maxWidth = Width; // -(cell.Column.LeftPadding + cell.Column.RightPadding);

            Paragraph par = CreateParagraph(field, text);


            XFont font = field.Font; //new XFont(document.Styles["Table"].Font.Name, fontsize);
            
            if (oGFX.MeasureString(text, font).Width < maxWidth.Value)
            {
                //par = cell.AddParagraph(instring);
                if ( oGFX.MeasureString ( text , font ).Height > Height )
                {
                    double origonalHeight = Height;
                    for ( ; oGFX.MeasureString ( text , font ).Height > Height ; )
                    {
                        Height = Height + origonalHeight;
                    }

                }
                return par;
            }
            else if ( CanWrap )
            {

                int stringlength = text.Length;

                // get maximum length per line so we know how much extra line space that we need.
                for ( ; stringlength > 0 ; stringlength-- )
                {
                    if ( oGFX.MeasureString ( text.Substring ( 0 , stringlength ) , font ).Width < maxWidth.Value )
                        break;
                }

                int numberOfLines;

                numberOfLines = ( text.Length / stringlength ) + 1;

                Height = Height * numberOfLines;

                return par;
            }

            else // String does not fit - start the truncation process...
            {

                int stringlength = text.Length;

                for (  ; stringlength > 0 ; stringlength --)
                {
                    if ( oGFX.MeasureString ( text.Substring ( 0 , stringlength ) + '\u2026' , font ).Width < maxWidth.Value )
                        break;
                }
                par = CreateParagraph ( field , text.Substring ( 0 , stringlength ) + '\u2026' );


                // Legacy method - few iterations but cuts much more text than necessary 
                //int stringlength = text.Length;
                //for (int i = 0; i < 3; i++)
                //{
                //    if (oGFX.MeasureString(text.Substring(0, stringlength) + '\u2026', font).Width > maxWidth.Value)
                //        stringlength -= (int)Math.Ceiling(text.Length * Math.Pow(0.5f, i));
                //    else
                //        if (i < 2)
                //            stringlength += (int)Math.Ceiling(text.Length * Math.Pow(0.5f, i));
                //}
                //par = CreateParagraph(field,text.Substring(0, stringlength) + '\u2026');
            }
            //par.Format.Font.Size = fontsize;
            return par;
        }

        //private TextFrame CreateTextFrame(Cell cell, DataRow dr)
        //{
        //    return CreateTextFrame(cell, cell.PrintText(dr));
        //}

        //private TextFrame CreateTextFrame(Cell cell, DataRow dr, Unit Width, Unit Height)
        //{
        //    return CreateTextFrame(cell, cell.PrintText(dr), Width, Height);
        //}


        //private TextFrame CreateTextFrame(Field field, string text, Unit Width = new Unit(), Unit Height = new Unit())
        //{
        //    TextFrame tf = new TextFrame();
        //    Paragraph p;
        //    p = tf.AddParagraph();

        //    p.AddText(text);
        //    p.Format.Font.Name = field.FontName;
        //    p.Format.Font.Color = field.getFontColour();
        //    p.Format.Font.Bold = field.isBold();
        //    p.Format.Font.Italic = field.isItalic();
        //    p.Format.Font.Underline = field.isUnderlined();
        //    p.Format.Alignment = field.getAlignment();

        //    tf.Width = Width;
        //    tf.Height = Height;

        //    tf.Orientation = field.getTextOrientation();

        //    //if (tf.Orientation == TextOrientation.Vertical)
        //    //    tf.LineFormat = new LineFormat() { Color = MigraDoc.DocumentObjectModel.Color.Parse("Red"), Width = 1, Visible = true, DashStyle = DashStyle.Solid };

        //    return tf;
        //}

        public void GenerateGridLines(string inputPath, string outputPath, double GridInterval = 10.0)
        {
            if (!File.Exists(inputPath)) throw new ArgumentException("Input file does not exist", "inputPath");

            PdfDocument Template = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);

            if (Template.Pages.Count < 1)
                throw new ArgumentException("Input file does not have any pages");

            PdfDocument output = new PdfDocument();

            PdfPage page = output.AddPage(Template.Pages[0]);

            double xCoursor = 0;
            double yCoursor = 0;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            XPen blackPen = new XPen(XColors.Blue, 1);
            XPen greyPen = new XPen(XColors.LightBlue, 0.1);

            int counter = 0;

            for (xCoursor = 0; xCoursor < page.Width; xCoursor += GridInterval)
            {

                gfx.DrawString(xCoursor.ToString(), new XFont("Calibri", 5), XBrushes.Black, new XPoint(xCoursor + 1, 5));
                gfx.DrawLine(greyPen, xCoursor, 0, xCoursor, page.Height);
                
                if (counter == 10)
                {
                    gfx.DrawLine(blackPen, xCoursor, 0, xCoursor, page.Height);
                    counter = 0;
                }
                counter++;
            }

            counter = 0;

            for (yCoursor = 0; yCoursor < page.Height; yCoursor += GridInterval)
            {

                gfx.DrawString(yCoursor.ToString(), new XFont("Calibri", 5), XBrushes.Black, new XPoint(1, yCoursor + 5));
                gfx.DrawLine(greyPen, 0, yCoursor, page.Width, yCoursor);

                if (counter == 10)
                {
                    gfx.DrawLine(blackPen, 0, yCoursor, page.Width, yCoursor);
                    counter = 0;
                }
                counter++;
            }

            output.Save(outputPath);
        }

        public void GenerateGridLines(string outputPath, double GridInterval = 10.0)
        {
            PdfPage page;
            PdfDocument output = new PdfDocument();

            if ( Template == null )
                if ( GetTemplate ( ) != null )
                    setTemplate ( GetTemplate ( ) , "" );

            if ( Template != null )
                page = output.AddPage ( TemplatePage );
            else
                page = output.AddPage ( );


            if ( Template == null)
                throw new ArgumentException("Template must not be null");


            //page = output.AddPage(TemplatePage);

            double xCoursor = 0;
            double yCoursor = 0;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            XPen blackPen = new XPen(XColors.Blue, 1);
            XPen greyPen = new XPen(XColors.LightBlue, 0.1);

            int counter = 0;

            for (xCoursor = 0; xCoursor < page.Width; xCoursor += GridInterval)
            {

                gfx.DrawString(xCoursor.ToString(), new XFont("Calibri", 5), XBrushes.Black, new XPoint(xCoursor + 1, 5));
                gfx.DrawLine(greyPen, xCoursor, 0, xCoursor, page.Height);
                
                if (counter == 10)
                {
                    gfx.DrawLine(blackPen, xCoursor, 0, xCoursor, page.Height);
                    counter = 0;
                }
                counter++;
            }

            counter = 0;

            for (yCoursor = 0; yCoursor < page.Height; yCoursor += GridInterval)
            {

                gfx.DrawString(yCoursor.ToString(), new XFont("Calibri", 5), XBrushes.Black, new XPoint(1, yCoursor + 5));
                gfx.DrawLine(greyPen, 0, yCoursor, page.Width, yCoursor);

                if (counter == 10)
                {
                    gfx.DrawLine(blackPen, 0, yCoursor, page.Width, yCoursor);
                    counter = 0;
                }
                counter++;
            }

            output.Save(outputPath);
        }

        protected override ReportType getReportType()
        {
            return ReportType.PDF;
        }

        //public override byte[] Generate(string Script, object Host, IScriptApplication Application = null, IFilter Filter = null)
        //{
        //    if (Script == null)
        //        return null;

        //    throw new NotImplementedException();
        //}

        public override int countPage(DataSet Data)
        {
            throw new NotImplementedException();
        }

        public override int countPage(DataTable Data)
        {
            throw new NotImplementedException();
        }

        public override bool isDocument()
        {
            return true;
        }

    }

    public class ExcelReport : AReport
    {
        public override bool setTemplate(string value) { return false; }

        public override bool setTemplate(byte[] value, string fileName) { return false; }

        protected override string makeRawXML ( )
        {
            throw new NotImplementedException ( );
        }

        public override byte[] Generate(DataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public override byte[] Generate(DataSet dataSet)
        {
            throw new NotImplementedException();
        }

        public override byte[] GenerateSample ( )
        {
            throw new NotImplementedException ( );
        }

        protected override ReportType getReportType()
        {
            return ReportType.EXCEL;
        }

        public override void saveTemplate(string filePath)
        {
            throw new NotImplementedException();
        }

        public override int countPage(DataSet Data)
        {
            throw new NotImplementedException();
        }

        public override int countPage(DataTable Data)
        {
            throw new NotImplementedException();
        }

        public override bool isDocument()
        {
            return false;
        }

        public override void saveTemplate(out string instructions, out byte[] templateFile)
        {
            throw new NotImplementedException();
        }
    }

    public class CSVReport : AReport
    {
        public override bool setTemplate ( string value ) { return false; }

        public override bool setTemplate ( byte[] value , string fileName ) { return false; }

        public static void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            if (includeHeaders)
            {
                List<string> headerValues = new List<string>();
                foreach (DataColumn column in sourceTable.Columns)
                {
                    headerValues.Add(QuoteValue(column.ColumnName));
                }

                writer.WriteLine(String.Join(",", headerValues.ToArray()));
            }

            string[] items = null;
            foreach (DataRow row in sourceTable.Rows)
            {
                items = row.ItemArray.Select(o => QuoteValue(o.ToString())).ToArray();
                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
        }

        private static string QuoteValue(string value)
        {
            return String.Concat("\"", value.Replace("\"", "\"\""), "\"");
        }

        public override byte[] Generate(DataTable dt)
        {

            MemoryStream ms = new MemoryStream();
            
            TextWriter tw = new StreamWriter(ms);

            if (fields.Count == 0)
            {
                WriteDataTable(dt, tw, true);
            }
            else
            {

                string line = string.Empty;
                foreach (Field column in fields)
                {
                    if (column is IDataField)
                        line += (QuoteValue(((IDataField)column).DataColumn)) + ",";
                    else
                        line += (QuoteValue(column.Name)) + ",";
                }
                line = line.Remove(line.Length - 1);
                tw.WriteLine(line);

                foreach (DataRow row in dt.Rows)
                {
                    line = string.Empty;

                    foreach (Field f in fields)
                    {
                        if (f is IDataField)
                            line += QuoteValue(row[((IDataField)f).DataColumn].ToString()) + ",";
                        else
                            line += (QuoteValue(f.DefualtText)) + ",";
                    }

                    line = line.Remove(line.Length - 1);
                    tw.WriteLine(line);

                }
            }
            
            tw.Close();

            memoryStorage = ms.ToArray();

            return memoryStorage;
        }

        public override byte[] Generate(DataSet dataSet)
        {
            if (dataSet.Tables.Contains(AReport.PAGEFIELDS))
            {
                return Generate(dataSet.Tables[AReport.PAGEFIELDS]);
            }
            throw new ArgumentException("DataSet must contain PAGEFIELDS");
        }

        public override byte[] GenerateSample ( )
        {
            throw new NotImplementedException ( );
        }

        protected override ReportType getReportType()
        {
            return ReportType.CSV;
        }

        protected override string makeRawXML ( )
        {
            throw new NotImplementedException ( );
        }

        public override void saveTemplate(string filePath)
        {
            throw new NotImplementedException();
        }

        public override int countPage(DataSet Data)
        {
            throw new NotImplementedException();
        }

        public override int countPage(DataTable Data)
        {
            throw new NotImplementedException();
        }

        public override bool isDocument()
        {
            return false;
        }

        public override void saveTemplate(out string instructions, out byte[] templateFile)
        {
            throw new NotImplementedException();
        }
    }

    public class XMLReport : AReport
    {
        public override bool setTemplate(string value) { return false; }

        public override bool setTemplate(byte[] value, string fileName) { return false; }

        public override byte[] Generate(DataTable dt)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            string output;

            ds.DataSetName = Name;

            output = ds.GetXml();

            //TextWriter tw = new StreamWriter(ExportPath);
            
            MemoryStream ms = new MemoryStream();

            TextWriter tw = new StreamWriter(ms);
            tw.Write(output);
            tw.Close();

            memoryStorage = ms.ToArray();

            return memoryStorage;
        }

        public override byte[] Generate(DataSet dataSet)
        {
            if (dataSet.Tables.Contains(AReport.PAGEFIELDS))
            {
                return Generate(dataSet.Tables[AReport.PAGEFIELDS]);
            }
            throw new ArgumentException("DataSet must contain PAGEFIELDS");
        }

        public override byte[] GenerateSample ( )
        {
            throw new NotImplementedException ( );
        }

        protected override ReportType getReportType()
        {
            return ReportType.XML;
        }

        protected override string makeRawXML ( )
        {
            throw new NotImplementedException ( );
        }

        public override void saveTemplate(string filePath)
        {
            throw new NotImplementedException();
        }

        public override int countPage(DataSet Data)
        {
            throw new NotImplementedException();
        }

        public override int countPage(DataTable Data)
        {
            throw new NotImplementedException();
        }

        public override bool isDocument()
        {
            return false;
        }

        public override void saveTemplate(out string instructions, out byte[] templateFile)
        {
            throw new NotImplementedException();
        }
    }

    public static class ListExtension
    {
        public static DataSet ToDataSet<T>(this IList<T> list)
        {
            Type elementType = typeof(T);
            DataSet ds = new DataSet();
            DataTable t = new DataTable();
            ds.Tables.Add(t);

            //add a column to table for each public property on T
            foreach (var propInfo in elementType.GetProperties())
            {
                Type ColType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                t.Columns.Add(propInfo.Name, ColType);
            }

            //go through each property on T and add each value to the table
            foreach (T item in list)
            {
                DataRow row = t.NewRow();

                foreach (var propInfo in elementType.GetProperties())
                {
                    row[propInfo.Name] = propInfo.GetValue(item, null) ?? DBNull.Value;
                }

                t.Rows.Add(row);
            }

            return ds;
        }
    }

}
