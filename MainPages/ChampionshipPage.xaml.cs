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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChampionshipSolutions.DM;
using ChampionshipSolutions.ViewModel;
using static ChampionshipSolutions.FileIO.FConnFile;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for ChampionshipPage.xaml
    /// </summary>
    public partial class ChampionshipPage : Page
    {

        public ChampionshipPage()
        {
            InitializeComponent();
            setChampionship(((App)App.Current).CurrentChampionship);
        }

        public void setChampionship ( ChampionshipVM Championship )
        {
            DataContext = Championship;
        }
    }
}
