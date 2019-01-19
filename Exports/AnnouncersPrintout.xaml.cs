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
using ChampionshipSolutions.DM;

namespace ChampionshipSolutions.ControlRoom
{
    /// <summary>
    /// Interaction logic for AnnouncersPrintout.xaml
    /// </summary>
    public partial class AnnouncersPrintout : Window
    {
        private AEvent Event { get; set; }
        
        private AnnouncersPrintout()
        {
            InitializeComponent();
        }

        public AnnouncersPrintout(AEvent Event)
        {
            this.Event = Event;
            InitializeComponent();
            refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new System.Windows.Controls.PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                printDlg.PrintVisual(gridPrintable, Event.Name + " Announcers Sheet");
            }

        }

        private void clearLabels()
        {
            this.lblEventName.Content = string.Empty;
            this.lblTopIndividuals .Content = string.Empty;
            //this.lblLowerYearGroup .Content = string.Empty;
            //this.lblFirstATeam .Content = string.Empty;
            //this.lblSecondATeam .Content = string.Empty;
            //this.lblFirstBTeam.Content = string.Empty;
            //this.lblSecondBTeam.Content = string.Empty;
            lblTotal.Content = "";
            lblTeam.Content = "";
            lblMB.Content = "";
            lblMG.Content = "";
            lblJB.Content = "";
            lblJG.Content = "";
            lblIB.Content = "";
            lblIG.Content = "";
            lblSB.Content = "";
            lblSG.Content = "";
        }

        private void refresh()
        {
            clearLabels();

            if (Event.customFieldExists("PrintedName"))
                this.lblEventName.Content = Event.getValue("PrintedName").ToString();
            else   
                this.lblEventName.Content = Event.Name;

            if (Event is ASquadEvent)
            {
                Squad squad;// = (ASquadEvent)Event;

                foreach (AResult r in Event.TopIndividuals())
                {
                    squad = (Squad)r.Competitor;

                    this.lblTopIndividuals.Content += string.Format("{0}  {1} \n", r.printRank, r.printTeam());

                    foreach (AAthlete a in squad.members)
                    {
                        if (a == null) continue;
                        this.lblTopIndividuals.Content += string.Format("\t{0}\n", a.PrintName());
                    }

                    this.lblTopIndividuals.Content += string.Format("\t{0}\n", r.printResultValueString());
                }
            }
            else
            {
                foreach (AResult r in Event.TopIndividuals())
                    this.lblTopIndividuals.Content += string.Format("{0}  {1} \n\t{2}\n\t{3}\n", r.printRank, r.printName(), r.printResultValueString(), r.printTeam());
            }
            foreach (ChampionshipTeamResult CTR in Event.Championship.getOverallSores().OrderBy ( o => o.Rank ))
            {
                if(CTR.ScoringTeamName=="B") continue;

                lblTeam.Content += CTR.Team.Name + "\n\n";

                lblMB.Content += Event.Championship.getOverallSores("Minor Boys").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";
                lblMG.Content += Event.Championship.getOverallSores("Minor Girls").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";
                lblJB.Content += Event.Championship.getOverallSores("Junior Boys").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";
                lblJG.Content += Event.Championship.getOverallSores("Junior Girls").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";
                lblIB.Content += Event.Championship.getOverallSores("Intermediate Boys").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";
                lblIG.Content += Event.Championship.getOverallSores("Intermediate Girls").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";
                lblSB.Content += Event.Championship.getOverallSores("Senior Boys").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";
                lblSG.Content += Event.Championship.getOverallSores("Senior Girls").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";

                lblTotal.Content += CTR.Points + "\n\n";
            }

            #region legacy version

            //foreach (AResult r in ((SchoolIndividualEvent)Event).TopLowerYearGroup())
            //    this.lblLowerYearGroup.Content += string.Format("{0}  {1}  {2}{3}", r.printRank, r.printName(), r.printParameter("Attends"), Environment.NewLine);

            //foreach (AResult r in Event.getScoringTeamResults(1, "A"))
            //{
            //    this.lblFirstATeam.Content += string.Format("{0}  {1}  {2}{3}", r.printRank, r.printName(), r.printParameter("Attends"), Environment.NewLine);
            //    this.lblFirstTeamAHeader.Content = "First Team - " + r.printTeam();
            //}

            //Team FirstTeamA = Event.getScoringTeam(1, "A");
            //if (FirstTeamA != null)
            //{
            //    this.lblFirstTeamAHeader.Content = "First Team - " + FirstTeamA.Name;

            //    foreach (AResult r in Event.getResultsForTeam(FirstTeamA))
            //    {
            //        this.lblFirstATeam.Content += string.Format("{0}  {1} \t{2}{3}", r.printRank, r.printName(), r.printParameter("Attends"), Environment.NewLine);
            //    }
            //}

            //foreach (AResult r in Event.getScoringTeam(2, "A"))
            //    this.lblSecondATeam.Content += string.Format("{0}  {1}  {2}{3}", r.printRank, r.printName(), r.printParameter("Attends"), Environment.NewLine);

            //foreach (AResult r in Event.getScoringTeam(1, "B"))
            //    this.lblFirstBTeam.Content += string.Format("{0}  {1}  {2}{3}", r.printRank, r.printName(), r.printParameter("Attends"), Environment.NewLine);

            //foreach (AResult r in Event.getScoringTeam(2, "B"))
            //    this.lblSecondBTeam.Content += string.Format("{0}  {1}  {2}{3}", r.printRank, r.printName(), r.printParameter("Attends"), Environment.NewLine);


            #endregion
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            refresh();
        }
    }
}
