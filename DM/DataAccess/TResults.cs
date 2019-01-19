using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM.DataAccess
{

    //[Table (Name = "Results")]
    //[InheritanceMapping(Code = "Result", Type = typeof(Result), IsDefault = true)]
    //public abstract partial class AResult
    //{

    //    [Column(IsPrimaryKey = true, Name ="ID")]
    //    protected int _ID { get; set; }

    //    [Column(IsDiscriminator = true, Name = "Discriminator")]
    //    private string _Discriminator { get; set; }

    //    [Column(Name = "Event_ID")]
    //    protected int _Event_ID;

    //    [Column(CanBeNull = true, Name ="Rank")]
    //    private int? _Rank { get; set; }

    //    [Column(Name = "Competitor_ID", CanBeNull = true)]
    //    protected int? _Competitor_ID;

    //    [Column(Name = "VestNumber_dbVestNumber", CanBeNull = true)]
    //    private string _vestNumberDB;

    //    [Column (Name = "Value_RawValue")]
    //    private int _Value_RawValue { get; set; }

    //    [Column (Name = "Value_ValueType")]
    //    private int _Value_ValueType { get; set; }

    //    private EntityRef<ACompetitor> _CompetitorStorage = new EntityRef<ACompetitor>();
    //    [Association(IsForeignKey = true, Storage = "_CompetitorStorage", ThisKey = "_Competitor_ID")]
    //    private ACompetitor _Competitor { get { return _CompetitorStorage.Entity; } set { _CompetitorStorage.Entity = value; } }

    //    private EntityRef<AEvent> _EventStorage = new EntityRef<AEvent>();
    //    [Association(IsForeignKey = true, Storage = "_EventStorage", ThisKey = "_Event_ID", DeleteOnNull =true)]
    //    private  AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

    //}
}