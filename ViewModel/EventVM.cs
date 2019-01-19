using ChampionshipSolutions.ControlRoom;
using ChampionshipSolutions.DM;
using GongSolutions.Wpf.DragDrop;
using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static ChampionshipSolutions.FileIO.FConnFile;


namespace ChampionshipSolutions.ViewModel
{
    public class EventVM : ObservableObject , IDropTarget
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a EventVM
        /// </summary>
        public EventVM()
        {
            Event = null ; // there is no default event type so this must be initialised to null
                           // and checked elsewhere to see if it is assigned.
        }

        public EventVM(AEvent Event)
        {
            this.Event = Event;
        }

        #endregion

        #region Members
        
        public AEvent Event { get; private set; }
        ObservableCollection<EventVM> _heats = new ObservableCollection<EventVM>();
        ObservableCollection<GroupVM> _groups = new ObservableCollection<GroupVM>();
        ObservableCollection<GroupVM> _allGroups = new ObservableCollection<GroupVM>();
        ObservableCollection<DM.Standard> _standards = new ObservableCollection<DM.Standard>();
        //ObservableCollection<DM.ACustomDataValue> _customData = new ObservableCollection<DM.ACustomDataValue>();

        #endregion

        private bool UpdateEvent()
        {
            clearHasChanges();
            return GetFileDetails ( ).IO.Update<AEvent> ( Event );
        }

        #region Properties

        public ChampionshipVM Championship
        {
            get
            {
                return ((App)App.Current).CurrentChampionship;
            }
        }

        public string Name
        {
            get
            {
                if (Event != null)
                    return Event.Name;
                else
                    return "";
            }
            set
            {
                if (Event.Name != value)
                {
                    Event.Name = value;
                    UpdateEvent ( );
                    RaisePropertyChanged("Name");
                    RaisePropertyChanged("ShortName");
                }
            }
        }

        public string ShortName
        {
            get
            {
                if (Event != null)
                    return Event.ShortName;
                else
                    return "";
            }
            set
            {
                if (Event.ShortName != value)
                {
                    Event.ShortName = value;
                    UpdateEvent ( );
                    RaisePropertyChanged("ShortName");
                }
            }
        }

        public string Type { get { return Event.getEventType ( ); } }

        public DateTime? StartTime
        {
            get { return Event.StartTime; }
            set
            {
                if ( Event.StartTime == value )
                    return;

                Event.StartTime = value;
                RaisePropertyChanged ( "StartTime" );
            }
        }

        public int Length
        {
            get { return Event.Length; }
            set
            {
                if ( Event.Length == value )
                    return;

                Event.Length = value;
                RaisePropertyChanged ( "Length" );
            }
        }

        public int MaxCompetitors
        {
            get { return Event.MaxCompetitors; }
            set
            {
                if ( Event.MaxCompetitors == value )
                    return;

                Event.MaxCompetitors = value;
                RaisePropertyChanged ( "MaxCompetitors" );
            }
        }

        public int MaxCompetitorsPerTeam
        {
            get { return Event.MaxCompetitorsPerTeam; }
            set
            {
                if ( Event.MaxCompetitorsPerTeam == value )
                    return;

                Event.MaxCompetitorsPerTeam = value;
                RaisePropertyChanged ( "MaxCompetitorsPerTeam" );
            }
        }

        public int MaxGuests
        {
            get { return Event.MaxGuests; }
            set
            {
                if ( Event.MaxGuests == value )
                    return;

                Event.MaxGuests = value;
                RaisePropertyChanged ( "MaxGuests" );
            }
        }

        public int Lanes
        {
            get { return Event.Lanes; }
            set
            {
                if ( Event.Lanes == value )
                    return;

                Event.Lanes = value;
                RaisePropertyChanged ( "Lanes" );
            }
        }

        public int TopIndividualCertificates
        {
            get { return Event.TopIndividualCertificates; }
            set
            {
                if ( Event.TopIndividualCertificates == value )
                    return;

                Event.TopIndividualCertificates = value;
                RaisePropertyChanged ( "TopIndividualCertificates" );
            }
        }

        public int TopLowerYearGroupInividualCertificates
        {
            get { return Event.TopLowerYearGroupInividualCertificates; }
            set
            {
                if ( Event.TopLowerYearGroupInividualCertificates == value )
                    return;

                Event.TopLowerYearGroupInividualCertificates = value;
                RaisePropertyChanged ( "TopLowerYearGroupInividualCertificates" );
            }
        }

        public string CountyBestPerformanceName
        {
            get { return Event.CountyBestPerformanceName; }
            set
            {
                if ( Event.CountyBestPerformanceName == value )
                    return;

                Event.CountyBestPerformanceName = value;
                RaisePropertyChanged ( "CountyBestPerformanceName" );
            }
        }

        public string CountyBestPerformanceArea
        {
            get { return Event.CountyBestPerformanceArea; }
            set
            {
                if ( Event.CountyBestPerformanceArea == value )
                    return;

                Event.CountyBestPerformanceArea = value;
                RaisePropertyChanged ( "CountyBestPerformanceArea" );
            }
        }

        public int CountyBestPerformanceYear
        {
            get { return Event.CountyBestPerformanceYear; }
            set
            {
                if ( Event.CountyBestPerformanceYear == value )
                    return;

                Event.CountyBestPerformanceYear = value;
                RaisePropertyChanged ( "CountyBestPerformanceYear" );
            }
        }

        public string PrintChampionshipBestPerformance
        {
            get { return Event.PrintChampionshipBestPerformance; }
            set
            {
                if ( Event.PrintChampionshipBestPerformance == value )
                    return;

                Event.PrintChampionshipBestPerformance = value;
                RaisePropertyChanged ( "PrintChampionshipBestPerformance" );
            }
        }




        public TemplateVM ResultsTemplate
        {
            get
            {
                if ( Event != null )
                    if (Event.ResultsTemplate != null)
                        return new TemplateVM( Event.ResultsTemplate, Championship );
                
                return null;
            }
            set
            {
                if ( value == null )
                    Event.ResultsTemplate = null;
                else if ( Event.ResultsTemplate != value.Template  )
                    Event.ResultsTemplate = value.Template ;

                //!*!
                UpdateEvent ( );

                RaisePropertyChanged ( "ResultsTemplate" );
                //SaveToDB ( );
            }
        }
        public TemplateVM DataEntryTemplate
        {
            get
            {
                if ( Event != null )
                    if ( Event.DataEntryTemplate != null )
                        return new TemplateVM ( Event.DataEntryTemplate , Championship );

                return null;
            }
            set
            {
                if ( value == null )
                    Event.DataEntryTemplate = null;
                else if ( Event.DataEntryTemplate != value.Template  )
                    Event.DataEntryTemplate = value.Template ;
                //!*!
                UpdateEvent ( );

                RaisePropertyChanged ( "DataEntryTemplate" );
                //SaveToDB ( );
            }
        }
        public TemplateVM CertificateTemplate
        {
            get
            {
                if ( Event != null )
                    if ( Event.CertificateTemplate != null )
                        return new TemplateVM ( Event.CertificateTemplate , Championship );

                return null;
            }
            set
            {
                if ( value == null )
                    Event.CertificateTemplate = null;
                else if ( Event.CertificateTemplate != value.Template )
                    Event.CertificateTemplate = value.Template ;

                //!*!
                UpdateEvent ( );
                RaisePropertyChanged ( "CertificateTemplate" );
                //SaveToDB ( );
            }
        }
        public TemplateVM VestTemplate
        {
            get
            {
                if ( Event != null )
                    if ( Event.VestTemplate != null )
                        return new TemplateVM ( Event.VestTemplate , Championship );

                return null;
            }
            set
            {
                if ( value == null )
                    Event.VestTemplate = null;
                else if ( Event.VestTemplate != value.Template )
                    Event.VestTemplate = value.Template;

                //!*!
                UpdateEvent ( );
                RaisePropertyChanged ( "VestTemplate" );
                //SaveToDB ( );
            }
        }

        public ObservableCollection<GroupVM> Groups
        {
            get
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //return _groups;

                //!*!
                EventGroups[] db = Event.Groups;

                foreach ( var x in db )
                    if ( _groups.Where ( g => g.Group == x.Group ).Count ( ) == 0 )
                        _groups.Add ( new GroupVM ( x.Group , Championship , this.Event ) );

                foreach ( var x in _groups.ToArray ( ) )
                    if ( db.Where(g => g.Group == x.Group ).Count() == 0 )
                        _groups.Remove ( x );

                return _groups;
            }
        }

        public ObservableCollection<GroupVM> AllGroups
        {
            get
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //return _allGroups;

                //return Championship.Groups;

                GroupVM[] db = Championship.Groups.ToArray();

                foreach ( var x in db )
                    if ( _allGroups.Where ( g => g.Group == x.Group ).Count ( ) == 0 )
                        _allGroups.Add ( new GroupVM ( x.Group , Championship , this.Event ) );

                foreach ( var x in _allGroups.ToArray ( ) )
                    if ( !db.Contains ( x ) )
                        _allGroups.Remove ( x );

                return _allGroups;

            }
        }

        public ObservableCollection<DM.Standard> Standards
        {
            get
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                //return _standards;

                if ( Event == null ) return _standards;

                //!*!
                DM.Standard[] db = Event.Standards;

                foreach ( var x in db )
                    if ( _standards.Where ( s => s == x ).Count ( ) == 0 )
                        _standards.Add ( x ) ;

                foreach ( var x in _standards.ToArray ( ) )
                    if ( db.Where ( s => s == x ).Count ( ) == 0 )
                        _standards.Remove ( x );

                return _standards;
            }
        }

        public ObservableCollection<EventVM> Heats
        {
            get
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                //return _heats;

                if ( Event == null ) return _heats;


                if ( Event is IFinalEvent )
                {
                    //!*!
                    IHeatEvent[] db = ((IFinalEvent)Event).getHeats().ToArray();

                    foreach ( var x in db )
                        if ( _heats.Where ( s => (AEvent)s.Event == (AEvent)x ).Count ( ) == 0 )
                            _heats.Add ( new EventVM ( (AEvent)x ) );

                    foreach ( var x in _heats.ToArray ( ) )
                        if ( db.Where ( s => (AEvent)s == (AEvent)x.Event ).Count ( ) == 0 )
                            _heats.Remove ( x );

                }

                return _heats;

            }
        }

        public Visibility hasHeatsVisibility
        {
            get
            {
                if ( Event is IFinalEvent )
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility IsHeat
        {
            get
            {
                if ( Event is IHeatEvent )
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public List<DM.ACustomDataValue> CustomData { get { return Event.CustomDataStore.ToList ( ); } }

        public Brush StateColour
        {
            get
            {
                switch (Event.State)
                {
                    case EventState.New:
                        return new SolidColorBrush (Color.FromArgb(128, 255, 255, 255)); // transparent white
                    case EventState.AthletesEntered:
                        return new SolidColorBrush(Color.FromArgb(128, 100, 100, 255)); // transparent blue tint
                    case EventState.ResultsEntered:
                        return new SolidColorBrush(Color.FromArgb(128, 255, 100, 100)); // transparent red tint
                    case EventState.CertificatesPrinted:
                        return new SolidColorBrush(Color.FromArgb(128, 100, 255, 100)); // transparent green tint
                    case EventState.SelectionComplete:
                        return new SolidColorBrush(Color.FromArgb(128, 100, 255, 255)); // transparent turquoise tint
                    case EventState.Locked:
                        return new SolidColorBrush(Color.FromArgb(128, 255, 255, 100)); // transparent yellow tint
                    default:
                        return new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)); // transparent white
                }
            }
        }

        public int CountEntered { get { return Event.countCompetitors(); } }

        public int CountResults { get { return Event.Results.Count(); } }

        public int CountSelected { get { return Event.countCurrentlySelected(); } }
        
        #endregion

        #region public methods

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        void SaveToDB ()
        {
            if (!GetFileDetails().isOpen) return;

            UpdateEvent ( );
            //SaveChanges();
        }

        bool CanSaveToDB()
        {
            if (!GetFileDetails().isOpen) return false;

            // Does the record need updating?
            if (hasChanges)
                return true;

            return false;
        }

        public ICommand Save { get { return new RelayCommand(SaveToDB, CanSaveToDB); } }

        public ICommand DeleteChampionship { get { return new RelayCommand(deleteFromDatabase, canDelete); } }

        private bool canDelete()
        {
            return Event.CanDelete();
        }

        private void deleteFromDatabase()
        {
            if (MessageBox.Show("Are you sure you want to delete " + Name + "? This cannot be undone.", "Are you sure",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if (context == null)
                    //throw new Exception("File error when deleting event");

                AEvent temp = this.Event;

                //context.Events.DeleteOnSubmit(temp);
                //SaveChanges();

                //!*!
                GetFileDetails ( ).IO.Delete<AEvent> ( temp );

                Event = null;

                Championship.UpdateEvents();
            }
        }

        public ICommand AddStandard {  get { return new RelayCommand ( addStandard , canModify ); } }

        public void addStandard()
        {
            DM.Standard standards = new DM.Standard();

            //GetFileDetails ( ).IO.Add<DM.Standard> ( standards );

            Event.addStandard ( standards );
            //SaveToDB ( );
            RaisePropertyChanged ( "Standards" );
        }

        public bool canModify()
        {
            if ( ! IsEntryForm() ) return Championship.canModify( );
            return true;
        }

        public bool canExecuteInFullOnly()
        {
            return !IsEntryForm( ) ;
        }

        public ICommand DeleteStandard {  get { return new RelayCommand ( deleteStandard , canModify ); } }

        public void deleteStandard(object obj)
        {
            if ( obj is DM.Standard std )
            {
                Event.removeStandard( std );

                //SaveToDB ( );

                RaisePropertyChanged( "Standards" );
            }
        }

        public ICommand ClearChampionshipBestPerformace { get { return new RelayCommand ( clearCBP , canModify ); } }

        private void clearCBP()
        {
            Event.CountyBestPerformanceName = null;
            Event.CountyBestPerformanceYear = 0;
            Event.CountyBestPerformanceArea = null;
            Event.CountyBestPerformance = null;
            RaisePropertyChanged ( "Event" );
        }

        public ICommand AddHeat { get { return new RelayCommand ( addHeat , canModify ); } }

        private void addHeat ( )
        {
            if ( Event is IFinalEvent )
            {
                IHeatEvent heat = ( (IFinalEvent)Event ).createHeat ( );

                //!*!
                //GetFileDetails ( ).IO.Add < AEvent > ( heat );

                //SaveToDB ( );
                RaisePropertyChanged ( "Heats" );
            }
        }

        public ICommand DeleteHeat { get { return new RelayCommand ( deleteHeat , canModify ); } }

        public void deleteHeat ( object obj )
        {
            if ( obj is EventVM )
            {
                if ( ((EventVM)obj).Event is DM.IHeatEvent )
                {
                    if ( Event is IFinalEvent )
                    {
                        IHeatEvent heat = (IHeatEvent) ((EventVM)obj).Event;

                        //2017-06-03
                        if (true)// ( heat.allCompetitorsinHeat ( ).Count == 0 )
                        {
                            try
                            {
                                ( (IFinalEvent) Event ).removeHeat ( heat );

                                //SaveToDB ( );
                                RaisePropertyChanged ( "Heats" );
                            }
                            catch ( Exception ex )
                            {
                                MessageBox.Show ( "Failed to remove heat \n" + ex.Message );
                            }
                        }
                        else
                        {
                            MessageBox.Show ( "You can not remove this heat as it already has competitors entered into it" );
                        }
                    }
                }
            }
        }

        public ICommand OpenResults { get { return new RelayCommand ( openResults, canExecuteInFullOnly ); } }

        private void openResults ()
        {
            ResultsWindow resultsWin = new ResultsWindow(Event);

            resultsWin.Show ( );
        }

        public ICommand OpenEntries { get { return new RelayCommand ( openEntries ); } }

        private void openEntries ( )
        {
            EntriesWindow entiesWin = new EntriesWindow(Event);

            entiesWin.Show ( );
        }

        public ICommand EditEvent { get { return new RelayCommand ( editEvent, canExecuteInFullOnly ); } }

        private void editEvent()
        {
            EditEvent ee = new EditEvent();
            ee.DataContext = this;

            ee.ShowDialog ( );

            SaveToDB ( );
        }

        public ICommand AddAthlete { get { return new RelayCommand ( addAthlete , canAddAthlete ); } }

        private bool canAddAthlete()
        {
            return canModify ( );
        }

        private void addAthlete (object obj)
        {
            if ( Event != null )
            {
                if ( obj is AthleteVM )
                {
                    try {

                        AthleteVM athlete = (AthleteVM)obj;
                        
                        if ( athlete.Team == null )
                        // Add to team
                        {
                            if ( athlete.Attends == null )
                                throw new Exception( "This athlete must be assigned to a school before they can be entered." );

                            if ( athlete.Attends.Teams.Count == 1 )
                                athlete.Athlete.setTeam( athlete.Attends.Teams.First( ).Team , Championship.Championship );
                            else
                                throw new Exception( "This athlete must be assigned to a team before they can be entered" );
                        }

                        Event.enterAthlete ( athlete.Athlete );

                        ( (AthleteVM)obj ).updateAvilableEvents ( );

                        RaisePropertyChanged ( "CountEntered" );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show ( ex.Message );
                    }
                    //SaveToDB ( );
                }
            }
        }

        public ICommand AddCustomData { get { return new RelayCommand ( addCustomData , canModify ); } }

        public void addCustomData ( )
        {
            DM.Dialogs.CustomDataDialogue cdd = new DM.Dialogs.CustomDataDialogue ();

            if ( cdd.ShowDialog ( ) == true )
            {
                ACustomDataValue value = cdd.Value;

                Event.addCustomDataValue ( value );

                //SaveToDB ( );
                RaisePropertyChanged ( "CustomData" );
            }
        }

        public ICommand DeleteCustomData { get { return new RelayCommand ( deleteCustomData , canModify ); } }

        public void deleteCustomData ( object obj )
        {
            if ( obj is ACustomDataValue value )
            {
                Event.deleteField( value.key );

                RaisePropertyChanged( "CustomData" );
            }
        }

        public ICommand EditCustomData { get { return new RelayCommand ( editCustomData , canModify ); } }

        public void editCustomData ( object obj )
        {
            if ( obj is ACustomDataValue value )
            {
                DM.Dialogs.CustomDataDialogue cdd = new DM.Dialogs.CustomDataDialogue ( value );

                if ( cdd.ShowDialog( ) == true )
                {
                    //!*!
                    GetFileDetails( ).IO.Update<ACustomDataValue>( value );
                    //SaveToDB ( );
                    RaisePropertyChanged( "CustomData" );
                }
            }
        }

        public void DragOver ( IDropInfo dropInfo )
        {
            if ( dropInfo.Data != null )
            {
                if ( dropInfo.Data is AthleteVM )
                {
                    //dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
            //ExampleItemViewModel sourceItem = dropInfo.Data as ExampleItemViewModel;
            //ExampleItemViewModel targetItem = dropInfo.TargetItem as ExampleItemViewModel;

            //if ( sourceItem != null && targetItem != null && targetItem.CanAcceptChildren )
            //{
                //dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                //dropInfo.Effects = DragDropEffects.Copy;
            //}
        }

        public void Drop ( IDropInfo dropInfo )
        {
            if ( dropInfo.Data != null )
            {
                if ( dropInfo.Data is AthleteVM )
                {
                    this.addAthlete ( dropInfo.Data );
                }
            }
        }

        #endregion

        public override string ToString ( )
        {
            return Name;
        }

    }
}
