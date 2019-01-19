using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "AthleteNotes")]
    [InheritanceMapping(Code = "PreviousResult", Type = typeof(PreviousResult))]
    [InheritanceMapping(Code = "PowerOfTenResult", Type = typeof(PowerOfTenResult))]
    [InheritanceMapping(Code = "PublicNote", Type = typeof(PublicNote))]
    [InheritanceMapping(Code = "DeclaredAvailibilityInformation" , Type = typeof( DeclaredAvailibilityInformation ) )]
    [InheritanceMapping(Code = "ConfidentialNote", Type = typeof(ConfidentialNote), IsDefault =true)]
    /// <summary>
    /// Use this class for public honours, they will appear on the athlete report print out.
    /// </summary>
    public partial class ConfidentialNote : IID
    {

        [Column(IsPrimaryKey = true, Name = "ID")]
        public int ID { get; set; }

        [Column(IsDiscriminator = true, Name = "Discriminator")]
        public string Discriminator { get; set; }

        [Column(CanBeNull = false, DbType = "Date", Name = "EnteredDate")]
        internal DateTime _EnteredDate { get; set; }

        [Column ( Name = "Key" )]
        internal string _Key { get; set; }

        [Column (Name = "Note")]
        internal string _Note { get; set; }

        [Column(Name = "Athlete_ID", CanBeNull = false)]
        internal int _Athlete_ID { get; set; }

        private DataState _DState ;

        public virtual DataState DState
        {
            get
            {
                return _DState;
            }
            set
            {
                _DState = value;
                __Person = new LazyRef<Person> ( DState , ( ) => _Athlete_ID , v => { _Athlete_ID = v; } );
            }
        }

        private LazyRef<Person> __Person;
        internal Person _Person { get { return __Person.Store; } set { __Person.Store = value; } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Person.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<ConfidentialNote> ( this );
        }
        //private EntityRef<Person> _AthleteStorage = new EntityRef<Person>();
        //[Association ( ThisKey = "_Athlete_ID", IsForeignKey = true, DeleteOnNull =true, Storage ="_AthleteStorage")]
        //internal Person _Person { get { return _AthleteStorage.Entity; } set { _AthleteStorage.Entity = value; } }

    }

    public partial class DeclaredAvailibilityInformation : PublicNote
    {
        // Dev note: this field shares the same data store as PreviousResult.Championship to avoid having to change the database structure
        // Athlete Declares themselves available for this championship
        // Changed to use Key value.
        //[Column (Name = "Championship")]
        //internal string _Championship { get; set; }

        // Dev note: this field shares the same data store as PreviousResult.Rank to avoid having to change the database structure
        [Column (Name = "Rank")]
        internal string _Availability { get; set; }

        // Dev note: this field shares the same data store as PreviousResult.ResultValue to avoid having to change the database structure
        [Column (Name = "ResultValue")]
        internal string _TransportMethod { get; set; }

        // Dev note: this field shares the same data store as PreviousResult.Event to avoid having to change the database structure
        [Column (Name = "Event")]
        internal string _PreferredEvent { get; set; }

        // Dev note: this field shares the same data store as PreviousResult.Venue to avoid having to change the database structure
        [Column ( Name = "Venue" )]
        internal string _PersonalBest { get; set; }
    }

    /// <summary>
    /// Stores results from previous championships
    /// </summary>
    public partial class PreviousResult
    {

        [Column(Name = "Championship")]
        internal string _Championship { get; set; }

        [Column(Name = "Event")]
        internal string _Event { get; set; }

        [Column(Name = "Rank")]
        internal string _Rank { get; set; }

        [Column(Name = "ResultValue")]
        internal string _ResultValue { get; set; }

        [Column(Name = "Venue")]
        internal string _Venue { get; set; }

        [Column (Name ="Team")]
        internal string _Team { get; set; }

        [Column (CanBeNull = true, DbType = "Date", Name = "EventDate")]
        internal DateTime? _EventDate { get; set; }

    }
}
