using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{

    public interface IEvent : ICustomData, IID
    {
        string Name { get; set; }
        string ShortName { get; set; }
        string Description { get; set; }
        EventRanges EventRanges { get; }
        ACompetitor getCompetitor(string Vest);
        DateTime? StartTime { get; set; }

        string getEventType ( );

        List<ACompetitor> getEnteredCompetitors();
        List<ACompetitor> getEnteredCompetitors(Team team);

        bool isTeamFull(Team team);
        bool isEventFull();
    }

    public interface IIndividualEvent : IEvent
    {
    }

    public interface ISquadEvent : IEvent
    {
    }

    public interface IStandards : IEvent
    {
        bool achievedStandard(ResultValue ResultValue);
        bool hasStandards();
        string getStandardShortString(ResultValue ResultValue);
        Standard[] Standards { get; }
        void addStandard ( Standard Standard );
        void removeStandard ( Standard Standard );
    }

    public interface IEnterAthletes : IEvent
    {
        bool canBeEntered(Athlete athlete);

        int countCompetitors();

        void enterAthlete(Athlete Athlete, VestNumber Vest, bool Guest = false);
        void enterAthlete(Athlete Athlete, bool Guest = false);
        
        List<ACompetitor> EnteredCompetitors { get; set; }

        bool isAvailable(Athlete proposedAthlete);

        List<Athlete> listAvailableAthletes(List<Athlete> proposedAthletes);
        void removeCompetitor(ACompetitor Competitor);
    }

    public interface IHeatEvent : IEvent
    {
        bool hasCompetitor(ACompetitor Competitor);
        //List<ACompetitor> allCompetitorsinHeat();

        AResult AddHeatResult(int Rank, IHeatedCompetitor Competitor, ResultValue resultValue);
        IndividualTimedFinalEvent Final { get; }
    }

    public interface IFinalEvent : IEvent
    {
        List<ACompetitor> promotedCompetitors();

        void promoteCompetitorToFinal(IHeatedCompetitor Competitor);
        void dropFromFinal(IHeatedCompetitor Competitor);

        List<IHeatEvent> getHeats();

        // How would we handle excess competitors?
        //void runHeatAsFinal();

        IndividualTimedHeatEvent createHeat ( );
        void removeHeat ( IHeatEvent Heat );

        bool HeatRunAsFinal { get; set; }
        int getHeatNumber(IHeatEvent heat);
    }

    public interface IEnterResults : IEvent
    {
        AResult AddDNF(ACompetitor Competitor);
        AResult AddPlaceholderResult(int Rank = 0);

        AResult AddResult(ACompetitor Competitor);
        AResult AddResult(ACompetitor Competitor, ResultValue resultValue);
        AResult AddResult(VestNumber vest, ResultValue resultValue);
        AResult AddResult(int Rank, ACompetitor Competitor);
        AResult AddResult(int Rank, ACompetitor Competitor, ResultValue resultValue);
        AResult AddResult(int Rank, ResultValue resultValue);
        AResult AddResult(int Rank, VestNumber vest);
        AResult AddResult(int Rank, VestNumber vest, ResultValue resultValue);
    }

    public interface IEnterTimedResults : IEnterResults
    {
        AResult AddResult(int Rank, ACompetitor Competitor, TimeSpan timedResultValue);
        AResult AddResult(int Rank, TimeSpan timedResultValue);
        AResult AddResult(ACompetitor Competitor, TimeSpan timedResultValue);
        AResult AddResult(VestNumber vest, TimeSpan timedResultValue);
        AResult AddResult(int Rank, VestNumber vest, TimeSpan timedResultValue);
    }

    public interface IEnterDistanceResults : IEnterResults
    {
        AResult AddResult(int Rank, ACompetitor Competitor, decimal distanceResultValue);
        AResult AddResult(int Rank, decimal distanceResultValue);
        AResult AddResult(ACompetitor Competitor, decimal distanceResultValue);
        AResult AddResult(VestNumber vest, decimal distanceResultValue);
        AResult AddResult(int Rank, VestNumber vest, decimal distanceResultValue);
    }

    public interface IResults : IEvent, IEnterResults
    {
        int getNextResultRank();

        List<AResult> getResultsForTeam(Team team);
        Team getScoringTeam(int Rank, string ScoringTeam);
        List<AResult> getScoringTeamResults(int Rank, string ScoringTeam);
        ScoringTeam[] getScoringTeams();
        
        void insertRankSpace(int Rank);
   
        bool isRankAvailable(int Rank);

        void moveResultDown(AResult result);
        void moveResultUp(AResult result);
        void removeResult(AResult result);

        ResultDisplayDescription ResultsDisplayDescription { get; set; }

        List<AResult> ScoringTeamA(Team team);
        List<AResult> ScoringTeamB(Team team);
        List<AResult> TopIndividuals();
        List<AResult> AllResults();

        AResult getResult(int Rank);
        AResult getResult(ACompetitor Competitor);
        bool hasResultFor(ACompetitor Competitor);

    }

    public interface IEventExports : IEvent
    {
        string CertificateTemplate { get; set; }
        string DataEntryTemplate { get; set; }

        List<CertificateData> getCertificateData();
        string ResultsTemplate { get; set; }

    }

    public interface ISelecion : IEvent
    {
        int countCurrentlySelected();

    }

    public interface IGroups : IEvent
    {
        List<Group> getGroups();
        bool hasRestriction();
        bool hasGroup ( Group Group );

        void addGroup(Group newRestriction);
        void clearGroups();

        bool isAvailable(Athlete proposedAthlete);
        List<Athlete> listAvailableAthletes(List<Athlete> proposedAthletes);
    }

    public interface ISchoolEvent : IEvent
    {
        int LowerYearGroup { get; set; }

        List<AResult> TopLowerYearGroup();

        List<CertificateData> getCertificatesLowerYearGroup();
    }

    public interface ISchoolResultsEvent : ISchoolEvent, IResults { }

    public interface ILaneAssignedEvent : IEvent 
    {
        bool hasLaneAssignementInformation ( );
        [Obsolete ("Lanes have been withdrawn from the application",true)]
        bool requiresLaneUpdate();
        [Obsolete ("Lanes have been withdrawn from the application",true)]
        void updateLanes();
        [Obsolete ("Lanes have been withdrawn from the application",true)]
        int laneForVest(int Vest);
        int getEmptyLane();
        bool isLaneFree(int Lane);
        void clearLanes();
        //[Obsolete ("Lanes have been withdrawn from the application",true)]
        int[] emptyAssingedLane();
        int[] emptyLanes();
    }

}
