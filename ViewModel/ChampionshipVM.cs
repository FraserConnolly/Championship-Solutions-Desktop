using System;
using ChampionshipSolutions.DM;
using System.Linq;
using static ChampionshipSolutions.FileIO.FConnFile;
using MicroMvvm;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using ChampionshipSolutions.Reporting;
using System.Windows.Data;
using System.Collections.Generic;
using ChampionshipSolutions.Reports;
using ChampionshipSolutions.ControlRoom;

namespace ChampionshipSolutions.ViewModel
{
    public class ChampionshipVM : ObservableObject
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a ChampionshipViewModel
        /// </summary>
        public ChampionshipVM()
        {
            selectDefaultChampionship();
            UpdateCounters ( );
        }

        #endregion

        #region Members

        Championship _championship;
        ObservableCollection<EventVM> _events = new ObservableCollection<EventVM>();
        ObservableCollection<Championship> _championships = new ObservableCollection<Championship>();
        ObservableCollection<GroupVM> _groups = new ObservableCollection<GroupVM>();
        ObservableCollection<TeamVM> _teams = new ObservableCollection<TeamVM>();
        ObservableCollection<TemplateVM> _templates = new ObservableCollection<TemplateVM>();
        ObservableCollection<SchoolVM> _schools = new ObservableCollection<SchoolVM>();
        //List<AthleteVM> _athletes = new List<AthleteVM>();
        ObservableCollection<AthleteVM> _athletes = new ObservableCollection<AthleteVM>();
        ObservableCollection<PersonVM> _people = new ObservableCollection<PersonVM>();
        //CollectionView _athletesCollectionView ;
        public WebServerVM WebServer { get; set; }
        #endregion

        #region Properties

        public Championship Championship
        {
            get
            {
                return _championship;
            }
            set
            {
                if (value == null)
                    // reject - I'm not sure why this is happening when Championships is being updated??
                    return;

                if (Championship != value)
                {
                    //SaveToDB();
                    _championship = value;
                    ChangeChampionship();
                }
            }
        }

        public string Name
        {
            get { return Championship.Name ; }
            set
            {
                if (Championship.Name != value)
                {
                    Championship.Name = value;
                    _championship.Save ( );
                    RaisePropertyChanged ( "Name" );
                    RaisePropertyChanged ( "ShortName" );
                    RaisePropertyChanged ( "Championships" );
                }
            }
        }

        public string ShortName
        {
            get { return Championship.ShortName; }
            set
            {
                if (Championship.ShortName != value)
                {
                    Championship.ShortName = value;
                    _championship.Save ( );
                    RaisePropertyChanged ( "ShortName");
                }
            }
        }

        public string Location
        {
            get { return _championship.Location; }
            set
            {
                if (_championship.Location == value)
                    return;

                _championship.Location = value;
                _championship.Save ( );
                RaisePropertyChanged("Location");
            }
        }

        public bool WebServerEnabled
        {
            get { return _championship.WebServerEnabled; }
            set
            {
                if (_championship.WebServerEnabled == value)
                    return;

                _championship.WebServerEnabled = value;
                WebServer.Enabled = value;
                RaisePropertyChanged("WebServerEnabled");
                _championship.Save ( );
            }
        }

        public int WebServerPort
        {
            get { return Championship.WebServerPort; }
            set
            {
                if (_championship.WebServerPort == value)
                    return;

                _championship.WebServerPort = value;
                WebServer.Port = value;
                RaisePropertyChanged ("WebServerPort");
                _championship.Save ( );
            }
        }

        public DateTime? Date
        {
            get { return _championship.Date; }
            set
            {
                if (_championship.Date == value)
                    return;

                _championship.Date = value;
                _championship.Save ( );
                RaisePropertyChanged ( "Date");
            }
        }

        public DateTime? AgeDateReference
        {
            get { return _championship.AgeDateReference; }
            set
            {
                if ( _championship.AgeDateReference == value )
                    return;

                _championship.AgeDateReference = value;
                _championship.Save ( );
                RaisePropertyChanged ( "AgeDateReference" );
            }
        }

        public Visibility ShowChampionshipSelector
        {
            get
            {
                if (AllowMultipleChampionship())
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public ObservableCollection<Championship> Championships
        {
            get
            {

                //CSDB context = FileIO.FConnFile.getContext();

                if ( ! GetFileDetails().isOpen )
                    return _championships;

                //!*!
                Championship[] dbChamps = GetFileDetails().IO.GetAll<DM.Championship>().ToArray();// context.Championships.ToArray();

                foreach (var champ in dbChamps)
                {
                    if ( !_championships.Contains ( champ ) )
                    {
                        _championships.Add ( champ );
                    }
                }

                foreach (var champ in _championships.ToArray())
                {
                    if (!dbChamps.Contains(champ))
                        _championships.Remove(champ);
                }

                return _championships;
            }
        }

        public ObservableCollection<EventVM> Events
        {
            get
            {
                AEvent[] dbEvents = Championship.Events;
                //AEvent[] dbEvents = GetFileDetails().IO.GetEventsForChampionship( Championship.ID ).ToArray();

                foreach ( var e in dbEvents.Where( x => x.IsFinal == true ))
                {
                    if ( _events.Where ( x => x.Event == e ).Count ( ) == 0 )
                        _events.Add ( new EventVM ( e ) );
                }

                foreach ( var e in _events.ToArray ( ) )
                {
                    if ( !dbEvents.Contains ( e.Event ) )
                        _events.Remove ( e );
                }

                return _events;
            }
        }

        public ObservableCollection<TeamVM> Teams
        {
            get
            {

                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //return _teams;

                //Team[] db = Championship.Teams;

                //!*!
                //Team[] db = GetFileDetails().IO.GetTeamsForChampionship(Championship.ID).ToArray() ;// Championship.Teams;
                Team[] db = Championship.Teams;

                foreach ( var x in db )
                    if ( _teams.Where ( t => t.Team ==  x ).Count() == 0 )
                        _teams.Add ( new TeamVM ( x , this ) );

                foreach ( var x in _teams.ToArray ( ) )
                    if ( !db.Contains ( x.Team ) )
                        _teams.Remove ( x );

                return _teams;
            }
        }

        public ObservableCollection<SchoolVM> Schools
        {
            get
            {
                School[] db = GetFileDetails().IO.GetAll<School>().OrderBy(s => s.Name).ToArray(); // context.Schools.ToArray().OrderBy(s=>s.Name).ToArray();

                foreach ( var x in db )
                    if ( _schools.Where ( s => s.School == x ).Count ( ) == 0 )
                        _schools.Add ( new SchoolVM ( x , this ) );

                foreach ( var x in _schools.ToArray ( ) )
                    if ( !db.Contains ( x.School ) )
                        _schools.Remove ( x );

                return _schools;
            }
        }

        public ObservableCollection<GroupVM> Groups
        {
            get
            {

                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                //return _groups;

                Group[] db = Championship.Groups;
                //!*!
                //Group[] db = GetFileDetails().IO.GetGroupsForChampionship(  Championship.ID ).ToArray();

                foreach ( var x in db )
                    if ( _groups.Where ( g => g.Group == x ).Count ( ) == 0 )
                        _groups.Add ( new GroupVM ( x , this ) );

                foreach ( var x in _groups.ToArray ( ) )
                    if ( !db.Contains ( x.Group ) )
                        _groups.Remove ( x );

                return _groups;
            }
        }

        public ObservableCollection<TemplateVM> Templates
        {
            get
            {
                DM.Template[] db = GetFileDetails().IO.GetAll<Template>().ToArray();

                foreach ( var x in db )
                    if ( _templates.Where ( t => t.Template == x ).Count ( ) == 0 )
                        _templates.Add ( new TemplateVM ( x , this ) );

                foreach ( var x in _templates.ToArray ( ) )
                    if ( !db.Contains ( x.Template ) )
                        _templates.Remove ( x );

                return _templates;
            }
        }

        #region Counters

        private bool UpdateCounter = true;
        private int CAthletes, CInTeams, CInEvent,CResults,CSelected;

        public string CountAthletes
        {
            get
            {
                Console.WriteLine ( "Counting Athletes" );
                return CAthletes.ToString ( );
                //return countAllAthletes( ).ToString ( );
            }
        }

        public string CountAthletesInTeams 
        {
            get
            {
                Console.WriteLine ( "Counting Athletes in Teams" );
                return CInTeams.ToString ( );
                //return Championship.ListAllAthletes ( ).Count.ToString ( );
            }
        }

        public string CountAthletesEntered
        {
            get
            {
                Console.WriteLine ( "Counting Competitors" );
                return CInEvent.ToString ( );
                //return Championship.ListAllAthletes ( ).Where( a => a.GetCompetitors(Championship).Count() > 0 ).Count().ToString ( );
            }
        }

        public string CountAthletesWithResults
        {
            get
            {
                Console.WriteLine ( "Counting Results" );
                return CResults.ToString ( );
                //return Championship.ListAllAthletes ( ).Where(a => a.HasResults()).Count().ToString ( );
            }
        }

        public string CountSelected
        {
            get
            {
                Console.WriteLine ( "Counting Selected" );
                return CSelected.ToString ( );
            }
        }

        public void UpdateCounters()
        {
            if ( UpdateCounter )
            {
                if ( GetFileDetails ( ).isOpen )
                {
                    SQLCommands.CounterCommands.CountAthletes ( GetFileDetails ( ).Connection , Championship.ID ,
                    out CAthletes , out CInTeams , out CInEvent , out CResults , out CSelected );

                    RaisePropertyChanged ( "CountAthletes" );
                    RaisePropertyChanged ( "CountAthletesInTeams" );
                    RaisePropertyChanged ( "CountAthletesEntered" );
                    RaisePropertyChanged ( "CountAthletesWithResults" );
                    RaisePropertyChanged ( "CountSelected" );
                }
            }
        }

        #endregion

        public ObservableCollection<AthleteVM> Athletes
        {
            get
            {

                //Athlete[] db = context.People.ToArray().Where(p => p.Discriminator == "Athlete" || p.Discriminator == "StudentAthlete").Select(p => p as Athlete).ToArray<DM.Athlete>();
                Athlete[] db = GetFileDetails().IO.GetAll<Person>().ToArray().Where(p => p.Discriminator == "Athlete" || p.Discriminator == "StudentAthlete").Select(p => p as Athlete).ToArray<DM.Athlete>();

                foreach (var x in db)
                    if (_athletes.Where(t => t.Athlete == x).Count() == 0)
                        _athletes.Add(new AthleteVM(x));

                foreach (var x in _athletes.ToArray())
                    if (!db.Contains(x.Athlete))
                        _athletes.Remove(x);

                return _athletes;
            }
        }

        public ObservableCollection<PersonVM> People
        {
            get
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //return _people;

                //Person[] db = context.People.ToArray().Where ( p => p.Discriminator == "Person" ).ToArray();
                //!*!
                Person[] db = GetFileDetails().IO.GetAll<Person>().ToArray().Where ( p => p.Discriminator == "Person" ).ToArray();

                foreach ( var x in db )
                    if ( _people.Where ( t => t.Person == x ).Count ( ) == 0 )
                        _people.Add ( new PersonVM ( ) { Person = x } );

                foreach ( var x in _people.ToArray ( ) )
                    if ( !db.Contains ( x.Person ) )
                        _people.Remove ( x );

                return _people;
            }
        }

        #endregion

        #region public methods

        #region Get Directories

        [Obsolete("Not required as working files are now stored next to the CSDB file.",true)]
        public string getHomeDir()
        {
            //var fileDetail = GetFileDetails( );
            //string tempPath = Path.Combine( Path.GetDirectoryName( fileDetail.FilePath ) , Championship.FixedName );
            string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Championship Solutions");
            //string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Championship Solutions");

            if (! Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            return tempPath;
        }

        public string getChampionshipHomeDir()
        {
            var fileDetail = GetFileDetails( );
            string tempPath = Path.Combine( Path.GetDirectoryName( fileDetail.FilePath ) , Championship.FixedName );
            //string tempPath = Path.Combine(getHomeDir(), Championship.FixedName);

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            return tempPath;
        }

        internal string getChampionshipWebDir ( )
        {
            string tempPath = Path.Combine(getChampionshipHomeDir(), "Web Pages");
            string tempApplicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Default Web Pages");

            if ( Directory.Exists( tempPath ) )
                return tempPath;
                //Directory.CreateDirectory ( tempPath );

            if ( !Directory.Exists( tempApplicationPath ) )
                Directory.CreateDirectory( tempPath );

            return tempApplicationPath;
        }

        internal string getChampionshipExportsDir ( )
        {
            string tempPath = Path.Combine(getChampionshipHomeDir(), "Exports");

            if ( !Directory.Exists ( tempPath ) )
                Directory.CreateDirectory ( tempPath );

            return tempPath;
        }

        internal string getChampionshipReportDir ( )
        {
            string tempPath = Path.Combine(getChampionshipHomeDir(), "Reports");
            string tempApplicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");

            if ( Directory.Exists( tempPath ) )
                return tempPath;

            if ( !Directory.Exists( tempApplicationPath ) )
                Directory.CreateDirectory( tempPath );

            return tempApplicationPath;
        }

        [Obsolete("Not required as it was never written.",true)]
        internal string getChampionshipScriptsDir( )
        {
            string tempPath = Path.Combine(getChampionshipHomeDir(), "Scripts");

            if ( !Directory.Exists ( tempPath ) )
                Directory.CreateDirectory ( tempPath );

            return tempPath;
        }

        #endregion

        #endregion

        #region Private Methods

        #region Update Function

        internal void UpdateAthletes ( )
        {
            RaisePropertyChanged ( "Athletes" );
            RaisePropertyChanged ( "People" );
        }

        internal void UpdateEvents ( )
        {
            RaisePropertyChanged ( "Events" );
        }

        internal void updateTeams ( )
        {
            RaisePropertyChanged ( "Teams" );
            foreach ( SchoolVM sch in Schools  )
                sch.UpdateAllTeams ( );
        }

        internal void updateSchools ( )
        {
            RaisePropertyChanged ( "Schools" );
            foreach ( SchoolVM sch in Schools )
                sch.UpdateAllTeams ( );
        }

        internal void updateGroups ( )
        {
            RaisePropertyChanged ( "Groups" );
        }

        internal void updateTemplates ( )
        {
            CSReportLibrary.getLibrary().loadTemplatesFromDatabase();
            RaisePropertyChanged ( "Templates" );
        }

        #endregion

        //private int countAllAthletes ()
        //{
        //    //CSDB context = getContext();

        //    //if ( context == null ) return 0;

        //    //return context.People.ToArray().Where ( p => p.Discriminator == "Athlete" || p.Discriminator == "StudentAthlete" ).Count ( );
        //    //!*!
        //    return GetFileDetails().IO.GetAll<Person>().ToArray ( ).Where ( p => p.Discriminator == "Athlete" || p.Discriminator == "StudentAthlete" ).Count ( );
        //}

        private Championship createNewChampionship()
        {
            showDialog:

            Window newChampionship = new Window();
            //StackPanel contents = new StackPanel();
            Grid contents = new Grid();
            TextBox tbxFixedName = new TextBox();
            Button btnDone = new Button();

            newChampionship.Width = 300;
            newChampionship.Height = 100;
            newChampionship.Title = "Create New Championship";
            newChampionship.Content = contents;

            contents.Children.Add(tbxFixedName);
            contents.Children.Add(btnDone);

            tbxFixedName.Text = "Enter the name of the new championship";

            btnDone.HorizontalAlignment = HorizontalAlignment.Right;
            btnDone.VerticalAlignment = VerticalAlignment.Bottom;
            btnDone.Content = "Done";
            btnDone.Width = 100;
            btnDone.Click += BtnDone_Click;

            if (newChampionship.ShowDialog() == true)
                return new Championship(newChampionship.Title);
            else
                goto showDialog;
        }

        private void BtnDone_Click(object sender, RoutedEventArgs e)
        {
            Grid contents = (Grid)((Button)sender).Parent;

            TextBox tbxFixedName = (TextBox)contents.Children[0];

            if (string.IsNullOrWhiteSpace(tbxFixedName.Text) || tbxFixedName.Text == "Enter the name of the new championship")
            {
                MessageBox.Show("The new championship must have a name");
            }
            else
            {
                ((Window)contents.Parent).DialogResult = true;
                ((Window)contents.Parent).Title = tbxFixedName.Text;
            }

        }

        private void ChangeChampionship()
        {
            if ( GetFileDetails ( ).isOpen )
            {
                try
                {

                    if ( Championship.ZippedFileStore == null )
                    {
                        // build directory structure

                        if ( Name != "" && Name != "Temporary" && !IsEntryForm() )
                        {
                            getChampionshipExportsDir ( );
                            //getChampionshipScriptsDir ( );
                            getChampionshipWebDir ( );

                            //DirectoryCopy ( Path.Combine ( Path.GetDirectoryName ( System.Reflection.Assembly.GetExecutingAssembly ( ).Location ) , "Default Scripts" ) , getChampionshipScriptsDir ( ) , true );
                            //DirectoryCopy ( Path.Combine ( Path.GetDirectoryName ( System.Reflection.Assembly.GetExecutingAssembly ( ).Location ) , "Default Web Pages" ) , getChampionshipWebDir ( ) , true );
                        }

                    }
                    else
                    {
                        throw new NotImplementedException ( );
                        // Todo extract zipped storage
                    }
                }
                catch
                {
                    throw new Exception ( "Failed to setup the storage area on load" );
                }

                WebServer = new WebServerVM ( this );

                if ( WebServerEnabled )
                    WebServer.start ( );
            }


            RaisePropertyChanged ("Championship");
            RaisePropertyChanged("Name");
            RaisePropertyChanged("ShortName");
            RaisePropertyChanged("Location");
            RaisePropertyChanged("Date");
            RaisePropertyChanged("WebServerPort");
            RaisePropertyChanged("WebServerEnabled");
        }


        /// <summary>
        /// Call Save changes after this function.
        /// This will throw an exception if the file is not a multiple championships file.
        /// </summary>
        private void AddToDatabase()
        {
            if (_championship == null) return;

            if ( GetFileDetails().IO.GetAll<Championship>().Count() > 0)
            {
                if (AllowMultipleChampionship())
                {
                    GetFileDetails ( ).IO.Add<Championship> ( _championship );
                }
                else
                {
                    // Can not add a database to a Single Championship or Entry Form file format
                    throw new Exception("Additional Championships can not be added to this file.");
                }
            }
            else
            {
                GetFileDetails ( ).IO.Add<Championship> ( _championship );
            }
        }

        private bool AllowMultipleChampionship()
        {
            return (FileIO.FConnFile.GetFileDetails().FileFormat == FConnFileHelper.FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP);
        }

        private bool isInDataBase()
        {
            return Championships.Contains(_championship);
        }

        private void selectDefaultChampionship()
        {
            
            if ( Championships.Count == 0 )
            {
                if ( isFileOpen( ) )
                {
                    this.Championship = createNewChampionship( );
                    SaveToDB( );
                }
                else
                    this.Championship = new Championship( "Temporary" );
            }
            else
                Championship = Championships[0];
        }

        #endregion

        #region Commands

        public ICommand Save { get { return new RelayCommand(SaveToDB, CanSaveToDB); } }

        void SaveToDB ()
        {
            if (!GetFileDetails().isOpen) return;

            if (!isInDataBase())
                AddToDatabase();

            //!*!
            GetFileDetails ( ).IO.Update<Championship> ( Championship );

            //SaveChanges();
            clearHasChanges();
        }

        bool CanSaveToDB()
        {
            // this code doesn't showt the update if only a child has updated.
            //if (!GetFileDetails().isOpen) return false;

            //if (isInDataBase())
            //{
            //    // Does the record need updating?
            //    if (hasChanges)
            //        return true;
            //}
            //else
            //{
            //    if (GetFileDetails().FileFormat == FConnFileHelper.FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP)
            //        return true;
            //    else
            //        if (getContext().Championships.Count() == 0)
            //            return true;
            //}

            //return false;

            if (!GetFileDetails().isOpen) return false;
            //return (getContext().HasChanges());
            return true;
        }

        public bool canModify ( ) { return !IsEntryForm( ) ; }

        public bool canModifyInEntryForm ( )
        {
            if ( IsEntryForm( ) ) return true;
            return canModify( );
        }

        public bool IsEditable { get { return !IsEntryForm( ); } }

        #region Templates

        public ICommand NewTemplate { get { return new RelayCommand ( newTemplate , canModify ); } }
        
        private void newTemplate ( )
        {
            ReportEditor re = new ReportEditor();
            //re.LoadReportFromApplication ( AReport.LoadTemplate ( this.Template.Instructions , this.Template.TemplateFile ) );
            re.ShowDialog ( );

            //this.Template.Instructions = re.report.rawXML;
            (( App )Application.Current ).ReportLibrary.addTemplatesToDB ( re.report );

            //SaveChanges ( );

            updateTemplates ();
        }

        #endregion

        #region Championship 

        public ICommand ImportEntryForm { get { return new RelayCommand( importEntryForm , canModify); } }


        public void importEntryForm( )
        {
            throw new NotImplementedException( );
        }

        public ICommand DeleteChampionship { get { return new RelayCommand(deleteFromDatabase, canDelete); } }

        private bool canDelete()
        {
            // can not delete the only championship
            if (Championships.Count == 1)
                return false;

            return Championship.CanDelete();
        }

        private void deleteFromDatabase()
        {
            if (MessageBox.Show("Are you sure you want to delete " + Name + "? This cannot be undone.", "Are you sure",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if (context == null)
                    //throw new Exception("File error when deleting championship");

                string dirPath = getChampionshipHomeDir();

                Championship temp = this.Championship;

                if (Directory.Exists(dirPath))
                    Directory.Delete(dirPath);

                selectDefaultChampionship();

                //context.Championships.DeleteOnSubmit(temp);
                //SaveChanges();

                //!*!
                GetFileDetails ( ).IO.Delete<Championship> ( temp );

                RaisePropertyChanged("Championships");
            }
        }

        public ICommand NewChampionship { get { return new RelayCommand(newChampionship); } }

        private void newChampionship()
        {
            if (hasChanges)
                SaveToDB();

            Championship = createNewChampionship();
            RaisePropertyChanged("Championships");
            SaveToDB();
        }

        #endregion

        #region Teams

        public ICommand NewTeam { get { return new RelayCommand ( newTeam , canModify ); } }

        private void newTeam ( )
        {
            Championship.addTeam ( new Team (string.Empty, Championship ) );
            //SaveToDB ( );
            updateTeams ( );
        }

        #endregion

        #region Schools

        public ICommand NewSchool { get { return new RelayCommand ( newSchool , canModify ); } }

        private void newSchool ( )
        {
            //getContext ( ).Schools.InsertOnSubmit( new School ( ) );
            //SaveToDB ( );
            GetFileDetails ( ).IO.Add<School> ( new School ( ) );
            RaisePropertyChanged ( "Schools" );
        }

        #endregion

        #region Exports

        public ICommand OpenExportsDir { get { return new RelayCommand ( openExportsDir ); } }

        private void openExportsDir ( )
        {
            System.Diagnostics.Process.Start ( getChampionshipExportsDir ( ) );
        }

        #endregion

        #region Web Server

        public ICommand OpenWebSite { get { return new RelayCommand ( openWebSite , canOpenWebsite ); } }

        private void openWebSite ( )
        {
            System.Diagnostics.Process.Start ( "http://127.0.0.1:" + WebServerPort );
        }

        private bool canOpenWebsite()
        {
            if (WebServer == null) return false;

            return ( WebServer.Enabled );
        }

        public ICommand OpenWebSiteDir { get { return new RelayCommand ( openWebSiteDir , canOpenWebsite ); } }

        private void openWebSiteDir()
        {
            System.Diagnostics.Process.Start ( getChampionshipWebDir() );
        }

        #endregion

        #region Groups

        public ICommand NewGroup { get { return new RelayCommand ( newGroup , canModify ); } }
        public ICommand NewGenderRestriction { get { return new RelayCommand ( newGender , canModify ); } }
        public ICommand NewAgeRestriction { get { return new RelayCommand ( newAge , canModify ); } }
        public ICommand NewDoBRestriction { get { return new RelayCommand ( newDoB , canModify ); } }

        private void newGroup ( )
        {
            new Group ( string.Empty , Championship ); // auto adds to championship in DM
            //SaveToDB ( );
            RaisePropertyChanged ( "Groups" );
        }

        private void newGender ( )
        {
            new GenderRestriction ( Championship ); // auto adds to championship in DM
            //SaveToDB ( );
            RaisePropertyChanged ( "Groups" );
        }

        private void newAge ( )
        {
            new AgeRestriction ( Championship ); // auto adds to championship in DM
            //SaveToDB ( );
            RaisePropertyChanged ( "Groups" );
        }

        private void newDoB ( )
        {
            new DoBRestriction ( Championship ); // auto adds to championship in DM
            //SaveToDB ( );
            RaisePropertyChanged ( "Groups" );
        }


        #endregion

        #region Events

        public ICommand NewEvent { get { return new RelayCommand ( newEvent , canModify ); } }

        private void newEvent(object obj)
        {
            NewEvent ne = new NewEvent();

            

            if ( ne.ShowDialog ( ) == true )
            {
                if ( ne.Event == null )
                    MessageBox.Show("An error occurred, the event was not made");

                Championship.addEvent ( ne.Event );
                //SaveToDB ( );

                EditEvent ee = new EditEvent ();
                ee.DataContext = new EventVM ( ne.Event );
                ee.ShowDialog ( );
                SaveToDB ( );

                RaisePropertyChanged ( "Events" );

                if ( obj is EventsPage )
                    ( (EventsPage)obj ).ReloadPage ( );
            }
            else
            {
                return;
            }
        }

        public bool canExecuteInFullOnly ( )
        {
            return !IsEntryForm( );
        }

        public ICommand DeleteEvent { get { return new RelayCommand ( delEvent , canExecuteInFullOnly ); } }

        private void delEvent(object obj)
        {
            // check if we can delete this even as canDeleteEvent doesn't check properly

            if ( obj is EventVM Event )
            {
                if ( Event.Event.CanDelete( ) )
                {
                    if ( MessageBox.Show( "Are you sure you want to delete " + Event.Name + "? This can not be undo." , "Remove Event" , MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
                    {
                        Event.Championship.Championship.RemoveEvent( Event.Event );

                        SaveToDB( );

                        RaisePropertyChanged( "Events" );
                        ((App) App.Current).EventPage.ReloadPage( );
                    }
                }
                else
                    MessageBox.Show( "This event can not be deleted because it has results or heats" );
            }
        }

        #endregion

        #region Athletes

        public ICommand AddPerson { get { return new RelayCommand ( newPerson , canModify ); } }
        private void newPerson ( )
        {
            //CSDB context = getContext();

            //if ( context == null ) return;

            Person p = new Person ( );

            //context.People.InsertOnSubmit ( p );
            //SaveToDB ( );

            //!*!
            GetFileDetails ( ).IO.Add<Person> ( p );

            editPerson ( new PersonVM ( ) { Person = p } );
        }


        public ICommand AddAthleteDialog        { get { return new RelayCommand ( newAthleteDiaglog ); } }
        private void newAthleteDiaglog ( )
        {
            //CSDB context = getContext();

            //if ( context == null ) return;

            Athlete p = new Athlete ( );

            //context.People.InsertOnSubmit ( p );
            //SaveToDB ( );

            GetFileDetails ( ).IO.Add<Person> ( p );

            editPerson ( new AthleteVM( p ) );
        }

        public ICommand OpenManageAthleteDialog { get { return new RelayCommand ( openManageAthleteDialog ); } }
        private void openManageAthleteDialog ( )
        {
            ManageAthletesWindow maw = new ManageAthletesWindow();
            maw.Show ( );
        }


        public ICommand EditPerson {  get { return new RelayCommand ( editPerson ); } }
        private void editPerson ( object obj )
        {
            if ( obj is PersonVM )
            {
                EditAthleteWindow eaw = new EditAthleteWindow();
                eaw.DataContext = obj;
                //eaw.ShowDialog ( ) ;
                eaw.Show ( ) ;
             
                //((PersonVM) obj).Person.Save ( );
                //RaisePropertyChanged ( "Athletes" );
                //RaisePropertyChanged ( "People" );
            }
        }

        public ICommand DeletePerson { get { return new RelayCommand ( deletePerson , canModifyInEntryForm ); } }
        private void deletePerson ( object obj )
        {
            if ( obj is PersonVM )
                if ( (( PersonVM ) obj ). CanDelete() )
                {
                    if ( MessageBox.Show("Are you sure you want to delete " + ( (PersonVM)obj ).PreferredName, "Delete Person",MessageBoxButton.YesNo) == MessageBoxResult.Yes )
                    {
                        GetFileDetails ( ).IO.Delete<Person> ( ((PersonVM)obj).Person );

                        RaisePropertyChanged ( "Athletes" );
                        RaisePropertyChanged ( "People" );
                    }
                    //MessageBox.Show ( "Would delete but not implemented" );
                }
                else
                {
                    MessageBox.Show ( "Can not be deleted" );
                }
        }

        public ICommand OpenProfile { get { return new RelayCommand ( openProfile ); } }

        private void openProfile ( object obj )
        {
            if ( obj is AthleteVM )
            {
                List<Athlete> athletes = new List<Athlete>();

                athletes.Add ( ( (AthleteVM)obj).Athlete );

                string previewFile;

                try
                {
                    previewFile = Exports.GenerateAthleteProfile ( athletes , false , PrintOptions.NO_PRINT , true, true );
                }
                catch ( Exception ex )
                {
                    MessageBox.Show ( "Failed to open athletes profile sheets. \n" + ex.Message );
                    return;
                }

                PDFViewer.OpenOnSTAThread( previewFile);

            }
        }

        #endregion

        #endregion

        #region Filters


        #endregion

        public override string ToString ( )
        {
            return this.Name;
        }

    }
}
