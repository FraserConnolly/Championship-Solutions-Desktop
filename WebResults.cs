using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;
using System.Data;
using ChampionshipSolutions.DM;
using System.Windows;
using Microsoft.Win32;

namespace ChampionshipSolutions
{
    public partial class ImportResultsPage
    {
        private readonly TimeSpan MINWAITTIME = new TimeSpan(0, 0, 0, 1, 0);          // 1 second
        private readonly TimeSpan MAXWAITTIME = new TimeSpan(0, 0, 3, 0, 0);          // 3 minutes
        private readonly TimeSpan ADDITIONALWAITTIME = new TimeSpan(0, 0, 0, 10, 0);  // 10 seconds
        private readonly TimeSpan DEFAULTTICK = new TimeSpan(0, 0, 0, 1);

        protected DispatcherTimer timmer;
        private int elapsedTimmer = 0;
        private TimeSpan waitInterval = new TimeSpan(0, 0, 0, 1, 0);

        private List<VestActions> rawResults = new List<VestActions>();

        // used by processVestAction to determine if a VestAction was successfully processed.
        private bool isProcessed;

        protected void timmer_Tick(object sender, EventArgs e)
        {

            elapsedTimmer++;
            refreshWaitBar(waitInterval.TotalSeconds, timeToNextCheck);
            //System.Diagnostics.Debug.WriteLine("WebGet Timer Tick: " + elapsedTimmer.ToString() + " " + timeToNextCheck.ToString());

            int itemsProcessed = 0;

            if (elapsedTimmer >= waitInterval.TotalSeconds)
            {
                itemsProcessed = processVestAction();
                elapsedTimmer = 0;

                if (itemsProcessed == -1)
                {
                    // A processing error has occurred, stop the thread.
                    stop();
                    return;
                }

                if (itemsProcessed > 0)
                    // Something happened so check again soon
                    waitInterval = MINWAITTIME;

                if (itemsProcessed == 0 && waitInterval < MAXWAITTIME)
                    // Nothing happened so hold back for longer before checking again.
                    waitInterval += ADDITIONALWAITTIME;

                resetTimer();
            }

        }

        private void resetTimer()
        {
            stop();
            start();
        }

        private void getRawResultsData()
        {
            rawResults = new List<VestActions>();

            //webGetData getData = new webGetData("VestActions.aspx");

            //DataSet ds = getData.getDataSet();

            // Load XML File from OneDrive Local directory

            string ChampionshipName = ((App)App.Current).CurrentChampionship.Name;
          //string fileName = @"D:\OneDrive\Championship Solutions\" + ChampionshipName + "_Results.xml";
            string onedrivePath = @"C:\Users\Frase\OneDrive";
            string tempPath = @"\Championship Solutions\"+ ChampionshipName + "_Results.xml";
            string fileName = onedrivePath + tempPath ;

            try
            {
                string t;

                const string userRoot = "HKEY_CURRENT_USER";
                //const string subkey =@"Software\Microsoft\CurrentVersion\SkyDrive";
                const string subkey =@"Software\SyncEngines\Providers\OneDrive\Personal";
                const string keyName = userRoot + "\\" + subkey;

                t = (string)Registry.GetValue ( keyName , "MountPoint" , fileName );

                if ( t == null )
                    throw new Exception ( "Not onedrive path found" );

                if ( t != onedrivePath )
                    fileName = t + tempPath;
            }
            catch
            { }

            if ( System.IO.File.Exists( fileName ) )
            {

                DataSet ds = new DataSet();
                ds.ReadXml(fileName);

                if ( ds.Tables.Count == 1 )
                    foreach ( DataRow dr in ds.Tables[0].Rows )
                    {
                        VestActions va = dataRowToVestAction ( dr );

                        if (va != null )
                            rawResults.Add ( va );
                    }
            }
        }

        private int processVestAction ( )
        {
            getRawResultsData ( );

            Championship CChampionship = ((App)App.Current).CurrentChampionship.Championship;

            int c = 0;

            foreach ( VestActions va in rawResults.Where ( va => CChampionship.VestActions.ToList ( ).Contains ( va ) == false && va.ChampionshipName == CChampionship.Name ) )
            {
                // this VA has not been actioned 

                isProcessed = false;

                switch ( va.Action )
                {
                    case VestActionDescriptions.InsertResultVest:
                        InsertResult ( va );
                        break;
                    case VestActionDescriptions.DeleteResultVest:
                        DeleteResult ( va );
                        break;
                    case VestActionDescriptions.StartEvent:
                        throw new NotImplementedException ( );
                    case VestActionDescriptions.FinishEvent:
                        throw new NotImplementedException ( );
                    case VestActionDescriptions.Unknown:
                        break;
                    case VestActionDescriptions.InsertPlaceHolder:
                        InsertPlaceholderResult ( va );
                        break;
                    case VestActionDescriptions.DeletePlaceHolder:
                        DeletePlaceholderResult ( va );
                        break;
                }

                if ( isProcessed )
                {
                    // this VA has now been actioned so add it to the processed set
                    CChampionship.AddVestAction ( va );
                    c++;
                }
                else
                {

                    //MainWindow.SQLiteSubmit ( );
                    //MainWindow.Context.SaveChanges();
                    if ( MessageBox.Show ( "A VestAction could not be processed, please resolve this problem before continuing." + Environment.NewLine + "Do you want to permanently ignore this result?" , "Vest Action Error" , MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
                    {
                        // ignore this entry
                        va.Ignored = true;
                        CChampionship.AddVestAction ( va );
                        va.Championship = CChampionship;
                        va.Save ( );
                        //MainWindow.SQLiteSubmit();
                        refreshUI ( );
                    }
                    else
                    {
                        //MainWindow.SQLiteSubmit();
                        refreshUI ( );
                        return -1;
                    }
                }
            }

            if ( c > 0 )
            {
                //MainWindow.SQLiteSubmit();
                refreshUI ( );
            }

            return c;
        }

        private void DeleteResult(VestActions va)
        {
            if (va.Vest == null)
                return;

            Championship CChampionship = ((App)App.Current).CurrentChampionship.Championship;

            List<ACompetitor> Competitors = CChampionship.getCompetitor(va.Vest);

            if (Competitors.Count == 0)
            {
                // Competitor could not be found
                // We don't have to hold here. 
                // We will assume this is an error on the data input part and ignore this request.
                //isProcessed = true;
            }
            else if (Competitors.Count==1)
            {
                // one competitor was found
                throw new NotImplementedException();
            }
            else if (Competitors.Count > 1)
            {
                // more than one competitor was found
                throw new NotImplementedException();
            }
        }

        // added so that a manual entry of a vest number could take place
        // 2017-12-16
        private void InsertResult ( string Vest )
        {
            var va = new VestActions ( ) { Vest = Vest, Description = "InsertResultVest"  };

            if ( InsertResult( va ) )
            {
                // processed 
                Championship CChampionship = ((App)App.Current).CurrentChampionship.Championship;
                CChampionship.AddVestAction( va );
            }
            else
            {
                MessageBox.Show( "Failed to insert result" );
            }
        }

        private bool InsertResult(VestActions va)
        {
            if (va.Vest == null)
                return false;
            Championship CChampionship = ((App)App.Current).CurrentChampionship.Championship;

            List<ACompetitor> Competitors = CChampionship.getCompetitor(va.Vest);

            if (Competitors.Count == 0)
            {
                // Competitor could not be found
                // We must hold here until a vest has been entered or this VestAction has been overwritten 
                // (added to the processed set)

                if (va.Vest == "001")
                {
                    va.EventCode = "MB";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format( "Placeholder for {0}", va.EventCode );
                    va.Save ( );
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode); 
                    return true;
                }

                if (va.Vest == "002")
                {
                    va.EventCode = "MG";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format("Placeholder for {0}", va.EventCode);
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode);
                    va.Save ( );

                    return true;
                }

                if (va.Vest == "003")
                {
                    va.EventCode = "JB";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format("Placeholder for {0}", va.EventCode);
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode);
                    va.Save ( );

                    return true;
                    
                }

                if (va.Vest == "004")
                {
                    va.EventCode = "JG";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format("Placeholder for {0}", va.EventCode);
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode);
                    va.Save ( );
                    return true;
                }

                if (va.Vest == "005")
                {
                    va.EventCode = "IB";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format("Placeholder for {0}", va.EventCode);
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode);
                    va.Save ( );
                    return true;
                }

                if (va.Vest == "006")
                {
                    va.EventCode = "IG";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format("Placeholder for {0}", va.EventCode);
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode);
                    va.Save ( );
                    return true;
                }

                if (va.Vest == "007")
                {
                    va.EventCode = "SB";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format("Placeholder for {0}", va.EventCode);
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode);
                    va.Save ( );
                    return true;
                }

                if (va.Vest == "008")
                {
                    va.EventCode = "SG";
                    InsertPlaceholderResult(va);
                    va.statusDescription = string.Format("Placeholder for {0}", va.EventCode);
                    MessageBox.Show("A placeholder has been entered for " + va.EventCode);
                    va.Save ( );
                    return true;
                }

                va.statusDescription = string.Format("A competitor could not be found for vest {0}", va.Vest );
                MessageBox.Show("A competitor could not be found for vest " + va.Vest);
                return false;

            }
            else if (Competitors.Count == 1)
            {
                // one competitor was found
                try
                {
                    Competitors[0].CompetingIn.AddResult(Competitors[0]);
                    va.statusDescription = string.Format("Competitor Added: {0} {1}", Competitors[0].getName(),Competitors[0].CompetingIn.Name);
                    va.Save ( );

                    isProcessed = true;
                    return true;

                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrWhiteSpace(va.statusDescription))
                        va.statusDescription += string.Format("Error State: {0}", ex.Message);
                    else
                        va.statusDescription = string.Format("Error State: {0}", ex.Message);
                    MessageBox.Show("Failed to insert a vest" + va.Vest + "because: " + ex.Message);   
                    va.Save ( );
                    return false;
                }
            }
            else if (Competitors.Count > 1)
            {
                // more than one competitor was found
                va.statusDescription = "Error State: More than one competitor was returned";
                MessageBox.Show("More than one competitor was found for vest " + va.Vest);
                va.Save ( );
                return false;
            }
            return false;
        }

        private void InsertPlaceholderResult(VestActions va)
        {
            if (string.IsNullOrWhiteSpace( va.EventCode))
                return;

            Championship CChampionship = ((App)App.Current).CurrentChampionship.Championship;

            AEvent Event = CChampionship.listAllEvents().Where(e => e.ShortName == va.EventCode).FirstOrDefault();

            if (Event == null)
                // do nothing as the event could not be found so ignore this VestAction
                return;

            try
            {
                Event.AddPlaceholderResult();
                isProcessed = true;
            }
            catch (Exception)
            {
                throw new ArgumentException("Failed to enter placeholder result for: " + Event.Name); 
            }
        }

        private void DeletePlaceholderResult(VestActions va)
        {
            if (string.IsNullOrWhiteSpace(va.EventCode))
                return;

            Championship CChampionship = ((App)App.Current).CurrentChampionship.Championship;

            AEvent Event = CChampionship.listAllEvents().Where(e => e.ShortName == va.EventCode).FirstOrDefault();

            if (Event == null)
                // do nothing as the event could not be found so ignore this VestAction
                return;
            
            throw new NotImplementedException();
        }

        public bool isRunning { get { return timmer.IsEnabled; } }

        // in seconds
        public int timeToNextCheck { get { return (waitInterval.TotalSeconds - elapsedTimmer) > 0 ? ((int)waitInterval.TotalSeconds - elapsedTimmer) : 0; } }
        
        // 2016 Method of converting XML results to Vest Action
        public static VestActions dataRowToVestAction(DataRow dr)
        {
            VestActions va = new VestActions();
            try
            {
                va.WebID = Convert.ToInt32(getStringSafe(dr, "ID"));
                va.Description = (string)dr["Description"];
                va.DateStamp = DateTime.Now; // DateTime.Parse((string)dr["TimeStamp"]);
                va.Vest = getStringSafe(dr, "Vest");
                va.ChampionshipName = (string)dr["Championship"];
                va.EventCode = getStringSafe(dr, "EventCode");
                va.Position = Convert.ToInt32(getStringSafe(dr, "Position"));
                string time = getStringSafe(dr, "Time");
                if (!string.IsNullOrWhiteSpace(time)) va.Time = TimeSpan.Parse(time);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to generate vest from DataRow");
                System.Diagnostics.Debug.WriteLine("\t" + ex.Message);
                return null;
            }

            return va;
        }


        public static string getStringSafe(DataRow dr, string pararmater)
        {
            if (dr == null)
                return null;
            try
            {
                if (dr[pararmater] != null)
                    return dr[pararmater].ToString();
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
