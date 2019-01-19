using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "SchoolTeams")]
    public partial class SchoolTeams : IID 
    {

        public SchoolTeams ()
        {
            DState = null;
        }

        [Column ( IsPrimaryKey = true , Name = "ID" )]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        //[Column(Name="School_ID", IsPrimaryKey = true)]
        [Column ( Name = "School_ID" )]
        internal int _School_ID { get; set; }

        //private EntityRef<School> _SchoolsStorage = new EntityRef<School>();
        //[Association ( IsForeignKey = true , Storage = "_SchoolsStorage" , ThisKey = "_School_ID" , DeleteOnNull = true , DeleteRule = "NO ACTION" )]
        //internal School _School { get { return _SchoolsStorage.Entity; } set { _SchoolsStorage.Entity = value; } }

        //[Column(Name = "Team_ID", IsPrimaryKey = true)]
        [Column ( Name = "Team_ID" )]
        internal int _Team_ID { get; set; }

        //private EntityRef<Team> _TeamStorage = new EntityRef<Team>();
        //[Association ( IsForeignKey = true , Storage = "_TeamStorage" , ThisKey = "_Team_ID" , DeleteOnNull = true , DeleteRule = "NO ACTION" )]
        //internal Team _Team { get { return _TeamStorage.Entity; } set { _TeamStorage.Entity = value; } }

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
                __Team = new LazyRef<Team> ( DState , ( ) => _Team_ID , v => { _Team_ID = v; } );
            }
        }

        private LazyRef<School> __School;
        internal School _School { get { return __School.Store; } set { __School.Store = value; } }

        private LazyRef<Team> __Team;
        internal Team _Team { get { return __Team.Store; } set { __Team.Store = value; } }

        public void voidStorage ( bool softRefresh = true )
        {
            __School.refresh ( );
            __Team.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<SchoolTeams> ( this );
        }

    }
}
