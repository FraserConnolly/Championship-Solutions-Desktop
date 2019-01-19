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

namespace ChampionshipSolutions.Reporting
{
    /// <summary>
    /// Interaction logic for NewTemplateDialog.xaml
    /// </summary>
    public partial class NewTemplateDialog : Window
    {
        public NewTemplateDialog()
        {
            InitializeComponent();
        }

        public string TemplateName { get; set; }
        public string Host { get; set; }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            TemplateName = this.tbxName.Text;
            Host = this.tbxHost.Text;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.tbxName.Text = TemplateName;
            this.tbxHost.Text = Host;
        }
    }
}
