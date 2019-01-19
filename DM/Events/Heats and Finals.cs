// 2017-06-03 changed to stop specific heats having their own competitors 

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
//using ChampionshipSolutions.Diag;
//using static ChampionshipSolutions.Diag.Diagnostics;

namespace ChampionshipSolutions.DM
{

    public partial class IndividualTimedHeatEvent : IndividualTimedEvent, IHeatEvent, ILaneAssignedEvent
    {

        public override string PrintableName { get { return string.Format ( "{0} {1}" , Name , Final.getHeatNumber ( this ).ToString ( ) ); } }

        public IndividualTimedFinalEvent Final { get { return _Final; } set { _Final = value; } }

        #region Constructor

        public IndividualTimedHeatEvent() : base(null, ResultDisplayDescription.NotDeclared, null) { }

        public IndividualTimedHeatEvent(string Name, ResultDisplayDescription ResultType, Championship Championship) : base(Name, ResultType, Championship) { }

        public IndividualTimedHeatEvent(IndividualTimedHeatEvent Event, IndividualTimedFinalEvent Final,  Championship Championship)
            : base(Event, Championship)
        {
            this.Final = Final;
        }

        #endregion

        #region Standards - Points to final

        public override bool achievedStandard(ResultValue ResultValue)
        {
            return Final.achievedStandard(ResultValue);
        }

        public override string getStandardShortString(ResultValue ResultValue)
        {
            return Final.getStandardShortString(ResultValue);
        }

        #endregion

        #region Results

        protected override AResult addResult(int Rank, ACompetitor Competitor, ResultValue resultValue, string VestNumber = "")
        {
            // on 2015-06-12 changed to always store a heats result in the heat rather than check with the final
            //return Final.AddResult(Rank, Competitor, resultValue);
            if ( Competitor is IHeatedCompetitor hc )
            {
                hc.HeatEvent = this;
                Competitor.Save( );
                return AddHeatResult( Rank , hc, resultValue );
            }
            throw new Exception("Competitor must inherit from IHeatedCompetitor");
        }

        public AResult AddHeatResult(int Rank, IHeatedCompetitor Competitor, ResultValue resultValue)
        {
            return base.addResult(Rank, (ACompetitor)Competitor, resultValue);
        }


        /// <summary>
        /// To Do - maybe?
        /// </summary>
        /// <param name = "st" ></ param >
        /// < returns ></ returns >
        //protected override List<CertificateData> getCertificatesTeams(ScoringTeam st)
        //{
        //    throw new NotImplementedException();
        //    return Final.getCertificatesTeams(st);
        //}

        #endregion

        #region Competitor

        //public override ACompetitor getCompetitor(string Vest)
        //{
        //    return allCompetitorsinHeat().Where(c => c.Vest.ToString() == Vest).FirstOrDefault();
        //}

        public override void enterAthlete(Athlete Athlete, VestNumber Vest, bool Guest = false)
        {
            Final.enterAthlete(Athlete, Vest, Guest);
        }

        public override void removeCompetitor(ACompetitor Competitor)
        {
            Final.removeCompetitor(Competitor);
        }


        public List<ACompetitor> allCompetitorsinHeat ( )
        {
            // 2017-06-03
            //return Final.allCompetitorsinHeat(this);
            return Final.getEnteredCompetitors( );
        }

        public override int countCompetitors ( )
        {
            // 2017-06-03
            //return Final.allCompetitorsinHeat( this ).Count;
            return Final.EnteredCompetitors.Count( );
        }

        /// <summary>
        /// Returns all currently entered competitors not including guests
        /// </summary>
        /// <returns>List of Competitors</returns>
        public override List<ACompetitor> getEnteredCompetitors ( )
        {
            //return Final.allCompetitorsinHeat( this ).Where( i => i.Guest == false ).OrderBy( a => a.Vest ).ToList( );
            return Final.EnteredCompetitors.Where( i => i.Guest == false ).OrderBy( a => a.Vest.IntOrder ).ToList( );
        }

        /// <summary>
        /// Returns all currently entered competitors for a Team
        /// </summary>
        /// <returns>List of Competitors</returns>
        public override List<ACompetitor> getEnteredCompetitors ( Team team )
        {
            //return Final.allCompetitorsinHeat( this ).Where( i => i.Guest == false && i.getTeam( ) == team ).OrderBy( a => a.Vest ).ToList( );
            return Final.EnteredCompetitors.Where( i => i.Guest == false && i.getTeam( ) == team ).OrderBy( a => a.Vest.IntOrder ).ToList( );
        }


        public override bool isEventFull ( )
        {
            if ( EventRanges.MaxCompetitors == 0 ) return false;

            return (this.allCompetitorsinHeat( ).Count( i => i.Guest == false ) >= EventRanges.MaxCompetitors);
        }

        public override bool isTeamFull ( Team teamToCheck )
        {
            if ( EventRanges.MaxCompetitorsPerTeam == 0 )
                return false;

            return (this.allCompetitorsinHeat( ).Count( i => i.Guest == false && i.getTeam( ) == teamToCheck ) >= EventRanges.MaxCompetitorsPerTeam);
        }

        /// <summary>
        /// Check if a competitor has been entered into this heat.
        /// </summary>
        /// <param name="Competitor">Competitor to check.</param>
        /// <returns>True if the competitor is in this heat.</returns>
        public bool hasCompetitor ( ACompetitor Competitor )
        {
            //return (Final.allCompetitorsinHeat( this ).Contains( Competitor ));
            return (Final.EnteredCompetitors.Contains( Competitor ));
        }

        #endregion

        #region Lanes



        ///// <summary>
        ///// Checks to see if any competitor has not been assigned a lane.
        ///// </summary>
        ///// <returns>True if at least one competitor doesn't have a lane.</returns>
        //[Obsolete ("Removed from this version",true)]
        //public override bool requiresLaneUpdate()
        //{
        //    // do not have to update an empty event
        //    if (this.allCompetitorsinHeat().Count == 0) return false;

        //    foreach (ILanedHeatedCompetitor c in this.allCompetitorsinHeat())
        //        if (!c.hasHeatLaneNumber())
        //            return true;

        //    return false;
        //}

        ///// <summary>
        ///// Will remove the LanNumber value from all entered competitors.
        ///// </summary>
        //public override void clearLanes()
        //{
        //    foreach (ILanedHeatedCompetitor c in allCompetitorsinHeat())
        //        c.setHeatLaneNumber(0);
        //}


        ///// <summary>
        ///// Allocates a lane for each of the entered competitors if they don't already have one.
        ///// </summary>
        //[Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        //public override void updateLanes()
        //{
        //    if (!requiresLaneUpdate()) return;

        //    foreach (ILanedHeatedCompetitor c in allCompetitorsinHeat())
        //    {
        //        // this competitor already has a lane number so ignore it.
        //        if (c.hasHeatLaneNumber()) continue;

        //        // get the integer vest number
        //        if ( c.Vest.tryIntVest( out int vest ) )
        //            c.setHeatLaneNumber( laneForVest( vest ) );
        //        else
        //            // we can only process integer vest numbers
        //            throw new ArgumentException( "Only integer vest numbers can be assigned a lane" );
        //    }

        //    if (requiresLaneUpdate())
        //        throw new ArgumentException("Failed to allocate a lane to at least one competitor");
        //}

        #endregion

        public override string getEventType ( )
        {
            return "Individual Timed Heat Event";
        }

    }

    public partial class IndividualTimedFinalEvent : IndividualTimedEvent, IFinalEvent, ILaneAssignedEvent
    {
        #region Constructor

        /// <summary>
        /// Required for entity framework
        /// </summary>
        public IndividualTimedFinalEvent() : base(null, ResultDisplayDescription.NotDeclared, null) { }

        /// <summary>
        /// Creates a new event object.
        /// </summary>
        /// <param name="Name">Name of the event.</param>
        /// <param name="ResultType">Type of result and format of that result.</param>
        /// <param name="Championship">The Championship this event belongs to.</param>
        public IndividualTimedFinalEvent(string Name, ResultDisplayDescription ResultType, Championship Championship) : base(Name, ResultType, Championship) { }

        public IndividualTimedFinalEvent(IndividualTimedFinalEvent Event, Championship Championship)
            : base(Event, Championship)
        {
            MaxCompetitorsPerHeat = Event.MaxCompetitorsPerHeat;
            
            foreach (IndividualTimedHeatEvent heat in Event.Heats)
            {
                copyHeat( this , heat );
            }

        }

        #endregion

        protected IndividualTimedHeatEvent[] Heats { get { return _Heats.ToArray(); } }

        public static IndividualTimedHeatEvent copyHeat ( IndividualTimedFinalEvent Final , IndividualTimedHeatEvent Heat )
        {
            IndividualTimedHeatEvent ITHE = new IndividualTimedHeatEvent( )
            {
                Name = Heat.Name ,
                Description = Heat.Description ,
                Championship = Final.Championship
            };
            Final.Championship.addEvent( ITHE );

            ITHE.DataEntryTemplate = null; // we are not going to set the templates for the entry forms.
            ITHE.CertificateTemplate = null;
            ITHE.ResultsTemplate = null;
            ITHE.VestTemplate = null;

            foreach ( EventGroups er in Final.Groups ) // this is a bit of bodge as we are taking the groups from the final not cloning the head.
                ITHE.addGroup( er.Group );

            ITHE.MaxCompetitors = Heat.MaxCompetitors;
            ITHE.MinCompetitors = Heat.MinCompetitors;
            ITHE.MaxCompetitorsPerTeam = Heat.MaxCompetitors;
            ITHE.MaxGuests = Heat.MaxGuests;
            ITHE.TopIndividualCertificates = Heat.TopIndividualCertificates;
            ITHE.TopLowerYearGroupInividualCertificates = Heat.TopLowerYearGroupInividualCertificates;
            ITHE.TeamASize = Heat.TeamASize;
            ITHE.TeamBSize = Heat.TeamBSize;
            ITHE.TeamBForScoringTeamOnly = Heat.TeamBForScoringTeamOnly;
            ITHE.ScoringTeams = Heat.ScoringTeams;
            ITHE.Lanes = Heat.Lanes;


            ITHE.EventRanges.MaxCompetitors = Heat.EventRanges.MaxCompetitors;
            ITHE.EventRanges.TopIndividualCertificates = Heat.EventRanges.TopIndividualCertificates;
            ITHE.EventRanges.TopLowerYearGroupInividualCertificates = Heat.EventRanges.TopLowerYearGroupInividualCertificates;
            ITHE.EventRanges.TeamASize = Heat.EventRanges.TeamASize;
            ITHE.EventRanges.TeamBSize = Heat.EventRanges.TeamBSize;
            ITHE.EventRanges.ScoringTeams = Heat.EventRanges.ScoringTeams;

            ITHE.Final = Final;

            //this._Heats.Add(ITHE);

            //DState.IO.Add<AEvent> ( ITHE );
            ITHE.Save( );
            Final.voidStorage( );
            return ITHE;
        }

        public IndividualTimedHeatEvent createHeat()
        {
            IndividualTimedHeatEvent ITHE = new IndividualTimedHeatEvent();

            ITHE.Name = this.Name + " Heat";
            ITHE.Description = this.Description;
            ITHE.Championship = this.Championship;
            this.Championship.addEvent(ITHE);

            ITHE.DataEntryTemplate = this.DataEntryTemplate;
            ITHE.CertificateTemplate = this.CertificateTemplate;
            ITHE.ResultsTemplate = this.ResultsTemplate;
            ITHE.VestTemplate = this.VestTemplate;

            foreach (EventGroups er in this.Groups)
                ITHE.addGroup( er.Group );

            //ITHE.EventRanges = EventRanges.copyEventRanges(this.EventRanges);

            ITHE.MaxCompetitors = MaxCompetitors;
            ITHE.MinCompetitors = MinCompetitors;
            ITHE.MaxCompetitorsPerTeam = MaxCompetitors;
            ITHE.MaxGuests = MaxGuests;
            ITHE.TopIndividualCertificates = TopIndividualCertificates;
            ITHE.TopLowerYearGroupInividualCertificates = TopLowerYearGroupInividualCertificates;
            ITHE.TeamASize = TeamASize;
            ITHE.TeamBSize = TeamBSize;
            ITHE.TeamBForScoringTeamOnly = TeamBForScoringTeamOnly;
            ITHE.ScoringTeams = ScoringTeams;
            ITHE.Lanes = Lanes;


            ITHE.EventRanges.MaxCompetitors = MaxCompetitorsPerHeat;
            ITHE.EventRanges.TopIndividualCertificates = 0;
            ITHE.EventRanges.TopLowerYearGroupInividualCertificates = 0;
            ITHE.EventRanges.TeamASize = 0;
            ITHE.EventRanges.TeamBSize = 0;
            ITHE.EventRanges.ScoringTeams = 0;

            ITHE.Final = this;

            //this._Heats.Add(ITHE);

            //DState.IO.Add<AEvent> ( ITHE );
            ITHE.Save ( );
            this.voidStorage ( );
            return ITHE;
        }

        public void removeHeat( IHeatEvent Heat )
        {
            if ( Heats.Contains ( Heat ) )
            {
                // 2017-06-03
                //if ( Heat.allCompetitorsinHeat ( ).Count == 0 )
                {
                    //!*! 
                    DState.IO.Delete<AEvent> ( this );

                    //_Heats.Remove ( (IndividualTimedHeatEvent)Heat );
                    ( (IndividualTimedHeatEvent)Heat ).Championship = null;
                }
                //else
                  //  throw new ArgumentException ( "Can not delete this heat as it has entered competitors" );
            }
        }

        #region Final Specific stuff

        public override bool isEventFull()
        {
            if (EventRanges.MaxCompetitors == 0) return false;

            return (this.promotedCompetitors().Count(i => i.Guest == false) >= EventRanges.MaxCompetitors);
        }

        public override bool isTeamFull(Team teamToCheck)
        {
            if (EventRanges.MaxCompetitorsPerTeam == 0)
                return false;

            return (this.promotedCompetitors().Count(i => i.Guest == false && i.getTeam() == teamToCheck) >= EventRanges.MaxCompetitorsPerTeam);
        }

        public int MaxCompetitorsPerHeat { get; set; }

        public void promoteCompetitorToFinal ( IHeatedCompetitor Competitor )
        {
            // Do not have to promote this competitor as they are already in the final.
            if ( Competitor.isInFinal ( ) ) return;

            if ( !this.isEventFull ( ) )
                if ( this.EnteredCompetitors.Contains ( (ACompetitor)Competitor ) )
                    if ( ( (ACompetitor)Competitor ).Result != null )
                        ( (IHeatedCompetitor)Competitor ).promoteToFinal ( );
                    else
                        if ( ((IndividualTimedFinalEvent) Competitor.CompetingIn).HeatRunAsFinal )
                            ( (IHeatedCompetitor)Competitor ).promoteToFinal ( );
                        else
                            throw new Exception ( "This competitor doesn't have a result in a heat so can not be promoted to the final." );
                else
                    throw new ArgumentException ( "This competitor is not in one of this events heats" );
            else
                throw new ArgumentException ( "This event is full" );

        }

        public void dropFromFinal(IHeatedCompetitor Competitor)
        {
            ((IHeatedCompetitor)Competitor).demoteFromFinal();
        }

        public int getHeatNumber(IHeatEvent heat)
        {
            if ( heat == null ) return 0;
            if (Heats.Contains((IndividualTimedHeatEvent)heat))
            {
                int i = 1;

                foreach (IHeatEvent h in Heats)
                {
                    if (h == heat)
                        break;
                    else
                        i++;
                }

                return i;
            }
            else
                throw new ArgumentException("Heat is not for this event");
        }

        ///// <summary>
        ///// How do we handle excess competitors that did not compete?
        ///// </summary>
        //public void runHeatAsFinal()
        //{
        //    // for all competitors 



        //    throw new NotImplementedException();
        //}

        public bool HeatRunAsFinal { get { return _HeatRunAsFinal ?? false; } set { _HeatRunAsFinal = value; Save ( ); } }

        public List<IHeatEvent> getHeats()
        {
            //return Heats;

            List<IHeatEvent> temp = new List<IHeatEvent>();

            foreach (IndividualTimedHeatEvent h in Heats)
            {
                temp.Add((IHeatEvent)h);
            }

            return temp;
        }

        #endregion

        #region Competitors

        public override List<ACompetitor> getEnteredCompetitors()
        {
            // 2016-06-28 HeatRunAsFinal concept has been removed as lane draw for final is no longer required.
            //if (HeatRunAsFinal)
                return EnteredCompetitors.ToList( ).OrderBy( p => p.Vest.IntOrder ).ToList( ) ;
            //else
                //return promotedCompetitors();
        }

        public List<ACompetitor> allCompetitorsinHeat(IHeatEvent Event)
        {
            List<ACompetitor> temp = new List<ACompetitor>();

            foreach (ACompetitor comp in EnteredCompetitors.OrderBy ( p => p.Vest.IntOrder))
                //2017-06-03
                temp.Add( comp );

            //if ( comp is IHeatedCompetitor)
            //        if (((IHeatedCompetitor)comp).HeatEvent == (AEvent)Event)
            //            temp.Add(comp);

            return temp;
        }

        /// <summary>
        /// Will enter an athlete into a heat.
        /// ToDo this does not check for max team restraint.
        /// </summary>
        public override void enterAthlete(Athlete Athlete, VestNumber Vest, bool Guest = false)
        {
            // check to see if this athlete is eligible
            if (!isAvailable(Athlete))
                throw new ArgumentException("Athlete is not eligible for this event");

            // Check to see if athlete has been entered already
            foreach (ACompetitor c in EnteredCompetitors)
                if (c.isAthlete(Athlete))
                    throw new ArgumentException("Athlete has already been entered");

            //// decide which heat to enter this athlete into
            //IHeatEvent selectedHeat = null;

            //// Automatic heat assignment based on vest number has been pulled from this application 
            ////so we simply assign heat using the least populated event

            ////// Try to find the heat that this vest goes with 
            //////selectedHeat = getAllocatedHeatForVest(Vest);

            //Team t = Athlete.getTeam( Championship ) ;

            //if (selectedHeat == null)
            //    selectedHeat = getLeatPopulatedEvent( t );

            ////foreach (IndividualTimedHeatEvent heat in Heats)
            ////{
            ////    // is the heat full, if so skip
            ////    if (heat.isEventFull()) continue;

            ////    // is the Team full for this heat, if so skip
            ////    if (heat.isTeamFull(Athlete.getTeam(this.Championship))) continue;

            ////    selectedHeat = heat;
            ////    break;
            ////}

            //if (selectedHeat == null) throw new ArgumentException("Could not find a heat with space for this competitor");
            // todo URGENT 2016-06-03 Student aThlete and StudentHeatedCompetitor shouldn't really be needed here.
            if ( Athlete is Athlete )
                //this.EnteredCompetitors.Add(new StudentHeatedCompetitor(Athlete, Vest, this) { Guest = Guest, HeatEvent = (AEvent)selectedHeat });
                this.AddCompetitor( new StudentHeatedCompetitor( Athlete , Vest , this ) { Guest = Guest } ); // , HeatEvent = (AEvent)selectedHeat });
            //else
            //this.EnteredCompetitors.Add(new HeatedCompetitor(Athlete, Vest, this) { Guest = Guest, HeatEvent = selectedHeat });
        }

        /// <summary>
        /// Will remove the competitor from the final and all heats.
        /// To Do check heat for results before removing
        /// </summary>
        public override void removeCompetitor(ACompetitor Competitor)
        {
            if (Competitor.getResult() == null)
            {
                // 2015-06-12 I've commented out the remove from heat command as the competitor is only stored in the final.
                //// remove from each of the heats
                foreach (IndividualTimedHeatEvent heat in Heats)
                    if (heat.hasCompetitor((Competitor)Competitor))
                        // ToDo checking for results this way will not work!
                        if (heat.hasResultFor(Competitor))
                            throw new ArgumentException("Can not remove this competitor from this event as they have a result");
                //else
                //            heat.EnteredCompetitors.Remove(Competitor);

                // remove from final
                if (EnteredCompetitors.Contains(Competitor))
                    this.RemoveCompetitor(Competitor);
            }
            else
            {
                throw new ArgumentException("Can not remove this competitor from this event as they have a result");
            }
        }

        public List<ACompetitor> promotedCompetitors()
        {
            List<ACompetitor> temp = new List<ACompetitor>();

            foreach (ACompetitor comp in EnteredCompetitors)
                if (comp is IHeatedCompetitor)
                    if (((IHeatedCompetitor)comp).isInFinal())
                        temp.Add(comp);
            return temp;
        }

        #endregion

        #region Result

        // How do we handle heat that are run into finals
        protected override AResult addResult(int Rank, ACompetitor Competitor, ResultValue resultValue, string VestNumber = "")
        {
            if ( Competitor is IHeatedCompetitor competitor )
            {

                // We know this result is for the final so promote the competitor and add it.
                if ( HeatRunAsFinal )
                {
                    promoteCompetitorToFinal( competitor );
                    //competitor.promoteToFinal ( );
                    return base.addResult( Rank , Competitor , resultValue , VestNumber );
                }

                if ( this.promotedCompetitors( ).Contains( Competitor ) )
                {
                    // We know this result is for the final so just add it.
                    return base.addResult( Rank , Competitor , resultValue , VestNumber );
                }
                else
                {
                    // Move this competitor to the final and add the result
                    competitor.promoteToFinal( );
                    return base.addResult( Rank , Competitor , resultValue , VestNumber );
                }


                // This result could be for a heat
                //foreach (IndividualTimedHeatEvent heat in Heats)
                //{
                //    // skip this heat if this competitor is not in it.
                //    if (!heat.hasCompetitor((Competitor)Competitor)) continue;

                //    if (heat.hasResultFor(Competitor))
                //    {
                //        // This competitor should be in the final so we'll promote them now.
                //        promoteCompetitorToFinal(competitor);

                //        // Now we can add the result.
                //        return base.addResult(Rank, Competitor, resultValue);
                //    }
                //    else
                //    {
                //        // add the result to the heat
                //        return heat.AddHeatResult(Rank, competitor, resultValue);
                //    }
                //}

                // is not in a heat 
                throw new ArgumentException( "This Competitor is not in the final or the heat run as final flag is not set." );
            }
            else
            {
                return base.addResult( Rank , Competitor , resultValue , VestNumber );
                //throw new ArgumentException("Competitor must be an individual");
            }
        }

        #endregion

        #region Lanes

        public override bool hasLaneAssignementInformation()
        {
            if (base.hasLaneAssignementInformation()) return true;

            // The final doesn't have lane assignment information but the heats might.
            // This will be true if any of the heats have this information.

            foreach (ILaneAssignedEvent h in Heats)
            {
                if (h.hasLaneAssignementInformation())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if any competitor has not been assigned a lane.
        /// </summary>
        /// <returns>True if at least one competitor doesn't have a lane.</returns>
        [Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        public override bool requiresLaneUpdate()
        {
            foreach ( IndividualTimedHeatEvent heat in Heats )
            {
                if ( heat.requiresLaneUpdate ( ) ) return true;
            }

            // don't bother with lane assignments if this event was run as a heat.
            if (HeatRunAsFinal) return false;


            // do not have to update an empty event
            if (EnteredCompetitors.Count() == 0) return false;

            // do not want to update lanes on an event which has already got results
            if (AllResults().Count > 0) return false;

            if (promotedCompetitors().Count() == 0)
                return true;

            foreach (ILanedCompetitor c in promotedCompetitors())
                if (!c.hasLaneNumber())
                    return true;

            return false;

        }


        /// <summary>
        /// Finds the heat that has this vest number allocated to it.
        /// </summary>
        /// <param name="vest"></param>
        /// <returns>Will return null if no heat claims this vest number.</returns>
        [Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        protected IHeatEvent getAllocatedHeatForVest ( VestNumber vest)
        {
            if (Heats.Count() == 0)
                throw new ArgumentException("There are no heats for this event.");


            if ( !vest.tryIntVest( out int VestNumber ) )
                throw new ArgumentException( "Vest must be able to convert to an integer." );

            foreach (IHeatEvent heat in Heats)
            {
                if ( heat is ILaneAssignedEvent iheat )
                {
                    if ( iheat.hasLaneAssignementInformation( ) )
                        if ( buildLaneDraw( iheat ).Contains( VestNumber ) ) return heat;
                    //{
                    //    // Check to see if a lane has an allocated vest 
                    //    int[] laneDraw = buildLaneDraw(iheat);
                    //    if (laneDraw.Contains(VestNumber))
                    //        return heat;
                    //}
                }
            }

            // none of the heats has a lane allocated for this vest.
            return null;
        }

        /// <summary>
        /// Returns the least populated event.
        /// </summary>
        /// <returns></returns>
        [Obsolete ( "Use getLeatPopulatedEvent ( Team team = null )", true )]
        protected IHeatEvent getLeatPopulatedEvent()
        {
            if (Heats.Count() == 0)
                throw new ArgumentException("There are no heats for this event.");

            int [] comps = Heats.Select( p => p.countCompetitors() ).ToArray();

            int minComps = Heats.Select(p => p.countCompetitors()).ToList().Min();

            IHeatEvent[] heat = Heats.Where(h => h.countCompetitors() == minComps).ToArray();

            foreach (IHeatEvent h in heat)
            {
                // is the heat full, if so skip
                if (h.isEventFull()) continue;

                return h;
            }

            // all events are full
            throw new ArgumentException("All events are full.");
        }

        /// <summary>
        /// Returns the least populated event for a Team.
        /// </summary>
        /// <returns></returns>
        protected IHeatEvent getLeatPopulatedEvent ( Team team = null )
        {
            if ( Heats.Count ( ) == 0 )
                throw new ArgumentException ( "There are no heats for this event." );

            int [] comps;
            int minComps;

            if ( team == null )
                comps = Heats.Select( p => p.countCompetitors() ).ToArray();
            else
                comps = Heats.Select( p => p.Final.allCompetitorsinHeat(p).Where(c => c.Team == team).Count() ).ToArray();

            minComps = comps.Min();

            IHeatEvent[] heat;
            if ( team == null )
                heat = Heats.Where ( h => h.countCompetitors ( ) == minComps ).ToArray ( );
            else
                heat = Heats.Where ( p => p.Final.allCompetitorsinHeat ( p ).Where ( c => c.Team == team ).Count ( ) == minComps ).ToArray();

            foreach ( IHeatEvent h in heat )
            {
                // is the heat full, if so skip
                if ( h.isEventFull ( ) ) continue;

                return h;
            }

            // all events are full
            throw new ArgumentException ( "All events are full." );
        }

        //2017-06-03
        ///// <summary>
        ///// Allocates a lane for each of the entered competitors if they don't already have one.
        ///// For final event this will promote competitors from the heats and assign lanes.
        ///// </summary>
        //[Obsolete ("Programmatic Lane Assignment has been pulled from the application", true)]
        //public override void updateLanes()
        //{
        //    if (!requiresLaneUpdate()) return;

        //    if ( this.Results.Count ( ) > 0 ) return;

        //    foreach ( IndividualTimedHeatEvent heat in Heats )
        //    {
        //        heat.updateLanes() ;
        //    }

        //    // check to see if the heats have been completed - must have a least three results to allocate lanes
        //    foreach ( IResults h in Heats )
        //        if ( h.AllResults ( ).Count < 3 )
        //            // this heat doesn't have enough results so we can't allocate lanes
        //            return ;

        //    int countHeat1, countHeat2, countHeatResults1, countHeatResults2;

        //    // count the number of entries into the heats
        //    countHeat1 = getHeats ( ).First ( ).allCompetitorsinHeat ( ).Count;
        //    countHeat2 = getHeats ( ).Last  ( ).allCompetitorsinHeat ( ).Count;

        //    // count results
        //    countHeatResults1 = ((AEvent)getHeats ( ).First ( )).Results.Count();
        //    countHeatResults2 = ((AEvent)getHeats ( ).Last  ( )).Results.Count();

        //    int [] heatArray = new int[] { countHeatResults1 , countHeatResults2 };

        //    List<AResult> promoted = new List<AResult>();


        //    if ( heatArray.Min ( ) == 3 && heatArray.Max ( ) == 4 )
        //    {
        //        List<AResult> winners = new List<AResult>();

        //        // take the top 2 from each heat
        //        foreach ( IResults h in Heats )
        //        {
        //            winners.Add ( (AResult)h.getResult ( 1 ) );
        //            winners.Add ( (AResult)h.getResult ( 2 ) );
        //        }

        //        List<AResult> allHeatResults = new List<AResult>();

        //        foreach ( IResults h in Heats )
        //            allHeatResults.AddRange ( h.AllResults ( ) );

        //        promoted.AddRange ( winners );


        //        List<AResult> orderedHeatResults = allHeatResults.OrderBy(s => s.Value.RawValue).ToList();

        //        // we are going to take the three fastest losers - Only two will go ahead but we need to check for a tie.
        //        IList<AResult> orderedHeatResultsNoWinners = (orderedHeatResults.Except(winners)).Take(3).ToList();

        //        if ( orderedHeatResultsNoWinners.Count ( ) == 0 )
        //        {
        //            // do nothing here
        //        }
        //        else if ( orderedHeatResultsNoWinners.Count ( ) == 1 ||
        //            orderedHeatResultsNoWinners.Count ( ) == 2 )
        //        {
        //            // These athletes will proceed 
        //            promoted.AddRange ( orderedHeatResultsNoWinners );
        //        }
        //        else if ( orderedHeatResultsNoWinners.Count ( ) == 3 )
        //        {
        //            if ( orderedHeatResultsNoWinners[1].Value.RawValue == orderedHeatResultsNoWinners[2].Value.RawValue )
        //            {
        //                // we have a tie
        //                if ( orderedHeatResultsNoWinners[1].Rank == orderedHeatResultsNoWinners[2].Rank )
        //                {
        //                    // This tie can not be broken
        //                    LogLine ( string.Format ( "There is an unbreakable tie in {0} because there is a 3/4 split in the heats and the second and third fastest losers have the same time and rank." , this.Name ) , MessagePriority.Infomation );
        //                    foreach ( ILanedHeatedCompetitor comp in EnteredCompetitors )
        //                        comp.promoteToFinal ( );
        //                    return;
        //                }
        //                else
        //                {
        //                    promoted.Add ( orderedHeatResultsNoWinners[0] );

        //                    if ( orderedHeatResultsNoWinners[1].Rank > orderedHeatResultsNoWinners[2].Rank )
        //                    {
        //                        // take competitor 1
        //                        promoted.Add ( orderedHeatResultsNoWinners[1] );
        //                    }
        //                    else
        //                    {
        //                        // take competitor 2
        //                        promoted.Add ( orderedHeatResultsNoWinners[2] );
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // promoted the first two losers
        //                promoted.Add ( orderedHeatResultsNoWinners[0] );
        //                promoted.Add ( orderedHeatResultsNoWinners[1] );
        //            }
        //        }
        //        else if ( orderedHeatResultsNoWinners.Count ( ) > 3 )
        //        {
        //            throw new Exception ( "It shouldn't be possible to have more than three athletes at this point" );
        //        }
        //    }
        //    else if ( countHeatResults1 >= 3 && countHeatResults2 >= 3 )
        //    {
        //        // standard promotion method - Take top three from each heat

        //        // take the top 3 from each heat
        //        foreach ( IResults h in Heats )
        //        {
        //            promoted.Add ( (AResult)h.getResult ( 1 ) );
        //            promoted.Add ( (AResult)h.getResult ( 2 ) );
        //            promoted.Add ( (AResult)h.getResult ( 3 ) );
        //        }

        //    }
        //    else
        //    {
        //        // should never happen
        //        LogLine ( string.Format ( "There aren't enough results to process the lanes for {0}" , this.Name ) , MessagePriority.Infomation );
        //        return;
        //    }


        //    foreach ( ACompetitor competitor in getEnteredCompetitors ( ) )
        //    {
        //        foreach ( AResult r in promoted )
        //            if ( r.Competitor == competitor )
        //                goto CompetitorHasBeenPromoted;

        //        // competitor is not in the promoted results

        //        if ( competitor is StudentHeatedCompetitor )
        //            if ( ( (StudentHeatedCompetitor)competitor ).isInFinal ( ) )
        //                promoted.Add ( competitor.Result );

        //        CompetitorHasBeenPromoted:
        //        continue;
        //    }

        //    // switch(straight / curved)

        //    int[] laneOrderCurved   = { 3, 4, 2, 5, 1, 6 };
        //    int[] laneOrderStraight = { 4, 5, 3, 6, 2, 7 };

        //    int i = 0;
        //    foreach ( AResult res in promoted.OrderBy ( r => r.Value.RawValue ) )
        //    {
        //        promoteCompetitorToFinal ( (IHeatedCompetitor)res.Competitor );

        //        if ( this.customFieldExists ( "Note" ) )
        //        {
        //            if ( this.getValue ( "Note" ).ToString ( ).Contains ( "Straight" ) )
        //            {
        //                if ( i < laneOrderStraight.Count ( ) )
        //                    ( (ILanedHeatedCompetitor)res.Competitor ).setLaneNumber ( laneOrderStraight[i++] );
        //                else
        //                    ( (ILanedHeatedCompetitor)res.Competitor ).setLaneNumber ( 0 );
        //            }
        //            else
        //            {
        //                if ( i < laneOrderCurved.Count ( ) )
        //                    ( (ILanedHeatedCompetitor)res.Competitor ).setLaneNumber ( laneOrderCurved[i++] );
        //                else
        //                    ( (ILanedHeatedCompetitor)res.Competitor ).setLaneNumber ( 0 );
        //            }
        //        }
        //        else
        //            throw new ArgumentException ( "Can not set lanes for final as we can't determine if it is a straight or curved track" );
        //    }


        //}

        #endregion

        public override string getEventType ( )
        {
            return "Individual Timed Final Event";
        }


    }

    public partial class IndividualTimedFinalSchoolEvent : IndividualTimedFinalEvent, ISchoolEvent, ISchoolResultsEvent
    {
        public IndividualTimedFinalSchoolEvent() : base() { }

        public IndividualTimedFinalSchoolEvent(IndividualTimedFinalSchoolEvent Event, Championship Championship)
            : base(Event, Championship)
        {
            this.LowerYearGroup = Event.LowerYearGroup;
        }

        public int LowerYearGroup { get { return _LowerYearGroup; } set { _LowerYearGroup = value; } }

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
            return "Individual Timed Final School Event";
        }


    }

}
