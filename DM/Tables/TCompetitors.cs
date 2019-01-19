using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "Competitors")]
    [InheritanceMapping(Code = "Competitor", Type = typeof(Competitor), IsDefault = true)]
    [InheritanceMapping(Code = "StudentCompetitor", Type = typeof(StudentCompetitor))]
    [InheritanceMapping(Code = "StudentHeatedCompetitor", Type = typeof(StudentHeatedCompetitor))]
    [InheritanceMapping(Code = "SpecialConsideration" , Type = typeof( SpecialConsideration ) )]
    [InheritanceMapping(Code = "Squad", Type = typeof(Squad))]
    public abstract partial class ACompetitor : IID
    {

        [Column(IsPrimaryKey = true, Name ="ID")]
        public int ID { get; set; }

        [Column(IsDiscriminator = true, Name = "Discriminator")]
        public string Discriminator { get; set; }

        [Column ( Name = "CompetingIn_ID" )]
        internal int _CompetingIn_ID { get; set; }

        [Column ( Name = "Vest_dbVestNumber" )]
        internal string _Vest_dbVestNumber { get; set; }

        [Column (Name ="Guest")]
        internal bool _Guest { get; set; }

        [Column (Name= "AvilableForNationals")]
        internal bool? _AvilableForNationals { get; set; }

        [Column (Name= "CoachForSW")]
        internal string _CoachForSW { get; set; }

        [Column (Name= "CoachForNationals")]
        internal string _CoachForNationals { get; set; }

        [Column (Name= "SelectedForNextEvent")]
        internal bool _SelectedForNextEvent { get; set; }

        [Column (Name= "PersonalBest_RawValue")]
        internal int _PersonalBest_RawValue { get; set; }

        [Column (Name= "PersonalBest_ValueType")]
        internal int _PersonalBest_ValueType { get; set; }

        [Column (Name= "LaneNumber")]
        internal int _LaneNumber { get; set; }

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
                __CompetingIn = new LazyRef<AEvent> ( DState , ( ) => _CompetingIn_ID , v => { _CompetingIn_ID = v; } );
                __CustomDataStore = new LazyList<ACustomDataValue> ( DState , c => c._Competitor_ID == ID );
            }
        }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __CompetingIn.refresh ( );
            __CustomDataStore.refresh ( );
        }

        private LazyRef<AEvent> __CompetingIn;
        internal AEvent _CompetingIn { get { return __CompetingIn.Store; } set { __CompetingIn.Store = value; } }


        //protected EntityRef<AEvent> _CompetingInStorage = new EntityRef<AEvent>();
        //[Association(IsForeignKey = true, Storage = "_CompetingInStorage", ThisKey = "_CompetingIn_ID", DeleteOnNull =true)]
        //internal AEvent _CompetingIn { get { return _CompetingInStorage.Entity; } set { _CompetingInStorage.Entity = value; } }

        private LazyList< ACustomDataValue> __CustomDataStore = new LazyList<ACustomDataValue>();
        internal ACustomDataValue[] _CustomDataStore { get { return __CustomDataStore.ToArray ( ); } }

        //private EntitySet<ACustomDataValue> _CustomDataStorage = new EntitySet<ACustomDataValue>();
        //[Association(OtherKey = "_Competitor_ID", Storage = "_CustomDataStorage")]
        //internal EntitySet<ACustomDataValue> _CustomDataStore { get { return _CustomDataStorage; } set { _CustomDataStorage.Assign(value); } }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<ACompetitor> ( this );
        }
    }

    public partial class Competitor : ACompetitor
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
                __Athlete = new LazyRefN<Athlete> ( DState , ( ) => _Athlete_ID , v => { _Athlete_ID = v; } );
            }
        }

        [Column ( Name = "Athlete_ID" , CanBeNull = true )]
        internal int? _Athlete_ID { get; set; }

        private LazyRefN < Athlete > __Athlete;
        internal Athlete _Athlete { get { return __Athlete.Store ; } set { __Athlete.Store = value; } }

        //private EntityRef<Athlete> _AthleteStorage = new EntityRef<Athlete>();
        ////[Association(ThisKey = "_Athlete_ID", IsForeignKey = true, Storage = "_AthleteStorage", DeleteOnNull =true)]
        //[Association(ThisKey = "_Athlete_ID", IsForeignKey = true, Storage = "_AthleteStorage")]
        //internal Athlete _Athlete { get { return _AthleteStorage.Entity; } set { _AthleteStorage.Entity = value; } }

        public override void voidStorage ( bool softRefresh = true )
        {
            base.voidStorage ( );
            __Athlete.refresh ( );
        }

    }


    public partial class StudentHeatedCompetitor : StudentCompetitor, IHeatedCompetitor, ILanedHeatedCompetitor
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
                __HeatEvent = new LazyRefN<AEvent> ( DState , ( ) => _HeatEvent_ID , v => { _HeatEvent_ID = v; } );
            }
        }

        [Column ( Name = "HeatEvent_ID" , CanBeNull = true )]
        internal int? _HeatEvent_ID { get; set; }

        [Column (Name= "InFinal")]
        internal bool _InFinal { get; set; }

        [Column (Name= "HeatLaneNumber")]
        internal int _HeatLaneNumber { get; set; }

        //private EntityRef<AEvent> _HeatEventStorage = new EntityRef<AEvent>();
        //[Association(IsForeignKey = true, Storage = "_HeatEventStorage", ThisKey = "_HeatEvent_ID")]
        //internal AEvent _HeatEvent { get { return _HeatEventStorage.Entity; } set { _HeatEventStorage.Entity = value; } }

        private LazyRefN < AEvent > __HeatEvent;
        internal AEvent _HeatEvent { get { return __HeatEvent.Store; } set { __HeatEvent.Store = value; } }

        public override void voidStorage ( bool softRefresh = true )
        {
            base.voidStorage ( );
            __HeatEvent.refresh ( );
        }


    }

    public partial class Squad : ACompetitor
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
                __Competitor1 = new LazyRefN<Athlete> ( DState , ( ) => _Competitor1_ID , v => { _Competitor1_ID = v; } );
                __Competitor2 = new LazyRefN<Athlete> ( DState , ( ) => _Competitor2_ID , v => { _Competitor2_ID = v; } );
                __Competitor3 = new LazyRefN<Athlete> ( DState , ( ) => _Competitor3_ID , v => { _Competitor3_ID = v; } );
                __Competitor4 = new LazyRefN<Athlete> ( DState , ( ) => _Competitor4_ID , v => { _Competitor4_ID = v; } );
            }
        }


        [Column (Name = "Competitor1_ID", CanBeNull = true)]
        internal int? _Competitor1_ID { get; set; }

        [Column(Name = "Competitor2_ID", CanBeNull = true)]
        internal int? _Competitor2_ID { get; set; }

        [Column(Name = "Competitor3_ID", CanBeNull = true)]
        internal int? _Competitor3_ID { get; set; }

        [Column(Name = "Competitor4_ID", CanBeNull = true)]
        internal int? _Competitor4_ID { get; set; }

        private LazyRefN < Athlete > __Competitor1;
        private LazyRefN < Athlete > __Competitor2;
        private LazyRefN < Athlete > __Competitor3;
        private LazyRefN < Athlete > __Competitor4;

        internal Athlete _Competitor1 { get { return __Competitor1.Store; } set { __Competitor1.Store = value; } }
        internal Athlete _Competitor2 { get { return __Competitor2.Store; } set { __Competitor2.Store = value; } }
        internal Athlete _Competitor3 { get { return __Competitor3.Store; } set { __Competitor3.Store = value; } }
        internal Athlete _Competitor4 { get { return __Competitor4.Store; } set { __Competitor4.Store = value; } }

        public override void voidStorage ( bool softRefresh = true )
        {
            base.voidStorage ( );
            __Competitor1.refresh ( );
            __Competitor2.refresh ( );
            __Competitor3.refresh ( );
            __Competitor4.refresh ( );
        }


        //private EntityRef<Athlete> _Competitor1Storage = new EntityRef<Athlete>();
        //[Association(IsForeignKey = true, Storage = "_Competitor1Storage", ThisKey = "_Competitor1_ID")]
        //internal Athlete _Competitor1 { get { return _Competitor1Storage.Entity; } set { _Competitor1Storage.Entity = value; } }

        //private EntityRef<Athlete> _Competitor2Storage = new EntityRef<Athlete>();
        //[Association(IsForeignKey = true, Storage = "_Competitor2Storage", ThisKey = "_Competitor2_ID")]
        //internal Athlete _Competitor2 { get { return _Competitor2Storage.Entity; } set { _Competitor2Storage.Entity = value; } }

        //private EntityRef<Athlete> _Competitor3Storage = new EntityRef<Athlete>();
        //[Association(IsForeignKey = true, Storage = "_Competitor3Storage", ThisKey = "_Competitor3_ID")]
        //internal Athlete _Competitor3 { get { return _Competitor3Storage.Entity; } set { _Competitor3Storage.Entity = value; } }

        //private EntityRef<Athlete> _Competitor4Storage = new EntityRef<Athlete>();
        //[Association(IsForeignKey = true, Storage = "_Competitor4Storage", ThisKey = "_Competitor4_ID")]
        //internal Athlete _Competitor4 { get { return _Competitor4Storage.Entity; } set { _Competitor4Storage.Entity = value; } }

    }

}