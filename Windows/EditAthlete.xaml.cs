using ChampionshipSolutions.ViewModel;
using System.Windows;

namespace ChampionshipSolutions
{

    /// <summary>
    /// Interaction logic for EditAthlete.xaml
    /// </summary>
    public partial class EditAthleteWindow : Window
    {
        public EditAthleteWindow ( )
        {
            InitializeComponent ( );
        }

        private void Button_Click ( object sender , RoutedEventArgs e )
        {
            ((PersonVM) DataContext ).Person.Save ( );
            ( (App)Application.Current ).CurrentChampionship.UpdateAthletes ( );
            this.Close ( );
        }
    }
}
