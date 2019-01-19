using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM.DataAccess
{
    //public partial class SchoolTeams : IDataAccess
    //{

    //}

//    [Table (Name = "SchoolTeams")]
//    public partial class SchoolTeams : IID 
//    {
//#pragma warning disable 0169

//        [Column ( IsPrimaryKey = true , Name = "ID" )]
//        protected int ID { get; set; }

//        //[Column(Name="School_ID", IsPrimaryKey = true)]
//        [Column(Name="School_ID")]
//        private int _School_ID;

//        private EntityRef<School> _SchoolsStorage = new EntityRef<School>();
//        [Association ( IsForeignKey = true , Storage = "_SchoolsStorage" , ThisKey = "_School_ID" , DeleteOnNull = true , DeleteRule = "NO ACTION" )]
//        private School _School { get { return _SchoolsStorage.Entity; } set { _SchoolsStorage.Entity = value; } }

//        //[Column(Name = "Team_ID", IsPrimaryKey = true)]
//        [Column(Name = "Team_ID")]
//        private int _Team_ID;

//        private EntityRef<Team> _TeamStorage = new EntityRef<Team>();
//        [Association ( IsForeignKey = true , Storage = "_TeamStorage" , ThisKey = "_Team_ID" , DeleteOnNull = true , DeleteRule = "NO ACTION" )]
//        private Team _Team { get { return _TeamStorage.Entity; } set { _TeamStorage.Entity = value; } }

//#pragma warning restore 0169
//    }
}
