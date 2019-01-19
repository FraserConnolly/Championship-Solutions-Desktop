using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    public partial class ReportDesigner : Window
    {
        private string report;

        public ReportDesigner ( )
        {
            InitializeComponent( );
            //SkinStorage.SetVisualStyle( reportDesigner , "SyncOrange" );
        }

        public ReportDesigner ( string report ) : this()
        {
            this.report = report;
        }

        private void Window_Loaded ( object sender , RoutedEventArgs e )
        {

            reportDesigner.DesignMode = Syncfusion.Windows.Reports.Designer.DesignMode.RDLC;
            reportDesigner.Assemblies = new List<Assembly>( );
            reportDesigner.EnableMDIDesigner = true;

            reportDesigner.Assemblies.Add( Assembly.GetExecutingAssembly( ) );

            if ( report != null )
                reportDesigner.OpenReport( report );
            else
                reportDesigner.NewReport( );

        }
    }
}
