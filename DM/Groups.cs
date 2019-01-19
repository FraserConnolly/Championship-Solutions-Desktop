using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChampionshipSolutions.DM;
//using System.Data.Linq.Mapping;
using Itenso.TimePeriod; //http://www.codeproject.com/Articles/168662/Time-Period-Library-for-NET
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    public partial class Group : IIdentity
    {

        //public int ID { get { return _ID; } set { _ID = value; } }

        public Championship Championship { get { return _Championship; } set { _Championship = value; } }

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

        #endregion

        public static Group CopyGroup ( Group group )
        {
            switch ( group.GetType ( ).Name )
            {
                case "Group":
                    return new Group ( (Group)group );
                case "GenderRestriction":
                    return new GenderRestriction ( (GenderRestriction)group );
                case "AgeRestriction":
                    return new AgeRestriction ( (AgeRestriction)group );
                case "DateRestriction":
                    return new DoBRestriction ( (DoBRestriction)group );

                default:
                    return null;
            }
        }

        /// <summary>
        /// List the Athletes that fall within a restriction from a list of Athletes.
        /// </summary>
        /// <param name="prospectiveAthletes">Athletes to be checked for suitability.</param>
        /// <returns>A list of Athletes that can be entered.</returns>
        public virtual List<Athlete> listAvailableAthletes ( List<Athlete> prospectiveAthletes )
        {
            return prospectiveAthletes;
        }

        /// <summary>
        /// List the Athletes that fall within a restriction from a list of Athletes who are in a specified Team.
        /// </summary>
        /// <param name="prospectiveAthletes">Athletes to be checked for suitability.</param>
        /// <param name="fromTeam">The team you want to look within.</param>
        /// <returns>A list of Athletes that can be entered.</returns>
        virtual public List<Athlete> listAvailableAthletes ( List<Athlete> prospectiveAthletes , Team fromTeam , Championship Championship )
        {
            return listAvailableAthletes ( prospectiveAthletes.Where ( i => i.getTeam ( Championship ) == fromTeam ).ToList ( ) );
        }

        virtual public bool isAvailable ( Athlete proposedAthlete )
        {
            List<Athlete> temp = new List<Athlete>();
            temp.Add ( proposedAthlete );

            return listAvailableAthletes ( temp ).Count == 1;
        }

        #region Constructors

        public Group ( ) { DState = null; }

        internal Group ( string Name ) :this() { this.Name = Name; }

        internal Group  ( Group xRestriction ) : this ( )
        {
            this.Name = xRestriction.Name;
            this.ShortName = xRestriction.ShortName;
        }

        public Group ( string Name , Championship Championship ) : this ( Name )
        {
            this.Championship = Championship;
            this.Championship.addGroup ( this );
        }

        #endregion

        public virtual bool hasRestrictions ( )
        {
            return false;
        }

        public bool CanDelete ( )
        {
            if ( Championship == null ) return true;

            if ( _Events.Count ( ) == 0 ) return true;

            //foreach ( AEvent e in Championship.Events )
            //if ( e.hasGroup ( this ) )
            //return false;

            return false; 
        }

        public override string ToString ( )
        {
            return this.Name.ToString ( );
        }

        public override bool Equals ( object obj )
        {
            try
            {
                return ( (Group)obj ).ID == this.ID;
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

        public static bool operator == ( Group x , Group y )
        {
            if ( ( (object)x ) == null && ( (object)y ) == null ) return true;
            if ( ( (object)x ) == null ) return false;
            if ( ( (object)y ) == null ) return false;

            return x.ID == y.ID;
        }

        public static bool operator != ( Group x , Group y )
        {
            return !( x == y );
        }

    }


    //public partial class Group : ARestriction 
    //{
    //    public Group(Championship Championship)
    //        : base("Group") 
    //    {
    //        Championship.addRestriction(this);
    //    }

    //    public Group() : base("Group") { }

    //    public Group(Group xRestriction) : base ( xRestriction )
    //    {
    //    }

    //    /// <summary>
    //    /// Will return all of the athletes as groups don't limit participation.
    //    /// </summary>
    //    /// <param name="prospectiveAthletes"></param>
    //    /// <returns></returns>
    //    public override List<Athlete> listAvailableAthletes(List<Athlete> prospectiveAthletes)
    //    {
    //        return prospectiveAthletes;
    //    }

    //    public override bool hasRestrictions()
    //    {
    //        return false;
    //    }
    //}


    public partial class GenderRestriction : Group
    {
        public bool Male { get { return _Male; } set { _Male = value; } }

        public bool Female { get { return _Female; } set { _Female = value; } }

        public void allowMale ( )
        {
            Male = true;
            Female = false;
        }

        public void allowFemale ( )
        {
            Male = false;
            Female = true;
        }

        public void allowAllGenders ( )
        {
            Male = true;
            Female = true;
        }

        public GenderRestriction ( Championship Championship )
            : base ( "Gender Restriction" , Championship )
        { }

        public GenderRestriction ( ) : base ( "Gender Restriction" ) { }

        public GenderRestriction ( GenderRestriction xRestriction ) : base ( xRestriction )
        {
            this.Female = xRestriction.Female;
            this.Male = xRestriction.Male;
        }

        public override List<Athlete> listAvailableAthletes ( List<Athlete> prospectiveAthletes )
        {
            List<Athlete> tempAvailable = new List<Athlete>();

            if ( Male == true && Female == true )
                return prospectiveAthletes;

            foreach ( Athlete a in prospectiveAthletes )
            {
                if ( Male )
                    if ( a.Gender == Gender.Male )
                    {
                        tempAvailable.Add ( a );
                        continue;
                    }

                if ( Female )
                    if ( a.Gender == Gender.Female )
                        tempAvailable.Add ( a );
            }

            return tempAvailable;
        }

        public override bool hasRestrictions ( )
        {

            if ( Male && Female )
                return false;

            if ( !Male && Female )
                return false;

            return true;
        }
    }

    public partial class AgeRestriction : Group
    {

        public AgeRestriction ( ) : base ( "Age Restriction" ) { }

        public AgeRestriction ( AgeRestriction xRestriction ) : base ( xRestriction )
        {
            this.dateReference = xRestriction.dateReference;
            this.maxAge = xRestriction.maxAge;
            this.minAge = xRestriction.minAge;
        }

        public AgeRestriction ( Championship Championship )
            : base ( "Age Restriction" , Championship )
        {
            dateReference = DateTime.Now;
            //Championship.addRestriction(this);
        }

        /// <summary>
        /// Minimum age in years
        /// </summary>
        public int minAge
        {
            get { return _minAge; }
            set
            {
                if ( value >= maxAge )
                { _maxAge = value; }

                _minAge = value;
            }
        }

        /// <summary>
        /// Maximum age in years
        /// </summary>
        public int maxAge
        {
            get { return _maxAge; }
            set
            {
                if ( value <= minAge )
                { _minAge = value; }
                _maxAge = value;
            }
        }

        public DateTime dateReference { get { return _dateReference; } set { _dateReference = value; } }

        public override List<Athlete> listAvailableAthletes ( List<Athlete> prospectiveAthletes )
        {
            // There is no restriction to apply
            if ( !hasRestrictions ( ) )
                return prospectiveAthletes;

            List<Athlete> tempAvailable = new List<Athlete>();

            // check each prospective athlete for their gender
            foreach ( Athlete a in prospectiveAthletes )
            {
                Age AthleteAge = a.getAge(dateReference);

                if ( AthleteAge != null )
                    if ( AthleteAge.Years >= minAge && AthleteAge.Years <= maxAge )
                        tempAvailable.Add ( a );
            }
            return tempAvailable;
        }

        public override bool hasRestrictions ( )
        {
            return !( minAge == 0 && maxAge == 0 );
        }

    }

    public partial class DoBRestriction : Group
    {

        public DoBRestriction ( ) : base ( "Date of Birth Restriction" ) { }

        public DoBRestriction ( DoBRestriction xRestriction ) : base ( xRestriction )
        {
            this.EndDate = xRestriction.EndDate;
            this.StartDate = xRestriction.StartDate;
        }

        public DoBRestriction ( Championship Championship ) : base ( "Date of Birth Restriction" , Championship ) { }

        public DoBRestriction ( DateTime startDate , DateTime endDate , Championship Championship )
            : base ( "Date Restriction" , Championship )
        {
            _range = new TimeRange ( startDate , endDate );
            //Championship.addRestriction(this);
        }

        private TimeRange _range;// = new TimeRange();

        /// <summary>
        /// Start of date range
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                if ( _range == null )
                    initRange ( );
                return _range.Start;
            }
            set
            {
                _StartDate = value;
                _range.Setup ( value , _range.End );
            }
        }

        /// <summary>
        /// End of date range
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                if ( _range == null )
                    initRange ( );
                return _range.End;
            }
            set
            {
                _EndDate = value;
                _range.Setup ( _range.Start , value );
            }
        }

        private void initRange ( )
        {
            if ( _StartDate == null || _EndDate == null )
                _range = new TimeRange ( DateTime.Now , DateTime.Now );
            else if ( _StartDate == DateTime.MinValue || _EndDate == DateTime.MinValue )
                _range = new TimeRange ( DateTime.Now , DateTime.Now );
            else
                _range = new TimeRange ( _StartDate , _EndDate );

        }

        public void setRange ( DateTime startDate , DateTime endDate )
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public TimeRange getTimeRange ( ) { return _range; }

        public override List<Athlete> listAvailableAthletes ( List<Athlete> prospectiveAthletes )
        {
            List<Athlete> tempAvailable = new List<Athlete>();

            // check each prospective athlete to see if their DoB is within range.
            foreach ( Athlete a in prospectiveAthletes )

                if ( getTimeRange ( ).HasInside ( (DateTime)a.DateOfBirth ) )
                    tempAvailable.Add ( a );

            return tempAvailable;
        }

        public override bool hasRestrictions ( ) { return !( _range == null ); }
    }

}
