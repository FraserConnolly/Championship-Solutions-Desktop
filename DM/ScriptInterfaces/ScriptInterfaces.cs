using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM.ScriptInterfaces
{

    public interface IScriptTeam
    {
        int ID { get; }
        string Name { get; }
        ICollection<IScriptSchool> HasSchools { get; }
    }

    public interface IScriptSchool
    {
        int ID { get; }
        string Name { get; }
        string HeadTeacherName { get; }

        ICollection<IScriptTeam> inTeams { get; }
        ICollection<IScriptContactDetail> getAllContacts();
    }

    public interface IScriptContactDetail
    {

    }
}
