using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for ImportResultsPage.xaml
    /// </summary>
    public partial class ImportResultsPage : Page
    {
        public ImportResultsPage ( )
        {
            InitializeComponent();

            timmer = new System.Windows.Threading.DispatcherTimer ( );
            timmer.Tick += timmer_Tick;

        }
        private void Window_Loaded ( object sender , RoutedEventArgs e )
        {
            refreshUI ( );
        }

        protected void refreshUI ( )
        {
            System.Windows.Data.CollectionViewSource aChampionshipViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("aChampionshipVestActionsViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            aChampionshipViewSource.Source = ( (App)App.Current ).CurrentChampionship.Championship.VestActions.OrderByDescending ( va => va.WebID ).ToList ( );
        }

        private void Window_Unloaded ( object sender , RoutedEventArgs e )
        {
            stop ( );
        }

        private void cmdStart_Click ( object sender , RoutedEventArgs e )
        {
            Thread t = new Thread(timmer.Start);
            t.Start ( );
            //start();
        }

        private void cmdStop_Click ( object sender , RoutedEventArgs e )
        {
            stop ( );
        }

        public void start ( )
        {
            this.cmdStart.IsEnabled = false;
            this.cmdStop.IsEnabled = true;

            timmer = new DispatcherTimer ( );
            timmer.Tick += timmer_Tick;
            timmer.Interval = DEFAULTTICK;
            timmer.Start ( );
        }

        public void stop ( )
        {
            this.cmdStart.IsEnabled = true;
            this.cmdStop.IsEnabled = false;

            timmer.Stop ( );
        }

        private void cmdRefresh_Click ( object sender , RoutedEventArgs e )
        {
            elapsedTimmer = (int)MAXWAITTIME.TotalSeconds + 1;
        }

        protected void refreshWaitBar ( double p , int timeToNextCheck )
        {
            Dispatcher.Invoke ( ( ) =>
            {
                this.progWaitBar.Maximum = p;
                this.progWaitBar.Value = timeToNextCheck;
            } );
        }

        private void cmdAddManual_Click ( object sender , RoutedEventArgs e )
        {

        }
    }
}
