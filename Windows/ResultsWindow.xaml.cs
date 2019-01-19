/*
 *  Filename         : ResultsWindow.xaml.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 *    2015-05-02
 *      Modified to work with IndividualTimedEvent rather than AEvent - This will have to be changed later.
 * 
 */

using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using ChampionshipSolutions.DM;
using static ChampionshipSolutions.FileIO.FConnFile;
using ChampionshipSolutions.ViewModel;
using ChampionshipSolutions.ControlRoom;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        public AEvent Event { get; set; }

        private ResultsWindow()
        {
            InitializeComponent();
        }

        public ResultsWindow(AEvent Event) : this()
        {
            this.Event = Event;

            if (Event is IHeatEvent)
            {
                this.lblEvent.Text = Event.Name + " " + ((IHeatEvent)Event).Final.getHeatNumber((IHeatEvent)Event);
            }
            else
            {
                this.lblEvent.Text = Event.Name;
            }

        }

        private void InsertAboveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AResult res = (AResult)this.resultsDataGrid.SelectedItem;
            EditResultWindow erw;

            if (res == null)
            {
                erw = new EditResultWindow(Event);
                erw.ShowDialog();
                return;
            }

            if (res.Rank.HasValue)
            {
                erw = new EditResultWindow(Event, res.Rank.Value);
            }
            else
            {
                erw = new EditResultWindow(Event, true);
            }

            try
            {
                erw.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            refreshData();
        }

        private void InsertBelowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AResult res = (AResult)this.resultsDataGrid.SelectedItem;

            EditResultWindow erw;

            if (res == null)
            {
                erw = new EditResultWindow(Event);
                erw.ShowDialog();
                return;
            }

            if (res.Rank.HasValue)
            {
                erw = new EditResultWindow(Event, (res.Rank.Value + 1));
            }
            else
            {
                erw = new EditResultWindow(Event, true);
            }

            try
            {
                erw.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            refreshData();

        }

        private void RemoveResult(object sender, RoutedEventArgs e)
        {
            AResult res = (AResult)this.resultsDataGrid.SelectedItem;

            if (res == null) return;

            if (MessageBox.Show("Are you sure you want to remove result " + res.printResult(), "Remove Result?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Event.removeResult(res);
                //MainWindow.Context.Results.DeleteOnSubmit(res);

                // hopefully no longer required because Result.Event has a delete on null attribute
                // getContext().Results.DeleteOnSubmit(res);

                //SaveChanges ( );

                //MainWindow.Context.Results.Remove(res);
                //MainWindow.SQLiteSubmit();
                //MainWindow.Context.SaveChanges();
                refreshData();
            }

        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AResult res = (AResult)this.resultsDataGrid.SelectedItem;

            if (res == null) return;

            EditResultWindow erw = new EditResultWindow(res);
            erw.ShowDialog();

            refreshData();
        }

        private void OpenResultsCard_Click(object sender, RoutedEventArgs e)
        {
            if (Event == null)
                return;

            List<AEvent> evs = new List<AEvent>();

            if (Event is IHeatEvent)
                evs.Add(((IHeatEvent)Event).Final);
            else
                evs.Add(Event);

            try
            {
                if ( ((App) App.Current).CurrentChampionship.ShortName.Contains( "XC" ) )
                {
                    Exports.printBothResults( evs , true );
                }
                else if ( ((App)App.Current).CurrentChampionship.ShortName.Contains( "TF" ) )
                {
                    Exports.GenerateResultEntryForms(evs, true);
                }
                else
                {
                    MessageBox.Show("This feature is only available for championships that are XC or TF.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open results sheet. \n" + ex.Message);
            }
        }

        private void OpenHeats_Click(object sender, RoutedEventArgs e)
        {
            if (Event is IFinalEvent)
            {
                foreach (IHeatEvent h in ((IFinalEvent)Event).getHeats())
                {
                    new ResultsWindow((AEvent)h).Show();
                }

                return;
            }
            else
            {
                MessageBox.Show( "This event does not have any heats" );
            }
        }

        private void ShowScan_Click(object sender, RoutedEventArgs e)
        {
            if (Event is IHeatEvent)
            {
                IHeatEvent iEvent = (IHeatEvent)Event;

                if (iEvent.Final.Files.Count() > 1)
                {
                    // multiple files
                    if (MessageBox.Show("Multiple scans are present, do you want to show only the most recent?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // show only the first file
                        new Scanning.ImageDisplay(iEvent.Final.Files.Last()).Show();
                    }
                    else
                    {
                        // show all files
                        foreach (FileStorage fs in iEvent.Final.Files)
                        {
                            new Scanning.ImageDisplay(fs).Show();
                        }
                    }
                }
                else if (Event.Files.Count() == 1)
                {
                    new Scanning.ImageDisplay(iEvent.Final.Files.First()).Show();
                }
                else
                {
                    MessageBox.Show( "There are no scans to display" );
                }
            }
            else
            {
                if (Event.Files.Count() > 1)
                {
                    // multiple files
                    if (MessageBox.Show("Multiple scans are present, do you want to show only the most recent?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // show only the first file
                        new Scanning.ImageDisplay(Event.Files.Last()).Show();
                    }
                    else
                    {
                        // show all files
                        foreach (FileStorage fs in Event.Files)
                        {
                            new Scanning.ImageDisplay(fs).Show();
                        }
                    }
                }
                else if (Event.Files.Count() == 1)
                {
                    new Scanning.ImageDisplay(Event.Files.First()).Show();
                }
                else
                {
                    MessageBox.Show( "There are no scans to display" );
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            refreshData();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            AResult res = (AResult)this.resultsDataGrid.SelectedItem;
            if (res == null) return;

            Event.moveResultUp(res);
            refreshData();
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            AResult res = (AResult)this.resultsDataGrid.SelectedItem;
            if (res == null) return;

            Event.moveResultDown(res);
            refreshData();
        }

        private void InsertDNF_Click(object sender, RoutedEventArgs e)
        {
            EditResultWindow erw = new EditResultWindow(Event, true, true);
            erw.ShowDialog();

            refreshData();
        }

        private void AthleteName_Click(object sender, RoutedEventArgs e)
        {

            ACompetitor comp = (ACompetitor)((Button)sender).Tag;

            if ( comp is Competitor )
                ( (App)App.Current ).CurrentChampionship.EditPerson.Execute ( new AthleteVM ( ( (Competitor)comp ).Athlete ) );

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshData();
            this.Title = "Results " + Event.Name;
        }

        private void refreshData()
        {
            System.Windows.Data.CollectionViewSource aChampionshipEventsResultsViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("aChampionshipEventsResultsViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            aChampionshipEventsResultsViewSource.Source = Event.Results.OrderBy(r => r.Rank);
            this.resultsDataGrid.ItemsSource = null;
            this.resultsDataGrid.ItemsSource = Event.Results.OrderBy(r => r.Rank);
            //this.resultsDataGrid.DataContext = Event.Results;

            if (selectedResult != null)
                this.resultsDataGrid.SelectedItem = selectedResult;

            this.dgScoringTeams.ItemsSource = Event.getScoringTeams().Where(e => e.Rank > 0);

            if (Event is IHeatEvent)
            {
                IHeatEvent iEvent = (IHeatEvent)Event;
                if (iEvent.Final.Files.Count() > 0)
                    this.btnShowScannedResults.Visibility = System.Windows.Visibility.Visible;
                else
                    this.btnShowScannedResults.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                if (Event.Files.Count() > 0)
                    this.btnShowScannedResults.Visibility = System.Windows.Visibility.Visible;
                else
                    this.btnShowScannedResults.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (Event is IFinalEvent)
            {
                this.btnSetHeatAsFinal.Visibility = System.Windows.Visibility.Visible;
                if (((IFinalEvent)Event).HeatRunAsFinal)
                {
                    this.btnOpenHeats.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    this.btnOpenHeats.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                this.btnOpenHeats.Visibility = System.Windows.Visibility.Collapsed;
                this.btnSetHeatAsFinal.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Auto Lane Assignment has been removed
            //if (Event is ILaneAssignedEvent)
                //if (((ILaneAssignedEvent)Event).requiresLaneUpdate())
                    //this.btnOpenResultsCard.Visibility = System.Windows.Visibility.Visible;
                //else
                    //this.btnOpenResultsCard.Visibility = System.Windows.Visibility.Collapsed;
            //else
                //this.btnOpenResultsCard.Visibility = System.Windows.Visibility.Collapsed;



        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private AResult selectedResult;

        private void resultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.resultsDataGrid.SelectedItem != null)
                selectedResult = (AResult)this.resultsDataGrid.SelectedItem;
        }

        //private void UpdateLaneDraw_RightClick(object sender, MouseButtonEventArgs e)
        //{
        //    // replace lane draw

        //    if (Event is ILaneAssignedEvent)
        //    {
        //        ILaneAssignedEvent iE = (ILaneAssignedEvent)Event;

        //        iE.clearLanes();

        //        UpdateLaneDraw_Click(sender, e);
        //    }
        //}

        //private void UpdateLaneDraw_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Event is ILaneAssignedEvent)
        //    {
        //        ILaneAssignedEvent iE = (ILaneAssignedEvent)Event;

        //        iE.updateLanes();
        //    }

        //}

        private void UnSetRunHeatAsFinal_Click(object sender, MouseButtonEventArgs e)
        {
            if (Event is IFinalEvent)
            {
                ((IFinalEvent)Event).HeatRunAsFinal = false;
                refreshData();
            }
        }

        private void SetRunHeatAsFinal_Click(object sender, RoutedEventArgs e)
        {
            if (Event is IFinalEvent)
            {
                ((IFinalEvent)Event).HeatRunAsFinal = true;
                refreshData();
            }
        }
    }
}
