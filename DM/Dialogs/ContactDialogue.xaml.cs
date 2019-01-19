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

namespace ChampionshipSolutions.DM.Dialogs
{
    /// <summary>
    /// Interaction logic for ContactDialogue.xaml
    /// </summary>
    public partial class ContactDialogue : Window
    {
        public ContactDialogue ( )
        {
            InitializeComponent ( );
        }

        private void Button_Click ( object sender , RoutedEventArgs e )
        {
            if ( DataContext is IID )
                ( (IID)DataContext ).Save ( );
            Close ( );
        }
    }
}
