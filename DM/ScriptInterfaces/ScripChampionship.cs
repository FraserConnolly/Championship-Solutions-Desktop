using ChampionshipSolutions.DM.ScriptInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChampionshipSolutions.DM.ScriptInterfaces
{
    public interface IScriptChampionship
    {
        string ChampionshipName { get; }

        ICollection<IScriptEvent> getEvents();
        IScriptEvent getEvent(int ID);
        IScriptEvent getEvent(string Name);
    }

    public interface IScriptEditChampionhip : IScriptChampionship
    {
        new string ChampionshipName { set; }
    }
}

namespace ChampionshipSolutions.DM
{
    public partial class Championship : IScriptChampionship, IScriptEditChampionhip
    {
        public string ChampionshipName { get { return this.Name; } set { this.Name = value; } }

        public IScriptEvent getEventShortName ( string EventCode )
        {
            return Events.Where ( e => e.ShortName.ToUpper() == EventCode.ToUpper() ).FirstOrDefault<IScriptEvent> ( );
        }

        public IScriptEvent getEvent(string Name)
        {
            return Events.Where(e => e.Name == Name).FirstOrDefault<IScriptEvent>();
        }

        public IScriptEvent getEvent(int ID)
        {
            return Events.Where(e => e.ID == ID).FirstOrDefault<IScriptEvent>();
        }

        public ICollection<IScriptEvent> getEvents()
        {
            return listAllEvents().ToList<IScriptEvent>();
        }

    }
}
