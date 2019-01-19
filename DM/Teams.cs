using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.SQLite;

namespace ChampionshipSolutions.DM
{
    /// <summary>
    /// Teams are usually geographical areas
    /// </summary>
    public partial class Team : IIdentity
    {
        //public int ID { get { return _ID; } set { _ID = value; } }

        #region Names

        public string Name { get { return _Name ?? ""; } set { _Name = value; } }

        /// <summary>
        /// Get or Set the short name of this team.
        /// By default this is the first 4 characters without any spaces.
        /// </summary>
        public string ShortName
        {
            get
            {
                if ( _ShortName != null )
                    return _ShortName;

                if ( Name == null )
                    return null;

                if ( Name.Length > 4 )
                    return Name.Replace ( " " , string.Empty ).Substring ( 0 , 4 ).Trim ( );
                else
                    return Name.Trim ( );
            }
            set
            {
                _ShortName = value;
            }
        }

        #endregion

        public Championship Championship { get { return _Championship; } set
            {
                _Championship = value;
                if ( value == null )
                {
                    //_Championship_ID = 0;
                }
                else
                {
                    _Championship_ID = Championship.ID;
                }
            }
        }

        public Team() { DState = null; }

        public Team(string TeamName) : this()
        {
            this.Name = TeamName;
        }

        public Team(string TeamName, Championship Championship) : this ( TeamName )
        {
            this.Championship = Championship;
        }

        private AthleteTeamChamptionship[] Athletes { get { return _Athletes.ToArray(); } }

        /// <summary>
        /// Bodge added in with SQLite
        /// </summary>
        [Obsolete("Hopefully isn't needed.",true)]
        internal void addAthlete(AthleteTeamChamptionship atc)
        {
            //!**! I don't think this is needed anymore
            //_Athletes.Add(atc);
        }

        public virtual List<Athlete> getAllAthletes()
        {
            return (from a in Athletes where a.Team == this && a.Championship == this.Championship select a.Athlete).ToList<Athlete>();
        }

        private SchoolTeams[] Schools
        {
            get { return _Schools.ToArray(); }
        }
        
        public virtual IEnumerable<School> HasSchools
        {
            get { return (from sch in Schools select sch.School); }
        }

        public void AddSchool(School school, DataAccess.Database IO)
        {
            SchoolTeams st = new SchoolTeams() { School = school, Team = this };

            //SQLCommands.SchoolTeamsSQLCommands.InsertSchoolTeam ( st , IO.Connection );

            DState.IO.Add<SchoolTeams>( st );

            this.voidStorage( );
            school.voidStorage ( );
            this.__Schools.refresh ( );
            
        }

        public void RemoveSchool (School school, SQLiteConnection Connection )
        {
            SchoolTeams st = (from sch in Schools where sch.School == school select sch).FirstOrDefault();
            //SQLCommands.SchoolTeamsSQLCommands.DeleteSchoolTeam ( school.ID, this.ID , Connection );

            DState.IO.Delete<SchoolTeams> ( st );

            school.voidStorage ( );
            this.__Schools.refresh ( );
        }

        public override string ToString()
        {
            return Name.ToString();
        }

        public bool CanDelete ( )
        {
            if ( Championship == null ) return true;

            if ( Championship.isLocked ( ) ) return false;

            // 2016-05-29 optimised by not using getAllAthletes which instantiates each athlete.
            return Athletes.Count ( ) == 0;
            //return ( getAllAthletes ( ).Count == 0 );
        }

        public override bool Equals ( object obj )
        {
            try
            {
                return ( (Team)obj ).ID == this.ID;
            }
            catch
            {
                return base.Equals ( obj );
            }
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public static bool operator == ( Team x , Team y )
        {
            if ( ( (object)x ) == null && ( (object)y ) == null ) return true;
            if ( ( (object)x ) == null ) return false;
            if ( ( (object)y ) == null ) return false;

            return x.ID == y.ID;
        }

        public static bool operator != ( Team x , Team y )
        {
            return !( x == y );
        }


    }

    public class ChampionshipTeamResult
    {
        public string ScoringTeamName { get; private set; }
        public int Rank { get; protected set; }
        public virtual Team Team { get; set; }
        public int Points { get; protected set; }
        protected int teamSize = -1;
        public string Description { get; set; }

        protected List<int> positions = new List<int>();

        public ChampionshipTeamResult(string ScoringTeamName)
        {
            this.ScoringTeamName = ScoringTeamName;
        }

        public int sumOfPositions
        {
            get
            {
                int sum = 0;

                if (teamSize==-1)
                {
                    foreach (int i in positions)
                        sum += i;
                    return sum;
                }

                if (positions.Count >= teamSize)
                {
                    positions.Sort();
                    foreach (int i in positions.Take(teamSize))
                    {
                        sum += i;
                    }
                }
                return sum;
            }
        }

        public void addPosition(int position)
        {
            if (position > 0)
                positions.Add(position);
        }

        public virtual string printPoints()
        {
            if (Points == 0)
                return string.Empty;
            else
                return Points.ToString();
        }

        public bool hasPosition ( int Rank )
        {
            return ( positions.Contains ( Rank ) );
        }

        public virtual int? orderableRank()
        {
            if (Rank == 0)
                return int.MaxValue;
            else
                return Rank;
        }

        public virtual string printSumOfPositions()
        {
            if (sumOfPositions == 0)
                return string.Empty;
            else
                return sumOfPositions.ToString();
        }

        /// <summary>
        /// Assigns the rank to each of the teams based on the sum of the positions.
        /// Do not mix A and B teams.
        /// </summary>
        /// <param name="teams">You must always pass all teams in a championship to this parameter</param>
        public static void assignPointsAndRanks(ChampionshipTeamResult[] ChampionshipTeams, ScoringTeam[] teams)
        {
            if (teams == null)
                return;

            // For each of the Championship teams loop through each of the scoring teams and sum their points
            foreach (ChampionshipTeamResult CTR in ChampionshipTeams)
            {
                foreach (ScoringTeam sc in teams.Where(t=>t.Team == CTR.Team))// teams.OrderBy(f => f.sumOfPositions))
                {
                    if (sc.Points == 0)
                        continue;
                    CTR.Points += sc.Points;
                    CTR.addPosition(sc.sumOfPositions);
                }
            }

            int rankCounter = 1;

            // Set the rank value of the championship teams
            foreach (ChampionshipTeamResult  CTR in ChampionshipTeams.OrderByDescending(c=>c.Points))
                CTR.Rank = rankCounter++;

        }
    }

    public class ScoringTeam : ChampionshipTeamResult
    {
        public ScoringTeam(string ScoringTeamName, int TeamSize):base(ScoringTeamName)
        {
            if ( TeamSize <= 0 )
                throw new ArgumentOutOfRangeException("TeamSize", "TeamSize must be a positive integer");
            
            this.teamSize = TeamSize;
        }

        public void setTeamSize(int teamSize)
        {
            this.teamSize = teamSize;
        }

        public string printPositions()
        {
            if (positions.Count == 0)
                return string.Empty;

            string temp = "";
            positions.Sort();
            foreach (int i in positions)
            {
                temp += i.ToString() + ", ";
            }

            return temp.Remove(temp.Length - 2);
        }

        public string Positions { get { return printPositions(); } }

        /// <summary>
        /// Returns the position of the last runner in the team.
        /// </summary>
        /// <returns>Will return null if the team is not full.</returns>
        public int? lastScoringPosition()
        {
            if (sumOfPositions < 1)
                return null;

            if (teamSize != -1)
            {
                return positions.Take(teamSize).Last();
            }
            else
            {
                return positions.Last();
            }
        }

        /// <summary>
        /// Assigns the rank to each of the teams based on the sum of the positions
        /// </summary>
        /// <param name="teams">You must always pass all teams in a championship to this parameter</param>
        public static void assignPointsAndRanks(ScoringTeam[] teams)
        {
            if (teams == null) return;

            if (teams.Count() == 0) return;

            //// bodge to tell the difference between scoring for XC and TF
            if (teams[0].Team.Championship.Name.Contains("XC"))
            {
                // with cross country we want the team to be exactly the specified team size, if it is too small return nothing


            int MaximumPoints = teams.Count();

            if (detectTie(teams))
            {
                // There is a tie that needs to be broken.


                //for (ScoringTeam[] tiedTeams = getTiedTeams(teams); tiedTeams != null; tiedTeams = getTiedTeams(teams))
                //{
                    
                //}

            }
            //else
            //{

                foreach (ScoringTeam sc in teams.OrderBy(f => f.sumOfPositions).ThenBy(f => f.lastScoringPosition()))
                {
                    if (sc.sumOfPositions == 0)
                        continue;
                    sc.Points = MaximumPoints;
                    sc.Rank = (teams.Count() - MaximumPoints) + 1;
                    MaximumPoints--;
                }

            //}
            }
            else
            {
                int MaximumPoints = teams.Count();

                // with Track and field we want the team to be up to the specified team size, if it is smaller then that is fine
                foreach (ScoringTeam sc in teams.OrderBy(f => f.sumOfPositions).ThenBy(f => f.lastScoringPosition()))
                {
                    if (sc.sumOfPositions == 0)
                        continue;

                    int p = 0;

                    foreach (int position in sc.positions)
                    {
                        // 7 should be Event.TeamASize + 1
                        p += (7 - position);
                    }

                    sc.Points = p;
                }

                foreach (ScoringTeam sc in teams.OrderByDescending(f => f.Points))
                {
                    if (sc.sumOfPositions == 0) continue;
                    sc.Rank = (teams.Count() - MaximumPoints) + 1;
                    MaximumPoints--;
                }

            }

            }

        private static bool detectTie(ScoringTeam[] teams)
        {
            return (teams.Where(f => f.sumOfPositions != 0).Select(f => f.sumOfPositions).Count() != teams.Where(f => f.sumOfPositions != 0).Select(f => f.sumOfPositions).Distinct().Count());
        }

        private static ScoringTeam[] getTiedTeams(ScoringTeam[] teams)
        {
            ScoringTeam[] tiedTeams = null;

            foreach (ScoringTeam sc in teams.OrderBy(f => f.sumOfPositions))
            {
                if (teams.Where(f => f.sumOfPositions == sc.sumOfPositions).Count() > 0)
                {
                    tiedTeams = teams.Where(f => f.sumOfPositions == sc.sumOfPositions).ToArray();
                    break;
                }
            }

            return tiedTeams;
        }
    }
}
