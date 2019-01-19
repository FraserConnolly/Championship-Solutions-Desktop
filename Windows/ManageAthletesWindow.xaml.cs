using ChampionshipSolutions.DM;
using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Linq;
using ChampionshipSolutions.ViewModel;
using System.ComponentModel;

namespace ChampionshipSolutions
{

    /// <summary>
    /// Interaction logic for ManageAthletesWindow.xaml
    /// </summary>
    public partial class ManageAthletesWindow : Window
    {
        public ManageAthletesWindow ( )
        {
            InitializeComponent( );

            aAthleteViewSource = ((System.Windows.Data.CollectionViewSource) (this.FindResource( "aAthleteViewSource" )));

            aAthleteViewSource.Source = ((ChampionshipSolutions.ViewModel.ChampionshipVM) this.DataContext).Athletes;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(aAthleteListView.ItemsSource);
            view.Filter = athleteFilter;
        }

        private void TextBox_TextChanged ( object sender , TextChangedEventArgs e )
        {
            AthleteFilter = ((TextBox) sender).Text;
        }

        System.Windows.Data.CollectionViewSource aAthleteViewSource;
        string _athleteFilterString;

        public string AthleteFilter
        {
            get
            {
                return _athleteFilterString;
            }
            set
            {
                _athleteFilterString = value;
                // aAthleteViewSource.Refresh();
                CollectionViewSource.GetDefaultView( aAthleteListView.ItemsSource ).Refresh( );
            }
        }

        private bool athleteFilter ( object item )
        {
            if ( _athleteFilterString == null ) return true;
            AthleteVM athlete = item as AthleteVM;

            // Is case sensitive
            //return athlete.Athlete.Fullname.Contains(_athleteFilterString);

            // is case insensitive
            return athlete.Athlete.Fullname.IndexOf( _athleteFilterString , StringComparison.OrdinalIgnoreCase ) >= 0;
        }

        private void Button_Click ( object sender , RoutedEventArgs e )
        {
            this.Close( );
        }

        // from http://stackoverflow.com/questions/30787068/wpf-listview-sorting-on-column-click
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        void GridViewColumnHeaderClickedHandler ( object sender , RoutedEventArgs e )
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if ( headerClicked != null )
            {
                if ( headerClicked.Role != GridViewColumnHeaderRole.Padding )
                {
                    if ( headerClicked != _lastHeaderClicked )
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if ( _lastDirection == ListSortDirection.Ascending )
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    Sort( header , direction );

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort ( string sortBy , ListSortDirection direction )
        {
            if      ( sortBy == "School Year" )     sortBy = "YearGroup";
            else if ( sortBy == "Date Of Birth" )   sortBy = "DateOfBirth";
            else if ( sortBy == "Gender" )          sortBy = "Gender";
            else if ( sortBy == "Name" )            sortBy = "PreferredName";
            else return;

            ICollectionView dataView = CollectionViewSource.GetDefaultView(aAthleteListView.ItemsSource);

            dataView.SortDescriptions.Clear( );
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add( sd );
            dataView.Refresh( );
        }
    }
}
        
//    /// <summary>
//        /// Interaction logic for ManageAthletesWindow.xaml
//        /// </summary>
//        public partial class ManageAthletesWindow : Window
//    {
//        System.Windows.Data.CollectionViewSource aAthleteViewSource;

//        public ManageAthletesWindow()
//        {
//            InitializeComponent();
//        }

//        private void Window_Loaded(object sender, RoutedEventArgs e)
//        {
//            aAthleteViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("aAthleteViewSource")));

//            aAthleteViewSource.Source = MainWindow.Context.People.ToList();

//            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(aAthleteListView.ItemsSource);
//            view.Filter = UserFilter;
//        }

//        private void btnEditAthlete_Click(object sender, RoutedEventArgs e)
//        {
//            Button btnEdit = (Button)sender;
//            Athlete Athlete = (Athlete)btnEdit.Tag;

//            EditAthleteWindow editWin;

//            if (Athlete.GetType().Name.Contains("StudentAthlete"))
//            {
//                editWin = new EditAthleteWindow((StudentAthlete)Athlete);
//                editWin.Show();
//            }
//            else
//            {
//                MessageBox.Show("Only student athletes can be edited", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void btnDone_Click(object sender, RoutedEventArgs e)
//        {
//            MainWindow.SQLiteSubmit();
//            this.Close();
//        }

//        public string TextFilter { get; set; }

//        public bool UserFilter (object p)
//        {
//            if (string.IsNullOrEmpty(TextFilter))
//                return true;
//            else
//                return (( p as Athlete ).PrintName().IndexOf(TextFilter,StringComparison.OrdinalIgnoreCase) >= 0);
//        }

//        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
//        {
//            TextFilter = ((TextBox)sender).Text;
//            CollectionViewSource.GetDefaultView(aAthleteListView.ItemsSource).Refresh();
//        }

//        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
//        {
//            MainWindow.SQLiteSubmit();
//        }

//        /// <summary>
//        /// This code currently doesn't work, don't copy it.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void btnDelete_Click(object sender, RoutedEventArgs e)
//        {
//            Athlete athlete = (Athlete)((Button)sender).Tag;

//            if (!athlete.CanBeDeleted)
//            {
//                MessageBox.Show(string.Format("{0} cannot be deleted as they are entered into at least one event", athlete.PrintName()));
//                return;
//            }

//            try
//            {
//                MainWindow.Context.People.DeleteOnSubmit(athlete);
                
//                Exception ex = MainWindow.SQLiteSubmit();

//                if (ex != null)
//                    MessageBox.Show(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//        }
//    }
//}
