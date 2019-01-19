using ChampionshipSolutions.DM;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.SQLCommands
{
    public static class SchoolTeamsSQLCommands
    {
        [Obsolete("Please use Connection and not context",true)]
        internal static int DeleteSchoolTeam(int sch, int team, CSDB Context)
        {
            string cmd = string.Format("DELETE FROM [SchoolTeams] WHERE [School_ID] = {0} AND [Team_ID] = {1}", sch, team);

            return Context.ExecuteCommand ( cmd );
        }

        [Obsolete("Do not send SQL commands directly to the database as this breaks the caching", true)]
        public static int DeleteSchoolTeam ( int sch , int team , SQLiteConnection Connection )
        {
            string cmd = string.Format("DELETE FROM [SchoolTeams] WHERE [School_ID] = {0} AND [Team_ID] = {1}", sch, team);
            object[] obj = Connection.ExecuteQuery(cmd).FirstOrDefault();
            return 1;
        }

        internal static void InsertSchoolTeam ( SchoolTeams ST , SQLiteConnection Connection )
        {
            using ( CSDB_BASE context = new CSDB ( Connection ) )

            {
                string cmd = string.Format(@"INSERT INTO SchoolTeams (ID, School_ID, Team_ID) VALUES ({0}, {1}, {2})",
                CSDB_BASE.getNextIdentifier(typeof(SchoolTeams),context), ST.School.ID,ST.Team.ID);

                object[] obj = context.ExecuteQuery(cmd).FirstOrDefault();
            }
        }
    }

    public static class ChampionshipAthleteTeamSQLCommands
    {
        [Obsolete("Please use Connection and not context",true )]
        public static int DeleteAthleteTeamChamptionships ( int Championship_ID, int Athlete_ID , CSDB Context )
        {
            string cmd = string.Format("DELETE FROM [AthleteTeamChamptionships] WHERE [Championship_ID] = {0} AND [Athlete_ID] = {1}", Championship_ID, Athlete_ID);

            return Context.ExecuteCommand ( cmd );
        }

        [Obsolete("Do not send SQL commands directly to the database as this breaks the caching", true)]
        public static int DeleteAthleteTeamChamptionships ( int Championship_ID , int Athlete_ID , SQLiteConnection Connection )
        {
            string cmd = string.Format("DELETE FROM [AthleteTeamChamptionships] WHERE [Championship_ID] = {0} AND [Athlete_ID] = {1}", Championship_ID, Athlete_ID);
            object[] obj = Connection.ExecuteQuery(cmd).FirstOrDefault();
            return 1;
        }

    }

    public static class CustomDataSQLCommands
    {
        [Obsolete("Please use Connection and not context", true)]
        public static int CleanUpCustomData ( CSDB Context )
        {
            return Context.ExecuteCommand ( "DELETE FROM[CustomDataValues] WHERE ( [Championship_ID] IS NULL ) AND ( [Competitor_ID] IS NULL ) AND ( [Event_ID] IS NULL )" );
        }

        [Obsolete("Do not send SQL commands directly to the database as this breaks the caching", true)]
        public static int CleanUpCustomData ( SQLiteConnection Connection )
        {
            string cmd = "DELETE FROM[CustomDataValues] WHERE ( [Championship_ID] IS NULL ) AND ( [Competitor_ID] IS NULL ) AND ( [Event_ID] IS NULL )";
            object[] obj = Connection.ExecuteQuery(cmd).FirstOrDefault();
            return 1;
        }
    }

    public static class CounterCommands
    {
        public static void CountAthletes ( SQLiteConnection Connection, int Championship_ID, 
            out int Athletes, out int InTeams, out int InEvents, out int HaveResults, out int Selected )
        {
            string cmd = string.Format(
                @"SELECT 
                  COUNT(People.ID) AS Athletes,
                  COUNT(AthleteTeamChamptionships.Championship_ID) AS InTeams,
                  COUNT(Competitors.ID) AS Competitors,
                  COUNT(Results.ID) AS Results,
                  IFNULL(SUM(Competitors.SelectedForNextEvent),0) AS Selected
                FROM
                  People
                  LEFT OUTER JOIN Competitors ON (People.ID = Competitors.Athlete_ID)
                  LEFT OUTER JOIN Results ON (Competitors.ID = Results.Competitor_ID)
                  LEFT OUTER JOIN AthleteTeamChamptionships ON (People.ID = AthleteTeamChamptionships.Athlete_ID)
                WHERE
                  AthleteTeamChamptionships.Championship_ID = {0} OR 
                  AthleteTeamChamptionships.Championship_ID IS NULL", Championship_ID);

            object[] obj = Connection.ExecuteQuery(cmd).FirstOrDefault();

            Athletes    = int.Parse ( obj[0].ToString ( ) );
            InTeams     = int.Parse ( obj[1].ToString ( ) );
            InEvents    = int.Parse ( obj[2].ToString ( ) );
            HaveResults = int.Parse ( obj[3].ToString ( ) );
            Selected    = int.Parse ( obj[4].ToString ( ) );
        }

    }


}
