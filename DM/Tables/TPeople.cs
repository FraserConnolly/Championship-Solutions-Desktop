using System;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    // If a new type of athlete is added remember to update countAthletes in ChampionshipVM.cs

    [Table(Name = "People")]
    [InheritanceMapping(Code = "Person", Type = typeof(Person))]
    [InheritanceMapping(Code = "Athlete", Type = typeof(Athlete), IsDefault = true)]
    [InheritanceMapping(Code = "StudentAthlete", Type = typeof(StudentAthlete))]
    public partial class Person :IID 
    {

        #region Properties

        [Column(IsPrimaryKey = true, Name ="ID")]
        public int ID { get; set; }

        [Column(IsDiscriminator = true, Name = "Discriminator")]
        internal string _Discriminator { get; set; }

        [Column (Name = "FirstName")]
        internal string _FirstName { get; set; }

        [Column (Name = "MiddleName")]
        internal string _MiddleName { get; set; }

        [Column (Name = "LastName")]
        internal string _LastName { get; set; }

        [Column ( Name = "PreferredName" )]
        internal string _PreferredName { get; set; }

        [Column (Name = "Title")]
        internal string _Title { get; set; }

        [Column (Name = "Suffix")]
        internal string _Suffix { get; set; }

        [Column ( Name = "Gender" )]
        internal int _Gender { get; set; }

        [Column(CanBeNull = true, Name = "DateOfBirth")]
        internal DateTime? _DateOfBirth { get; set; }

        #endregion

#pragma warning disable 0169

        //[Column(Name = "School_ID", CanBeNull = true)]
        //internal int? _Attends_ID;

#pragma warning restore 0169

        //private EntityRef<School> _SchoolStorage = new EntityRef<School>();
        //[Association ( IsForeignKey = true , Storage = "_SchoolStorage" , ThisKey = "_Attends_ID" )]
        //internal School _Attends { get { return _SchoolStorage.Entity; } set { _SchoolStorage.Entity = value; } }

        //private EntitySet<AContactDetail> _ContactStorage = new EntitySet<AContactDetail>();
        //[Association(OtherKey = "_Person_ID", Storage = "_ContactStorage")]
        //internal EntitySet<AContactDetail> _Contacts { get { return _ContactStorage; } set { _ContactStorage.Assign(value); } }

        //private EntitySet<ConfidentialNote> _NotesStorage = new EntitySet<ConfidentialNote>();
        //[Association ( OtherKey = "_Athlete_ID" , Storage = "_NotesStorage" )]
        //internal EntitySet<ConfidentialNote> _Notes { get { return _NotesStorage; } set { _NotesStorage.Assign ( value ); } }

        private DataState _DState;

        public virtual DataState DState
        {
            get
            {
                return _DState;
            }
            set
            {
                _DState = value;
                __Contacts = new LazyList<AContactDetail> ( DState , c => c._Person_ID == ID );
                __Notes = new LazyList<ConfidentialNote> ( DState , c => c._Athlete_ID == ID );
            }
        }

        private LazyList<AContactDetail> __Contacts { get; set; }
        internal AContactDetail[] _Contacts { get { return __Contacts.ToArray ( ); } }

        private LazyList<ConfidentialNote> __Notes;
        internal ConfidentialNote[] _Notes { get { return __Notes.ToArray ( ); } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Notes.refresh ( );
            __Contacts.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<Person> ( this );
        }


    }

    public partial class Athlete : Person
    {

        public override DataState DState
        {
            get
            {
                return base.DState;
            }
            set
            {
                base.DState = value;
                __Competitors = new LazyList<Competitor> ( DState , c => c._Athlete_ID == ID );
                __AthleteTeamChampionship = new LazyList<AthleteTeamChamptionship> ( DState , c => c._Athlete_ID == ID );
                __Attends = new LazyRefN<School> ( DState , ( ) => _Attends_ID , v => { _Attends_ID = v; } );
            }
        }

#pragma warning disable 0169

        // moved to Person to try to fix an error when loading SchoolsPage
        [Column ( Name = "School_ID" , CanBeNull = true )]
        internal int? _Attends_ID { get; set; }

        [Column ( Name = "GlobalAthleteID" , CanBeNull = true )]
        internal int? _GlobalAthleteID { get; set; }

#pragma warning restore 0169

        //private EntityRef<School> _SchoolStorage = new EntityRef<School>();
        //[Association ( IsForeignKey = true , Storage = "_SchoolStorage" , ThisKey = "_Attends_ID" )]
        //internal School _Attends { get { return _SchoolStorage.Entity; } set { _SchoolStorage.Entity = value; } }

        //private EntitySet<AthleteTeamChamptionship> _AthleteTeamChamptionshipStorage = new EntitySet<AthleteTeamChamptionship>();
        //[Association(Storage = "_AthleteTeamChamptionshipStorage", OtherKey = "_Athlete_ID")]
        //internal EntitySet<AthleteTeamChamptionship> _AthleteTeamChamptionship { get { return _AthleteTeamChamptionshipStorage; } set { _AthleteTeamChamptionshipStorage.Assign(value); } }

        //private EntitySet<Competitor> _CompetitorsStorage = new EntitySet<Competitor>();
        //[Association(OtherKey = "_Athlete_ID", Storage = "_CompetitorsStorage")]
        //internal EntitySet<Competitor> _Competitors { get { return _CompetitorsStorage; } set { _CompetitorsStorage.Assign(value); } }

        private LazyRefN< School> __Attends;
        internal School _Attends { get { return __Attends.Store ; } set { __Attends.Store = value; } }

        private LazyList <AthleteTeamChamptionship> __AthleteTeamChampionship = new LazyList<AthleteTeamChamptionship>(); 
        internal AthleteTeamChamptionship[] _AthleteTeamChampionship { get { return __AthleteTeamChampionship.ToArray ( ); } }

        private LazyList<Competitor> __Competitors = new LazyList<Competitor>();
        internal Competitor[] _Competitors { get { return __Competitors.ToArray ( ); } }

        public override void voidStorage ( bool softRefresh = true )
        {
            base.voidStorage ( );
            __Competitors.refresh ( );
            __AthleteTeamChampionship.refresh ( );
            __Attends.refresh ( );
        }


    }

}