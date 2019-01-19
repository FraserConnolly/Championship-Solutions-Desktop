using ChampionshipSolutions.DM;
using Microsoft.Win32;
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
using System.Xml;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for AthltesPage.xaml
    /// </summary>
    public partial class AthltesPage : Page
    {
        public AthltesPage ( )
        {
            InitializeComponent();
        }

        private void ImportEntryForm_Click ( object sender , RoutedEventArgs e )
        {
            ChampionshipSolutions.MainWindow.Frame.Navigate ( new ImportEntryFormPage ( this ) );
        }

        //private void Button_Click ( object sender , RoutedEventArgs e )
        //{
        //    MessageBox.Show ( "Disabled" );
        //}

        private void Page_Loaded ( object sender , RoutedEventArgs e )
        {
            ( (App)App.Current ).CurrentChampionship.UpdateCounters ( );
            refreshData ( );
        }

        public void clearData ( )
        {
            this.lblEJB.Content = "";
            this.lblEJG.Content = "";
            this.lblEIB.Content = "";
            this.lblEIG.Content = "";
            this.lblESB.Content = "";
            this.lblESG.Content = "";
            this.lblETotal.Content = "";

            this.lblSelJB.Content = "";
            this.lblSelJG.Content = "";
            this.lblSelIB.Content = "";
            this.lblSelIG.Content = "";
            this.lblSelSB.Content = "";
            this.lblSelSG.Content = "";
            this.lblSelTotal.Content = "";

            this.lblGrandTotal.Content = "";
        }

        public void refreshData ( )
        {
            clearData ( );

            //if ( Championship == null ) return;

            //if ( Championship.Name != "WSAA Athletics Championship 2015" && Championship.Name != "South West TF 2014-15" ) return;

            try
            {


                Championship Championship = ((App)Application.Current).CurrentChampionship.Championship;

                Dictionary<Group[], int> AgeGroups = new Dictionary<Group[],int>();

                AgeGroups.Add ( new Group[] { Championship.Groups.Where( g => g.Name=="Junior").First(),
                Championship.Groups.Where( g => g.Name=="Boys").First() } , 0 );

                AgeGroups.Add ( new Group[] { Championship.Groups.Where( g => g.Name=="Junior").First(),
                Championship.Groups.Where( g => g.Name=="Girls").First() } , 0 );

                AgeGroups.Add ( new Group[] { Championship.Groups.Where( g => g.Name=="Intermediate").First(),
                Championship.Groups.Where( g => g.Name=="Boys").First() } , 0 );

                AgeGroups.Add ( new Group[] { Championship.Groups.Where( g => g.Name=="Intermediate").First(),
                Championship.Groups.Where( g => g.Name=="Girls").First() } , 0 );

                AgeGroups.Add ( new Group[] { Championship.Groups.Where( g => g.Name=="Senior").First(),
                Championship.Groups.Where( g => g.Name=="Boys").First() } , 0 );

                AgeGroups.Add ( new Group[] { Championship.Groups.Where( g => g.Name=="Senior").First(),
                Championship.Groups.Where( g => g.Name=="Girls").First() } , 0 );


                int[] countEntered = new int[9];
                int[] nonEntered = new int[9];
                int grandTotal = 0;
                int a = 0;

                // Count SW JB Events
                nonEntered[0] = 14;
                // Count SW JG Events
                nonEntered[1] = 12;
                // Count SW IB Events
                nonEntered[2] = 17;
                // Count SW IG Events
                nonEntered[3] = 17;
                // Count SW SB Events
                nonEntered[4] = 17;
                // Count SW SG Events
                nonEntered[5] = 17;

                foreach ( Group[] ageGroup in AgeGroups.Keys )
                {
                    //Group[] ageGroup = AgeGroups[a];


                    foreach ( AEvent Event in Championship.listAllEvents ( ))
                    {
                        bool inGroup = true;

                        foreach ( Group g in ageGroup )
                            if ( !Event.hasGroup ( g ) )
                            {
                                inGroup = false;
                                break;
                            }

                        if ( !inGroup ) continue;


                        if ( Event.countCurrentlySelected ( ) == 0 )
                        {
                            grandTotal += 1;
                            //nonEntered[a] += 1;
                        }
                        else
                        {
                            nonEntered[a] -= 1;
                            grandTotal += Event.countCurrentlySelected ( );
                            countEntered[a] += Event.countCurrentlySelected ( );
                        }
                    }

                    switch ( a )
                    {
                        case 0:
                            lblEJB.Content = nonEntered[a];
                            lblSelJB.Content = countEntered[a];
                            break;
                        case 1:
                            lblEJG.Content = nonEntered[a];
                            lblSelJG.Content = countEntered[a];
                            break;
                        case 2:
                            lblEIB.Content = nonEntered[a];
                            lblSelIB.Content = countEntered[a];
                            break;
                        case 3:
                            lblEIG.Content = nonEntered[a];
                            lblSelIG.Content = countEntered[a];
                            break;
                        case 4:
                            lblESB.Content = nonEntered[a];
                            lblSelSB.Content = countEntered[a];
                            break;
                        case 5:
                            lblESG.Content = nonEntered[a];
                            lblSelSG.Content = countEntered[a];
                            break;
                        default:
                            break;
                    }

                    a++;

                }// end for


                this.lblETotal.Content = nonEntered.Sum ( );
                this.lblSelTotal.Content = countEntered.Sum ( );
                lblGrandTotal.Content = grandTotal;
            }
            catch (Exception ex)
            {
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Error displaying selected Athlete counters" , ChampionshipSolutions.Diag.MessagePriority.Error );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message , ChampionshipSolutions.Diag.MessagePriority.Error );
            }
        }

        private void ImportEntryFormCSDB_Click ( object sender , RoutedEventArgs e )
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Championship Solutions Database (.csdb)|*.csdb";

            if ( openFileDialog.ShowDialog( ) != true )
                return;

            FileIO.FConnFile.ImportEntryForm( openFileDialog.FileName );
        }


        private void Button_Click ( object sender , RoutedEventArgs e )
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Data Set (.xml)|*.xml";

            if ( openFileDialog.ShowDialog( ) != true )
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load( openFileDialog.FileName );

            int current_Person_ID = 0;
            Athlete athlete = new Athlete();

            //CSDB context = ChampionshipSolutions.FileIO.FConnFile.getContext();

            foreach ( XmlElement row in doc.ChildNodes [ 1 ].ChildNodes [ 1 ].ChildNodes )
            {
                //if (current_Person_ID == 0) current_Person_ID = int.Parse(row.Attributes["People_ID"].Value);

                if ( current_Person_ID != int.Parse( row.Attributes [ "People_ID" ].Value ) )
                {

                    //FileIO.FConnFile.SaveChanges();

                    // new person
                    athlete = new Athlete( );

                    athlete.ID = int.Parse( row.Attributes [ "People_ID" ].Value );

                    current_Person_ID = athlete.ID;

                    athlete.FirstName = row.Attributes [ "FirstName" ].Value;
                    athlete.MiddleName = row.Attributes [ "MiddleName" ].Value;
                    athlete.LastName = row.Attributes [ "LastName" ].Value;

                    System.Diagnostics.Debug.WriteLine( "New Athlete: " + athlete.Fullname );

                    athlete.Gender = (Gender) int.Parse( row.Attributes [ "Gender" ].Value );

                    if ( !string.IsNullOrWhiteSpace( row.Attributes [ "DateOfBirth" ].Value ) )
                        athlete.DateOfBirth = DateTime.Parse( row.Attributes [ "DateOfBirth" ].Value );

                    if ( !string.IsNullOrWhiteSpace( row.Attributes [ "phoneNumber" ].Value ) )
                        athlete.AddContact( new PhoneContactDetail( ) { phoneNumber = row.Attributes [ "phoneNumber" ].Value } );

                    if ( !string.IsNullOrWhiteSpace( row.Attributes [ "SchoolCode" ].Value ) )
                    {
                        //athlete.Attends = context.Schools.ToArray( ).Where( s => s.ShortName == row.Attributes [ "SchoolCode" ].Value ).FirstOrDefault( );
                        athlete.Attends = FileIO.FConnFile.GetFileDetails().IO.GetAll<School>().ToArray( ).Where( s => s.ShortName == row.Attributes [ "SchoolCode" ].Value ).FirstOrDefault( );

                        if ( athlete.Attends == null )
                        {
                            System.Diagnostics.Debug.WriteLine( "Failed to find school: " + row.Attributes [ "School" ].Value );
                        }
                    }

                    FileIO.FConnFile.GetFileDetails( ).IO.Add<Person>( athlete );

                }

                PreviousResult pr = new PreviousResult(athlete, DateTime.Now);

                pr.Championship = row.Attributes [ "Championship" ].Value;
                pr.Venue = row.Attributes [ "ChampionshipLocation" ].Value;
                pr.Team = row.Attributes [ "Team" ].Value;
                pr.Event = row.Attributes [ "Event" ].Value;
                pr.Rank = row.Attributes [ "Rank" ].Value;

                if ( !string.IsNullOrWhiteSpace( row.Attributes [ "Value_RawValue" ].Value ) )
                {
                    pr.Event = row.Attributes [ "EventIfHeat" ].Value;

                    ResultValue rv = new ResultValue((ResultDisplayDescription)int.Parse(row.Attributes["Value_ValueType"].Value));
                    rv.setResult( int.Parse( row.Attributes [ "Value_RawValue" ].Value ) );

                    pr.ResultValue = rv.ToString( );
                }

                athlete.AddNote( pr );



                //< FIELD FieldName = "School" DisplayLabel = "School" FieldType = "String" FieldClass = "TField" />

                //< FIELD FieldName = "Event_ID" DisplayLabel = "Event_ID" FieldType = "Integer" FieldClass = "TField" />
                //< FIELD FieldName = "VestNumber" DisplayLabel = "VestNumber" FieldType = "String" FieldClass = "TField" />
                //< FIELD FieldName = "EventIDIfHeat" DisplayLabel = "EventIDIfHeat" FieldType = "Integer" FieldClass = "TField" />
                //< FIELD FieldName = "FinalEventID" DisplayLabel = "FinalEventID" FieldType = "Integer" FieldClass = "TField" />
                //< FIELD FieldName = "TypeDescriminator" DisplayLabel = "TypeDescriminator" FieldType = "Integer" FieldClass = "TField" />
                //< FIELD FieldName = "ContactName" DisplayLabel = "ContactName" FieldType = "String" FieldClass = "TField" />

            }

            //FileIO.FConnFile.SaveChanges( );

        }
    }
}
