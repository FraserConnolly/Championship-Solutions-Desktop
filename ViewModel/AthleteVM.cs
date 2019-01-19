using ChampionshipSolutions.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using static ChampionshipSolutions.FileIO.FConnFile;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using ChampionshipSolutions.ControlRoom;

namespace ChampionshipSolutions.ViewModel
{
    public class AthleteVM : PersonVM
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a Team View Model
        /// </summary>
        public AthleteVM() : base()
        {
        }

        public AthleteVM(Athlete x) : base()
        {
            if (x is Athlete)
                base.Person = x;
            else
                throw new ArgumentException("Athlete must be of type Athlete");
        }

        #endregion

        #region Members

        #endregion

        #region Properties

        public Athlete Athlete { get { if (Person == null) return null; if (Person is Athlete) return (Athlete)Person; else return null; } }

        public ChampionshipVM Championship { get { return ((App)App.Current).CurrentChampionship; } }

        public bool HasSchool
        {
            get
            {
                if (Athlete != null)
                    return (Athlete.Attends != null);

                return false;
            }
        }

        public SchoolVM Attends
        {
            get
            {
                if (Athlete == null) return null;

                if (Championship == null) return null;

                if (Athlete.Attends == null) return null;

                return new SchoolVM(Athlete.Attends, Championship);
            }
            set
            {
                if ( value == null )
                {
                    //Athlete.Attends = null;
                    //SaveToDB ( );
                    //RaisePropertyChanged ( "Attends" );
                }
                else if ( value.School != Athlete.Attends )
                {
                    if ( value.School == null ) // this bad data
                        return;
                    if ( value.School.Name == "No School" ) // this has come from the drop down box via the converter.
                        Athlete.Attends = null;
                    else
                        Athlete.Attends = value.School;
                    SaveToDB ( );
                    RaisePropertyChanged ( "Attends" );
                }
            }
        }

        public TeamVM Team
        {
            get
            {
                if (Athlete == null) return null;

                if (Championship == null) return null;

                DM.Team t = Athlete.getTeam(Championship.Championship);

                if (t == null) return null;

                return new TeamVM(t, Championship);
            }
            set
            {
                TeamVM CurrentTeam = Team;

                if (CurrentTeam == value) return;

                try
                {
                    Team t = null;

                    if (value != null)
                        t = value.Team;

                    School school = Athlete.Attends ;

                   Athlete.setTeam(t, Championship.Championship);

                    //if ( ATC != null )
                    //    SQLCommands.ChampionshipAthleteTeamSQLCommands.DeleteAthleteTeamChamptionships ( Championship.Championship.ID , Athlete.ID  , GetFileDetails().Connection );

                    SaveToDB();
                    RaisePropertyChanged("Team");
                    RaisePropertyChanged("Attends");
                    RaisePropertyChanged("AllAvailableSchools");
                    RaisePropertyChanged("AvailableEvents");

                    if ( t.HasSchools.Contains(school))
                        Athlete.Attends = school;
                    SaveToDB( );
                    RaisePropertyChanged( "Attends" );

                }
                catch (Exception ex)
                {
                    Team = CurrentTeam;
                    RaisePropertyChanged("Team"); // Force team back to its previous state after an error
                    MessageBox.Show(ex.Message);
                }

            }
        }

        public ObservableCollection<SchoolVM> AllAvailableSchools
        {
            get
            {
                //if (Team == null) return new ObservableCollection<SchoolVM>();
                if (Team == null) return Championship.Schools;

                return Team.Schools;
            }
        }

        public List<EventVM> AvailableEvents
        {
            get
            {
                List<EventVM> Events = new List<EventVM>();

                foreach (AEvent Event in Championship.Championship.listAllAvailableEvents(Athlete))
                   Events.Add(new EventVM(Event));
                return Events;
            }
        }

        public List<ACompetitor> EnteredEvents
        {
            get
            {
                return Athlete.GetCompetitors(Championship.Championship ).ToList<ACompetitor>();
            }
        }

        public List<AResult> Results
        {
            get
            {
                return Athlete.getAllResults();
            }
        }

        public string GlobalAthleteID
        {
        get
            {
                if ( Athlete.GlobalAthleteID == null )
                    return "";
                else
                    return Athlete.GlobalAthleteID.Value.ToString ( );
            }
            set
            {

                if ( value == null )
                {
                    Athlete.GlobalAthleteID = null;
                }
                else if ( int.TryParse( value , out int ivalue ) )
                {
                    Athlete.GlobalAthleteID = ivalue;
                }
                else
                {
                    Athlete.GlobalAthleteID = null;
                    throw new Exception( "Can not convert this string to a number" );
                }
            }
        }


        #endregion

        #region Private methods

        private bool isInChampionship()
        {
            if (Championship == null) return false;

            if (Athlete == null) return false;

            return Athlete.inChampionship(Championship.Championship);
        }

        internal void updateAvilableEvents()
        {
            RaisePropertyChanged("AvailableEvents");
            RaisePropertyChanged("EnteredEvents");
        }

        #endregion

        #region Commands

        public ICommand AddSpecialConsideration { get { return new RelayCommand(addSpecialConsideration, canAddSpecialConsideration); } }

        private bool canAddSpecialConsideration()
        {
            if (Championship.canModify() == false) return false;

            foreach (AEvent Event in Championship.Championship.Events)
                if (Event.isAvailable(Athlete))
                    return true;

            return false;
        }

        private void addSpecialConsideration()
        {

            List<AEvent> events = new List<AEvent>();

            foreach (AEvent Event in Championship.Championship.listAllEvents())
                if (Event.isAvailable(Athlete))
                    events.Add(Event);

            SelectEvent se = new SelectEvent(events);

            if (se.ShowDialog() == true)
            {
                AEvent selEvent = se.SelectedEvent;
                SpecialConsideration sc = new SpecialConsideration();

                sc.Athlete = Athlete;
                sc.CompetingIn = selEvent;
                sc.PersonalBest = AEvent.MakeNewResultsValue(selEvent);

                selEvent.EnterSpecialConsideration(sc);

                SaveToDB();
                RaisePropertyChanged("EnteredEvents");
            }
        }
        
        public ICommand RemoveCompetitor { get { return new RelayCommand ( removeCompetitor , canModify ); } }

        private bool canModify()
        {
            if ( IsEntryForm( ) ) return true;
            return Championship.canModify ( );
        }

        private void removeCompetitor ( object obj )
        {
            if ( obj is ACompetitor )
            {
                try
                {
                    ACompetitor comp = (ACompetitor)obj;

                    comp.CompetingIn.removeCompetitor ( comp );
                    RaisePropertyChanged ( "EnteredEvents" );
                    updateAvilableEvents( );
                }
                catch ( Exception ex )
                {
                    MessageBox.Show ( "Failed to remove this athlete from this event \n" + ex.Message );
                }
            }
        }

        public ICommand OpenPoT { get { return new RelayCommand ( openPoT ); } }

        private void openPoT ( )
        {
            const string PowerOfTenWebAddress = "https://www.thepowerof10.info";

            if ( Athlete.GlobalAthleteID == null )
            {
                System.Diagnostics.Process.Start(
                   $"{PowerOfTenWebAddress}/athletes/athleteslookup.aspx?surname={Athlete.LastName}&firstname={Athlete.FirstName}&club=" );
            }
            else if ( Athlete.GlobalAthleteID == 0 )
            {
                System.Diagnostics.Process.Start(
                   $"{PowerOfTenWebAddress}/athletes/athleteslookup.aspx?surname={Athlete.LastName}&firstname={Athlete.FirstName }&club=" );
            }
            else
            {
                System.Diagnostics.Process.Start(
                     $"{PowerOfTenWebAddress}/athletes/profile.aspx?athleteid={Athlete.GlobalAthleteID}" );
            }
        }

        public ICommand OpenAchievements { get => new RelayCommand( OpenCerts ); }

        private void OpenCerts ( )
        {
            
            string[] certs = Athlete.GenerateCertificates(  );

            if ( certs.Length == 0 )
            {
                MessageBox.Show( $"There are no certificates for {Athlete.PreferredName}" );
            }
            else if ( certs.Length == 1 )
            {
                PDFViewer.OpenOnSTAThread( Championship.getChampionshipExportsDir( ) + "\\" + certs [ 0 ].TrimStart( "exports\\".ToCharArray( ) ) );
            }
            else
            {
                for ( int i = 0 ; i < certs.Length ; i++ )
                {
                    certs [ i ] = Championship.getChampionshipExportsDir( ) + "\\" + certs [ i ].TrimStart( "exports\\".ToCharArray( ) );
                }

                string FilePath = $"{Championship.getChampionshipExportsDir( )}\\{Athlete.Fullname} - {Athlete.ID}.pdf";

                Exports.MergeMultiplePDFIntoSinglePDF( FilePath , certs );

                if ( System.IO.File.Exists( FilePath ) )
                {
                    PDFViewer.OpenOnSTAThread( FilePath );
                }
                else
                {
                    MessageBox.Show( "Error whilst generating certificates" );
                }
            }
        }

        #endregion

    }


}

