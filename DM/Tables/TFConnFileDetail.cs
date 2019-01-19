using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChampionshipSolutions.DM.FConnFileHelper;

namespace ChampionshipSolutions.DM
{

    [Table(Name = "FileDetails")]
    public partial class FConnFileDetail
    {

        [Column(Name = "ID", IsPrimaryKey =true)]
        public int ID { get { return 1; } set { } }

        [Column(Name = "FormatCode")]
        public int _FormatCode { get; set; }

        [Column(Name = "FileState")]
        public int _State { get; set; }

        [Column (Name= "MajorFileVersion")]
        public int MajorFileVersion { get; set; }

        [Column(Name = "MinorFileVersion")]
        public int MinorFileVersion { get; set; }

        [Column(CanBeNull = true, Name="CreatedBy")]
        public string CreatedBy { get; set; }

        //[Column (CanBeNull =true, Name = "CreatedOn")]
        //public DateTime? CreatedOn { get; set; }

        public FConnFileFormat FormatCode { get { return ResolveFileFormat((byte)_FormatCode); } }

        public FConnFileState State { get { return ResolveFileState(FormatCode, (byte)_State); } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            // do nothing as this class has no relationships
        }

    }
}
