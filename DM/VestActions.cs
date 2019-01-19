using System;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{
    public enum VestActionDescriptions
	{
	    InsertResultVest, DeleteResultVest, 
        StartEvent, FinishEvent, 
        InsertPlaceHolder, DeletePlaceHolder, 
        Unknown 
	}

    public partial class VestActions : IID 
    {
        public VestActions()
        {
            DState = null;
        }

        //public int ID { get { return _ID; } set { _ID = value; } }

        public int WebID { get { return _WebID; } set { _WebID = value; } }

        // commented out in V3-0
        //protected int? Championship_ID;

        public string Description { get { return _Description; } set { _Description = value; } }

        public DateTime DateStamp { get { return _DateStamp; } set { _DateStamp = value; } }

        public string Vest { get { return _Vest; } set { _Vest = value; } }

        public string ChampionshipName { get { return _ChampionshipName; } set { _ChampionshipName = value; } }

        public string EventCode { get { return _EventCode; } set { _EventCode = value; } }

        public int? Position { get { return _Position; } set { _Position = value; } }

        public TimeSpan? Time { get { return _Time; } set { _Time = value; } }

        public bool Ignored { get { return _Ignored; } set { _Ignored = value; } }

        public string statusDescription { get { return _statusDescription; } set { _statusDescription = value; } }

        public Championship Championship { get { return _Championship; } set { _Championship = value; } }

        public override bool Equals(object obj)
        {
            try
            {
                return ((VestActions)obj).WebID == this.WebID;
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

        public static bool operator ==(VestActions x, VestActions y)
        {
            if (((object)x) == null && ((object)y) == null) return true;
            if (((object)x) == null) return false;
            if (((object)y) == null) return false;

            return x.ID == y.ID;
        }

        public static bool operator !=(VestActions x, VestActions y)
        {
            return !(x == y);
        }

        public VestActionDescriptions Action
        {
            get
            {
                switch (Description)
                {
                    case "InsertResultVest":
                        return VestActionDescriptions.InsertResultVest;
                    case "DeleteVest":
                        return VestActionDescriptions.DeleteResultVest;
                    case "StartEvent":
                        return VestActionDescriptions.StartEvent;
                    case "FinishEvent":
                        return VestActionDescriptions.FinishEvent;
                    case "InsertPlaceHolder":
                        return VestActionDescriptions.InsertPlaceHolder;
                    case "DeletePlaceHolder":
                        return VestActionDescriptions.DeletePlaceHolder;
                    default:
                        return VestActionDescriptions.Unknown;
                }
            }
        }

    }
}
