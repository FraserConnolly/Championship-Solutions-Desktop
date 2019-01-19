using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "Staff")]
    public partial class Staff : IID 
    {

        public Staff()
        {
            DState = null;
        }

        [Column ( IsPrimaryKey = true , Name = "ID" )]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column(Name="School_ID")]
        internal int _School_ID;

        [Column (Name ="Title")]
        internal string _Title;

        //private EntityRef<School> _SchoolsStorage = new EntityRef<School>();
        //[Association ( IsForeignKey = true , Storage = "_SchoolsStorage" , ThisKey = "_School_ID" , DeleteOnNull = true , DeleteRule = "NO ACTION" )]
        //internal School _School { get { return _SchoolsStorage.Entity; } set { _SchoolsStorage.Entity = value; } }

        [Column(Name = "Person_ID")]
        private int _Person_ID;

        //private EntityRef<Person> _PersonStorage = new EntityRef<Person>();
        //[Association ( IsForeignKey = true , Storage = "_PersonStorage" , ThisKey = "_Person_ID" , DeleteOnNull = true , DeleteRule = "NO ACTION" )]
        //internal Person _Person { get { return _PersonStorage.Entity; } set { _PersonStorage.Entity = value; } }

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
                __School = new LazyRef<School> ( DState , ( ) => _School_ID , v => { _School_ID = v; } );
                __Person = new LazyRef<Person> ( DState , ( ) => _Person_ID , v => { _Person_ID = v; } );
            }
        }

        private LazyRef<School> __School;
        internal School _School { get { return __School.Store; } set { __School.Store = value; } }

        private LazyRef<Person> __Person;
        internal Person _Person { get { return __Person.Store; } set { __Person.Store = value; } }

        public void voidStorage ( bool softRefresh = true )
        {
            __School.refresh ( );
            __Person.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<Staff> ( this );
        }


    }
}
