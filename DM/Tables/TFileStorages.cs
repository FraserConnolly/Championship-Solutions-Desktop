using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "FileStorages")]
    public partial class FileStorage
    {

        [Column(IsPrimaryKey = true, Name ="ID")]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column ( CanBeNull = true , Name = "Event_ID" )]
        internal int? _Event_ID { get; set; }

        [Column (Name= "Name")]
        internal string _Name { get; set; }

        [Column (Name= "ShortName")]
        internal string _ShortName { get; set; }

        [Column (Name= "CreatedOn")]
        internal DateTime _CreatedOn { get; set; }

        [Column (Name= "Extension")]
        internal string _Extension { get; set; }

        [Column (Name= "FileData")]
        internal byte[] _FileData { get; set; }

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
                __Event = new LazyRefN<AEvent> ( DState , ( ) => _Event_ID , v => { _Event_ID = v; } );
            }
        }


        //private EntityRef<AEvent > _EventStorage = new EntityRef<AEvent>();
        //[Association(IsForeignKey = true, Storage = "_EventStorage", ThisKey = "_Event_ID")]
        //internal AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

        internal LazyRefN<AEvent> __Event;
        internal AEvent _Event { get { return __Event.Store; } set { __Event.Store = value; } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Event.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<FileStorage> ( this );
        }

    }

}