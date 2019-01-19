using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "EventGroups")]
    public partial class EventGroups : IID
    {

        public EventGroups()
        {
            DState = null;
        }

        [Column ( IsPrimaryKey = true , Name = "ID" )]
        public int ID { get; set; }

        public string Discriminator { get; set; }
        
        //[Column (IsPrimaryKey =true, Name = "Event_ID")]
        [Column ( Name = "Event_ID" )]
        internal int _Event_ID { get; set; }

        //[Column (IsPrimaryKey = true, Name = "Restriction_ID")]
        [Column ( Name = "Group_ID" )]
        internal int _Group_ID { get; set; }

        //private EntityRef<AEvent> _EventStorage = new EntityRef<AEvent>();
        //[Association(Storage = "_EventStorage", IsForeignKey = true, ThisKey = "_Event_ID", DeleteOnNull=true)]
        //internal AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

        //private EntityRef<Group> _GroupStorage = new EntityRef<Group>();
        //[Association(Storage = "_GroupStorage" , IsForeignKey = true, ThisKey = "_Group_ID" , DeleteOnNull = true)]
        //internal Group _Group { get { return _GroupStorage.Entity; } set { _GroupStorage.Entity = value; } }

        private DataState _DState;

        public DataState DState { get { return _DState; }
            set
            {
                _DState = value;
                __Group = new LazyRef<Group> ( DState , ( ) => _Group_ID , v => { _Group_ID = v; } );
                __Event = new LazyRef<AEvent> ( DState , ( ) => _Event_ID , v => { _Event_ID = v; } );
            } 
        }

        internal LazyRef<Group> __Group;// = new LazyRef<Group>();
        internal Group _Group { get { return __Group.Store; } set { __Group.Store = value; } }

        internal LazyRef<AEvent> __Event;//= new LazyRef<AEvent>(null, ( ) => _Event_ID , v => { _Event_ID = v; });
        internal AEvent _Event { get { return __Event.Store; } set { __Event.Store = value; } }

        //private Group __Group;
        //internal Group _Group
        //{
        //    get
        //    {
        //        if ( DState.NeedsUpdating )
        //            __Group = DState.IO.GetID<Group> ( _Group_ID );


        //        return __Group;
        //    }
        //    set
        //    {
        //        _Group_ID = value.ID;
        //        DState.NeedsUpdating = true;
        //    }
        //}


        //private AEvent __Event;
        //internal AEvent _Event
        //{
        //    get
        //    {
        //        if ( DState.NeedsUpdating )
        //            __Event = DState.IO.GetID<AEvent> ( _Event_ID );


        //        return __Event;
        //    }
        //    set
        //    {
        //        _Event_ID = value.ID;
        //        DState.NeedsUpdating = true;
        //    }
        //}

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Event.refresh ( );
            __Group.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<EventGroups> ( this );
        }

    }
}
