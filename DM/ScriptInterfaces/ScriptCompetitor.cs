using ChampionshipSolutions.DM.ScriptInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM.ScriptInterfaces
{
    public interface IScriptCompetitor
    {
        IScriptEvent CompetingIn { get; }
        string Vest { get; }

        IScriptTeam getTeam();

        string printTeam { get; }

        string printVestNumber();

        string getName();
        string Name { get; }
    }

    public interface IScriptEditCompetitor : IScriptCompetitor
    {
        new IScriptEvent CompetingIn { get; set; }
        new string Vest { get; set; }
    }

    //public interface ILanedCompetitor : ICompetitor
    //{
    //    bool hasLaneNumber();
    //    int getLaneNumber();
    //    void setLaneNumber(int Number);

    //    int LaneNumber { get; }
    //}

    //public interface IHeatedCompetitor : ICompetitor
    //{
    //    IEvent HeatEvent { get; }

    //    void promoteToFinal();
    //    void demoteFromFinal();
    //    bool isInFinal();
    //}

    //public interface IWHeatedCompetitor : IHeatedCompetitor, IWCompetitor
    //{
    //    new IEvent HeatEvent { set; }
    //}

    //public interface ILanedHeatedCompetitor : ILanedCompetitor, IHeatedCompetitor
    //{
    //    bool hasHeatLaneNumber();
    //    int getHeatLaneNumber();
    //    void setHeatLaneNumber(int Number);

    //    int HeatLaneNumber { get; }
    //}

}

namespace ChampionshipSolutions.DM
{
    public abstract partial class ACompetitor : IScriptCompetitor
    {
        IScriptEvent IScriptCompetitor.CompetingIn { get { return this.CompetingIn; } }
        string IScriptCompetitor.Vest { get { return this.Vest.ToString(); } }
        // to do 
        IScriptTeam IScriptCompetitor.getTeam()
        {
            return null; 
            // return this.getTeam(); 
        }
    }
}
