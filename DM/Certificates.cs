using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Diagnostics;

namespace ChampionshipSolutions.DM
{
    public abstract partial class ACertificate //: IIdentity
    {
        #region Names

        string _Name, _ShortName;

        public string Name { get { return _Name; } set { _Name = value; } }

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

        public ACertificate ( ) { }

        public ACertificate ( string Description )
        {
            this.Name = Description;
        }

        //protected int Competitor_ID { get { return _Competitor_ID; } set { _Competitor_ID = value; } }

        //protected int Event_ID { get { return _Event_ID; } set { _Event_ID = value; } }

        //public int File_ID { get { return _File_ID; } set { _File_ID = value; } }

        public AEvent Event { get; set; }

        public FileStorage File { get; set; }

        public ACompetitor Competitor { get; set; }

        public static List<CertificateData> GenerateCertificates ( ACompetitor Competitor , AEvent Event )
        {
            // All error checking moved to hasEarnedCertificate() for efficacy.
            //if ( Event == null )
            //    throw new ArgumentNullException ( "Event can not be null when generating certificates" );

            //if ( Competitor == null )
            //    throw new ArgumentNullException ( "Competitor can not be null when generating certificates" );

            //if ( Competitor.CompetingIn != Event )
            //    throw new ArgumentException ( "Competitor must be competing for the Event used as a parameter" );

            //if ( Event.Results.Count ( ) == 0 )
            //    // There are not results to process certificates for
            //    return new List<CertificateData> ( );

            //if ( Event.Results.Where ( r => r.Competitor == Competitor ).Count ( ) == 0 )
            //    // This competitor does not appear in this events results
            //    return new List<CertificateData> ( );

            if ( Competitor?.Result?.CertificateEarned == true )
                return Event.getCertificateData ( ).Where ( c => c.Competitor == Competitor ).ToList ( );
            else
                return new List<CertificateData> ( );

        }

        #region Static GenerateCertificates Helpers

        public static List<CertificateData> GenerateCertificates ( ACompetitor Competitor )
        {
            return GenerateCertificates ( Competitor , Competitor.CompetingIn );
        }

        public static List<CertificateData> GenerateCertificates ( Athlete Athlete , Championship Championship = null )
        {
            List<CertificateData> temp = new List<CertificateData>();
            //List<Competitor> competitors = new List<Competitor>();
            List<ACompetitor> competitors = new List<ACompetitor>();

            if ( Championship == null )
                competitors.AddRange ( Athlete._Competitors );
            else
                competitors.AddRange ( Athlete.GetCompetitors ( Championship ) );


            foreach ( Competitor competitor in competitors )
                temp.AddRange ( GenerateCertificates ( competitor ) );

            return temp;
        }

        public static List<CertificateData> GenerateCertificates ( AEvent Event )
        {
            if ( !Event.IsFinal ) return new List<CertificateData> ( );

            List<CertificateData> temp;
            //List<CertificateData> temp = new List<CertificateData>();
            
            Stopwatch sw2 = new Stopwatch();
            sw2.Start ( );

            // generates certificate data based on individuals 
            //foreach ( ACompetitor Competitor in Event.getEnteredCompetitors ( ) )
            //    temp.AddRange ( GenerateCertificates ( Competitor , Event ) );


            temp = Event.getCertificateData ( );
            sw2.Stop ( );

            return temp;
        }

        public static List<CertificateData> GenerateCertificates ( Championship Championship )
        {
            List<CertificateData> temp = new List<CertificateData>();
            foreach ( AEvent Event in Championship.listAllEvents ( ) )
            {
                temp.AddRange ( ACertificate.GenerateCertificates ( Event ) );
            }
            return temp;
        }

        #endregion
    }


    public class CertificateInvididual : ACertificate
    {
        //public CertificateInvididual()
        //{
        //}
    }

    public class CertificateScoringTeam : ACertificate
    {
        //public CertificateScoringTeam()
        //{
        //}
    }
}
