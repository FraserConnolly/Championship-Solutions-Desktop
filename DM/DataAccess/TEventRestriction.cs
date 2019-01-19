using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ChampionshipSolutions.DM.DataAccess
{

//    [Table (Name = "EventGroups")]
//    public partial class EventGroups
//    {
//#pragma warning disable 0169

//        [Column ( IsPrimaryKey = true , Name = "ID" )]
//        protected int _ID { get; set; }

//        //[Column (IsPrimaryKey =true, Name = "Event_ID")]
//        [Column ( Name = "Event_ID")]
//        private int _Event_ID;

//        //[Column (IsPrimaryKey = true, Name = "Restriction_ID")]
//        [Column ( Name = "Group_ID")]
//        private int _Group_ID;

//        private EntityRef<AEvent> _EventStorage = new EntityRef<AEvent>();
//        [Association(Storage = "_EventStorage", IsForeignKey = true, ThisKey = "_Event_ID", DeleteOnNull=true)]
//        private AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

//        private EntityRef<Group> _GroupStorage = new EntityRef<Group>();
//        [Association(Storage = "_GroupStorage" , IsForeignKey = true, ThisKey = "_Group_ID" , DeleteOnNull = true)]
//        private Group _Group { get { return _GroupStorage.Entity; } set { _GroupStorage.Entity = value; } }

//#pragma warning restore 0169
//    }
}
