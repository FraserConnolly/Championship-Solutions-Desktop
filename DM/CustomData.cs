/*
 *  Filename         : CustomData.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2015-05-02
 *  Copyright        : FConn Ltd 2015
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 *      2016-03-25
 *          Split into TCustomData and Custom data to seperate the SQLite logic.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{
    public static class CustomData 
    {
        public static ACustomDataValue createIntField(ACustomDataValue[] data, ICustomData obj, string key)
        {
            ACustomDataValue cdv = new CustomDataValueInt() { key = key };
            if (!customFieldExists(data, key))
            {
                obj.addCustomDataValue ( cdv );
                //data.Add(cdv);
                return cdv;
            }
            else
            {
                return data.Where(c => c.key == key).FirstOrDefault();
            }
        }

        public static ACustomDataValue createStringField(ACustomDataValue[] data , ICustomData obj , string key)
        {
            ACustomDataValue cdv = new CustomDataValueString() { key = key };
            if (!customFieldExists(data, key))
            {
                //data.Add(cdv);
                obj.addCustomDataValue ( cdv );
                return cdv;
            }
            else
            {
                return data.Where(c => c.key == key).FirstOrDefault();
            }

        }
        
        public static ACustomDataValue deleteField (ACustomDataValue[] data, ICustomData obj, string key)
        {
            if (customFieldExists(data, key))
            {
                ACustomDataValue cdv = (from c in data where c.key == key select c ).First();

                cdv.Event = null;           // Marks the cdv for deletion
                cdv.Championship = null;    // Marks the cdv for deletion
                cdv.Competitor = null;      // Marks the cdv for deletion

                return cdv;
            }

            return null;
        }

        public static bool customFieldExists(ACustomDataValue[] data, string key)
        {
            return data.Where(x => x.key == key).Count() > 0;
        }

        public static object getValue(ACustomDataValue[] data, string key)
        {
            if (customFieldExists(data, key))
                return data.Where(x=> x.key == key).First().getValue();
            
            return null;
        }

        public static ACustomDataValue setValue(ACustomDataValue[] data, string key, object value, ICustomData obj)
        {
            if (!customFieldExists(data, key))
            {
                if (value is int)
                    createIntField ( data , obj , key );
                else if (value is string)
                    createStringField ( data , obj , key );
            }

            ACustomDataValue cdv = data.Where(x => x.key == key).First();

            cdv.setValue(value);
            cdv.Save( );
            return cdv;
        }

        internal static void CopyCustomData(ICollection<ACustomDataValue> origonalCustomData, ICustomData destinationObj)
        {
            destinationObj.clearAllFields();

            foreach (ACustomDataValue cdv in origonalCustomData)
            {
                switch (cdv.GetType().Name)
                {
                    case "CustomDataValueString":
                        destinationObj.createStringField(cdv.key);
                        break;
                    case "CustomDataValueInt":
                        destinationObj.createIntField(cdv.key);
                        break;
                    default:
                        break;
                }

                destinationObj.setValue(cdv.key, cdv.getValue());
            }
        }
    }



    public abstract partial class ACustomDataValue
    {

        public AEvent Event { get { return _Event; } set { _Event = value; } }
        public Championship Championship { get { return _Championship; } set { _Championship = value; } }
        public ACompetitor Competitor { get { return _Competitor; } set { _Competitor = value; } }

        public string key { get { return _key; } set { _key = value; } }
        public string printValue { get { return getValue()?.ToString(); } }

        public abstract void setValue(object value);
        public abstract object getValue();
    }

    public partial class CustomDataValueString : ACustomDataValue
    {
        public override void setValue(object value)
        {
            if (value is string)
                _stringValue = (string)value;
        }

        public override object getValue()
        {
            return _stringValue;
        }
    }

    public partial class CustomDataValueInt : ACustomDataValue
    {
        public override void setValue(object value)
        {
            if (value is int)
                _intValue = (int)value;
        }

        public override object getValue()
        {
            return _intValue;
        }
    }

    public interface ICustomData
    {
        void createIntField(string key);
        void createStringField(string key);
        void deleteField(string key);
        bool customFieldExists(string key);
        object getValue (string key);
        void setValue(string key, object value);
        void clearAllFields();
        void addCustomDataValue ( ACustomDataValue value );
        void removeCustomDataValue ( ACustomDataValue value );
    }
}
