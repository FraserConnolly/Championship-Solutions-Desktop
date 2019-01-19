/*
 *  Filename         : VestNumber.cs
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
    public class VestNumber : IComparable 
    {
        public static VestNumber MakeFromString(string vestNumber)
        {
            VestNumber v = new VestNumber();
            v.setVestNumber(vestNumber);
            return v;
        }

        public string dbVestNumber { get { return toDbString(); } set { setVestNumber(value); } }

        public string printVestString { get { return vestNumber; } }

        private string vestNumber { get; set; }
        public int IntOrder { get
            {
                if ( int.TryParse( vestNumber , out int result ) )
                    return result;
                return int.MaxValue;
            }
        }

        private string toDbString()
        {
            if (vestNumber == null) return string.Empty;
            return vestNumber.ToString();
        }

        public void setVestNumber(string newNumber)
        {
            vestNumber = newNumber; // Convert.ToInt32(newNumber);
        }

        public void setVestNumber(int newNumber)
        {
            vestNumber = newNumber.ToString();
        }

        public bool tryIntVest(out int VestNumber)
        {
            // get the integer vest number
            return int.TryParse(this.printVestString, out VestNumber);
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(vestNumber))
                return "";
            return vestNumber.ToString();
        }


        public static bool operator ==(VestNumber x, VestNumber y)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            return x.dbVestNumber == y.dbVestNumber;
        }

        public static bool operator !=(VestNumber x, VestNumber y)
        {
            return !(x == y);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            VestNumber v = obj as VestNumber;
            if ((System.Object)v == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (dbVestNumber == v.dbVestNumber);
        }

        public bool Equals(VestNumber v)
        {
            // If parameter is null return false:
            if ((object)v == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (dbVestNumber == v.dbVestNumber);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }



        public int CompareTo(object obj)
        {

            if (obj==null)
                return 1;

            VestNumber otherVest = obj as VestNumber;

            if (otherVest != null)
                return this.vestNumber.CompareTo(otherVest.vestNumber);
            else
                throw new ArgumentException("Object is not a vest");
        }
    }// VestNumber

    internal abstract class ANumber
    {
        public int length { get; private set; }

        public ANumber(int Length)
        {
            this.length = Length;
        }

        abstract public override string ToString();
        abstract public string Symbol();
        abstract public void increment();
        abstract public void decrement();
    }

    internal class IntNumber : ANumber
    {
        protected int Value { get; set; }

        public IntNumber(int length, int Value = 0)
            : base(length)
        {
            this.Value = Value;
        }

        public override string ToString()
        {
            //string formatString = "";

            //for (int i = 0; i < length; i++)
            //{
            //    formatString += "0";
            //}
            return Value.ToString("D" + length.ToString());
        }

        public void increment(int add = 1)
        {
            Value += add;

            // stop the value ever going below 0
            if (Value < 0)
            {
                Value = 0;
            }
        }

        public void decrement(int minus = 1)
        {
            increment(minus - (minus * 2));
        }

        public override void increment()
        {
            increment(1);
        }

        public override void decrement()
        {
            decrement(1);
        }

        public override string Symbol()
        {
            return "n" + length.ToString();
        }
    }

    internal class CharNumber : IntNumber
    {
        private new char[] Value { get; set; }


        public CharNumber(int length, char[] Value = null)
            : base(length)
        {
            if (Value == null)
            {
                char[] defaultChar = new char[length];

                for (int i = 0; i < length; i++)
                {
                    defaultChar[i] = 'a';
                }

                this.Value = defaultChar;
            }
            else
            {
                this.Value = Value;
            }
        }// constructor

        /// <summary>
        ///  Used to take the char[] Value into an integer
        ///  
        ///  a = aa = 0
        ///  b = ab = 1
        ///  ...
        ///  z = az = 25 ( (0 * 26) + 25 )
        ///  ? = ba = 26 ( (1 * 26) + 0 )
        ///  ? = bb = 27 ( (1 * 26) + 1 )
        ///  ...
        ///  ? = bl = 37 ( (1 * 26) + 11 )
        ///  ...
        ///  ? = ca = 52 ( (2 * 26) + 0 )
        ///  ...
        ///  ? = 
        /// </summary>
        /// 
        public int charToInt()
        {

            //char x = 9; // '9' = ASCII 57
            //int b;
            //b = x - '0'; //That is '9' - '0' = 57 - 48 = 9

            int tempInt = 0;

            if (length != Value.Length)
            {
                if (length > Value.Length)
                {
                    char[] temp = new char[length];

                    int i = 0;
                    foreach (char ch in Value)
                    {
                        temp[i++] = ch;
                    }

                    int remaining = length - Value.Length;

                    for (; remaining > 0; remaining--)
                    {
                        temp[Value.Length - remaining] = 'a';
                    }

                    Value = temp;
                }
                else
                {
                    // error can not continue
                    return 0;
                }
            }

            int multiplier = 0;//26 * (length - 1);

            for (int k = length; k > 0; )
            {
                if (Char.ToLower(Value[--k]) != 'a')
                {
                    tempInt += (multiplier) + (int)(Char.ToLower(Value[k]) - 'a');
                }

                multiplier += 25;
            }

            return tempInt;
        }

        private void IntToChar()
        {
        }

        public override string Symbol()
        {
            return "l" + length;
        }
        
        public override string ToString()
        {
            string temp = "";

            foreach (char c in Value)
            {
                temp += c;
            }

            return  temp ;
        }

    }


    
}
