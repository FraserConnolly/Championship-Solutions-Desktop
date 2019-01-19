/*
 *  Filename         : Event.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 *    2015-05-02
 *      Added int Lanes to EventRanges
 *      Added ResultsDisplayDescription
 *      Added ResultsTemplateName field
 *      Added DataEntryTemplateName field
 *      Added CertificateTemplateName field
 *      Added Standards
 *      Modified AddResults functional definition to accept ResultValue instead of TimeSpan
 *      Changed IndividualEvent to AIndividualEvent
 *      Added IndividualDistanceEvent
 *      Added IndividualTimedHeatEvent
 *      Added IndividualTimedFinalEvent
 *      
 *  2015-05-16
 *      Huge overhaul
 *      Introduction of IIndividualEvent and ISquadEvent
 *
 *  2015-10-21 
 *      Conversion to work with Linq to SQL for use with SQLite database
 *
 *  2016-03-22
 *      Split Linq data into separate file
 *      bool CanDelete(); Added
 *
 *  2016-06-04
 *      Added #define ReorderResults - if this is defined then if a new result is added then it will
 *           push other results out of the way, if not an exception will be thrown. Similarly if a result
 *           is removed it either will bring all ranks lower than it up one or will leave space.
 *
 */

//#define ReorderResults
#define WSAACrossCountry

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using Itenso.TimePeriod;
using System.Text;
using System.Text.RegularExpressions;

namespace ChampionshipSolutions.DM
{

    public enum EventState
    {
        New = 0,
        AthletesEntered,
        ResultsEntered,
        CertificatesPrinted,
        SelectionComplete,
        Locked
    }

    /// <summary>
    /// Defines Min and Max number of Competitors and the Max number of guests for an event.
    /// </summary>
    public class EventRanges
    {
        public static EventRanges copyEventRanges(EventRanges copyRanges)
        {
            EventRanges eventRanges = new EventRanges( )
            {
                Lanes = copyRanges.Lanes ,
                MaxCompetitors = copyRanges.MaxCompetitors ,
                MaxCompetitorsPerTeam = copyRanges.MaxCompetitorsPerTeam ,
                MaxGuests = copyRanges.MaxGuests ,
                MinCompetitors = copyRanges.MinCompetitors ,
                ScoringTeams = copyRanges.ScoringTeams ,
                TeamASize = copyRanges.TeamASize ,
                TeamBForScoringTeamOnly = copyRanges.TeamBForScoringTeamOnly ,
                TeamBSize = copyRanges.TeamBSize ,
                TopIndividualCertificates = copyRanges.TopIndividualCertificates ,
                TopLowerYearGroupInividualCertificates = copyRanges.TopLowerYearGroupInividualCertificates
            };
            return eventRanges;
        }

        public int MaxCompetitors { get; set; }
        public int MinCompetitors { get; set; }
        public int MaxGuests { get; set; }
        public int MaxCompetitorsPerTeam { get; set; }

        /// <summary>
        /// The first x athletes who will be issued a finishing certificate.
        /// </summary>
        public int TopIndividualCertificates { get; set; }

        /// <summary>
        /// The first x athletes of the lower year group who will be issued a finishing certificate.
        /// Applies to StudentAthletes only.
        /// </summary>
        public int TopLowerYearGroupInividualCertificates { get; set; }

        /// <summary>
        /// The number of competitors in a scoring team
        /// </summary>
        public int TeamASize { get; set; }

        /// <summary>
        /// The number of competitors in the B scoring team
        /// </summary>
        public int TeamBSize { get; set; }

        /// <summary>
        /// If true certificates will only be produced for competitors in the scoring part of Team B.
        /// If false certificates will be produced for all competitors who are in team B and who did 
        /// finish but did not get into team A or B.
        /// </summary>
        public bool TeamBForScoringTeamOnly { get; set; }

        /// <summary>
        /// Defines how many scoring teams are eligible for certificates.
        /// set to 0 for no scoring teams.
        /// set to -1 for all teams.
        /// </summary>
        public int ScoringTeams { get; set; }

        /// <summary>
        /// Defines how many lanes are used for track events.
        /// set to 0 for events that do not use lanes.
        /// </summary>
        public int Lanes { get; set; }

    }

    public partial class EventGroups 
    {
        public AEvent Event { get { return _Event ; } set { _Event = value; } }

        public Group Group { get { return _Group; } set { _Group = value; } }

        public override string ToString ( )
        {
            return this.Event.Name + " " + Group.Name ;
        }

    }

    public abstract partial class AEvent : IGroups, IEvent, IResults, ICustomData, IStandards, IIdentity, IComparable<AEvent> 
    {

        public int CompareTo ( AEvent other )
        {

            if ( int.TryParse( this.ShortName , out int ThisEventNumber ) &&
                int.TryParse( other.ShortName , out int OtherEventNumber ) )
            {
                // Short Name is a number 
                return ThisEventNumber.CompareTo( OtherEventNumber );
            }
            else
            {
                var r = new Regex(@"([a-zA-Z]+)(\d+)" );

                if ( r.IsMatch( ShortName ) && r.IsMatch( other.ShortName ) )
                {
                    // regex matches
                    int comp;

                    comp = string.Compare( r.Match( ShortName ).Groups [ 1 ].ToString( ) , r.Match( other.ShortName ).Groups [ 1 ].ToString( ) );

                    if ( comp == 0 )
                        return int.Parse( r.Match( ShortName ).Groups [ 2 ].ToString( ) ).CompareTo( int.Parse( r.Match( other.ShortName ).Groups [ 2 ].ToString( ) ) );
                    else
                        return comp;

                }
                else
                {
                    return string.Compare( ShortName , other.ShortName );
                }
            }
        }

        public EventState State { get { return _State; } set { _State = value; } }

        #region Names

        public string Name { get { return _Name ?? ""; } set { _Name = value; } }

        /// <summary>
        /// Get or Set the short name of this team.
        /// By default this is the first 4 characters without any spaces.
        /// </summary>
        public string ShortName
        {
            get
            {
                if ( _ShortName != null )
                    return _ShortName;

                if ( Name == null )
                    return null;

                if ( Name.Length > 4 )
                    return Name.Replace ( " " , string.Empty ).Substring ( 0 , 4 ).Trim ( );
                else
                    return Name.Trim ( );
            }
            set
            {
                _ShortName = value;
            }
        }

        public virtual string PrintableName { get { return Name; } }

        #endregion

        public string Description { get { return _Description; } set { _Description = value; } }

        public DateTime? StartTime
        {
            get
            {
                if ( _StartTime == null )
                    return null;
                return DateTime.Parse( _StartTime );
            }
            set
            {
                if ( value == null )
                    _StartTime = null;
                else
                    _StartTime = value.Value.TimeOfDay.ToString ( );
            }
        }
        public int Length { get { return _EndTime; } set { _EndTime = value; } }

        public virtual string getEventType (  )
        {
            return "Abstract Event";
        }

        public static AEvent CopyEvent(AEvent Event, Championship Championship)
        {

            // Note I've had to use if statements rather than switch statements because of Entity Frameworks Dynamic Proxies

            if ( Event.GetType() == typeof(SquadTimedEvent) )
                    return new SquadTimedEvent((SquadTimedEvent)Event, Championship);
                
            if ( Event.GetType( ) == typeof( SquadDistanceEvent ))
                    return new SquadDistanceEvent((SquadDistanceEvent)Event, Championship);
              
            if ( Event.GetType( ) == typeof( IndividualTimedSchoolEvent ))
                    return new IndividualTimedSchoolEvent((IndividualTimedSchoolEvent)Event, Championship);
                
            if ( Event.GetType( ) == typeof( IndividualTimedFinalEvent ))
                    return new IndividualTimedFinalEvent((IndividualTimedFinalEvent)Event, Championship);
                
            if ( Event.GetType( ) == typeof( IndividualTimedFinalSchoolEvent ))
                    return new IndividualTimedFinalSchoolEvent((IndividualTimedFinalSchoolEvent)Event, Championship);
                
            if ( Event.GetType( ) == typeof( IndividualDistanceEvent ))
                    return new IndividualDistanceEvent((IndividualDistanceEvent)Event, Championship);

            if (Event.GetType( ) == typeof( IndividualTimedEvent ))
                return new IndividualTimedEvent((IndividualTimedEvent)Event, Championship);
            
            return null;
        }

        public Championship Championship { get { return _Championship; } set { _Championship = value; } }

        #region Championship Best Performance

        public ResultValue CountyBestPerformance
        {
            get
            {
                return new ResultValue ( this.ResultsDisplayDescription )
                    { RawValue = _CountyBestPerformance_RawValue } ;
            }
            set
            {
                if ( value == null )
                    _CountyBestPerformance_RawValue = 0;
                else
                    _CountyBestPerformance_RawValue = value.RawValue;
            }
        }

        public string CountyBestPerformanceName { get { return  _CountyBestPerformanceName ?? "" ; } set { _CountyBestPerformanceName = value; } }

        public int CountyBestPerformanceYear { get { return _CountyBestPerformanceYear; } set { _CountyBestPerformanceYear = value; } }

        public string CountyBestPerformanceArea { get { return _CountyBestPerformanceArea ?? "" ; } set { _CountyBestPerformanceArea = value; } }

        public string PrintChampionshipBestPerformance
        {
            get
            {
                if ( CountyBestPerformance == null ) return string.Empty;
                return CountyBestPerformance.ToString ( );
            }
            set
            {
                ResultValue rv = AEvent.MakeNewResultsValue(this);
                rv.setResultString ( value );
                CountyBestPerformance = rv;
            }
        }

        #endregion

        #region Event Ranges

        public int MaxCompetitors { get { return _MaxCompetitors; } set { _MaxCompetitors = value; } }
        public int MinCompetitors { get { return _MinCompetitors; } set { _MinCompetitors = value; } }
        public int MaxGuests { get { return _MaxGuests; } set { _MaxGuests = value; } }
        public int MaxCompetitorsPerTeam { get { return _MaxCompetitorsPerTeam; } set { _MaxCompetitorsPerTeam = value; } }

        /// <summary>
        /// The first x athletes who will be issued a finishing certificate.
        /// </summary>
        public int TopIndividualCertificates { get { return _TopIndividualCertificates; } set { _TopIndividualCertificates = value; } }

        /// <summary>
        /// The first x athletes of the lower year group who will be issued a finishing certificate.
        /// Applies to StudentAthletes only.
        /// </summary>
        public int TopLowerYearGroupInividualCertificates { get { return _TopLowerYearGroupInividualCertificates; } set { _TopLowerYearGroupInividualCertificates = value; } }

        /// <summary>
        /// The number of competitors in a scoring team
        /// </summary>
        public int TeamASize { get { return _TeamASize; } set { _TeamASize = value; } }

        /// <summary>
        /// The number of competitors in the B scoring team
        /// </summary>
        public int TeamBSize { get { return _TeamBSize; } set { _TeamBSize = value; } }

        /// <summary>
        /// If true certificates will only be produced for competitors in the scoring part of Team B.
        /// If false certificates will be produced for all competitors who are in team B and who did 
        /// finish but did not get into team A or B.
        /// </summary>
        public bool TeamBForScoringTeamOnly { get { return _TeamBForScoringTeamOnly; } set { _TeamBForScoringTeamOnly = value; } }

        /// <summary>
        /// Defines how many scoring teams are eligible for certificates.
        /// set to 0 for no scoring teams.
        /// set to -1 for all teams.
        /// </summary>
        public int ScoringTeams { get { return _ScoringTeams; } set { _ScoringTeams = value; } }

        /// <summary>
        /// Defines how many lanes are used for track events.
        /// set to 0 for events that do not use lanes.
        /// </summary>
        public int Lanes { get { return _Lanes; } set { _Lanes = value; } }

        public EventRanges EventRanges
        {
            get
            {
                return new EventRanges ( )
                {
                    MaxCompetitors = _MaxCompetitors ,
                    MinCompetitors = _MinCompetitors ,
                    MaxCompetitorsPerTeam = _MaxCompetitors ,
                    MaxGuests = _MaxGuests ,
                    TopIndividualCertificates = _TopIndividualCertificates ,
                    TopLowerYearGroupInividualCertificates = _TopLowerYearGroupInividualCertificates ,
                    TeamASize = _TeamASize ,
                    TeamBSize = _TeamBSize ,
                    TeamBForScoringTeamOnly = _TeamBForScoringTeamOnly ,
                    ScoringTeams = _ScoringTeams ,
                    Lanes = _Lanes
                };
            }
        }

            // Event ranges brought inside AEvent in V3-0
            //public EventRanges EventRanges {
            //    get
            //    {
            //        return new EventRanges()
            //        {
            //            MaxCompetitors = _MaxCompetitors,
            //            MinCompetitors = _MinCompetitors,
            //            MaxCompetitorsPerTeam = _MaxCompetitors,
            //            MaxGuests = _MaxGuests,
            //            TopIndividualCertificates = _TopIndividualCertificates,
            //            TopLowerYearGroupInividualCertificates = _TopLowerYearGroupInividualCertificates,
            //            TeamASize = _TeamASize,
            //            TeamBSize = _TeamBSize,
            //            TeamBForScoringTeamOnly = _TeamBForScoringTeamOnly,
            //            ScoringTeams = _ScoringTeams,
            //            Lanes = _Lanes 
            //        };
            //    }
            //    set
            //    {
            //        if (setEventRanges(value))
            //        {
            //            _MaxCompetitors = value.MaxCompetitors;
            //            _MinCompetitors = value.MinCompetitors;
            //            _MaxCompetitorsPerTeam = value.MaxCompetitors;
            //            _MaxGuests = value.MaxGuests;
            //            _TopIndividualCertificates = value.TopIndividualCertificates;
            //            _TopLowerYearGroupInividualCertificates = value.TopLowerYearGroupInividualCertificates;
            //            _TeamASize = value.TeamASize;
            //            _TeamBSize = value.TeamBSize;
            //            _TeamBForScoringTeamOnly = value.TeamBForScoringTeamOnly;
            //            _ScoringTeams = value.ScoringTeams;
            //            _Lanes = value.Lanes;
            //        }
            //    }
            //}

            //private bool setEventRanges (EventRanges newRanges)
            //{

            //    if (newRanges.MaxGuests < 0)
            //        throw new ArgumentOutOfRangeException("MaxGuests", "The maximum number of guests can not be less than 0.");

            //    if (newRanges.MaxCompetitors < 0)
            //        throw new ArgumentOutOfRangeException("MaxCompetitors", "The maximum number of competitors can not be less than 0.");

            //    if (newRanges.MinCompetitors > newRanges.MaxCompetitors)
            //        throw new ArgumentOutOfRangeException("MinCompetitors", "Minimum competitors is greater than the maximum.");

            //    if (newRanges.MaxCompetitors < EnteredCompetitors.Count(i => i.Guest == false))
            //        throw new ArgumentOutOfRangeException("MaxCompetitors", "There are already more than the maximum number of competitors.");

            //    if (newRanges.MaxCompetitors < EnteredCompetitors.Count(i => i.Guest == true))
            //        throw new ArgumentOutOfRangeException("MaxGuests", "There are already more than the maximum number of guests.");

            //    return true;
            //}

            #endregion

        #region Standards

            //public virtual Standards Standards { get { return _Standards; } set { _Standards = value; } }
        public Standard[] Standards { get
            {
                //if ( this.ResultsDisplayDescription == ResultDisplayDescription.DistanceCentimeters ||
                //        this.ResultsDisplayDescription == ResultDisplayDescription.DistanceMeters ||
                //        this.ResultsDisplayDescription == ResultDisplayDescription.DistanceMetersCentimeters )
                //    return Standards.OrderByDescending ( s => s.StandardValue.RawValue ).ToArray ( );
                //else
                //    return Standards.OrderBy ( s => s.StandardValue.RawValue ).ToArray ( );

                return _Standards.ToArray ( );
            }
        }

        /// <summary>
        /// Checks to see if there is a District, County, Entry, National Standard or Championship Best Performance to compare against.
        /// </summary>
        /// <returns>True if there is a standard to compare against</returns>
        public bool hasStandards()
        {
            if ( Standards.Count ( ) == 0 ) return false;

            foreach ( Standard std in Standards )
                if ( Standard.hasStandards ( std ) ) return true;

            return false;
            //return Standards.hasStandards(this.Standards);
        }

        public virtual bool achievedStandard(ResultValue ResultValue)
        {
            return Standard.achievedStandard(this, ResultValue);
        }

        public virtual string getStandardShortString(ResultValue ResultValue)
        {
            return Standard.getStandardShortString(this, ResultValue);
        }

        public void addStandard(Standard standard)
        {
            standard.Event = this;
            this.DState.IO.Add<Standard> ( standard );
            __Standards.refresh ( );
            //_Standards.Add ( standard );
        }

        public void removeStandard(Standard standard)
        {
            DState.IO.Delete<Standard> ( standard );
            __Standards.refresh ( );
            //_Standards.Remove ( standard );
            standard.Event = null;
        }

        #endregion

        #region Constructors

        public AEvent()
        {
            DState = null;
        }

        public AEvent(string Name, ResultDisplayDescription ResultType, Championship Championship) : this() 
        {
            this.Name = Name;
            //EnteredCompetitors = new List<ACompetitor>();
            //_CustomDataStore = new CustomData();

            // this null check should stop linq-to-sql mistakenly thinking we are trying to delete a new event.
            if (Championship != null )
                this.Championship = Championship;
            ResultsDisplayDescription = ResultType;
        }


        /// <summary>
        /// TO Do copy restrictions
        /// </summary>
        /// <param name="Event"></param>
        public AEvent(AEvent Event, Championship Championship) : this ( Event.Name, Event.ResultsDisplayDescription, Championship )
        {

            ShortName = Event.ShortName;
            StartTime = Event.StartTime;
            VestTemplate = Event.VestTemplate;
            ResultsTemplate = Event.ResultsTemplate;
            MaxCompetitors = Event.MaxCompetitors;
            MinCompetitors = Event.MinCompetitors;
            MaxCompetitorsPerTeam = Event.MaxCompetitors;
            MaxGuests = Event.MaxGuests;
            TopIndividualCertificates = Event.TopIndividualCertificates;
            TopLowerYearGroupInividualCertificates = Event.TopLowerYearGroupInividualCertificates;
            TeamASize = Event.TeamASize;
            TeamBSize = Event.TeamBSize;
            TeamBForScoringTeamOnly = Event.TeamBForScoringTeamOnly;
            ScoringTeams = Event.ScoringTeams;
            Lanes = Event.Lanes;

            // 2017-05-12 do not copy over template data for entry forms.
            //Description = Event.Description;
            //DataEntryTemplate = Event.DataEntryTemplate;
            //CertificateTemplate = Event.CertificateTemplate;

            DState = Championship.DState;
            DState.IO.Add<AEvent>( this );

            Standard.CopyStandards( this , Event.Standards );
            CustomData.CopyCustomData( Event.CustomDataStore , this );

            foreach ( EventGroups er in Event.Groups)
            {
                foreach (Group cer in this.Championship.Groups)
                {
                    if (er.Group.Name == cer.Name)
                    {
                        this.addGroup(cer);
                        break;
                    }
                }
            }

            }

        #endregion

        #region Competitors

        public ACompetitor[] EnteredCompetitors { get { return _EnteredCompetitors.ToArray(); }  }

        /// <summary>
        /// Adds the competitor to the database.
        /// </summary>
        /// <param name="competitor"></param>
        protected void AddCompetitor(ACompetitor competitor)
        {
            State = EventState.AthletesEntered;

            competitor.CompetingIn = this;

            //!*!
            ////Championship.database.Add<ACompetitor> ( competitor );
            DState.IO.Add<ACompetitor> ( competitor );
            __EnteredCompetitors.refresh ( );



            Save ( );

            //_EnteredCompetitors.Add(competitor);

            //if ( competitor is Competitor )
            //{
            //( (Competitor)competitor ).Athlete._Competitors.Add ( (Competitor)competitor );
            //}
        }

        /// <summary>
        /// Removes a competitor from the database
        /// </summary>
        /// <param name="competitor"></param>
        protected void RemoveCompetitor(ACompetitor competitor)
        {
            //!**!
            competitor.CompetingIn = null; // marks competitor for deletion 

            //Championship.database.Delete<ACompetitor> ( competitor );
            DState.IO.Delete<ACompetitor> ( competitor );

            //_EnteredCompetitors.Remove(competitor);
        }

        public virtual int countCompetitors() { return EnteredCompetitors.Count(); }

        public virtual bool isEventFull()
        {
            // if Max Competitors is 0 then the event can have an infinite number of competitors.
            if (EventRanges.MaxCompetitors == 0)
                return false;

            return (EnteredCompetitors.Count(i => i.Guest == false) >= EventRanges.MaxCompetitors);
        }

        public virtual bool isTeamFull(Team teamToCheck)
        {
            if (EventRanges.MaxCompetitorsPerTeam == 0)
                return false;

            return (EnteredCompetitors.Count(i => i.Guest == false && i.getTeam() == teamToCheck) >= EventRanges.MaxCompetitorsPerTeam);
        }

        public void enterAthlete(Athlete Athlete, bool Guest = false)
        {
            enterAthlete(Athlete, getNextVest(Athlete), Guest);
        }

        public abstract void enterAthlete(Athlete Athlete, VestNumber Vest, bool Guest = false);

        /// <summary>
        /// Added in V3-0
        /// </summary>
        /// <param name="specialConsideration"></param>
        public void EnterSpecialConsideration(SpecialConsideration specialConsideration)
        {
            AddCompetitor(specialConsideration);
        }

        /// <summary>
        /// Returns true if this athlete can be entered into this event.
        /// Note this will return false if they are already entered, if the team is full or the event is full.
        /// </summary>
        /// <param name="athlete"></param>
        /// <returns></returns>
        public abstract bool canBeEntered(Athlete athlete);

        /// <summary>
        /// Returns all currently entered competitors not including guests and not including special considerations
        /// </summary>
        /// <returns>List of Competitors</returns>
        public virtual List<ACompetitor> getEnteredCompetitors()
        {
            return EnteredCompetitors.Where(i => i.Guest == false && (i is SpecialConsideration) == false ).OrderBy(a => a.Vest.IntOrder).ToList();
        }

        /// <summary>
        /// Returns all currently entered competitors for a Team
        /// </summary>
        /// <returns>List of Competitors</returns>
        public virtual List<ACompetitor> getEnteredCompetitors(Team team)
        {
            return EnteredCompetitors.Where(i => i.Guest == false && ( i is SpecialConsideration ) == false && i.getTeam() == team).OrderBy( a => a.Vest.IntOrder ).ToList();
        }

        public static ACompetitor getCompetitor(AEvent Event, VestNumber vest)
        {
            return ((from c in Event.getEnteredCompetitors() where c.Vest == vest select c).FirstOrDefault());
        }

        /// <summary>
        /// Added in to give support for squad events
        /// </summary>
        /// <param name="Athlete"></param>
        public abstract void removeAthlete(Athlete Athlete);

        public virtual void removeCompetitor(ACompetitor Competitor)
        {
            if (Competitor.getResult() == null)
            {
                if (EnteredCompetitors.Contains(Competitor))
                {
                    //!*!
                    //Championship.database.Delete<ACompetitor> ( Competitor );
                    DState.IO.Delete<ACompetitor> ( Competitor );
                    //_EnteredCompetitors.Remove(Competitor);
                    Competitor.CompetingIn = null; // This will delete the competitor in SQLite

                    __EnteredCompetitors.refresh ( );
                }
            }
            else
            {
                throw new ArgumentException("Can not remove this competitor from this event as they have a result");
            }
        }

        // Changed 2015-05-12 to work better with heats/finals
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Vest"></param>
        /// <returns></returns>
        public virtual ACompetitor getCompetitor(string Vest)
        {
            return this.getEnteredCompetitors().Where(c => c.Vest.ToString() == Vest).FirstOrDefault();
        }

        #endregion

        #region Vests

        /// <summary>
        /// </summary>
        /// <param name="Athlete"></param>
        ///// <returns>Returns the next available vest number </returns>
        protected virtual VestNumber getNextVest( Athlete Athlete )
        //#if ( WSAACrossCountry )
        {
            if ( Championship.ShortName.Contains( "XC" ) )
            {

                Team t = Athlete.getTeam(this.Championship);

                int StartingVestNumber = 0;
                int multiplier = 0;

                switch ( t.Name )
                {
                    case "Avon":
                        StartingVestNumber = 101;
                        break;
                    case "Cornwall":
                        StartingVestNumber = 229;
                        break;
                    case "Devon":
                        StartingVestNumber = 357;
                        break;
                    case "Dorset":
                        StartingVestNumber = 485;
                        break;
                    case "Gloucestershire":
                        StartingVestNumber = 613;
                        break;
                    case "Somerset":
                        StartingVestNumber = 741;
                        break;
                    case "Wiltshire":
                        StartingVestNumber = 869;
                        break;

                    case "Swindon":
                        StartingVestNumber = 485;
                        break;
                    case "West Wiltshire":
                        StartingVestNumber = 613;
                        break;
                    case "Kennet":
                        StartingVestNumber = 101;
                        break;
                    case "North Wiltshire":
                        StartingVestNumber = 229;
                        break;
                    case "Salisbury":
                        StartingVestNumber = 357;
                        break;
                    default:
                        break;
                }

                switch ( this.ShortName )
                {
                    case "MB":
                        multiplier = 0;
                        break;
                    case "JB":
                        multiplier = 2;
                        break;
                    case "IB":
                        multiplier = 4;
                        break;
                    case "SB":
                        multiplier = 6;
                        break;
                    case "MG":
                        multiplier = 1;
                        break;
                    case "JG":
                        multiplier = 3;
                        break;
                    case "IG":
                        multiplier = 5;
                        break;
                    case "SG":
                        multiplier = 7;
                        break;
                    default:
                        break;
                }

                StartingVestNumber = StartingVestNumber + ( 16 * multiplier );

                VestNumber v = new VestNumber ( );


                for ( int i = 0 ; i < 16 ; i++ )
                {
                    v.setVestNumber( StartingVestNumber + i );

                    int q = (from comps in this.getEnteredCompetitors(t) where comps.Vest == v select comps).Count();

                    if ( q == 0 )
                    {
                        return v;
                    }

                }

                throw new ArgumentException( "No remaining vest numbers available" );
            }
            else if ( Championship.ShortName.Contains( "TF" ) )
            {
                int[] StartingVestNumber;
                Team t = Athlete.getTeam(this.Championship);

                if ( t == null )
                    throw new ArgumentException( Athlete.PreferredName + " has not been entered into a team." );

                switch ( t.Name )
                {
                    case "Swindon":
                        StartingVestNumber = new int[] { 7 , 8 , 17 , 18 };
                        break;
                    case "West Wiltshire":
                        StartingVestNumber = new int[] { 9 , 10 , 19 , 20 };
                        break;
                    case "Kennet":
                        StartingVestNumber = new int[] { 1 , 2 , 11 , 12 };
                        break;
                    case "North Wiltshire":
                        StartingVestNumber = new int[] { 3 , 4 , 13 , 14 };
                        break;
                    case "Salisbury":
                        StartingVestNumber = new int[] { 5 , 6 , 15 , 16 };
                        break;
                    default:
                        throw new ArgumentException( "Could not identity team" );
                }

                VestNumber v = new VestNumber();

                for ( int i = 0 ; i < 4 ; i++ )
                {
                    v.setVestNumber( StartingVestNumber[i] );

                    int q = (from comps in this.getEnteredCompetitors(t) where comps.Vest == v select comps).Count();

                    if ( q == 0 )
                    {
                        return v;
                    }

                }

                throw new ArgumentException( "A vest could not be found" );

            }
            else
            {
                return new VestNumber( ) { dbVestNumber = "" };
            }
        }

        #endregion

        #region Results

        public ResultDisplayDescription ResultsDisplayDescription
        {
            get { return (ResultDisplayDescription)_ResultsDisplayDescription; }
            set { _ResultsDisplayDescription = (int)value; }
        }

        // To DO make this a non generic extension class
        public static ResultValue MakeNewResultsValue(AEvent Event, bool DNF = false)
        {
            if (Event == null) return null;

            if (DNF)
                return new ResultValue(ResultDisplayDescription.DNF);
            if (Event is IHeatEvent)
                return new ResultValue(((IHeatEvent)Event).Final.ResultsDisplayDescription);
            else
                return new ResultValue(Event.ResultsDisplayDescription);

        }

        public AResult[] Results { get { return _Results.ToArray(); } }

        public virtual List<AResult> AllResults()
        {
            return Results.ToList();
        }

        public List<AResult> TopIndividuals()
        {
            return Results.OrderBy(f => f.getRank()).Take(EventRanges.TopIndividualCertificates).ToList();
        }

        #region Scoring Teams

        public ScoringTeam[] getScoringTeams ( )
        {
            List<ScoringTeam> scoringTeams = new List<ScoringTeam>();

            foreach ( Team t in this.Championship.listAllTeams ( ) )
            {
                ScoringTeam stA = null, stB = null;
                if ( EventRanges.TeamASize > 0 )
                    stA = new ScoringTeam ( "A" , EventRanges.TeamASize ) { Team = t };
                if ( EventRanges.TeamBSize > 0 )
                    stB = new ScoringTeam ( "B" , EventRanges.TeamBSize ) { Team = t };


                if ( stA != null )
                {
                    List<AResult> resultsA = ScoringTeamA(t);

                    // bodge to tell the difference between scoring for XC and TF
                    if ( Championship.Name.Contains ( "XC" ) )
                    {
                        // with cross country we want the team to be exactly the specified team size, if it is too small return nothing
                        if ( resultsA.Count ( ) == EventRanges.TeamASize )
                            foreach ( AResult result in resultsA )
                                stA.addPosition ( result.Rank.Value );
                        scoringTeams.Add ( stA );
                    }
                    else
                    {
                        // with Track and field we want the team to be up to the specified team size, if it is smaller then that is fine
                        stA.setTeamSize ( -1 );

                        foreach ( AResult result in resultsA )
                            stA.addPosition ( result.Rank.Value );
                        scoringTeams.Add ( stA );
                    }
                }

                if ( stB != null )
                {
                    List<AResult> resultsB = ScoringTeamB(t);

                    if ( resultsB.Count ( ) >= EventRanges.TeamBSize )
                        foreach ( AResult result in resultsB )
                            stB.addPosition ( result.Rank.Value );

                    scoringTeams.Add ( stB );
                }
            }

            ScoringTeam.assignPointsAndRanks ( scoringTeams.Where ( sc => sc.ScoringTeamName == "A" ).ToArray ( ) );
            ScoringTeam.assignPointsAndRanks ( scoringTeams.Where ( sc => sc.ScoringTeamName == "B" ).ToArray ( ) );

            return scoringTeams.OrderBy ( sc => sc.ScoringTeamName ).ThenBy ( sc => sc.orderableRank ( ) ).ToArray ( );
        }

        public List<AResult> getScoringTeamResults(int Rank, string ScoringTeam)
        {
            Team t = getScoringTeam(Rank,ScoringTeam);

            if(t == null)
                return new List<AResult>();

            switch (ScoringTeam)
            {
                case "A":
                    return ScoringTeamA(t);
                case "B":
                    return ScoringTeamB(t);
                default:
                   return new List<AResult>();
            }
        }

        public Team getScoringTeam(int Rank, string ScoringTeam)
        {
            ScoringTeam[] STs = getScoringTeams();

            ScoringTeam st = STs.Where(s => s.Rank == Rank && s.ScoringTeamName == ScoringTeam).FirstOrDefault();

            if (st == null)
                return null;

            return st.Team;
        }

        public List<AResult> ScoringTeamA (Team team)
        {
            if (this.EventRanges.TeamASize == 0)
                return new List<AResult>();

            List<AResult> temp = Results.Where(r => r.printTeam() == team.Name && r.isComplete()).OrderBy(f => f.getRank()).Take(EventRanges.TeamASize).ToList();

            // bodge to tell the difference between scoring for XC and TF
            if (Championship.Name.Contains("XC"))
            {
                // with cross country we want the team to be exactly the specified team size, if it is too small return nothing
                if (temp.Count >= EventRanges.TeamASize) 
                    return temp;
                else
                    return new List<AResult>();
            }
            else
            {
                // with Track and field we want the team to be up to the specified team size, if it is smaller then that is fine
                return temp.Where(t => t.Rank <= EventRanges.TeamASize).ToList();
            }
        }

        public List<AResult> ScoringTeamB(Team team)
        {
            if(this.EventRanges.TeamBSize==0)
                return new List<AResult>(); 

            if(Results.Where(r => r.printTeam() == team.Name && r.isComplete()).Count() >= (EventRanges.TeamASize + EventRanges.TeamBSize))
            {
                List<AResult> temp = Results.Where(r => r.printTeam() == team.Name && r.isComplete()).Except(ScoringTeamA(team)).OrderBy(r => r.getRank()).ToList();

            if (temp.Count >= EventRanges.TeamBSize)
                return temp;
            else
                return new List<AResult>();
            }
            else
            {
                return new List<AResult>();
            }
        }

        /// <summary>
        /// Used in the cross country championships to build team certificates. Not used in T&F.
        /// </summary>
        /// <param name="st">ScoringTeam.Rank must be equal or less than the number of Scoring Teams for this event or an exception will be thrown.</param>
        /// <returns>List of CertificateData structure. 
        ///     Can return with an empty list. 
        ///     Will not return null.</returns>
        protected List<CertificateData> getCertificatesTeams ( ScoringTeam st )
        {
            List<CertificateData> CD = new List<CertificateData>();

            // Parameter requested a scoring team that does not qualify for certificates
            if ( st.Rank > EventRanges.ScoringTeams )
                throw new ArgumentException ( "Rank" , "The Rank parameter must be equal or less than the number of Scoring Teams eligible for this event" );

            List<AResult> tResults = new List<AResult>();

            switch ( st.ScoringTeamName )
            {
                case "A":
                    //// For WSAA 2014-15
                    tResults = ScoringTeamA ( st.Team );
                    // For SW 2014-15
                    // tResults = this.getResultsForTeam(st.Team);// ScoringTeamA(st.Team);
                    break;
                case "B":
                    tResults = ScoringTeamB ( st.Team );
                    break;
                default:
                    break;
            }

            //ScoringTeam st = getScoringTeams().Where( f => f.Team == Team && f.ScoringTeamName == ScoringTeam).FirstOrDefault();

            if ( st == null )
                throw new ArgumentException ( "ScoringTeam" , "There is no scoring team by this name" );

            int counter = 1;

            foreach ( AResult Result in tResults )
                CD.AddRange ( buildCertificateData ( Result.Competitor , "Team" , counter++ , st ) );

            return CD;
        }

        #endregion

        #region Results Inserting / Editing

        public int getNextResultRank()
        {
            int nextResult = 1;

            foreach (AResult result in ( from r in Results where r.Rank.HasValue orderby r.Rank.Value ascending select r ) )
                nextResult = result.Rank.Value + 1;

            return nextResult;
        }

        public bool isRankAvailable(int Rank)
        {
            return ((from r in Results where r.isPlaceholder() == false && r.Rank == Rank select r).Count() == 0);
        }

        public void moveResultUp(AResult result)
        {
            swapResult(result, result.Rank.Value - 1);
            //swapResult(result, Results.Where(r => r.Rank == result.Rank - 1).FirstOrDefault());
        }

        public void moveResultDown(AResult result)
        {
            swapResult(result, result.Rank.Value + 1);
            //swapResult(result, Results.Where(r => r.Rank == result.Rank + 1).FirstOrDefault());
        }

        private void swapResult(AResult res1, int Rank)
        {
            if (res1 == null) return;
            if (Rank < 1) return;

            if (res1.Rank == Rank) return;

            if (isRankAvailable(Rank))
            {
                res1.Rank = Rank;
            }
            else
            {
                swapResult(res1, Results.Where( r => r.Rank == Rank ).FirstOrDefault());
            }

            //!*!
            //Championship.database.Update<AResult> ( res1 );
            DState.IO.Update<AResult> ( res1 );
        }

        private void swapResult(AResult res1, AResult res2)
        {
            if (res1 == null) return;
            if (res2 == null) return;

            int? res1Rank = res1.Rank;
            int? res2Rank = res2.Rank;

            res1.Rank = res2Rank;
            res2.Rank = res1Rank;

            //!*!
            //Championship.database.Update<AResult> ( res1 );
            DState.IO.Update<AResult> ( res1 );
            //Championship.database.Update<AResult> ( res2 );
            DState.IO.Update<AResult> ( res2 );


        }

        public void removeResult(AResult result)
        {
            if (result == null) throw new ArgumentNullException();

#if ( ReorderResults )
            foreach (AResult r in Results.Where(r => r.Rank > result.Rank))
                r.Rank--;
#endif

            //!*!
            //Championship.database.Delete<AResult> ( result );
            DState.IO.Delete<AResult> ( result );
            __Results.refresh ( );
            //result.Competitor.Result = null;
            ////_Results.Remove(result);
            result.Event = null;
        }

        public void removeResult(int Rank)
        {
            AResult result = getResult(Rank);

            if (result == null)
                return;

            this.removeResult(result);
        }

        public void insertRankSpace ( int Rank )
        {
            foreach ( AResult r in Results.Where ( r => r.Rank >= Rank ) )
                r.Rank++;

            AddPlaceholderResult ( Rank );
        }

        /// <summary>
        /// Removes a placeholder result without changing any other ranks
        /// </summary>
        /// <param name="Rank"></param>
        protected void removePlaceholder ( int Rank )
        {
            // oldResult will only have a value if the oldResult is a placeholder
            AResult oldResult = getResult(Rank);
            if ( oldResult != null )
                if ( oldResult.getTypeDescription ( ) == ResultTypeDescription.Placeholder )
                    removeResult ( oldResult );
            //Results.Remove(oldResult);
        }

#if ( ReorderResults )
        protected void makeSpaceForResult ( int rank )
        {
            if ( !isRankAvailable ( rank ) )
                // need to make a space
                foreach ( AResult r in Results.Where ( r => r.Rank >= rank ) )
                    r.Rank++;
        }
#endif

#region Add Results

        /// <summary>
        /// Adds result to the database.
        /// </summary>
        /// <param name="result"></param>
        protected void AddResult ( AResult result )
        {
            State = EventState.ResultsEntered;
            result._Event_ID = ID;

            //_Results.Add ( result );

            //!*! check for competitor id being stored
            //Championship.database.Add<AResult> ( result );
            DState.IO.Add<AResult> ( result );
            __Results.refresh ( );

            Save ( );
        }

        protected abstract AResult addResult ( int Rank , ACompetitor Competitor , ResultValue resultValue , string VestNumber = "" );
        //protected abstract AResult addResult (int Rank, ACompetitor Competitor, ResultValue resultValue);

        public AResult AddResult ( int Rank , ACompetitor Competitor , ResultValue resultValue ) { return addResult ( Rank , Competitor , resultValue , Competitor.Vest.ToString ( ) ); }
        public AResult AddResult ( ACompetitor Competitor ) { return addResult ( 0 , Competitor , new ResultValue ( this.ResultsDisplayDescription ) , Competitor.Vest.ToString ( ) ); }
        public AResult AddResult ( int Rank , ResultValue resultValue ) { return addResult ( Rank , null , resultValue ); }
        public AResult AddResult ( ACompetitor Competitor , ResultValue resultValue ) { return addResult ( 0 , Competitor , resultValue , Competitor.Vest.ToString ( ) ); }
        public AResult AddResult ( VestNumber vest , ResultValue resultValue ) { return addResult ( 0 , AEvent.getCompetitor ( this , vest ) , resultValue , vest.ToString ( ) ); }
        public AResult AddResult ( int Rank , VestNumber vest , ResultValue resultValue ) { return addResult ( Rank , AEvent.getCompetitor ( this , vest ) , resultValue , vest.ToString ( ) ); }
        public AResult AddResult ( int Rank , VestNumber vest ) { return addResult ( Rank , AEvent.getCompetitor ( this , vest ) , new ResultValue ( this.ResultsDisplayDescription ) , vest.ToString ( ) ); }
        public AResult AddResult ( int Rank , ACompetitor Competitor ) { return addResult ( Rank , Competitor , new ResultValue ( this.ResultsDisplayDescription ) , Competitor.Vest.ToString ( ) ); }
        public AResult AddPlaceholderResult ( int Rank = 0 ) { return addResult ( Rank , null , new ResultValue ( this.ResultsDisplayDescription ) ); }

        public AResult AddDNF ( ACompetitor Competitor )
        {
            AResult newResult = new Result() { Event = this, Competitor = Competitor , Value = AEvent.MakeNewResultsValue(this,true)};
            AddResult ( newResult );
            return newResult;
        }

#endregion

#endregion

        public AResult getResult(int Rank) { return Results.ToList().Where(r => r.Rank == Rank).FirstOrDefault(); }

        public AResult getResult(ACompetitor Competitor) { return Results.Where(r => r.Competitor == Competitor).FirstOrDefault(); }

        /// <summary>
        /// Checks to see if a Competitor has already got a result in this event.
        /// </summary>
        /// <param name="Competitor"></param>
        /// <returns>True if a result is available for this competitor.</returns>
        public bool hasResultFor(ACompetitor Competitor)
        {
            return getResult(Competitor) != null;
        }

        public List<AResult> getResultsForTeam(Team team) { return Results.Where(r => r.printTeam() == team.Name).OrderBy(r => r.Rank).ToList(); }

#endregion

        #region Certificates

        /// <summary>
        /// Builds the certificate data structure for a competitor
        /// </summary>
        /// <param name="Competitor"></param>
        /// <param name="CertificateType"></param>
        /// <param name="counter"></param>
        /// <param name="ScoringTeam"></param>
        /// <returns>List of CertificateData structure. 
        ///     Can return with an empty list. 
        ///     Will not return null.</returns>
        internal virtual List<CertificateData> buildCertificateData ( ACompetitor Competitor , string CertificateType , int counter , ScoringTeam ScoringTeam = null )
        {
            List<CertificateData> temp = new List<CertificateData>();

            if ( Competitor == null )
                return temp;

            if ( Competitor.getResult ( ) == null )
                return temp;

            if ( !Competitor.getResult ( ).Rank.HasValue )
                // this competitor was not ranked.
                return temp;

            string SchStr = Competitor.printParameter("Attends");

            // modified 2015-06-07 to add PrintName 

            CertificateData CD = new CertificateData()
            {
                Championship = this.Championship.ToString(),
                TeamName = Competitor.getTeam().Name,
                CompetitorsName = Competitor.getName(),
                EventName = this.Name,
                SchoolName = SchStr,
                CertifiacteType = CertificateType,
                Competitor = Competitor,
                RankCounter = counter
            };

            if ( this.customFieldExists ( "PrintedName" ) )
            {
                CD.EventName = this.getValue ( "PrintedName" ).ToString ( );
            }



            switch ( CertificateType )
            {
                case "TopIndividuals":

                    CD.CertificateName = "Top Individuals";

                    // CompetitorsName updated for SW 2014-15
                    //CD.CompetitorsName = String.Format("{0} - {1}", Competitor.getName(), Competitor.getTeam().Name);
                    CD.CompetitorsName = String.Format ( "{0}" , Competitor.getName ( ) );

                    // SchoolName field updated for SW 2014-15
                    CD.SchoolName = String.Format ( "{0} - {1}" , Competitor.getTeam ( ).Name , SchStr );

                    CD.Rank = AResult.IntToString ( Competitor.getResult ( ).Rank.Value ) + " " + Competitor.getResult().printResultValue ;

                    // To do 2016-05-14 add standards achieved to the certificate.

                    //CD.Rank = CD.Competitor.Result.printRankAndResult();
                    break;
                case "TopLowerYearGroupIndividuals":
                    // Not used in SW 2014-15 or T&F
                    CD.CertificateName = "Top Lower Year Group Individuals";
                    CD.CompetitorsName = String.Format ( "{0} - {1} - ({2})" , Competitor.getName ( ) , Competitor.getTeam ( ).Name , Competitor.getResult ( ).Rank.Value.ToString ( ) );

                    CD.Rank = string.Format ( "{0} Year {1}" , AResult.IntToString ( counter ) , ( (StudentCompetitor)Competitor ).YearGroup.ToString ( ) );

                    break;
                case "Team":
                    // updated for SW 2014-15
                    //CD.CompetitorsName = String.Format("{0} - {1} - ({2})", Competitor.getTeam().Name, Competitor.getName(), Competitor.Result.Rank.Value.ToString());
                    CD.CompetitorsName = String.Format ( "{1} - ({2})" , Competitor.getTeam ( ).Name , Competitor.getName ( ) , Competitor.getResult ( ).Rank.Value.ToString ( ) );
                    // SchoolName field updated for SW 2014-15
                    CD.SchoolName = String.Format ( "{0} - {1}" , Competitor.getTeam ( ).Name , SchStr );

                    if ( ScoringTeam != null )
                    {
                        // updated for SW 2014-15
                        // CD.Rank = string.Format("{0} Team", AResult.IntToString(ScoringTeam.Rank), ScoringTeam.ScoringTeamName);
                        CD.Rank = string.Format ( "{0} {1} Team" , AResult.IntToString ( ScoringTeam.Rank ) , ScoringTeam.ScoringTeamName );
                        CD.CertificateName = CD.Rank;
                    }

                    CD.CertificateName = CD.Rank;

                    break;
                default:
                    break;
            }

            temp.Add ( CD );

            if ( temp.Count > 0 )
            {
                State = EventState.CertificatesPrinted;
                Save ( );
            }

            return temp;
        }

        /// <summary>
        /// Gets all CertificateData structures for top individuals, top lower year group and scoring teams.
        /// </summary>
        /// <returns></returns>
        public List<CertificateData> getCertificateData()
        {
            List<CertificateData> CD = new List<CertificateData>();

            CD.AddRange(getCertificatesTopIndividual());

            if (this is ISchoolEvent)
                CD.AddRange( ((ISchoolEvent)this) .getCertificatesLowerYearGroup());

            foreach (ScoringTeam st in getScoringTeams().Where(f => f.Rank <= EventRanges.ScoringTeams && f.Rank != 0))
                CD.AddRange(getCertificatesTeams(st));

            return CD;
        }

        // 2017-01-10 - Attempt at a more efficient way to determine if an athlete has earned a certificate.

        public bool hasEarnedCertificate ( ACompetitor competitor )
        {
            if ( competitor == null ) return false;

            if ( competitor.CompetingIn != this ) return false;

            if ( competitor.Result == null ) return false;

            if ( ! competitor.Result.Rank.HasValue ) return false;

            // Are they in the top x athletes
            if ( competitor.Result.Rank.Value <= this.EventRanges.TopIndividualCertificates ) return true;

            // is this a schools event
            if ( this is ISchoolEvent )
                if ( this.EventRanges.TopLowerYearGroupInividualCertificates != 0 )
                    if ( competitor.checkParameter ( "YearGroup" ).ToString ( ) == ( (ISchoolEvent)this ).LowerYearGroup.ToString ( ) )
                        // we need to check if they were in the top x of the lower year group
                        if ( IndividualTimedSchoolEvent.TopLowerYearGroup ( (ISchoolResultsEvent)this ).Contains ( competitor.Result ) )
                            return true;

            // now we need to check if they are in a team.

            // there is no team A so we can also assume there will be no be team either
            if ( this.EventRanges.TeamASize == 0 ) return false;

            ScoringTeam [] st = getScoringTeams ();

            List<ScoringTeam> reliventTeams =  st.Where ( x => x.Team == competitor.Team && x.Rank <= this.EventRanges.ScoringTeams && x.Points != 0 ).ToList();

            // This competitor's team has not achieved a certification rank
            if ( reliventTeams.Count == 0 ) return false; 

            foreach ( ScoringTeam t in  reliventTeams ) // checks both A and B teams
                if ( t.hasPosition ( competitor.Result.Rank.Value ) )
                    return true;

            // not in any team
            return false;
        }

        /// <summary>
        /// Gets CertificateData for the top Top Individuals
        /// </summary>
        /// <returns></returns>
        protected List<CertificateData> getCertificatesTopIndividual()
        {
            List<CertificateData> CD = new List<CertificateData>();

            int counter = 1;

            foreach (AResult Result in TopIndividuals())
                CD.AddRange(buildCertificateData(Result.Competitor,"TopIndividuals",counter++));

            return CD;
        }

        #endregion

        #region Restrictions

        public virtual EventGroups[] Groups { get { return _Groups.ToArray(); } }

        public List<Group> getGroups()
        {
            // Where clause removed as it should be handled by Linq-to-SQL
            return (from er in Groups select er.Group).ToList<Group>();
            //return (from er in Restrictions where er.Event == this select er.Restriction).ToList<ARestriction>();
        }

        public bool hasRestriction()
        {
            return (getGroups().Count(r => r.hasRestrictions() == true) > 0);
        }

        public bool hasGroup ( Group Group )
        {
            return ( getGroups ( ).Where ( r => r == Group ).Count() > 0 );
        }

        public void addGroup(Group newRestriction)
        {
            //_Groups.Add(new EventGroups() { Event = this, Group = newRestriction });
            //!**!

            EventGroups eg = new EventGroups ( ) { DState = null, Event = this , Group = newRestriction };

            Championship.DState.IO.Add<EventGroups> ( eg );

            __Groups.refresh ( );
            eg.Group.__Events.refresh ( );
        }

        public void clearGroups()
        {
            //foreach (EventGroups er in _Groups )
            //er.Event = null;

            // !*!
            ////Championship.database.DeleteRange ( _Groups );
            DState.IO.DeleteRange ( _Groups );
            //_Groups.Clear();
        }

        public EventGroups clearGroup ( Group Group )
        {

            EventGroups eg = (from g in Groups where g.Group == Group select g).FirstOrDefault();

            if ( eg == null ) return null;



            //Championship.database.DeleteRange ( _Groups );
            ////DState.IO.DeleteRange ( _Groups );
            DState.IO.Delete<EventGroups> ( eg );
            //_Groups.Remove ( eg );
            __Groups.refresh ( );
            eg.Group.__Events.refresh ( );

            // to do should we also be refreshing eg.Groups?

            //eg.Event = null; // should remove the EventGroup from Database

            return eg;
            
        }

        /// <summary>
        /// Checks to see if an athlete passes the requirements of this event.
        /// Does not check if the event if full, the team is full or if the athlete has already been entered.
        /// </summary>
        /// <param name="proposedAthlete"></param>
        /// <returns></returns>
        public bool isAvailable(Athlete proposedAthlete)
        {
            if (!hasRestriction())
            {
                // there are no restrictions so will always return true.
                return true;
            }

            foreach (Group restriction in getGroups())
            {
                if (! restriction.isAvailable(proposedAthlete))
                {
                    // This athlete has failed a check.
                    return false;
                }
            }

            // This athlete has passed all of the restrictions.
            return true;
        }

        public List<Athlete> listAvailableAthletes(List<Athlete> proposedAthletes)
        {
            List<Athlete> temp = new List<Athlete>();

            foreach (Athlete athlete in proposedAthletes)
                if (isAvailable(athlete))
                    temp.Add(athlete);

            return temp;
        }

#endregion

        #region Templates

        

        public Template ResultsTemplate { get { return _ResultsTemplate; } set { _ResultsTemplate = value; } }

        public Template DataEntryTemplate { get { return _DataEntryTemplate; } set { _DataEntryTemplate = value; } }

        public Template CertificateTemplate { get { return _CertificateTemplate; } set { _CertificateTemplate = value; } }

        public Template VestTemplate { get { return _VestTemplate; } set { _VestTemplate = value; } }
        

#endregion

        public int countCurrentlySelected()
        {
                return EnteredCompetitors.Where(c => c.SelectedForNextEvent).Count();
        }

        public void setSelectionComplete()
        {
            State = EventState.SelectionComplete;
            Save ( );
        }


        #region CustomData

        public ACustomDataValue[] CustomDataStore { get { return _CustomDataStore.ToArray(); } }

        // 2016-04-05 replaced by CustomData.cs -> internal static void CopyCustomData
        //private void setCustomData(ICollection<ACustomDataValue> cd)
        //{
        //    // clear current custom data store if there is any
        //    foreach (ACustomDataValue cdv in this.CustomDataStore)
        //    {
        //        deleteField(cdv.key);
        //    }

        //    // re-populate the custom data store with the new data
        //    foreach (ACustomDataValue cdv in cd)
        //    {
        //        if (cdv is CustomDataValueInt)
        //            this.createIntField(cdv.key);

        //        if (cdv is CustomDataValueString)
        //            this.createStringField(cdv.key);

        //        setValue(cdv.key, cdv.getValue());
        //    }
        //}

        public void addCustomDataValue(ACustomDataValue Value)
        {
            if ( Value != null )
            {
                Value.Event = this;
                Value._Event_ID = ID;

                DState.IO.Add<ACustomDataValue> ( Value );
                __CustomDataStore.refresh( );
            }
        }

        public void removeCustomDataValue( ACustomDataValue Value )
        {
            if ( Value == null ) return;

            DState.IO.Delete<ACustomDataValue> ( Value );
            __CustomDataStore.refresh( );
        }

        public void createIntField(string key)
        {
            CustomData.createIntField( CustomDataStore, this , key);
        }

        public void createStringField(string key)
        {
            CustomData.createStringField( CustomDataStore, this , key);
        }

        public void deleteField(string key)
        {
            removeCustomDataValue ( CustomData.deleteField ( CustomDataStore , this , key ) );
        }

        public bool customFieldExists(string key)
        {
            return CustomData.customFieldExists( CustomDataStore, key);
        }

        public object getValue(string key)
        {
            object value = CustomData.getValue( CustomDataStore, key);

            if ( value != null ) return value;

            if ( value is string ) return string.Empty;
            if ( value is int ) return 0;

            return null;
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

        #region File Storage 

        public string getMostRecentlyScan()
        {
            if ( this is IHeatEvent iEvent )
            {
                if ( iEvent.Final.Files.Count( ) > 1 )
                {
                    // multiple files
                    return iEvent.Final.Files.Last( ).ID.ToString( );
                }
                else if ( this.Files.Count( ) == 1 )
                {
                    return iEvent.Final.Files.First( ).ID.ToString( );
                }
            }
            else
            {
                if ( this.Files.Count( ) > 1 )
                {
                    // multiple files
                    return Files.Last( ).ID.ToString( );
                }
                else if ( Files.Count( ) == 1 )
                {
                    return Files.First( ).ID.ToString( );
                }
            }

            return null;
        }

        //private string printHex(byte[] array)
        //{
        //     return Convert.ToBase64String(array);
        //}


        public FileStorage[] Files { get { return _Files.ToArray(); } }

        public bool IsFinal {
            get
            {
                return ! (this is IHeatEvent);
            }
        }

        public int FinalID
        {
            get
            {
                if ( IsFinal )
                    return ID;
                else
                    return ( (IHeatEvent)this ).Final.ID;
            }
        }

        /// <summary>
        /// Add binary file to the database
        /// </summary>
        /// <param name="File"></param>
        public void AddFile (FileStorage File)
        {
            File.Event = this;
            File._Event_ID = ID;
            //_Files.Add(File);

            //!*!
            //Championship.database.Add<FileStorage> ( File );
            DState.IO.Add<FileStorage> ( File );
            __Files.refresh ( );
        }

        /// <summary>
        /// Removes a file from the database
        /// </summary>
        /// <param name="File"></param>
        public void RemoveFile (FileStorage File)
        {
            //File.Event = null; // marks file to be deleted.
            //_Files.Remove(File);
            //Championship.database.Delete<FileStorage> ( File );
            DState.IO.Delete<FileStorage> ( File );
            __Files.refresh ( );
        }

#endregion

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// An event can only be deleted if it has no results.
        /// This is set by the foreign key constraint so must be honoured.
        /// </summary>
        /// <returns>True if the Event can be deleted.</returns>
        public bool CanDelete()
        {
            // If this event has one or more heats then  we can not delete it
            if ( this is IFinalEvent )
            {
                if ( ( (IFinalEvent)this ).getHeats ( ).Count > 0 )
                    return false;
                return Results.Count ( ) == 0;
            }
            else
            {
                // if there is a result then we can not delete it.
                return Results.Count ( ) == 0;
            }
        }

        public override bool Equals ( object obj )
        {
            try
            {
                return ( (AEvent)obj ).ID == this.ID;
            }
            catch 
            {
                return base.Equals ( obj );
            }
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public static bool operator == ( AEvent x , AEvent y )
        {
            if ( ( (object)x ) == null && ( (object)y ) == null ) return true;
            if ( ( (object)x ) == null ) return false;
            if ( ( (object)y ) == null ) return false;

            return x.ID == y.ID;
        }

        public static bool operator != ( AEvent x , AEvent y )
        {
            return !( x == y );
        }

    }

}
