using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{
    public class SpecialConsideration : StudentCompetitor 
    {
        // force vest to always be SC
        public override VestNumber Vest
        {
            get
            {
                return new VestNumber() { dbVestNumber = "SC" };
            }
            set
            {
                base.Vest = new VestNumber() { dbVestNumber = "SC" };
            }
        }

        public override bool Guest
        {
            get
            {
                return true;
            }
        }
    }
}
