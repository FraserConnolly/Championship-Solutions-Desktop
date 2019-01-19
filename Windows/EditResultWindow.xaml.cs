/*
 *  Filename         : EditResultsWindow.xaml.cs
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
 *    2016-04-24
 *      Moved to Championship Solutions V3-0 and converted to work with FConnFile
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

namespace ChampionshipSolutions.ControlRoom
{
    /// <summary>
    /// Interaction logic for EditResultWindow.xaml
    /// </summary>
    public partial class EditResultWindow : Window
    {
        public AResult Result;
        private AEvent Event;
        private ACompetitor Competitor;
        
        private bool isDNF = false;
        private bool newResult = false;
        private bool locked = false;

        private EditResultWindow()
        {
            InitializeComponent();
            newResult = true;
        }

        public EditResultWindow(AResult result)
        {
            InitializeComponent();
            Result = result;
            Event = result.Event;
            refreshData();
            lockEntry();
        }

        private void refreshData()
        {
            Competitor = null;
            tbxVest.Text = string.Empty;

            this.Title = "Edit Result - " + Event.Name;
            if (Result != null)
            {
                tbxVest.Text = Result.printVestNo();
                tbxDuration.Text = Result.printResultValueString();
                tbxRank.Text = Result.Rank.ToString();

                if (Result.isPlaceholder())
                {
                    unLockEntry();
                }
            }
        }

        public EditResultWindow(AEvent Event, bool newResult = false, bool isDNF = false)
        {
            InitializeComponent();

            if (Event == null)
                throw new ArgumentNullException () ;

            this.Event = Event;

            if (newResult)
            {
                if (isDNF)
                {
                    // this is a DNF entry
                    this.tbxVest.Text = "";

                    this.tbxDuration.Text = "";
                    this.tbxDuration.IsEnabled = false;
                    this.tbxRank.IsEnabled = false;
                    this.tbxRank.Text = "";

                    this.tbxCompName.Text = "";

                    this.btnSetAsDNF.IsEnabled = false;
                    this.btnPrevious.IsEnabled = false;
                    this.btnNext.IsEnabled = false;
                    this.btnUnlock.IsEnabled = false;

                    this.isDNF = true;
                    this.newResult = true;
                }
                else
                {
                    // we will open a new result at the next available rank
                    this.tbxRank.Text = Event.getNextResultRank().ToString();
                    this.newResult = true;
                }
            }
            else
            {
                if (Event.Results.Count() == 0)
                {
                    // create first result
                    newResult = true;
                    this.tbxRank.Text = Event.getNextResultRank().ToString();
                }
                else if (Event.Results.Count() > 0)
                {
                    // select the first result
                    Result = Event.Results.OrderBy(r => r.Rank).First();
                    tbxRank.Text = Result.Rank.ToString();
                    refreshData();
                    lockEntry();
                }
            }

        }

        public EditResultWindow(AEvent Event, int rank)
            : this(Event, true)
        {
            this.tbxRank.Text = rank.ToString();

            if (!Event.isRankAvailable(rank))
            {
                // rank is not available so lets make space
                Event.insertRankSpace(rank);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NextResult_Click(object sender, RoutedEventArgs e)
        {
            int rank;
            ProcessResultForm();
            if (int.TryParse(tbxRank.Text, out rank))
            {
                Result = Event.getResult(++rank);

                if (Result == null)
                {
                    tbxRank.Text = rank.ToString();
                    prepNewResult();
                }
                else
                {
                    refreshData();
                }
            }
        }

        private void PreviousResult_Click(object sender, RoutedEventArgs e)
        {
            int rank;
            ProcessResultForm();
            if (int.TryParse(tbxRank.Text, out rank))
            {
                if (rank == 1)
                    if (Result != null)
                        if (!Result.isPlaceholder())
                            --rank;
                
                if (rank > 1)
                    Result = Event.getResult(--rank);


                if (rank < 1)
                {
                    // out of range push results down
                    rank = 1;
                    Event.insertRankSpace(rank);
                    Result = Event.getResult(rank);
                }

                if (Result == null)
                {
                    tbxRank.Text = rank.ToString();
                    prepNewResult();
                }
                else
                {
                    refreshData();
                }
            }
        }

        private void prepNewResult()
        {
            newResult = true;
            tbxVest.Text = "";
            unLockEntry();
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            ProcessResultForm();
            this.Close();
        }

        private void ProcessResultForm()
        {
            int rank;
            if (!locked)
            {
                try
                {
                    if (int.TryParse(tbxRank.Text, out rank))
                    {
                        if (Event.isRankAvailable(rank))
                        {
                            if (Competitor == null)
                            {
                                ResultValue rv = AEvent.MakeNewResultsValue(Event);

                                if (rv.setResultString(tbxDuration.Text))
                                {
                                    Result = Event.AddResult(rank, rv);
                                }
                                else
                                {
                                    Result = Event.AddPlaceholderResult(rank);
                                }
                            }
                            else
                            {
                                // Competitive, CompetativeTimed or Competitive DNF
                                if (isDNF)
                                {
                                    Result = Event.AddDNF(Competitor); //new ResultCompetiveDNF();
                                }
                                else
                                {
                                    // Competitive or CompetativeTimed
                                    if (string.IsNullOrWhiteSpace(tbxDuration.Text))
                                    {
                                        Result = Event.AddResult(rank, Competitor); //new Result();
                                    }
                                    else
                                    {
                                        ResultValue rv = AEvent.MakeNewResultsValue(Event);

                                        if (rv.setResultString(tbxDuration.Text))
                                        {
                                            Result = Event.AddResult(rank, Competitor, rv);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Failed To phrase Duration");
                                            return;
                                        }

                                    }
                                }
                            }

                            lockEntry();
                        }
                        else // rank is not available
                        {
                            if (MessageBox.Show("This rank has already been used, do you want to use the next available rank for this event?", "Rank not recognized", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                this.tbxRank.Text = Event.getNextResultRank().ToString();
                                ProcessResultForm();
                            }
                            else
                            {
                                return;
                            }
                        }
                    }// no rank
                    else
                    {
                        if (isDNF)
                        {
                            if (Competitor != null)
                            {
                                Result = Event.AddDNF(Competitor); //new ResultCompetiveDNF();
                            }
                            else
                            {
                                MessageBox.Show("That vest number was not entered into this event");
                            }
                        }
                        else
                        {
                            if (MessageBox.Show("Rank has not been set/recognized, do you want to use the next available rank for this event?", "Rank not recognized", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                this.tbxRank.Text = Event.getNextResultRank().ToString();
                                ProcessResultForm();
                            }
                            else
                            {
                                return;
                            }
                        }
                    } // if rank
                }// try
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else //entry form is locked - do nothing
            {
            }
        }

        private void lockEntry()
        {
            this.tbxRank.IsEnabled = false;
            this.tbxDuration.IsEnabled = false;
            this.btnSetAsDNF.IsEnabled = false;
            this.tbxVest.IsEnabled = false;
            newResult = false;
            locked = true;
        }

        private void unLockEntry()
        {
            if (Result == null)
                this.tbxRank.IsEnabled = true;
            this.tbxDuration.IsEnabled = true;
            this.btnSetAsDNF.IsEnabled = true;
            this.tbxVest.IsEnabled = true;
            locked = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshData();
        }

        private void btnSetAsDNF_Click(object sender, RoutedEventArgs e)
        {
            if (Competitor != null)
            {
                isDNF = true;

                tbxDuration.Text = "";

                ProcessResultForm();
            }
            else
            {
                MessageBox.Show("Only vest numbers that are associated with a competitor can be set as DNF");
            }
        }

        private void btnOpenCompetitor_Click(object sender, RoutedEventArgs e)
        {
            if(Competitor != null)
                if(Competitor.checkParameter("Athlete") != null)
                    try
                    {
                        // disabled in V3-0 as unsure if it will be brought across. 2016-04-24
                        //new EditAthleteWindow((Athlete)Competitor.checkParameter("Athlete")).Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not edit this athlete" + Environment.NewLine + ex.Message);
                    }
        }

        private void tbxVest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.tbxVest.Text))
            {
                Competitor = Event.getCompetitor(this.tbxVest.Text);

                if (Competitor != null)
                {
                    this.tbxCompName.Text = Competitor.Name;
                    if(!locked)
                        this.btnSetAsDNF.IsEnabled = true;
                    this.btnOpenCompetitor.IsEnabled = true;
                    return;
                }
            }

            this.btnOpenCompetitor.IsEnabled = false;
            this.btnSetAsDNF.IsEnabled = false;
            this.tbxCompName.Text = string.Empty;
        }

        private void ResetResult_Click(object sender, RoutedEventArgs e)
        {
            refreshData();

            if (Result != null)
            {
                if (Result.Rank.HasValue)
                {
                    int rank = Result.Rank.Value;
                    Event.removeResult(Result);
                    Event.insertRankSpace(rank);
                }
                else
                {
                    Event.removeResult(Result);
                } 
                unLockEntry();
            }
            else
            {
                MessageBox.Show("An error occurred when resetting this result");
            }
        }

    }
}
