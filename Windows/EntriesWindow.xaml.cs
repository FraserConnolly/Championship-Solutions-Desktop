using ChampionshipSolutions.DM;
using ChampionshipSolutions.ViewModel;
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

namespace ChampionshipSolutions.ControlRoom
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class EntriesWindow : Window
    {
        public virtual AEvent Event { get; set; }
        //private bool showOnlySWAvailable = true;

        #region Constructors

        private EntriesWindow()
        {
            InitializeComponent();
        }

        public EntriesWindow(AEvent Event) : this()
        {
            if (Event == null)
                throw new ArgumentNullException();

            this.Event = Event;
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshData();
            this.Title = "Selection - " + Event.Name;
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AthleteName_Click(object sender, RoutedEventArgs e)
        {
            ACompetitor comp = (ACompetitor)((Button)sender).Tag;

            if (comp == null)
                return;

            Athlete ath = (Athlete)comp.checkParameter("Athlete");

            EditAthleteWindow eaw = new EditAthleteWindow();

            eaw.DataContext = new AthleteVM ( ath );

            eaw.Show();
        }

        private void refreshData()
        {
            System.Windows.Data.CollectionViewSource championshipEventsEnteredCompetitorsViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("championshipEventsEnteredCompetitorsViewSource")));
            
            //if(showOnlySWAvailable)
            //    championshipEventsEnteredCompetitorsViewSource.Source = Event.EnteredCompetitors.Where(c => c.isAvilableForSW()).ToList();//.OrderBy(c => c.Result.printRank);
            //else
                championshipEventsEnteredCompetitorsViewSource.Source = Event.EnteredCompetitors;//.OrderBy(c => c.Result.printRank);
        }

        //private void cbxShowSWOnly_Checked(object sender, RoutedEventArgs e)
        //{
        //    showOnlySWAvailable = true;
        //    refreshData();
        //}

        //private void cbxShowSWOnly_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    showOnlySWAvailable = false;
        //    refreshData();
        //}

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshData();
        }
    }
}
