using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "Results")]
    [InheritanceMapping(Code = "Result", Type = typeof(Result), IsDefault = true)]
    public abstract partial class AResult : IID
    {

        [Column(IsPrimaryKey = true, Name ="ID")]
        public int ID { get; set; }

        [Column (IsDiscriminator = true, Name = "Discriminator")]
        public string Discriminator { get; set; }

        [Column ( Name = "Event_ID" )]
        internal int _Event_ID { get; set; }

        [Column(CanBeNull = true, Name ="Rank")]
        internal int? _Rank { get; set; }

        [Column ( Name = "Competitor_ID" , CanBeNull = true )]
        internal int? _Competitor_ID { get; set; }

        [Column ( Name = "VestNumber_dbVestNumber" , CanBeNull = true )]
        internal string _vestNumberDB { get; set; }

        [Column (Name = "Value_RawValue")]
        internal int _Value_RawValue { get; set; }

        [Column (Name = "Value_ValueType")]
        internal int _Value_ValueType { get; set; }

        //private EntityRef<ACompetitor> _CompetitorStorage = new EntityRef<ACompetitor>();
        //[Association(IsForeignKey = true, Storage = "_CompetitorStorage", ThisKey = "_Competitor_ID")]
        //internal ACompetitor _Competitor { get { return _CompetitorStorage.Entity; } set { _CompetitorStorage.Entity = value; } }

        //private EntityRef<AEvent> _EventStorage = new EntityRef<AEvent>();
        //[Association(IsForeignKey = true, Storage = "_EventStorage", ThisKey = "_Event_ID", DeleteOnNull =true)]
        //internal AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

        private DataState _DState = new DataState();

        public virtual DataState DState
        {
            get
            {
                return _DState;
            }
            set
            {
                _DState = value;
                __Competitor = new LazyRefN<ACompetitor> ( DState , ( ) => _Competitor_ID , v => { _Competitor_ID = v; } );
                __Event = new LazyRef<AEvent> ( DState , ( ) => _Event_ID , v => { _Event_ID = v; } );
            }
        }

        private LazyRefN< ACompetitor> __Competitor;
        internal ACompetitor _Competitor { get { return __Competitor.Store; } set { __Competitor.Store = value; } }

        private LazyRef<AEvent> __Event;
        internal AEvent _Event { get { return __Event.Store; } set { __Event.Store = value; } }

        public void voidStorage ( bool softRefresh = true )
        {
            __Event.refresh ( );
            __Competitor.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<AResult> ( this );
        }


    }
}