using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    [Table ( Name = "VestActions" )]
    public partial class VestActions : IID
    {

        [Column ( Name = "ID" , IsPrimaryKey = true )]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column ( Name = "WebID" )]
        internal int _WebID { get; set; }

        [Column ( Name = "Championship_ID" )]
        internal int _Championship_ID { get; set; }

        [Column ( Name = "Description" )]
        internal string _Description { get; set; }

        [Column ( Name = "DateStamp" )]
        internal DateTime _DateStamp { get; set; }

        [Column ( Name = "Vest" )]
        internal string _Vest { get; set; }

        [Column ( Name = "Championship" )]
        internal string _ChampionshipName { get; set; }

        [Column ( Name = "EventCode" )]
        internal string _EventCode { get; set; }

        [Column ( Name = "Position" )]
        internal int? _Position { get; set; }

        [Column ( Name = "Time" )]
        internal TimeSpan? _Time { get; set; }

        [Column ( Name = "Ignored" )]
        internal bool _Ignored { get; set; }

        [Column ( Name = "statusDescription" )]
        internal string _statusDescription { get; set; }

        //private EntityRef<Championship> _ChampionshipStorage = new EntityRef<Championship>();
        //[Association ( IsForeignKey = true , Storage = "_ChampionshipStorage" , ThisKey = "_Championship_ID" )]
        //internal Championship _Championship { get { return _ChampionshipStorage.Entity; } set { _ChampionshipStorage.Entity = value; } }

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
            }
        }

        private LazyRef<Championship> __Championship;
        internal Championship _Championship { get { return __Championship.Store; } set { __Championship.Store = value; } }

        public void voidStorage ( bool softRefresh = true )
        {
            __Championship.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<VestActions> ( this );
        }

    }

}
