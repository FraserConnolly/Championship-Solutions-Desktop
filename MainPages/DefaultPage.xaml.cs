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
using ChampionshipSolutions.Controls;
using System.IO;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for DefaultPage.xaml
    /// </summary>
    public partial class DefaultPage : Page
    {
        //MenuItem RecentlyUsedMenuItem;

        public DefaultPage ( )
        {
            InitializeComponent();
            //this.RecentlyUsedMenuItem = new MenuItem ( );
        }

        //public DefaultPage(MenuItem RecentlyUsedMenuItem) : this ()
        //{
            //this.RecentlyUsedMenuItem = RecentlyUsedMenuItem;
        //}

        private void Page_Loaded ( object sender , RoutedEventArgs e )
        {
            DataContext = ( (App)Application.Current ).mruManager.RecentFiles();
        }



        private void Button_Click ( object sender , RoutedEventArgs e )
        {
            string fName = (sender as Button).Content as string;
            if ( !File.Exists ( fName ) )
            {
                if ( MessageBox.Show ( string.Format ( "{0} doesn't exist. Remove from recent workspaces?" , fName ) , "File not found" , MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
                    ( (App)Application.Current ).mruManager.RemoveRecentFile ( fName );
                return;
            }

            ( (App)Application.Current ).openFile( fName );
        }
    }
}
