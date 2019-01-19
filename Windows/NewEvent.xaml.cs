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
    /// Interaction logic for NewEvent.xaml
    /// </summary>
    public partial class NewEvent : Window
    {
        public NewEvent ( )
        {
            InitializeComponent ( );

            rbtnDistance.Checked += rbtn_Checked;
            rbtnHeats.Checked += rbtn_Checked;
            rbtnSchool.Checked += rbtn_Checked;
            rbtnIndividual.Checked += rbtn_Checked;

            rbtnTimed.Checked += rbtn_Checked;
            rbtnSingle.Checked += rbtn_Checked;
            rbtnNotSchool.Checked += rbtn_Checked;
            rbtnSquad.Checked += rbtn_Checked;

            rbtn_Checked ( null,null );
        }


        public DM.AEvent Event { get; private set;}

        private void btnDone_Click ( object sender , RoutedEventArgs e )
        {
            if ( rbtnIndividual.IsChecked == true )
                if ( rbtnTimed.IsChecked == true )
                    if ( rbtnSchool.IsChecked == true )
                        if ( rbtnHeats.IsChecked == true )
                            Event = new DM.IndividualTimedFinalSchoolEvent ( );
                        else
                            Event = new DM.IndividualTimedSchoolEvent ( );
                    else
                        if ( rbtnHeats.IsChecked == true )
                            Event = new DM.IndividualTimedFinalEvent ( );
                        else
                            Event = new DM.IndividualTimedEvent ( );
                else
                    Event = new DM.IndividualDistanceEvent ( );

            else
                if ( rbtnTimed.IsChecked == true )
                    Event = new DM.SquadTimedEvent ( );
                else
                    Event = new DM.SquadDistanceEvent ( );

            setResultsDisplay ( );

            DialogResult = true;
            Close ( );
        }

        private void setResultsDisplay ( )
        {
            if ( Event == null ) return;

            if(rbtnDistance.IsChecked == true )
            {
                if ( rbtnResultsDisplay6.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.DistanceMeters;
                if ( rbtnResultsDisplay7.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.DistanceMetersCentimeters;
                if ( rbtnResultsDisplay8.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.DistanceCentimeters;
            }
            else
            {
                if ( rbtnResultsDisplay1.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.TimedMinuetsSeconds;
                if ( rbtnResultsDisplay2.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.TimedMinuetsSecondsTenths ;
                if ( rbtnResultsDisplay3.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.TimedMinuetsSecondsHundreds ;
                if ( rbtnResultsDisplay4.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.TimedSecondsTenths ;
                if ( rbtnResultsDisplay5.IsChecked == true ) Event.ResultsDisplayDescription = DM.ResultDisplayDescription.TimedSecondsHundreds ;
            }
        }

        private void btnCncel_Click ( object sender , RoutedEventArgs e )
        {
            DialogResult = false;
            Close ( );
        }

        private void rbtn_Checked ( object sender , RoutedEventArgs e )
        {
            if(rbtnDistance.IsChecked == true )
            {
                rbtnResultsDisplay1.IsEnabled = false;
                rbtnResultsDisplay2.IsEnabled = false;
                rbtnResultsDisplay3.IsEnabled = false;
                rbtnResultsDisplay4.IsEnabled = false;
                rbtnResultsDisplay5.IsEnabled = false;
                rbtnResultsDisplay6.IsEnabled = true; 
                rbtnResultsDisplay7.IsEnabled = true; 
                rbtnResultsDisplay8.IsEnabled = true;
            }
            else
            {
                rbtnResultsDisplay1.IsEnabled = true;
                rbtnResultsDisplay2.IsEnabled = true;
                rbtnResultsDisplay3.IsEnabled = true;
                rbtnResultsDisplay4.IsEnabled = true;
                rbtnResultsDisplay5.IsEnabled = true;
                rbtnResultsDisplay6.IsEnabled = false;
                rbtnResultsDisplay7.IsEnabled = false;
                rbtnResultsDisplay8.IsEnabled = false;
            }


            if ( rbtnIndividual.IsChecked == true  )
                if ( rbtnTimed.IsChecked == true )
                    goto Enabled;
                else
                    goto Disable;
            else
                goto Disable;

            Enabled:
            this.rbtnNotSchool.IsEnabled = true;
            this.rbtnSchool.IsEnabled = true;
            this.rbtnHeats.IsEnabled = true;
            this.rbtnSingle.IsEnabled = true;
            return;

            Disable:
            this.rbtnNotSchool.IsEnabled = false;
            this.rbtnSchool.IsEnabled = false;
            this.rbtnHeats.IsEnabled = false;
            this.rbtnSingle.IsEnabled = false;
            return;
        }
    }
}
