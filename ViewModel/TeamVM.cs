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

namespace ChampionshipSolutions.ViewModel
{
    public class TeamVM : ObservableObject
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a Team View Model
        /// </summary>
        public TeamVM ( )
        {
        }

        public TeamVM ( Team Team , ChampionshipVM Championship )
        {
            _team = Team;
            _championship = Championship;
        }

        public TeamVM ( Team Team , ChampionshipVM Championship , School School ) : this ( Team , Championship )
        {
            _school = School;
        }


        #endregion

        #region Members

        Team _team;
        ChampionshipVM _championship;
        School _school; // used for comparison purposes - may be a bad idea.
        ObservableCollection<SchoolVM> _schools = new ObservableCollection<SchoolVM>();

        #endregion

        #region Properties

        public Team Team
        {
            get { return _team; }
            set
            {
                if ( value == null )
                    // reject - I'm not sure why this is happening when Championships is being updated??
                    return;

                if ( Team != value )
                {
                    SaveToDB ( );
                    _team = value;
                }
            }
        }

        public ChampionshipVM Championship
        {
            get
            {
                return _championship;
            }
        }

        public string Name
        {
            get { return Team.Name; }
            set
            {
                if ( Team.Name != value )
                {
                    Team.Name = value;
                    Team.Save ( );
                    RaisePropertyChanged ( "Name" );
                    RaisePropertyChanged ( "ShortName" );
                }
            }
        }

        public string ShortName
        {
            get { return Team.ShortName; }
            set
            {
                if ( Team.ShortName != value )
                {
                    Team.ShortName = value;
                    Team.Save ( );
                    RaisePropertyChanged ( "ShortName" );
                }
            }
        }

        public bool? HasSchool
        {
            get
            {
                if ( _school == null ) return null;

                return ( Team.HasSchools.Contains ( _school ) );
            }
            set
            {
                if ( _school == null ) return;

                switch ( value )
                {
                    case false:
                        this.Team.RemoveSchool(_school, GetFileDetails().Connection);
                        //getContext ( ).SchoolTeams.DeleteOnSubmit ( st );
                        //SQLCommands.SchoolTeamsSQLCommands.DeleteSchoolTeam ( st , getContext ( ) );
                        RaisePropertyChanged ( "Team" );
                        RaisePropertyChanged ( "Schools" );
                        foreach ( var s in Championship.Schools.Where( s => s.Name == _school.Name ) )
                            s.UpdateAllTeams( );
                        break;
                    case true:
                        this.Team.AddSchool ( _school , GetFileDetails().IO );
                        RaisePropertyChanged ( "Schools" );
                        RaisePropertyChanged( "Team" );
                        foreach ( var s in Championship.Schools.Where ( s => s.Name == _school.Name ) )
                            s.UpdateAllTeams( );
                        break;
                    default:
                        break;
                }
            }
        }

        public ObservableCollection<SchoolVM> Schools
        {
            get
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //return _schools;

                School[] db = Team.HasSchools.ToArray();

                foreach ( var x in db )
                    if ( _schools.Where ( t => t.School == x ).Count ( ) == 0 )
                        _schools.Add ( new SchoolVM ( x , Championship ) );

                foreach ( var x in _schools.ToArray ( ) )
                    if ( !db.Contains ( x.School ) )
                        _schools.Remove ( x );

                return _schools;
            }
        }

        #endregion

        #region Commands
        public ICommand NewEntryForms { get { return new RelayCommand( newEntryForms , canMakeEntryForm ); } }

        public bool canMakeEntryForm( )
        {
            try
            {
                if ( IsEntryForm( ) ) return false;
                return !(GetFileDetails( ).IO.GetAll<Competitor>( ).Count > 0);
            }
            catch
            {
                return false;
            }
        }

        public void newEntryForms( )
        {
            if ( GetFileDetails( ).IO.GetAll<Competitor>( ).Count > 0 )
                MessageBox.Show( "Can not generate entry from as this championship has athletes entered into events." );
            else
                CreateEntryForms( Championship , new TeamVM[] { this } );
        }

        public ICommand Save { get { return new RelayCommand ( SaveToDB , CanSaveToDB ); } }

        void SaveToDB ( )
        {
            if ( !GetFileDetails ( ).isOpen ) return;

            //!*!
            GetFileDetails ( ).IO.Update<Team> ( Team );

            //SaveChanges ( );
            clearHasChanges ( );
        }

        bool CanSaveToDB ( )
        {
            if ( !GetFileDetails ( ).isOpen ) return false;

            // Does the record need updating?
            if ( hasChanges ) return true;

            return false;
        }


        public ICommand DeleteTeam { get { return new RelayCommand ( deleteFromDatabase , canDelete ); } }


        private bool canDelete ( )
        {
            if ( Championship.canModify() )
                return Team.CanDelete ( );
            return false;
        }
        public bool CanModify { get { return Championship.canModify( ); } }

        private void deleteFromDatabase ( )
        {
            if ( MessageBox.Show ( "Are you sure you want to delete " + Name + "? This cannot be undone." , "Are you sure" ,
                MessageBoxButton.YesNo , MessageBoxImage.Exclamation , MessageBoxResult.No ) == MessageBoxResult.Yes )
            {
                if (!Team.CanDelete())
                {
                    MessageBox.Show ( "This Team can not be deleted as there is at least one athlete entered into it." );
                    return;
                }
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //throw new Exception ( "File error when deleting team" );

                Team temp = Team;

                Championship.Championship.RemoveTeam ( temp );
                //SaveChanges ( );
                Championship.updateTeams ( );
            }
        }

        #endregion

        public override bool Equals ( object obj )
        {
            // If parameter is null return false.
            if ( obj == null )
                return false;

            // If parameter cannot be cast to Point return false.
            TeamVM v = obj as TeamVM;
            if ( (System.Object)v == null )
                return false;

            if ( v.Team == null ) return false;

            if ( v.Championship == null ) return false;

            // Return true if the fields match:
            return ( Team.ID == v.Team.ID && Championship.Championship.ID == v.Championship.Championship.ID );
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public override string ToString ( )
        {
            return this.Name;
        }

        public static bool operator == ( TeamVM a , TeamVM b )
        {
            // If both are null, or both are same instance, return true.
            if ( System.Object.ReferenceEquals ( a , b ) )
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ( ( (object)a == null ) || ( (object)b == null ) )
            {
                return false;
            }

            return a.Equals ( b );
        }

        public static bool operator != ( TeamVM a , TeamVM b )
        {
            return !( a == b );
        }
    }
}
