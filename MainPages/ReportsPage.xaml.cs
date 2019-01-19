using ChampionshipSolutions.ControlRoom;
using ChampionshipSolutions.DM;
using ChampionshipSolutions.ViewModel;
using ChampionshipSolutions.Reporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public ReportsPage ( )
        {
            InitializeComponent();
        }

        private void generateDataEventyForms_Click ( object sender , RoutedEventArgs e )
        {
            List<AEvent> evs = new List<AEvent>();

            //if ( ((App)Application.Current ).CurrentChampionship.Name.Contains ( "TF" ) )
            //{

            foreach ( EventVM Event in ( (App)Application.Current ).CurrentChampionship.Events )
                evs.Add ( Event.Event );

            try
            {
                Exports.GenerateResultEntryForms ( evs , true );
            }
            catch ( Exception ex )
            {
                MessageBox.Show ( "Failed to open results sheet. \n" + ex.Message );
            }

            //}
            //else
            //{
                //// 2015-06-07 printBothResults is part of the cross country application.
                //new Print ( ).printBothResults ( evs , true );
            //}
        }


        private void ExportSchoolsWithContacs_Click ( object sender , RoutedEventArgs e )
        {
            Exports.exportSchoolsListForContacts ( );
        }

        private void ExportSchoolsAndSelecton_Click ( object sender , RoutedEventArgs e )
        {
            Exports.exportSchoolsListForSelection ( );
        }

        private void ExportStandards_Click ( object sender , RoutedEventArgs e )
        {
            Exports.exportStandards ( );
        }

        private void ExportSelected_Click ( object sender , RoutedEventArgs e )
        {
            Exports.exportSelectedCompeitors ( );
        }

        private void ExportEntires_Click ( object sender , RoutedEventArgs e )
        {
            Exports.exportEnteredCompeitors ( );
        }

        private void ExportResultsXML_Click ( object sender , RoutedEventArgs e )
        {
            XmlDocument doc = Exports.AllResults(((App)Application.Current ).CurrentChampionship.Championship);
            doc.Save ( ( (App)Application.Current ).CurrentChampionship.getChampionshipExportsDir() + @"\Results.xml" );
        }

        private void ExportEvents ( object sender , RoutedEventArgs e )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add ( "EventCode" );
            dt.Columns.Add ( "Event" );
            dt.Columns.Add ( "Count_Competitors" );

            foreach ( AEvent Event in ((App)Application.Current ).CurrentChampionship.Championship.Events )
            {
                DataRow dr = dt.NewRow();

                dr["EventCode"] = Event.ShortName;
                dr["Event"] = Event.Name;
                dr["Count_Competitors"] = Event.countCompetitors ( );

                dt.Rows.Add ( dr );
            }

            System.IO.TextWriter tw = new System.IO.StreamWriter( ( (App)Application.Current ).CurrentChampionship.getChampionshipExportsDir() + @"\Events.csv");

            CSVReport.WriteDataTable ( dt , tw , true );

            tw.Close ( );
        }

        private void ExportAthleteProfiles_Click ( object sender , RoutedEventArgs e )
        {
            List<Athlete> athletes = new List<Athlete>();


            foreach ( EventVM Event in ( (App)Application.Current ).CurrentChampionship.Events )
            {
                foreach ( ACompetitor competitor in Event.Event.EnteredCompetitors.Where ( c => c.SelectedForNextEvent ) )
                {
                    if ( competitor is Competitor )
                    {
                        if ( ! athletes.Contains ( ( (Competitor)competitor ).Athlete ) )
                            athletes.Add ( ( (Competitor)competitor ).Athlete );
                    }
                }
            }


            try
            {
                athletes = athletes.OrderBy ( c => c.DateOfBirth ).OrderBy ( c => c.Gender ).ToList ( );
                Exports.GenerateAthleteProfile ( athletes , false );
            }
            catch ( Exception ex )
            {
                MessageBox.Show ( "Failed to open athletes profile sheets. \n" + ex.Message );
            }

        }

        private void ExportVestNumbers_Click ( object sender , RoutedEventArgs e )
        {
            List<Team> teams = ( (App)Application.Current ).CurrentChampionship.Championship.Teams.ToList();
            List<AEvent> events = ( (App)Application.Current ).CurrentChampionship.Championship.Events.ToList();

            Exports.GenerateVestNumbers ( teams , events );
        }

        private void ExportEnvelopes_Click (object sender, RoutedEventArgs e)
        {
            List<Team> teams = ( (App)Application.Current ).CurrentChampionship.Championship.Teams.ToList();
            List<AEvent> events = ( (App)Application.Current ).CurrentChampionship.Championship.Events.ToList();

            Exports.GenerateEnvelope ( teams , events );
        }

        private void ExportCompetitors_Click ( object sender , RoutedEventArgs e )
        {
            Exports.exportCompetitors();
        }

        private void OpenReportEditor_Click ( object sender , RoutedEventArgs e )
        {
            new ReportDesigner( ).Show( );
        }

        private void Page_Loaded ( object sender , RoutedEventArgs e )
        {
            string ReportsPath = ((App)App.Current).CurrentChampionship.getChampionshipReportDir();

            var Reports = System.IO.Directory.EnumerateFiles( ReportsPath ).Where( s => s.EndsWith( ".rdlc" ) );

            gbxReports.Children.Clear( );

            foreach ( var Report in Reports )
            {
                var btn = new Button( ) { Content = System.IO.Path.GetFileNameWithoutExtension( Report ) };

                btn.Click += Btn_Click;

                gbxReports.Children.Add( btn );
            }
        }

        private void Btn_Click ( object sender , RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                string ReportsPath = ((App)App.Current).CurrentChampionship.getChampionshipReportDir();
                string filename = btn.Content.ToString();

                string Report = System.IO.Path.Combine( ReportsPath , filename + ".rdlc" );

                if ( File.Exists(Report))
                {
                    new ReportDesigner( Report ).Show( );
                }
                else
                {
                    MessageBox.Show( "File not found" );
                }
            }
        }
    }
}
