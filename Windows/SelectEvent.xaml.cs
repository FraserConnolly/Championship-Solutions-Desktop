using ChampionshipSolutions.DM;
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

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for SelectEvent.xaml
    /// </summary>
    public partial class SelectEvent : Window
    {
        public SelectEvent()
        {
            InitializeComponent();
        }

        public SelectEvent(List<AEvent> Events)
            : this()
        {
            this.cbxSelectEvent.ItemsSource = Events;
        }

        public AEvent SelectedEvent { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SelectedEvent = null;
            this.Close();
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            SelectedEvent = (AEvent)this.cbxSelectEvent.SelectedItem;
        }
    }
}
