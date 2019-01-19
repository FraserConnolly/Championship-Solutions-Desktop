using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "CustomDataValues")]
    [InheritanceMapping(Code = "CustomDataValueString", Type = typeof(CustomDataValueString), IsDefault = true)]
    [InheritanceMapping(Code = "CustomDataValueInt", Type = typeof(CustomDataValueInt))]
    public abstract partial class ACustomDataValue : IID
    {
        public ACustomDataValue()
        {
            DState = null;
        }

        [Column(IsPrimaryKey = true, Name = "ID")]
        public int ID { get; set; }

        [Column(IsDiscriminator = true, Name = "Discriminator")]
        public string Discriminator { get; set; }

        [Column ( CanBeNull = true , Name = "Event_ID" )]
        internal int? _Event_ID { get; set; }

        [Column ( CanBeNull = true , Name = "Championship_ID" )]
        internal int? _Championship_ID { get; set; }

        [Column ( CanBeNull = true , Name = "Competitor_ID" )]
        internal int? _Competitor_ID { get; set; }

        [Column (Name = "key")]
        internal string _key { get; set; }

        //private EntityRef<AEvent> _EventStorage = new EntityRef<AEvent>();
        //[Association ( IsForeignKey = true , Storage = "_EventStorage" , ThisKey = "_Event_ID" )]
        //internal AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

        //private EntityRef<Championship> _ChampionshipStorage = new EntityRef<Championship>();
        //[Association ( IsForeignKey = true , Storage = "_ChampionshipStorage" , ThisKey = "_Championship_ID" )]
        //internal Championship _Championship { get { return _ChampionshipStorage.Entity; } set { _ChampionshipStorage.Entity = value; } }

        //private EntityRef<ACompetitor> _CompetitorStorage = new EntityRef<ACompetitor>();
        //[Association ( IsForeignKey = true , Storage = "_CompetitorStorage" , ThisKey = "_Competitor_ID" )]
        //internal ACompetitor _Competitor { get { return _CompetitorStorage.Entity; } set { _CompetitorStorage.Entity = value; } }



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
                __Event = new LazyRefN<AEvent> ( DState , ( ) => _Event_ID , v => { _Event_ID = v; } );
                __Championship = new LazyRefN<Championship> ( DState , ( ) => _Championship_ID , v => { _Championship_ID = v; } );
                __Competitor = new LazyRefN<ACompetitor> ( DState , ( ) => _Competitor_ID , v => { _Competitor_ID = v; } );
            }
        }

        private LazyRefN<Championship> __Championship;
        internal Championship _Championship { get { return __Championship.Store; } set { __Championship.Store = value; } }

        private LazyRefN<AEvent > __Event;
        internal AEvent _Event { get { return __Event.Store; } set { __Event.Store = value; } }

        private LazyRefN< ACompetitor > __Competitor;
        internal ACompetitor _Competitor { get { return __Competitor.Store; } set { __Competitor.Store = value; } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Event.refresh ( );
            __Championship.refresh ( );
            __Competitor.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<ACustomDataValue> ( this );
        }

    }

    public partial class CustomDataValueString : ACustomDataValue
    {

        [Column(Name = "stringvalue")]
        internal string _stringValue { get; set; }

    }

    public partial class CustomDataValueInt : ACustomDataValue
    {

        [Column(Name = "intvalue")]
        internal int _intValue { get; set; }

    }
}