using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ChampionshipSolutions.DM
{
    [Table ( Name = "Standards" )]
    public partial class Standard
    {

        [Column ( IsPrimaryKey = true , Name = "ID" )]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column ( Name = "RawValue" )]
        internal int _RawValue { get; set; }

        [Column ( Name = "Event_ID" )]
        internal int _Event_ID { get; set; }

        [Column ( Name = "Name" )]
        internal string _Name { get; set; }

        [Column ( Name = "ShortName" )]
        internal string _ShortName { get; set; }

        //private EntityRef<AEvent> _EventStorage = new EntityRef<AEvent>();
        //[Association ( IsForeignKey = true , Storage = "_EventStorage" , ThisKey = "_Event_ID" , DeleteOnNull = true )]
        //internal AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

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
                __Event = new LazyRef<AEvent> ( DState , ( ) => _Event_ID , v => { _Event_ID = v; } );
            }
        }

        private LazyRef<AEvent > __Event;
        internal AEvent _Event { get { return __Event.Store; } set { __Event.Store = value; } }

        public void voidStorage ( bool softRefresh = true )
        {
            __Event.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<Standard> ( this );
        }


    }

    #region Legacy Standards data

    #region Private Data Stores

    //[Column ( Name = "NationalStandard_RawValue" )]
    //private int _NationalStandard_RawValue { get; set; }

    //[Column ( Name = "NationalStandard_ValueType" )]
    //private int _NationalStandard_ValueType { get; set; }

    //[Column ( Name = "EntryStandard_RawValue" )]
    //private int _EntryStandard_RawValue { get; set; }

    //[Column ( Name = "EntryStandard_ValueType" )]
    //private int _EntryStandard_ValueType { get; set; }

    //[Column ( Name = "CountyStandard_RawValue" )]
    //private int _CountyStandard_RawValue { get; set; }

    //[Column ( Name = "CountyStandard_ValueType" )]
    //private int _CountyStandard_ValueType { get; set; }

    //[Column ( Name = "DistricStandard_RawValue" )]
    //private int _DistricStandard_RawValue { get; set; }

    //[Column ( Name = "DistricStandard_ValueType" )]
    //private int _DistricStandard_ValueType { get; set; }

    //[Column ( Name = "CountyBestPerformance_RawValue" )]
    //private int _CountyBestPerformance_RawValue { get; set; }

    //[Column ( Name = "CountyBestPerformance_ValueType" )]
    //private int _CountyBestPerformance_ValueType { get; set; }

    #endregion

    //[Column ( Name = "CountyBestPerformanceName" )]
    //private string _CountyBestPerformanceName { get; set; }

    //[Column ( Name = "CountyBestPerformanceYear" )]
    //private int _CountyBestPerformanceYear { get; set; }

    //[Column ( Name = "CountyBestPerformanceArea" )]
    //private string _CountyBestPerformanceArea { get; set; }


    #endregion

}
