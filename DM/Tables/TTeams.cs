using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    /// <summary>
    /// Teams are usually geographical areas
    /// </summary>
    [Table(Name = "Teams")]
    public partial class Team
    {

        [Column(IsPrimaryKey = true,Name ="ID")]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column (Name= "Name")]
        internal string _Name { get; set; }

        [Column (Name= "ShortName")]
        internal string _ShortName { get; set; }

        [Column ( Name = "Championship_ID" , CanBeNull = false )]
        internal int _Championship_ID { get; set; }


        //private EntityRef<Championship> _ChampionshipStorage = new EntityRef<Championship>();
        //[Association ( IsForeignKey = true , Storage = "_ChampionshipStorage" , ThisKey = "_Championship_ID" , DeleteOnNull = true )]
        //internal Championship _Championship { get { return _ChampionshipStorage.Entity; } set { _ChampionshipStorage.Entity = value; } }

        //private EntitySet<AthleteTeamChamptionship> _AthletesStorage = new EntitySet<AthleteTeamChamptionship>();
        //[Association(Storage = "_AthletesStorage", OtherKey = "_Team_ID")]
        //internal EntitySet<AthleteTeamChamptionship> _Athletes { get { return _AthletesStorage; } set { _AthletesStorage.Assign(value); } }

        //private EntitySet<SchoolTeams> _SchoolsStorage = new EntitySet<SchoolTeams>();
        //[Association(Storage = "_SchoolsStorage", OtherKey = "_Team_ID")]
        //internal EntitySet<SchoolTeams> _Schools { get { return _SchoolsStorage; } set { _SchoolsStorage.Assign(value); } }

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
                __Athletes = new LazyList<AthleteTeamChamptionship> ( DState , c => c._Team_ID == ID );
                __Schools = new LazyList<SchoolTeams> ( DState , c => c._Team_ID == ID );
            }
        }

        private LazyRef<Championship> __Championship;
        internal Championship _Championship { get { return __Championship.Store; } set { __Championship.Store = value; } }

        private LazyList< AthleteTeamChamptionship> __Athletes = new LazyList<AthleteTeamChamptionship>();
        internal AthleteTeamChamptionship[] _Athletes { get { return __Athletes.ToArray ( ); } }

        private LazyList<SchoolTeams> __Schools = new LazyList<SchoolTeams>();
        internal SchoolTeams[] _Schools { get { return __Schools.ToArray ( ); } }

        public void voidStorage ( bool softRefresh = true )
        {
            __Championship.refresh ( );
            __Athletes.refresh ( );
            __Schools.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<Team> ( this );
        }

    }
}
