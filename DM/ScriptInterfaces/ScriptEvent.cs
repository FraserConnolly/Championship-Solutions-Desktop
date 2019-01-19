using ChampionshipSolutions.DM.ScriptInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM.ScriptInterfaces
{
    public interface IScriptEvent
    {
        int ID { get; }
        string Name { get; }
        string Description { get; }
        DateTime? StartTime { get; }
        ICollection<ICompetitor> getEnteredCompetitors();
        ICollection<ICompetitor> getEnteredCompetitors(Team team);

        EventRanges EventRanges { get; }
        IScriptCompetitor getCompetitor(string Vest);

        bool isTeamFull(Team team);
        bool isEventFull();

        ICollection<IScriptResult> getResults();

        AResult AddResult(int Rank, VestNumber vest, ResultValue resultValue);
        void removeResult(int Rank);

    }

    public interface IScriptResult
    {
        IScriptEvent Event { get; }
        int? Rank { get; }
        string VestNumber { get; }
        string ResultStr { get; }
    }
}

namespace ChampionshipSolutions.DM
{
    public abstract partial class AEvent : IScriptEvent
    {
        public ICollection<IScriptResult> getResults()
        {
            if (this.Results == null)
                return new List<IScriptResult>();

            return this.Results.OrderBy(r => r.Rank).ToList<IScriptResult>();
        }

        IScriptCompetitor IScriptEvent.getCompetitor(string Vest)
        {
            throw new NotImplementedException();
            //return this.getCompetitor(Vest);
        }

        ICollection<ICompetitor> IScriptEvent.getEnteredCompetitors()
        {
            throw new NotImplementedException();
        }

        ICollection<ICompetitor> IScriptEvent.getEnteredCompetitors(Team team)
        {
            throw new NotImplementedException();
        }

    }

    public abstract partial class AResult : IScriptResult
    {
        public string ResultStr { get { return printResult(); } }
        IScriptEvent IScriptResult.Event { get { return this.Event; } }
        int? IScriptResult.Rank { get { return Rank; } }
        string IScriptResult.VestNumber { get { return VestNumber.ToString(); } }

    }

}
