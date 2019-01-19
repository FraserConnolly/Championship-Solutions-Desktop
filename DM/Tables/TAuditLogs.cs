using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "AuditLogs")]
    public partial class AuditLog : IID
    {
        public DataState DState { get; set; }

        [Column(IsPrimaryKey = true, Name = "ID")]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column(DbType = "DateTime", Name = "EventDateUTC")]
        internal DateTime _EventDateUTC { get; set; }

        [Column(Name = "User")]
        internal string _User { get; set; }

        [Column(Name = "EventType")]
        internal string _EventType { get; set; }

        [Column(Name = "TableName")]
        internal string _TableName { get; set; }

        [Column(CanBeNull = true, Name = "RecordId")]
        internal int? _RecordId { get; set; }

        //private EntitySet<AuditLogDetails> _AuditLogDetailsStorage = new EntitySet<AuditLogDetails>();
        //[Association(Storage = "_AuditLogDetailsStorage", OtherKey = "_AuditLog_ID")]
        //internal EntitySet<AuditLogDetails> _Details { get { return _AuditLogDetailsStorage; } set { _AuditLogDetailsStorage.Assign(value); } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            // to do - Audit log relationship
            // do nothing has relationship has been disabled during development 
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<AuditLog> ( this );
        }
    }

    [Table(Name = "AuditLogDetails")]
    public partial class AuditLogDetails : IID
    {

        public DataState DState { get; set; }

        [Column(IsPrimaryKey = true, Name = "ID")]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column(Name = "ColumnName")]
        internal string _ColumnName { get; set; }

        [Column(Name = "OriginalValue")]
        internal string _OriginalValue { get; set; }
        
        [Column(Name = "NewValue")]
        internal string _NewValue { get; set; }

        [Column(Name = "AuditLogRecord_AuditLogId")]
        internal int _AuditLog_ID { get; set; }

        //private EntityRef<AuditLog> _AuditLogStorage = new EntityRef<AuditLog>();
        //[Association(IsForeignKey =true, DeleteOnNull =true, Storage = "_AuditLogStorage", ThisKey = "_AuditLog_ID")]
        //internal AuditLog _AuditLogRecord { get { return _AuditLogStorage.Entity; } set { _AuditLogStorage.Entity = value; } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            // to do - Audit log relationship
            // do nothing has relationship has been disabled during development 
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<AuditLogDetails> ( this );
        }
    }

}
