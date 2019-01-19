using ChampionshipSolutions.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChampionshipSolutions.Reporting;

namespace ChampionshipSolutions
{
    public static class PrintCertificates
    {
        public static string[] PrintAllEnum(Type en)
        {
            return Enum.GetNames(en);
        }

        //Modified 2015-05-22 removed Template Path from parameters as the template can change with each new competitor.
        public static List<string> SaveIndividualCertificates(List<CertificateData> CDs, string ExportPath)
        {
            List<string> pdfCertificates = new List<string>();


            //Generate the individual Certificates
            foreach (CertificateData CD in CDs)
            {
                //PDFReport CertsReport = (PDFReport)MainWindow.ReportLibrary["Certificate"]; //(PDFReport)AReport.LoadTemplate(CD.Competitor.CompetingIn.CertificateTemplate);
                PDFReport CertsReport = (PDFReport) Reports.CSReportLibrary.getLibrary() ["Certificate"]; //(PDFReport)AReport.LoadTemplate(CD.Competitor.CompetingIn.CertificateTemplate);

                // To do 2016-05-14 this is a bodge. The template is being chosen based on a hard coded string and not on the selection in EditEvent

                ACertificate cert = new CertificateInvididual()
                {
                    Competitor = CD.Competitor,
                    Event = CD.Competitor.CompetingIn,
                    File = new FileStorage(ExportPath, "PDF", CertsReport.Generate(new List<CertificateData>() { CD }.ToDataSet().Tables[0]))
                };

                //if (Properties.Settings.Default.SaveCertificateToDB)
                //{
                //    CD.Competitor.Certificates.Add(cert);

                //    if (Properties.Settings.Default.SaveCertificateImage)
                //        Printing.GetPdfThumbnail(
                //            Printing.saveArrayToTemp(cert.File.FileData),
                //            string.Format("{0}{1} {4} {3} {2}.png", ExportPath, CD.EventName, CD.Competitor.getName(), CD.CertifiacteType, CD.Competitor.printVestNumber()),
                //            1, 1, GhostscriptSharp.Settings.GhostscriptPageSizes.a5);
                //}

                //Printing.PrintPDF(cert.File.FileData);

                CertsReport.SaveGeneratedFile(string.Format("{0}\\{1} {4} {3} {2}.pdf", ExportPath, CD.EventName, CD.Competitor.getName(), CD.CertifiacteType, CD.Competitor.printVestNumber()));

                pdfCertificates.Add(string.Format("{0}\\{1} {4} {3} {2}.pdf", ExportPath, CD.EventName, CD.Competitor.getName(), CD.CertifiacteType, CD.Competitor.printVestNumber()));
            }

            return pdfCertificates;
        }

        // Modified 2015-05-22 removed TemplatePath from parameter list as Template can now change with each competitor
        public static List<string> SaveAllEventCertificates(List<CertificateData> CDs, string ExportPath)
        {
            List<string> pdfCertificates = new List<string>();


            //        // Generate Event Certificates
            //foreach (AEvent Event in MainWindow.CurrentChampionship.listAllEvents())
            foreach (AEvent Event in ((App)App.Current).CurrentChampionship.Championship.listAllEvents())
            {
                //PDFReport CertsReport = (PDFReport)MainWindow.ReportLibrary["Certificate"]; //(PDFReport)AReport.LoadTemplate(Event.CertificateTemplate);
                PDFReport CertsReport = (PDFReport) ChampionshipSolutions.Reports.CSReportLibrary.getLibrary()["Certificate"]; 

                List<CertificateData> GroupedCertificates = CDs.Where(cd => cd.EventName == Event.Name).OrderBy(cd => cd.CertifiacteType).ThenBy(cd => cd.RankCounter).ToList();

                byte[] b = CertsReport.Generate(GroupedCertificates.ToDataSet().Tables[0]);

                CertsReport.SaveGeneratedFile ( string.Format("{0}{1} {2}.pdf", ExportPath, Event.Name, "All"));
                pdfCertificates.Add(string.Format("{0}{1} {2}.pdf", ExportPath, Event.Name, "All"));
            }

            return pdfCertificates;

        }

        // Modified 2015-05-22 removed TemplatePath from parameter list as Template can now change with each competitor
        public static List<string> SaveAllEventCertificatesByType(List<CertificateData> CDs, string ExportPath, AEvent [] Events = null)
        {
            List<string> pdfCertificates = new List<string>();

            if ( Events == null )
                Events = ( (App)App.Current ).CurrentChampionship.Championship.listAllEvents ( ).ToArray();

            // Generate Event Certificates
            //foreach (AEvent Event in MainWindow.CurrentChampionship.listAllEvents())
            foreach (AEvent Event in Events)
            {
                //PDFReport CertsReport = (PDFReport)MainWindow.ReportLibrary["Certificate"]; // (PDFReport)AReport.LoadTemplate(Event.CertificateTemplate);
                //PDFReport CertsReport = (PDFReport) ChampionshipSolutions.Reports.CSReportLibrary.getLibrary() ["Certificate"]; // (PDFReport)AReport.LoadTemplate(Event.CertificateTemplate);

                if ( !Event.IsFinal ) continue;

                if ( Event.CertificateTemplate == null ) continue;

                PDFReport CertsReport = (PDFReport) PDFReport.LoadTemplate( Event.CertificateTemplate.Instructions, Event.CertificateTemplate.TemplateFile);

                if ( CertsReport == null ) continue;

                List<CertificateData> GroupedCertificates = CDs.Where(cd => cd.Competitor.CompetingIn.Name == Event.Name).ToList();

                foreach (string str in GroupedCertificates.Select(c => c.CertificateName).Distinct())
                {
                    List<CertificateData> GroupedCertificatesType = GroupedCertificates.Where(cd => cd.CertificateName == str).OrderBy(cd => cd.CertifiacteType).ThenBy(cd => cd.RankCounter).ToList();

                    byte[] b = CertsReport.Generate(GroupedCertificatesType.ToDataSet().Tables[0]);

                    CertsReport.SaveGeneratedFile ( string.Format("{0}\\{1} {2}.pdf", ExportPath, Event.Name, str));
                    pdfCertificates.Add(string.Format("{0}\\{1} {2}.pdf", ExportPath, Event.Name, str));
                }
            }

            return pdfCertificates;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CDs"></param>
        /// <param name="TemplatePath"></param>
        /// <param name="ExportPath"></param>
        /// <returns>List of generated file paths</returns>
        public static List<string> SaveCertificates(List<CertificateData> CDs, string ExportPath)
        {
            List<string> pdfCertificates = new List<string>();

            pdfCertificates.AddRange(SaveIndividualCertificates(CDs, ExportPath + "Individuals\\"));
            pdfCertificates.AddRange(SaveAllEventCertificates(CDs, ExportPath));
            pdfCertificates.AddRange(SaveAllEventCertificatesByType(CDs, ExportPath));

            return pdfCertificates;
        }
    }
}
