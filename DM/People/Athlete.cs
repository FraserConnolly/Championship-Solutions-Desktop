/*
 *  Filename         : Athlete.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 * 
 * */

using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    public partial class AthleteTeamChamptionship
    {
        //public int ID { get { return _ID; } set { _ID = value; } }

        public Championship Championship { get { return _Championship; } set { _Championship = value; } }

        public Athlete Athlete { get { return _Athlete; } set { _Athlete = value; } }

        public Team Team { get { return _Team; } set { _Team = value; } }

        public ACompetitor PreferedEvent { get { return _PreferedEvent; } set { _PreferedEvent = value; } }
    }

    // Moved StudentAthlete.Attends to Athlete in V3-0 as School is no synonymous for club 
    // so a regular athlete may well attend a club.
    public partial class Athlete : Person
    {
        public School Attends { get { return _Attends; } set { _Attends = value; } }
       
        public int? GlobalAthleteID { get { return _GlobalAthleteID; } set { _GlobalAthleteID = value; } }

        #region Preferred Event

        // 2016-03-25 this may not work for Squads
        public AEvent getPreferedEvent(Championship Championship)
        {
            AthleteTeamChamptionship atc =
                (from t in Teams where t.Championship.Name == Championship.Name select t).FirstOrDefault();

            if (atc == null) return null;

            if (atc.PreferedEvent == null) return null;

            return atc.PreferedEvent.CompetingIn;
        }

        // 2016-03-25 this may not work for Squads
        public void setPreferedEvent(Championship Championship, ACompetitor Competitor)
        {
            AthleteTeamChamptionship atc = Teams.Where(a => a.Championship == Championship).FirstOrDefault();

            atc.PreferedEvent = Competitor;
        }

        #endregion

        #region Teams

        private AthleteTeamChamptionship[] Teams 
        {
            get { return _AthleteTeamChampionship.ToArray(); }
        }

        public Team getTeam(Championship championship)
        {
            //return (from t in Teams where t.Athlete.PrintName() == this.PrintName() && t.Championship.Name == championship.Name select t.Team).FirstOrDefault();
            return (from t in Teams where t.Championship.Name == championship.Name select t.Team).FirstOrDefault();
        }

        /// <summary>
        /// Joins this athlete to a team for a specific championship. 
        /// It can also clear the Attends field if the new team does not also have that school/club.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="championship"></param>
        public void setTeam (Team team, Championship championship)
        {
            if ( getTeam ( championship ) == team ) return ; // nothing has changed

            if ( GetCompetitors( championship ).Count ( ) > 0 ) throw new Exception ( "Can not set team after events have been entered" );

            AthleteTeamChamptionship championships = (from c in Teams where c.Championship == championship && c.Athlete == this select c).FirstOrDefault();

            if ( team == null )
            {
                // Delete the link record in AthleteTeamChampionship
                // Sadly the auto delete on Null in System.Data.Linq fails here and tries to also delete the Team record as well as the ATC link.
                // so the actual link deletion takes place in AthleteVM.cs

                if ( championship == null ) return; // there is nothing to do here

                DState.IO.Delete<AthleteTeamChamptionship> ( championship );
            }
            else
            {
                if ( championships == null ) // set Team for the first time
                {
                    championships = new AthleteTeamChamptionship() { Athlete = this, Championship = championship, Team = team };

                    DState.IO.Add<AthleteTeamChamptionship> ( championships );

                }
                else // Change team
                {
                    championships.Team.voidStorage ( );
                    championships.Team = team;
                    DState.IO.Update<AthleteTeamChamptionship> ( championships );
                }
            }

            championships.Team.voidStorage ( );
            championships.voidStorage ( );
            __AthleteTeamChampionship.refresh ( );

            if ( Attends != null )
                if (! Attends.inTeams.Contains( team ) )
                    Attends = null;

        }

        #endregion

        #region Constructors

        public Athlete() :base() {  }

        public Athlete(Person name) : base(name) {  }

        #endregion

        public bool inChampionship(Championship Championship)
        {
            return (((from t in Teams where t.Championship == Championship select t).FirstOrDefault()) != null);
        }

        public bool isOrphan()
        {
            return (Teams.Count() == 0);
        }

        // Does not include squads and counts across Championships
        public bool HasResults()
        {
            foreach ( ACompetitor competitor in _Competitors )
                if ( competitor.getResult ( ) != null ) return true;

            return false;
        }

        #region Competitors

        //public Competitor[] Competitors { get { return _Competitors.ToArray(); } }

        /// <summary>
        /// Gets all of the competitors for this athlete in this Championship.
        /// This call is a huge performance hit as it goes through every Event in the championship.
        /// </summary>
        [Obsolete]
        public ACompetitor[] Competitors
        {
            get
            {
                // _Competitors shows all competitors across all Championships, this isn't helpful.
                //return _Competitors.ToArray();

                List<ACompetitor> temp = new List<ACompetitor>();

                foreach ( AthleteTeamChamptionship t in this.Teams )
                    foreach ( AEvent Event in t.Championship.Events )
                        foreach ( ACompetitor c in Event.EnteredCompetitors )
                            if ( c.isAthlete ( this ) )
                                temp.Add ( c );

                return temp.ToArray ( );
            }
        }

        public ICollection<Squad> Squads()
        {
            List<Squad> temp = new List<Squad>();

            foreach (AthleteTeamChamptionship t in this.Teams)
            {
                foreach (AEvent Event in t.Championship.Events)
                {
                    if (Event is ISquadEvent)
                    {
                        foreach (ACompetitor c in Event.EnteredCompetitors)
                        {
                            if (c.isAthlete(this))
                                temp.Add((Squad)c);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return temp;
        }

        public ICollection<ACompetitor> AllCompetitors()
        {
            List<ACompetitor> t = new List<ACompetitor>();
            foreach ( ACompetitor c in _Competitors )
                t.Add ( c );

            foreach ( ACompetitor c in Squads ( ) )
                t.Add ( c );

            return t;
        }

        public ACompetitor getCompetitorIn(Championship Championship, AEvent Event)
        {
            return _Competitors.Where( e => e.CompetingIn == Event && e.CompetingIn.Championship == Championship ).FirstOrDefault();
        }

        // brought back in for Python Scripts
        [Obsolete ( "Not squad friendly" )]
        public List<Competitor> CompetingAs(Championship Championship)
        {
            return _Competitors.Where(e => e.forChampionship(Championship) == true && e is Competitor).Select(c => c as Competitor).ToList();
        }

        public List<Competitor> GetCompetitors ( Championship Championship )
        {
            return _Competitors.Where ( e => e.forChampionship ( Championship ) == true && e is Competitor ).Select ( c => c as Competitor ).ToList ( );
        }

        public List<AEvent> CompetitngIn(Championship Championship)
        {
            return _Competitors.Where(e => e.forChampionship(Championship) == true).Select( e => e.CompetingIn).ToList();
        }

        /// <summary>
        /// Gets all results for an athlete in a specified Championship including heat results.
        /// </summary>
        public List<AResult> getAllResults ( Championship Championship = null )
        {
            List<AResult> results = new List<AResult>();
            List<ACompetitor> competing ;//= new List<ACompetitor>();

            if ( Championship == null )
                competing = _Competitors.ToList<ACompetitor> ( );
            else
                //competing = this.CompetingAs ( Championship );
                competing = GetCompetitors ( Championship ).ToList<ACompetitor> ( );

            foreach ( Competitor c in competing )
            {
                if ( c is StudentHeatedCompetitor )
                    if ( ( (StudentHeatedCompetitor)c ).getHeatResult ( ) != null )
                        results.Add ( ( (StudentHeatedCompetitor)c ).getHeatResult ( ) );

                if ( c.getResult ( ) != null )
                    results.Add ( c.getResult ( ) );
            }
            return results;
        }

        public override bool CanBeDeleted
        {
            get
            {
                // check competitors before squads to save on processing.
                __Competitors.refresh( );
                if (_Competitors.Count() > 0) return false;
                return (Squads().Count() == 0);
            }
        }

        #endregion

        #region Notes (Power of Ten only)

        public List<PowerOfTenResult> GetPowerOfTenResultsNotes ( )
        {
            return ( from note in Notes where note.GetType ( ) == typeof ( PowerOfTenResult ) orderby note.EnteredDate select (PowerOfTenResult)note ).ToList ( );
        }

        public PowerOfTenResult CreatePowerOfTenResult ( string Championship , string Event , string Rank , string ResultValue , DateTime EventDate )
        {
            return (PowerOfTenResult)AddNote ( new PowerOfTenResult ( this , DateTime.Now , Championship , Event , Rank , ResultValue , EventDate ) );
        }

        #endregion

        /// <summary>
        /// Generates certificates for the currently selected championship
        /// </summary>
        /// <returns>File paths to where the certificates were generated</returns>
        public string [] GenerateCertificates ( )
        {
            return ChampionshipSolution.getCS( ).OpenCert( ID );
        }

        public bool isAvilableFor ( string Championship )
        {
            bool Available = false;

                IList<DeclaredAvailibilityInformation> info =
                    GetAvailibilityNotes ( ).Where ( a => a.Championship == Championship ).ToList();

                foreach ( DeclaredAvailibilityInformation i in info )
                    if ( i.Availability.ToLower ( ).Trim ( ) != "available" )
                        return false;
                    else
                        Available = true;

            return Available;
        }

    }

    public partial class StudentAthlete : Athlete
    {


    }

}
