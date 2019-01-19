/*
 *  Filename         : Results.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 *    2015-05-02
 *      Added Result Display Description enum
 *      Moved existing Results sub classes to legacy commented code
 *      Added ResultValue class
 *
 *    2016-03-25
 *      Split into TResults and Results so the SQLite logic is seperate
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Itenso.TimePeriod;
using System.Globalization;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    /// <summary>
    /// Defines how much information is available within this results object
    /// </summary>
    public enum ResultTypeDescription
    {
        Competative,            // Vest no,     Not timed
        CompetativeWithValue,   // vest no,     Valued
        CompetativeDNF,         // Vest no,     Not timed
        ValuePlaceholder,       // no vest no,  Valued
        Placeholder             // no vest no,  Not timed
    }

    /// <summary>
    /// Defines whether the result is a distance or time as well as how it is displayed.
    /// </summary>
    /// If you make a change here you must also change it in the Database 
    /// and in the NewEvent Window.
    public enum ResultDisplayDescription
    {
        NotDeclared,                   // 0 // avoid using
        TimedMinuetsSeconds,           // 1 // m:ss     or mm:ss
        TimedMinuetsSecondsTenths,     // 2 // m:ss.t   or mm:ss.t
        TimedMinuetsSecondsHundreds,   // 3 // m:ss.hh  or mm:ss.hh
        TimedSecondsTenths,            // 4 // s.t      or ss.t
        TimedSecondsHundreds,          // 5 // s.hh     or ss.hh
        DistanceMeters,                // 6 // 0m       0 decimal places
        DistanceMetersCentimeters,     // 7 // 0.00m    2 decimal places
        DistanceCentimeters,           // 8 // 0cm      0 decimal places
        DNF                            // 9 // Did not finish
    }

    public class ResultValue
    {
        /// <summary>
        /// For a timed result this is stored as milliseconds
        /// For a distance result this is stored as millimetres
        /// </summary>
        public int RawValue { get; set; }

        public ResultDisplayDescription ValueType { get; set; }

        #region Constructors
        public ResultValue()
        {
        }

        public ResultValue(ResultDisplayDescription ValueType)
        {
            this.ValueType = ValueType;
        }

        public ResultValue(ResultValue rv)
        {
            this.RawValue = rv.RawValue;
            this.ValueType = rv.ValueType;
        }

        /// <summary>
        /// Constructor for timed results
        /// </summary>
        /// <param name="ValueType"></param>
        /// <param name="value"></param>
        public ResultValue(ResultDisplayDescription ValueType, TimeSpan value)
        {
            this.ValueType = ValueType;
            setTime(value);
        }

        /// <summary>
        /// Constructor for distance results
        /// </summary>
        /// <param name="ValueType"></param>
        /// <param name="value"></param>
        public ResultValue(ResultDisplayDescription ValueType, decimal value)
        {
            this.ValueType = ValueType;
            setDistance(value);
        }

        /// <summary>
        /// Constructor for any results
        /// </summary>
        /// <param name="ValueType"></param>
        /// <param name="RawValue"></param>
        public ResultValue ( ResultDisplayDescription ValueType , int RawValue )
        {
            this.ValueType = ValueType;
            this.RawValue = RawValue ;
        }

        #endregion

        public bool isDistance()
        {
            return (
               ValueType == ResultDisplayDescription.DistanceCentimeters ||
               ValueType == ResultDisplayDescription.DistanceMeters ||
               ValueType == ResultDisplayDescription.DistanceMetersCentimeters
               );
        }

        public bool isTime()
        {
            return (
                ValueType == ResultDisplayDescription.TimedMinuetsSeconds ||
                ValueType == ResultDisplayDescription.TimedMinuetsSecondsHundreds ||
                ValueType == ResultDisplayDescription.TimedMinuetsSecondsTenths ||
                ValueType == ResultDisplayDescription.TimedSecondsHundreds ||
                ValueType == ResultDisplayDescription.TimedSecondsTenths
                );
        }

        public void setTime(TimeSpan time)
        {
            if (isTime())
            {
                RawValue = (int)time.TotalMilliseconds;
                return;
            }

            throw new ArgumentException("Can not set a time for a distance result");
        }

        public void setDistance(decimal distance)
        {
            if (isDistance())
            {
                switch (ValueType)
                {
                    case ResultDisplayDescription.DistanceMeters:
                        RawValue = DistanceHelper.fromMetersToMillimeters(distance);
                        return;
                    case ResultDisplayDescription.DistanceMetersCentimeters:
                        RawValue = DistanceHelper.fromMetersToMillimeters(distance);
                        return;
                    case ResultDisplayDescription.DistanceCentimeters:
                        RawValue = DistanceHelper.fromCentimetersToMillimeters(distance);
                        return;
                    default:
                        throw new ArgumentException("Unknown result type");
                }
            }
            else
            {
                throw new ArgumentException("Can not set distance on a timed result");
            }
        }

        public void setResult(int Result)
        {
            RawValue = Result;
        }

        public string getResultString()
        {
            if (!HasValue()) return string.Empty;

            TimeSpan ts;

            // To Do when converting from Portable Library to Class Library the Math.Floor function caused an ambiguity between double and decimal.
            // As such I have explicitly case Second and Minuet and Millisecond to decimal

            switch (ValueType)
            {
                case ResultDisplayDescription.TimedMinuetsSeconds:
                    ts = TimeSpan.FromMilliseconds(RawValue);
                    return string.Format("{0}:{1:00}", Math.Floor(ts.TotalMinutes), Math.Floor((decimal)ts.Seconds));

                case ResultDisplayDescription.TimedMinuetsSecondsTenths:
                    ts = TimeSpan.FromMilliseconds(RawValue);
                    return string.Format("{0}:{1:00}.{2:0}", Math.Floor(ts.TotalMinutes), Math.Floor((decimal)ts.Seconds), Math.Floor((decimal)ts.Milliseconds / 100));

                case ResultDisplayDescription.TimedMinuetsSecondsHundreds:
                    ts = TimeSpan.FromMilliseconds(RawValue);
                    return string.Format("{0}:{1:00}.{2:00}", Math.Floor(ts.TotalMinutes), Math.Floor((decimal)ts.Seconds), Math.Floor((decimal)ts.Milliseconds / 10));

                case ResultDisplayDescription.TimedSecondsTenths:
                    ts = TimeSpan.FromMilliseconds(RawValue);
                    return string.Format("{0}.{1:0}s", Math.Floor(ts.TotalSeconds), Math.Floor((decimal)ts.Milliseconds / 100));

                case ResultDisplayDescription.TimedSecondsHundreds:
                    ts = TimeSpan.FromMilliseconds(RawValue);
                    return string.Format("{0}.{1:00}s", Math.Floor(ts.TotalSeconds), Math.Floor((decimal)ts.Milliseconds / 10));

                case ResultDisplayDescription.DistanceMeters:
                    return string.Format("{0:0}m", Decimal.Truncate((Decimal)(DistanceHelper.fromMillimetersToMeters(RawValue))));

                case ResultDisplayDescription.DistanceMetersCentimeters:
                    return string.Format("{0:0.00}m", DistanceHelper.TruncateDecimal(DistanceHelper.fromMillimetersToMeters(RawValue), 2));

                case ResultDisplayDescription.DistanceCentimeters:
                    return string.Format("{0:0}cm", Decimal.Truncate(DistanceHelper.fromMillimetersToCentimeters(RawValue)));





                //case ResultDisplayDescription.TimedMinuetsSeconds:
                //    ts = TimeSpan.FromMilliseconds(RawValue);
                //    return string.Format("{0}:{1:00}", Math.Floor(ts.TotalMinutes), Math.Floor(ts.Seconds));

                //case ResultDisplayDescription.TimedMinuetsSecondsTenths:
                //    ts = TimeSpan.FromMilliseconds(RawValue);
                //    return string.Format("{0}:{1:00}.{2:0}", Math.Floor(ts.TotalMinutes), Math.Floor(ts.Seconds), Math.Floor(ts.Milliseconds / 100));

                //case ResultDisplayDescription.TimedMinuetsSecondsHundreds:
                //    ts = TimeSpan.FromMilliseconds(RawValue);
                //    return string.Format("{0}:{1:00}.{2:00}", Math.Floor(ts.TotalMinutes), Math.Floor(ts.Seconds), Math.Floor(ts.Milliseconds / 10));

                //case ResultDisplayDescription.TimedSecondsTenths:
                //    ts = TimeSpan.FromMilliseconds(RawValue);
                //    return string.Format("{0}.{1:0}s", Math.Floor(ts.TotalSeconds), Math.Floor(ts.Milliseconds / 100));

                //case ResultDisplayDescription.TimedSecondsHundreds:
                //    ts = TimeSpan.FromMilliseconds(RawValue);
                //    return string.Format("{0}.{1:0}s", Math.Floor(ts.TotalSeconds), Math.Floor(ts.Milliseconds / 10));

                //case ResultDisplayDescription.DistanceMeters:
                //    return string.Format("{0:0}m", Decimal.Truncate((Decimal)(DistanceHelper.fromMillimetersToMeters(RawValue))));

                //case ResultDisplayDescription.DistanceMetersCentimeters:
                //    return string.Format("{0:0.00}m", DistanceHelper.TruncateDecimal( DistanceHelper.fromMillimetersToMeters(RawValue),2));

                //case ResultDisplayDescription.DistanceCentimeters:
                //    return string.Format("{0:0}cm", Decimal.Truncate(DistanceHelper.fromMillimetersToCentimeters(RawValue)));

                default:
                    throw new ArgumentNullException("Value Type", "Value type not recognised");
            }
        }

        /// <summary>
        /// Set Results from a string in a variety of different formats depending on ResultDisplayDescription.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool setResultString(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            bool setSuccessful = false;
            TimeSpan ts;
            decimal d;

            // To Do when converting from Portable Library to Class Library the Math.Floor function caused an ambiguity between double and decimal.
            // As such I have explicitly case Second and Minuet and Millisecond to decimal

            switch (ValueType)
            {
                case ResultDisplayDescription.TimedMinuetsSeconds:
                    if (tryConvertMinuetSecondsToTimeSpan(value, out ts))
                    {
                        setTime(ts);
                        setSuccessful = true;
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("Failed to convert time " + value + " to TimedMinuetsSeconds");

                    break;
                case ResultDisplayDescription.TimedMinuetsSecondsTenths:
                    if (tryConvertMinuetSecondsToTimeSpan(value, out ts))
                    {
                        setTime(ts);
                        setSuccessful = true;
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("Failed to convert time " + value + " to TimedMinuetsSecondsTenths");

                    break;
                case ResultDisplayDescription.TimedMinuetsSecondsHundreds:
                    if (tryConvertMinuetSecondsToTimeSpan(value, out ts))
                    {
                        setTime(ts);
                        setSuccessful = true;
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("Failed to convert time " + value + " to TimedMinuetsSecondsHundreds");

                    break;
                case ResultDisplayDescription.TimedSecondsTenths:
                    if (tryConvertSecondsToTimeSpan(value, out ts))
                    {
                        setTime(ts);
                        setSuccessful = true;
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("Failed to convert time " + value + " to TimedSecondsTenths");

                    break;
                case ResultDisplayDescription.TimedSecondsHundreds:
                    if ( value.EndsWith ( "s" ) ) value = value.TrimEnd ( 's' );
                    if (tryConvertSecondsToTimeSpan(value, out ts))
                    {
                        setTime(ts);
                        setSuccessful = true;
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("Failed to convert time " + value + " to TimedSecondsHundres");

                    break;
                case ResultDisplayDescription.DistanceMeters:
                    if (decimal.TryParse(value, out d))
                    {
                        setDistance(d);
                        setSuccessful = true;
                    }
                    break;
                case ResultDisplayDescription.DistanceMetersCentimeters:
                    if ( value.EndsWith ( "m" ) ) value = value.TrimEnd ( 'm' );
                    if (decimal.TryParse(value, out d))
                    {
                        setDistance(d);
                        setSuccessful = true;
                    }

                    break;
                case ResultDisplayDescription.DistanceCentimeters:
                    if (decimal.TryParse(value, out d))
                    {
                        setDistance(d);
                        setSuccessful = true;
                    }

                    break;
                default:
                    throw new ArgumentNullException("Value Type", "Value type not recognised");
            }



#if (DEBUG)
            if (!this.HasValue())
                throw new ArgumentException("No Value was stored");
#endif

            return setSuccessful;
        }

        /// <summary>
        /// Converts time stored as a string of seconds i.e. '12.3' seconds or '12.34' seconds
        /// </summary>
        /// <param name="value">String representation of time.</param>
        /// <param name="ts"></param>
        /// <returns>True if the time span was created successfully.</returns>
        private bool tryConvertSecondsToTimeSpan(string value, out TimeSpan ts)
        {
            double d;

            // Seconds must be convertible to double
            if (!double.TryParse(value, out d))
            {
                ts = new TimeSpan();
                return false;
            }

            // Convert decimal to TimeSpan
            try
            {
                ts = TimeSpan.FromSeconds(d);
                return true;
            }
            catch (Exception)
            {
                ts = new TimeSpan();
                return false;
            }
        }

        /// <summary>
        /// Converts time stored as a string of minuets and seconds i.e. '12:34.5' seconds or '12:34.56' seconds
        /// </summary>
        /// <param name="value">String representation of time.</param>
        /// <param name="ts"></param>
        /// <returns>True if the time span was created successfully.</returns>
        private bool tryConvertMinuetSecondsToTimeSpan(string value, out TimeSpan ts)
        {
            if (TimeSpan.TryParseExact(value, @"m\:ss", CultureInfo.CurrentCulture, out ts))
                return true;

            if (TimeSpan.TryParseExact(value, @"m\:ss\.f", CultureInfo.CurrentCulture, out ts))
                return true;

            if (TimeSpan.TryParseExact(value, @"m\:ss\.ff", CultureInfo.CurrentCulture, out ts))
                return true;

            return false;
        }

        public bool HasValue()
        {
            return RawValue > 0;
        }

        public override string ToString()
        {
            return getResultString();
        }

    }

    public abstract partial class AResult
    {
        /// <summary>
        /// For use by Entity Frameworks only
        /// </summary>
        public AResult ( ) { DState = null; }

        public AEvent Event { get { return _Event; } set { _Event = value; } }

        /// <summary>
        /// Can be null 
        /// </summary>
        public int? Rank { get { return _Rank; } set { _Rank = value; } }

        //private int? Competitor_ID { get { return _Competitor_ID; } set { _Competitor_ID = value; } }

        public ACompetitor Competitor { get { return _Competitor; } set { _Competitor = value; } }

        public virtual VestNumber VestNumber
        {
            get
            {
                if (Competitor != null)
                    return Competitor.Vest;
                else
                    return new VestNumber() { dbVestNumber = _vestNumberDB };
            }
            set
            {
                if (Competitor == null)
                    _vestNumberDB = value.dbVestNumber;
                else
                    throw new NotImplementedException("Can not set vest number here");
            }
        }

        public ResultTypeDescription TypeDescriminator
        {
            get
            {

                if (this.Competitor == null)
                    if (HasValue())
                        return ResultTypeDescription.ValuePlaceholder;
                    else
                        return ResultTypeDescription.Placeholder;
                else
                    if (HasValue())
                        if (Value.ValueType == ResultDisplayDescription.DNF)
                            return ResultTypeDescription.CompetativeDNF;
                        else
                            return ResultTypeDescription.CompetativeWithValue;
                    else
                        return ResultTypeDescription.Competative;

            }
            //set { _TypeDescriminator = (int)value; }
        }

        #region Result Value 

        public ResultValue Value
        {
            get
            {
                return new ResultValue()
                {
                    RawValue = _Value_RawValue,
                    ValueType = (ResultDisplayDescription)_Value_ValueType
                };
            }
            set
            {
                _Value_RawValue = value.RawValue;
                _Value_ValueType = (int)value.ValueType;
            }
        }

        public bool HasValue()
        {
            if ( Value == null ) return false;

            return Value.HasValue();
        }

        #endregion

        public abstract string printResult();
        public virtual string printResultValueString()
        {
            if (Value != null)
                return Value.getResultString();
            else
                return string.Empty;
        }
        public abstract string printVestNo();
        public abstract string printParameter(string Parameter);
        public abstract string printName();
        public virtual string printRankAndResult()
        {
            if (printRank != "DNF")
                return IntToString(Rank.Value) + " " + printResultValueString();
            else
                return printRank + " " + printResultValueString();
        }
        public virtual string printTeam() { return string.Empty; }
        public virtual string printTeamShort() { return string.Empty; }
        public virtual ResultTypeDescription getTypeDescription() { return TypeDescriminator; }
        public virtual bool isPlaceholder() { return TypeDescriminator == ResultTypeDescription.Placeholder; }

        #region Read-Only Properties
        
        public string printRank
        {
            get
            {
                if (Rank.HasValue)
                    return Rank.Value.ToString();
                return "DNF";
            }
        }
        public string printResultValue
        {
            get
            {
                return printResultValueString();
            }
        }
        public string achievedStandardsShort
        {
            get
            {
                return Event.getStandardShortString(this.Value) ;
            }
        }
        public string printNameStr { get { return printName(); } }

        #endregion

        /// <summary>
        /// Returns true if this result is not a DNF record
        /// </summary>
        public virtual bool isComplete() { return TypeDescriminator != ResultTypeDescription.CompetativeDNF ; }
        public virtual int getRank()
        {
            if (Rank.HasValue)
                return Rank.Value;
            return int.MaxValue;
        }

        protected virtual bool _CertificateEarned(){ return false; }
        public virtual bool CertificateEarned { get { return _CertificateEarned(); } }

        public static string IntToString(int Integer)
        {
            switch (Integer)
            {
                case 1:
                    return "First";
                case 2:
                    return "Second";
                case 3:
                    return "Third";
                case 4:
                    return "Fourth";
                case 5:
                    return "Fifth";
                case 6:
                    return "Sixth";
                default:
                    return Integer.ToString();
            }
        }

        public static ResultDisplayDescription getResultsDisplayDescription(string DisplayDescription)
        {
            switch (DisplayDescription)
            {
                default:
                    return ResultDisplayDescription.NotDeclared;
                case "TimedMinuetsSeconds":
                    return ResultDisplayDescription.TimedMinuetsSeconds;
                case "TimedMinuetsSecondsTenths":
                    return ResultDisplayDescription.TimedMinuetsSecondsTenths;
                case "TimedMinuetsSecondsHundreds":
                    return ResultDisplayDescription.TimedMinuetsSecondsHundreds;
                case "TimedSecondsTenths":
                    return ResultDisplayDescription.TimedSecondsTenths;
                case "TimedSecondsHundreds":
                    return ResultDisplayDescription.TimedSecondsHundreds;
                case "DistanceMeters":
                    return ResultDisplayDescription.DistanceMeters;
                case "DistanceMetersCentimeters":
                    return ResultDisplayDescription.DistanceMetersCentimeters;
                case "DistanceCentimeters":
                    return ResultDisplayDescription.DistanceCentimeters;
            }
        }
    }

    public class Result : AResult
    {

        // FC 2016-01-10 do not need to store typeDescriminator as it can be worked out on the fly
        //public Result(ResultTypeDescription Type) { TypeDescriminator = Type; }

        public override string printResult()
        {
            switch (TypeDescriminator)
            {
                case ResultTypeDescription.Competative:
                    return Competitor.getName();
                case ResultTypeDescription.CompetativeWithValue:
                    return Competitor.getName() + " " + base.printResultValueString();
                case ResultTypeDescription.CompetativeDNF:
                    return Competitor.getName() + " DNF";
                case ResultTypeDescription.ValuePlaceholder:
                    return "Placeholder " + printResultValueString();
                case ResultTypeDescription.Placeholder:
                    return "Placeholder";
                default:
                    return string.Empty;
            }

        }

        public override string printVestNo()
        {
            if (Competitor != null)
            {
                return Competitor.printVestNumber();
            }
            else if (VestNumber != null)
            {
                return VestNumber.printVestString;
            }
            else
            {
                return string.Empty;
            }
        }

        public override string printParameter(string Parameter)
        {
            if (Competitor == null)
                return string.Empty ;

            object obj = Competitor.checkParameter(Parameter);

            if (obj == null)
                return string.Empty;

            return obj.ToString();
        }

        public override string printName()
        {
            switch (TypeDescriminator)
            {
                case ResultTypeDescription.Competative:
                    return Competitor.getName();
                case ResultTypeDescription.CompetativeWithValue:
                    return Competitor.getName();
                case ResultTypeDescription.CompetativeDNF:
                    return Competitor.getName();
                case ResultTypeDescription.ValuePlaceholder:
                    return "Placeholder";
                case ResultTypeDescription.Placeholder:
                    return "Placeholder";
                default:
                    return string.Empty;
            }

        }

        protected override bool _CertificateEarned()
        {
            if (this.TypeDescriminator == ResultTypeDescription.CompetativeWithValue || this.TypeDescriminator == ResultTypeDescription.Competative)
            {
                //List<CertificateData> cd = Event.getCertificateData();

                //cd = cd.Where(c => c.Competitor == this.Competitor).ToList();

                //if (cd.Count > 0)
                //    return true;

                return Event.hasEarnedCertificate ( Competitor );
            }

            return false;
        }

        public override string printTeam()
        {
            if (Competitor != null)
                return Competitor.printTeam;

            return base.printTeam();
        }

        public override string printTeamShort()
        {
            if (Competitor != null)
                return Competitor.getTeam().ShortName;

            return base.printTeamShort();
        }

    }

}
