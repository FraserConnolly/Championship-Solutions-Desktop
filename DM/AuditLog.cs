/*
 *  Filename         : AuditLog.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 *      Based on http://stackoverflow.com/questions/20961489/how-to-create-an-audit-trail-with-entity-framework-5-and-mvc-4
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{
    public partial class AuditLog 
    {
        public AuditLog()
        {
            DState = null;
        }

        public string User { get { return _User; } set { _User = value; } }
        public string TableName { get { return _TableName; } set { _TableName = value; } }
        public int? RecordId { get { return _RecordId; } set { _RecordId = value; } }
        public string EventType { get { return _EventType; } set { _EventType = value; } }
        public DateTime EventDateUTC { get { return _EventDateUTC; } set { _EventDateUTC = value; } }

        //public AuditLogDetails[] Changes { get { return _Details.ToArray(); } }

        [Obsolete ("TO DO make this work")]
        public void AddDetail ( AuditLogDetails Detail )
        {
            //Detail.AuditLogRecord = this;
            //_Details.Add(Detail);
        }

        public override string ToString()
        {
            if (EventType != null && TableName != null && RecordId.HasValue)
                return "EventType " + EventType + " to " + TableName + " Record " + RecordId.ToString();
            else
                return base.ToString();
        }

    }

    public partial class AuditLogDetails
    {
        public AuditLogDetails ( )
        {
            DState = null;
        }

        public string ColumnName { get { return _ColumnName; } set { _ColumnName = value; } }
        public string OriginalValue { get { return _OriginalValue; } set { _OriginalValue = value; } }
        public string NewValue { get { return _NewValue; } set { _NewValue = value; } }

        //public AuditLog AuditLogRecord { get { return _AuditLogRecord; } set { _AuditLogRecord = value; } }

        public override string ToString()
        {
            if (ColumnName != null && OriginalValue != null && NewValue != null)
                return "Column " + ColumnName + " changed from " + OriginalValue + " to " + NewValue;
            else
                return base.ToString();

        }
    }


}
