using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{
    public abstract class AIndividualEvent : AEvent, IIndividualEvent
    {

        #region Constructor

        public AIndividualEvent() : base(null, ResultDisplayDescription.NotDeclared, null) { }

        public AIndividualEvent(string Name, ResultDisplayDescription ResultType, Championship Championship) : base(Name, ResultType, Championship) { }

        public AIndividualEvent(AIndividualEvent Event, Championship Championship)
            : base(Event, Championship)
        { }

        #endregion

        #region Competitors

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

            Competitor comp = new StudentCompetitor(Athlete, Vest, this);

            comp.Guest = Guest;

            //EnteredCompetitors.Add(comp);
            this.AddCompetitor(comp);

            Athlete.voidStorage ( );
        }

        public override bool canBeEntered(Athlete Athlete)
        {
            if (isEventFull())
                return false;

            if (isTeamFull(Athlete.getTeam(this.Championship)))
                return false;

            if (EnteredCompetitors.Count(i => i.isAthlete(Athlete)) != 0)
                return false;

            if (!isAvailable(Athlete))
                return false;

            return true;
        }

        public override void removeAthlete(Athlete Athlete)
        {
            if (Athlete == null) return;

            ACompetitor c = EnteredCompetitors.Where(e => e.isAthlete(Athlete)).FirstOrDefault();

            if ( c != null )
                this.removeCompetitor(c);
        }

        #endregion

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
                throw new ArgumentException(string.Format("Rank {0} has already been used", Rank ));
#endif

            // No competitor or value
            if (Competitor == null && !resultValue.HasValue())
            {
                //newResult = new Result(ResultTypeDescription.Placeholder) { Event = this, Rank = Rank };
                newResult = new Result() { Event = this, Rank = Rank, VestNumber = DM.VestNumber.MakeFromString(VestNumber) };
                //Results.Add(newResult);
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

        public override string getEventType ( )
        {
            return "Abstract Individual Event";
        }


    }

    public partial class IndividualTimedEvent : AIndividualEvent, IEnterTimedResults, ILaneAssignedEvent
    {

#region Constructor

        public IndividualTimedEvent() : base(null, ResultDisplayDescription.NotDeclared, null) { }

        public IndividualTimedEvent(string Name, ResultDisplayDescription ResultType, Championship Championship) : base(Name, ResultType, Championship) { }

        public IndividualTimedEvent(IndividualTimedEvent Event, Championship Championship) : base (Event, Championship)
        { }

#endregion

#region Results

#region Add Result Helper for timed events

        public AResult AddResult(int Rank, ACompetitor Competitor, TimeSpan timedResultValue)
        {
            return addResult(Rank, Competitor, new ResultValue(this.ResultsDisplayDescription, timedResultValue));
        }

        public AResult AddResult(int Rank, TimeSpan timedResultValue)
        {
            return addResult(Rank, null, new ResultValue(this.ResultsDisplayDescription, timedResultValue));
        }

        public AResult AddResult(ACompetitor Competitor, TimeSpan timedResultValue)
        {
            return addResult(0, Competitor, new ResultValue(this.ResultsDisplayDescription, timedResultValue));
        }

        public AResult AddResult(VestNumber vest, TimeSpan timedResultValue)
        {
            return addResult(0, AEvent.getCompetitor(this, vest), new ResultValue(this.ResultsDisplayDescription, timedResultValue));
        }

        public AResult AddResult(int Rank, VestNumber vest, TimeSpan timedResultValue)
        {
            return addResult(Rank, AEvent.getCompetitor(this, vest), new ResultValue(this.ResultsDisplayDescription, timedResultValue));
        }


#endregion

#endregion

#region Lanes

        public virtual bool hasLaneAssignementInformation ( )
        {
            // After discussions with Tim Whiting on 2018-06-02 this function has been deprecated.
            // It will now always return false.
            // Lane draw was partially removed around 2016 as it was too difficult to achieve on the day.
            // this old remanent caused issues when creating event cards in 2018 as some events
            // still had this flag in custom data.
            // return customFieldExists("HasLanes");
            return false;
        }

        /// <summary>
        /// Checks to see if any competitor has not been assigned a lane.
        /// </summary>
        /// <returns>True if at least one competitor doesn't have a lane.</returns>
        [Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        public virtual bool requiresLaneUpdate ( )
        {
            // do not have to update an empty event
            if (EnteredCompetitors.Count() == 0) return false;

            // do not want to update lanes on an event which has already got results
            if (AllResults().Count > 0) return false;

            foreach (ILanedCompetitor c in getEnteredCompetitors() ) // EnteredCompetitors)
                if (!c.hasLaneNumber())
                    return true;
         
            return false;
        }

        /// <summary>
        /// Will remove the LaneNumber value from all entered competitors.
        /// </summary>
        public virtual void clearLanes()
        {
            foreach (ILanedCompetitor c in EnteredCompetitors)
                c.setLaneNumber(0);
        }
        

        /// <summary>
        /// Allocates a lane for each of the entered competitors if they don't already have one.
        /// </summary>
        [Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        public virtual void updateLanes ()
        {
            if (!requiresLaneUpdate()) return;
            
            foreach (ILanedCompetitor c in EnteredCompetitors)
            {
                // this competitor already has a lane number so ignore it.
                if (c.hasLaneNumber()) continue;

                // get the integer vest number
                int vest;
                if (c.Vest.tryIntVest(out vest))
                    c.setLaneNumber(laneForVest(vest));
                else
                // we can only process integer vest numbers
                throw new ArgumentException("Only integer vest numbers can be assigned a lane");
            }

            if (requiresLaneUpdate())
                throw new ArgumentException("Failed to allocate a lane to at least one competitor");
        }

        /// <summary>
        /// Looks for the lane that a vest number should run in for this event.
        /// </summary>
        /// <param name="Vest"></param>
        /// <returns>0 if no lane can be found.</returns>
        [Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        public int laneForVest(int Vest)
        {
            if (Vest == 0) throw new ArgumentException("Vest can not be 0");

            if (buildLaneDraw(this).Contains(Vest))
            {
                int counter = 0;
                foreach (int vest in buildLaneDraw(this))
                {
                    if (vest == Vest)
                    {
                        if (isLaneFree(counter))
                            return counter;
                        else
                            // the lane this vest is supposed to use has already been taken to we'll get a new one.
                            return getEmptyLane();
                    }
                    counter++;
                }
            }

            // Failed to find a lane for this vest so we'll allocate it a free one.
            return getEmptyLane();
        }

        /// <summary>
        /// Checks each of the competitors to see if one of them is already in this lane.
        /// </summary>
        /// <param name="Lane"></param>
        /// <returns>True if lane is free.</returns>
        public bool isLaneFree(int Lane)
        {
            return ! assignedLanes(this).ContainsKey(Lane);
        }

        /// <summary>
        /// Will try to find an empty lane that doesn't have a vest allocated to it.
        /// If a lane can't be found it will offer a lane that is designated for another vest.
        /// If there are no free lanes at all then an error will be thrown.
        /// </summary>
        /// <returns></returns>
        public int getEmptyLane()
        {
            // how many lanes do me have
            int MaxLanes = this.EventRanges.Lanes;

            // get the lane draw - 2016-06-17 functionality pulled from application
            //int[] LaneDraw = buildLaneDraw(this);

            // get lane usage
            Dictionary<int, ILanedCompetitor> usedLanes = assignedLanes(this);
            int countUsedLanes = usedLanes.Count;

            if (MaxLanes <= countUsedLanes)
                // there are no more free lanes
                throw new ArgumentException("There are no more free lanes");

            //List<int> freeButReserved = new List<int>();

            for (int i = 1; i < EventRanges.Lanes + 1; i++)
            {
                if (isLaneFree(i))
                    //if (LaneDraw[i] == 0)
                        // i has not been allocated nor is it reserved for a vest
                        return i;
                    //else
                        //freeButReserved.Add(i);
            }

            //if (freeButReserved.Count >= 1)
                //return freeButReserved.First();
                
            // it shouldn't be possible to reach this exception.
            throw new ArgumentException("There are no more free lanes");
        }



        /// <summary>
        /// Used to find empty lanes when generating reports
        /// </summary>
        /// <returns>
        /// Returns an array of empty but assigned lanes.
        /// </returns>
        [Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        public int[] emptyAssingedLane ( )
        {
            int[] lanes = buildLaneDraw(this);
            List<int> emptyLanes = new List<int>();

            for (int i = 0; i < lanes.Count(); i++)
            {
                if (lanes[i] != 0)
                {
                    // we have assignment information for this lane - Check if the lane is empty.
                    if (isLaneFree(i))
                        emptyLanes.Add(i);
                }
            }

            // there are no more assigned empty lanes
            return emptyLanes.ToArray();
        }

        public int[] emptyLanes ( )
        {
            List<int> emptyLanes = new List<int>();

            for ( int i = 1 ; i < Lanes + 1 ; i++ )
                // we have assignment information for this lane - Check if the lane is empty.
                if ( isLaneFree ( i ) )
                    emptyLanes.Add ( i );

            // there are no more assigned empty lanes
            return emptyLanes.ToArray ( );
        }

        [Obsolete ("Lane draw has been pulled.",true)]
        protected static int[] buildLaneDraw(ILaneAssignedEvent Event)
        {
            int[] lanes = new int[Event.EventRanges.Lanes + 1];

            for ( int i = 1; i < Event.EventRanges.Lanes + 1; i++ )
            {
                if ( Event.customFieldExists("Lane" + i))
                    lanes[i] = (int)Event.getValue("Lane" + i);
            }

            return lanes;
        }

        protected static Dictionary<int, ILanedCompetitor> assignedLanes(ILaneAssignedEvent Event)
        {
            Dictionary<int, ILanedCompetitor> lanes = new Dictionary<int, ILanedCompetitor>();

            if (Event is IHeatEvent)
            {
                // For heated events
                foreach (ILanedHeatedCompetitor c in Event.getEnteredCompetitors())
                {
                    // we don't care about competitors who don't have numbers
                    if (!c.hasHeatLaneNumber () ) continue;

                    lanes.Add(c.HeatLaneNumber, c);
                }
            }
            else
            {
                // For final and single events
                foreach (ILanedCompetitor c in Event.getEnteredCompetitors())
                {
                    // we don't care about competitors who don't have numbers
                    if (!c.hasLaneNumber()) continue;

                    lanes.Add(c.LaneNumber, c);
                }
            }
             return lanes;
        }

#endregion

        public override string getEventType ( )
        {
            return "Individual Timed Event";
        }


    }

    public partial class IndividualTimedSchoolEvent : IndividualTimedEvent, ISchoolResultsEvent
    {

#region Constructor

        public IndividualTimedSchoolEvent() : base(null, ResultDisplayDescription.NotDeclared, null) { }

        public IndividualTimedSchoolEvent(string Name, ResultDisplayDescription ResultType, Championship Championship) : base(Name, ResultType, Championship) { }

        public IndividualTimedSchoolEvent(IndividualTimedSchoolEvent Event, Championship Championship)
            : base(Event, Championship)
        {
            this.LowerYearGroup = Event.LowerYearGroup;
        }

#endregion

#region Competitors

        public override void enterAthlete(Athlete Athlete, VestNumber Vest, bool Guest = false)
        {
            if (isEventFull())
                throw new ArgumentException(String.Format("{0} is already full with {1} competitors.", Name, getEnteredCompetitors().Count));

            if (isTeamFull(Athlete.getTeam(this.Championship)))
                throw new ArgumentException(String.Format("{0} is already full with {1} competitors.", Name, getEnteredCompetitors(Athlete.getTeam(this.Championship)).Count));

            if (EnteredCompetitors.Count(i => i.isAthlete(Athlete)) != 0)
                throw new ArgumentException("Athlete has already been entered");

            StudentCompetitor comp = new StudentCompetitor(Athlete, Vest, this);

            comp.Guest = Guest;

            this.AddCompetitor(comp);

            Athlete.voidStorage ( );
        }

#endregion

        public int LowerYearGroup { get { return _LowerYearGroup; } set { _LowerYearGroup = value; } }

        // static helper
        public static List<AResult> TopLowerYearGroup(ISchoolResultsEvent Event)
        {
            if (Event.LowerYearGroup == 0)
                return new List<AResult>();
            return Event.AllResults().Where(f => f.printParameter("YearGroup") == ((ISchoolEvent)f.Event).LowerYearGroup.ToString()).OrderBy(f => f.getRank()).Take(Event.EventRanges.TopLowerYearGroupInividualCertificates).ToList();
        }

        // static helper
        public static List<CertificateData> getCertificatesLowerYearGroup(ISchoolResultsEvent Event)
        {
            List<CertificateData> CD = new List<CertificateData>();
            if (Event.EventRanges.TopLowerYearGroupInividualCertificates == 0)
                return CD;

            // Certificate
            int counter = 1;

            foreach (AResult Result in Event.TopLowerYearGroup())
                CD.AddRange(((AEvent)Event).buildCertificateData(Result.Competitor, "TopLowerYearGroupIndividuals", counter++));

            return CD;
        }

        public List<AResult> TopLowerYearGroup()
        {
            return IndividualTimedSchoolEvent.TopLowerYearGroup((ISchoolResultsEvent)this);
        }

        public List<CertificateData> getCertificatesLowerYearGroup()
        {
            return IndividualTimedSchoolEvent.getCertificatesLowerYearGroup((ISchoolResultsEvent)this);
        }

        public override string getEventType ( )
        {
            return "Individual Timed School Event";
        }


    }

    public class IndividualDistanceEvent : AIndividualEvent, IEnterDistanceResults
    {

#region Constructor

        public IndividualDistanceEvent() : base(null, ResultDisplayDescription.NotDeclared, null) { }

        public IndividualDistanceEvent(string Name, ResultDisplayDescription ResultType, Championship Championship) : base(Name, ResultType, Championship) { }

        public IndividualDistanceEvent(IndividualDistanceEvent Event, Championship Championship) : base(Event, Championship) { }

#endregion

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
            return "Individual Distance Event";
        }


    }

}
