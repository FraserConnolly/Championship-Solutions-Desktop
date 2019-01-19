using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM
{
    public partial class Staff : IID
    {
        //public int ID { get { return _ID; } set { _ID = value; } }

        public Person Person { get { return _Person; } set { _Person = value; } }

        public School School { get { return _School; } set { _School = value; } }

        public string Title { get { return _Title; } set { _Title = value; } }
    }
}
