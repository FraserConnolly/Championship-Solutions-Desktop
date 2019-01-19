using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChampionshipSolutions.Reporting;
using ChampionshipSolutions.DM;
using ChampionshipSolutions.Reports;
using ChampionshipSolutions.DM.ScriptInterfaces;
using System.Diagnostics;
using System.Windows;
using System.Threading;

namespace ChampionshipSolutions
{
    public class ChampionshipSolution : IScriptApplication
    {
        #region Singleton Constructor

        private static ChampionshipSolution _instance;

        private ChampionshipSolution ( ) { }

        public static ChampionshipSolution getCS ( )
        {
            if ( _instance == null )
                _instance = new ChampionshipSolution ( );
            return _instance;
        }

        #endregion

        public Championship getCurrentChampionship ( )
        {
            return ( (App)App.Current ).CurrentChampionship.Championship;
        }

        [Obsolete ( "Can not use this save method any longer" , true )]
        public void saveChangesToDatabase ( )
        {
            MessageBox.Show ( "Web server tried to save" );
            //!*!
            //FileIO.FConnFile.SaveChanges();
        }

        public CSReportLibrary getReportLibrary ( )
        {
            return CSReportLibrary.getLibrary ( );
        }

        public ACompetitor getCompetitorByID ( int id )
        {
            //return MainWindow.Context.Competitors.ToList().Where(c => c.ID == id).FirstOrDefault();
            //return FileIO.FConnFile.getContext().Competitors.ToList().Where(c => c.ID == id).FirstOrDefault();
            //return FileIO.FConnFile.GetFileDetails().IO.GetAll<ACompetitor>().ToList().Where(c => c.ID == id).FirstOrDefault();
            return FileIO.FConnFile.GetFileDetails ( ).IO.GetID<ACompetitor> ( id );
        }

        public void AddResult ( int eventid , int rank , string vest , string value )
        {
            Championship championship = getCurrentChampionship();
            IScriptEvent Event = championship.getEvent(eventid);

            ResultValue rv = AEvent.MakeNewResultsValue((AEvent)Event);
            rv.setResultString ( System.Web.HttpUtility.UrlDecode ( value ) );
            //!**!
            Event.AddResult ( rank , VestNumber.MakeFromString ( System.Web.HttpUtility.UrlDecode ( vest ) ) , rv );
            //saveChangesToDatabase();
        }

        public void RemoveResult ( int eventid , int rank )
        {
            Championship championship = getCurrentChampionship();
            IScriptEvent Event = championship.getEvent(eventid);

            Event.removeResult ( rank );
            //!**!
            //saveChangesToDatabase();
        }

        public void SetValue ( int eventid , int rank , string value )
        {
            Championship championship = getCurrentChampionship();
            IScriptEvent Event = championship.getEvent(eventid);

            AResult result = ((AEvent)Event).getResult(rank);

            if ( result == null )
                return;

            ResultValue rv = AEvent.MakeNewResultsValue((AEvent)Event);
            rv.setResultString ( System.Web.HttpUtility.UrlDecode ( value ) );


            result.Value = rv;
            result.Save ( );
            //!**!
            //saveChangesToDatabase();
        }

        public string[] OpenCert ( int eventid , string type )
        {
            Championship championship = getCurrentChampionship();
            IScriptEvent Event = championship.getEvent(eventid);

            List<string> tempFiles = new List<string>();

            Stopwatch sw1 = new Stopwatch();
            sw1.Start ( );

            foreach ( string s in GenerateTempCertificates ( (AEvent)Event , type ) )
                tempFiles.Add ( @"\exports\" + System.IO.Path.GetFileName ( s ) );

            sw1.Stop ( );
            return tempFiles.ToArray ( );
        }

        public string[] OpenCert ( int AthleteID )
        {
            List<string> tempFiles = new List<string>();

            foreach ( string s in PrintCertificates.SaveIndividualCertificates (
                ACertificate.GenerateCertificates ( getAthlete ( AthleteID ) , getCurrentChampionship ( ) ) ,
                //AppDomain.CurrentDomain.BaseDirectory + @"WebPages\PDFs\").ToArray())
                ( (App)Application.Current ).CurrentChampionship.getChampionshipExportsDir ( ) ).ToArray ( ) )
                tempFiles.Add ( @"\exports\" + System.IO.Path.GetFileName ( s ) );

            return tempFiles.ToArray ( );
        }

        /// <summary>
        /// Written to produce certificates that were not collected on the day. 2017-01-22
        /// </summary>
        /// <returns></returns>
        public string [] CustomOpenCert ( )
        {

            List<CertificateData> certs = new List<CertificateData>();

            //foreach ( ACompetitor a in getCurrentChampionship().ListAllCompetitors().OrderBy(x => x.checkParameter("School")).ToList() )
            //{
            //    if ( a.Result == null ) continue ;

            //    if ( a.result.rank <= 13 ) continue;

            //    if ( a.Result.CertificateEarned )
            //        certs.AddRange ( ACertificate.GenerateCertificates( a ) );
            //}

            foreach ( AEvent Event in getCurrentChampionship().listAllEvents() )
            {
                certs.AddRange ( Event.getCertificateData ( ) ) ;
            }

            List<string> tempFiles = new List<string>();

            tempFiles.AddRange ( PrintCertificates.SaveIndividualCertificates ( certs ,
                ( (App)Application.Current ).CurrentChampionship.getChampionshipExportsDir ( ) ) );

            Exports.MergeMultiplePDFIntoSinglePDF ( ( (App)Application.Current ).CurrentChampionship.getChampionshipExportsDir ( )  + "\\All Certs.pdf" , tempFiles.ToArray ( ) );

            return tempFiles.ToArray ( );
        }

        /// <param name="Type">Can be "Team", "Top Individuals" or "Top Lower Year Group"</param>
        /// <returns>Array of file paths</returns>
        public string[] GenerateTempCertificates ( AEvent Event , String Type , bool Print = false , bool Open = false , string ExportPath = "" )
        {

            if ( Event == null )
                return new string[] { };

            List<string> files;

            if ( string.IsNullOrWhiteSpace ( ExportPath ) )
                files =
                    PrintCertificates.SaveAllEventCertificatesByType (
                        ACertificate.GenerateCertificates ( Event ) ,
                        ( (App)Application.Current ).CurrentChampionship.getChampionshipExportsDir ( ) ,
                        new AEvent[] { Event } );
            else
                files =
                    PrintCertificates.SaveAllEventCertificatesByType (
                        ACertificate.GenerateCertificates ( Event ) ,
                        ExportPath ,
                        new AEvent[] { Event } );

            // Why am i trying to save here anyway
            // 2016-05-29 !**!
            //saveChangesToDatabase();

            if ( Print )
                files.Where ( s => s.Contains ( Type ) ).ToList ( ).ForEach ( file => Printing.PrintPDF ( file ) );
            else if ( Open )
                files.Where ( s => s.Contains ( Type ) ).ToList ( ).ForEach ( file => Process.Start ( file ) );

            if ( string.IsNullOrWhiteSpace ( Type ) )
                return files.ToArray ( );
            else
                return files.Where ( s => s.Contains ( Type ) ).ToArray ( );
        }

        public Athlete getAthlete ( int id )
        {
            return getCurrentChampionship ( ).ListAllAthletes ( ).Where ( a => a.ID == id ).FirstOrDefault ( );
        }

        public string[] getSchools ( int TeamID )
        {
            Team t = getCurrentChampionship().listAllTeams().Where(team => team.ID == TeamID).FirstOrDefault();

            if ( t == null ) return new string[] { };

            return ( from s in t.HasSchools select s.Name ).ToArray ( );
        }

        [Obsolete ( "I have pulled athlete creation away from the web page" , true )]
        public int SetAthlete ( int athleteid , int teamid , string name , string gender , string school , string dob )
        {
            if ( athleteid == 0 )
            {     // new athlete
                if ( teamid == 0 ) return 0;
                if ( name == "" ) return 0;
                if ( school == "" ) return 0;
                if ( dob == "" ) return 0;

                Athlete ath = new StudentAthlete();

                ath.setFullName ( System.Web.HttpUtility.UrlDecode ( name ) );

                if ( gender == "Male" )
                    ath.Gender = Gender.Male;
                else if ( gender == "Female" )
                    ath.Gender = Gender.Female;
                else
                    ath.Gender = Gender.NotStated;

                Team team = getCurrentChampionship().Teams.ToList().Where(t => t.ID == teamid).FirstOrDefault();

                ath.setTeam ( team , getCurrentChampionship ( ) );

                School sch = (from s in team.HasSchools.ToList() where s.Name == System.Web.HttpUtility.UrlDecode(school) select s).FirstOrDefault();

                if ( sch == null ) return 0;

                ( (StudentAthlete)ath ).Attends = sch;

                if ( !string.IsNullOrEmpty ( dob ) )
                {
                    DateTime.TryParse( dob , out DateTime DoB );

                    ath.DateOfBirth = DoB;
                }
                else return 0;

                //MainWindow.Context.People.InsertOnSubmit(ath);
                //FileIO.FConnFile.getContext().People.InsertOnSubmit(ath);
                //!*!
                FileIO.FConnFile.GetFileDetails ( ).IO.Add<Person> ( ath );

                saveChangesToDatabase ( );

                return ath.ID;
            }
            else
            {
                // existing athlete

                // todo

                Athlete origonalAthlete = getAthlete(athleteid);

                if ( origonalAthlete == null ) return 0;

                if ( origonalAthlete.Fullname != System.Web.HttpUtility.UrlDecode ( name ) )
                    origonalAthlete.setFullName ( System.Web.HttpUtility.UrlDecode ( name ) );

                School sch = (from s in origonalAthlete.getTeam(getCurrentChampionship()).HasSchools.ToList() where s.Name == System.Web.HttpUtility.UrlDecode(school) select s).FirstOrDefault();

                if ( sch == null ) return 0;

                if ( origonalAthlete is StudentAthlete )
                    ( (StudentAthlete)origonalAthlete ).Attends = sch;

                if ( !string.IsNullOrEmpty ( dob ) )
                {
                    DateTime.TryParse( dob , out DateTime DoB );

                    if ( origonalAthlete.DateOfBirth != DoB )
                        origonalAthlete.DateOfBirth = DoB;
                }
                else return 0;

                saveChangesToDatabase ( );

                return athleteid;
            }
        }

        public int SetAthleteName ( int athleteid , string name , string preferredName )
        {
            if ( athleteid == 0 )
                return 0;

            // existing athlete

            Athlete Athlete = getAthlete(athleteid);

            if ( Athlete == null ) return 0;

            string origonalName = Athlete.PreferredName;

            if ( Athlete.Fullname != System.Web.HttpUtility.UrlDecode ( name ) )
                Athlete.setFullName ( System.Web.HttpUtility.UrlDecode ( name ) );

            if ( origonalName != System.Web.HttpUtility.UrlDecode ( preferredName ) )
                Athlete.PreferredName = ( System.Web.HttpUtility.UrlDecode ( preferredName ) );

            Athlete.Save ( );

            return athleteid;
        }


        public Athlete[] findAthletes ( int TeamID = 0 , string nameFilter = "" )
        {
            try
            {
                if ( TeamID == 0 && string.IsNullOrWhiteSpace ( nameFilter ) )
                {
                    //return MainWindow.Context.People.ToList().Where(a => a is AAthlete).Select(a => (AAthlete)a).ToArray<AAthlete>();

                    return getCurrentChampionship ( ).ListAllAthletes ( ).ToArray ( );
                }
                if ( TeamID == 0 && !string.IsNullOrWhiteSpace ( nameFilter ) )
                    return getCurrentChampionship ( ).ListAllAthletes ( ).Where ( a => a.Fullname.Contains ( nameFilter ) ).ToArray ( );

                if ( TeamID != 0 && string.IsNullOrWhiteSpace ( nameFilter ) )
                    return getCurrentChampionship ( ).listAllTeams ( ).Where ( t => t.ID == TeamID ).FirstOrDefault ( ).getAllAthletes ( ).ToArray ( );

                if ( TeamID != 0 && !string.IsNullOrWhiteSpace ( nameFilter ) )
                    return getCurrentChampionship ( ).listAllTeams ( ).Where ( t => t.ID == TeamID ).FirstOrDefault ( ).getAllAthletes ( ).Where ( a => a.Fullname.Contains ( nameFilter ) ).ToArray ( );
            }
            catch
            {
            }

            return new Athlete[] { };
        }

        /// <summary>
        /// Promotes or demotes a competitor from a final.
        /// </summary>
        /// <param name="CompetitorID"></param>
        public void Promote ( int CompetitorID )
        {
            ACompetitor c = getCompetitorByID(CompetitorID);

            if ( c.CompetingIn is IndividualTimedFinalEvent )
            {
                if ( ( (IHeatedCompetitor)c ).isInFinal ( ) )
                    ( (IHeatedCompetitor)c ).demoteFromFinal ( );
                else
                    ( (IndividualTimedFinalEvent)c.CompetingIn ).promoteCompetitorToFinal ( (IHeatedCompetitor)c );
            }
        }

        public List<ChampionshipTeamResult> getOverallResults ( )
        {
            List<ChampionshipTeamResult> temp = new List<ChampionshipTeamResult>();

            Dictionary<Group[], int> AgeGroups = new Dictionary<Group[],int>();

            if ( getCurrentChampionship( ).Groups.Where( g => g.Name == "Minor" ).Count( ) > 0 )
            {
                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Minor").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Boys").First() } , 0 );

                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Minor").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Girls").First() } , 0 );
            }

            if ( getCurrentChampionship( ).Groups.Where( g => g.Name == "Junior" ).Count( ) > 0 )
            {
                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Junior").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Boys").First() } , 0 );

                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Junior").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Girls").First() } , 0 );
            }

            if ( getCurrentChampionship( ).Groups.Where( g => g.Name == "Intermediate" ).Count( ) > 0 )
            {
                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Intermediate").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Boys").First() } , 0 );

                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Intermediate").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Girls").First() } , 0 );
            }

            if ( getCurrentChampionship( ).Groups.Where( g => g.Name == "Senior" ).Count( ) > 0 )
            {
                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Senior").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Boys").First() } , 0 );

                AgeGroups.Add( new Group[] { getCurrentChampionship().Groups.Where( g => g.Name=="Senior").First(),
                getCurrentChampionship().Groups.Where( g => g.Name=="Girls").First() } , 0 );
            }

            foreach ( Team t in getCurrentChampionship ( ).Teams )
                foreach ( Group[] groups in AgeGroups.Keys.ToArray ( ) )
                    AgeGroups[groups] = getCurrentChampionship ( ).getOverallSores ( groups ).Where ( scores => scores.ScoringTeamName == "A" && t == scores.Team ).Select ( c => c.Points ).First ( );

            foreach ( Group[] groups in AgeGroups.Keys )
                temp.AddRange ( getCurrentChampionship ( ).getOverallSores ( groups ).Where ( scores => scores.ScoringTeamName == "A" ) );

            temp = temp.OrderBy ( ctr => ctr.Team.Name ).ToList ( );

            return temp;

        }

        #region Notes

        public void SetAvailability ( int AthleteID , string Championship , bool Available , string Transport , string PreferredEvent , string PersonalBest )
        {
            if ( AthleteID == 0 )
                return;

            // existing athlete

            Athlete Athlete = getAthlete(AthleteID);

            if ( Athlete == null ) return;

            DeclaredAvailibilityInformation info = new DeclaredAvailibilityInformation(Athlete , DateTime.Now);

            info.Championship = System.Web.HttpUtility.UrlDecode ( Championship );
            info.Availability = Available ? "Available" : "Not Available";
            info.TransportMethod = System.Web.HttpUtility.UrlDecode ( Transport );
            info.PreferredEvent = System.Web.HttpUtility.UrlDecode ( PreferredEvent );
            info.PersonalBest = System.Web.HttpUtility.UrlDecode ( PersonalBest );
            //info.EnteredDate = System.DateTime.Now;

            Athlete.AddNote ( info );
        }

        public void SetNote ( int AthleteID , string Key , string Note )
        {
            if ( AthleteID == 0 )
                return;

            // existing athlete

            Athlete Athlete = getAthlete(AthleteID);

            if ( Athlete == null ) return;

            PublicNote note = new PublicNote(Athlete, DateTime.Now);

            note.Key = System.Web.HttpUtility.UrlDecode ( Key );
            note.Note = System.Web.HttpUtility.UrlDecode ( Note );

            // only add a note that either has a Key, a Note or Both.
            if ( !( string.IsNullOrWhiteSpace ( note.Key ) && string.IsNullOrWhiteSpace ( note.Note ) ) )
                Athlete.AddNote ( note );
            else
            {
                //hmm
            }

        }





        #endregion

        #region Printing
 
        public void Print ( int AthleteID, string Files, string Printer )
        {
            Athlete Athlete = getAthlete((int)AthleteID);

            if ( Athlete == null ) return;

            List<Athlete> athletes = new List<Athlete>() { Athlete };

            List<string> tempFiles = new List<string>();


            foreach ( string s in PrintCertificates.SaveIndividualCertificates (
                ACertificate.GenerateCertificates ( Athlete , getCurrentChampionship ( ) ) ,
                ( (App)Application.Current ).CurrentChampionship.getChampionshipExportsDir ( ) ).ToArray ( ) )
                    Printing.PrintPDF ( s , Printer );

            PrintAthleteNotes ( AthleteID , Printer );

        }

        public void PrintAthleteNotes ( int AthleteID , string Printer = "A5-1" )
        {
            Thread t = new Thread( new ParameterizedThreadStart ( PrintAthleteNotesT ));

            t.Start ( new Tuple<int , string> ( AthleteID , Printer ) );
        }
        private void PrintAthleteNotesT ( object obj ) //Tuple < int, string > tuple ) // AthleteID, string Printer = "A5-1" )
        {
            // FC 2017-01-11 To Do
            Tuple<int,string> tuple = (Tuple<int,string>)obj;

            int AthleteID = tuple.Item1;
            string Printer = tuple.Item2;

            if ( AthleteID == 0 )
                return;

            // existing athlete

            Athlete Athlete = getAthlete((int)AthleteID);

            if ( Athlete == null ) return;

            List<Athlete> athletes = new List<Athlete>() { Athlete };

            PrintOptions po;

            switch ( Printer )
            {
                case "A5-1":
                    po = PrintOptions.A5_1;
                    break;
                case "A5-2":
                    po = PrintOptions.A5_2;
                    break;
                case "PDF":
                    po = PrintOptions.PDF;
                    break;
                default:
                    po = PrintOptions.NO_PRINT;
                    break;
            }

            try
            {
                Exports.GenerateAthleteProfile ( athletes , false , po , false , true );
            }
            catch // ( Exception ex )
            {
                //MessageBox.Show ( "Failed to open athletes profile sheets. \n" + ex.Message );
                return;
            }

        }


        #endregion

        #region Contacts

        public void AddPhoneNumber( int AthleteID, string PhoneNumber )
        {
            if ( string.IsNullOrWhiteSpace ( PhoneNumber ) ) return;

            if ( AthleteID == 0 )
                return;

            // existing athlete

            Athlete Athlete = getAthlete(AthleteID);

            if ( Athlete == null ) return;

            Athlete.AddContact( new PhoneContactDetail ( ) { phoneNumber = System.Web.HttpUtility.UrlDecode ( PhoneNumber ) } );
        }

        public void AddEmail ( int AthleteID , string EmailAddress )
        {
            if ( string.IsNullOrWhiteSpace ( EmailAddress ) ) return;

            if ( AthleteID == 0 )
                return;

            // existing athlete

            Athlete Athlete = getAthlete(AthleteID);

            if ( Athlete == null ) return;

            Athlete.AddContact ( new EmailContactDetail ( ) { EmailAddress = System.Web.HttpUtility.UrlDecode ( EmailAddress ) } );

        }

        public void DeleteContact ( int AthleteID , int contactid )
        {
            if ( AthleteID == 0 )
                return;

            // existing athlete

            Athlete Athlete = getAthlete(AthleteID);

            if ( Athlete == null ) return;

            AContactDetail detail = Athlete.getAllContacts().Where( c => c.ID == contactid).First();

            Athlete.removeContact ( detail );       
        }

        #endregion
    }
}
