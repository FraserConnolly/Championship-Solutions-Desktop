/*
 *  Filename         : School.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{

    public partial class SchoolTeams
    {
        //public int ID { get { return _ID; } set { _ID = value; } }

        public School School { get { return _School; } set { _School = value; } }

        public Team Team { get { return _Team; } set { _Team = value; } }
    }

    public partial class School : IIdentity
    {
        #region Names

        public string Name { get { return _Name ?? ""; } set { _Name = value; } }

        /// <summary>
        /// Get or Set the short name of this team.
        /// By default this is the first 4 characters without any spaces.
        /// </summary>
        public string ShortName
        {
            get
            {
                if ( _ShortName != null )
                    return _ShortName;

                if ( Name == null )
                    return null;

                if ( Name.Length > 4 )
                    return Name.Replace ( " " , string.Empty ).Substring ( 0 , 4 ).Trim ( );
                else
                    return Name.Trim ( );
            }
            set
            {
                _ShortName = value;
            }
        }

        #endregion

        public string LetterGreating { get { return _LetterGreating; } set { _LetterGreating = value; } }

        #region Constructors

        public School()  
        {
            DState = null;
        }

        public School(string SchoolName) : this ( )
        {
            this.Name = SchoolName; 
        }

        #endregion

        private SchoolTeams[] Teams
        {
            get
            {
                try { return _Teams.ToArray ( ); }
                catch { return new SchoolTeams[] { }; }
            }
        }

        public Team[] inTeams
        {
            get { return (from t in Teams select t.Team).ToArray(); }
        }

        public Team[] TeamsForChampionship (Championship Championship)
        {
            return ( from t in Teams where t.Team.Championship == Championship select t.Team ).ToArray ( );
        }

        public int CountSelected(Championship Championship)
        {
             return getSelectedForNextEvent(Championship).Count ;
        }

        public List<ACompetitor> getSelectedForNextEvent(Championship Championship)
        {
            List<ACompetitor> comps = new List<ACompetitor>();

            foreach (AEvent Event in Championship.listAllEvents())
            {
                foreach (ACompetitor EC in Event.EnteredCompetitors.Where(c => c.printParameter("Attends") == Name && c.SelectedForNextEvent))
                {
                    bool isSame = false;
                    foreach (ACompetitor C in comps)
                    {
                        if (C.isAthlete(((Competitor)EC).Athlete))
                        {
                            isSame = true;
                            break;
                        }
                    }
                    
                    if (!isSame)
                        comps.Add(EC);
                }
            }

            return comps;
        }

        #region Staff Details

        //public Person Head { get { return _Head; } set { _Head = value; } }

        //public Staff[] Staff { get { return _Staff.ToArray(); } }

        #endregion

        public override bool Equals(object obj)
        {
            try
            {
                return ((School)obj).ID == this.ID;
            }
            catch 
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name.ToString();
        }

        public static bool operator ==(School x, School y)
        {
            if (((object)x) == null && ((object)y) == null) return true ;
            if (((object)x) == null) return false;
            if (((object)y) == null) return false;
                
                return x.ID == y.ID;
        }

        public static bool operator !=(School x, School y)
        {
            return !(x == y);
        }

        public bool CanDelete ( )
        {
            return true;
        }
    }

}
