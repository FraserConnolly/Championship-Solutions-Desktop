using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChampionshipSolutions.DM
{

    /// <summary>
    /// Use this class for public honours, they will appear on the athlete report print out.
    /// </summary>
    public partial class ConfidentialNote
    {

        public DateTime EnteredDate { get { return _EnteredDate; } private set { _EnteredDate = value; } }

        public string Note { get { return _Note ?? ""; } set { _Note = value; } }

        public string Key { get { return _Key ?? ""; } set { _Key = value; } }

        public Person Person { get { return _Person; } internal set { _Person = value; } }

        public ConfidentialNote(Person Person, DateTime DateStamp):this()
        {
            this.Person = Person;
            this.EnteredDate = DateStamp;
        }

        public ConfidentialNote ()
        {
            DState = null;
        }

        public ConfidentialNote( ConfidentialNote Note )
        {
            EnteredDate = Note.EnteredDate;
            this.Note = Note.Note;
            this.Key = Note.Key;
        }


        public virtual string PrintNote { get
            {
                if ( string.IsNullOrEmpty ( Key ) )
                    return Note;
                else
                    return string.Format("{0}: {1}",Key, Note);
            } }

        #region GUI

        public Visibility ShowResult
        {
            get
            {
                if ( this is PreviousResult )
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ShowAvailibility
        {
            get
            {
                if ( this is DeclaredAvailibilityInformation )
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ShowConfidential
        {
            get
            {
                if ( this.GetType() == typeof( ConfidentialNote ) )
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }
 
        public bool IsReadOnly
        {
            get
            {
                if ( this is PowerOfTenResult )
                    return true ;

                return false;
            }
        }

        #endregion


    }

    /// <summary>
    /// Use this class for confidential information such as behavioural or coaching notes.
    /// they will not appear on the athlete's print out
    /// </summary>
    public partial class  PublicNote : ConfidentialNote
    {
        public PublicNote ( ) : base () { }
         
        public PublicNote(Person Person, DateTime DateStamp) : base( Person , DateStamp) { }

        public PublicNote( PublicNote Note ) : base( Note ) { }
    }

    public partial class DeclaredAvailibilityInformation : PublicNote
    {
        public DeclaredAvailibilityInformation ( ) : base ( ) { }

        public DeclaredAvailibilityInformation ( Person Person , DateTime DateStamp ) : base ( Person , DateStamp ) { }
        public DeclaredAvailibilityInformation( DeclaredAvailibilityInformation Note ) : base( Note )
        {
            Championship = Note.Championship;
            Availability = Note.Availability;
            TransportMethod = Note.TransportMethod;
            PreferredEvent = Note.PreferredEvent;
            PersonalBest = Note.PersonalBest;
        }

        // Athlete Declares themselves available for this championship
        public string Championship { get { return _Key ?? ""; } set { _Key = value; } }

        public string Availability { get { return _Availability ?? ""; } set { _Availability = value; } }

        public string TransportMethod { get { return _TransportMethod ?? ""; } set { _TransportMethod = value; } }

        public string PreferredEvent { get { return _PreferredEvent ?? ""; } set { _PreferredEvent = value; } }

        public string PersonalBest { get { return _PersonalBest ?? ""; } set { _PersonalBest = value; } }

        public override string PrintNote { get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append ( "Is " );
                sb.Append ( Availability );
                sb.Append ( " for " );
                sb.Append ( Championship );

                if (! string.IsNullOrWhiteSpace ( TransportMethod ) )
                {
                    sb.Append ( " traveling by " );
                    sb.Append ( TransportMethod );
                }

                if ( !string.IsNullOrWhiteSpace ( PreferredEvent ) )
                {
                    sb.Append ( " and would prefer to compete in " );
                    sb.Append ( PreferredEvent );
                }

                if ( !string.IsNullOrWhiteSpace ( PersonalBest ) )
                {
                    sb.Append ( " with a personal best of " );
                    sb.Append ( PersonalBest );
                }

                sb.Append ( "." );

                return sb.ToString ( );
            }
        }


    }


    /// <summary>
    /// Stores results from previous championships
    /// </summary>
    public partial class PreviousResult : PublicNote
    {

        public string Championship { get { return _Championship ?? ""; } set { _Championship = value; } }

        public string Event { get { return _Event ?? ""; } set { _Event = value; } }

        public string Rank { get { return _Rank ?? ""; } set { _Rank = value; } }

        public string ResultValue { get { return _ResultValue ?? ""; } set { _ResultValue = value; } }

        public string Venue { get { return _Venue ?? ""; } set { _Venue = value; } }

        public string Team { get { return _Team ?? ""; } set { _Team = value; } }

        public DateTime? EventDate { get { return _EventDate; } set { _EventDate = value; } }

        public PreviousResult ( Person Person, DateTime DateStamp ) : base ( Person, DateStamp ) { }

        public PreviousResult ( Person Person , DateTime DateStamp, string Championship, string Event, string Rank, string ResultValue, DateTime EventDate) : base ( Person , DateStamp )
        {

            this.Championship = Championship;
            this.Event = Event;
            this.Rank = Rank;
            this.ResultValue = ResultValue;
            this.EventDate = EventDate;

        }
        public PreviousResult( ) : base( ) { }
        public PreviousResult( PreviousResult Note ) : base( Note )
        {
            Championship = Note.Championship;
            Event = Note.Event;
            Rank = Note.Rank;
            ResultValue = Note.ResultValue;
            Venue = Note.Venue;
            Team = Note.Team;
            EventDate = Note.EventDate;
        }

        public override string PrintNote
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Championship: ");
                sb.Append(Championship);

                sb.Append(" - Event: ");
                sb.Append(Event);

                sb.Append(" - Team: ");
                sb.Append(Team);

                if (! string.IsNullOrWhiteSpace ( Rank) )
                {
                    sb.Append(" - Rank: ");
                    sb.Append(Rank);

                    sb.Append(" - Performance: ");
                    sb.Append( ResultValue );
                }

                sb.Append(".");

                return sb.ToString();
            }
        }

    }

    /// <summary>
    /// A results automatically collected from Power Of Ten
    /// </summary>
    public partial class PowerOfTenResult : PreviousResult
    {
        public PowerOfTenResult ( ) : base ( ) { }

        public PowerOfTenResult ( Person Person , DateTime DateStamp ) : base ( Person, DateStamp ) { }

        public PowerOfTenResult ( Athlete Athlete , DateTime DateStamp, string Championship, string Event, string Rank, string ResultValue, DateTime EventDate) : base( Athlete , DateStamp, Championship, Event, Rank, ResultValue, EventDate) { }

        public PowerOfTenResult ( PowerOfTenResult Note ) : base( Note ) { }
    }

}
