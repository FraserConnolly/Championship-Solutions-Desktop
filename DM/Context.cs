using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if(AUDIT)
using Doddle.Linq.Audit;
#endif
using System.Data.SQLite;
using System.Threading;

namespace ChampionshipSolutions.DM
{

#if (AUDIT)
    public abstract class CSDB_BASE : Doddle.Linq.Audit.LinqToSql.AuditableDataContext , IDisposable  //: DataContext
#else
    public abstract class CSDB_BASE : DataContext, IDisposable  //: DataContext
#endif
    {
        public static Semaphore ContextSigletonLock = new Semaphore(1,1);

        public CSDB_BASE ( IDbConnection connection ) : base ( connection )
        {
            ContextSigletonLock.WaitOne ( );
#if (AUDIT)
            DefaultAuditDefinitions ( );
#endif
        }

        public new void Dispose ( )
        {
            base.Dispose ( );
            ContextSigletonLock.Release ( );
        }

        /// <summary>
        /// System.Data.Linq can not create an SQLite databases
        /// </summary>
        public new void CreateDatabase ( )
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// System.Data.Linq can not delete an SQLite databases
        /// </summary>
        public new void DeleteDatabase ( )
        {
            //throw new NotImplementedException();
        }

        public bool HasChanges ( )
        {
            try
            {
                ChangeSet cs = this.GetChangeSet();

                return ( cs.Inserts.Count + cs.Deletes.Count + cs.Updates.Count != 0 );
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine ( "Error in getting changes " + ex.Message );
                Diag.Diagnostics.LogLine ( "Error in getting changes in context" );
                Diag.Diagnostics.LogLine ( ex.Message );
                return false;
                throw;
            }
        }

        public new void SubmitChanges ( )
        {
            SubmitChangesEx ( );
        }

        private List<object> tempChanges;

        private static object writeLock = new object();

        public Exception SubmitChangesEx ( )
        {
            TextWriter tw = new StringWriter();

            this.Log = tw;

            if ( !HasChanges ( ) ) return null;

            lock ( writeLock )
            {

                ChangeSet cs = this.GetChangeSet();

                tempChanges = new List<object> ( );

                foreach ( object change in cs.Inserts )
                {
                    tempChanges.Add ( change );
                }

                foreach ( object change in cs.Updates )
                    tempChanges.Add ( change );

                foreach ( object change in cs.Deletes )
                    tempChanges.Add ( change );

                IList<object> inserst = cs.Inserts;

                foreach ( object o in inserst )
                    if ( o is IID )
                        if ( ( (IID)o ).ID == 0 )
                            // we need to get a new ID before submitting
                            ( (IID)o ).ID = getNextIdentifier ( ( (IID)o ).GetType ( ), this );

                try
                {
                    base.SubmitChanges ( );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine ( ex.Message );
                    System.Diagnostics.Debug.WriteLine ( tw.ToString ( ) );
                    Diag.Diagnostics.LogLine ( "Error in getting changes in context" );
                    Diag.Diagnostics.LogLine ( ex.Message );
                    Diag.Diagnostics.LogLine ( tw.ToString() );
                    return ex;
                }
                return null; // no exceptions occurred
            }

        }


        private static string GetTableName ( Type T )
        {
            Type t = T;
            TableAttribute ta = null;

            while ( true )
            {
                ta = (TableAttribute)Attribute.GetCustomAttribute ( t , typeof ( TableAttribute ) );

                if ( ta != null )
                    break;

                t = t.BaseType;

                if ( t == null )
                    break;
            }

            if ( ta == null )
                return string.Empty;

            return ta.Name;
        }

        public static int getNextIdentifier ( Type T , DataContext Connection)
        {

            try
            {
                string tableName = GetTableName(T);

                if ( tableName != string.Empty )
                {

                    var results = Connection.ExecuteQuery ("SELECT [NextIDInt] FROM [NextID] WHERE [Table] = '" + tableName + "'");

                    if ( results.Count ( ) == 0 ) return 0;

                    int nextID = (int)results.First()[0];

                    Connection.ExecuteQuery ( string.Format ( "UPDATE [NextID] SET [NextIDInt] = {0} WHERE [Table] = '{1}'" , nextID + 1 , tableName ) ).Count();

                    return nextID;
                }
                else
                    return 0;
            }
            catch
            {
                return 0;
            }


        }

#region AuditLogging
#if (AUDIT)
       // Added as an experiment with V3-0 2016-03-26
        // Taken from http://doddleaudit.codeplex.com/

        public Table<AuditLog> AuditLog;

        protected override void InsertAuditRecordToDatabase ( EntityAuditRecord record )
        {
            // Bodge to prevent duplicate audit records being made.
            if ( tempChanges.Contains ( record.Entity ) )
                tempChanges.Remove ( record.Entity );
            else
                return;

            AuditLog audit = new AuditLog() { ID = getNextIdentifier(typeof(AuditLog), this), EventDateUTC = DateTime.UtcNow };

            audit.EventDateUTC = DateTime.Now;

            switch ( record.Action )
            {
                case AuditAction.Insert:
                    audit.EventType = "INSERT";
                    break;
                case AuditAction.Update:
                    audit.EventType = "UPDATE";
                    break;
                case AuditAction.Delete:
                    audit.EventType = "DELETE";
                    break;
                default:
                    break;
            }

            audit.TableName = record.EntityTable;
            audit.RecordId = record.EntityTableKey;

            foreach ( ModifiedEntityProperty av in record.ModifiedProperties )
            {
                if ( av.NewValue.StartsWith ( "System." ) ) continue;
                if ( av.NewValue.StartsWith ( "ChampionshipSolutions.DM." ) ) continue;

                AuditLogDetails detail = new AuditLogDetails() { ID = getNextIdentifier(typeof(AuditLogDetails), this) };

                detail.ColumnName = av.MemberName;
                detail.OriginalValue = av.OldValue;
                detail.NewValue = av.NewValue;

                audit.AddDetail ( detail );
            }

            this.AuditLog.InsertOnSubmit ( audit );

        }

        protected override void DefaultAuditDefinitions ( )
        {


            // Tables that refer to an abstract class have to be explicitly listed for the audit to work.

            this.Championships.Audit ( );

            //this.Groups.Audit ( );

            this.Audit<Group> ( );
            this.Audit<GenderRestriction> ( );
            this.Audit<AgeRestriction> ( );
            this.Audit<DoBRestriction> ( );

            this.EventGroups.Audit ( );
            this.SchoolTeams.Audit ( );

            this.Audit<IndividualDistanceEvent> ( );
            this.Audit<IndividualTimedEvent> ( );
            this.Audit<IndividualTimedFinalEvent> ( );
            this.Audit<IndividualTimedFinalSchoolEvent> ( );
            this.Audit<IndividualTimedHeatEvent> ( );
            this.Audit<SquadDistanceEvent> ( );
            this.Audit<SquadTimedEvent> ( );

            this.Audit<ConfidentialNote> ( );
            this.Audit<PublicNote> ( );
            this.Audit<PreviousResult> ( );
            this.Audit<PowerOfTenResult> ( );

            this.Audit<Competitor> ( );
            this.Audit<StudentCompetitor> ( );
            this.Audit<StudentHeatedCompetitor> ( );
            this.Audit<Squad> ( );

            this.Audit<Person> ( );
            this.Audit<Athlete> ( );
            this.Audit<StudentAthlete> ( );

            this.Audit<CustomDataValueInt> ( );
            this.Audit<CustomDataValueString> ( );

            this.Audit<Result> ( );

            this.Schools.Audit ( );

            this.Teams.Audit ( );

            this.Standards.Audit ( );

            this.FileStore.Audit ( );

        }
#endif

#endregion
    }

    public class CSDB_Person : CSDB_BASE
    {
        public CSDB_Person ( IDbConnection connection ) : base ( connection ) { }
        public Table<Person> People;
    }

    public class CSDB_Event : CSDB_BASE
    {
        public CSDB_Event ( IDbConnection connection ) : base ( connection ) { }
        //public Table<Person> People;
        public Table<AEvent> Events;
        public Table<Group> Groups;
    }

    public class CSDB_Championship : CSDB_BASE
    {
        public CSDB_Championship ( IDbConnection connection ) : base ( connection ) { }
        public Table<Championship> Championships;
        public Table<Team> Teams;
        public Table<School> Schools;
        public Table<Group> Groups;
        public Table<Person> People;
        public Table<Template> Templates;
    }

    public class CSDB_Templates : CSDB_BASE
    {
        public CSDB_Templates ( IDbConnection connection ) : base ( connection ) { }
        public Table<Template> Templates;
    }

    public class CSDB : CSDB_BASE
    {
        public CSDB ( IDbConnection connection ) : base ( connection ) { }

        public Table<Championship> Championships { get; private set; }
        public Table<ACustomDataValue> CustomDataValues { get; private set; }
        public Table<EventGroups> EventGroups { get; private set; }
        public Table<AEvent> Events { get; private set; }
        public Table<Group> Groups { get; private set; }
        public Table<Standard> Standards { get; private set; }
        public Table<FileStorage> FileStore { get; private set; }
        public Table<ACompetitor> Competitors { get; private set; }
        public Table<AthleteTeamChamptionship> AthleteTeamChampionship { get; private set; }
        public Table<Person> People { get; private set; }
        public Table<AResult> Results { get; private set; }
        public Table<School> Schools { get; private set; }
        public Table<SchoolTeams> SchoolTeams { get; private set; }
        public Table<Team> Teams { get; private set; }
        public Table<VestActions> VestActions { get; private set; }
        //public Table<ACertificate> Certificates;
        public Table<Template> Templates { get; private set; }
        public Table<FConnFileDetail> FileDetails { get; private set; }

    }


    public static class Extensions
    {
        /// <summary>
        /// Absolutely must ask for a return value or it wont be fired.
        /// </summary>
        /// <param name="con"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IEnumerable<object[]> ExecuteQuery (
            this DbConnection con , string query )
        {
            using ( CSDB_BASE context = new CSDB ( con ) )
            {
                using ( DbCommand cmd = con.CreateCommand ( ) )
                {
                    cmd.CommandText = query;
                    con.Open ( );
                    using ( DbDataReader rdr =
                        cmd.ExecuteReader ( CommandBehavior.CloseConnection ) )
                    {
                        while ( rdr.Read ( ) )
                        {
                            object[] res = new object[rdr.FieldCount];
                            rdr.GetValues ( res );
                            yield return res;
                        }
                    }
                }
            }
        }



        public static IEnumerable<object[]> ExecuteQuery(
            this DataContext ctx, string query)
        {
            using (DbCommand cmd = ctx.Connection.CreateCommand())
            {
                cmd.CommandText = query;
                ctx.Connection.Open();
                using (DbDataReader rdr =
                    cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        object[] res = new object[rdr.FieldCount];
                        rdr.GetValues(res);
                        yield return res;
                    }
                }
            }
        }
    }

}
