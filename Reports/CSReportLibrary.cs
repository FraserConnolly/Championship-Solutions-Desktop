using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChampionshipSolutions.Reporting;
using ChampionshipSolutions.DM;
using System.Data.Linq;
using System.IO;
using System.Windows;

namespace ChampionshipSolutions.Reports
{
    public class CSReportLibrary
    {

        //!!ChampionshipSolutions.DM.CSDB context = FileIO.FConnFile.getContext(); // MainWindow.Context;
        //!!Table<Template> dbTemplates;

        private static CSReportLibrary _instance;

        private ICollection<AReport> _templates = new List<AReport>();
        
        public static CSReportLibrary getLibrary ()
        {
            if (_instance == null)
                _instance = new CSReportLibrary();
            return _instance;
        }

        private CSReportLibrary()
        {
            loadTemplatesFromDatabase();
        }

        internal void closeLibrary()
        {
            _templates = new List<AReport> ( );
            _instance = null;
        }

        /// <summary>
        /// (Re-)loads templates from the database
        /// </summary>
        public void loadTemplatesFromDatabase ( )
        {
            _templates.Clear ( );

            foreach ( Template t in FileIO.FConnFile.GetFileDetails().IO.GetAll<Template>() )
            {
                AReport report = AReport.LoadTemplate ( t.Instructions , t.TemplateFile );

                if ( report != null )
                    _templates.Add ( report );
            }
        }

        public void addTemplatesToDB(AReport report)
        {
            string instructions;
            byte[] template;

            report.saveTemplate(out instructions, out template);
            
            Template dbTemplate = new Template();
            dbTemplate.Description = "";
            dbTemplate.TemplateName = report.Name;
            dbTemplate.InsertedDate = DateTime.Now;
            dbTemplate.TemplateFile = template;
            dbTemplate.Instructions = instructions;

            if ( ! FileIO.FConnFile.GetFileDetails ( ).IO.Add<Template> ( dbTemplate ) )
            {
                throw new Exception ( "Failed to create a new template" );
            }
            else
            {
                loadTemplatesFromDatabase ( );
            }
        }

        public AReport this[string Report]
        {
            get
            {
                foreach (AReport report in _templates)
                {
                    if (Report.ToLower() == report.Name.ToLower())
                        return report;
                }
                return null;
            }
        }
    }
}
