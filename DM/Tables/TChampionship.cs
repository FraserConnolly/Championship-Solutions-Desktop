using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "Championships")]
    [InheritanceMapping(Code = "Championship", Type = typeof(Championship), IsDefault = true)]
    public partial class Championship : IID 
    {

        [Column(IsPrimaryKey = true, Name = "ID")]
        public int ID { get; set; }

        [Column(IsDiscriminator = true, Name = "Discriminator")]
        public string Discriminator { get; set; }

        [Column (Name = "FixedName")]
        internal string _FixedName { get; set; }

        [Column(Name = "Name")]
        internal string _Name { get; set; }

        [Column(Name = "ShortName")]
        internal string _ShortName { get; set; }

        [Column(CanBeNull = true, DbType = "DateTime", Name = "Date")]
        internal DateTime? _Date { get; set; }

        [Column(Name = "Location")]
        internal string _Location { get; set; }

        [Column(Name = "Locked")]
        internal string _Locked { get; set; }

        [Column(CanBeNull = true, Name = "AgeDateReference")]
        internal DateTime? _AgeDateReference { get; set; }

        [Column(Name = "WebServerEnabled")]
        internal bool _WebServerEnabled { get; set; }

        [Column(Name = "WebServerPort")]
        internal int _WebServerPort { get; set; }

        [Column(CanBeNull = true, Name = "ZippedFileStore_ID")]
        internal int? _ZippedFileStore_ID { get; set; }

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
                __Events = new LazyList<AEvent> ( DState , c => c._Championship_ID == ID );
                __Teams  = new LazyList<Team>   ( DState , c => c._Championship_ID == ID );
                __Groups  = new LazyList<Group> ( DState , c => c._Championship_ID == ID );
                __VestActions  = new LazyList<VestActions> ( DState , c => c._Championship_ID == ID );
                __CustomDataStore = new LazyList<ACustomDataValue> ( DState , c => c._Championship_ID == ID );
                __ZippedFileStore = new LazyRefN<FileStorage> ( DState , ( ) => _ZippedFileStore_ID , v => { _ZippedFileStore_ID = v; } );
            }
        }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Events.refresh ( );
            __Teams.refresh ( );
            __Groups.refresh ( );
            __VestActions.refresh ( );
            __CustomDataStore.refresh ( );
            __ZippedFileStore.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<Championship> ( this );
        }


        private LazyList<AEvent > __Events = new LazyList<AEvent>();
        internal AEvent[] _Events { get { return __Events.ToArray(); } }


        //private EntitySet<AEvent> _EventsStorage = new EntitySet<AEvent>();
        //[Association(Storage = "_EventsStorage", OtherKey = "_Championship_ID")]
        //internal EntitySet<AEvent> _Events { get { return _EventsStorage; } set { _EventsStorage.Assign(value); } }

        private LazyList<Team> __Teams = new LazyList<Team>();
        internal Team[] _Teams { get { return __Teams.ToArray ( ); } }

        //private EntitySet<Team> _CTeamsStorage = new EntitySet<Team>();
        //[Association(Storage = "_CTeamsStorage", OtherKey = "_Championship_ID")]
        //internal EntitySet<Team> _Teams { get { return _CTeamsStorage; } set { _CTeamsStorage.Assign ( value ); } }

        private LazyList<Group> __Groups = new LazyList<Group>();
        internal Group[] _Groups { get { return __Groups.ToArray ( ); } }


        //private EntitySet<Group> _GroupsStorage = new EntitySet<Group>();
        //[Association(Storage = "_GroupsStorage", OtherKey = "_Championship_ID")]
        //internal EntitySet<Group> _Groups { get { return _GroupsStorage; } set { _GroupsStorage.Assign(value); } }

        private LazyList<VestActions> __VestActions = new LazyList<DM.VestActions>();
        internal VestActions[] _VestActions { get { return __VestActions.ToArray ( ); } }

        //private EntitySet<VestActions> _VestActionsStorage = new EntitySet<VestActions>();
        //[Association(Storage = "_VestActions", OtherKey = "_Championship_ID")]
        //internal EntitySet<VestActions> _VestActions { get { return _VestActionsStorage; } set { _VestActionsStorage.Assign(value); } }

        private LazyList<ACustomDataValue> __CustomDataStore = new LazyList<ACustomDataValue> { };
        internal ACustomDataValue[] _CustomDataStore { get { return __CustomDataStore.ToArray ( ); } }


        //private EntitySet<ACustomDataValue> _CustomDataStorage = new EntitySet<ACustomDataValue>();
        //[Association(OtherKey = "_Championship_ID", Storage = "_CustomDataStorage")]
        //internal EntitySet<ACustomDataValue> _CustomDataStore { get { return _CustomDataStorage; } set { _CustomDataStorage.Assign(value); } }

        private LazyRefN<FileStorage> __ZippedFileStore;
        internal FileStorage _ZippedFileStore { get { return __ZippedFileStore.Store; } set { __ZippedFileStore.Store = value; } }

        //private EntityRef<FileStorage> _ZippedFileStoreStorage = new EntityRef<FileStorage>();
        //[Association(ThisKey = "_ZippedFileStore_ID", Storage = "_ZippedFileStoreStorage")]
        //internal FileStorage _ZippedFileStore { get { return _ZippedFileStoreStorage.Entity; } set { _ZippedFileStoreStorage.Entity = value; } }

    }

}

