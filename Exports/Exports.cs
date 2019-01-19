//#define EXCEL
#define EEPLUS
#define SyncFusion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChampionshipSolutions.DM;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Diagnostics;
using ChampionshipSolutions.Reporting;
using System.Data;
using System.Threading;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Windows.Forms;
using ChampionshipSolutions.ViewModel;
using System.ComponentModel;
using ChampionshipSolutions.Reports;
#if ( EXCEL )
using Microsoft.Office.Interop.Excel;
#elif ( EEPLUS )
using OfficeOpenXml;
#endif

//#if ( EXCEL )
//#if ( EEPLUS )
//#error Can not compile with both EXCEL Interop and EEPlus
//#endif
//#if ( SyncFusion ) 
//#error Can not compile with both EXCEL Interop and SyncFusion
//#endif
//#endif

namespace ChampionshipSolutions
{

    public enum PrintOptions
    {
        NO_PRINT,
        A4_1,
        A4_2,
        A5_1,
        A5_2,
        A3_1,
        A3_2,
        PDF
    }

    public static class Exports
    {
#if ( EXCEL || EEPLUS )
        private static object ExcelLock = new object();
#endif

        /// <summary>
        /// Each event has a single export
        /// </summary>
        public static void exportCompetitors()
        {
            ChampionshipVM champVM = ((App)App.Current).CurrentChampionship; // MainWindow.CurrentChampionship;
            Championship champ = ((App)App.Current).CurrentChampionship.Championship; // MainWindow.CurrentChampionship;

            foreach (AEvent Event in champ.listAllEvents())
            {

                string ExportDir = champVM.getChampionshipExportsDir(); // @Properties.Settings.Default.ProgrammePath;
                string Extention = @".csv";

                string ExportPath = string.Format("{0}\\{1}{2}", ExportDir, Event.Name, Extention);

                TextWriter tw = new StreamWriter(ExportPath, false);

                foreach (Team team in champ.listAllTeams())
                {
                    
                    List<ACompetitor> AllAthletes = Event.getEnteredCompetitors(team);

                    string line = string.Empty;

                    foreach (Competitor athlete in AllAthletes)
                    {
                        line = string.Empty;

                        line += QuoteValue(Event.ToString()) + ",";
                        line += QuoteValue(team.ToString()) + ",";
                        line += QuoteValue(athlete.Vest.ToString()) + ",";
                        line += QuoteValue(athlete.getName()) + ",";

                        line = line.Remove(line.Length - 1);
                        tw.WriteLine(line);
                    }

                }//Each Event

                tw.Close();
            
            }// each team
        }

        /// <summary>
        /// Each Team has a single export
        /// </summary>
        public static void exportEnteredCompeitors ( )
        {
            ChampionshipVM champVM = ((App)App.Current).CurrentChampionship; // MainWindow.CurrentChampionship;
            Championship champ = ((App)App.Current).CurrentChampionship.Championship; // MainWindow.CurrentChampionship;

            string[] AgeGroups = new string[]{
                "Junior Boys","Junior Girls",
                "Intermediate Boys","Intermediate Girls",
                "Senior Boys","Senior Girls"};

            foreach ( Team t in champ.listAllTeams ( ) )
            {
                DataTable dt = new DataTable();
                dt.Columns.Add ( "Time" );
                dt.Columns.Add ( "Code" );
                dt.Columns.Add ( "Event" );
                dt.Columns.Add( "Vest" );
                dt.Columns.Add( "Name" );
                dt.Columns.Add( "School" );
                //dt.Columns.Add( "DOB" );
                //dt.Columns.Add ( "Competitor 1" );
                //dt.Columns.Add ( "Competitor 1 Vest" );
                //dt.Columns.Add ( "Competitor 1 School" );
                //dt.Columns.Add ( "Competitor 1 DOB" );
                //dt.Columns.Add ( "Competitor 2" );
                //dt.Columns.Add ( "Competitor 2 Vest" );
                //dt.Columns.Add ( "Competitor 2 School" );
                //dt.Columns.Add ( "Competitor 2 DOB" );
                //dt.Columns.Add ( "Competitor 3" );
                //dt.Columns.Add ( "Competitor 3 Vest" );
                //dt.Columns.Add ( "Competitor 3 School" );
                //dt.Columns.Add ( "Competitor 3 DOB" );
                //dt.Columns.Add ( "Note" );

                foreach ( AEvent Event in champ.listAllEvents ( ) )
                {
                    DataRow dr = dt.NewRow();

                    dt.Rows.Add ( dr );
                    if ( Event.StartTime.HasValue )
                        dr["Time"] = Event.StartTime.Value.ToShortTimeString( );
                    dr["Code"] = Event.ShortName;
                    dr["Event"] = Event.Name;

                    ACompetitor[] comps = Event.getEnteredCompetitors(t).ToArray();

                    foreach ( var comp in comps )
                    {
                        if ( comp == null ) continue;

                        DataRow cr = dt.NewRow();

                        if ( Event.StartTime.HasValue )
                            cr [ "Time" ] = Event.StartTime.Value.ToShortTimeString( );
                        cr [ "Code" ] = Event.ShortName;
                        cr [ "Event" ] = Event.Name;
                        cr [ "Name" ] = comp.Name;
                        cr [ "School" ] = comp.printParameter( "Attends" );
                        cr [ "Vest" ] = comp.printVestNumber( );

                        dt.Rows.Add( cr );
                    }
                    //if ( comps.Length == 0 ) continue;

                    //if ( comps.Length > 0 ) dr["Competitor 1"] = comps[0].Name;
                    //if ( comps.Length > 0 ) dr["Competitor 1 School"] = comps[0].printParameter ( "Attends" );
                    //if ( comps.Length > 0 ) dr["Competitor 1 DOB"] = comps[0].checkParameter ( "DateOfBirth" );
                    //if ( comps.Length > 0 ) dr["Competitor 1 Vest"] = comps[0].printVestNumber ( );
                    //if ( comps.Length > 1 ) dr["Competitor 2"] = comps[1].Name;
                    //if ( comps.Length > 1 ) dr["Competitor 2 School"] = comps[1].printParameter ( "Attends" );
                    //if ( comps.Length > 1 ) dr["Competitor 2 DOB"] = comps[1].checkParameter ( "DateOfBirth" );
                    //if ( comps.Length > 1 ) dr["Competitor 2 Vest"] = comps[1].printVestNumber ( );
                    //if ( comps.Length > 2 ) dr [ "Competitor 3" ] = comps [ 2 ].Name;
                    //if ( comps.Length > 2 ) dr [ "Competitor 3 School" ] = comps [ 2 ].printParameter( "Attends" );
                    //if ( comps.Length > 2 ) dr [ "Competitor 3 DOB" ] = comps [ 2 ].checkParameter( "DateOfBirth" );
                    //if ( comps.Length > 2 ) dr [ "Competitor 3 Vest" ] = comps [ 2 ].printVestNumber( );

                    //if ( comps.Length > 3 ) dr["Note"] = "There is at least one more competitor that can't be shown here.";
                }



                CSVReport csv = new CSVReport();
                string ExportPath = champVM.getChampionshipExportsDir() + "\\" + t.Name + ".csv"; 
                csv.Generate ( dt );
                csv.SaveGeneratedFile ( ExportPath );
            }// for Team

        }

        public static void exportSelectedCompeitors ( )
        {
            try
            {
                ChampionshipVM champVM = ((App)App.Current).CurrentChampionship; // MainWindow.CurrentChampionship;
                Championship champ = ((App)App.Current).CurrentChampionship.Championship; // MainWindow.CurrentChampionship;

                //string[] AgeGroups = new string[]{
                //"Minor Boys","Minor Girls",
                //"Junior Boys","Junior Girls",
                //"Intermediate Boys","Intermediate Girls",
                //"Senior Boys","Senior Girls"};

                DataTable dt = new DataTable();

                //dt.Columns.Add("Event");
                dt.Columns.Add ( "Competitor" );
                dt.Columns.Add ( "Team" );
                dt.Columns.Add ( "ThisPerformance" );
                dt.Columns.Add ( "BestPerformance" );
                dt.Columns.Add ( "DOB" );
                dt.Columns.Add ( "EventID" );
                dt.Columns.Add ( "Transport" );
                dt.Columns.Add ( "School" );
                dt.Columns.Add ( "Email" );
                dt.Columns.Add ( "ESAATransport" );


                foreach ( AEvent Event in champ.listAllEvents ( ) )
                {

                    foreach ( ACompetitor comp in Event.EnteredCompetitors.Where( c => c.SelectedForNextEvent ).OrderBy( c => c.printTeam ) ) 
                    {

                        DataRow dr = dt.NewRow();

                        dt.Rows.Add( dr );

                        dr [ "EventID" ] = Event.Name;

                        dr [ "Competitor" ] = comp.Name;
                        dr [ "Team" ] = comp.printTeam;

                        //SN5 5TE
                        //16 Pearl Road


                        object dobParamater = comp.checkParameter ( "DateOfBirth" );
                        if ( dobParamater is DateTime dob )
                            dr [ "DOB" ] = dob.ToShortDateString( );



                        string coach = ( from t in ((Competitor)comp).Athlete.GetAvailibilityNotes() where t.Championship == "South West" select t.TransportMethod ).FirstOrDefault();
                        string BP = ( from t in ((Competitor)comp).Athlete.GetAvailibilityNotes() where t.Championship == "South West" select t.PersonalBest ).FirstOrDefault();
                        var ESAA = ( from t in ((Competitor)comp).Athlete.GetAvailibilityNotes() where t.Championship == "National" select t ).FirstOrDefault();

                        if ( ESAA != null )
                            if ( ESAA.Availability == "Available" )
                                dr [ "ESAATransport" ] = ESAA.TransportMethod;
                            else
                                dr [ "ESAATransport" ] = "Not Available";
                        else
                            dr [ "ESAATransport" ] = "???";


                        //dr["Transport"] = comp.CoachForSW;
                        if ( coach != null )
                            dr["Transport"] = coach;

                        dr["School"] = ( (Athlete)( (Competitor)comp ).Athlete ).Attends.Name;

                        string email = string.Empty;

                        foreach ( string contact in ( (Athlete)( (Competitor)comp ).Athlete ).getAllContacts().Where(c => c is EmailContactDetail ).Select (c => c.printValue ))
                            email += contact + ";";

                        dr["Email"] = email;


                        //ResultValue BestRV;

                            if ( BP != null )
                            {
                                dr["BestPerformance"] = BP;
                            }

                        dr["ThisPerformance"] = comp.Result?.printResultValue;

                        //if ( comp.PersonalBest.HasValue ( ) )
                        //{

                        //    switch ( Event.ResultsDisplayDescription )
                        //    {
                        //        case ResultDisplayDescription.NotDeclared:
                        //            dr["BestPerformance"] = "Err";
                        //            break;
                        //        case ResultDisplayDescription.TimedMinuetsSeconds:
                        //            if ( comp.Result.Value.RawValue > comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        case ResultDisplayDescription.TimedMinuetsSecondsTenths:
                        //            if ( comp.Result.Value.RawValue > comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        case ResultDisplayDescription.TimedMinuetsSecondsHundreds:
                        //            if ( comp.Result.Value.RawValue > comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        case ResultDisplayDescription.TimedSecondsTenths:
                        //            if ( comp.Result.Value.RawValue > comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        case ResultDisplayDescription.TimedSecondsHundreds:
                        //            if ( comp.Result.Value.RawValue > comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        case ResultDisplayDescription.DistanceMeters:
                        //            if ( comp.Result.Value.RawValue < comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        case ResultDisplayDescription.DistanceMetersCentimeters:
                        //            if ( comp.Result.Value.RawValue < comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        case ResultDisplayDescription.DistanceCentimeters:
                        //            if ( comp.Result.Value.RawValue < comp.PersonalBest.RawValue )
                        //                dr["BestPerformance"] = comp.PersonalBestStr;
                        //            break;
                        //        default:
                        //            break;
                        //    }

                        //}

                    }

                }
                CSVReport csv = new CSVReport();
                string ExportPath = champVM.getChampionshipExportsDir ( ) + "\\" + "Selected.csv"; // @Properties.Settings.Default.TestExportPath + "Selected.csv";
                csv.Generate ( dt );
                csv.SaveGeneratedFile ( ExportPath );
                Process.Start ( ExportPath );
            }
            catch ( Exception ex )
            {
                MessageBox.Show ( "Failed to export Selected Competitors report" );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Failed to export Selected Competitors report" , ChampionshipSolutions.Diag.MessagePriority.Error );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message , ChampionshipSolutions.Diag.MessagePriority.Error );
            }

        }

        public static void exportStandards()
        {
            AEvent CurrentEvent = null; // for diagnostic purposes.
            try
            {

                ChampionshipVM champVM = ((App)App.Current).CurrentChampionship; // MainWindow.CurrentChampionship;
                Championship champ = ((App)App.Current).CurrentChampionship.Championship; // MainWindow.CurrentChampionship;
                //string[] AgeGroups = new string[]{
                //    "Minor Boys","Minor Girls",
                //    "Junior Boys","Junior Girls",
                //    "Intermediate Boys","Intermediate Girls",
                //    "Senior Boys","Senior Girls"};

                DataTable dt = new DataTable();

                dt.Columns.Add ( "Competitor" );
                dt.Columns.Add ( "Event" );
                dt.Columns.Add ( "BestPerformance" );
                dt.Columns.Add ( "Standard" );
                dt.Columns.Add ( "Selected" );
                dt.Columns.Add ( "Available" );



                foreach ( AEvent Event in champ.listAllEvents ( ) )
                {
                    CurrentEvent = Event;

                    if ( Event is IFinalEvent iEvent )
                    {
                        foreach ( AEvent heat in iEvent.getHeats( ) )
                        {
                            foreach ( ACompetitor comp in heat.getEnteredCompetitors( ).Where( c => c.Result != null ).Where( c => c.Result.achievedStandardsShort.Length > 1 ) ) // Event.EnteredCompetitors.Where(c => c.Result != null).Where(c => c.Result.achievedStandardsShort.Length > 1))
                            {
                                try
                                {

                                    DataRow dr = dt.NewRow();

                                    dt.Rows.Add( dr );

                                    dr [ "Event" ] = heat.Name;
                                    dr [ "Competitor" ] = comp.Name;
                                    dr [ "Standard" ] = ((IHeatedCompetitor) comp).HeatResult.achievedStandardsShort;
                                    dr [ "Selected" ] = comp.SelectedForNextEvent;
                                    dr [ "Available" ] = comp.AvilableForSW;
                                    dr [ "BestPerformance" ] = ((IHeatedCompetitor) comp).HeatResult.printResultValue;
                                }
                                catch ( Exception ex )
                                {
                                    Console.WriteLine( "Failed to export standards info for " + comp.Name );
                                    Console.WriteLine( ex.Message );
                                    continue;
                                }
                            }
                        }
                    }

                    foreach ( ACompetitor comp in Event.getEnteredCompetitors ( ).Where ( c => c.Result != null ).Where ( c => c.Result.achievedStandardsShort.Length > 1 ) )
                    {

                        DataRow dr = dt.NewRow();

                        dt.Rows.Add ( dr );

                        dr["Event"] = Event.Name;
                        dr["Competitor"] = comp.Name;
                        if ( comp.Result != null )
                        {
                            dr["Standard"] = comp.Result.achievedStandardsShort;
                            dr["BestPerformance"] = comp.Result.printResultValue;
                        }
                        dr["Selected"] = comp.SelectedForNextEvent;
                        dr["Available"] = comp.AvilableForSW;

                    }

                    AResult res = Event.getResult(1);

                    if ( res != null )
                    {
                        if ( res.achievedStandardsShort == "" )
                        {
                            DataRow dr = dt.NewRow();

                            dt.Rows.Add ( dr );

                            dr["Event"] = Event.Name;
                            dr["Competitor"] = res.Competitor.Name;
                            dr["Standard"] = "1st Place - No standard";
                            dr["Selected"] = res.Competitor.SelectedForNextEvent;
                            dr["Available"] = res.Competitor.AvilableForSW;
                            dr["BestPerformance"] = res.Competitor.Result.printResultValue;
                        }
                    }

                }

                CSVReport csv = new CSVReport();
                string ExportPath = champVM.getChampionshipExportsDir() + "\\" + "Standards.csv"; // @Properties.Settings.Default.TestExportPath + "Standards.csv";
                csv.Generate ( dt );
                csv.SaveGeneratedFile ( ExportPath );
                Process.Start ( ExportPath );
            }
            catch (Exception ex)
            {
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Failed to export Standards Achieved report" , ChampionshipSolutions.Diag.MessagePriority.Error );
                if ( CurrentEvent != null )
                {
                    MessageBox.Show ( "Failed to export Standards Achieved report. The error occurred whilst processing " + CurrentEvent.Name );
                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( "The error occurred whilst processing " + CurrentEvent.Name , ChampionshipSolutions.Diag.MessagePriority.Error );
                }
                else
                {
                    MessageBox.Show ( "Failed to export Standards Achieved report." );
                }

                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message , ChampionshipSolutions.Diag.MessagePriority.Error );
            }
        }

        public static void exportSchoolsListForSelection()
        {
            ChampionshipVM champVM = ((App)App.Current).CurrentChampionship; // MainWindow.CurrentChampionship;
            Championship champ = ((App)App.Current).CurrentChampionship.Championship; // MainWindow.CurrentChampionship;

            List<School> schools = new List<School>();

            // bodge to allow this to work with SW schools
            if ( champ.Name.StartsWith("SW"))
                champ.listAllTeams().Where(t => t.Name == "Wiltshire").ToList()
                    .ForEach(t => schools.AddRange(t.HasSchools));
            else
                champ.listAllTeams()
                    .ForEach(t => schools.AddRange(t.HasSchools));



            DataTable dt = new DataTable();

            dt.Columns.Add("School");
            dt.Columns.Add("Competitor");
            dt.Columns.Add("PhoneNumber");

            foreach (School sch in schools)
            {
                if (sch.CountSelected(champ) == 0) continue;


                foreach (ACompetitor comp in sch.getSelectedForNextEvent(champ).OrderBy(c => c.Name)) // Event.EnteredCompetitors.Where(c => c.Result != null).Where(c => c.Result.achievedStandardsShort.Length > 1))
                {

                    DataRow dr = dt.NewRow();

                    dt.Rows.Add(dr);

                    dr["School"] = sch.Name;
                    dr["Competitor"] = comp.Name;

                    try
                    {
                        List<AContactDetail> numbers = ((Competitor)comp).Athlete.getAllContacts();
                        string strnumbers = "";
                        foreach (AContactDetail cd in numbers)
                        {
                            strnumbers += cd.printValue + @" \ ";
                        }

                        dr["PhoneNumber"] = strnumbers.TrimEnd(' ').TrimEnd('\\').Trim();
                    }
                    catch { }
                }
            }

            CSVReport csv = new CSVReport();
            string ExportPath = champVM.getChampionshipExportsDir() + "\\" + "SchoolsPaymentNational.csv"; // @Properties.Settings.Default.TestExportPath + "SchoolsPaymentNational.csv";
            csv.Generate(dt);
            csv.SaveGeneratedFile ( ExportPath ) ;
        }

        public static void exportSchoolsListForContacts()
        {
            ChampionshipVM champVM = ((App)App.Current).CurrentChampionship; // MainWindow.CurrentChampionship;
            Championship champ = ((App)App.Current).CurrentChampionship.Championship; // MainWindow.CurrentChampionship;

            List<School> schools = new List<School>();

            // bodge to allow this to work with SW schools
            if ( champ.Name.StartsWith("SW"))
                champ.listAllTeams().Where(t => t.Name == "Wiltshire").ToList()
                    .ForEach(t => schools.AddRange(t.HasSchools));
            else
                champ.listAllTeams()
                    .ForEach(t => schools.AddRange(t.HasSchools));

            DataTable dt = new DataTable();

            dt.Columns.Add("Area");
            dt.Columns.Add("School");

            dt.Columns.Add("Contact 1 Name");
            dt.Columns.Add("Contact 1 Email");

            dt.Columns.Add("Contact 2 Name");
            dt.Columns.Add("Contact 2 Email");

            dt.Columns.Add("Contact 3 Name");
            dt.Columns.Add("Contact 3 Email");

            foreach (School sch in schools)
            {
                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);

                dr["Area"] = sch.inTeams.First().Name;
                dr["School"] = sch.Name;

                int c = 1;

                //throw new NotImplementedException ( "Contacts for Schools needs to be re-written" );

                //foreach (AContactDetail contact in sch.getAllContacts())//.getSelectedForNextEvent(champ).OrderBy(c => c.Name)) // Event.EnteredCompetitors.Where(c => c.Result != null).Where(c => c.Result.achievedStandardsShort.Length > 1))
                //{

                //    if ( ! ( contact is EmailContactDetail ) ) continue;

                //    EmailContactDetail eContact = (EmailContactDetail)contact;

                //    string Name, Email;


                //    Name = contact.ContactName;
                //    Email = eContact.EmailAddress;

                //    switch (c)
                //    {
                //        case 1:
                //            dr["Contact 1 Name"] = Name;
                //            dr["Contact 1 Email"] = Email;
                //            c++;
                //            break;

                //        case 2:
                //            dr["Contact 2 Name"] = Name;
                //            dr["Contact 2 Email"] = Email;
                //            c++;
                //            break;

                //        case 3:
                //            dr["Contact 3 Name"] = Name;
                //            dr["Contact 3 Email"] = Email;
                //            c++;
                //            break;

                //        default:
                //            c = 1;
                //            break;
                //    }
                //}
            }

            CSVReport csv = new CSVReport();
            string ExportPath = champVM.getChampionshipExportsDir() + "\\" + "SchoolsContacts.csv"; // @Properties.Settings.Default.TestExportPath + "SchoolsContacts.csv";
            csv.Generate(dt);
            csv.SaveGeneratedFile ( ExportPath );
        }
    
        private static string QuoteValue(string value)
        {
            return String.Concat("\"", value.Replace("\"", "\"\""), "\"");
        }

#region Vest Numbers
     
        public static void GenerateVestNumbers(List<Team> teams, List<AEvent> events)
        {
            Thread printVestThread = new Thread(new ParameterizedThreadStart(_generateVestNumbers));
            printVestThread.Start(new ExportInstuctions(events, teams, PrintOptions.NO_PRINT));
        }

        private static void _generateVestNumbers(object Instructions)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            PDFReport VestPDF;
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("School");
            dataTable.Columns.Add("AreaShortName");
            dataTable.Columns.Add("AreaName");
            dataTable.Columns.Add("VestNo");
            dataTable.Columns.Add("EventCode");

            ExportInstuctions instructions = (ExportInstuctions)Instructions;

            string lFolderPath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\" ; // @Properties.Settings.Default.VestExportPath;


            List<AEvent> events = instructions.events;
            List<Team> teams = instructions.areas;
            
            bool print = ( instructions.print != PrintOptions.NO_PRINT );

            Wait wait = new Wait();
            wait.Bar1.setLabel("Competitors");
            wait.Bar2.setLabel("Events");
            wait.Bar3.setLabel("teams");
            wait.Bar1.setEnabled(true);
            wait.Bar2.setEnabled(true);
            wait.Bar3.setEnabled(true);

            wait.Bar3.setMax(teams.Count);

            Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
            t.Start(wait);


            foreach (Team a in teams)
            {

                wait.Bar2.setValue(0);
                foreach (AEvent Event in events)
                {
                    Template template = Event.VestTemplate;

                    try
                    {
                        //VestPDF = (PDFReport)AReport.LoadTemplate(TemplatePath);
                        //template = "XCVestNumber";
                        //VestPDF = (PDFReport)( (App)App.Current ).ReportLibrary[template]; // (PDFReport)MainWindow.ReportLibrary[TemplatePath];
                        VestPDF = (PDFReport) AReport.LoadTemplate ( template.Instructions , template.TemplateFile );
                    }
                    catch
                    {
                        wait.Complete();
                        t.Abort();
                        return;
                    }

                    List<ACompetitor> lsCompetitors = Event.getEnteredCompetitors(a);  // e.EnteredCompetitors.Where( c => c.getTeam() == a).ToList();

                    if (lsCompetitors.Count > 0)
                    {
                        wait.Bar1.setValue(0);
                        wait.Bar1.setMax(lsCompetitors.Count);

                        // start a new data table
                        dataTable.Rows.Clear();

                        foreach (Competitor c in lsCompetitors)
                        {
                            //StudentCompetitor sc = (StudentCompetitor)c;

                            DataRow dr = dataTable.NewRow();

                            dr["AreaName"] = a.Name.ToUpper();
                            dr["EventCode"] = Event.ShortName;
                            dr["VestNo"] = c.Vest.ToString();

                            string SchoolName = "";
                            if (c.checkParameter("Attends") != null)
                                SchoolName = c.checkParameter("Attends").ToString();

                            object yearGroup = c.checkParameter("YearGroup");
                            dr["Name"] = c.Athlete.PrintName() + " - Year " + (yearGroup != null ? yearGroup.ToString() : " ").ToString(); // sc.formatYearGroup();
                            dr["School"] = SchoolName;
                            dr["AreaShortName"] = a.ShortName;
                            //string BarCode = "*" + "V-" + c.Vest.ToString() + "%E-" + e.ShortName + "*";
                            //dr["VestBarcode"] = BarCode;

                            dataTable.Rows.Add(dr);

                            wait.Bar1.increment();

                        } // end of for competitors


                        VestPDF.Generate(dataTable);
                        VestPDF.SaveGeneratedFile ( lFolderPath + a.Name + " " + Event.ShortName + " Vests.pdf" );
                    }// end if there are competitors
                    wait.Bar2.increment();
                } // end of for events
                wait.Bar3.increment();
            } // end of for areas

            wait.Complete();
            t.Abort();

            sw.Stop();


        }

#endregion

#region Generate Athlete Profiles

        private static AutoResetEvent _resetForAthleteProfile = new AutoResetEvent(false);

        public static string GenerateAthleteProfile ( List<Athlete> Athletes , bool Open = false , PrintOptions Print = PrintOptions.NO_PRINT , bool synchronis = false , bool save = true )
        {

            BackgroundWorker worker = new BackgroundWorker();

            string generatedFile = null;

            worker.DoWork += ( o , a ) =>
            {
                _resetForAthleteProfile.Reset ( );
                generatedFile = _generateAthleteProfile ( new ExportInstuctions ( Athletes , Open , Print ) { Save = save } );
            };

            worker.RunWorkerCompleted += ( o , a ) =>
            {
                _resetForAthleteProfile.Set ( );
            };

            worker.RunWorkerAsync ( );

            Thread.Sleep ( 10 );

            if ( synchronis )
                _resetForAthleteProfile.WaitOne ( new TimeSpan ( 0 , 0 , 10 ) );

            return generatedFile;
        }

        private static string _generateAthleteProfile ( object Instructions )
        {
            Stopwatch sw = new Stopwatch();

            sw.Start ( );

            List<string> filePaths = new List<string>();

            PDFReport ProfilePDF;

            #region prepare data tables

            DataSet ds = new DataSet();
            DataTable PageFields = new DataTable(AReport.PAGEFIELDS);
            DataTable NotesTable = new DataTable("NotesTable");

            ds.Tables.Add ( PageFields );
            ds.Tables.Add ( NotesTable );

            PageFields.Columns.Add ( "Index" , typeof ( int ) );
            PageFields.Columns.Add ( "AthleteName" , typeof ( string ) );
            PageFields.Columns.Add ( "School" , typeof ( string ) );
            PageFields.Columns.Add ( "Team" , typeof ( string ) );
            PageFields.Columns.Add ( "DOB" , typeof ( string ) );
            PageFields.Columns.Add ( "ESAAReg" , typeof ( string ) );
            PageFields.Columns.Add ( "TShirt" , typeof ( string ) );

            NotesTable.Columns.Add ( "PageIndex" , typeof ( int ) );
            NotesTable.Columns.Add ( "Date" , typeof ( string ) );
            NotesTable.Columns.Add ( "String" , typeof ( string ) );

            #endregion

            ExportInstuctions instructions = (ExportInstuctions)Instructions;

            // To do set the output path properly
            string lFolderPath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\"; // @Properties.Settings.Default.TestExportPath;

            List<Athlete> Athletes = instructions.athletes;
            bool print = ( instructions.print != PrintOptions.NO_PRINT );

            Wait wait = new Wait();
            wait.Bar1.setLabel ( "Athletes" );
            wait.Bar1.setEnabled ( true );

            Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
            t.Start ( wait );

            wait.Bar1.setValue ( 0 );
            wait.Bar1.setMax ( Athletes.Count );

            #region for each Athlete

            try
            {
                ProfilePDF = (PDFReport)CSReportLibrary.getLibrary( )["Athlete Profile"];
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
                //wait.Bar1.increment( );
                wait.Complete ( );
                t.Abort ( );
                return null;
            }

            if ( ProfilePDF == null )
            {
                wait.Complete( );
                t.Abort( );
                throw new Exception( "Could not find a template called 'Athlete Profile'" );
            }


            foreach ( Athlete ath in Athletes )
            {

                Console.WriteLine ( "Making event card for " + ath.Fullname );

                Championship Championship = ( (App)App.Current ).CurrentChampionship.Championship;

                // start a new data table
                PageFields.Rows.Clear ( );
                NotesTable.Rows.Clear ( );

                // Build Main Page info
                DataRow drPage = PageFields.NewRow();

                drPage["Index"] = 0;
                drPage["AthleteName"] = ath.Fullname;

                if ( ath.Attends != null )
                    drPage["School"] = ath.Attends.Name;
                else
                    drPage["School"] = "";

                if ( ath.getTeam ( Championship ) != null )
                    drPage["Team"] = ath.getTeam ( ( (App)App.Current ).CurrentChampionship.Championship ).Name;

                if ( ath.DateOfBirth != null )
                    drPage["DOB"] = ath.DateOfBirth.Value.ToShortDateString ( );

                if ( ath.GetPublicNotes ( ).Where ( p => p.Key == "ESAA_Registration" ).Count ( ) == 1 )
                    drPage["ESAAReg"] = ath.GetPublicNotes ( ).Where ( p => p.Key == "ESAA_Registration" ).Select ( p => p.Note ).First ( );
                else
                    drPage["ESAAReg"] = "??";

                if ( ath.GetPublicNotes ( ).Where ( p => p.Key == "TShirt" ).Count ( ) == 1 )
                    drPage["TShirt"] = ath.GetPublicNotes ( ).Where ( p => p.Key == "TShirt" ).Select ( p => p.Note ).First ( );
                else
                    drPage["TShirt"] = "??";

                PageFields.Rows.Add ( drPage );

                List<AEvent> SelectedFor = ath.CompetingAs(Championship).Where(c => c.SelectedForNextEvent).Select(c => c.CompetingIn).ToList();
                List<AResult> Results = ath.getAllResults(Championship);
                List<AContactDetail> Contacts = ath.getAllContacts();
                List<PublicNote> Notes = ath.GetPublicNotes();
                Notes.AddRange ( ath.GetAvailibilityNotes ( ) );
                List<PreviousResult> PreviousResults = ath.GetPreviousResultsNotes();
                //List<PowerOfTenResult> PowerOfTen = ath.getAllResults(Championship);

                // Console.WriteLine ( "generating result card for " + Event.Name );

                #region for each competitor

                // Build Notes Table
                foreach ( AEvent Event in SelectedFor )
                {
                    DataRow dr = NotesTable.NewRow();
                    dr["PageIndex"] = drPage["Index"];
                    dr["Date"] = "";

                    dr["String"] = string.Format ( "Selected for {0} at the South West championships" ,
                        Event.Name );

                    NotesTable.Rows.Add ( dr );

                }

                foreach ( AResult result in Results )
                {

                    DataRow dr = NotesTable.NewRow();
                    dr["PageIndex"] = drPage["Index"];
                    if ( Championship.Date.HasValue )
                        dr["Date"] = Championship.Date.Value.ToShortDateString ( );
                    else
                        dr["Date"] = "";

                    dr["String"] = string.Format ( "{0} - {1} - {2}" ,
                        result.Event.Name , result.Rank , result.ResultStr );



                    NotesTable.Rows.Add ( dr );

                } // end of for Results


                foreach ( AContactDetail contact in Contacts )
                {

                    DataRow dr = NotesTable.NewRow();
                    dr["PageIndex"] = drPage["Index"];
                    dr["Date"] = "";
                    dr["String"] = contact.printValue.ToString().Trim().Replace("<","").Replace(">","");

                    NotesTable.Rows.Add ( dr );
                } // end of for Contacts

                foreach ( PublicNote note in Notes )
                {
                    if ( note.Key == "TShirt" ) continue;
                    if ( note.Key == "ESAA_Registration" ) continue;

                    DataRow dr = NotesTable.NewRow();
                    dr["PageIndex"] = drPage["Index"];
                    dr["Date"] = note.EnteredDate.ToShortDateString ( );
                    dr["String"] = note.PrintNote;

                    NotesTable.Rows.Add ( dr );

                } // end of for Notes

                foreach ( PreviousResult pResult in PreviousResults )
                {

                    DataRow dr = NotesTable.NewRow();
                    dr["PageIndex"] = drPage["Index"];
                    if ( pResult.EventDate.HasValue )
                        dr["Date"] = pResult.EventDate.Value.ToShortDateString ( );
                    else
                        dr["Date"] = "";
                    dr["String"] = pResult.PrintNote;

                    NotesTable.Rows.Add ( dr );

                } // end of for Previous Results

                #endregion

                string ExportPath = lFolderPath + ath.PreferredName + " Profile.pdf";
                byte[] generatedPDF;

                generatedPDF = ProfilePDF.Generate ( ds );
                try
                {
                    if ( instructions.Save )
                    {
                        ProfilePDF.SaveGeneratedFile ( ExportPath );
                        filePaths.Add ( ExportPath );
                    }
                    if ( instructions.open )
                        PDFViewer.OpenOnSTAThread ( new MemoryStream ( generatedPDF ) );
                }
                catch ( Exception ex )
                {
                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( "An error occurred whilst saving " + ath.PreferredName + " Profile.pdf" , ChampionshipSolutions.Diag.MessagePriority.Error );
                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message , ChampionshipSolutions.Diag.MessagePriority.Error );
                    wait.Invoke ( (MethodInvoker)delegate { MessageBox.Show ( ex.Message ); } );
                }
                wait.Bar1.increment ( );
            } // end of for events

            #endregion

            string finishedFilePath = null;

            if ( filePaths.Count == 1 )
            {
                finishedFilePath = filePaths.First ( );

                if ( instructions.open )
                    PDFViewer.OpenOnSTAThread( finishedFilePath );

                if ( instructions.print != PrintOptions.NO_PRINT )
                {
                    string p = null;

                    switch ( instructions.print )
                    {
                        case PrintOptions.A5_1:
                            p = "A5-1";
                            break;
                        case PrintOptions.A5_2:
                            p = "A5-2";
                            break;
                        case PrintOptions.PDF:
                            p = "PDF";
                            break;
                        default:
                            p = null;
                            break;
                    }

                    Printing.PrintPDF ( finishedFilePath , p);

                }
            }
            else if ( filePaths.Count > 1 )
            {
                try
                {
                    finishedFilePath = lFolderPath + "Combined Profile Cards.pdf";
                    MergeMultiplePDFIntoSinglePDF ( finishedFilePath , filePaths.ToArray ( ) );
                }
                catch ( Exception ex )
                {
                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( "An error occurred whilst saving Combined Profile Cards.pdf" , ChampionshipSolutions.Diag.MessagePriority.Error );
                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message , ChampionshipSolutions.Diag.MessagePriority.Error );
                    wait.Invoke ( (MethodInvoker)delegate { MessageBox.Show ( ex.Message ); } );
                }


                if ( instructions.open )
                    PDFViewer.OpenOnSTAThread( finishedFilePath );

                if ( instructions.print != PrintOptions.NO_PRINT )
                {
                    Printing.PrintPDF ( finishedFilePath );
                }
            }

            wait.Complete ( );
            t.Abort ( );

            sw.Stop ( );

            Debug.WriteLine ( "Created profile cards in " + sw.Elapsed.TotalSeconds + "s" );

            Console.WriteLine ( "**************************" );
            Console.WriteLine ( "********DB Reads = " + FileIO.FConnFile.GetFileDetails ( ).IO.DBReadCounter + "*****" );
            Console.WriteLine ( "*****Cache Reads = " + FileIO.FConnFile.GetFileDetails ( ).IO.CacheReadCounter + "***" );
            Console.WriteLine ( "**************************" );

            return finishedFilePath;
        }


#endregion

#region Generate Result Entry Forms

        private static object EntryFromLock = new object();

        public static string GenerateResultEntryForms(List<AEvent> events, bool Open = false, bool Print = false, bool synchronis = false)
        {

            // Added in 2018-06-02 as the non synchronous functions were breaking the synchronous ones.
            lock ( EntryFromLock )
            {
                AutoResetEvent _resetEventForEntryForm = new AutoResetEvent(false);
                BackgroundWorker worker = new BackgroundWorker();

                string generatedFile = null;

                worker.DoWork += ( o , a ) =>
                {
                    _resetEventForEntryForm.Reset( );
                    generatedFile = _generateResultEntryForms( new ExportInstuctions( events , Open , (Print ? PrintOptions.A4_1 : PrintOptions.NO_PRINT) ) );
                };

                worker.RunWorkerCompleted += ( o , a ) =>
                {
                    _resetEventForEntryForm.Set( );
                };

                worker.RunWorkerAsync( );
                if ( synchronis )
                    _resetEventForEntryForm.WaitOne( );

                return generatedFile;
            }
        }

        private static object ResultEntryLock = new object();

        private static string _generateResultEntryForms ( object Instructions )
        {
            lock ( ResultEntryLock )
            {
                Stopwatch sw = new Stopwatch();

                sw.Start ( );

                List<string> filePaths = new List<string>();

                PDFReport VestPDF;

#region prepare data tables

                DataSet ds = new DataSet();
                DataTable PageFields = new DataTable(AReport.PAGEFIELDS);
                DataTable ResultsTable = new DataTable("ResultsTable");
                DataTable StdTable = new DataTable("StdTable");
                DataTable Heat1 = new DataTable("Heat1");
                DataTable Heat2 = new DataTable("Heat2");
                DataTable Final = new DataTable("Final");

                ds.Tables.Add ( PageFields );
                ds.Tables.Add ( ResultsTable );
                ds.Tables.Add ( StdTable );
                ds.Tables.Add ( Heat1 );
                ds.Tables.Add ( Heat2 );
                ds.Tables.Add ( Final );

                PageFields.Columns.Add ( "Index" , typeof ( int ) );
                PageFields.Columns.Add ( "ChampionshipName" , typeof ( string ) );
                PageFields.Columns.Add ( "EventName" , typeof ( string ) );
                PageFields.Columns.Add ( "AgeGroup" , typeof ( string ) );
                PageFields.Columns.Add ( "StartTime" , typeof ( string ) );
                PageFields.Columns.Add ( "HeatStartTime" , typeof ( string ) );
                PageFields.Columns.Add ( "EventID" , typeof ( string ) );
                PageFields.Columns.Add ( "HeatEventID" , typeof ( string ) );
                PageFields.Columns.Add ( "EventBarcode" , typeof ( string ) );
                PageFields.Columns.Add ( "Note" , typeof ( string ) );

                PageFields.Columns.Add ( "Jump1" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump2" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump3" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump4" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump5" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump6" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump7" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump8" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump9" , typeof ( string ) );
                PageFields.Columns.Add ( "Jump10" , typeof ( string ) );

                ResultsTable.Columns.Add ( "PageIndex" , typeof ( int ) );
                ResultsTable.Columns.Add ( "TableIndex" , typeof ( int ) );
                ResultsTable.Columns.Add ( "HeatLane" , typeof ( string ) );
                ResultsTable.Columns.Add ( "Lane" , typeof ( string ) );
                ResultsTable.Columns.Add ( "ShadeFinal" , typeof ( bool ) );
                ResultsTable.Columns.Add ( "Vest" , typeof ( string ) );
                ResultsTable.Columns.Add ( "Heat" , typeof ( string ) );
                ResultsTable.Columns.Add ( "CompetitorsName" , typeof ( string ) );

                ResultsTable.Columns.Add ( "HeatRank" , typeof ( string ) );
                ResultsTable.Columns.Add ( "HeatValue" , typeof ( string ) );
                ResultsTable.Columns.Add ( "HeatStdStr" , typeof ( string ) );

                ResultsTable.Columns.Add ( "Rank" , typeof ( string ) );
                ResultsTable.Columns.Add ( "Value" , typeof ( string ) );
                ResultsTable.Columns.Add ( "StdStr" , typeof ( string ) );

                StdTable.Columns.Add ( "PageIndex" , typeof ( int ) );
                StdTable.Columns.Add ( "StdName" , typeof ( string ) );
                StdTable.Columns.Add ( "StdValue" , typeof ( string ) );

#endregion

                ExportInstuctions instructions = (ExportInstuctions)Instructions;

                // To do set the output path properly
                string lFolderPath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\"; // @Properties.Settings.Default.TestExportPath;

                List<AEvent> events = instructions.events;
                bool print = ( instructions.print != PrintOptions.NO_PRINT );

                Wait wait = new Wait();
                wait.Bar1.setLabel ( "Events" );
                wait.Bar2.setLabel ( "Competitor" );
                wait.Bar1.setEnabled ( true );
                wait.Bar2.setEnabled ( true );

                Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
                t.Start ( wait );

                wait.Bar1.setValue ( 0 );
                wait.Bar1.setMax ( events.Count );

#region for each event

                foreach ( AEvent Event in events )
                {

                    Console.WriteLine ( "Making event card for " + Event.Name );

                    //if ( Event is ILaneAssignedEvent )
                        //((ILaneAssignedEvent)Event ).updateLanes ( );

                    // we always ignore heat events
                    if ( Event is IHeatEvent ) continue;

                    Template template = Event.DataEntryTemplate;


                    try
                    {
                        //VestPDF = (PDFReport)AReport.LoadTemplate ( TemplatePath );
                        //VestPDF = (PDFReport)( (App)App.Current ).ReportLibrary[Event.DataEntryTemplate];// (PDFReport)AReport.LoadTemplate ( TemplatePath );
                        //VestPDF = (PDFReport)( (App)App.Current ).ReportLibrary["EventResultsCardField"];// (PDFReport)AReport.LoadTemplate ( TemplatePath );
                        VestPDF = (PDFReport)AReport.LoadTemplate ( template.Instructions , template.TemplateFile );
                    }
                    catch ( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine ( ex.Message );
                        wait.Bar1.increment ( );
                        continue;
                        //wait.Complete ( );
                        //t.Abort ( );
                        //return;
                    }


                    // start a new data table
                    PageFields.Rows.Clear ( );
                    StdTable.Rows.Clear ( );
                    ResultsTable.Rows.Clear ( );

                    // Build Main Page info
                    DataRow drPage = PageFields.NewRow();

                    drPage["Index"] = 0;
                    drPage["ChampionshipName"] = Event.Championship.Name;

                    if ( Event.customFieldExists ( "PrintedName" ) )
                        drPage["EventName"] = Event.getValue ( "PrintedName" );
                    else
                        drPage["EventName"] = Event.Name;

                    if ( Event.customFieldExists ( "AgeGroup" ) )
                        drPage["AgeGroup"] = Event.getValue ( "AgeGroup" );

                    if ( Event.StartTime != null )
                        drPage [ "StartTime" ] = Event.StartTime.Value.ToShortTimeString( );

                    if ( Event.customFieldExists ( "ProgramID" ) )
                        drPage["EventID"] = Event.getValue ( "ProgramID" );
                    else
                        drPage["EventID"] = Event.ShortName;

                    if ( Event.customFieldExists ( "Barcode" ) )
                        drPage["EventBarcode"] = Event.getValue ( "Barcode" ).ToString ( ).ToUpper ( ).Trim ( );
                    else
                        drPage["EventBarcode"] = string.Format ( "*C-{0}%E-{1}*" , Event.Championship.ShortName , Event.ShortName ).ToUpper ( );


                    if ( Event.customFieldExists ( "Jump1" ) )
                        drPage["Jump1"] = Event.getValue ( "Jump1" );

                    if ( Event.customFieldExists ( "Jump2" ) )
                        drPage["Jump2"] = Event.getValue ( "Jump2" );

                    if ( Event.customFieldExists ( "Jump3" ) )
                        drPage["Jump3"] = Event.getValue ( "Jump3" );

                    if ( Event.customFieldExists ( "Jump4" ) )
                        drPage["Jump4"] = Event.getValue ( "Jump4" );

                    if ( Event.customFieldExists ( "Jump5" ) )
                        drPage["Jump5"] = Event.getValue ( "Jump5" );

                    if ( Event.customFieldExists ( "Jump6" ) )
                        drPage["Jump6"] = Event.getValue ( "Jump6" );

                    if ( Event.customFieldExists ( "Jump7" ) )
                        drPage["Jump7"] = Event.getValue ( "Jump7" );

                    if ( Event.customFieldExists ( "Jump8" ) )
                        drPage["Jump8"] = Event.getValue ( "Jump8" );

                    if ( Event.customFieldExists ( "Jump9" ) )
                        drPage["Jump9"] = Event.getValue ( "Jump9" );

                    if ( Event.customFieldExists ( "Jump10" ) )
                        drPage["Jump10"] = Event.getValue ( "Jump10" );

                    if ( Event.customFieldExists( "Note" ) )
                        drPage [ "Note" ] = Event.getValue( "Note" );

                    PageFields.Rows.Add ( drPage );

                    List<ACompetitor> lsCompetitors = Event.EnteredCompetitors.ToList();

                    wait.Bar2.setValue ( 0 );
                    wait.Bar2.setMax ( lsCompetitors.Count );

                    if ( Event is IFinalEvent )
                    {
                        if ( Event.customFieldExists ( "ProgramID" ) )
                            drPage["EventID"] = "H:" + ( (IFinalEvent)Event ).getHeats ( )[0].getValue ( "ProgramID" ) + " F:" + Event.getValue ( "ProgramID" );
                        else
                            drPage["EventID"] = string.Format ( "H: {0} F: {1}" , ( (IFinalEvent)Event ).getHeats ( )[0].ShortName , Event.ShortName );

                        if ( ((IFinalEvent) Event).getHeats( ) [ 0 ].StartTime != null )
                        // we have a heat start time to put on the results card
                        {
                            if ( Event.StartTime.HasValue )
                            {
                                // we have already set the final start time so lets modify that value rather than use HeatStartTime.
                                // this gives us better flexibility over the format of the string.
                                // Added 2017-05-12
                                drPage [ "StartTime" ] = $"Heats: {((IFinalEvent) Event).getHeats( ) [ 0 ].StartTime.Value.ToShortTimeString( )} Final: { Event.StartTime.Value.ToShortTimeString( )}";
                            }
                            else
                            {
                                drPage [ "HeatStartTime" ] = "Heat Start :" + ((IFinalEvent) Event).getHeats( ) [ 0 ].StartTime.Value.ToShortTimeString( );
                            }
                        }
                    }

                    //!**! added for safety but does slow things down
                    //Event.voidStorage ( );

                   // Console.WriteLine ( "generating result card for " + Event.Name );

#region for each competitor

                    // Build Results Table
                    foreach ( ACompetitor c in lsCompetitors.OrderBy ( p => p.Vest.IntOrder ).ThenBy( p => p.Vest.ToString() ) )
                    {
                        if ( c is SpecialConsideration ) continue;

                        DataRow dr = ResultsTable.NewRow();
                        dr["PageIndex"] = drPage["Index"];
                        if ( c is StudentCompetitor )
                            dr["CompetitorsName"] = c.Name + " (" + c.printParameter ( "Attends" ) + ")";
                        else
                            dr["CompetitorsName"] = c.Name;

                        dr["Vest"] = c.printVestNumber ( );

                        if ( Event is IFinalEvent )
                        {
                            if ( c is ILanedHeatedCompetitor heatedC )
                            {
                                dr [ "Heat" ] = ((IFinalEvent) Event).getHeatNumber( heatedC.HeatEvent ).ToString( );

                                if ( dr [ "Heat" ].ToString( ) == "0" )
                                    dr [ "Heat" ] = string.Empty;

                                if ( heatedC.HeatLaneNumber > 0 )
                                    dr [ "HeatLane" ] = ((ILanedHeatedCompetitor) c).HeatLaneNumber;
                                else
                                    dr [ "HeatLane" ] = " ";
                                
                                //dr["TableIndex"] = ( ( (IFinalEvent)Event ).getHeatNumber ( (IHeatEvent)( (ILanedHeatedCompetitor)c ).HeatEvent ) * 10 ) + ( ( (ILanedHeatedCompetitor)c ).HeatLaneNumber );
                                dr [ "TableIndex" ] = ResultsTable.Rows.Count + 1;
                            }
                            else
                            {
                                // error condition
                            }
                        }

                        if ( c is IHeatedCompetitor ch )
                        {
                            if ( ch.HeatEvent?.hasResultFor( (ACompetitor) c ) == true)
                            {
                                dr [ "HeatRank" ] = (ch.HeatEvent.getResult( c )).printRank;
                                dr [ "HeatValue" ] = (ch.HeatEvent.getResult( c )).printResultValue;
                                dr [ "HeatStdStr" ] = ch.HeatEvent.getStandardShortString( (ch.HeatEvent.getResult( c ).Value) );
                            }

                            if ( ch.isInFinal( ) )
                            {
                                // " " with space to trick the ShadeOnDefault property into not shading
                                if ( c.hasLaneNumber( ) )
                                    dr [ "Lane" ] = c.LaneNumber.ToString( );
                                else
                                    dr [ "Lane" ] = " ";

                                dr [ "Rank" ] = " ";
                                dr [ "Value" ] = " ";
                                dr [ "StdStr" ] = " ";
                            }
                        }
                        else
                        {
                            if ( c.hasLaneNumber( ) )
                            {
                                dr [ "Lane" ] = c.LaneNumber.ToString( );
                                dr [ "TableIndex" ] = c.LaneNumber;
                            }
                            else
                            {
                                // FC Disabled entering N/A if we don't have lane information.
                                //dr["Lane"] = "N/A";
                                dr [ "TableIndex" ] = ResultsTable.Rows.Count + 1; // c.LaneNumber;
                            }
                        }

                        if ( c.CompetingIn.hasResultFor ( c ) )
                        {
                            dr["Rank"] = c.Result.Rank;
                            dr["Value"] = c.Result.printResultValue;
                            dr["StdStr"] = c.Result.achievedStandardsShort;

                            // " " with space to trick the ShadeOnDefault property into not shading
                            if ( dr["Rank"].ToString() == "")
                                dr["Rank"] = " ";
                            if (dr["Value"].ToString() == "")
                                dr["Value"] = " ";
                            if (dr["StdStr"].ToString() == "")
                                dr["StdStr"] = " ";
                        }

                        ResultsTable.Rows.Add ( dr );

                        wait.Bar2.increment ( );

                    } // end of for competitors

#endregion

#region lanes

                    // Add blank rows if necessary

                    if ( Event is ILaneAssignedEvent )
                    {
                        // only check for blank rows if there are no results.
                        if ( ( (ILaneAssignedEvent)Event ).hasLaneAssignementInformation ( ) && !Event.Results.Any() )
                        {
                            ILaneAssignedEvent ae = (ILaneAssignedEvent)Event;

                            if ( Event is IFinalEvent )
                            {
                                // check each heat for empty lanes
                                foreach ( IHeatEvent heat in ( (IFinalEvent)Event ).getHeats ( ) )
                                {
                                    ILaneAssignedEvent lheat = (ILaneAssignedEvent)heat;

                                    int[] emptyLanes = lheat.emptyAssingedLane();
                                    //int[] emptyLanes = lheat.emptyLanes();

                                    foreach ( int i in emptyLanes )
                                    {
                                        // we add a blank lane in here
                                        DataRow dr = ResultsTable.NewRow();
                                        dr["PageIndex"] = drPage["Index"];

                                        dr["Heat"] = heat.Final.getHeatNumber ( heat ).ToString ( );
                                        dr["HeatLane"] = i.ToString ( );
                                        dr["TableIndex"] = ( ( heat.Final.getHeatNumber ( heat ) ) * 10 ) + ( i );

                                        ResultsTable.Rows.Add ( dr );
                                    }
                                }
                            }
                            else
                            {

                                for ( int l = 1 ; l < Event.EventRanges.Lanes + 1 ; l++ )
                                {
                                    if ( ae.isLaneFree ( l ) )
                                    {
                                        // we add a blank lane in here
                                        DataRow dr = ResultsTable.NewRow();
                                        dr["PageIndex"] = drPage["Index"];

                                        // 2015-06-07 FC disabled empty lane numbers to stop confusion at the TF 2015 championship where not all lanes will be used.
                                        //dr["Lane"] = l.ToString();
                                        dr["TableIndex"] = l;

                                        ResultsTable.Rows.Add ( dr );
                                    }
                                }

                            }
                        }
                        else
                        // there aren't any lane assignment information so we will just add empty lines at the bottom of the page.
                        {
                            for ( int l = ResultsTable.Rows.Count ; l < Event.EventRanges.Lanes ; l++ )
                            {
                                // we add a blank lane in here
                                DataRow dr = ResultsTable.NewRow();
                                dr["PageIndex"] = drPage["Index"];
                                dr["Lane"] = "";
                                //dr["TableIndex"] = l;
                                dr["TableIndex"] = int.MaxValue;

                                ResultsTable.Rows.Add ( dr );
                            }
                        }
                    }
                    else
                    {
                        // this isn't a lane assigned event to just add rows at the bottom.
                        if ( lsCompetitors.Count < Event.EventRanges.Lanes )
                        {
                            int i = Event.EventRanges.Lanes - lsCompetitors.Count;

                            while ( i > 0 )
                            {
                                DataRow dr = ResultsTable.NewRow();
                                dr["PageIndex"] = drPage["Index"];
                                dr["TableIndex"] = ( Event.EventRanges.Lanes + 1 ) - i;

                                ResultsTable.Rows.Add ( dr );

                                i--;
                            }
                        }
                    }

#endregion

#region standards

                    // Build Standards Table

                    if ( Event is IStandards )
                    {
                        if ( Event.hasStandards ( ) )
                        {
                            DataRow drStd;

                            DM.Standard [] stds;

                            if ( Event.ResultsDisplayDescription == ResultDisplayDescription.DistanceCentimeters ||
                                Event.ResultsDisplayDescription == ResultDisplayDescription.DistanceMeters ||
                                Event.ResultsDisplayDescription == ResultDisplayDescription.DistanceMetersCentimeters )
                                stds = Event.Standards.OrderByDescending ( s => s.StandardValue.RawValue ).ToArray();
                            else
                                stds = Event.Standards.OrderBy ( s => s.StandardValue.RawValue ).ToArray();


                            foreach ( DM.Standard std in stds)
                            {
                                if ( std.StandardValue.HasValue ( ) )
                                {
                                    drStd = StdTable.NewRow ( );

                                    drStd["PageIndex"] = drPage["Index"];
                                    drStd["StdName"] = std.Name;
                                    drStd["StdValue"] = std.StandardValue.getResultString ( );

                                    StdTable.Rows.Add ( drStd );
                                }
                            }


                            if ( Event.CountyBestPerformance.HasValue ( ) )
                            {
                                drStd = StdTable.NewRow ( );

                                drStd["PageIndex"] = drPage["Index"];
                                drStd["StdName"] = "Championship Best Performance";
                                drStd["StdValue"] = Event.CountyBestPerformance.getResultString ( );

                                StdTable.Rows.Add ( drStd );
                            }
                        }
                    } // end of if standards

#endregion

                    string ExportPath = lFolderPath + Event.Name + " Result Card.pdf";
                    filePaths.Add ( ExportPath );
                    VestPDF.Generate ( ds );
                    try
                    {
                        VestPDF.SaveGeneratedFile ( ExportPath );
                    }
                    catch ( Exception ex )
                    {
                        wait.Invoke ( (MethodInvoker)delegate { MessageBox.Show ( ex.Message ); } );
                    }
                    wait.Bar1.increment ( );
                } // end of for events

#endregion

                string finishedFilePath = null;

                if ( filePaths.Count == 1 )
                {
                    finishedFilePath = filePaths.First ( );

                    if ( instructions.open )
                        PDFViewer.OpenOnSTAThread( finishedFilePath );

                    if ( instructions.print != PrintOptions.NO_PRINT )
                        Printing.PrintPDF ( finishedFilePath );
                }
                else if (filePaths.Count > 1)
                {
                    try
                    {
                        finishedFilePath = lFolderPath + "Combined Result Cards.pdf";
                        MergeMultiplePDFIntoSinglePDF ( finishedFilePath , filePaths.ToArray ( ) );
                    }
                    catch ( Exception ex )
                    {
                        wait.Invoke ( (MethodInvoker)delegate { MessageBox.Show ( ex.Message ); } );
                    }


                    if ( instructions.open )
                        PDFViewer.OpenOnSTAThread( finishedFilePath );

                    if ( instructions.print != PrintOptions.NO_PRINT )
                        Printing.PrintPDF ( finishedFilePath );
                }
                else
                {
                    MessageBox.Show( "No event cards were generated." );
                }

                wait.Complete ( );
                t.Abort ( );

                sw.Stop ( );

                Debug.WriteLine ( "Created event Results cards in " + sw.Elapsed.TotalSeconds + "s" );

                Console.WriteLine ( "**************************" );
                Console.WriteLine ( "********DB Reads = " + FileIO.FConnFile.GetFileDetails().IO.DBReadCounter + "*****" );
                Console.WriteLine ( "*****Cache Reads = " + FileIO.FConnFile.GetFileDetails ( ).IO.CacheReadCounter + "***" );
                Console.WriteLine ( "**************************" );

                return finishedFilePath;
            }
        }

        #endregion

#region Cross Country Specific EXCEL exports
        public static void GenerateEnvelope ( List<Team> areas , List<AEvent> events )
        {
            Thread printEnvelopesThread = new Thread(new ParameterizedThreadStart(generateEvelopes));
            printEnvelopesThread.Start ( new ExportInstuctions ( events , areas , PrintOptions.NO_PRINT ) );
        }

        private static void generateEvelopes ( object Instructions )
        {

#if ( !EXCEL && !EEPLUS )
            return;
#else

            string lFolderPath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\Templates\\" ;
            string templatePath = lFolderPath + "Envelopes.xlsx";


            if ( File.Exists ( templatePath ) )
            {
                Stopwatch sw = new Stopwatch();
                sw.Start ( );

                //TODO make request folder a Boolean return type and check for cancellation


                //if (!requestFolder())
                //{
                //    return;
                //}

                string lFolderPDFPath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\";

                ExportInstuctions instructions = (ExportInstuctions)Instructions;

                List<AEvent> events = instructions.events;
                List<Team> areas = instructions.areas;
                bool print = ( instructions.print != PrintOptions.NO_PRINT ) ;

                Wait wait = new Wait();
                wait.Bar1.setLabel ( "Competitors" );
                wait.Bar2.setLabel ( "Events" );
                wait.Bar3.setLabel ( "Areas" );
                wait.Bar1.setEnabled ( true );
                wait.Bar2.setEnabled ( true );
                wait.Bar3.setEnabled ( true );

                wait.Bar2.setMax ( events.Count );
                wait.Bar3.setMax ( areas.Count );

                Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
                t.Start ( wait );

                lock ( ExcelLock )
                {

#if ( EXCEL )
                    Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
                    Microsoft.Office.Interop.Excel.Workbook wb;
                    Microsoft.Office.Interop.Excel.Worksheet sheet;
#elif ( EEPLUS )
                    ExcelPackage excel;// = new ExcelPackage();
                    ExcelWorkbook wb;
                    ExcelWorksheet sheet;
#endif
                    try
                    {
#if ( EXCEL )
                        wb = excel.Workbooks.Open ( templatePath , false , false );
                        sheet = wb.ActiveSheet;
#elif ( EEPLUS )
                        excel = new ExcelPackage ( new FileInfo ( templatePath ) );
                        wb = excel.Workbook;
                        sheet = wb.Worksheets.First ( );
#endif
                    }
                    catch
                    {
                        wait.Complete ( );
                        t.Abort ( );
                        return;
                    }

                    //Competitor comps = new Competitors();

                    foreach ( Team a in areas )
                    {
                        wait.Bar2.setValue ( 0 );
                        foreach ( AEvent e in events )
                        {
                            List<ACompetitor> lsCompetitors = e.getEnteredCompetitors(a);

                            // 2016-12-09 removed the requirement for athletes to be pre-entered so that the envelopes for empty teams will still be generated.
                            //if ( lsCompetitors.Count > 0 )
                            //{

                            wait.Bar1.setValue ( 0 );
                            wait.Bar1.setMax ( lsCompetitors.Count );
#if ( EXCEL )
                            sheet.Range["Clear"].Value = "";
#elif ( EEPLUS )
                            wb.Names["Clear"].Value = "";
#endif

                            int j = 0;
                            //int j = 1;

                            foreach ( Competitor c in lsCompetitors )
                            {
                                //StudentCompetitor sc = (StudentCompetitor)c;

#if ( EXCEL )
                                sheet.Range["VestNo"].Offset[j , 0].Value = c.Vest.printVestString;
                                    //sheet.Range["VestNo"].Offset[j, 1].Value = c.Athlete.PrintName() + " - " + c.Result.printParameter("YearGroup") + " - " + ((StudentAthlete)c.Athlete).Attends;
                                    sheet.Range["VestNo"].Offset[j , 1].Value = c.Athlete.PrintName ( ) + " - " + c.checkParameter ( "YearGroup" ).ToString ( ) + " - " + c.checkParameter ( "Attends" ).ToString ( );
#elif ( EEPLUS )
                                int row;
                                row = wb.Names["VestNo"].Start.Row;
                                sheet.Cells[row + j , 1].Value = c.Vest.printVestString;
                                //sheet.Cells[ row + j , 1 ].Value = c.Athlete.PrintName() + " - " + c.Result.printParameter("YearGroup") + " - " + ((StudentAthlete)c.Athlete).Attends;
                                sheet.Cells[row + j , 2].Value = c.Athlete.PrintName ( ) + " - " + c.checkParameter ( "YearGroup" ).ToString ( ) + " - " + c.checkParameter ( "Attends" ).ToString ( );
#endif
                                j++;
                                wait.Bar1.increment ( );
                            }

#if ( EXCEL )
                                sheet.Range["AreaName"].Value = a.Name + " " + e.Name;
                                Thread.Sleep ( 100 );
                                wb.ExportAsFixedFormat ( Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF , lFolderPDFPath + e.Name + " " + a.Name + " Envelopes.pdf" );
                                Thread.Sleep ( 100 );
#elif ( EEPLUS )
                            // May need to sleep here!
                            wb.Names["AreaName"].Value = a.Name + " " + e.Name;

                            string exportFileName = lFolderPDFPath + e.Name + " " + a.Name + " Envelopes";

                            excel.SaveAs ( new FileInfo ( exportFileName + ".xlsx" ) );

                            SFExToPDF.ConvertExcelToPDF ( exportFileName + ".xlsx", exportFileName + ".pdf" );
                            //SFExToPDF.ConvertExcelToImage ( exportFileName + ".xlsx" );
                            // save as pdf
#endif
                            //} // end if count competitors
                            wait.Bar2.increment ( );
                        }// end for each event
                        wait.Bar3.increment ( );
                    } // end for each area

#if ( EXCEL )
                    wb.Close ( false );
                    Thread.Sleep ( 100 );
                    excel.Quit ( );
#elif ( EEPLUS )
                    // hopefully nothing to do here as I can't find a close or quit command.
#endif
                }// end ExcelLock
                wait.Complete ( );
                t.Abort ( );

                sw.Stop ( );

            }
            else  // template not found
            {
                return;
            }
#endif
        }

        public static void printBothResults ( List<AEvent> events , bool open = false , bool Print = false )
        {
            ExportInstuctions ins = new ExportInstuctions( events , Print )
            {
                //ins.open = open;
                Save = true
            };
            List<string> files = new List<string>();

            files.AddRange ( printResultsSheet ( ins ) );
            files.AddRange ( printTeamResultsSheet ( ins ) );

            if ( open )
                PDFViewer.OpenOnSTAThread ( files.Where ( x => x.EndsWith ( ".pdf" ) ).ToArray ( ) );
        }

        /// <summary>
        /// Only returns File Names if the process is done synchronously
        /// </summary>
        public static string[] printResults ( List<AEvent> events , bool Asyncronis = true )
        {
            if ( Asyncronis )
            {
                Thread printThread = new Thread(new ParameterizedThreadStart(printResultsSheetV));
                printThread.Start ( new ExportInstuctions ( events , false ) { Save = true } );
                return new string[] { };
            }
            else
            {
                return printResultsSheet ( new ExportInstuctions ( events , false ) { Save = true } );
            }
        }

        /// <summary>
        /// Only returns File Names if the process is done synchronously
        /// </summary>
        public static string[] printTeamResults ( List<AEvent> events , bool Asyncronis = true )
        {
            if ( Asyncronis )
            {
                Thread printTeamThead = new Thread(new ParameterizedThreadStart(printTeamResultsSheetV));
                printTeamThead.Start ( new ExportInstuctions ( events , false ) { Save = true } );
                return new string[] { };
            }
            else
            {
                return printTeamResultsSheet ( new ExportInstuctions ( events , false ) { Save = true } );
            }
        }

        private static void printResultsSheetV ( object Instructions ) { printResultsSheet ( Instructions ); }

        private static void printTeamResultsSheetV ( object Instructions ) { printTeamResultsSheet ( Instructions ); }

        private static string[] printTeamResultsSheet ( object Instructions )
        {
#if ( !EXCEL && !EEPLUS )
            return null;
#else
            string lFolderPath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\" ;
            string lTemplatePath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\Templates\\" ;
            string templatePath = lTemplatePath + "TeamResults.xlsx";

            ExportInstuctions instructions = (ExportInstuctions)Instructions;

            List<AEvent> events = instructions.events;
            bool print = ( instructions.print != PrintOptions.NO_PRINT );
            bool open = instructions.open;

            Wait wait = new Wait();
            wait.Bar1.setLabel ( "Competitors" );
            wait.Bar2.setLabel ( "Areas" );
            wait.Bar3.setLabel ( "Events" );

            wait.Bar1.setEnabled ( true );
            wait.Bar2.setEnabled ( true );

            wait.Bar3.setMax ( events.Count );
            wait.Bar3.setEnabled ( true );

            Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
            t.Start ( wait );

            List<string> fileNames = new List<string>();

            lock ( ExcelLock )
            {

#if ( EXCEL )
                Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook wb;
                Microsoft.Office.Interop.Excel.Worksheet sheet;
#elif ( EEPLUS )
                ExcelPackage excel;
                ExcelWorkbook wb;
                ExcelWorksheet sheet;
#endif
                try
                {
#if ( EXCEL )
                    wb = excel.Workbooks.Open ( @Properties.Settings.Default.OLD_RootPathTemplate + "TeamResults.xlsx" , false , false );
                    sheet = wb.ActiveSheet;
#elif ( EEPLUS )
                    excel = new ExcelPackage ( new FileInfo ( templatePath ) );
                    wb = excel.Workbook;
                    sheet = wb.Worksheets.First ( );
#endif
                }
                catch (Exception ex)
                {
                    wait.Complete ( );
                    t.Abort ( );
                    return new string[] { };
                }


                //AResult results;//= new AResult();
                foreach ( AEvent e in events )
                {
                    // Clear cells from the last event

#if ( EXCEL )
                    sheet.Range["Clear1"].Value = "";
#elif ( EEPLUS )
                    wb.Names["Clear1"].Value = "";
#endif

                    bool competitors = false;
                    List<Team> areas = ((App)App.Current).CurrentChampionship.Championship.Teams.ToList(); // MainWindow.CurrentChampionship.listAllTeams();

                    wait.Bar2.setValue ( 0 );
                    wait.Bar2.setMax ( areas.Count );

                    //// WSAA 2014-15
                    var topData = new object[16, 5];

                    // SW 2015
                    //var topData = new object[14, 7];
                    //var bottomData = new object[1, 7];

                    foreach ( Team a in areas )
                    {
                        List<AResult> lresults = e.Results.Where(f => f.printTeam() == a.Name && f.isComplete()).ToList();
                        if ( true )
                        {
                            competitors = true;
                            wait.Bar1.setValue ( 0 );
                            wait.Bar1.setMax ( lresults.Count );


                            int i = 0;  //changed from 1 when changing to range based updates
                            int column;
                            switch ( a.Name )
                            {
                                case "Avon":
                                    column = 1;
                                    break;
                                case "Cornwall":
                                    column = 2;
                                    break;
                                case "Devon":
                                    column = 3;
                                    break;
                                case "Dorset":
                                    column = 4;
                                    break;
                                case "Gloucestershire":
                                    column = 5;
                                    break;
                                case "Somerset":
                                    column = 6;
                                    break;
                                case "Wiltshire":
                                    column = 7;
                                    break;


                                case "Swindon":
                                    column = 4;
                                    break;
                                case "North Wiltshire":
                                    column = 2;
                                    break;
                                case "West Wiltshire":
                                    column = 5;
                                    break;
                                case "Kennet":
                                    column = 1;
                                    break;
                                case "Salisbury":
                                    column = 3;
                                    break;
                                default:
                                    continue;
                            }


                            //int column = a.getLogicalNumber();

                            foreach ( AResult r in lresults.OrderBy ( f => f.Rank.Value ) )
                            {
                                topData[i , column - 1] = string.Format ( "{0} / {1} / Y{4} {2} {3}" , r.Rank.Value.ToString ( ) , r.printVestNo ( ) , "\n" , r.Competitor.getName ( ) , r.printParameter ( "YearGroup" ) ); //((StudentCompetitor)r.Competitor).YearGroup);

                                wait.Bar1.increment ( );
                                i++;
                            }

                            foreach ( ScoringTeam ar in e.getScoringTeams ( ).Where ( f => f.ScoringTeamName == "A" && f.Team == a ) )// results.getAreaResults(e, "A", a))
                            {
                                // row chanced for SW 2014-15
                                //topData[10, column - 1] = ar.printSumOfPositions();

                                topData[13 , column - 1] = ar.printSumOfPositions ( );
                                // not required for SW 2014-15

                                string points = ar.printPoints( );
                                
                                topData[15 , column - 1] = points;
                                switch ( points )
                                {
                                    case "1":
                                        topData[14 , column - 1] = 5;  
                                        break;
                                    case "2":
                                        topData[14 , column - 1] = 4;
                                        break;
                                    case "3":
                                        topData[14 , column - 1] = 3;
                                        break;
                                    case "4":
                                        topData[14 , column - 1] = 2;
                                        break;
                                    case "5":
                                        topData[14 , column - 1] = 1;
                                        break;
                                }
                            }

                            //i = 1;

                            //foreach (AreaResult ar in results.getAreaResults("B"))
                            //{
                            //    string range = "BTeamHeader";
                            //    sheet.Range[range].Offset[i, 0].Value = ar.area.getCode();
                            //    sheet.Range[range].Offset[i, 1].Value = ar.positions;
                            //    sheet.Range[range].Offset[i, 2].Value = ar.total;
                            //    sheet.Range[range].Offset[i, 3].Value = ar.points;

                            //    i++;
                            //}
#if ( EXCEL )

                            var writeRange = sheet.Range["Clear1"];
                            writeRange.Value2 = topData;
                            // not required for SW 2014-15
                            writeRange = sheet.Range["Clear2"];
                            writeRange.Value2 = bottomData;

                            sheet.Range["AreaName"].Value = e.Name;
#elif ( EEPLUS )
                            var writeRange = wb.Names["Clear1"];
                            writeRange.Value = topData;
                            // not required for SW 2014-15

                            wb.Names["AreaName"].Value = e.Name;
#endif




                        }
                        wait.Bar2.increment ( );
                    }
                    if ( competitors )
                    {
                        try
                        {
                            if ( instructions.Save )
                            {
#if ( EXCEL )
                                wb.SaveAs ( @Properties.Settings.Default.ResultsPath + e.Name + " Team.xlsx" );
                                wb.ExportAsFixedFormat ( Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF , @Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf" );
#elif ( EEPLUS )
                                excel.Workbook.Calculate ( );
                                excel.SaveAs ( new FileInfo ( lFolderPath + e.Name + " Team.xlsx" ) );
                                SFExToPDF.ConvertExcelToPDF ( lFolderPath + e.Name + " Team.xlsx", lFolderPath + e.Name + " Team.pdf" );
#endif
                                fileNames.Add ( lFolderPath + e.Name + " Team.xlsx" );
                                fileNames.Add ( lFolderPath + e.Name + " Team.pdf" );
                            }

                            //new AdobePrn(@Properties.Settings.Default.ResultsPDFPath + e.ShortName + "\\Team.pdf", @Properties.Settings.Default.prnOpA3ResultsByTeam, _main);

                            if ( open )
                                PDFViewer.OpenOnSTAThread ( lFolderPath + e.Name + " Team.pdf" );
                            //Process.Start ( @Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf" );

                            if ( print )
                                Printing.PrintPDF ( lFolderPath + e.Name + " Team.pdf" );
                                //Printing.PrintPDF ( @Properties.Settings.Default.ResultsPDFPath + e.Name + " Team.pdf" );
                        }
                        catch
                        {
                            MessageBox.Show ( "Could not save the file " + lFolderPath + e.Name + " Team.pdf" );
                        }
                    }
                    wait.Bar3.increment ( );
                }// for each event

#if ( EXCEL )
                wb.Close ( false );
                excel.Quit ( );
#elif ( EEPLUS )
                // hopefully nothing to do here as I can't find a close or quit command.
#endif
            }// end ExcelLock

            wait.Complete ( );
            t.Abort ( );
            return fileNames.ToArray ( );
#endif
            }

        private static string[] printResultsSheet ( object Instructions )
        {
#if ( !EXCEL && !EEPLUS )
            return;
#else
            string lFolderPath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\" ;
            string lTemplatePath = ((App)App.Current).CurrentChampionship.getChampionshipExportsDir() + "\\Templates\\" ;
            string templatePath = lTemplatePath + "EventResults.xlsx";

            ExportInstuctions instructions = (ExportInstuctions)Instructions;

            List<AEvent> events = instructions.events;
            bool print = ( instructions.print != PrintOptions.NO_PRINT );
            bool open = instructions.open;

            Wait wait = new Wait();
            wait.Bar1.setLabel ( "Competitors" );
            wait.Bar2.setLabel ( "Events" );
            wait.Bar1.setEnabled ( true );
            wait.Bar2.setEnabled ( true );
            wait.Bar2.setMax ( events.Count );

            Thread t = new Thread(new ParameterizedThreadStart(wait.Start));
            t.Start ( wait );

            List<string> fileNames = new List<string>();

            lock ( ExcelLock )
            {

#if ( EXCEL )
                Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook wb;
                Microsoft.Office.Interop.Excel.Worksheet sheet;
#elif ( EEPLUS )
                ExcelPackage excel;
                ExcelWorkbook wb;
                ExcelWorksheet sheet;
#endif
                try
                {
#if ( EXCEL )
                    wb = excel.Workbooks.Open ( templatePath , false , false );
                    sheet = wb.ActiveSheet;
#elif ( EEPLUS )
                    excel = new ExcelPackage ( new FileInfo ( templatePath ) );
                    wb = excel.Workbook;
                    sheet = wb.Worksheets.First ( );
#endif
                }
                catch ( Exception ex )
                {
                    wait.Complete ( );
                    t.Abort ( );
                    return new string[] { };
                }

                //AResult results;// = new Results();

                foreach ( AEvent e in events )
                {
                    List<AResult> lresults = e.Results.Where(r => r.isComplete()).ToList();

                    // Condition removed 2016-01-15 so that empty results sheets will be made.
                    //if (lresults.Count > 0)
                    if ( true )
                    {
                        wait.Bar1.setValue ( 0 );
                        wait.Bar1.setMax ( lresults.Count );


                        // WSAA 2014-15
                        var leftData = new object[46, 5];
                        var rightData = new object[26, 5];
                        var ATeamData = new object[5, 5];
                        var BTeamData = new object[5, 5];
                        var OverallData = new object[5, 5];
                        // Clear cells from the last event
#if ( EXCEL )
                        sheet.Range["Clear1"].Value = ""; // left
                        sheet.Range["Clear2"].Value = ""; // right
                        sheet.Range["Clear3"].Value = ""; // ATeam
                        sheet.Range["Clear4"].Value = ""; // BTeam
                        sheet.Range["Clear5"].Value = ""; // Overall
#elif ( EEPLUS )
                        wb.Names["Clear1"].Value = ""; // left
                        wb.Names["Clear2"].Value = ""; // right
                        wb.Names["Clear3"].Value = ""; // ATeam
                        wb.Names["Clear4"].Value = ""; // BTeam
                        wb.Names["Clear5"].Value = ""; // Overall
#endif


                        //// SW 2014-15
                        //var leftData = new object[46, 5];
                        //var rightData = new object[32, 5];
                        //var ATeamData = new object[7, 5];
                        ////var OverallData = new object[5, 5];
                        //// Clear cells from the last event
                        //sheet.Range["Clear1"].Value = ""; // left
                        //sheet.Range["Clear2"].Value = ""; // right
                        //sheet.Range["Clear3"].Value = ""; // ATeam

                        int highestRank = 0;

                        foreach ( AResult r in lresults )
                        {

                            if ( r.Rank.Value < 47 )
                            {
                                // use leftData
                                int row = r.Rank.Value;

                                leftData[row - 1 , 0] = r.printRank;
                                leftData[row - 1 , 1] = r.printVestNo ( );
                                leftData[row - 1 , 2] = string.Format ( "{0}{1} - {2} - {3}" , ( r.CertificateEarned ? " * " : "" ) , r.printName ( ) , r.printResultValueString ( ) , r.printParameter ( "Attends" ) );
                                leftData[row - 1 , 3] = r.printParameter ( "YearGroup" ); //((StudentCompetitor) r.Competitor).YearGroup;
                                leftData[row - 1 , 4] = r.printTeamShort ( );

                            }
                            else
                            {
                                int row = r.Rank.Value - 46;

                                // use rightData
                                rightData[row - 1 , 0] = r.printRank;
                                rightData[row - 1 , 1] = r.printVestNo ( );
                                rightData[row - 1 , 2] = string.Format ( "{0}{1} - {2} - {3}" , ( r.CertificateEarned ? "*" : "" ) , r.printName ( ) , r.printResultValueString ( ) , r.printParameter ( "Attends" ) );
                                rightData[row - 1 , 3] = r.printParameter ( "YearGroup" ); //((StudentCompetitor)r.Competitor).YearGroup;
                                rightData[row - 1 , 4] = r.printTeamShort ( );

                            }

                            if ( r.Rank > highestRank )
                                highestRank = r.Rank.Value;

                            wait.Bar1.increment ( );
                        }

                        // move away from the last entered rank
                        highestRank++;

                        List<AResult> DNFresults = e.Results.Where(r => r.getTypeDescription() == ResultTypeDescription.CompetativeDNF).ToList();

                        foreach ( AResult r in DNFresults )
                        {

                            if ( highestRank < 47 )
                            {
                                // use leftData
                                int row = highestRank;

                                leftData[row - 1 , 0] = r.printRank;
                                leftData[row - 1 , 1] = r.printVestNo ( );
                                leftData[row - 1 , 2] = string.Format ( "{0}{1} - {2} - {3}" , ( r.CertificateEarned ? "*" : "" ) , r.printName ( ) , r.printResultValueString ( ) , r.printParameter ( "Attends" ) );
                                leftData[row - 1 , 3] = r.printParameter ( "YearGroup" ); //((StudentCompetitor) r.Competitor).YearGroup;
                                leftData[row - 1 , 4] = r.printTeamShort ( );

                            }
                            else
                            {
                                int row = highestRank - 46;

                                // use rightData
                                rightData[row - 1 , 0] = r.printRank;
                                rightData[row - 1 , 1] = r.printVestNo ( );
                                rightData[row - 1 , 2] = string.Format ( "{0}{1} - {2} - {3}" , ( r.CertificateEarned ? "*" : "" ) , r.printName ( ) , r.printResultValueString ( ) , r.printParameter ( "Attends" ) );
                                rightData[row - 1 , 3] = r.printParameter ( "YearGroup" ); //((StudentCompetitor)r.Competitor).YearGroup;
                                rightData[row - 1 , 4] = r.printTeamShort ( );

                            }

                            highestRank++;

                        }

                        int i = 0;

                        foreach ( ScoringTeam ar in e.getScoringTeams ( ).Where ( f => f.ScoringTeamName == "A" ).OrderBy ( f => f.orderableRank ( ) ) )// results.getAreaResults(e, "A"))
                        {

                            if ( ar.Points == 0 ) continue;

                            ATeamData[i , 0] = "    " + ar.Team.ShortName;
                            ATeamData[i , 2] = ar.printPositions ( );
                            ATeamData[i , 3] = ar.printSumOfPositions ( );
                            ATeamData[i , 4] = ar.printPoints ( );

                            i++;
                        }

                        i = 0;

                        // Not used for SW 2014-15
                        foreach ( ScoringTeam ar in e.getScoringTeams ( ).Where ( f => f.ScoringTeamName == "B" ).OrderBy ( f => f.orderableRank ( ) ) )
                        {
                            if ( ar.Points == 0 ) continue;

                            BTeamData[i , 0] = "    " + ar.Team.ShortName;
                            BTeamData[i , 2] = ar.printPositions ( );
                            BTeamData[i , 3] = ar.printSumOfPositions ( );
                            BTeamData[i , 4] = ar.printPoints ( );

                            i++;
                        }

                        i = 0;

                        foreach ( ChampionshipTeamResult or in e.Championship.getOverallSores ( ).Where ( f => f.ScoringTeamName == "A" ).OrderBy ( f => f.orderableRank ( ) ) )// results.getOverAllResults("A"))
                        {
                            OverallData[i , 0] = "    " + or.Team.Name;

                            ChampionshipTeamResult bResult = e.Championship.getOverallSores().Where(f => f.ScoringTeamName == "B" && f.Team == or.Team).FirstOrDefault();

                            if ( bResult != null )
                            {
                                OverallData[i , 3] = string.Format ( "{0} / {1}" , or.Points , bResult.Points );
                            }
                            else
                            {
                                OverallData[i , 3] = string.Format ( "{0} / {1}" , or.Points , 0 );
                            }

                            i++;
                        }
#if ( EXCEL )
                        sheet.Range["TeamName"].Value = e.Name;

                        var writeRange = sheet.Range["Clear1"];
                        writeRange.Value2 = leftData;

                        writeRange = sheet.Range["Clear2"];
                        writeRange.Value2 = rightData;

                        writeRange = sheet.Range["Clear3"];
                        writeRange.Value2 = ATeamData;

                        // not used for SW 2014-15
                        writeRange = sheet.Range["Clear4"];
                        writeRange.Value2 = BTeamData;

                        writeRange = sheet.Range["Clear5"];
                        writeRange.Value2 = OverallData;
#elif ( EEPLUS )
                        wb.Names["TeamName"].Value = e.Name;

                        var writeRange = wb.Names["Clear1"];
                        writeRange.Value = leftData;

                        writeRange = wb.Names["Clear2"];
                        writeRange.Value = rightData;

                        writeRange = wb.Names["Clear3"];
                        writeRange.Value = ATeamData;

                        // not used for SW 2014-15
                        writeRange = wb.Names["Clear4"];
                        writeRange.Value = BTeamData;

                        writeRange = wb.Names["Clear5"];
                        writeRange.Value = OverallData;
#endif

                        string eName = lFolderPath + e.Name + " Full.xlsx";
                        string pName = lFolderPath + e.Name + " Full.pdf";

                        try
                        {
                            if ( instructions.Save )
                            {
#if ( EXCEL )
                                wb.SaveAs ( @Properties.Settings.Default.ResultsPath + e.Name + " Full.xlsx" );

                                wb.ExportAsFixedFormat ( Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF , @Properties.Settings.Default.ResultsPDFPath + e.Name + " Full.pdf" );
#elif ( EEPLUS )
                                excel.Workbook.Calculate ( );
                                excel.SaveAs ( new FileInfo ( eName ) );
                                SFExToPDF.ConvertExcelToPDF ( eName, pName );
#endif
                                fileNames.Add ( eName );
                                fileNames.Add ( pName );
                            }
                            //new AdobePrn(@Properties.Settings.Default.ResultsPDFPath + e.ShortName + "\\Full.pdf", @Properties.Settings.Default.prnOpA3Results, _main);

                            if ( open )
                                //Process.Start ( @Properties.Settings.Default.ResultsPDFPath + e.Name + " Full.pdf" );
                                PDFViewer.OpenOnSTAThread ( pName );

                            if ( print )
                                Printing.PrintPDF ( pName );
                        }
                        catch
                        {
                            MessageBox.Show ( "Could not save the file " + pName );
                        }
                        wait.Bar2.increment ( );

                    }
                }
#if ( EXCEL )

                wb.Close ( false );
                excel.Quit ( );
#elif ( EEPLUS )
#endif
            } // end Excel Lock
            wait.Complete ( );
            t.Abort ( );
            return fileNames.ToArray ( );
#endif
            }


        #endregion

        public static void MergeMultiplePDFIntoSinglePDF(string outputFilePath, string[] pdfFiles)
        {
            Console.WriteLine("Merging started.....");
            PdfDocument outputPDFDocument = new PdfDocument();
            foreach (string pdfFile in pdfFiles)
            {
                PdfDocument inputPDFDocument = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import);
                outputPDFDocument.Version = inputPDFDocument.Version;
                foreach (PdfPage page in inputPDFDocument.Pages)
                {
                    outputPDFDocument.AddPage(page);
                }
            }
            //try
            //{
                outputPDFDocument.Save(outputFilePath);
                Console.WriteLine("Merging Completed");
            //}
            //catch (Exception)
            //{
            //    System.Diagnostics.Debug.WriteLine("Failed to save file. \n" + outputFilePath);                
            //}
        }

        // Currently only used in cross country and only as a web export
#region XML Exports

        public static XmlElement EventResult(AEvent Event, XmlDocument doc)
        {
            XmlElement xmlEvent = doc.CreateElement("EventResult");
            
            XmlAttribute attEventName = doc.CreateAttribute("EventName");
            attEventName.Value = Event.PrintableName;

            xmlEvent.Attributes.Append(attEventName);

            foreach (AResult Result in Event.Results.OrderBy(f => f.getRank()))
            {
                XmlElement xmlIndividual = doc.CreateElement("IndividualResult");

                XmlElement xmlRank = doc.CreateElement("Rank");
                XmlElement xmlVest = doc.CreateElement("Vest");
                XmlElement xmlCompetitorName = doc.CreateElement("CompetitorName");

                XmlElement xmlCertificate = doc.CreateElement("CertificateEarned");

                xmlRank.InnerText = Result.getRank().ToString();
                xmlVest.InnerText = Result.printVestNo();
                xmlCompetitorName.InnerText = Result.printName();//.printParameter("Fullname");//.Competitor.checkParameter("Fullname").ToString();

                xmlIndividual.AppendChild(xmlRank);
                xmlIndividual.AppendChild(xmlVest);
                xmlIndividual.AppendChild(xmlCompetitorName);

                if (Result.HasValue())
                {
                    XmlElement xmlTime = doc.CreateElement("Time");
                    XmlElement xmlTimeValue = doc.CreateElement("TimeValue");

                    //xmlTime.InnerText = string.Format("{0:00}:{1:00}.{2:0}", Result.Duration.Value.Minutes, Result.Duration.Value.Seconds, Result.Duration.Value.Milliseconds);
                    xmlTime.InnerText = Result.Value.getResultString();
                    xmlTimeValue.InnerText = Result.Value.getResultString();

                    xmlIndividual.AppendChild(xmlTime);
                    xmlIndividual.AppendChild(xmlTimeValue);
                }

                if (Result.Competitor != null)
                {
                    XmlElement xmlYearGroup = doc.CreateElement("YearGroup");
                    XmlElement xmlSchool = doc.CreateElement("School");
                    XmlElement xmlTeam = doc.CreateElement("Team");
                    XmlElement xmlTeamShort = doc.CreateElement("TeamShort");

                    xmlYearGroup.InnerText = ((StudentCompetitor)Result.Competitor).YearGroup.ToString();
                    xmlSchool.InnerText = Result.printParameter("Attends");
                    xmlTeam.InnerText = Result.printTeam();
                    xmlTeamShort.InnerText = Result.printTeamShort();

                    xmlIndividual.AppendChild(xmlSchool);
                    xmlIndividual.AppendChild(xmlYearGroup);
                    xmlIndividual.AppendChild(xmlTeam);
                    xmlIndividual.AppendChild(xmlTeamShort);

                }

                xmlCertificate.InnerText = "false";
                xmlIndividual.AppendChild(xmlCertificate);


                xmlEvent.AppendChild(xmlIndividual);
            }

             //request scoring teams

            foreach (XmlElement xmlE in ScoringTeamResults(Event,doc))
            {
                xmlEvent.AppendChild(xmlE);
            }

            return xmlEvent;

        }

        public static XmlDocument AllResults(Championship Championship)
        {
            XmlDocument doc = createResultDocument(Championship);

            foreach (AEvent Event in Championship.listAllEvents())
            {
                if ( Event is IFinalEvent FinalEvent )
                    foreach ( IHeatEvent heat in FinalEvent.getHeats() )
                        doc.FirstChild.AppendChild( EventResult( (AEvent)heat , doc ) );

                doc.FirstChild.AppendChild(EventResult(Event,doc));
            }

            doc.FirstChild.AppendChild(OverallChampionshipResults(Championship,doc));

            //doc.AppendChild(OverallChampionshipResults(Championship));

            //XmlReader schema = XmlReader.Create("ResultsSchema.xsd");

            //if (ValidateXMLSchema("ResultsSchema.xsd", doc))
            //{
            //    return doc;
            //}
            //else
            //{
            //    throw new ArgumentException("Failed to validate XML Result document");
            //}
            return doc;
        }

        private static XmlElement[] ScoringTeamResults(AEvent Event, XmlDocument doc)
        {

            List<XmlElement> elements = new List<XmlElement>();

            foreach (ScoringTeam sc in Event.getScoringTeams())
            {
                if (sc.sumOfPositions == 0)
                    continue;

                XmlElement xmlSC = doc.CreateElement("ScoringTeamResult");
                
                XmlElement xmlTeam = doc.CreateElement("Team");
                XmlElement xmlTeamShort = doc.CreateElement("TeamShort");
                XmlElement xmlScoringTeam = doc.CreateElement("ScoringTeam");
                XmlElement xmlPositions = doc.CreateElement("Positions");
                XmlElement xmlRank = doc.CreateElement("Rank");
                XmlElement xmlPoints = doc.CreateElement("Points");
                XmlElement xmlSumPositions = doc.CreateElement("SumOfPositions");


                xmlTeam.InnerText = sc.Team.Name;
                xmlTeamShort.InnerText = sc.Team.ShortName;
                xmlScoringTeam.InnerText = sc.ScoringTeamName;
                xmlPositions.InnerText = sc.printPositions();
                xmlRank.InnerText = sc.Rank.ToString();
                xmlPoints.InnerText = sc.Points.ToString();
                xmlSumPositions.InnerText = sc.sumOfPositions.ToString();


                xmlSC.AppendChild(xmlScoringTeam);
                xmlSC.AppendChild(xmlRank);
                xmlSC.AppendChild(xmlTeam);
                xmlSC.AppendChild(xmlTeamShort);
                xmlSC.AppendChild(xmlPositions);
                xmlSC.AppendChild(xmlSumPositions);
                xmlSC.AppendChild(xmlPoints);

                elements.Add(xmlSC);
            }

            return elements.ToArray();
        }

        public static XmlElement OverallChampionshipResults(Championship Championship,XmlDocument doc)
        {
            XmlElement xmlOverallChampionship = doc.CreateElement("OverallResult");

            foreach (ChampionshipTeamResult CTR in Championship.getOverallSores())
            {
                XmlElement xmlChampionshipTeamResults = doc.CreateElement("ChampionshipTeamResult");

                if (CTR.sumOfPositions == 0)
                    continue;

                XmlElement xmlTeam = doc.CreateElement("Team");
                XmlElement xmlTeamShort = doc.CreateElement("TeamShort");
                XmlElement xmlScoringTeam = doc.CreateElement("ScoringTeam");
                XmlElement xmlRank = doc.CreateElement("Rank");
                XmlElement xmlPoints = doc.CreateElement("Points");
                XmlElement xmlSumPositions = doc.CreateElement("SumOfPositions");


                xmlTeam.InnerText = CTR.Team.Name;
                xmlTeamShort.InnerText = CTR.Team.ShortName;
                xmlScoringTeam.InnerText = CTR.ScoringTeamName;
                xmlRank.InnerText = CTR.Rank.ToString();
                xmlPoints.InnerText = CTR.Points.ToString();
                xmlSumPositions.InnerText = CTR.sumOfPositions.ToString();


                xmlChampionshipTeamResults.AppendChild(xmlScoringTeam);
                xmlChampionshipTeamResults.AppendChild(xmlRank);
                xmlChampionshipTeamResults.AppendChild(xmlTeam);
                xmlChampionshipTeamResults.AppendChild(xmlTeamShort);
                xmlChampionshipTeamResults.AppendChild(xmlPoints);
                xmlChampionshipTeamResults.AppendChild(xmlSumPositions);

                xmlOverallChampionship.AppendChild(xmlChampionshipTeamResults);
            }



            return xmlOverallChampionship;
        }

        private static XmlDocument createResultDocument(Championship Championship)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = doc.CreateElement("ChampionshipResults");

            XmlAttribute attChampionship = doc.CreateAttribute("Championship");
            XmlAttribute attLastUpdated = doc.CreateAttribute("LastUpdated");

            attChampionship.Value = Championship.Name;
            attLastUpdated.Value = xmlDateTime( DateTime.Now ) ;

            rootNode.Attributes.Append(attChampionship);
            rootNode.Attributes.Append(attLastUpdated);

            doc.AppendChild(rootNode);

            return doc;
        }

        private static bool ValidateXMLSchema(string schemaPath, XmlDocument xml)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                //settings.Schemas.Add("http://www.contoso.com/books", "contosoBooks.xsd");
                settings.Schemas.Add(null, schemaPath);
                settings.ValidationType = ValidationType.Schema;

                XmlDocument document = xml;
                //document.Load(xml);

                ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

                // the following call to Validate succeeds.
                document.Validate(eventHandler);

                // add a node so that the document is no longer valid
                //XPathNavigator navigator = document.CreateNavigator();
                //navigator.MoveToFollowing("price", "http://www.contoso.com/books");
                //XmlWriter writer = navigator.InsertAfter();
                //writer.WriteStartElement("anotherNode", "http://www.contoso.com/books");
                //writer.WriteEndElement();
                //writer.Close();

                // the document will now fail to successfully validate
                //document.Validate(eventHandler);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Debug.WriteLine("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    Debug.WriteLine("Warning {0}", e.Message);
                    break;
            }

        }

        static string xmlDateTime(DateTime dateTime)
        {
            if (dateTime != null)
                return string.Format("{0:yyyy}-{0:MM}-{0:dd}T{0:HH}:{0:mm}:{0:ss}", dateTime);
            return string.Empty;
        }

#endregion

    }

    class ExportInstuctions
    {

        //public Instruction(List<AEvent> Events)
        //{
        //    print = false;
        //    events = Events;
        //    areas = new List<Team>();
        //}

        public ExportInstuctions(List<Athlete> Athletes, Boolean Open = false, PrintOptions Print = PrintOptions.NO_PRINT)
        {
            this.open = Open;
            print = Print;
            athletes = Athletes;
            events = new List<AEvent>();
            areas = new List<Team> ( );
        }

        public ExportInstuctions(List<AEvent> Events , Boolean Open = false, PrintOptions Print = PrintOptions.NO_PRINT )
        {
            this.open = Open;
            print = Print;
            events = Events;
            areas = new List<Team>();
        }

        public ExportInstuctions(List<AEvent> Events, PrintOptions Print = PrintOptions.NO_PRINT )
        {
            print = Print;
            events = Events;
            areas = new List<Team>();
        }

        public ExportInstuctions(List<AEvent> Events, List<Team> Areas, PrintOptions Print = PrintOptions.NO_PRINT )
        {
            print = Print;
            events = Events;
            areas = Areas;
        }

        public ExportInstuctions(List<Team> Areas, PrintOptions Print = PrintOptions.NO_PRINT )
        {
            print = Print;
            events = new List<AEvent>();
            areas = Areas;
        }

        public ExportInstuctions(List<Team> Areas)
        {
            print = PrintOptions.NO_PRINT;
            events = new List<AEvent>();
            areas = Areas;
        }

        public ExportInstuctions(List<AEvent> Events, string CertType, char Team = 'A', int TeamPosition = 1, PrintOptions Print = PrintOptions.NO_PRINT )
        {
            print = Print;
            events = Events;
            areas = new List<Team>();
            team = Team;
            certType = CertType;
            teamPosition = TeamPosition;
        }

        public string strTeamPosition()
        {
            return new IntToStr().ToStr(teamPosition);
        }

        public List<AEvent> events { get; set; }
        public List<Athlete> athletes { get; set; }
        public List<Team> areas { get; set; }
        public PrintOptions print { get; set; }
        public char team { get; set; }
        public string certType { get; set; }
        public int teamPosition { get; set; }
        public Boolean open { get; set; }
        public string OutputPath { get; set; }
        public bool Save { get; set; }

    }

    class IntToStr
    {
        public IntToStr ( )
        {
        }
        public string ToStr ( int Integer )
        {
            switch ( Integer )
            {
                case 1:
                    return "First";
                case 2:
                    return "Second";
                case 3:
                    return "Third";
                case 4:
                    return "Fourth";
                case 5:
                    return "Fifth";
                case 6:
                    return "Sixth";
                default:
                    return Integer.ToString ( );
            }
        }
    }


}
