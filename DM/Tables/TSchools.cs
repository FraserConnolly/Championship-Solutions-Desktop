using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "Schools")]
    public partial class School 
    {

        [Column(IsPrimaryKey = true, Name ="ID")]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column (Name= "Name")]
        internal string _Name { get; set; }

        [Column (Name= "ShortName")]
        internal string _ShortName { get; set; }

        [Column (Name= "Head")]
        internal int? _Head_ID { get; set; }

        [Column (Name= "LetterGreating")]
        internal string _LetterGreating { get; set; }

        //private EntitySet<SchoolTeams> _TeamsStorage = new EntitySet<SchoolTeams>();
        //[Association(Storage = "_TeamsStorage", OtherKey = "_School_ID")]
        //internal EntitySet<SchoolTeams> _Teams { get { return _TeamsStorage; } set { _TeamsStorage.Assign(value); } }

        // This caused an error that prevented SchoolsPage from opening. 
        //private EntitySet<StudentAthlete> _PersonStorage = new EntitySet<StudentAthlete>();
        //[Association ( Storage = "_PersonStorage" , OtherKey = "_Attends_ID" )]
        //private EntitySet<StudentAthlete> _People { get { return _PersonStorage; } set { _PersonStorage.Assign ( value ); } }   

        //private EntitySet<Staff> _StaffStorage = new EntitySet<Staff>();
        //[Association ( Storage = "_StaffStorage" , OtherKey = "_School_ID" )]
        //internal EntitySet<Staff> _Staff { get { return _StaffStorage; } set { _StaffStorage.Assign ( value ); } }

        //private EntityRef<Person> _HeadStorage = new EntityRef<Person>();
        //[Association ( Storage = "_HeadStorage" , ThisKey = "_Head_ID" )]
        //private Person _Head { get { return _HeadStorage.Entity; }  set { _HeadStorage.Entity = value ; } }

        // removed in V3-0
        //private EntitySet<AContactDetail> _ContactStorage = new EntitySet<AContactDetail>();
        //[Association(Storage = "_ContactStorage", OtherKey = "_School_ID")]
        //protected virtual ICollection<AContactDetail> _Contacts { get { return _ContactStorage; } set { _ContactStorage.Assign(value); } }

        // Added _People in V3-0 to see if a school has any people assigned to it to prevent illegal deletion 

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
                __Teams = new LazyList<SchoolTeams> ( DState , c => c._School_ID == ID );
                __Staff = new LazyList<Staff> ( DState , c => c._School_ID == ID );
            }
        }

        private LazyList<SchoolTeams> __Teams = new LazyList<SchoolTeams>();
        internal SchoolTeams[] _Teams { get { return __Teams.ToArray ( ); } }

        private LazyList<Staff> __Staff = new LazyList<Staff>();
        internal Staff[] _Staff { get { return __Staff.ToArray ( ); } }

        public void voidStorage ( bool softRefresh = true )
        {
            __Staff.refresh ( );
            __Teams.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<School> ( this );
        }


    }
}
