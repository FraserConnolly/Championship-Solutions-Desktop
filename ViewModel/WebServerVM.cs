using ChampionshipSolutions.ControlRoom;
using ChampionshipSolutions.DM;
using ChampionshipSolutions.DM.ScriptInterfaces;
using ChampionshipSolutions.Reporting;
using ChampionshipSolutions.WebServices;
using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChampionshipSolutions.ViewModel
{
    public class WebServerVM : ObservableObject
    {

        #region Private Members

        private bool runStatus;
        private string status;
        private WebServer CSWebService;
        private string hostDirectory;
        private ChampionshipVM Championship;

        #endregion

        #region Public Properties

        public bool Enabled
        {
            get { return runStatus; }
            set
            {
                if (runStatus != value)
                {
                    if (value)
                        start();
                    else
                        stop();
                    RaisePropertyChanged("Enabled");
                }
            }
        }

        public int Port
        {
            get { return ((App)App.Current).CurrentChampionship.WebServerPort ; }
            set
            {
                if(CSWebService != null)
                    CSWebService.ShutDown();

                if(Enabled)
                    ConfigureWebService ( );


                RaisePropertyChanged ( "Port");
                RaisePropertyChanged ( "HostName" );
                RaisePropertyChanged ( "HttpIPAddresses" );
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    RaisePropertyChanged("Status");
                }
            }
        }

        public string HostDirectory
        {
            get
            {
                hostDirectory = Championship.getChampionshipWebDir();

                if (hostDirectory.EndsWith(@"\"))
                    return hostDirectory;
                else
                    return hostDirectory + @"\";
            }
        }

        public string HostName
        {
            get
            {
                var h = GetHostName();
                var p = Port;
                if ( h == "" )
                    return "";
                if ( Port != 80 )
                    return $"http://{h}:{Port}";
                return $"http://{h}";
            }
        }

        public string [] HttpIPAddresses { get
            {
                var s = GetAllIPs( );
                var p = Port;
                for ( int i = 0 ; i<s.Count() ;i++ )
                {
                    if ( p != 80 )
                        s [ i ] = $"http://{s [ i ]}:{Port}";
                    s [ i ] = $"http://{s [ i ]}";
                }

                return s;
            }
        }

        public static string GetHostName ()
        {
            try
            {
                var str =  Dns.GetHostName( );

                if ( string.IsNullOrWhiteSpace( str ) )
                    return "";

                return str;

            }
            catch ( Exception )
            {
                return "";
            }
        }


        public static string[] GetAllIPs ( )
        {
            List<string> addresses = new List<string>();
            try
            {
                IPHostEntry remoteIP;

                //using host name, get the IP address list..
                IPHostEntry ipEntry = Dns.GetHostEntry(GetHostName());
                IPAddress[] addr = ipEntry.AddressList;

                int i = 0;
                foreach ( var a in addr )
                {
                    Console.WriteLine( "IP Address {0}: {1} " , i , a.ToString( ) );
                    //HostNames
                    remoteIP = Dns.GetHostEntry( a );
                    Console.WriteLine( "HostName {0}: {1} " , i , remoteIP.HostName );
                    i++;
                    if ( a.GetAddressBytes().Count() == 4 )
                        addresses.Add( a.ToString( ) );
                }
                return addresses.ToArray();
            }
            catch ( Exception )
            {
                return addresses.ToArray();
            }
        }

        #endregion

        #region Commands

        public void start()
        {
            //throw new NotImplementedException();
            runStatus = false;

            //if (CSWebService == null)
                ConfigureWebService();

            if (runStatus)
            {
                //runStatus = true;
                Status = "Web Server is running";
            }
            //else
            //{
            //    //CSWebService.ShutDown();
            //    //runStatus = false;
            //    Status = "Web Server is not running";
            //}
        }

        bool CanStart()
        {
            if (!runStatus)
                return ((App)App.Current).CurrentChampionship.Championship.WebServerEnabled && (! string.IsNullOrWhiteSpace(HostDirectory));
            else
                return false;
        }

        public void stop()
        {
            if ( CSWebService != null )
            {
                CSWebService.DynamicFileRequestedEvent -= FileNeededEvent;
                CSWebService.ShutDown ( );
            }
            runStatus = false;
        }

        bool CanStop()
        {
            return (runStatus);
        }


        public ICommand Start { get { return new RelayCommand(start, CanStart); } }
        public ICommand Stop  { get { return new RelayCommand(stop , CanStop); } }

        #endregion

        #region Private Methods

        private void ConfigureWebService()
        {

            if (string.IsNullOrWhiteSpace(HostDirectory))
                return;

            CSWebService = new WebServer(
                HostDirectory,
                Championship.WebServerPort
                );

            // To do should this actually be pointing to the ChampionshipVM object?
            ScriptProcessor.addApplicationScopeObject(
                "Championship", ((App)App.Current).CurrentChampionship.Championship);

            ScriptProcessor.addApplicationScopeObject(
                "CS", ChampionshipSolution.getCS());

            ScriptProcessor.addApplicationScopeObject(
                "Report", ChampionshipSolutions.Reports.CSReportLibrary.getLibrary());

            CSWebService.DynamicFileRequestedEvent += FileNeededEvent; 

            runStatus = true;
        }

        private MemoryStream FileNeededEvent(object sender, DynamicFileRequestedEventArgs e)
        {

            if (e.FilePath.StartsWith(@"/exports/"))
            {
                string exportPath = ((App)Application.Current).CurrentChampionship.getChampionshipHomeDir(); // @"F:\Championship Solutions\Events\WSAA XC 2015-16\Testing Results\PDFs";

                if (File.Exists(exportPath + e.FullPath))
                {
                    return FileToMemoryStream( exportPath + e.FullPath );
                }
                else
                {
                    return null;
                }

            }
            else if (e.FilePath.StartsWith(@"/files/"))
            {
                //CSDB context = FileIO.FConnFile.getContext();

                int fileID = 0;

                if (int.TryParse(e.FileName.Trim( ".jpg".ToCharArray()), out fileID))
                {
                    FileStorage file = FileIO.FConnFile.GetFileDetails().IO.GetAll<FileStorage>().Where(f => f.ID == fileID).FirstOrDefault();

                    if (file == null) return null;

                    return new MemoryStream(file.FileData);
                }
            }
            else if (e.FilePath.StartsWith(@"/reports/"))
            {
                if ( e.FileName.StartsWith ( "report_" ) )
                {
                    string reportName = e.FileName.Remove(0, 7);
                    reportName = reportName.Replace ( ".pdf" , "" );

                    switch ( reportName.ToLower() )
                    {
                        case "eventcard":
                            if ( e.Arguments.AllKeys.Contains( "eventid" ) )
                            {
                                IScriptEvent Event = Championship.Championship.getEvent(int.Parse(e.Arguments["eventid"]));

                                if ( Event == null ) return null;

                                List<AEvent> evs = new List<AEvent>();
                                evs.Add ( (AEvent)Event );

                                string fileNames = Exports.GenerateResultEntryForms(evs,false,false,true) ;

                                if ( fileNames != null )
                                    return FileToMemoryStream ( fileNames );
                            }
                            break;
                        case "announcers":
                            if ( e.Arguments.AllKeys.Contains( "eventid" ) )
                            {
                            }
                            break;
                        case "athleteprofile":
                            if ( e.Arguments.AllKeys.Contains( "athleteid" ) )
                            {
                                Athlete athlete = Championship.Championship.getAthlete((int.Parse(e.Arguments["athleteid"])));

                                if ( athlete == null ) return null;

                                List<Athlete> evs = new List<Athlete>();
                                evs.Add ( (Athlete)athlete );

                                string fileNames = Exports.GenerateAthleteProfile(evs,false,PrintOptions.NO_PRINT,true) ;

                                if ( fileNames != null )
                                    return FileToMemoryStream ( fileNames );

                            }
                            break;
                    }
                }

            }
                #region Legacy Report Generation from WSAA XC 2016
            //else
            //{

                //if (e.FileName.StartsWith("report_"))
                //{
                //    string reportName = e.FileName.Remove(0, 7);
                //    reportName = reportName.TrimEnd((".pdf").ToCharArray());

                //    AReport rep;

                //    switch (reportName)
                //    {
                //        case "vest":

                //            rep = ReportLibrary["XCVestNumber"];

                //            if (e.Arguments.ContainsKey("vest"))
                //            {
                //                string tempFile = new Print().generateSingleVestNumberPDF(
                //                MainWindow.CurrentChampionship.getCompetitor(e.Arguments["vest"]).FirstOrDefault()
                //                , "");

                //                if (tempFile == null) return null;

                //                using (FileStream file = new FileStream(tempFile, FileMode.Open, System.IO.FileAccess.Read))
                //                {
                //                    MemoryStream ms = new MemoryStream();

                //                    file.CopyTo(ms);

                //                    return new MemoryStream(ms.ToArray());
                //                }

                //                //File.Delete(tempFile);
                //            }
                //            break;
                //        case "results":
                //            if (e.Arguments.ContainsKey("eventid"))
                //            {
                //                IScriptEvent Event = MainWindow.CurrentChampionship.getEvent(int.Parse(e.Arguments["eventid"]));

                //                if (Event == null) return null;

                //                List<AEvent> evs = new List<AEvent>();
                //                evs.Add((AEvent)Event);

                //                string[] fileNames = new Print().printResults(evs, false);

                //                if (fileNames.Length == 1)
                //                    return FileToMemoryStream(fileNames[0]);
                //            }
                //            break;

                //        case "teamresults":
                //            if (e.Arguments.ContainsKey("eventid"))
                //            {
                //                IScriptEvent Event = MainWindow.CurrentChampionship.getEvent(int.Parse(e.Arguments["eventid"]));

                //                if (Event == null) return null;

                //                List<AEvent> evs = new List<AEvent>();
                //                evs.Add((AEvent)Event);

                //                string[] fileNames = new Print().printTeamResults(evs, false);

                //                if (fileNames.Length == 1)
                //                    return FileToMemoryStream(fileNames[0]);
                //            }

                //            break;
                //        default:
                //            return null;
                //    } // end switch


                //} // end if report
            //}
            #endregion
            return null;

        }

        private MemoryStream FileToMemoryStream(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            MemoryStream ms = new MemoryStream( );
            file.CopyTo( ms );

            // I don't understand why I have to make a second MemoryStream object
            // but it is essential! 2019-01-19
            return new MemoryStream(ms.ToArray());
        }

        #endregion

        #region Constructors

        public WebServerVM(ChampionshipVM Championship)
        {
            this.Championship = Championship;
        }

        #endregion
    }
}
