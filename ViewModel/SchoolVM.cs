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
    public class SchoolVM : ObservableObject
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a Team View Model
        /// </summary>
        public SchoolVM( )
        {
        }

        public SchoolVM( School School, ChampionshipVM Championship )
        {
            _school = School;
            _championship = Championship;
        }

        #endregion

        #region Members

        School _school;
        ChampionshipVM _championship;
        ObservableCollection<TeamVM> _teams = new ObservableCollection<TeamVM>();
        ObservableCollection<TeamVM> _allTeams = new ObservableCollection<TeamVM>();
        ObservableCollection<PersonVM> _staff = new ObservableCollection<PersonVM>();
        bool _showTeams;
        #endregion

        private void saveSchool()
        {
            GetFileDetails ( ).IO.Update<School> ( School );
            clearHasChanges ( );
        }

        #region Properties

        public School School
        {
            get { return _school; }
            set
            {
                if (value == null)
                    // reject - I'm not sure why this is happening when Championships is being updated??
                    return;

                if (School != value)
                {
                    //!!SaveToDB();
                    _school = value;
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
            get { return School.Name; }
            set
            {
                if (School.Name != value)
                {
                    School.Name = value;
                    saveSchool ( );
                    RaisePropertyChanged ( "Name" );
                    RaisePropertyChanged ( "ShortName" );
                }
            }
        }

        public string ShortName
        {
            get { return School.ShortName; }
            set
            {
                if (School.ShortName != value)
                {
                    School.ShortName = value;
                    saveSchool ( );
                    RaisePropertyChanged("ShortName");
                }
            }
        }

        public ObservableCollection<TeamVM> Teams
        {
            get
            {

                //CSDB context = FileIO.FConnFile.getContext();

                //if (context == null)
                    //return _teams;

                Team[] db = School.TeamsForChampionship(Championship.Championship);

                foreach (var x in db)
                    if (_teams.Where(t => t.Team == x).Count() == 0)
                        _teams.Add(new TeamVM(x, Championship, School));

                foreach (var x in _teams.ToArray())
                    if (!db.Contains(x.Team))
                        _teams.Remove(x);

                return _teams;
            }
        }

        public ObservableCollection<TeamVM> AllTeams
        {
            get
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //return _allTeams;

                TeamVM[] db = Championship.Teams.ToArray();

                foreach ( var x in db )
                    if ( _allTeams.Where ( t => t.Team == x.Team ).Count ( ) == 0 )
                        _allTeams.Add ( new TeamVM ( x.Team , Championship, School ) );

                foreach ( var x in _allTeams.ToArray ( ) )
                    if ( !db.Contains ( x ) )
                        _allTeams.Remove ( x );

                return _allTeams;
            }
        }

        //public ObservableCollection<PersonVM> Staff
        //{
        //    get
        //    {
        //        if ( School == null ) return _staff;

        //        Staff[] db = School.Staff;

        //        foreach ( var x in db )
        //            if ( _staff.Where ( s => s.Person == x.Person ).Count ( ) == 0 )
        //                _staff.Add ( new PersonVM ( ) { Person = x.Person } );

        //        foreach ( var x in _staff.ToArray ( ) )
        //            if ( db.Where ( s => s.Person == x.Person ).Count () > 0 )
        //                _staff.Remove ( x );

        //        return _staff;
        //    }
        //}

        //public PersonVM Head
        //{
        //    get
        //    {
        //        if ( School == null ) return null;

        //        return new PersonVM ( ) { Person = School.Head };
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public Visibility ShowTeams { get { return Visibility.Visible ; } }
        public Visibility ShowTeams { get { return _showTeams ? Visibility.Visible : Visibility.Collapsed; } }

        #endregion

        #region Commands

        private bool canModify ( )
        {
            if ( ( (App)App.Current ).CurrentChampionship == null ) return false;

            return ( (App)App.Current ).CurrentChampionship.canModify ( );
        }

        public ICommand Save { get { return new RelayCommand ( saveSchool , CanSaveToDB ); } }

        //void SaveToDB ( )
        //{
        //    if ( !GetFileDetails ( ).isOpen ) return;

        //    SaveChanges ( );
        //    clearHasChanges ( );
        //}

        bool CanSaveToDB ( )
        {
            if ( !GetFileDetails ( ).isOpen ) return false;

            // Does the record need updating?
            if ( hasChanges ) return true;

            return false;
        }


        public ICommand DeleteSchool { get { return new RelayCommand ( deleteFromDatabase , canDelete ); } }

        private bool canDelete( )
        {
            if ( Championship.canModify( ) )
                return School.CanDelete( );
            return false;
        }
        public bool CanModify { get { return Championship.canModify( ); } }


        private void deleteFromDatabase ( )
        {
            if ( MessageBox.Show ( "Are you sure you want to delete " + Name + "? This cannot be undone." , "Are you sure" ,
                MessageBoxButton.YesNo , MessageBoxImage.Exclamation , MessageBoxResult.No ) == MessageBoxResult.Yes )
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //throw new Exception ( "File error when deleting team" );

                School temp = School;

                //foreach ( TeamVM t in Teams )
                //{

                //    //SQLCommands.SchoolTeamsSQLCommands.DeleteSchoolTeam ( st , getContext ( ) );
                //}

                GetFileDetails ( ).IO.Delete<School>( temp );

                //foreach ( Team t in Championship.Championship.Teams )
                    //t.RemoveSchool ( temp );

                //context.Schools.DeleteOnSubmit ( temp );
                //SaveChanges ( );
                //saveSchool ( );
                Championship.updateTeams ( );
                Championship.updateSchools ( );
            }
        }

        public ICommand ToggleShowTeams {  get { return new RelayCommand(toggleShowTeams); } }

        private void toggleShowTeams( ) { _showTeams = ! _showTeams; RaisePropertyChanged("ShowTeams");  }

        public void UpdateAllTeams ( )
        {
            RaisePropertyChanged ( "Teams" );
            RaisePropertyChanged ( "AllTeams" );
        }

        #region Staff

        //public ICommand SetNewHead { get { return new RelayCommand ( setNewHead , canModify ); } }
        //public ICommand ClearHead { get { return new RelayCommand ( clearHead , canModify ); } }

        //public ICommand AddStaff { get { return new RelayCommand ( addStaff , canModify ); } }
        //public ICommand EditStaff { get { return new RelayCommand ( editStaff , canModify ); } }
        //public ICommand RemoveStaff { get { return new RelayCommand ( removeStaff , canModify ); } }
        //public ICommand SelectFromExistingStaff { get { return new RelayCommand ( selectFromExistingStaff , canModify ); } }



        #endregion

        #endregion

        public override string ToString ( )
        {
            return Name;
        }

        public override bool Equals ( object obj )
        {
            // If parameter is null return false.
            if ( obj == null )
                return false;

            // If parameter cannot be cast to Point return false.
            SchoolVM v = obj as SchoolVM;
            if ( (System.Object)v == null )
                return false;

            // Return true if the fields match:
            return ( School.ID == v.School.ID );
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

    }
}
