using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for WebServerPage.xaml
    /// </summary>
    public partial class WebServerPage : Page
    {
        public WebServerPage ( )
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate ( object sender , RequestNavigateEventArgs e )
        {
            if ( sender is Hyperlink h )
                Process.Start( h.NavigateUri.ToString( ) );
            e.Handled = true;
        }
    }
}
