using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{

    public struct CertificateData
    {
        /// <summary>
        /// Used for the host application to determine which certificate template to use with this data
        /// </summary>
        public string CertifiacteType { get; set; }

        /// <summary>
        /// Used as a printable description for the host application
        /// </summary>
        public string CertificateName { get; set; }

        /// <summary>
        /// Printed onto the certificate in the Rank field
        /// </summary>
        public string Rank { get; set; }

        /// <summary>
        /// Printed onto the certificate in the Scoring Team field
        /// </summary>
        public string ScoringTeam { get; set; }
        
        /// <summary>
        /// Printed onto the certificate in the Championship field
        /// </summary>
        public string Championship { get; set; }

        /// <summary>
        /// Printed onto the certificate in the Tag line field
        /// </summary>
        public string ChampionshipTagLine { get; set; }

        /// <summary>
        /// Printed onto the certificate in the Event Name field
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Printed onto the certificate in the Competitors Name field
        /// </summary>
        public string CompetitorsName { get; set; }

        /// <summary>
        /// Printed onto the certificate in the School Name field
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// Printed onto the certificate in the Team Name field
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// Effectively gives page order when grouping certificates
        /// </summary>
        public int RankCounter { get; set; }

        public override string ToString()
        {
            return CompetitorsName;
        }

        public ACompetitor Competitor { get; set; }
    }

}
