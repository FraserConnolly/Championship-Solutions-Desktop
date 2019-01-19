using System;
using System.Collections;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "Events")]
    [InheritanceMapping(Code = "IndividualTimedHeatEvent", Type = typeof(IndividualTimedHeatEvent))]
    [InheritanceMapping(Code = "IndividualTimedFinalEvent", Type = typeof(IndividualTimedFinalEvent))]
    [InheritanceMapping(Code = "IndividualTimedFinalSchoolEvent", Type = typeof(IndividualTimedFinalSchoolEvent))]
    [InheritanceMapping(Code = "IndividualTimedEvent", Type = typeof(IndividualTimedEvent), IsDefault = true)]
    [InheritanceMapping(Code = "SchoolTimedIndividualEvent", Type = typeof(IndividualTimedSchoolEvent))]
    [InheritanceMapping(Code = "IndividualDistanceEvent", Type = typeof(IndividualDistanceEvent))]
    [InheritanceMapping(Code = "SquadTimedEvent", Type = typeof(SquadTimedEvent))]
    [InheritanceMapping(Code = "SquadDistanceEvent", Type = typeof(SquadDistanceEvent))]
    public abstract partial class AEvent : IID
    {

        [Column(IsPrimaryKey = true,Name ="ID")]
        public int ID { get; set; }

        [Column(IsDiscriminator = true,Name = "Discriminator")]
        public string Discriminator { get; set; }

        [Column (Name= "Name")]
        internal string _Name { get; set; }

        [Column (Name= "ShortName")]
        internal string _ShortName { get; set; }

        [Column (Name= "Description")]
        internal string _Description { get; set; }

        [Column (Name= "StartTime")]
        internal string _StartTime { get; set; }

        [Column ( Name = "EndTime" )]
        internal int _EndTime { get; set; }

        [Column(Name = "State")]
        internal EventState _State { get; set; }

        private DataState _DState ;

        public virtual  DataState DState { get
            {
                return _DState;
            }
            set
            {
                _DState = value;
                __EnteredCompetitors = new LazyList<ACompetitor> ( DState , c => c._CompetingIn_ID == ID );
                __Standards = new LazyList<Standard> ( DState , c => c._Event_ID == ID );
                __Results = new LazyList<AResult> ( DState , c => c._Event_ID == ID );
                __Groups = new LazyList<EventGroups> ( DState , c => c._Event_ID == ID );
                __Files = new LazyList<FileStorage> ( DState , c => c._Event_ID == ID );
                __CustomDataStore = new LazyList<ACustomDataValue> ( DState , c => c._Event_ID == ID );
                __Championship = new LazyRef<Championship> ( DState , ( ) => _Championship_ID , v => { _Championship_ID = v; } );
                __ResultsTemplate = new LazyRefN<Template> ( DState , ( ) => _ResultsTemplate_ID , v => { _ResultsTemplate_ID = v; } );
                __DataEntryTemplate = new LazyRefN<Template> ( DState , ( ) => _DataEntryTemplate_ID , v => { _DataEntryTemplate_ID = v; } );
                __CertificateTemplate = new LazyRefN<Template> ( DState , ( ) => _CertificateTemplate_ID , v => { _CertificateTemplate_ID = v; } );
                __VestTemplate = new LazyRefN<Template> ( DState , ( ) => _VestTemplate_ID , v => { _VestTemplate_ID = v; } );
            }
        }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __EnteredCompetitors.refresh ( );
            __Standards.refresh ( );
            __Results.refresh ( );
            __Groups.refresh ( );
            __Files.refresh ( );
            __CustomDataStore.refresh ( );
            __Championship.refresh ( );
            __ResultsTemplate.refresh ( );
            __DataEntryTemplate.refresh ( );
            __CertificateTemplate.refresh ( );
            __VestTemplate.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<AEvent> ( this );
        }

        #region Templates

        [Column (Name = "ResultsTemplate", CanBeNull =true)]
        internal int? _ResultsTemplate_ID { get; set; }

        private LazyRefN<Template> __ResultsTemplate;
        internal Template _ResultsTemplate { get { return __ResultsTemplate.Store; } set { __ResultsTemplate.Store = value; } }


        //private EntityRef<Template> _ResultsTemplateStorage = new EntityRef<Template>();
        //[Association ( IsForeignKey = true , Storage = "_ResultsTemplateStorage" , ThisKey = "_ResultsTemplate_ID" )]
        //internal Template _ResultsTemplate { get { return _ResultsTemplateStorage.Entity; } set { _ResultsTemplateStorage.Entity = value; } }


        [Column (Name = "DataEntryTemplate" , CanBeNull = true )]
        internal int? _DataEntryTemplate_ID { get; set; }

        //private EntityRef<Template> _DataEntryTemplateStorage = new EntityRef<Template>();
        //[Association ( IsForeignKey = true , Storage = "_DataEntryTemplateStorage" , ThisKey = "_DataEntryTemplate_ID" )]
        //internal Template _DataEntryTemplate { get { return _DataEntryTemplateStorage.Entity; } set { _DataEntryTemplateStorage.Entity = value; } }

        private LazyRefN<Template> __DataEntryTemplate;
        internal Template _DataEntryTemplate { get { return __DataEntryTemplate.Store; } set { __DataEntryTemplate.Store = value; } }

        [Column (Name = "CertificateTemplate" , CanBeNull = true )]
        internal int? _CertificateTemplate_ID { get; set; }

        //private EntityRef<Template> _CertificateTemplateStorage = new EntityRef<Template>();
        //[Association ( IsForeignKey = true , Storage = "_CertificateTemplateStorage" , ThisKey = "_CertificateTemplate_ID" )]
        //internal Template _CertificateTemplate { get { return _CertificateTemplateStorage.Entity; } set { _CertificateTemplateStorage.Entity = value; } }

        private LazyRefN<Template> __CertificateTemplate;
        internal Template _CertificateTemplate { get { return __CertificateTemplate.Store; } set { __CertificateTemplate.Store = value; } }

        [Column (Name = "VestTemplate" , CanBeNull = true )]
        internal int? _VestTemplate_ID { get; set; }

        //private EntityRef<Template> _VestTemplateStorage = new EntityRef<Template>();
        //[Association ( IsForeignKey = true , Storage = "_VestTemplateStorage" , ThisKey = "_VestTemplate_ID" )]
        //internal Template _VestTemplate { get { return _VestTemplateStorage.Entity; } set { _VestTemplateStorage.Entity = value; } }

        private LazyRefN<Template> __VestTemplate;
        internal Template _VestTemplate { get { return __VestTemplate.Store; } set { __VestTemplate.Store = value; } }

        #endregion

        #region Event Range Data

        [Column (Name = "EventRanges_MaxCompetitors")]
        internal int _MaxCompetitors { get; set; }

        [Column(Name = "EventRanges_MinCompetitors")]
        internal int _MinCompetitors { get; set; }

        [Column(Name = "EventRanges_MaxGuests")]
        internal int _MaxGuests { get; set; }

        [Column(Name = "EventRanges_MaxCompetitorsPerTeam")]
        internal int _MaxCompetitorsPerTeam { get; set; }

        [Column(Name = "EventRanges_TopIndividualCertificates")]
        internal int _TopIndividualCertificates { get; set; }

        [Column(Name = "EventRanges_TopLowerYearGroupInividualCertificates")]
        internal int _TopLowerYearGroupInividualCertificates { get; set; }

        [Column(Name = "EventRanges_TeamASize")]
        internal int _TeamASize { get; set; }

        [Column(Name = "EventRanges_TeamBSize")]
        internal int _TeamBSize { get; set; }

        [Column(Name = "EventRanges_TeamBForScoringTeamOnly")]
        internal bool _TeamBForScoringTeamOnly { get; set; }

        [Column(Name = "EventRanges_ScoringTeams")]
        internal int _ScoringTeams { get; set; }

        [Column(Name = "EventRanges_Lanes")]
        internal int _Lanes { get; set; }

        #endregion

        #region Championship Best Performance

        [Column ( Name = "CountyBestPerformance_RawValue" )]
        internal int _CountyBestPerformance_RawValue { get; set; }

        [Column ( Name = "CountyBestPerformanceName" )]
        internal string _CountyBestPerformanceName { get; set; }

        [Column ( Name = "CountyBestPerformanceYear" )]
        internal int _CountyBestPerformanceYear { get; set; }

        [Column ( Name = "CountyBestPerformanceArea" )]
        internal string _CountyBestPerformanceArea { get; set; }

        #endregion

        [Column(Name = "Championship_ID")]
        internal int _Championship_ID { get; set; }

        [Column ( Name = "ResultsDisplayDescription" )]
        internal int _ResultsDisplayDescription { get; set; }

        //private EntityRef<Championship> _ChampionshipStorage = new EntityRef<Championship>();
        //[Association(IsForeignKey = true, Storage = "_ChampionshipStorage", ThisKey = "_Championship_ID", DeleteOnNull =true)]
        //internal Championship _Championship { get { return _ChampionshipStorage.Entity; } set { _ChampionshipStorage.Entity = value; } }

        private LazyRef<Championship> __Championship;
        internal Championship _Championship { get { return __Championship.Store; } set { __Championship.Store = value; } }


        //private EntitySet<ACompetitor> _EnteredCompetitorsStorage = new EntitySet<ACompetitor>();
        //[Association(Storage = "_EnteredCompetitorsStorage", ThisKey = "ID", OtherKey = "_CompetingIn_ID")]
        //internal EntitySet<ACompetitor> _EnteredCompetitors { get { return _EnteredCompetitorsStorage; } set { _EnteredCompetitorsStorage.Assign(value); } }

        private LazyList<ACompetitor> __EnteredCompetitors { get; set; } 
        internal ACompetitor[] _EnteredCompetitors { get { return __EnteredCompetitors.ToArray(); } }


        //private EntitySet<Standard> _StandardsStorage = new EntitySet<Standard>();
        //[Association ( Storage = "_StandardsStorage" , ThisKey = "ID" , OtherKey = "_Event_ID" )]
        //internal EntitySet<Standard> _Standards { get { return _StandardsStorage; } set { _StandardsStorage.Assign ( value ); } }

        //private Standard[] __Standards = new Standard[] { };
        private LazyList<Standard> __Standards = new LazyList<Standard>();
        internal Standard[] _Standards { get { return __Standards.ToArray(); } }

        //private EntitySet<AResult> _ResultsStorage = new EntitySet<AResult>();
        //[Association(Storage = "_ResultsStorage", OtherKey = "_Event_ID")]
        //internal EntitySet<AResult> _Results { get { return _ResultsStorage; } set { _ResultsStorage.Assign(value); } }

        //private AResult[] __Results = new AResult[] { };
        private LazyList < AResult >  __Results = new LazyList<AResult>();
        internal AResult[] _Results { get { return __Results.ToArray(); } }

        //private EntitySet<EventGroups> _EventGroupsStorage = new EntitySet<EventGroups>();
        //[Association(Storage = "_EventGroupsStorage" , OtherKey = "_Event_ID")]
        //internal EntitySet<EventGroups> _Groups { get { return _EventGroupsStorage; } set { _EventGroupsStorage.Assign(value); } }

        //private EventGroups[] __Groups = new EventGroups[] { };
        private LazyList<EventGroups> __Groups = new LazyList<EventGroups>();
        internal EventGroups[] _Groups { get { return __Groups.ToArray(); } }
        
        //private EntitySet<ACustomDataValue> _CustomDataStoreStorage = new EntitySet<ACustomDataValue>();
        //[Association(Storage = "_CustomDataStoreStorage", OtherKey = "_Event_ID" )]
        //internal EntitySet<ACustomDataValue> _CustomDataStore { get { return _CustomDataStoreStorage; } set { _CustomDataStoreStorage.Assign(value); } }

        //private ACustomDataValue[] __CustomDataStore = new ACustomDataValue[] { };
        private LazyList<ACustomDataValue> __CustomDataStore = new LazyList<ACustomDataValue>();
        internal ACustomDataValue[] _CustomDataStore { get { return __CustomDataStore.ToArray(); } }

        //private EntitySet<FileStorage> _FilesStorage = new EntitySet<FileStorage>();
        //[Association(OtherKey = "_Event_ID", Storage = "_FilesStorage")]
        //internal EntitySet<FileStorage> _Files { get { return _FilesStorage; } set { _FilesStorage.Assign(value); } }

        private LazyList<FileStorage> __Files = new LazyList<FileStorage>();
        internal FileStorage[] _Files { get { return __Files.ToArray(); } }


    }


    public partial class IndividualTimedEvent : AIndividualEvent
    {

        [Column ( CanBeNull = true , Name = "Final_ID" )]
        internal int? _Final_ID { get; set; }

    }

    public partial class IndividualTimedHeatEvent : IndividualTimedEvent
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
                __Final = new LazyRefN<AEvent> ( DState , ( ) => _Final_ID , v => { _Final_ID = v; } );
            }
        }

        //private EntityRef<IndividualTimedFinalEvent> _FinalStorage = new EntityRef<IndividualTimedFinalEvent>();
        //[Association(Storage = "_FinalStorage", ThisKey = "_Final_ID", IsForeignKey = true)]
        //internal IndividualTimedFinalEvent _Final { get { return _FinalStorage.Entity; } set { _FinalStorage.Entity = value; } }

        private LazyRefN< AEvent > __Final;
        internal IndividualTimedFinalEvent _Final { get { return (IndividualTimedFinalEvent)__Final.Store; } set { __Final.Store = value; } }

        public override void voidStorage ( bool softRefresh = true )
        {
            base.voidStorage ( );
            __Final.refresh ( );
        }
    }

        public partial class IndividualTimedFinalEvent : IndividualTimedEvent
        {

        [Column ( CanBeNull = true , Name = "HeatRunAsFinal" )]
        internal bool? _HeatRunAsFinal { get; set; }

        public override DataState DState
        {
            get
            {
                return base.DState;
            }

            set
            {
                base.DState = value;
                __Heats = new LazyList<IndividualTimedHeatEvent> ( DState , c => c._Final_ID == ID );
            }
        }


        //private EntitySet<IndividualTimedHeatEvent> _HeatsStorage = new EntitySet<IndividualTimedHeatEvent>();
        //[Association(ThisKey = "ID", OtherKey = "_Final_ID", Storage = "_HeatsStorage")]
        //internal EntitySet<IndividualTimedHeatEvent> _Heats { get { return _HeatsStorage; } set { _HeatsStorage.Assign(value); } }

        private LazyList<IndividualTimedHeatEvent> __Heats = new LazyList<IndividualTimedHeatEvent>();
        internal IndividualTimedHeatEvent[] _Heats
        {
            get
            {
                return __Heats.ToArray();
            }
        }

        public override void voidStorage ( bool softRefresh = true )
        {
            base.voidStorage ( );
            __Heats.refresh ( );
        }

    }

    public partial class IndividualTimedFinalSchoolEvent : IndividualTimedFinalEvent, ISchoolEvent
    {

        [Column(Name = "LowerYearGroup")]
        internal int _LowerYearGroup { get; set; }

    }



    public partial class IndividualTimedSchoolEvent : IndividualTimedEvent
    {

        [Column(Name = "LowerYearGroup")]
        internal int _LowerYearGroup { get; set; }

    }

}