using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using Xceed.Wpf.Toolkit;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for ScriptsPage.xaml
    /// </summary>
    public partial class LoadingPage : Page
    {
        public LoadingPage ( )
        {
            InitializeComponent();
        }

        string filePath { get; set; }

        private void Window_Loaded ( object sender , RoutedEventArgs e )
        {
            BusyIndicator.IsBusy = true;
            BusyIndicator.BusyContent = "Loading File, Please Wait...";
        }

    }
}
