/*
 *  Filename         : Championship.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 *  2015-05-02
 *      Added CustomData to AChampionship
 *
 *  2016-03-22
 *       Split Linq data from main class
 *
 *  2016-03-29
 *       AChampionship Abstract class changed to Championship and abstract modifier removed.
 *       SchoolChampionship has merged into Championship. 
 *       Championship now has no inheritance. 
 *       (Discriminator has been commented out of TChampionships but not removed from the database.)
 */

 //#define TeamTesting

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    public partial class Championship: ICustomData
    {
        //private DataAccess.Database _database;
        //public DataAccess.Database database
        //{
        //    get
        //    {
        //        return _database;
        //    }
        //    set
        //    {
        //        _database = value;
        //        DState.IO = value;
        //        DState.NeedsUpdating = true;
        //    }
        //}

        public static Championship copyChampionship ( string newChampionshipName, Championship origonalChampionship, DataAccess.Database Connection , Team team = null )
        {
            Championship newChamp = new Championship(newChampionshipName, origonalChampionship.AgeDateReference);

            newChamp.Location = origonalChampionship.Location;
            newChamp.Locked = origonalChampionship.Locked;
            newChamp.ShortName = origonalChampionship.ShortName;
            newChamp._FixedName = origonalChampionship.FixedName;

            Connection.Add<Championship>( newChamp );

            // Restrictions
            foreach (Group res in origonalChampionship.Groups)
                newChamp.addGroup(Group.CopyGroup ( res));

#if ( !TeamTesting )
            // Teams (and schools)
            Team[] teams;
            if ( team == null )
                teams = origonalChampionship.Teams;
            else
                teams = new Team[] { team };

            foreach ( Team t in teams)
            {
                Team t2 = new Team(t.Name, newChamp) { ShortName = t.ShortName };
                newChamp.addTeam( t2 );

                foreach ( School sch in t.HasSchools )
                {
                    School s2 = new DM.School( sch.Name );
                    s2.ShortName = sch.ShortName;
                    Connection.Add<School>( s2 );

                    t2.AddSchool( s2 , Connection );

                    foreach ( var athlete in  origonalChampionship.DState.IO.GetAll<Athlete>().Where( a => a.Attends == sch))
                    {
                        // copy athlete

                        Athlete a = new Athlete( )
                        {
                            FirstName = athlete.FirstName,
                            MiddleName = athlete.MiddleName,
                            LastName = athlete.LastName,
                            PreferredName = athlete.PreferredName,
                            Attends = s2,
                            GlobalAthleteID = athlete.GlobalAthleteID,
                            DateOfBirth = athlete.DateOfBirth,
                            Gender = athlete.Gender,
                            Suffix = athlete.Suffix,
                            Title = athlete.Title
                        };

                        Connection.Add<Person>( a );

                        // copy athlete notes

                        foreach ( var note in athlete._Notes )
                        {
                            if ( note.GetType( ) == typeof( ConfidentialNote ) )
                                a.AddNote( new ConfidentialNote( note ) );
                            else if ( note.GetType( ) == typeof( PublicNote ) )
                                a.AddNote( new PublicNote( (PublicNote)note ) );
                            // 2017-03-26
                            // Deliberately not copying availability information between championships!
                            // this could be a mistake.
                            //else if ( note.GetType() == typeof(DeclaredAvailibilityInformation ))
                            //    a.AddNote( new DeclaredAvailibilityInformation( (DeclaredAvailibilityInformation)note ) );
                            else if ( note.GetType( ) == typeof( PowerOfTenResult ) )
                                a.AddNote( new PowerOfTenResult( (PowerOfTenResult)note ) );
                            else if ( note.GetType( ) == typeof( PreviousResult ) )
                                a.AddNote( new PreviousResult( (PreviousResult)note ) );
                        }


                        // copy athlete contacts

                        foreach ( var contact in athlete.Contacts )
                        {
                            AContactDetail detail = null;

                            if ( contact is EmailContactDetail )
                                detail = new EmailContactDetail( ( (EmailContactDetail)contact ).EmailAddress , contact.ContactName );

                            if ( contact is AddressContactDetail )
                                detail = new AddressContactDetail(
                                    contact.ContactName ,
                                    ( (AddressContactDetail)contact ).FirstLine ,
                                    ( (AddressContactDetail)contact ).SecondLine ,
                                    ( (AddressContactDetail)contact ).ThirdLine ,
                                    ( (AddressContactDetail)contact ).FourthLine ,
                                    ( (AddressContactDetail)contact ).PostCode );

                            if ( contact is PhoneContactDetail )
                                detail = new PhoneContactDetail( )
                                {
                                    ContactName = contact.ContactName ,
                                    phoneNumber = ( (PhoneContactDetail)contact ).phoneNumber
                                };

                            if ( contact is MobileContactDetail )
                                detail = new MobileContactDetail( )
                                {
                                    ContactName = contact.ContactName ,
                                    phoneNumber = ( (MobileContactDetail)contact ).phoneNumber
                                };


                            if ( detail != null )
                                a.AddContact( detail );

                        }

                    }

                    origonalChampionship.DState.IO.DeleteRange<Person>(
                        origonalChampionship.DState.IO.GetAll<Athlete>( ).Where( a => a.Attends == sch ).ToArray() );
                }

            }
#endif 

            // Events (not competitors or results)
            foreach ( AEvent Event in origonalChampionship.listAllEvents( ) )
                AEvent.CopyEvent( Event , newChamp );


            // Custom Data
            CustomData.CopyCustomData(origonalChampionship.CustomDataStore,newChamp);

            newChamp.Save( );

            return newChamp;
        }

        //public int ID { get { return _ID; } set { _ID = value; } }

        #region Names

                public string FixedName { get { return _FixedName ?? ""; } }

                public string Name { get { return _Name ?? ""; } set { _Name = value; } }

                /// <summary>
                /// Get or Set the short name of this team.
                /// By default this is the first 4 characters without any spaces.
                /// </summary>
                public string ShortName
                {
                    get
                    {
                        if (_ShortName != null)
                            return _ShortName;

                        if (Name == null)
                            return null;

                        if (Name.Length > 4)
                            return Name.Replace(" ", string.Empty).Substring(0, 4).Trim();
                        else
                            return Name.Trim();
                    }
                    set
                    {
                        _ShortName = value;
                    }
                }

        #endregion

        public DateTime? Date { get { return _Date; } set { _Date = value; } }

        public string printDate () { return Date.ToString(); }

        public string Location { get { return _Location; } set { _Location = value; } }

        private string Locked { get { return _Locked; } set { _Locked = value; } }

        public bool WebServerEnabled { get { return _WebServerEnabled; } set { _WebServerEnabled = value; } }

        public int WebServerPort { get { return _WebServerPort; } set { _WebServerPort = value; } }

        public FileStorage ZippedFileStore { get { return _ZippedFileStore; } set { _ZippedFileStore = value; } }

#region Constructors

        public Championship () { DState = null; }

        public Championship(string ChampName) : this()
        {
            _FixedName = ChampName;
            Name = ChampName;
        }

        /// <summary>
        /// Constructor for new championships
        /// </summary>
        /// <param name="ChampionshipName">Championship Name</param>
        public Championship(string ChampionshipName, DateTime? DateReference) : this(ChampionshipName) 
        {
            this.AgeDateReference = DateReference;
        }

        #endregion

#if ( !TeamTesting )


        #region Teams

        public Team[] Teams { get
            {
                return _Teams.ToArray ( );
            }
        }

        /// <summary>
        /// Creates a new team and adds it to the Championship
        /// </summary>
        /// <param name="TeamName">The name of the new team to be added</param>
        /// <returns>True if added successfully</returns>
        public bool addTeam(string TeamName)
        {
            return addTeam(new Team(TeamName, this));
        }

        /// <summary>
        /// Adds a team to the championship
        /// </summary>
        /// <param name="team">The team to be added to the championship</param>
        /// <returns>True if added successfully</returns>
        public bool addTeam(Team team)
        {

            // Validation removed in V3-0.

            //// Check to see if the team is already in this championship
            //int count = Teams.Count(t => t.Name == team.Name);

            //if (count == 0)
            //{
            //    // Team Name has not been used
            //    _Teams.Add(team);
            //    return true;
            //}

            //return true;

            //// Team name has already been added
            //throw new ArgumentOutOfRangeException ( "team" , String.Format ( "The team {0} has already been entered" , team.Name ) );

            // !*!
            DState.IO.Add<Team> ( team );
            __Teams.refresh ( );
            //_Teams.Add ( team );
            return true;
        }

        public List<Team> listAllTeams()
        {
            return Teams.ToList();
        }

        public void RemoveTeam(Team Team)
        {
            if ( !Team.CanDelete ( ) ) return;

            //!*!
            DState.IO.Delete<Team> ( Team );
            __Teams.refresh ( );
            //_Teams.Remove ( Team );
            Team.Championship = null;
        }

        public void RemoveGroup ( Group temp )
        {
            if ( !temp.CanDelete ( ) ) return;

            //!*!
            DState.IO.Delete<Group> ( temp );
            __Groups.refresh ( );
            //_Groups.Remove ( temp );
            temp.Championship = null;
        }


        #endregion

#else 
        public List<Team> listAllTeams ( )
        {
            return new List<Team> ( );
        }

#endif

        #region Events

        public AEvent[] Events { get { return _Events.OrderBy(c => c ).ToArray ( ); } }
        //public AEvent[] Events { get { return database.GetEventsForChampionship ( ID ).ToArray ( ); } }

        public virtual void addEvent(string EventName, ResultDisplayDescription ResultType, int MaxSquadSize = 1, int MinSquadSize = 0)
        {
            if (MaxSquadSize == 1)
                addEvent(new IndividualDistanceEvent(EventName, ResultType, this));
            else
                throw new NotImplementedException();
                //addEvent(new SquadEvent(EventName, this, MaxSquadSize, MinSquadSize));
        }

        public virtual void addEvent(AEvent Event)
        {
            Event.DState = DState.IO.getDataState ( );
            Event.Championship = this;
            Event._Championship_ID = ID;
            __Events.refresh ( );
            //_Events.Add(Event);
            //!*!
            DState.IO.Add<AEvent> ( Event );
        }

        public void RemoveEvent ( AEvent Event )
        {
            Event.Championship = null;
            //!*!
            DState.IO.Delete<AEvent> ( Event );
            __Events.refresh ( );
            //_Events.Remove ( Event );
        }


        /// <summary>
        /// returns all events that are not heats.
        /// </summary>
        /// <returns></returns>
        public virtual ICollection<AEvent> listAllEvents()
        {
            return Events.Where(p => p.GetType().GetInterfaces().Contains(typeof(IHeatEvent)) == false).ToList();

        }

        public static List<AEvent> listAllAvailableEvents(Championship Championship, Athlete Athlete)
        {
            List<AEvent> Events = new List<AEvent>();

            // Can not enter an athlete that isn't in a team.
            if ( Athlete.getTeam ( Championship ) == null ) return Events; 

            foreach (AEvent Event in Championship.listAllEvents())
                //if (Event.isAvailable( Athlete ) )
                    if (Event.canBeEntered(Athlete))
                        Events.Add(Event);

            return Events;
        }

        public List<AEvent> listAllAvailableEvents(Athlete Athlete)
        {
            List<AEvent> Events = new List<AEvent>();

            foreach (AEvent Event in listAllAvailableEvents(this, Athlete))
                //if (Event.canBeEntered(Athlete))
                    Events.Add(Event);

            return Events;


            //return listAllAvailableEvents(this, Athlete);
        }

#endregion

#if ( !TeamTesting )

#region ChampionshipScoreing

        public ChampionshipTeamResult[] getOverallSores()
        {
            List<ChampionshipTeamResult> CTRs = new List<ChampionshipTeamResult>();

            List<ScoringTeam> STs = new List<ScoringTeam>();
            foreach (Team t in listAllTeams())
            {
                ChampionshipTeamResult stA = new ChampionshipTeamResult("A") { Team = t };
                ChampionshipTeamResult stB = new ChampionshipTeamResult("B") { Team = t };

                CTRs.Add(stA);
                CTRs.Add(stB);
            }

            foreach (AEvent Event in listAllEvents())
                STs.AddRange(Event.getScoringTeams());

            ChampionshipTeamResult.assignPointsAndRanks(CTRs.Where(p => p.ScoringTeamName == "A").ToArray(), STs.Where(p => p.ScoringTeamName == "A").ToArray());
            ChampionshipTeamResult.assignPointsAndRanks(CTRs.Where(p => p.ScoringTeamName == "B").ToArray(), STs.Where(p => p.ScoringTeamName == "B").ToArray());

            return CTRs.ToArray();
        }

        [Obsolete ("This funciton requires CustomData - AgeGroup which is depreciated",true)]
        public ChampionshipTeamResult[] getOverallSores(string AgeGroup)
        {
            List<ChampionshipTeamResult> CTRs = new List<ChampionshipTeamResult>();

            List<ScoringTeam> STs = new List<ScoringTeam>();
            foreach (Team t in listAllTeams())
            {
                ChampionshipTeamResult stA = new ChampionshipTeamResult("A") { Team = t };
                ChampionshipTeamResult stB = new ChampionshipTeamResult("B") { Team = t };

                CTRs.Add(stA);
                CTRs.Add(stB);
            }

            // only get results in this age group
            foreach (AEvent Event in listAllEvents())
            {
                if (Event.customFieldExists("AgeGroup"))
                {
                    if (Event.getValue("AgeGroup").ToString() == AgeGroup)
                    {
                        STs.AddRange(Event.getScoringTeams());
                    }
                }
            }

            ChampionshipTeamResult.assignPointsAndRanks(CTRs.Where(p => p.ScoringTeamName == "A").ToArray(), STs.Where(p => p.ScoringTeamName == "A").ToArray());
            ChampionshipTeamResult.assignPointsAndRanks(CTRs.Where(p => p.ScoringTeamName == "B").ToArray(), STs.Where(p => p.ScoringTeamName == "B").ToArray());

            return CTRs.ToArray();
        }

        public ChampionshipTeamResult[] getOverallSores ( Group[] Groups )
        {
            List<ChampionshipTeamResult> CTRs = new List<ChampionshipTeamResult>();

            List<ScoringTeam> STs = new List<ScoringTeam>();
            foreach ( Team t in listAllTeams ( ) )
            {
                ChampionshipTeamResult stA = new ChampionshipTeamResult("A") { Team = t };
                ChampionshipTeamResult stB = new ChampionshipTeamResult("B") { Team = t };

                foreach ( Group g in Groups )
                {
                    stA.Description += g.Name + " ";
                    stB.Description += g.Name + " ";
                }

                stA.Description = stA.Description.Trim ( );
                stB.Description = stB.Description.Trim ( );

                CTRs.Add ( stA );
                CTRs.Add ( stB );
            }

            // only get results in this age group
            foreach ( AEvent Event in listAllEvents ( ) )
            {
                bool inGroup = true;

                foreach ( Group g in Groups )
                    if ( !Event.hasGroup ( g ) )
                    {
                        inGroup = false;
                        break;
                    }

                if ( !inGroup ) continue;

                STs.AddRange ( Event.getScoringTeams ( ) );
            }

            ChampionshipTeamResult.assignPointsAndRanks ( CTRs.Where ( p => p.ScoringTeamName == "A" ).ToArray ( ) , STs.Where ( p => p.ScoringTeamName == "A" ).ToArray ( ) );
            ChampionshipTeamResult.assignPointsAndRanks ( CTRs.Where ( p => p.ScoringTeamName == "B" ).ToArray ( ) , STs.Where ( p => p.ScoringTeamName == "B" ).ToArray ( ) );

            return CTRs.ToArray ( );
        }

        #endregion
#endif
        #region EventRestrictions

        public Group[] Groups { get { return _Groups.ToArray(); } }

        public void addGroup(Group group)
        {
            group.Championship = this;
            group._Championship_ID = ID;
            //_Groups.Add(group);
            //!*!
            DState.IO.Add<Group> ( group );

            __Groups.refresh ( );
        }

        #region Restriction Factory

        public GenderRestriction newMaleRestriction()
        {
            GenderRestriction gr = new GenderRestriction(this);
            gr.allowMale();
            return gr;
        }

        public GenderRestriction newFemaleRestriction()
        {
            GenderRestriction gr = new GenderRestriction(this);
            gr.allowFemale();
            return gr;
        }

        public AgeRestriction newAgeRestriction(int minAge, int maxAge, DateTime referenceDate)
        {
            AgeRestriction ar = new AgeRestriction(this);

            ar.dateReference = referenceDate;
            ar.minAge = minAge;
            ar.maxAge = maxAge;

            return ar;
        }

#endregion

#endregion
#if ( !TeamTesting )
        public List<Athlete> ListAllAthletes()
        {
            List<Athlete> Athletes = new List<Athlete>();

            foreach (Team t in Teams)
                Athletes.AddRange(t.getAllAthletes());

            return Athletes;
        }
#endif 
        public List<ACompetitor> ListAllCompetitors()
        {
            List<ACompetitor> Competitors = new List<ACompetitor>();

            foreach (AEvent e in Events)
                Competitors.AddRange(e.EnteredCompetitors);

            return Competitors;
        }

        public List<ACompetitor> ListAllAvailableCompetitors()
        {
            return ListAllCompetitors().Where(c => c.AvilableForSW == true).ToList();
        }

        public List<ACompetitor>getCompetitor(string Vest)
        {
            if (string.IsNullOrWhiteSpace(Vest))
                return null;

            List<ACompetitor> comps = new List<ACompetitor>();
            foreach (AEvent Event in listAllEvents())
                comps.AddRange(Event.getEnteredCompetitors().Where(c => c.Vest.ToString() == Vest));

            return comps;
        }

        public Athlete getAthlete(int ID)
        {
            if (ID == 0)
                return null;

            return ListAllAthletes().Where(a => a.ID == ID).FirstOrDefault();
        }


        public VestActions[] VestActions { get { return _VestActions.ToArray(); } }

        public void AddVestAction(VestActions va)
        {
            va.Championship = this;
            //!*!
            DState.IO.Add<VestActions> ( va );
            __VestActions.refresh ( );
            Save ( );
            //_VestActions.Add(va);
        }

#region CustomData

        public ACustomDataValue[] CustomDataStore { get { return _CustomDataStore.ToArray(); } }

        public void addCustomDataValue ( ACustomDataValue Value )
        {
            if ( Value != null )
            {
                Value.Championship = this;
                DState.IO.Add<ACustomDataValue> ( Value );
                //_CustomDataStore.Add ( Value );
            }
        }

        public void removeCustomDataValue ( ACustomDataValue Value )
        {
            if ( Value != null )
                //!**!
                DState.IO.Delete<ACustomDataValue> ( Value );
                //_CustomDataStore.Remove ( Value );
        }

        public void createIntField(string key)
        {
            CustomData.createIntField ( CustomDataStore , this , key );
        }

        public void createStringField(string key)
        {
            CustomData.createStringField ( CustomDataStore , this , key );
        }

        public void deleteField(string key)
        {
            CustomData.deleteField ( CustomDataStore , this , key );
        }

        public bool customFieldExists(string key)
        {
            return CustomData.customFieldExists(CustomDataStore, key);
        }

        public object getValue(string key)
        {
            return CustomData.getValue(CustomDataStore, key);
        }

        public void setValue(string key, object value)
        {
            __CustomDataStore.refresh( );
            CustomData.setValue(CustomDataStore, key, value, this);
        }

        public void clearAllFields()
        {
            foreach (var field in CustomDataStore)
                deleteField(field.key);
        }

#endregion

#region Locking

        public bool isLocked() { return ! ( Locked == null ) ; }

        /// <summary>
        /// Locks this championship and sets the unlock password.
        /// If the Championship is already locked this will throw an exception 
        /// </summary>
        public void Lock(string Password = "")
        {
            if (isLocked())
                throw new Exception("Championship is already locked.");
            else
            {
                if (Password == null)
                    Locked = "";
                else
                    Locked = Password;
            }
        }

        /// <summary>
        /// Unlocks the championship if the password matches the lock password.
        /// If the password does not match then an exception is thrown.
        /// </summary>
        /// <returns>isLocked()</returns>
        public bool Unlock(string Password = "")
        {
            if (Locked != Password)
                throw new Exception("The entered password did not match.");
            else
                Locked = null;

            return isLocked();
        }

#endregion

        public DateTime? AgeDateReference { get { return _AgeDateReference; } set { _AgeDateReference = value; } }
#if ( !TeamTesting )
        public List<School> getSchools()
        {
            List<School> schools = new List<School>();

            foreach (Team t in Teams)
                schools.AddRange(t.HasSchools);

            return schools;
        }
#endif 
        public override string ToString()
        {
            if (Name == null)
                return string.Empty;

            return Name.ToString();
        }

        /// <summary>
        /// Check if there are any child objects that can not be deleted.
        /// </summary>
        /// <returns></returns>
        public bool CanDelete()
        {
            foreach (AEvent Event in Events)
                if (!Event.CanDelete())
                    return false;

            return true;
        }

        public override bool Equals ( object obj )
        {
            try
            {
                return ( (Championship)obj ).ID == this.ID;
            }
            catch ( Exception )
            {
                return base.Equals ( obj );
            }
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public static bool operator == ( Championship x , Championship y )
        {
            if ( ( (object)x ) == null && ( (object)y ) == null ) return true;
            if ( ( (object)x ) == null ) return false;
            if ( ( (object)y ) == null ) return false;

            return x.ID == y.ID;
        }

        public static bool operator != ( Championship x , Championship y )
        {
            return !( x == y );
        }


    }

}
