/*
 *  Filename         : Helper.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes    :
 * 
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;


namespace ChampionshipSolutions.DM
{
    public static class StringHelper
    {
        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }

    public static class DateHelper
    {
        public static void DateDifference(DateTime? d1, DateTime? d2, out int day, out int month, out int year)
        {
            if (d1 == null || d2 == null)
            {
                throw new ArgumentOutOfRangeException("d1 or d2", "Input dates must not be null");
            }

            int increment;

            //int year, month, day = 0;

            DateTime fromDate;
            DateTime toDate;

            if (d1 > d2)
            {
                fromDate = (DateTime)d2;
                toDate = (DateTime)d1;
            }
            else
            {
                fromDate = (DateTime)d1;
                toDate = (DateTime)d2;
            }

            /// 
            /// Day Calculation
            /// 
            increment = 0;

            if (fromDate.Day > toDate.Day)
            {
                increment = monthDay[fromDate.Month - 1];

            }
            /// if it is february month
            /// if it's to day is less then from day
            if (increment == -1)
            {
                if (DateTime.IsLeapYear(fromDate.Year))
                {
                    // leap year february contain 29 days
                    increment = 29;
                }
                else
                {
                    increment = 28;
                }
            }
            if (increment != 0)
            {
                day = (toDate.Day + increment) - fromDate.Day;
                increment = 1;
            }
            else
            {
                day = toDate.Day - fromDate.Day;
            }

            ///
            ///month calculation
            ///
            if ((fromDate.Month + increment) > toDate.Month)
            {
                month = (toDate.Month + 12) - (fromDate.Month + increment);
                increment = 1;
            }
            else
            {
                month = (toDate.Month) - (fromDate.Month + increment);
                increment = 0;
            }

            ///
            /// year calculation
            ///
            year = toDate.Year - (fromDate.Year + increment);

        }

        static private int[] monthDay = new int[12] { 31, -1, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    }

    public static class EnumHelper
    {
        public static string[] GetValues(this Enum e)
        {
            List<string> enumNames = new List<string>();

            foreach (FieldInfo fi in e.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumNames.Add(fi.Name);
            }

            return enumNames.ToArray<string>();
        }
    }

    public static class DistanceHelper
    {

        public static int fromMetersToMillimeters(decimal  value)
        {
            return (int)Decimal.Truncate((value * 1000));
        }

        public static int fromCentimetersToMillimeters(decimal value)
        {
            return (int)Decimal.Truncate(value * 10);
        }

        public static decimal fromMillimetersToMeters(int value)
        {
            return (decimal)value / 1000;
        }

        public static decimal fromMillimetersToCentimeters(int value)
        {
            return (decimal)value / 10;
        }

        public static decimal TruncateDecimal(this decimal value, int decimalPlaces)
        {
            decimal integralValue = Decimal.Truncate(value);

            decimal fraction = value - integralValue;

            decimal factor = (decimal)Math.Pow(10, decimalPlaces);

            decimal truncatedFraction = Decimal.Truncate(fraction * factor) / factor;

            decimal result = integralValue + truncatedFraction;

            return result;
        }
    }

}
