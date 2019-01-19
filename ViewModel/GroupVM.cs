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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using TAlex.WPF.Controls;

namespace ChampionshipSolutions.ViewModel
{
    public class GroupVM : ObservableObject
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a Team View Model
        /// </summary>
        public GroupVM ( )
        {
        }

        public GroupVM ( Group Group, ChampionshipVM Championship)
        {
            _group = Group;
            _championship = Championship;
        }

        public GroupVM ( Group Group, ChampionshipVM Championship , AEvent Event) : this (Group, Championship)
        {
            _event = Event;
        }


        #endregion

        #region Members

        Group _group;
        ChampionshipVM _championship;
        AEvent _event; // used for comparison purposes - may be a bad idea.

        #endregion

        #region Properties

        public Group Group
        {
            get { return _group; }
            set
            {
                if ( value == null )
                    // reject - I'm not sure why this is happening when Championships is being updated??
                    return;

                if ( Group != value )
                {
                    //SaveToDB ( );
                    _group = value;
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
            get { return Group.Name; }
            set
            {
                if ( Group.Name != value )
                {
                    Group.Name = value;
                    SaveToDB ( );
                    RaisePropertyChanged ( "Name" );
                    RaisePropertyChanged ( "ShortName" );
                }
            }
        }

        public string ShortName
        {
            get { return Group.ShortName; }
            set
            {
                if ( Group.ShortName != value )
                {
                    Group.ShortName = value;
                    SaveToDB ( );
                    RaisePropertyChanged ( "ShortName" );
                }
            }
        }

        public bool? HasEvent
        {
            get
            {
                if (_event == null) return null;

                return (_event.hasGroup(_group));
            }
            set
            {
                if ( _event == null) return;

                switch (value)
                {
                    case false:
                        _event.clearGroup ( Group );
                        //SchoolTeams st = this.Team.RemoveSchool(_school);

                        //getContext ( ).SchoolTeams.DeleteOnSubmit ( st );
                        //SQLCommands.SchoolTeamsSQLCommands.DeleteSchoolTeam ( st , getContext ( ) );


                        RaisePropertyChanged ( "Event" );
                        break;
                    case true :
                        this._event.addGroup ( Group );
                        //this.Team.AddSchool(_school);
                        SaveToDB ( );
                        RaisePropertyChanged( "Event" );
                        break;
                    default:
                        break;
                }
            }
        }

        public bool CanModify { get { return Championship.canModify( ); } }

        public bool? HasGroup
        {
            get
            {
                if ( _event == null ) return null;

                return ( _event.getGroups().Contains ( Group ));
            }
            set
            {
                if ( _event == null ) return;

                switch ( value )
                {
                    case false:
                        EventGroups eg = _event.clearGroup ( Group );
                        break;
                    case true:
                        _event.addGroup ( Group );
                        break;
                    default:
                        break;
                }

                SaveToDB ( );

                ( (App)App.Current ).EventPage.ReloadPage ( );

            }
        }

        #endregion

        #region Commands

        public ICommand Save { get { return new RelayCommand ( SaveToDB , CanSaveToDB ); } }

        void SaveToDB ( )
        {
            if ( !GetFileDetails ( ).isOpen ) return;

            GetFileDetails ( ).IO.Update<Group> ( Group );
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


        public ICommand Delete { get { return new RelayCommand ( deleteFromDatabase , canDelete ); } }

        private bool canDelete ( ) { return Group.CanDelete() ; }

        private void deleteFromDatabase ( )
        {
            if ( MessageBox.Show ( "Are you sure you want to delete " + Name + "? This cannot be undone." , "Are you sure" ,
                MessageBoxButton.YesNo , MessageBoxImage.Exclamation , MessageBoxResult.No ) == MessageBoxResult.Yes )
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //throw new Exception ( "File error when deleting group" );

                Group temp = Group;

                //!*!
                Championship.Championship.RemoveGroup ( temp );
                //SaveToDB ( );
                Championship.updateGroups ( );
                
            }
        }

        public ICommand Edit { get { return new RelayCommand ( edit , canEdit ); } }

        private bool canEdit ( )
        {
            if ( ! Championship.canModify ( ) ) return false;
            if ( Group is AgeRestriction ) return true;
            if ( Group is DoBRestriction ) return true;
            if ( Group is GenderRestriction ) return true;
            return false;
        }

        private void edit ( )
        {
            Window win = new Window ( );
            win.Height = 300;
            win.Width  = 300;
            //win.WindowStyle = WindowStyle.SingleBorderWindow;
            win.ResizeMode = ResizeMode.NoResize;
            win.DataContext = this;

            Grid grid = new Grid();

            grid.Children.Add ( 
                new Button ( ) {
                    IsDefault =true,
                    IsCancel =true,
                    Margin = new Thickness ( 0 , 0, 10 , 10 ) ,
                    Height = 25 ,
                    Width = 100 ,
                    HorizontalAlignment = HorizontalAlignment.Right ,
                    VerticalAlignment = VerticalAlignment.Bottom ,
                    Content = "Done" ,
                    Command = FinishEditing ,
                    CommandParameter = win } );

            StackPanel stack = new StackPanel ( ) { Height=355, Margin = new Thickness(10,10,10,35) };
            //stack.Background = System.Windows.Media.Brushes.Beige;
            grid.Children.Add ( stack );
            win.Content = grid;

            if ( Group is AgeRestriction )
            {
                win.Title = "Age Restriction Editor";

                win.Width = 270;

                NumericUpDown maxAge = new NumericUpDown () { Margin= new Thickness(0,3,0,3), Maximum=999, Minimum= 0, FontSize = 15 };

                Binding maxBinding = new Binding();
                maxBinding.Source = Group;
                maxBinding.Path = new PropertyPath ( "maxAge" );
                maxBinding.Mode = BindingMode.TwoWay;
                maxBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding ( maxAge , NumericUpDown.ValueProperty , maxBinding );

                NumericUpDown minAge = new NumericUpDown () { Margin= new Thickness(0,3,0,3), Maximum=999, Minimum= 0, FontSize = 15 };

                Binding minBinding = new Binding();
                minBinding.Source = Group;
                minBinding.Path = new PropertyPath ( "minAge" );
                minBinding.Mode = BindingMode.TwoWay;
                minBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding ( minAge , NumericUpDown.ValueProperty , minBinding );

                DatePicker date = new DatePicker() { Margin= new Thickness(0,3,0,3), FontSize=15 } ;

                Binding dateBinding = new Binding();
                dateBinding.Source = Group;
                dateBinding.Path = new PropertyPath ( "dateReference" );
                dateBinding.Mode = BindingMode.TwoWay;
                dateBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding ( date , DatePicker.SelectedDateProperty , dateBinding );

                stack.Children.Add ( new Label ( ) { Margin = new Thickness ( 0 , 6 , 0 , 3 ) , Content = "Date Reference"} );
                stack.Children.Add ( date );

                stack.Children.Add ( new Label ( ) { Margin = new Thickness ( 0 , 6 , 0 , 3 ) , Content = "Minimum Age (Years)" } );
                stack.Children.Add ( minAge );

                stack.Children.Add ( new Label ( ) { Margin = new Thickness ( 0 , 6 , 0 , 3 ) , Content = "Maximum Age (Years)"} );
                stack.Children.Add ( maxAge );

                win.ShowDialog ( );

                return;
            }

            if ( Group is DoBRestriction )
            {
                // not yet tested!
                win.Title = "Date Of Birth Restriction Editor";
                win.Width = 270;

                DatePicker startDate = new DatePicker() { Margin= new Thickness(0,3,0,3), FontSize=15 } ;

                Binding startDateBinding = new Binding();
                startDateBinding.Source = Group;
                startDateBinding.Path = new PropertyPath ( "StartDate" );
                startDateBinding.Mode = BindingMode.TwoWay;
                startDateBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding ( startDate , DatePicker.SelectedDateProperty , startDateBinding );

                DatePicker endDate = new DatePicker() { Margin= new Thickness(0,3,0,3), FontSize=15 } ;

                Binding endDateBinding = new Binding();
                endDateBinding.Source = Group;
                endDateBinding.Path = new PropertyPath ( "EndDate" );
                endDateBinding.Mode = BindingMode.TwoWay;
                endDateBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding ( endDate , DatePicker.SelectedDateProperty , endDateBinding );

                stack.Children.Add ( new Label ( ) { Margin = new Thickness ( 0 , 6 , 0 , 3 ) , Content = "Start of Date of Birth Range" } );
                stack.Children.Add ( startDate );

                stack.Children.Add ( new Label ( ) { Margin = new Thickness ( 0 , 6 , 0 , 3 ) , Content = "End of Date of Birth Range" } );
                stack.Children.Add ( endDate );


                win.ShowDialog ( );

                return;
            }

            if ( Group is GenderRestriction )
            {
                win.Title = "Gender Restriction Editor";
                win.Height = 130;
                win.Width = 315;


                CheckBox male = new CheckBox () { Margin= new Thickness(0,3,0,3), Content="Male", FontSize = 15 };

                Binding maleBinding = new Binding();
                maleBinding.Source = Group;
                maleBinding.Path = new PropertyPath ( "Male" );
                maleBinding.Mode = BindingMode.TwoWay;
                maleBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding ( male , CheckBox.IsCheckedProperty, maleBinding );

                CheckBox female = new CheckBox () { Margin= new Thickness(0,3,0,3), Content="Female" , FontSize = 15 };

                Binding femaleBinding = new Binding();
                femaleBinding.Source = Group;
                femaleBinding.Path = new PropertyPath ( "Female" );
                femaleBinding.Mode = BindingMode.TwoWay;
                femaleBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding ( female  , CheckBox.IsCheckedProperty , femaleBinding );

                stack.Children.Add ( male );
                stack.Children.Add ( female );

                win.ShowDialog ( );

                return;
            }
        }

        public ICommand FinishEditing { get { return new RelayCommand ( finishEditing , canFinishEdit ); } }

        private bool canFinishEdit()
        {
            return true;
            // to do
        }

        private void finishEditing(object obj)
        {
            Window temp = (Window) obj;

            temp.Close ( );

            Group.Save ( );

            //SaveToDB ( );
        }

        #endregion

        public override bool Equals ( object obj )
        {
            // If parameter is null return false.
            if ( obj == null )
                return false;

            // If parameter cannot be cast to Point return false.
            GroupVM v = obj as GroupVM;
            if ( (System.Object)v == null )
                return false;

            // Return true if the fields match:
            return ( Group.ID == v.Group.ID && Championship.Championship.ID == v.Championship.Championship.ID );
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public override string ToString ( )
        {
            return this.Name;
        }

    }
}
