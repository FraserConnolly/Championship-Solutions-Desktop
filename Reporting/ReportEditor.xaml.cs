using Microsoft.Win32;
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
using System.IO;
using System.Xml;
using System.CodeDom.Compiler;
using System.Reflection;
using ChampionshipSolutions.Reporting.Template;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Folding;
using ChampionshipSolutions.ControlRoom;

namespace ChampionshipSolutions.Reporting
{
    enum ReportEditorStates
    {
        NotLoaded,
        NewlyCreated,
        OpenedFromFile,
        OpenedFromApplication,
    }

    /// <summary>
    /// Interaction logic for ReportEditor.xaml
    /// </summary>
    public partial class ReportEditor : Window
    {
        ReportEditorStates State = ReportEditorStates.NotLoaded;
        private AReport _report;
        string savePath;
        string selectedTemplate;

        public AReport report
        {
            get
            {
                return _report;
            }
            private set
            {
                _report = value;
            }
        }

        public void LoadReportFromApplication( AReport Report)
        {
            _report = Report;
            State = ReportEditorStates.OpenedFromApplication;
        }

        public ReportEditor()
        {
            //if (report == null) report = new PDFReport();
            InitializeComponent();

            foldingManager = FoldingManager.Install ( textEditor.TextArea );
            foldingStrategy = new XmlFoldingStrategy ( );
            foldingStrategy.UpdateFoldings ( foldingManager , textEditor.Document );

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds ( 2 );
            foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            foldingUpdateTimer.Start ( );

        }

        public ReportEditor(AReport report) : this( )
        {
            this.report = report;
        }

        private void setState()
        {
            switch ( State )
            {
                case ReportEditorStates.NotLoaded:
                    this.btnLoad.IsEnabled = true;
                    this.btnBrowse.IsEnabled = false;
                    this.tbxTemplateFileName.IsEnabled = false;
                    this.textEditor.IsEnabled = false;
                    //this.tbxFields.IsEnabled = false;
                    //this.tbxTables.IsEnabled = false;
                    this.btnDone.IsEnabled = false;
                    this.btnPreview.IsEnabled = false;
                    this.btnSave.IsEnabled = false;
                    this.btnSaveAs.IsEnabled = false;
                    this.btnCompile.IsEnabled = false;
                    this.tbxReportName.IsEnabled = false;
                    this.cbxReportType.IsEnabled = false;
                    this.btnCancel.IsEnabled = true;
                    break;
                case ReportEditorStates.NewlyCreated:
                    this.btnLoad.IsEnabled = false;
                    this.btnBrowse.IsEnabled = true;
                    this.tbxTemplateFileName.IsEnabled = true;
                    this.textEditor.IsEnabled = true;
                    //this.tbxFields.IsEnabled = true;
                    //this.tbxTables.IsEnabled = true;
                    this.btnDone.IsEnabled = true;
                    this.btnPreview.IsEnabled = true;
                    this.btnSave.IsEnabled = true;
                    this.btnSaveAs.IsEnabled = true;
                    this.btnCompile.IsEnabled = false;
                    this.tbxReportName.IsEnabled = true;
                    this.cbxReportType.IsEnabled = true;
                    this.btnCancel.IsEnabled = true;
                    break;
                case ReportEditorStates.OpenedFromFile:
                    this.btnLoad.IsEnabled = false;
                    this.btnBrowse.IsEnabled = true;
                    this.tbxTemplateFileName.IsEnabled = true;
                    this.textEditor.IsEnabled = true;
                    //this.tbxFields.IsEnabled = true;
                    //this.tbxTables.IsEnabled = true;
                    this.btnDone.IsEnabled = true;
                    this.btnPreview.IsEnabled = true;
                    this.btnSave.IsEnabled = true;
                    this.btnSaveAs.IsEnabled = true;
                    this.btnCompile.IsEnabled = false;
                    this.tbxReportName.IsEnabled = true;
                    this.cbxReportType.IsEnabled = false;
                    this.btnCancel.IsEnabled = true;
                    break;
                case ReportEditorStates.OpenedFromApplication:
                    this.btnLoad.IsEnabled = false;
                    this.btnBrowse.IsEnabled = true;
                    this.tbxTemplateFileName.IsEnabled = true;
                    this.textEditor.IsEnabled = true;
                    //this.tbxFields.IsEnabled = true;
                    //this.tbxTables.IsEnabled = true;
                    this.btnDone.IsEnabled = true;
                    this.btnPreview.IsEnabled = true;
                    this.btnSave.IsEnabled = true;
                    this.btnSaveAs.IsEnabled = true;
                    this.btnCompile.IsEnabled = false;
                    this.tbxReportName.IsEnabled = true;
                    this.cbxReportType.IsEnabled = false;
                    this.btnCancel.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog browser = new OpenFileDialog();
            browser.DefaultExt = ".pdf";
            browser.Multiselect = false;
            browser.ShowDialog();

            if (string.IsNullOrWhiteSpace(browser.FileName))
            {
                MessageBox.Show("Error getting file");
                return;
            }

            this.tbxTemplateFileName.Text = browser.FileName;

        }

        private void Window_Activated(object sender, EventArgs e)
        {
        }

        private void cbxReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cbxReportType.SelectedItem == null) return;

            AReport newReport;

            switch ((ReportType)this.cbxReportType.SelectedItem)
            {
                case ReportType.PDF:
                    newReport = new PDFReport();
                    break;
                case ReportType.EXCEL:
                    newReport = new PDFReport();
                    break;
                case ReportType.CSV:
                    newReport = new PDFReport();
                    break;
                case ReportType.XML:
                    newReport = new PDFReport();
                    break;
                default:
                    throw new ApplicationException("Failed to select a report type");
            }

            newReport.Name = report.Name;

            foreach (Field f in report.fields)
                newReport.fields.Add(f);

            foreach (Table t in report.tables)
                newReport.tables.Add(t);

            //newReport.ExportPath = report.ExportPath;

            //newReport.TemplatePath = report.TemplatePath;

            newReport.Script = report.Script;

            newReport.SampleDataSet = report.SampleDataSet;

            newReport.SetTemplate ( report.GetTemplate ( ) );

            report = newReport;
        }

        private void loadReport()
        {
            if (report == null) return;

            this.tbxReportName.Text = report.Name;
            foreach (ReportType i in cbxReportType.Items)
            {
                if (i == report.Type)
                    cbxReportType.SelectedItem = i;
            }

            if (report is PDFReport)
            {
                this.btnBrowse.IsEnabled = true;
                this.tbxTemplateFileName.IsEnabled = true;
                this.tbxTemplateFileName.Text = ""; //((PDFReport)report).TemplatePath;
            }
            else
            {
                this.btnBrowse.IsEnabled = false;
                this.tbxTemplateFileName.IsEnabled = false;
            }

            //if( report.Script != null)
            //{
            //    if (report.Script is CSharpScripting)
            //    {
            //        rbtCSharp.IsChecked = true;
            //        updateTemplates = true;
            //        loadCSharp();
            //    }
            //    else if (report.Script is PythonScripting)
            //        loadPython();
            //}

            LoadXML ( );

            LoadFields ( );
            LoadTables ( );

        }

        private void tbxReportName_TextChanged ( object sender , TextChangedEventArgs e )
        {
            report.Name = this.tbxReportName.Text;
        }

        private void tbxTemplateFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            report.setTemplate (this.tbxTemplateFileName.Text);
        }

        #region Scripting

        ////private void loadPython()
        ////{
        ////    PythonScripting script = (PythonScripting)report.Script;

        ////    this.tbxUsingStatements.Text = script.UsingStatements;
        ////    this.tbxAdditionalMethods.Text = script.AdditionalMethods;

        ////    if (updateTemplates)
        ////    {
        ////        this.lbxExecutionTemplates.Items.Clear();

        ////        //this.lbxExecutionTemplates.Items.Add("Header");

        ////        script.getTemplates().ToList().ForEach(x =>
        ////            this.lbxExecutionTemplates.Items.Add(x));

        ////        updateTemplates = false;
        ////    }

        ////    this.tbxCompletedCode.Text = script.Code;
        ////}

        //private bool updateTemplates = false;

        ////private void loadCSharp()
        ////{
        ////    //CSharpScripting script = (CSharpScripting)report.Script;

        ////    //this.tbxUsingStatements.Text = script.UsingStatements;
        ////    //this.tbxAdditionalMethods.Text = script.AdditionalMethods;

        ////    //if (updateTemplates)
        ////    //{
        ////    //    this.lbxExecutionTemplates.Items.Clear();

        ////    //    this.lbxExecutionTemplates.Items.Add("Header");

        ////    //    script.getTemplates().ToList().ForEach(x =>
        ////    //        this.lbxExecutionTemplates.Items.Add(x));

        ////    //    updateTemplates = false;
        ////    //}

        ////    //this.tbxCompletedCode.Text = script.Code;
        ////}



        ////private void rbtPython_Checked(object sender, RoutedEventArgs e)
        ////{
        ////    this.tabPythonScrips.Visibility = Visibility.Visible;
        ////    this.tabCSharpScrips.Visibility = Visibility.Collapsed;
        ////}

        //private void rbtCSharp_Checked(object sender, RoutedEventArgs e)
        //{
        //    //this.tabPythonScrips.Visibility = Visibility.Collapsed;
        //    //this.tabCSharpScrips.Visibility = Visibility.Visible;
        //    this.tabScrips.Visibility = Visibility.Visible;
        //}

        //private void rbtNoScript_Checked(object sender, RoutedEventArgs e)
        //{
        //    //this.tabPythonScrips.Visibility = Visibility.Collapsed;
        //    //this.tabCSharpScrips.Visibility = Visibility.Collapsed;
        //    this.tabScrips.Visibility = Visibility.Collapsed;
        //}

        //private void btnAddExecutionTemplate_Click(object sender, RoutedEventArgs e)
        //{
        //    if (report.Script == null)
        //        report.Script = new CSharpScripting();

        //    NewTemplateDialog TD = new NewTemplateDialog();
        //    if (TD.ShowDialog() == true)
        //    {
        //        report.Script.AddTemplate(TD.TemplateName, TD.Host, string.Empty);
        //        updateTemplates = true;
        //        loadCSharp();
        //    }
        //}

        //private void btnRemExecutionTemplate_Click(object sender, RoutedEventArgs e)
        //{
        //    if (report.Script == null)
        //        return;

        //    if (this.selectedTemplate == null)
        //        return;

        //    if (this.selectedTemplate == "Header") return;

        //    report.Script.RemoveTemplate(this.selectedTemplate.ToString());
        //    updateTemplates = true;
        //    loadCSharp();
        //}


        //private void tbxUsingStatements_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (report.Script == null)
        //        report.Script = new CSharpScripting();

        //    ((CSharpScripting)report.Script).UsingStatements = this.tbxUsingStatements.Text;

        //    loadCSharp();
        //}

        //private void lbxExecutionTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (this.lbxExecutionTemplates.SelectedItem == null) return;

        //    if (report.Script == null) return;

        //    if (this.lbxExecutionTemplates.SelectedItem.ToString() == "Header")
        //    {
        //        if (report.Script is CSharpScripting)
        //        {
        //            this.tbxTemplateCode.Text = ((CSharpScripting)report.Script).HeaderCode;
        //        }
        //    }
        //    else
        //    {
        //        this.tbxTemplateCode.Text = report.Script.getTemplateCode(this.lbxExecutionTemplates.SelectedItem.ToString());
        //    }
        //    this.selectedTemplate = this.lbxExecutionTemplates.SelectedItem.ToString();
        //}

        //private void tbxTemplateCode_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (this.selectedTemplate == null)
        //        return;

        //    if (report.Script == null) return;

        //    if (this.selectedTemplate == "Header")
        //    {
        //        if (report.Script is CSharpScripting)
        //        {
        //            ((CSharpScripting)report.Script).HeaderCode = this.tbxTemplateCode.Text;
        //        }
        //    }
        //    else
        //    {
        //        report.Script.setTemplateCode(selectedTemplate, this.tbxTemplateCode.Text);
        //    }

        //    loadCSharp();
        //}



        //private void tbxAdditionalMethods_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (report.Script == null) return;

        //    if (report.Script is CSharpScripting)
        //    {
        //        ((CSharpScripting)report.Script).AdditionalMethods = this.tbxAdditionalMethods.Text;
        //        loadCSharp();
        //    }
        //}

        #endregion

        #region XML Displays

        private void LoadFields()
        {
            //if (report.rawXML == null) return;

            //XmlDocument instructions = new XmlDocument();

            //instructions.LoadXml(report.rawXML);

            //XmlNode ReportInstructions = instructions.DocumentElement.SelectSingleNode("/FConnTemplate/Report");

            //if (ReportInstructions.SelectSingleNode("Fields") != null)
            //    this.tbxFields.Text = ReportInstructions.SelectSingleNode("Fields").OuterXml.Replace(">",">\n");
        }

        private void LoadTables()
        {
            //if (report.rawXML == null) return;

            //XmlDocument instructions = new XmlDocument();

            //instructions.LoadXml(report.rawXML);

            //XmlNode ReportInstructions = instructions.DocumentElement.SelectSingleNode("/FConnTemplate/Report");

            //if (ReportInstructions.SelectSingleNode("Tables") != null)
            //    this.tbxTables.Text = ReportInstructions.SelectSingleNode("Tables").OuterXml.Replace(">", ">\n");
        }

        private void LoadXML ( )
        {
            if ( report.rawXML == null ) return;

            XmlDocument instructions = new XmlDocument();

            instructions.LoadXml ( report.rawXML );

            //XmlNode ReportInstructions = instructions.DocumentElement.SelectSingleNode("/FConnTemplate/Report");
            XmlNode ReportInstructions = instructions.DocumentElement.SelectSingleNode("/FConnTemplate");

            StringBuilder xmlString  = new StringBuilder();

            XmlWriter writer = XmlWriter.Create( xmlString , new XmlWriterSettings() { Indent = true } );

            ReportInstructions.WriteTo ( writer );

            writer.Flush ( );

            this.textEditor.Text = xmlString.ToString ( ); // ReportInstructions.OuterXml.Replace ( ">" , ">\n" );

            ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy indentStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy ( );

            indentStrategy.IndentLines ( this.textEditor.Document , 0 , this.textEditor.Document.LineCount );

            //foreach ( var line in this.textEditor.Document.Lines )
            //{
                //indentStrategy.IndentLine ( this.textEditor.Document , line );
            //}
        }

        #endregion

        #region File System

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(savePath))
                btnSaveAs_Click(null, null);
            else
                save(savePath);
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            saveAs();
        }

        private void saveAs(bool Compile = false)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();

            if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                MessageBox.Show("Error selecting directory");
                return;
            }

            save(dialog.SelectedPath, Compile);
        }

        private void save(string path, bool Compile = false)
        {
            //report.loadFields ( this.tbxFields.Text );
            //report.loadTables ( this.tbxTables.Text );
            report = AReport.LoadTemplate ( this.textEditor.Text );
            report.saveTemplate(path);

            if (Compile)
                if (report.Script != null)
                    if (report.Script is CSharpScripting)
                    {
                        CompilerResults cr;
                        CSharpScripting.CompileCode(report.Script.Code, path + "\\" + report.Name + "\\" + report.Name + ".dll", out cr);

                        if (cr.Errors.Count > 0)
                            foreach (CompilerError err in cr.Errors)
                                MessageBox.Show(err.ErrorText);
                    }
        }

        private void btnCompile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(savePath))
                saveAs(true);
            else
                save(savePath, true);

        }

        private void btnLoad_Click ( object sender , RoutedEventArgs e )
        {
            if ( State != ReportEditorStates.NotLoaded )
                if ( MessageBox.Show ( "Loading a template will delete any unsaved changes, do you want to continue" , "Continue?" , MessageBoxButton.OKCancel ) != MessageBoxResult.OK )
                    return;

            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.ShowDialog ( );

            if ( string.IsNullOrWhiteSpace ( dialog.FileName ) )
            {
                MessageBox.Show ( "Error selecting a template" );
                return;
            }

            try
            {
                MessageBox.Show ( "Disabled" );
                return;
                //report = AReport.LoadXMLTemplate ( dialog.FileName , null );
            }
            catch ( Exception ex )
            {
                MessageBox.Show ( ex.Message );
            }

            loadReport ( );

        }

        #endregion


        #region Folding
        FoldingManager foldingManager;
        XmlFoldingStrategy foldingStrategy;

        void foldingUpdateTimer_Tick ( object sender , EventArgs e )
        {
            if ( foldingStrategy != null )
            {
                foldingStrategy.UpdateFoldings ( foldingManager , textEditor.Document );
            }
        }
        #endregion


        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            byte[] previewFile = null;

            try
            {
                previewFile = report.GenerateSample ( );
            }
            catch ( Exception ex )
            {
                MessageBox.Show ( "Failed to render preview" + "\n" + ex.Message );
            }

            if ( previewFile == null )
            {
                MessageBox.Show ( "Failed to render preview" );
                return;
            }

            WinFormPDFViewer viewer = new WinFormPDFViewer();
            viewer.LoadPDF ( previewFile );


        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click ( object sender , RoutedEventArgs e )
        {
            this.Close ( );
        }

        private void textEditor_LostFocus ( object sender , RoutedEventArgs e )
        {
            try
            {
                byte[] temp = report.GetTemplate();
                report = AReport.LoadTemplate ( this.textEditor.Text );
                report.SetTemplate ( temp );
                loadReport ( );
            }
            catch ( Exception ex )
            {
                MessageBox.Show ( string.Format ( "Failed to generate report. \n {0}" , ex.Message ) );
            }
        }

        private void Window_Loaded ( object sender , RoutedEventArgs e )
        {
            this.cbxReportType.Items.Clear ( );
            foreach ( ReportType type in Enum.GetValues ( typeof ( ReportType ) ) )
                this.cbxReportType.Items.Add ( type );

            if ( report == null )
            {
                State = ReportEditorStates.NewlyCreated;

                report = new PDFReport ( );
            }
            setState ( );

            loadReport ( );
        }
    }
}
