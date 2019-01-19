/*
 *  Filename         : Standards.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2015-05-02
 *  Copyright        : Fraser Connolly 2014
 *  Summery          : Stores multiple ResultValue to represent the various standards for an event.
 *                       
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{
    public partial class Standard : IIdentity
    {
        //public int ID { get { return _ID; } set { _ID = value; } }

        public Standard ()
        {
            DState = null;
        }

        #region Names

        public string Name { get { return _Name ?? ""; } set { _Name = value; Save ( ); } }

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
                _ShortName = value; Save ( );
            }
        }

        #endregion

        public ResultValue StandardValue
        {
            get
            {
                //if ( Event == null ) return null;
                // Adjusted for performance, now just checks for the event ID - hopefully this will prevent
                // the lazy loading from loading the whole event.
                // To do there is still a performance issue here.
                if ( _Event_ID == 0 ) return null;
                return new ResultValue ( Event.ResultsDisplayDescription , _RawValue );
            }
            set
            {
                if ( value != null )
                    _RawValue = value.RawValue;
                else
                    _RawValue = 0;

                Save ( );
            }
        }

        public AEvent Event { get { return _Event; } set { _Event = value; } }

        public string PrintStandard
        {
            get
            {
                if ( StandardValue == null ) return string.Empty;
                return StandardValue.ToString ( );
            }
            set
            {
                ResultValue rv = AEvent.MakeNewResultsValue(Event);
                rv.setResultString ( value );
                StandardValue = rv;// StandardValue.setResultString ( value ) );
            }
        }

        #region static methods

        /// <summary>
        /// Checks to see if there is a District, County, Entry, National Standard or Championship Best Performance to compare against.
        /// </summary>
        /// <returns>True if there is a standard to compare against</returns>
        public static bool hasStandards ( Standard standard )
        {
            if ( standard == null ) return false;

            if ( standard.StandardValue.HasValue ( ) ) return true;

            return false;
        }

        /// <summary>
        /// Compares a set of standards against a result
        /// </summary>
        /// <param name="standard">Standard to be compared against, can be null.</param>
        /// <param name="result">Result to be compared, can be null</param>
        /// <returns>Will always return false if standard is null</returns>
        public static bool achievedStandard ( AEvent Event , ResultValue result )
        {
            if ( result == null ) return false;

            if ( Event == null ) return false;

            if ( Event.CountyBestPerformance.HasValue ( ) )
            {
                if ( result.isTime ( ) )
                {
                    if ( Event.CountyBestPerformance.RawValue >= result.RawValue ) return true;
                }
                else if ( result.isDistance ( ) )
                {
                    if ( Event.CountyBestPerformance.RawValue <= result.RawValue ) return true;
                }
                else
                {
                    throw new ArgumentException ( "Result must have a declared ValueType" , "result" );
                }
            }

            foreach ( Standard standard in Event.Standards )
            {
                if ( standard == null ) return false;

                // will always be false if there are no standards
                if ( !hasStandards ( standard ) ) return false;

                // will always return false if there is no result to compare against
                if ( !result.HasValue ( ) ) return false;

                // TODO add a check for standards values to have a defined ValueType

                if ( result.isTime ( ) )
                {

                    if ( standard.StandardValue .HasValue ( ) )
                        if ( standard.StandardValue.RawValue >= result.RawValue ) return true;

                }
                else if ( result.isDistance ( ) )
                {
                    if ( standard.StandardValue.HasValue ( ) )
                        if ( standard.StandardValue.RawValue <= result.RawValue ) return true;
                }
                else
                {
                    throw new ArgumentException ( "Result must have a declared ValueType" , "result" );
                }
            }

            // did not achieve any standards
            return false;

        }


        /// <summary>
        /// Compares a set of standards against a result and produces a string of the achieved standards
        /// </summary>
        /// <param name="standard">Standard to be compared against, can be null.</param>
        /// <param name="result">Result to be compared, can be null</param>
        /// <returns>Will always return false if standard is null</returns>
        public static string getStandardShortString ( AEvent Event , ResultValue result )
        {
            if ( result == null ) return string.Empty;

            if ( Event == null ) return string.Empty;

                // will always return false if there is no result to compare against
            if ( result.HasValue ( ) == false ) return string.Empty;

            string temp = string.Empty;

            if ( Event.CountyBestPerformance.HasValue ( ) )
            {
                if ( result.isTime ( ) )
                {
                    if ( Event.CountyBestPerformance.RawValue >= result.RawValue )
                            temp += "CBP" + " / ";
                }
                else if ( result.isDistance ( ) )
                {
                    if ( Event.CountyBestPerformance.RawValue <= result.RawValue )
                            temp += "CBP" + " / ";
                }
                else
                {
                    throw new ArgumentException ( "Result must have a declared ValueType" , "result" );
                }
            }


            IEnumerable<Standard> standards = Event.Standards;

            if ( result.isTime( ) )
                standards = standards.OrderBy( s => s.StandardValue.RawValue );
            else if ( result.isDistance( ) )
                standards = standards.OrderByDescending( s => s.StandardValue.RawValue );
            else
                throw new ArgumentException( "Result must have a declared ValueType" , "result" );

            foreach ( Standard standard in standards )
            {

                if ( standard == null ) continue;

                // will always be false if there are no standards
                if ( !hasStandards( standard ) ) continue;

                // TODO add a check for standards values to have a defined ValueType

                if ( result.isTime( ) )
                {
                    // check for standard
                    if ( standard.StandardValue.HasValue( ) )
                        if ( standard.StandardValue.RawValue >= result.RawValue )
                        {
                            temp += standard.ShortName + " / ";
                            break;
                        }

                }
                else if ( result.isDistance( ) )
                {
                    // check for standard
                    if ( standard.StandardValue.HasValue( ) )
                        if ( standard.StandardValue.RawValue <= result.RawValue )
                        {
                            temp += standard.ShortName + " / ";
                            break;
                        }
                }
            }

            if ( temp.EndsWith ( " / " ) )
                return temp.Remove ( temp.Length - 3 , 3 );
            else
                return temp;

        }


        internal static void CopyStandards ( AEvent aEvent , Standard[] standards )
        {
            foreach ( Standard std in standards )
                aEvent.addStandard ( new Standard ( ) { _RawValue = std._RawValue } );
        }


        //internal static Standard CopyStandard ( Standard standard )
        //{
        //    if ( standard == null )
        //        return null;

        //    return new Standard ( )
        //    {
        //        Event = standard.Event ,
        //        StandardValue = standard.StandardValue
        //    };
        //}
    }

    #endregion


    // Legacy Standards Class from Pre V3-0
    //    public partial class Standards : IID
    //    {
    //        public Standards()
    //        {
    //            DistricStandard =       new ResultValue();
    //            CountyStandard =        new ResultValue();
    //            EntryStandard =         new ResultValue();
    //            NationalStandard =      new ResultValue();
    //            CountyBestPerformance = new ResultValue();
    //        }

    //        public int ID { get { return _ID; } set { _ID = value; } }

    //        public const string NATIONALSTDDESC = "National Standard";
    //        public const string ENTRYSTDDESC = "Entry Standard";
    //        public const string COUNTYSTDDESC = "County Standard";
    //        public const string DISTRICTSTDDESC = "District Standard";

    //        public const string NATIONALSTDSHORT = "NS";
    //        public const string ENTRYSTDSHORT = "ES";
    //        public const string COUNTYSTDSHORT = "CS";
    //        public const string DISTRICTSTSHORT = "DS";

    //        public const string CHAMPBPDESC = "Championship Best Performance";
    //        public const string CHAMPBPSHORT = "CBP";


    //        public ResultValue NationalStandard
    //        {
    //            get
    //            {
    //                return new ResultValue()
    //                {
    //                    RawValue = _NationalStandard_RawValue,
    //                    ValueType = (ResultDisplayDescription)_NationalStandard_ValueType
    //                };
    //            }
    //            set
    //            {
    //                _NationalStandard_RawValue = value.RawValue;
    //                _NationalStandard_ValueType = (int)value.ValueType;
    //            }
    //        }
    //        public ResultValue EntryStandard 
    //        {
    //            get
    //            {
    //                return new ResultValue()
    //        {
    //            RawValue = _EntryStandard_RawValue,
    //                    ValueType = (ResultDisplayDescription)_EntryStandard_ValueType
    //                };
    //    }
    //    set
    //            {
    //                _EntryStandard_RawValue = value.RawValue;
    //                _EntryStandard_ValueType = (int)value.ValueType;
    //            }
    //        }
    //        public ResultValue CountyStandard 
    //        {
    //            get
    //            {
    //                return new ResultValue()
    //{
    //    RawValue = _CountyStandard_RawValue,
    //                    ValueType = (ResultDisplayDescription)_CountyStandard_ValueType
    //                };
    //            }
    //            set
    //            {
    //                _CountyStandard_RawValue = value.RawValue;
    //                _CountyStandard_ValueType = (int)value.ValueType;
    //            }
    //        }
    //        public ResultValue DistricStandard
    //        {
    //            get
    //            {
    //                return new ResultValue()
    //                {
    //                    RawValue = _DistricStandard_RawValue,
    //                    ValueType = (ResultDisplayDescription)_DistricStandard_ValueType
    //                };
    //            }
    //            set
    //            {
    //                _DistricStandard_RawValue = value.RawValue;
    //                _DistricStandard_ValueType = (int)value.ValueType;
    //            }
    //        }
    //        public ResultValue CountyBestPerformance
    //        {
    //            get
    //            {
    //                return new ResultValue()
    //                {
    //                    RawValue = _CountyBestPerformance_RawValue,
    //                    ValueType = (ResultDisplayDescription)_CountyBestPerformance_ValueType
    //                };
    //            }
    //            set
    //            {
    //                _CountyBestPerformance_RawValue = value.RawValue;
    //                _CountyBestPerformance_ValueType = (int)value.ValueType;
    //            }
    //        }

    //        public string CountyBestPerformanceName { get { return _CountyBestPerformanceName; } set { _CountyBestPerformanceName = value; } }

    //        public int CountyBestPerformanceYear { get { return _CountyBestPerformanceYear; } set { _CountyBestPerformanceYear = value; } }

    //        public string CountyBestPerformanceArea { get { return _CountyBestPerformanceArea; } set { _CountyBestPerformanceArea = value; } }

    //        /// <summary>
    //        /// Checks to see if there is a District, County, Entry, National Standard or Championship Best Performance to compare against.
    //        /// </summary>
    //        /// <returns>True if there is a standard to compare against</returns>
    //        public static bool hasStandards(Standards standard)
    //        {
    //            if (standard == null) return false;

    //            if (standard.DistricStandard.HasValue()) return true;
    //            if (standard.CountyStandard.HasValue()) return true;
    //            if (standard.EntryStandard.HasValue()) return true;
    //            if (standard.NationalStandard.HasValue()) return true;
    //            if (standard.CountyBestPerformance.HasValue()) return true;

    //            return false;
    //        }

    //        /// <summary>
    //        /// Compares a set of standards against a result
    //        /// </summary>
    //        /// <param name="standard">Standard to be compared against, can be null.</param>
    //        /// <param name="result">Result to be compared, can be null</param>
    //        /// <returns>Will always return false if standard is null</returns>
    //        public static bool achievedStandard(Standards standard, ResultValue result)
    //        {
    //            if(standard == null) return false;
    //            if(result == null) return false;

    //            // will always be false if there are no standards
    //            if (!hasStandards(standard)) return false;

    //            // will always return false if there is no result to compare against
    //            if (!result.HasValue()) return false;

    //            // TODO add a check for standards values to have a defined ValueType

    //            if (result.isTime())
    //            {

    //                // check for district standard
    //                if (standard.DistricStandard.HasValue())
    //                    if (standard.DistricStandard.RawValue >= result.RawValue) return true;

    //                // check for county standard
    //                if (standard.CountyStandard.HasValue())
    //                    if (standard.CountyStandard.RawValue >= result.RawValue) return true;

    //                // check for entry standard
    //                if (standard.EntryStandard.HasValue())
    //                    if (standard.EntryStandard.RawValue >= result.RawValue) return true;

    //                // check for national standard
    //                if (standard.NationalStandard.HasValue())
    //                    if (standard.NationalStandard.RawValue >= result.RawValue) return true;

    //                // check for championship best performance
    //                if (standard.CountyBestPerformance.HasValue())
    //                    if (standard.CountyBestPerformance.RawValue >= result.RawValue) return true;
    //            }
    //            else if (result.isDistance())
    //            {
    //                // check for district standard
    //                if (standard.DistricStandard.HasValue())
    //                    if (standard.DistricStandard.RawValue <= result.RawValue) return true;

    //                // check for county standard
    //                if (standard.CountyStandard.HasValue())
    //                    if (standard.CountyStandard.RawValue <= result.RawValue) return true;

    //                // check for entry standard
    //                if (standard.EntryStandard.HasValue())
    //                    if (standard.EntryStandard.RawValue <= result.RawValue) return true;

    //                // check for national standard
    //                if (standard.NationalStandard.HasValue())
    //                    if (standard.NationalStandard.RawValue <= result.RawValue) return true;

    //                // check for championship best performance
    //                if (standard.CountyBestPerformance.HasValue())
    //                    if (standard.CountyBestPerformance.RawValue <= result.RawValue) return true;
    //            }
    //            else
    //            {
    //                throw new ArgumentException("Result must have a declared ValueType", "result");
    //            }

    //            // did not achieve any standards
    //            return false;

    //        }


    //        /// <summary>
    //        /// Compares a set of standards against a result and produces a string of the achieved standards
    //        /// </summary>
    //        /// <param name="standard">Standard to be compared against, can be null.</param>
    //        /// <param name="result">Result to be compared, can be null</param>
    //        /// <returns>Will always return false if standard is null</returns>
    //        public static string getStandardShortString(Standards standard, ResultValue result)
    //        {
    //            if (standard == null) return string.Empty;
    //            if (result == null) return string.Empty;

    //            // will always be false if there are no standards
    //            if (!hasStandards(standard)) return string.Empty;

    //            // will always return false if there is no result to compare against
    //            if (!result.HasValue()) return string.Empty;

    //            // TODO add a check for standards values to have a defined ValueType

    //            string temp = string.Empty;

    //            if (result.isTime())
    //            {
    //                // check for district standard
    //                if (standard.DistricStandard.HasValue())
    //                    if (standard.DistricStandard.RawValue >= result.RawValue)
    //                        temp += Standards.DISTRICTSTSHORT + " / ";

    //                // check for county standard
    //                if (standard.CountyStandard.HasValue())
    //                    if (standard.CountyStandard.RawValue >= result.RawValue)
    //                        temp += Standards.COUNTYSTDSHORT + " / ";

    //                // check for entry standard
    //                if (standard.EntryStandard.HasValue())
    //                    if (standard.EntryStandard.RawValue >= result.RawValue)
    //                        temp += Standards.ENTRYSTDSHORT + " / ";

    //                // check for national standard
    //                if (standard.NationalStandard.HasValue())
    //                    if (standard.NationalStandard.RawValue >= result.RawValue)
    //                        temp += Standards.NATIONALSTDSHORT + " / ";

    //                // check for championship best performance
    //                if (standard.CountyBestPerformance.HasValue())
    //                    if (standard.CountyBestPerformance.RawValue >= result.RawValue)
    //                        temp += Standards.CHAMPBPSHORT + " / ";

    //                if (temp.EndsWith(" / "))
    //                    return temp.Remove(temp.Length - 3, 3);
    //                else
    //                    return temp;
    //            }
    //            else if (result.isDistance())
    //            {
    //                // check for district standard
    //                if (standard.DistricStandard.HasValue())
    //                    if (standard.DistricStandard.RawValue <= result.RawValue)
    //                        temp += Standards.DISTRICTSTSHORT + " / ";

    //                // check for county standard
    //                if (standard.CountyStandard.HasValue())
    //                    if (standard.CountyStandard.RawValue <= result.RawValue)
    //                        temp += Standards.COUNTYSTDSHORT + " / ";

    //                // check for entry standard
    //                if (standard.EntryStandard.HasValue())
    //                    if (standard.EntryStandard.RawValue <= result.RawValue)
    //                        temp += Standards.ENTRYSTDSHORT + " / ";

    //                // check for national standard
    //                if (standard.NationalStandard.HasValue())
    //                    if (standard.NationalStandard.RawValue <= result.RawValue)
    //                        temp += Standards.NATIONALSTDSHORT + " / ";

    //                // check for championship best performance
    //                if (standard.CountyBestPerformance.HasValue())
    //                    if (standard.CountyBestPerformance.RawValue <= result.RawValue)
    //                        temp += Standards.CHAMPBPSHORT + " / ";

    //                if (temp.EndsWith(" / "))
    //                    return temp.Remove(temp.Length - 3, 3);
    //                else
    //                    return temp;

    //            }
    //            else
    //            {
    //                throw new ArgumentException("Result must have a declared ValueType", "result");
    //            }

    //        }


    //        internal static Standards CopyStandards(Standards standards)
    //        {
    //            if (standards == null)
    //                return null;

    //            return new Standards()
    //            {
    //                CountyBestPerformanceArea = standards.CountyBestPerformanceArea,
    //                CountyBestPerformanceName = standards.CountyBestPerformanceName,
    //                CountyBestPerformanceYear = standards.CountyBestPerformanceYear,
    //                DistricStandard = new ResultValue(standards.DistricStandard),
    //                CountyStandard = new ResultValue(standards.CountyStandard),
    //                EntryStandard = new ResultValue(standards.EntryStandard),
    //                NationalStandard = new ResultValue(standards.NationalStandard),
    //                CountyBestPerformance = new ResultValue(standards.CountyBestPerformance)
    //            };
    //        }
    //    }

}






