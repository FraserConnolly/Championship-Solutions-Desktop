using System;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "AthleteTeamChamptionships")]
    public partial class AthleteTeamChamptionship : IID
    {

        public AthleteTeamChamptionship ()
        {
            DState = null;
        }

        [Column ( IsPrimaryKey = true , Name = "ID" )]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column (Name = "Championship_ID")]
        internal int _Championship_ID { get; set; }

        [Column ( Name = "Athlete_ID" )]
        internal int _Athlete_ID { get; set; }

        [Column ( Name = "Team_ID" )]
        internal int _Team_ID { get; set; }

        [Column ( Name = "PreferedEvent_ID" , CanBeNull = true )]
        internal int? _PreferedEvent_ID { get; set; }

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
                __Championship = new LazyRef<Championship> ( DState , ( ) => _Championship_ID , v => { _Championship_ID = v; } );
                __Athlete = new LazyRef<Athlete> ( DState , ( ) => _Athlete_ID , v => { _Athlete_ID = v; } );
                __Team = new LazyRef<Team> ( DState , ( ) => _Team_ID , v => { _Team_ID = v; } );
                __PreferedEvent = new LazyRefN<ACompetitor> ( DState , ( ) => _PreferedEvent_ID , v => { _PreferedEvent_ID = v; } );
            }
        }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Championship.refresh ( );
            __Athlete.refresh ( );
            __Team.refresh ( );
            __PreferedEvent.refresh ( );
        }

        public void Save()
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<AthleteTeamChamptionship> ( this );
        }

        private LazyRef<Championship> __Championship;
        internal Championship _Championship { get { return __Championship.Store; } set { __Championship.Store = value; } }


        //private EntityRef<Championship> _ChampionshipStorage = new EntityRef<Championship>();
        //[Association(IsForeignKey = true, ThisKey = "_Championship_ID", Storage = "_ChampionshipStorage")]
        //internal Championship _Championship { get { return _ChampionshipStorage.Entity; } set { _ChampionshipStorage.Entity = value; } }

        private LazyRef<Athlete> __Athlete;
        internal Athlete _Athlete { get { return __Athlete.Store; } set { __Athlete.Store = value; } }


        //private EntityRef<Athlete> _AthleteStorage = new EntityRef<Athlete>();
        //[Association(IsForeignKey = true, ThisKey = "_Athlete_ID", Storage = "_AthleteStorage")]
        //internal Athlete _Athlete { get { return _AthleteStorage.Entity; } set { _AthleteStorage.Entity = value; } }

        private LazyRef<Team> __Team;
        internal Team _Team { get { return __Team.Store; } set { __Team.Store = value; } }


        //private EntityRef<Team> _TeamStorage = new EntityRef<Team>();
        //[Association(IsForeignKey = true, ThisKey = "_Team_ID", Storage = "_TeamStorage", DeleteOnNull = true)]
        //internal Team _Team { get { return _TeamStorage.Entity; } set { _TeamStorage.Entity = value; } }

        private LazyRefN<ACompetitor> __PreferedEvent;
        internal ACompetitor _PreferedEvent { get { return __PreferedEvent.Store; } set { __PreferedEvent.Store = value; } }


        //private EntityRef<ACompetitor> _PreferedEventStorage = new EntityRef<ACompetitor>();
        //[Association(IsForeignKey = true, ThisKey = "_PreferedEvent_ID", Storage = "_PreferedEventStorage")]
        //internal ACompetitor _PreferedEvent { get { return _PreferedEventStorage.Entity; } set { _PreferedEventStorage.Entity = value; } }

    }
}
