//using System;
//using System.Collections.Generic;
//using System.Data.Linq.Mapping;
//using System.Data.Linq;

//namespace ChampionshipSolutions.DM
//{
//    [Table ( Name = "Certificates" )]
//    [InheritanceMapping ( Code = "CertificateInvididual" , Type = typeof ( CertificateInvididual ) , IsDefault = true )]
//    [InheritanceMapping ( Code = "CertificateScoringTeam" , Type = typeof ( CertificateScoringTeam ) )]
//    public abstract partial class ACertificate
//    {
//        // Removed from V3-0
//        //[Column(IsPrimaryKey = true, Name = "ID")]
//        //private int _ID { get; set; }

//        [Column ( IsDiscriminator = true , Name = "Discriminator" )]
//        private string _Discriminator { get; set; }

//        [Column ( Name = "Name" )]
//        private string _Name { get; set; }

//        [Column ( Name = "ShortName" )]
//        private string _ShortName { get; set; }

//        [Column(IsPrimaryKey =true, Name = "Competitor_ID")]
//        private int _Competitor_ID;

//        [Column(IsPrimaryKey = true, Name = "Event_ID")]
//        private int _Event_ID;

//        [Column(IsPrimaryKey = true, Name = "File_ID")]
//        private int _File_ID;

//        private EntityRef<AEvent> _EventStorage = new EntityRef<AEvent>();
//        [Association ( Storage = "_EventStorage" , ThisKey = "_Event_ID" , IsForeignKey = true , DeleteOnNull = true )]
//        private AEvent _Event { get { return _EventStorage.Entity; } set { _EventStorage.Entity = value; } }

//        private EntityRef<FileStorage> _FileStorage = new EntityRef<FileStorage>();
//        [Association ( Storage = "_FileStorage" , ThisKey = "_File_ID" , IsForeignKey = true , DeleteOnNull = true )]
//        private FileStorage _File { get { return _FileStorage.Entity; } set { _FileStorage.Entity = value; } }

//        private EntityRef<ACompetitor> _CompetitorStorage = new EntityRef<ACompetitor>();
//        [Association ( Storage = "_CompetitorStorage" , ThisKey = "_Competitor_ID" , IsForeignKey = true , DeleteOnNull = true )]
//        private ACompetitor _Competitor { get { return _CompetitorStorage.Entity; } set { _CompetitorStorage.Entity = value; } }

//    }
//}