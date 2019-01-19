/*
 *  Filename         : Identity.cs
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

namespace ChampionshipSolutions.DM
{
    public interface IIdentity : IID
    {

        string Name { get; set; }

        /// <summary>
        /// Get or Set the short name of this team.
        /// By default this is the first 4 characters without any spaces.
        /// </summary>
        string ShortName { get; set; }

    }

    public interface IID
    {
        int ID { get; set; }
        string Discriminator { get; set; }
        DataState DState { get; set; }
        void voidStorage ( bool softRefresh = true );
        void Save ( );
    }


}

