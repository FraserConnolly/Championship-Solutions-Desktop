using MicroMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    /// Interaction logic for ImportResultsPage.xaml
    /// </summary>
    public partial class ImportEntryFormPage : Page
    {
        private Page returnPage;

        public ImportEntryFormPage ( )
        {
            InitializeComponent();
            xmlExample.Text = 
@"<? xml version = ""1.0"" encoding = ""utf-8"" ?>
<EntryForm>
    <Teams>
        <Team>{Team Short Name}</Team>
    </Teams>
    <Schools>
        <School>{School Short Name}</School>
    </Schools>
    <Entries>
        <Athlete ID = ""2275"" Name = ""Emily-Rose Duffy"" SchoolCode = ""Lydi"" CategoryCode = ""IG"" TeamCode = ""SWI"" DateOfBirth = ""2001-00-22"" GlobalID = ""2310"" >
            <Events>
                <Event EventCode = ""T44"" VestNo = ""0"" />
            </Events>
            <Changes>
                <Change Name = ""Emily  Duffy"" DateOfBirth = ""2001-00-22"" SchoolCode = ""Lydi "" />
            </Changes>
        </Athlete>
    </Entries>
</EntryForm>";
        }

        public ImportEntryFormPage(Page ReturnPage):this()
        {
            returnPage = ReturnPage;
        }

        private void Wizard_Finish ( object sender , RoutedEventArgs e )
        {
            MainWindow.Unlock();
            if ( returnPage != null ) MainWindow.Frame.Navigate ( returnPage );
        }

        private void Wizard_PageChanged ( object sender , RoutedEventArgs e )
        {
            //MessageBox.Show ( "You have changed the page." );
        }

        private void Wizard_Help ( object sender , RoutedEventArgs e )
        {
            switch ( ImportWizard.CurrentPage.Name )
            {
                case "IntroPage":
                    break;
                case "OpenImportPage":
                    break;
                case "CheckTeams":
                    MessageBox.Show ( 
@"If one or more teams is not in the database
then you have to click on the + button to add them
before you can continue" );
                    break;
                case "CheckSchools":
                    MessageBox.Show (
@"If one or more schools is not in the database
then you have to click on the + button to add them
before you can continue" );
                    break;
                case "LastPage":
                    MessageBox.Show ( "Nearly done, just hit finish" );
                    break;
                default:
                    MessageBox.Show ( "I don't know what page I'm on" );
                    break;
            }
        }

        private void Wizard_Cancel ( object sender , RoutedEventArgs e )
        {
            MainWindow.Unlock ( );
            if ( returnPage != null ) MainWindow.Frame.Navigate ( returnPage );
        }

        private void Page_Loaded ( object sender , RoutedEventArgs e )
        {
            MainWindow.Lock ( );
        }

        private void Grid_Drop ( object sender , DragEventArgs e )
        {
            if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                // HandleFileOpen ( files[0] );

                if ( files.Count ( ) == 1 )
                {
                    if ( files.First ( ).EndsWith ( ".xml" ) )
                    {
                        openImportFile ( files.First ( ) );
                    }
                    else
                    {
                        OpenMessage.Text = "The entry form should be an xml file";
                        OpenImportPage.CanSelectNextPage = false;
                        OpenMessage.Foreground = Brushes.Red;
                    }
                }
                else if ( files.Count ( ) > 1 )
                {
                    OpenMessage.Text = "Please only drop one file at a time";
                    OpenImportPage.CanSelectNextPage = false;
                    OpenMessage.Foreground = Brushes.Red;
                }
                else
                {
                    OpenMessage.Text = "";
                    OpenImportPage.CanSelectNextPage = false;
                    OpenMessage.Foreground = Brushes.Red;
                }

            }
        }

        private ImportDataStorage entryForm;

        private void openImportFile ( string v )
        {
            if ( true )
            {
                entryForm = new ImportDataStorage ( v, this );

                CheckSchools.DataContext = entryForm;
                CheckTeams.DataContext = entryForm;
                CheckAthletes.DataContext = entryForm;

                OpenMessage.Text = "Click next to continue";
                OpenMessage.Foreground = Brushes.CadetBlue;
                OpenImportPage.CanSelectNextPage = true;

                BackgroundWorker worker = new BackgroundWorker();

                BusyIndicator.IsBusy = true;

                worker.DoWork += ( o , a ) =>
                {
                    entryForm.ReadTeamsAndSchools ( );
                };

                worker.RunWorkerCompleted += ( o , a ) =>
                {
                    BusyIndicator.IsBusy = false;
                    changeHappend ( );
                };

                worker.RunWorkerAsync ( );
            }
        }

        public void changeHappend()
        {
            if ( entryForm != null )
            {
                if ( entryForm.Schools.Count( c => c.item != null ) == entryForm.Schools.Count( ) )
                    Dispatcher.Invoke( new Action( ( ) =>
                        CheckSchools.CanSelectNextPage = true ) );
                else
                    Dispatcher.Invoke( new Action( ( ) =>
                    CheckSchools.CanSelectNextPage = false ) );

                if ( entryForm.Teams.Count ( c => c.item != null ) == entryForm.Teams.Count ( ) )
                    Dispatcher.Invoke( new Action( ( ) =>
                    CheckTeams.CanSelectNextPage = true));
                else
                    Dispatcher.Invoke( new Action( ( ) =>
                    CheckTeams.CanSelectNextPage = false));
            }
        }

        private void Finished_Loaded ( object sender , RoutedEventArgs e )
        {
            if ( entryForm == null ) return;

            BackgroundWorker worker = new BackgroundWorker();

            BusyIndicator.IsBusy = true;

            worker.DoWork += ( o , a ) =>
            {
                entryForm.EnterAthletes ( );
            };

            worker.RunWorkerCompleted += ( o , a ) =>
            {
                BusyIndicator.IsBusy = false;
            };

            worker.RunWorkerAsync ( );

        }
    }

    public class ImportDataStorage
    {
        public string FilePath { get; set; }
        public List<IIdentityStorage> Teams { get; private set; }
        public List<IIdentityStorage> Schools { get; private set; }
        public List<AthleteStorage> Athletes { get; private set; }
        private ImportEntryFormPage Wizard { get; set; }
        //public DataTable Teams { get; set; }
        //public DataTable Schools { get; set; }
        //public DataTable Athletes { get; set; }

        private XmlDocument doc = new XmlDocument();

        public ImportDataStorage ( string FileName, ImportEntryFormPage Wizard )
        {
            this.Wizard = Wizard;
            FilePath = FileName;

            Teams = new List<IIdentityStorage> ( );
            Schools  = new List<IIdentityStorage> ( );
            Athletes = new List<AthleteStorage> ( );

            //Teams = new DataTable ( "Teams" );
            //Teams.Columns.Add ( "Name" );
            //Teams.Columns.Add ( "ShortName" );
            //Teams.Columns.Add ( "AthleteCount" );


            //Schools = new DataTable ( "Schools" );
            //Schools.Columns.Add ( "Name" );
            //Schools.Columns.Add ( "ShortName" );
            //Schools.Columns.Add ( "AthleteCount" );

        }

        public void ReadTeamsAndSchools ( )
        {
            Schools.Clear ( );
            Teams.Clear ( );
            Athletes.Clear ( );

            doc.Load ( FilePath );

            foreach ( XmlElement teams in doc.SelectSingleNode ( "EntryForm" ).SelectSingleNode ( "Teams" ).ChildNodes )
            {
                IIdentityStorage t = new IIdentityStorage();

                t.ShortName = teams.InnerText;

                t.item = FileIO.FConnFile.GetFileDetails ( ).IO.GetAll<DM.Team> ( ).Where ( te => te.ShortName.Trim ( ) == t.ShortName.Trim ( ) ).FirstOrDefault ( );
                t.processor = this;
                Teams.Add ( t );
            }

            foreach ( XmlElement school in doc.SelectSingleNode ( "EntryForm" ).SelectSingleNode ( "Schools" ).ChildNodes )
            {
                IIdentityStorage t = new IIdentityStorage();

                t.ShortName = school.InnerText;

                t.item = FileIO.FConnFile.GetFileDetails ( ).IO.GetAll<DM.School> ( ).Where ( te => te.ShortName.Trim() == t.ShortName.Trim() ).FirstOrDefault ( );

                t.isSchool = true;
                t.processor = this;
                Schools.Add ( t );
            }

            foreach ( XmlElement athlete in doc.SelectSingleNode ( "EntryForm" ).SelectSingleNode ( "Entries" ).ChildNodes )
            {
                try
                {

                AthleteStorage a = new AthleteStorage();

                a.Name = athlete.Attributes["Name"].Value.ToString ( ).Replace ( "  " , " " );
                a.DateOfBirth = DateTime.Parse ( athlete.Attributes["DateOfBirth"].Value );

                a.School = FileIO.FConnFile.GetFileDetails ( ).IO.GetAll<DM.School> ( ).Where ( te => te.ShortName.Trim ( ) == athlete.Attributes["SchoolCode"].Value.Trim ( ) ).FirstOrDefault ( );
                a.Team = FileIO.FConnFile.GetFileDetails ( ).IO.GetAll<DM.Team> ( ).Where ( te => te.ShortName.Trim ( ) == athlete.Attributes["TeamCode"].Value.Trim ( ) ).FirstOrDefault ( );

                if ( athlete.Attributes["GlobalID"].Value != "" )
                    a.GlobalID = int.Parse ( athlete.Attributes["GlobalID"].Value );

                if ( athlete.Attributes["CategoryCode"].Value.Contains ( "B" ) )
                    a.Gender = DM.Gender.Male;
                else
                    a.Gender = DM.Gender.Female;

                a.EventCodes = new List<string> ( );

                if ( athlete.SelectSingleNode ( "Events" ).HasChildNodes )
                    foreach ( XmlElement ev in athlete.SelectSingleNode ( "Events" ).ChildNodes )
                        a.EventCodes.Add ( ev.Attributes["EventCode"].Value );

                Athletes.Add ( a );

                Console.WriteLine ( "Athlete Read: " + a.Name );
            }
                catch ( Exception ex)
                {
                    MessageBox.Show ( "Failed to read " + athlete.Attributes["Name"].Value.ToString ( ) + "\n" + ex.Message );
                    continue;
                }

            }

            Wizard.changeHappend ( );
        }

        internal void EnterAthletes ( )
        {
            foreach ( AthleteStorage athlete in Athletes )
            {
                try {
                    DM.Athlete ath;

                    if ( athlete.GlobalID != 0 )
                    {
                        ath = FileIO.FConnFile.GetFileDetails ( ).IO.GetID<DM.Athlete> ( athlete.GlobalID );

                        if ( ath == null )
                            ath = new DM.Athlete ( );
                    }
                    else
                        ath = new DM.Athlete ( );

                    ath.setFullName ( athlete.Name );
                    FileIO.FConnFile.GetFileDetails ( ).IO.Update<DM.Person> ( ath );
                    ath.setTeam ( athlete.Team , ( (App)App.Current ).CurrentChampionship.Championship );
                    ath.Attends = athlete.School;
                    ath.DateOfBirth = athlete.DateOfBirth;
                    ath.Gender = athlete.Gender;

                    FileIO.FConnFile.GetFileDetails ( ).IO.Update<DM.Person> ( ath );

                    foreach ( string EventCode in athlete.EventCodes )
                    {
                        DM.AEvent Event = (DM.AEvent) ( (App)App.Current ).CurrentChampionship.Championship.getEventShortName(EventCode);
                        if ( Event == null ) throw new Exception( $"Event with code '{EventCode}' could not be found in the database." );
                        Event.enterAthlete ( ath );
                    }
                }
                catch (Exception ex)
                {
                    var result = MessageBox.Show ( "Failed to enter an athlete " + athlete.Name + "\n" + ex.Message + "\nDo you wish to abort the rest of the import process?"
                        , "Import Error", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No );

                    Console.WriteLine ( "Failed to enter an athlete " + athlete.Name );
                    Console.WriteLine ( ex.Message );
                    if ( result == MessageBoxResult.Yes )
                    {
                        Console.WriteLine( "Aborted import process" );
                        return;
                    }
                    continue;
                }

            }
        }

        public class IIdentityStorage : ObservableObject
        {
            public DM.IIdentity item { get; set; }
            public string ShortName { get; set; }
            public string Name { get { if ( item == null ) return ""; return item.Name; } }
            public int AthleteCount { get; set; }
            public ICommand New { get { return new RelayCommand ( newItem , canNew ); } }
            public bool isSchool { get; set; }
            public ImportDataStorage processor {get ; set ; }

            private void newItem ( )
            {
                if ( isSchool )
                {
                    item = new DM.School ( ) { ShortName = this.ShortName };
                    FileIO.FConnFile.GetFileDetails ( ).IO.Add<DM.School> ( item );
                }
                else
                {
                    item = new DM.Team ( null , ( (App)App.Current ).CurrentChampionship.Championship ) { ShortName = this.ShortName };
                    FileIO.FConnFile.GetFileDetails ( ).IO.Add<DM.Team> (item);
                }

                RaisePropertyChanged ( "New" );
                processor.Wizard.changeHappend ( );
            }

            private bool canNew ( ) { return item == null; }
        }

        public struct AthleteStorage 
        {
            public bool isNewAthete { get; set; }
            public DM.Athlete athlete { get; set; }
            public string Name { get; set; }
            public DM.Team Team {get;set;}
            public DM.School School { get; set; }
            public int GlobalID { get; set; }
            public DateTime DateOfBirth { get; set; }
            public DM.Gender Gender { get; set; }

            // To do this doesn't have anywhere to store vest numbers yet
            public List<string> EventCodes { get; set; }
        }
    }


}
