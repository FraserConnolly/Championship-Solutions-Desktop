using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{

    /// <summary>
    /// Currently limited to only one squad per team
    /// </summary>
    public abstract class ASquadEvent : AEvent, ISquadEvent
    {
        #region Constructors

        public ASquadEvent() : base(null, ResultDisplayDescription.NotDeclared, null) { }

        public ASquadEvent(ASquadEvent Event, Championship Championship) : base(Event, Championship) { }

        #endregion

        public override void enterAthlete(Athlete Athlete, VestNumber Vest, bool Guest = false)
        {
            if (isEventFull())
                throw new ArgumentException(String.Format("{0} is already full with {1} competitors.", Name, getEnteredCompetitors().Count));

            if (isTeamFull(Athlete.getTeam(this.Championship)))
                throw new ArgumentException(String.Format("{0} is already full with {1} competitors.", Name, getEnteredCompetitors(Athlete.getTeam(this.Championship)).Count));

            if (EnteredCompetitors.Count(i => i.isAthlete(Athlete)) != 0)
                throw new ArgumentException("Athlete has already been entered");

            if (!isAvailable(Athlete))
                throw new ArgumentException("Athlete is not eligible for this event");

            // find if there is already a squad for this team.

            Team t = Athlete.getTeam(this.Championship);

            if (t == null) return;

            Squad comp = getSquadForTeam(t);

            if (comp == null)
            {
                comp = new Squad() { CompetingIn = this, Vest = Vest };
                this.AddCompetitor(comp);
            }

            comp.addToSquad(Athlete);

            comp.Guest = Guest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns>Will return null if this team does not have a squad</returns>
        public Squad getSquadForTeam(Team t)
        {
            foreach (Squad s in EnteredCompetitors)
            {
                if (s.Team == t)
                {
                    return s;
                }
            }
            return null;
        }

        public override void removeCompetitor(ACompetitor Competitor)
        {
            base.removeCompetitor(Competitor);
        }

        public override bool canBeEntered(Athlete athlete)
        {
            //if (isEventFull())
            //    return false;

            //if (isTeamFull(Athlete.getTeam(this.Championship)))
            //    return false;

            if (EnteredCompetitors.Count(i => i.isAthlete(athlete)) != 0)
                return false;

            if (!isAvailable(athlete))
                return false;

            // To do is the following statement actually true? 2015-06-02
            // You can never enter a single athlete into a squad event

            Team t = athlete.getTeam(this.Championship);

            // no team, no entry!
            if ( t == null ) return false;

            Squad s = getSquadForTeam(t);

            // no squad so we can make a new one.
            if (s == null) return true;

            // the squad has at least one empty space
            if (!s.isSquadFull()) return true;

            return false;
        }


        #region Results

        protected override AResult addResult(int Rank, ACompetitor Competitor, ResultValue resultValue, string VestNumber = "")
        {
            AResult newResult;

            // Check that this competitor hasn't already got a result
            if (Competitor != null)
                if (hasResultFor(Competitor))
                    throw new ArgumentException("This competitor already has a result in this race!");

            if (Rank == 0)
                Rank = getNextResultRank();

            // removes any placeholder for this rank
            removePlaceholder(Rank);

            if (!isRankAvailable(Rank))
#if ( ReorderResults )
                makeSpaceForResult(Rank);
#else
                throw new ArgumentException ( string.Format ( "Rank {0} has already been used" , Rank ) );
#endif

            // No competitor or value
            if ( Competitor == null && !resultValue.HasValue())
            {
                //newResult = new Result(ResultTypeDescription.Placeholder) { Event = this, Rank = Rank };
                newResult = new Result() { Event = this, Rank = Rank, VestNumber = DM.VestNumber.MakeFromString(VestNumber) };
                AddResult(newResult);
                return newResult;
            }

            // No competitor but does have a value
            if (Competitor == null && resultValue.HasValue())
            {
                //newResult = new Result(ResultTypeDescription.ValuePlaceholder) { Event = this, Rank = Rank, Value = resultValue };
                newResult = new Result() { Event = this, Rank = Rank, Value = resultValue, VestNumber = DM.VestNumber.MakeFromString(VestNumber) };
                AddResult(newResult);
                return newResult;
            }

            // Competitor but no result value
            if (Competitor != null && !resultValue.HasValue())
            {
                //newResult = new Result(ResultTypeDescription.Competative) { Event = this, Rank = Rank, Competitor = Competitor };
                newResult = new Result() { Event = this, Rank = Rank, Competitor = Competitor };
                AddResult(newResult);
                return newResult;
            }

            // Competitor and result value
            if (Competitor != null && resultValue.HasValue())
            {
                //newResult = new Result(ResultTypeDescription.CompetativeWithValue) { Event = this, Competitor = Competitor, Rank = Rank, Value = resultValue };
                newResult = new Result() { Event = this, Competitor = Competitor, Rank = Rank, Value = resultValue };
                AddResult(newResult);
                return newResult;
            }

            throw new ArgumentException("Unknown exception when adding a result");
        }

        #endregion

        #region Certificates

        internal override List<CertificateData> buildCertificateData(ACompetitor Competitor, string CertificateType, int counter, ScoringTeam ScoringTeam = null)
        {
            List<CertificateData> temp = new List<CertificateData>();

            if (Competitor == null)
                return temp;

            if (Competitor.getResult() == null)
                return temp;

            if (!Competitor.getResult().Rank.HasValue)
                // this competitor was not ranked.
                return temp;

            Squad squad = (Squad)Competitor;

            foreach (Athlete a in squad.members)
            {
                if (a == null) continue;

                string SchStr = string.Empty;
                if (a is StudentAthlete)
                    SchStr = ((StudentAthlete)a).Attends.Name;

                // modified 2015-06-07 to add PrintName 

                CertificateData CD = new CertificateData()
                {
                    Championship = this.Championship.ToString(),
                    TeamName = Competitor.getTeam().Name,
                    CompetitorsName = a.PrintName(),
                    EventName = this.Name,
                    SchoolName = SchStr,
                    CertifiacteType = CertificateType,
                    Competitor = Competitor,
                    RankCounter = counter
                };

                if (this.customFieldExists("PrintedName"))
                {
                    CD.EventName = this.getValue("PrintedName").ToString();
                }

                switch (CertificateType)
                {
                    case "TopIndividuals":

                        CD.CertificateName = "Top Individuals";

                        // CompetitorsName updated for SW 2014-15
                        //CD.CompetitorsName = String.Format("{0} - {1}", Competitor.getName(), Competitor.getTeam().Name);
                        CD.CompetitorsName = String.Format("{0}", a.PrintName());

                        // SchoolName field updated for SW 2014-15
                        CD.SchoolName = String.Format("{0} - {1}", Competitor.getTeam().Name, SchStr);

                        //CD.Rank = AResult.IntToString( Competitor.getResult().Rank.Value );
                        CD.Rank = CD.Competitor.Result.printRankAndResult();
                        break;
                    case "TopLowerYearGroupIndividuals":
                        // Not used in SW 2014-15
                        CD.CertificateName = "Top Lower Year Group Individuals";
                        CD.CompetitorsName = String.Format("{0} - {1} - ({2})", a.PrintName(), Competitor.getTeam().Name, Competitor.getResult().Rank.Value.ToString());

                        CD.Rank = string.Format("{0} Year {1}", AResult.IntToString(counter), ((StudentCompetitor)Competitor).YearGroup.ToString());

                        break;
                    case "Team":
                        // updated for SW 2014-15
                        //CD.CompetitorsName = String.Format("{0} - {1} - ({2})", Competitor.getTeam().Name, Competitor.getName(), Competitor.Result.Rank.Value.ToString());
                        CD.CompetitorsName = String.Format("{1} - ({2})", Competitor.getTeam().Name, Competitor.getName(), Competitor.getResult().Rank.Value.ToString());
                        // SchoolName field updated for SW 2014-15
                        CD.SchoolName = String.Format("{0} - {1}", Competitor.getTeam().Name, SchStr);

                        if (ScoringTeam != null)
                        {
                            // updated for SW 2014-15
                            CD.Rank = string.Format("{0} Team", AResult.IntToString(ScoringTeam.Rank), ScoringTeam.ScoringTeamName);
                            //CD.Rank = string.Format("{0} {1} Team", AResult.IntToString(ScoringTeam.Rank), ScoringTeam.ScoringTeamName);
                            CD.CertificateName = CD.Rank;
                        }

                        CD.CertificateName = CD.Rank;

                        break;
                    default:
                        break;
                }

                temp.Add(CD);
            } // for each member
            return temp;
        }

        #endregion


        public override void removeAthlete(Athlete Athlete)
        {
            if (Athlete == null) return;

            Squad s = (Squad)EnteredCompetitors.Where(e => e.isAthlete(Athlete)).FirstOrDefault();

            if ( s != null )
            {
                if (s.Competitor1 != null)
                    if (s.Competitor1 == Athlete)
                        s.Competitor1 = null;

                if (s.Competitor2!= null)
                    if (s.Competitor2 == Athlete)
                        s.Competitor2 = null;

                if (s.Competitor3 != null)
                    if (s.Competitor3 == Athlete)
                        s.Competitor3 = null;

                if (s.Competitor4 != null)
                    if (s.Competitor4 == Athlete)
                        s.Competitor4 = null;
            }

            if (s.isSquadEmpty())
            {
                this.removeCompetitor(s);
            }
        }

        public override string getEventType ( )
        {
            return "Abstract Squad Event";
        }

    }

    public class SquadTimedEvent : ASquadEvent, IEnterTimedResults
    {
        public SquadTimedEvent ( ) : base ( ) { }

        public SquadTimedEvent ( SquadTimedEvent Event , Championship Championship ) : base ( Event , Championship ) { }

        #region Results

        #region Add Result Helper for timed events

        public AResult AddResult ( int Rank , ACompetitor Competitor , TimeSpan timedResultValue )
        {
            return addResult ( Rank , Competitor , new ResultValue ( this.ResultsDisplayDescription , timedResultValue ) );
        }

        public AResult AddResult ( int Rank , TimeSpan timedResultValue )
        {
            return addResult ( Rank , null , new ResultValue ( this.ResultsDisplayDescription , timedResultValue ) );
        }

        public AResult AddResult ( ACompetitor Competitor , TimeSpan timedResultValue )
        {
            return addResult ( 0 , Competitor , new ResultValue ( this.ResultsDisplayDescription , timedResultValue ) );
        }

        public AResult AddResult ( VestNumber vest , TimeSpan timedResultValue )
        {
            return addResult ( 0 , AEvent.getCompetitor ( this , vest ) , new ResultValue ( this.ResultsDisplayDescription , timedResultValue ) );
        }

        public AResult AddResult ( int Rank , VestNumber vest , TimeSpan timedResultValue )
        {
            return addResult ( Rank , AEvent.getCompetitor ( this , vest ) , new ResultValue ( this.ResultsDisplayDescription , timedResultValue ) );
        }


        #endregion

        #endregion

        public override string getEventType ( )
        {
            return "Squad Timed Event";
        }

    }

    public class SquadDistanceEvent : ASquadEvent, IEnterDistanceResults 
    {
        public SquadDistanceEvent() : base() { }
        public SquadDistanceEvent(SquadDistanceEvent Event, Championship Championship) : base(Event, Championship) { }

        #region Results

        #region Add Result Helper for distance events


        public AResult AddResult(int Rank, ACompetitor Competitor, decimal distanceResultValue)
        {
            return addResult(Rank, Competitor, new ResultValue(this.ResultsDisplayDescription, distanceResultValue));
        }

        public AResult AddResult(int Rank, decimal distanceResultValue)
        {
            return addResult(Rank, null, new ResultValue(this.ResultsDisplayDescription, distanceResultValue));
        }

        public AResult AddResult(ACompetitor Competitor, decimal distanceResultValue)
        {
            return addResult(0, Competitor, new ResultValue(this.ResultsDisplayDescription, distanceResultValue));
        }

        public AResult AddResult(VestNumber vest, decimal distanceResultValue)
        {
            return addResult(0, AEvent.getCompetitor(this, vest), new ResultValue(this.ResultsDisplayDescription, distanceResultValue));
        }

        public AResult AddResult(int Rank, VestNumber vest, decimal distanceResultValue)
        {
            return addResult(Rank, AEvent.getCompetitor(this, vest), new ResultValue(this.ResultsDisplayDescription, distanceResultValue));
        }


        #endregion

        #endregion

        public override string getEventType ( )
        {
            return "Squad Distance Event";
        }
    }

}
