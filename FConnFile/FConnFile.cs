using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using ChampionshipSolutions.DM;
using System.Data.SQLite;
using static ChampionshipSolutions.DM.FConnFileHelper;
using System.IO;
using System.Diagnostics;
using ChampionshipSolutions.ViewModel;

namespace ChampionshipSolutions.FileIO
{

    public static class FConnFile
    {

        public struct FileDetails
        {
            private DM.DataAccess.Database _IO;

            public string FilePath { get; set; }
            public SQLiteConnection Connection { get; set; }
            public bool isOpen { get; set; }
            public FConnFileFormat FileFormat { get; set; }
            public FConnFileState FileState { get; set; }
            public DM.DataAccess.Database IO
            {
                get
                {
                    if ( _IO == null )
                        // try to create a context
                        if ( Connection != null )
                        {
                            _IO = new DM.DataAccess.Database ( Connection );
                        }
                        else
                            // Can not make a context without a connection
                            return null;

                    return _IO;
                }
                internal set
                {
                    _IO = value;
                }
            }
        }

        #region Initialiser

        static FConnFile ()
        {
            primaryFile = new FileDetails();
            secondaryFile = new FileDetails();
        }

        #endregion

        #region Private Members

        private static FileDetails primaryFile;
        private static FileDetails secondaryFile;

        #endregion

        #region Public Properties
        public static string User { get; set; }

        #endregion

        #region Public Methods

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        [DebuggerStepThrough]
        public static FileDetails GetFileDetails( bool Secondary = false )
        {
            if (Secondary)
                return secondaryFile;
            return primaryFile;
        }

        public static void CloseFile( bool Secondary = false )
        {
            FileDetails fd = primaryFile; ;

            if (Secondary)
            {
                fd = secondaryFile;
                secondaryFile = new FileDetails();
            }
            else
                primaryFile = new FileDetails();

            //fd.Context.SubmitChanges();
            fd.isOpen = false;
            fd.Connection.Close();
            fd.Connection.Dispose( );
            fd.IO.End( );
            fd.IO = null;
            

        }

        public static FileDetails CreateSingleChampionshipFile( string FilePath, bool Secondary = false )
        {

            FileDetails fd = new FileDetails( )
            {
                FileFormat = FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP ,
                FileState = FConnFileState.CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP_EDITABLE ,
                FilePath = FilePath ,
                Connection = CreateFConnFile( FilePath , User , 
                        FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP , 
                        FConnFileState.CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP_EDITABLE )
            };
            return fd;
        }

        //public static FileDetails CreateMultipleChampionshipFile( string FilePath, bool Secondary = false )
        //{

        //    FileDetails fd = new FileDetails();

        //    fd.FilePath = FilePath;
        //    fd.FileFormat = FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP;
        //    fd.FileState = FConnFileState.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP_EDITABLE;

        //    fd.Connection =
        //        CreateFConnFile(FilePath, User, FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP, FConnFileState.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP_EDITABLE);

        //    return fd;
        //}

        internal static FileDetails CreateEntryFormFile( string FilePath, bool Secondary = false )
        {

            FileDetails fd = new FileDetails( )
            {
                FilePath = FilePath ,
                FileFormat = FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM ,
                FileState = FConnFileState.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM_FIRST_OPEN ,
                Connection = CreateFConnFile( FilePath , User , 
                        FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM , 
                        FConnFileState.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM_FIRST_OPEN )
            };
            return fd;

        }

        /// <summary>
        /// Only works for single championship files.
        /// </summary>
        /// <param name="FilePath"></param>
        public static void ImportEntryForm ( string FilePath )
        {
            FileDetails EntryForm;
            FileDetails MainDatabase;

            // open the entry form as a secondary file.
            if ( OpenFile( FilePath , true ) )
                EntryForm = GetFileDetails( true );
            else
                throw new Exception( "Failed to open entry form." );

            if ( GetFileDetails( ).isOpen )
                MainDatabase = GetFileDetails( );
            else
                throw new Exception( "You must first have a main database." );


            var eChampionship = EntryForm.IO.GetAll<Championship>( ).FirstOrDefault( );
            var mChampionship = MainDatabase.IO.GetAll<Championship>( ).FirstOrDefault( );

            if ( eChampionship == null ) return;
            if ( mChampionship == null ) return;

            //foreach ( var eteam in eChampionship.listAllTeams( ) )
            //{
            //    var mteam = mChampionship.listAllTeams().Where( t => t.Name == eteam.Name ).FirstOrDefault();

            //    if ( mteam == null ) throw new Exception( "The team for this entry form could not be found" );

            //    // Copy Schools
            //    foreach ( var school in eteam.HasSchools )
            //    {
            //        School s2 = new School( school.Name );
            //        s2.ShortName = school.ShortName;
            //        MainDatabase.IO.Add<School>( s2 );
            //        mteam.AddSchool ( s2 , MainDatabase.IO );
            //    }

            //}

            foreach ( var athlete in EntryForm.IO.GetAll<Athlete>( ) )
            {
                // copy athlete

                string SchoolName = athlete.Attends?.Name;
                School School = null;
                if ( SchoolName != null )
                    School = MainDatabase.IO.GetAll<School>().Where(sch => sch.Name == SchoolName).FirstOrDefault();

                if ( School == null )
                    Debug.WriteLine( "Warning: School not found for " + athlete.Fullname );

                Team ET = athlete.getTeam ( eChampionship );
                Team MT = null;
                if ( ET != null )
                    MT = mChampionship.Teams.Where( t => t.Name == ET.Name).FirstOrDefault();

                Athlete a = new Athlete( )
                {
                    FirstName = athlete.FirstName,
                    MiddleName = athlete.MiddleName,
                    LastName = athlete.LastName,
                    PreferredName = athlete.PreferredName,
                    Attends = School,
                    GlobalAthleteID = athlete.GlobalAthleteID,
                    DateOfBirth = athlete.DateOfBirth,
                    Gender = athlete.Gender,
                    Suffix = athlete.Suffix,
                    Title = athlete.Title
                };

                MainDatabase.IO.Add<Person>( a );

                // Set athlete team.
                if ( MT != null )
                    a.setTeam( MT , mChampionship );
                else
                    if ( ET != null )
                    Debug.WriteLine( "Warning: Could not find the team for " + athlete.Fullname );


                // copy athlete notes

                foreach ( var note in athlete.Notes )
                {
                    if ( note.GetType( ) == typeof( ConfidentialNote ) )
                        a.AddNote( new ConfidentialNote( note ) );
                    else if ( note.GetType( ) == typeof( PublicNote ) )
                        a.AddNote( new PublicNote( (PublicNote) note ) );
                    // 2017-03-26
                    // Deliberately not copying availability information between championships!
                    // this could be a mistake.
                    //else if ( note.GetType() == typeof(DeclaredAvailibilityInformation ))
                    //    a.AddNote( new DeclaredAvailibilityInformation( (DeclaredAvailibilityInformation)note ) );
                    else if ( note.GetType( ) == typeof( PowerOfTenResult ) )
                        a.AddNote( new PowerOfTenResult( (PowerOfTenResult) note ) );
                    else if ( note.GetType( ) == typeof( PreviousResult ) )
                        a.AddNote( new PreviousResult( (PreviousResult) note ) );
                }


                // copy athlete contacts

                foreach ( var contact in athlete.Contacts )
                {
                    AContactDetail detail = null;

                    if ( contact is EmailContactDetail )
                        detail = new EmailContactDetail( ((EmailContactDetail) contact).EmailAddress , contact.ContactName );

                    if ( contact is AddressContactDetail )
                        detail = new AddressContactDetail(
                            contact.ContactName ,
                            ((AddressContactDetail) contact).FirstLine ,
                            ((AddressContactDetail) contact).SecondLine ,
                            ((AddressContactDetail) contact).ThirdLine ,
                            ((AddressContactDetail) contact).FourthLine ,
                            ((AddressContactDetail) contact).PostCode );

                    if ( contact is PhoneContactDetail )
                        detail = new PhoneContactDetail( )
                        {
                            ContactName = contact.ContactName ,
                            phoneNumber = ((PhoneContactDetail) contact).phoneNumber
                        };

                    if ( contact is MobileContactDetail )
                        detail = new MobileContactDetail( )
                        {
                            ContactName = contact.ContactName ,
                            phoneNumber = ((MobileContactDetail) contact).phoneNumber
                        };


                    if ( detail != null )
                        a.AddContact( detail );

                }

                // enter the athlete into their events

                foreach ( var competitor in athlete.AllCompetitors() )
                {
                    var eEvent = competitor.CompetingIn;
                    if ( eEvent == null ) continue;

                    var mEvent = mChampionship.Events.Where( Event => Event.ShortName == eEvent.ShortName).FirstOrDefault();

                    if ( mEvent == null )
                        Debug.WriteLine( "Warning: Event not found: " + eEvent.Name );

                    mEvent.enterAthlete( a );
                }

            }

            mChampionship.Save( );

            CloseFile( true );

        }

        public static bool OpenFile( string FilePath, bool Secondary = false )
        {
            try
            {
                if ( !FilePath.ToLower( ).EndsWith( ".csdb" ) )
                    return false;

                FileDetails fd = new FileDetails( )
                {
                    FilePath = FilePath ,
                    Connection = OpenFConnFile( FilePath ) ,
                    //fd.Context = new CSDB( fd.Connection );
                    isOpen = true
                };
                var FileDetails = fd.IO.GetAll<FConnFileDetail>().ToArray()[0];

                if ( FileDetails == null )
                    throw new ArgumentException( "This file is corrupt and can not be opened." );

                fd.FileFormat = FileDetails.FormatCode;
                fd.FileState = FileDetails.State;

                if ( fd.FileFormat == FConnFileFormat.UNKNOWN )
                    throw new ArgumentException( "This file is not supported by this application." );

                if ( FileDetails.MajorFileVersion < MIN_SUPPORTED_MAJOR_VERSION( fd.FileFormat ) &&
                    FileDetails.MinorFileVersion < MIN_SUPPORTED_MINOR_VERSION( fd.FileFormat ) )
                    throw new ArgumentException( "This file format is too old to be opened." );

                if ( FileDetails.MajorFileVersion < CURRENT_MAJOR_VERSION( fd.FileFormat ) &&
                    FileDetails.MinorFileVersion < CURRENT_MINOR_VERSION( fd.FileFormat ) )
                    UpgradeFile( fd );

                SetFileDetails( fd , Secondary );

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string[] CreateEntryForms( ChampionshipVM championship, TeamVM [] teams )
        {
            if ( !isFileOpen( ) )
                throw new Exception( "Championship file is not open" );

            if ( championship == null )
                throw new Exception( "Championship is null" );

            List<string> files = new List<string>();

            foreach ( var team in teams )
                buildEntryForm( team , championship );

            return files.ToArray( );
        }

        private static string buildEntryForm(TeamVM team, ChampionshipVM championship)
        {
            var entryForm = CreateEntryFormFile( Path.Combine( championship.getChampionshipExportsDir( ) , team.Name + " " + championship.Name + " Entry Form.csdb" ) , true );
            if ( OpenFile( entryForm.FilePath , true ) )
                entryForm = GetFileDetails( true );
            else
                throw new Exception( "Failed to open entry form." );

            Championship newChampionship = Championship .copyChampionship( championship.Championship.Name , championship.Championship, entryForm.IO, team.Team);

            foreach ( var templateVM in championship.Templates )
            {
                Template template = templateVM.Template;

                Template t = new Template() { Description = template.Description, InsertedDate = template.InsertedDate, Instructions = template.Instructions, TemplateFile = template.TemplateFile, TemplateName = template.TemplateName };

                entryForm.IO.Add<Template>( t );
            }


            string file = entryForm.FilePath ;

            CloseFile( true );

            GC.Collect( );

            return file;
        }

        [DebuggerStepThrough]
        public static bool IsEntryForm( bool Secondary = false )
        {
            if ( Secondary )
                return secondaryFile.FileFormat == FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM;
            return primaryFile.FileFormat == FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM;
        }

        [Obsolete ("Please do not use the static context",true)]
        public static void SaveChanges(bool Secondary = false)
        {
            //if (Secondary)
                //secondaryFile.Context.SubmitChanges();
            //else
                //primaryFile.Context.SubmitChanges();
        }

        [DebuggerStepThrough]
        public static bool isFileOpen(bool Secondary = false)
        {
            if (Secondary)
                return secondaryFile.isOpen;
            return primaryFile.isOpen;
        }

        [Obsolete ("Please do not use the static context", true)]
        public static CSDB getContext (bool Secondary = false)
        {
            //if (Secondary)
            //return secondaryFile.Context;
            //return primaryFile.Context;
            return null;
        }

        #endregion

        #region Private Methods

        #region SQL Commands

        private static string createFileDetails()
        {
            string action = @"CREATE TABLE [FileDetails] (
                                [ID] INTEGER  NOT NULL PRIMARY KEY,
                                [FormatCode] integer,
                                [MajorFileVersion] integer NOT NULL,
                                [MinorFileVersion] integer NOT NULL,
                                [CreatedBy] varchar,
                                [CreatedOn] DateTime,
                                [FileState] integer )";

            return action;
        }

        private static string createChampionships()
        {
            string action = @"
            CREATE TABLE [Championships] (
            [ID] INTEGER  NOT NULL PRIMARY KEY, 
            [FixedName] nvarchar NOT NULL, 
            [Name] nvarchar  NULL, 
            [ShortName] nvarchar  NULL, 
            [Date] datetime  NULL, 
            [Location] nvarchar  NULL, 
            [Locked] nvarchar  NULL, 
            [AgeDateReference] datetime NULL, 
            [GlobalChampionshipID] guid NULL, 
            [WebServerEnabled] BOOLEAN NOT NULL DEFAULT 0,
            [WebServerPort] integer NOT NULL DEFAULT 80,
            [Discriminator] nvarchar(128) NOT NULL
            )";

            return action.ToString();

            // 2016-03-29 V3-0
            // Discriminator is still in the database but has no use in the application as
            // the inheritance structure of AChampionship -> Championship -> has been flattened into Championship.
            // it could be removed in later versions. 
            //
            // WebServerEnabled and WebServerPort added.
            // FixedName added - This should be globally unique.
            // Locked added
            // ZippedFileStore and related Foreign key is added in createChampionshipForiegnKey which must be called after createFileStorage added.
        }

        private static string createChampionshipForiegnKey()
        {
            return @"
            ALTER TABLE [Championships] 
            ADD COLUMN [ZippedFileStore_ID] integer NULL
            REFERENCES FileStorages(ID) 
            ON DELETE CASCADE
            ";
        }

        private static string createPeople()
        {
            string action = @"
            CREATE TABLE [People] (
            [ID] INTEGER  NOT NULL PRIMARY KEY,
            [FirstName] nvarchar  NULL,
            [MiddleName] nvarchar  NULL,
            [LastName] nvarchar  NULL,
            [Title] nvarchar  NULL,
            [Suffix] nvarchar  NULL,
            [Gender] integer  NOT NULL DEFAULT 0,
            [DateOfBirth] datetime  NULL,
            [PreferredName] nvarchar  NULL,
            [GlobalAthleteID] integer  NULL,
            [Discriminator] nvarchar(128)  NOT NULL,
            [School_ID] integer NULL,

            FOREIGN KEY ([School_ID]) 
            REFERENCES Schools(ID) 
            ON DELETE SET NULL
            )";

            return action.ToString();

            // Suffux changed to Suffix in V3-0
            // PreferredName added in V3-0
            // GlobalAthleteID changed to integer - relates to PowerOfTen
        }

        private static string createSchools()
        {
            string action = @"
            CREATE TABLE[Schools] (
            [ID] INTEGER NOT NULL PRIMARY KEY,
            [Head] INTEGER NULL,
            [LetterGreating] nvarchar NULL,
            [Name] nvarchar NULL,
            [ShortName] nvarchar NULL,
            CONSTRAINT Foreign_key01
                FOREIGN KEY (Head)
                REFERENCES People(ID)
                ON DELETE SET NULL
            )";

            return action.ToString();

            // HeadTeacherName removed in V3-0
            // HeadTeacher_PersonID added in V3-0
        }

    private static string createStaff()
        {
            return @"CREATE TABLE Staff (
                      ID         integer NOT NULL PRIMARY KEY,
                      Title      nvarchar,
                      Person_ID  integer NOT NULL,
                      School_ID  integer NOT NULL,
                      /* Foreign keys */
                      CONSTRAINT Foreign_key02
                        FOREIGN KEY (School_ID)
                        REFERENCES Schools(ID)
                        ON DELETE CASCADE, 
                      CONSTRAINT Foreign_key01
                        FOREIGN KEY (Person_ID)
                        REFERENCES People(ID)
                        ON DELETE CASCADE
                    )";

            // Added in V3-0
        }

        private static string createTeams()
        {
            string action = @"

            CREATE TABLE[Teams] (
            [ID] INTEGER NOT NULL PRIMARY KEY,
            [Name] nvarchar NULL,
            [ShortName] nvarchar NULL,
            [Championship_ID] integer NOT NULL, 
            FOREIGN KEY ([Championship_ID]) REFERENCES[Championships]([ID]) ON DELETE CASCADE
            )";

            return action.ToString();
        }

        private static string createSchoolTeams()
        {
            //string action = @"
            //CREATE TABLE [SchoolTeams] (
            //[School_ID] integer NOT NULL,
            //[Team_ID]   integer NOT NULL,
            //PRIMARY KEY([School_ID], [Team_ID]),

            //FOREIGN KEY ([School_ID]) REFERENCES[Schools]([ID]),
            //FOREIGN KEY ([Team_ID]) REFERENCES[Teams]([ID])
            //)";

            return @"
            CREATE TABLE [SchoolTeams] (
            [ID] INTEGER NOT NULL PRIMARY KEY,
            [School_ID] integer NOT NULL,
            [Team_ID]   integer NOT NULL,

            FOREIGN KEY ([School_ID]) REFERENCES[Schools]([ID]),
            FOREIGN KEY ([Team_ID]) REFERENCES[Teams]([ID])
            )";


            // Added ID as primary key in V3-0 to work around a linq-to-sql bug that was deleting teams from the teams table.
        }

        private static string createVestActions()
        {
            string action = @"
            CREATE TABLE[VestActions] (
            [ID] INTEGER NOT NULL PRIMARY KEY,
            [WebID] integer NOT NULL,
            [Description] nvarchar NULL,
            [DateStamp] datetime NOT NULL,
            [Vest] nvarchar NULL,
            [Championship] nvarchar NULL,
            [EventCode] nvarchar NULL,
            [Position] integer NULL,
            [Time] datetime NULL,
            [Ignored] bit NOT NULL,
            [statusDescription] nvarchar NULL,
            [Championship_ID] integer NOT NULL,

            CONSTRAINT CHAMPIONSHIP_VESTACTION
                FOREIGN KEY (Championship_ID)
                REFERENCES Championships(ID)
                ON DELETE CASCADE

            )";

            return action.ToString();

            // AChampionship_ID renamed to Championship_ID in V3-0
            // Championship_ID set to NOT NULL
        }

        private static string createTemplates()
        {
            string action = @"
            CREATE TABLE[Templates] (
            [ID] INTEGER NOT NULL PRIMARY KEY,
            [InsertedDate]
            datetime NOT NULL,
            [TemplateName]
            nvarchar NULL,
            [Description] nvarchar NULL,
            [Instructions] TEXT NULL,
            [TemplateFile] blob NULL
            )";

            return action.ToString();
        }

        private static string createStandards()
        {
            return @"CREATE TABLE Standards (
                  ID         integer NOT NULL PRIMARY KEY,
                  Name       nvarchar,
                  ShortName  nvarchar,
                  Event_ID   integer NOT NULL,
                  RawValue   integer NOT NULL,
                  CONSTRAINT Foreign_key01
                    FOREIGN KEY (Event_ID)
                    REFERENCES Events(ID)
                    ON DELETE CASCADE
                )";

            //string action = @"

            //CREATE TABLE[Standards] (
            //[ID] INTEGER NOT NULL PRIMARY KEY,
            //[NationalStandard_RawValue] integer NOT NULL,
            //[NationalStandard_ValueType] integer NOT NULL,
            //[EntryStandard_RawValue] integer NOT NULL,
            //[EntryStandard_ValueType] integer NOT NULL,
            //[CountyStandard_RawValue] integer NOT NULL,
            //[CountyStandard_ValueType] integer NOT NULL,
            //[DistricStandard_RawValue] integer NOT NULL,
            //[DistricStandard_ValueType] integer NOT NULL,
            //[CountyBestPerformance_RawValue] integer NOT NULL,
            //[CountyBestPerformance_ValueType] integer NOT NULL,
            //[CountyBestPerformanceName] nvarchar NULL,
            //[CountyBestPerformanceYear] integer NOT NULL,
            //[CountyBestPerformanceArea] nvarchar NULL
            //)";

            //return action.ToString();

            // Overhauled in V3-0 to support 1 event -> many standards
            // Best Performance details moved to Event
        }

        private static string createResults()
        {
            string action = @"
            CREATE TABLE [Results] (
            [ID] INTEGER  PRIMARY KEY NOT NULL,
            [Rank] integer  NULL,
            [VestNumber_dbVestNumber] nvarchar  NULL,
            [TypeDescriminator] integer  NULL,
            [Value_RawValue] integer  NOT NULL,
            [Value_ValueType] integer  NOT NULL,
            [Competitor_ID] integer  NULL,
            [Event_ID] integer NOT NULL,
            [Discriminator] NVARCHAR(128) DEFAULT 'Result' NOT NULL,

            CONSTRAINT EVENT_RESULT
                FOREIGN KEY (Event_ID)
                REFERENCES Events(ID)
                ON DELETE RESTRICT,

            CONSTRAINT COMPETITOR_RESULT
                FOREIGN KEY (Competitor_ID)
                REFERENCES Competitors(ID)
                ON DELETE RESTRICT
            )";

            return action.ToString();

            // Event_ID set to NOT NULL in V3-0
            // Note we can not use Event_ID and RANK as primary keys as this 
            // would prevent tied results which must be supported in future versions.
            //
            // 2016-03-25 relationship with Competitor set to restrict as an experiment to prevent 
            //  Competitors with Results being deleted, also relationship with event set to restrict 
            //  to prevent events with results from being deleted.

        }

        private static string createRestrictions()
        {
            string action = @"
            CREATE TABLE [Groups] (
            [ID] INTEGER  NOT NULL PRIMARY KEY, 
            [Name] nvarchar  NULL, 
            [ShortName] nvarchar  NULL, 
            [minAge] integer  NULL, 
            [maxAge] integer  NULL, 
            [dateReference] datetime  NULL, 
            [StartDate] datetime  NULL, 
            [EndDate] datetime  NULL, 
            [Male] bit  NULL, 
            [Female] bit  NULL, 
            [Discriminator] nvarchar(128)  NOT NULL, 
            [Championship_ID] integer  NULL,

            FOREIGN KEY ([Championship_ID]) REFERENCES[Championships]([ID]) ON DELETE CASCADE

            )";

            return action.ToString();

            // AChampionship_ID renamed to Championship_ID in V3-0
            // Table name changed from Restrictions to Groups
        }

        private static string createFileStorages()
        {
            string action = @"
            CREATE TABLE [FileStorages] (
            [ID] INTEGER  NOT NULL PRIMARY KEY, 
            [CreatedOn] datetime  NOT NULL, 
            [Extension] nvarchar  NULL, 
            [FileData] blob  NULL, 
            [Name] nvarchar  NULL, 
            [ShortName] nvarchar  NULL, 
            [Event_ID] integer  NULL,

            FOREIGN KEY ([Event_ID]) REFERENCES[Events]([ID]) ON DELETE CASCADE
            )";

            return action.ToString();

            // AEvent_ID renamed to Event_ID in V3-0
        }

        private static string createEvents()
        {
            return @"CREATE TABLE Events (
              ID                                                  integer NOT NULL PRIMARY KEY,
              Description                                         nvarchar,
              StartTime                                           varchar(20),
              EndTime                                             integer NOT NULL,
              EventRanges_MaxCompetitors                          integer NOT NULL,
              EventRanges_MinCompetitors                          integer NOT NULL,
              EventRanges_MaxGuests                               integer NOT NULL,
              EventRanges_MaxCompetitorsPerTeam                   integer NOT NULL,
              EventRanges_TopIndividualCertificates               integer NOT NULL,
              EventRanges_TopLowerYearGroupInividualCertificates  integer NOT NULL,
              EventRanges_TeamASize                               integer NOT NULL,
              EventRanges_TeamBSize                               integer NOT NULL,
              EventRanges_TeamBForScoringTeamOnly                 bit NOT NULL,
              EventRanges_ScoringTeams                            integer NOT NULL,
              EventRanges_Lanes                                   integer NOT NULL,
              ResultsDisplayDescription                           integer NOT NULL,
              ResultsTemplate                                     integer,
              DataEntryTemplate                                   integer,
              CertificateTemplate                                 integer,
              VestTemplate                                        integer,
              Name                                                nvarchar,
              ShortName                                           nvarchar,
              MaxCompetitorsPerHeat                               integer,
              HeatRunAsFinal                                      bit,
              LowerYearGroup                                      integer,
              LowerYearGroup1                                     integer,
              State                                               integer,
              Discriminator                                       nvarchar(128) NOT NULL,
              Championship_ID                                     integer NOT NULL,
              Final_ID                                            integer,
              [CountyBestPerformance_RawValue]                    integer NOT NULL,
              [CountyBestPerformanceName]                         nvarchar NULL,
              [CountyBestPerformanceYear]                         integer NOT NULL,
              [CountyBestPerformanceArea]                         nvarchar NULL,
              /* Foreign keys */
              CONSTRAINT Foreign_key04
                FOREIGN KEY (VestTemplate)
                REFERENCES Templates(ID)
                ON DELETE SET NULL, 
              CONSTRAINT Foreign_key03
                FOREIGN KEY (DataEntryTemplate)
                REFERENCES Templates(ID)
                ON DELETE SET NULL, 
              CONSTRAINT Foreign_key01
                FOREIGN KEY (Championship_ID)
                REFERENCES Championships(ID)
                ON DELETE CASCADE, 
              CONSTRAINT Foreign_key03
                FOREIGN KEY (Final_ID)
                REFERENCES Events(ID)
                ON DELETE CASCADE, 
              CONSTRAINT Foreign_key01
                FOREIGN KEY (ResultsTemplate)
                REFERENCES Templates(ID)
                ON DELETE SET NULL, 
              CONSTRAINT Foreign_key04
                FOREIGN KEY (CertificateTemplate)
                REFERENCES Templates(ID)
                ON DELETE SET NULL
            )";

            // Championship_ID set to NOT NULL in V3-0
            // The four templates now point to the ID of Templates rather than a location on the disk.
            // Standards_ID removed in V3-0
            // EndTime added in V3-0
            // Start time now evaluates to TimeSpan.Parse() and from TimeSpan.ToString()
            // Championship Best Performance field added in V3-0
        }

        private static string createEventRestrictions()
        {
            string action = @"
            CREATE TABLE [EventGroups] (
            [ID] INTEGER  NOT NULL PRIMARY KEY,
            [Event_ID] integer NOT NULL,
            [Group_ID] integer NOT NULL,
            
            CONSTRAINT CHAMPIONSHIP_EVENT
                FOREIGN KEY (Event_ID)
                REFERENCES Events(ID)
                ON DELETE CASCADE,

            CONSTRAINT CHAMPIONSHIP_EVENT
                FOREIGN KEY (Group_ID)
                REFERENCES Groups(ID)
                ON DELETE CASCADE

            )";

            return action.ToString();
            // V3-0 Table name changed from EventRestrictions to EventGroups

        }

        private static string createCustomDataValues()
        {
            string action = @"
            CREATE TABLE [CustomDataValues] (
            [ID] INTEGER  PRIMARY KEY NOT NULL,
            [key] nvarchar  NULL,
            [intvalue] integer  NULL,
            [stringvalue] nvarchar  NULL,
            [Discriminator] nvarchar(128)  NOT NULL,
            [Championship_ID] integer  NULL,
            [Event_ID] integer  NULL,
            [Competitor_ID] INTEGER  NULL,

            FOREIGN KEY ([Championship_ID]) REFERENCES[Championships]([ID]) ON DELETE CASCADE,
            FOREIGN KEY ([Event_ID]) REFERENCES[Events]([ID]) ON DELETE CASCADE,
            FOREIGN KEY ([Competitor_ID]) REFERENCES[Competitors]([ID]) ON DELETE CASCADE

            )";

            return action.ToString();

            // Championship_ID, Event_ID and Compeitor_ID had prefix 'A' removed in V3-0
            // value/value1 renamed to intvalue/stringvalue in V3-0
        }

        private static string createContactDetails()
        {
            string action = @"
            CREATE TABLE [ContactDetails] (
            [ID] INTEGER  NOT NULL PRIMARY KEY,
            [Primary] bit  NOT NULL,
            [ContactName] nvarchar  NULL,
            [FirstLine] nvarchar  NULL,
            [SecondLine] nvarchar  NULL,
            [ThirdLine] nvarchar  NULL,
            [FourthLine] nvarchar  NULL,
            [PostCode] nvarchar  NULL,
            [EmailAddress] nvarchar  NULL,
            [phoneNumber] nvarchar  NULL,
            [Discriminator] nvarchar(128)  NOT NULL,
            [Person_ID] integer  NULL,

            FOREIGN KEY ([Person_ID]) REFERENCES[People]([ID]) ON DELETE CASCADE
            )";

            return action.ToString();

            // Removed School_ID in V3-0
        }

        private static string createCompetitors()
        {
            string action = @"
            CREATE TABLE [Competitors] ( 
            [ID] INTEGER  NOT NULL PRIMARY KEY, 
            [Vest_dbVestNumber] nvarchar  NULL, 
            [Guest] bit  NOT NULL, 
            [AvilableForSW] bit  NULL, 
            [AvilableForNationals] bit  NULL, 
            [CoachForSW] nvarchar  NULL, 
            [CoachForNationals] nvarchar  NULL, 
            [SelectedForNextEvent] bit  NOT NULL, 
            [LaneNumber] integer DEFAULT '0' NOT NULL, 
            [InFinal] bit  NULL, 
            [HeatLaneNumber] integer  NULL, 
            [Discriminator] nvarchar(128)  NOT NULL, 
            [Athlete_ID] integer  NULL, 
            [CompetingIn_ID] integer NOT NULL, 
            [HeatEvent_ID] integer  NULL, 
            [Competitor1_ID] integer  NULL, 
            [Competitor2_ID] integer  NULL, 
            [Competitor3_ID] integer  NULL, 
            [Competitor4_ID] integer  NULL, 
            [PersonalBest_RawValue] integer DEFAULT '0' NOT NULL, 
            [PersonalBest_ValueType] integer DEFAULT '0' NOT NULL,
            
            CONSTRAINT SQUAD_COMPETITOR_1
                FOREIGN KEY (Competitor1_ID)
                REFERENCES People(ID)
                ON DELETE RESTRICT,

            CONSTRAINT SQUAD_COMPETITOR_2
                FOREIGN KEY (Competitor2_ID)
                REFERENCES People(ID)
                ON DELETE RESTRICT,
            
            CONSTRAINT SQUAD_COMPETITOR_3
                FOREIGN KEY (Competitor3_ID)
                REFERENCES People(ID)
                ON DELETE RESTRICT,
            
            CONSTRAINT SQUAD_COMPETITOR_4
                FOREIGN KEY (Competitor4_ID)
                REFERENCES People(ID)
                ON DELETE RESTRICT,
            
            CONSTRAINT ATHLETE_COMPETITOR
                FOREIGN KEY (Athlete_ID)
                REFERENCES People(ID)
                ON DELETE RESTRICT,

            CONSTRAINT EVENT_COMPETITOR
                FOREIGN KEY (CompetingIn_ID)
                REFERENCES Events(ID)
                ON DELETE CASCADE,

            CONSTRAINT EVENT_HEAT_COMPETITOR
                FOREIGN KEY (HeatEvent_ID)
                REFERENCES Events(ID)
                ON DELETE SET NULL
            )";

            return action.ToString();

            // 2016-03-25 relationships with Athletes set to restrict as an experiment to prevent 
            //  Athletes who have been entered from being deleted.

        }

        private static string createCertificates()
        {
            string action = @"
            CREATE TABLE [Certificates] (
            [Name] nvarchar  NULL,
            [ShortName] nvarchar  NULL,
            [Discriminator] nvarchar(128)  NOT NULL,
            [Competitor_ID] integer NOT NULL,
            [Event_ID] integer NOT NULL,
            [File_ID] integer NOT NULL,

            PRIMARY KEY([File_ID], [Event_ID],[Competitor_ID]),
            
            FOREIGN KEY ([File_ID]) REFERENCES[FileStorages]([ID]) ON DELETE CASCADE,
            FOREIGN KEY ([Event_ID]) REFERENCES[Events]([ID]) ON DELETE CASCADE,
            FOREIGN KEY ([Competitor_ID]) REFERENCES[Competitors]([ID]) ON DELETE CASCADE
     
            )";

            return action.ToString();

            // Made File_ID, Event_IC and Competitor_ID primary keys in V3-0
            // ID removed in V3-0 Primary Keys set to [File_ID], [Event_ID] and [Competitor_ID] instead

        }

        private static string createAthleteNotes()
        {
            return @"
            CREATE TABLE [AthleteNotes](
            [ID] integer  NOT NULL PRIMARY KEY, 
            [Athlete_ID] integer NOT NULL, 
            [Championship] varchar NULL,
            [Key] varchar NULL,
            [Note] varchar NULL,
            [Event] varchar NULL,
            [Rank] varchar NULL,
            [ResultValue] varchar NULL,
            [Discriminator] nvarchar(128) NOT NULL, 
            [EventDate] datetime NULL,
            [Venue] varchar NULL,
            [Team] varchar NULL,
            [EnteredDate] datetime NOT NULL,

            FOREIGN KEY ([Athlete_ID]) REFERENCES[People]([ID]) ON DELETE CASCADE)
            ";

        }

        private static string createAthleteTeamChamptionships()
        {
            string action = @"
            CREATE TABLE [AthleteTeamChamptionships] (
            [ID] integer  NOT NULL PRIMARY KEY, 
            [Athlete_ID] integer  NULL,
            [Championship_ID] integer  NULL,
            [Team_ID] integer  NULL,
            [PreferedEvent_ID] integer  NULL,
            
            
            FOREIGN KEY ([Championship_ID]) REFERENCES[Championships]([ID]) ON DELETE CASCADE,
            FOREIGN KEY ([Athlete_ID]) REFERENCES[People]([ID]) ON DELETE CASCADE,
            FOREIGN KEY ([Team_ID]) REFERENCES[Teams]([ID]) ON DELETE CASCADE,
            FOREIGN KEY ([PreferedEvent_ID]) REFERENCES[Events]([ID]) ON DELETE SET NULL
            )";

            return action.ToString();

            // ID removed in V3-0 Primary Keys set to [Athlete_ID], [Championship_ID] instead
            // ID put back in because of the same reason that SchoolTeams was causing issues upon auto deletion.

        }

        private static string createAuditLogs()
        {
            string action = @"
            CREATE TABLE [AuditLogs] (
            [ID] INTEGER  NOT NULL PRIMARY KEY, 
            [User] nvarchar  NULL, 
            [EventType] nvarchar  NULL, 
            [TableName] nvarchar  NULL, 
            [RecordId] int NULL, 
            [EventDateUTC] datetime  NOT NULL 
            )";

            return action.ToString();

            // Added in V3-0
        }

        private static string createAuditLogDetails()
        {
            string action = @"
            CREATE TABLE [AuditLogDetails] (
            [ID] INTEGER  NOT NULL PRIMARY KEY,
            [ColumnName] nvarchar  NULL,
            [OriginalValue] nvarchar  NULL,
            [NewValue] nvarchar  NULL,
            [AuditLogRecord_AuditLogId] int NOT NULL,
            FOREIGN KEY ([AuditLogRecord_AuditLogId]) REFERENCES[AuditLogs]([ID]) ON DELETE CASCADE
            )";

            return action.ToString();

            // Added in V3-0

        }

        private static string createNextID()
        {
            return @"
            CREATE TABLE[NextID] (

            [Table] varchar NOT NULL PRIMARY KEY,
            [NextIDInt] int NOT NULL
            )";
        }

        #endregion

        public static string getFilePath()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog( )
            {

                // Set filter for file extension and default file extension 
                DefaultExt = ".csdb" ,
                Filter = "Championship Solutions Database (.csdb)|*.csdb"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
                return dlg.FileName;

            return null;
        }

        private static SQLiteConnection OpenFConnFile(string FilePath)
        {
            if (!System.IO.File.Exists(FilePath)) return null;
            SQLiteConnection con;
#if ( ENABLE_ENCRYPTION )
            con = new System.Data.SQLite.SQLiteConnection("data source=" + FilePath + @";foreign keys=true;Password=89c6FHgf9UYSR9N52myH;");
#else
            con = new System.Data.SQLite.SQLiteConnection("data source=" + FilePath + @";foreign keys=true;");
#endif 

            con.StateChange += Con_StateChange;
            con.Update += Con_Update;
            con.Disposed += Con_Disposed;

            return con;
        }

        private static void Con_Disposed ( object sender , EventArgs e )
        {
            //Debug.WriteLine ( "Connection has been disposed of" );
        }

        private static void Con_Update ( object sender , UpdateEventArgs e )
        {
            //Debug.WriteLine ( "Connection is updating the database" );
        }

        private static void Con_StateChange ( object sender , System.Data.StateChangeEventArgs e )
        {

            //System.Diagnostics.Debug.Write ( "Connection state has changed to " );

            //switch ( ( (SQLiteConnection)sender ).State )
            //{
            //    case System.Data.ConnectionState.Open:
            //        System.Diagnostics.Debug.WriteLine ( "Open" );
            //        break;
            //    case System.Data.ConnectionState.Broken:
            //        System.Diagnostics.Debug.WriteLine ( "Broken" );
            //        break;
            //    case System.Data.ConnectionState.Closed:
            //        System.Diagnostics.Debug.WriteLine ( "Closed" );
            //        break;
            //    case System.Data.ConnectionState.Connecting :
            //        System.Diagnostics.Debug.WriteLine ( "Connecting" );
            //        break;
            //    case System.Data.ConnectionState.Executing:
            //        System.Diagnostics.Debug.WriteLine ( "Executing" );
            //        break;
            //    case System.Data.ConnectionState.Fetching:
            //        System.Diagnostics.Debug.WriteLine ( "Fetching" );
            //        break;
            //    default:
            //        System.Diagnostics.Debug.WriteLine ( "Unknown" );
            //        break;
            //}

        }

        private static SQLiteConnection CreateFConnFile(string FilePath, string CreatedBy, FConnFileFormat Format, FConnFileState State)
        {
            return CreateFConnFile( FilePath , CreatedBy , Format , State , out Exception ex );
        }

        private static SQLiteConnection CreateFConnFile(string FilePath, string CreatedBy, FConnFileFormat Format, FConnFileState State, out Exception Exception)
        {
            SQLiteConnection con = null;

            try
            {
                SQLiteConnection.CreateFile(FilePath);
                con = new System.Data.SQLite.SQLiteConnection("data source=" + FilePath + "; foreign keys = true;");

                SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con);

                con.Open();
#if (ENABLE_ENCRYPTION)
                con.ChangePassword("89c6FHgf9UYSR9N52myH");
#endif 
                com.CommandText = createFileDetails();
                com.ExecuteNonQuery();

                com.CommandText = createNextID();
                com.ExecuteNonQuery();

                string action = @"INSERT INTO [FileDetails] (
                                [ID],
                                [FormatCode],
                                [MajorFileVersion],
                                [MinorFileVersion],
                                [CreatedBy],
                                [FileState]) VALUES (
                                1, {0}, {1}, {2}, '{3}', {4}
                                )";

                action = string.Format(action, (byte)Format, CURRENT_MAJOR_VERSION(Format), CURRENT_MINOR_VERSION(Format), CreatedBy, (byte)State);

                com.CommandText = action;
                com.ExecuteNonQuery();

                switch (Format)
                {
                    case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_SINGLE_CHAMPIONSHIP:
                        CreateStdCSTables(com);
                        break;
                    case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_MULTIPLE_CHAMPIONSHIP:
                        CreateStdCSTables(com);
                        break;
                    case FConnFileFormat.CHAMPIONSHIP_SOLUTIONS_ENTRY_FORM:
                        CreateStdCSTables(com);
                        break;
                }

                InitaliseNextID(con);

                con.Close();

                Exception = null;
                return OpenFConnFile(FilePath);

            }
            catch (Exception ex)
            {
                if (con != null)
                    con.Close();

                Exception = ex;
                return null;
            }

        }

        private static void UpgradeFile(FileDetails fd)
        {
            throw new NotImplementedException();
        }

        private static void SetFileDetails(FileDetails fd, bool Secondary = false)
        {

            if (Secondary)
            {
                secondaryFile = fd;
                return;
            }

            primaryFile = fd;

        }

        private static void InitaliseNextID(SQLiteConnection Connection)
        {
            const int DEFAULT_START_INT = 1;

            string commandText = @"SELECT name FROM sqlite_master WHERE type = 'table'";

            SQLiteCommand cmd = Connection.CreateCommand();

            cmd.CommandText = commandText;

            SQLiteDataReader dr = cmd.ExecuteReader();


            if (! dr.HasRows)
                return;

            while (dr.Read())
            {

                string insertText = "INSERT INTO [NextID] ( [Table] , [NextIDInt] ) VALUES ( '{0}', {1} )";

                insertText = string.Format(insertText, dr[0], DEFAULT_START_INT);

                SQLiteCommand com = Connection.CreateCommand();

                com.CommandText = insertText;

                com.ExecuteNonQuery();
            }
            
        }

        /// <summary>
        /// Creates the tables used by the three CS file formats.
        /// </summary>
        private static void CreateStdCSTables(SQLiteCommand com)
        {

            com.CommandText = createChampionships();
            com.ExecuteNonQuery();

            com.CommandText = createTeams();
            com.ExecuteNonQuery();

            com.CommandText = createSchools();
            com.ExecuteNonQuery();

            com.CommandText = createSchoolTeams();
            com.ExecuteNonQuery();

            com.CommandText = createStaff ( );
            com.ExecuteNonQuery ( );

            com.CommandText = createPeople();
            com.ExecuteNonQuery();

            com.CommandText = createAthleteNotes();
            com.ExecuteNonQuery();

            com.CommandText = createEvents();
            com.ExecuteNonQuery();

            com.CommandText = createRestrictions();
            com.ExecuteNonQuery();

            com.CommandText = createEventRestrictions();
            com.ExecuteNonQuery();

            com.CommandText = createCompetitors();
            com.ExecuteNonQuery();

            com.CommandText = createResults();
            com.ExecuteNonQuery();

            com.CommandText = createCertificates();
            com.ExecuteNonQuery();

            com.CommandText = createVestActions();
            com.ExecuteNonQuery();

            com.CommandText = createFileStorages();
            com.ExecuteNonQuery();

            com.CommandText = createChampionshipForiegnKey();
            com.ExecuteNonQuery();

            com.CommandText = createStandards();
            com.ExecuteNonQuery();

            com.CommandText = createTemplates();
            com.ExecuteNonQuery();

            com.CommandText = createCustomDataValues();
            com.ExecuteNonQuery();

            com.CommandText = createContactDetails();
            com.ExecuteNonQuery();

            com.CommandText = createAthleteTeamChamptionships();
            com.ExecuteNonQuery();

            com.CommandText = createAuditLogs();
            com.ExecuteNonQuery();

            com.CommandText = createAuditLogDetails();
            com.ExecuteNonQuery();

        }

        #endregion

    }
}
