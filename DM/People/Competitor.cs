using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ChampionshipSolutions.DM
{

    public interface ICompetitor
    {
        AEvent CompetingIn { get; set; }

        VestNumber Vest { get; set; }

        Team getTeam();

        string printTeam { get; }

        string printVestNumber();

        string getName();
        string Name { get; }
    }

    public interface ILanedCompetitor : ICompetitor
    {
        bool hasLaneNumber();
        int getLaneNumber();
        void setLaneNumber ( int Number );

        int LaneNumber { get; }
    }

    public interface IHeatedCompetitor : ICompetitor 
    {
        IndividualTimedHeatEvent HeatEvent { get; set; }

        void promoteToFinal();
        void demoteFromFinal();
        bool isInFinal();

        AResult HeatResult { get; }
    }

    public interface ILanedHeatedCompetitor : ILanedCompetitor, IHeatedCompetitor
    {
        bool hasHeatLaneNumber();
        int getHeatLaneNumber();
        void setHeatLaneNumber(int Number);

        int HeatLaneNumber { get; }
    }

    public abstract partial class ACompetitor : ICompetitor, ILanedCompetitor, ICustomData
    {
        // 2016-06-09 temporary for TF event 
        // To Do these strings should be dynamic
        // must also be changed in wizardpage1.html and wizardpage2.html
        public bool AvilableForSW { get { return isAvilableFor ( "South West" ); } }
        public bool AvilableForNationals { get { return isAvilableFor ( "National" ); } }

        public ACompetitor ()
        {
            DState = null;
        }

        public AEvent CompetingIn { get { return _CompetingIn; } set { _CompetingIn = value; } }

        private string Vest_dbVestNumber { get { return _Vest_dbVestNumber; } set { _Vest_dbVestNumber = value; } }

        public virtual VestNumber Vest { get { return new VestNumber() { dbVestNumber = Vest_dbVestNumber }; } set { Vest_dbVestNumber = value.dbVestNumber; } }

        public abstract Team getTeam();

        public virtual Team Team { get { return getTeam ( ); } }

        public virtual string printTeam { get { return getTeam().Name; } }

        public string printVestNumber() { return Vest.ToString(); }

        public abstract string getName();
        public string Name { get { return getName(); } }

        /// <summary>
        /// Used to decide if this competitor is an orphan, if it is it will be deleted.
        /// </summary>
        /// <returns></returns>
        public abstract bool isValid();

        public abstract bool isAthlete(Athlete athlete);

        public abstract object checkParameter(string Parameter);
        public virtual string printParameter(string Parameter)
        {
            object obj = checkParameter(Parameter);

            if (obj == null)
                return string.Empty;
            else
                return obj.ToString();
        }

        public bool forChampionship(Championship Championship)
        {
            // Added 2016-03-25
            if (CompetingIn == null) return false;

            return (CompetingIn.Championship == Championship);
        }

        public virtual bool InFinal { get { return true; }
            set { // do nothing 
            }
        }

        public virtual AResult getResult()
        {
            if (CompetingIn == null) return null;
            if (CompetingIn.hasResultFor(this))
                return CompetingIn.Results.Where(r => r.Competitor == this).First();
            else
                return null;
        }

        public AResult Result { get { return getResult(); } }



        //private List<ACertificate> _certificates;
        //public virtual List<ACertificate> Certificates
        //{
        //    get
        //    {
        //        if (_certificates == null) _certificates = new List<ACertificate>();
        //        return _certificates;
        //    }
        //    set
        //    {
        //        _certificates = value;
        //    }
        //}

        public virtual bool Guest { get { return _Guest; } set { _Guest = value; } }

        //public bool AvilableForSW
        //{
        //    get
        //    {
        //        object obj = getValue("SWCoach");

        //        if (obj == null)
        //            return false;
        //        else
        //            if (obj is string)
        //        {
        //            if ((string)obj != "no")
        //                return true;
        //            else
        //                return false;
        //        }
        //        else
        //            return false;
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}


        //public bool? AvilableForNationals { get { return _AvilableForNationals; } set { _AvilableForNationals = value; } }

        //public string CoachForSW { get { return _CoachForSW; } set { _CoachForSW = value; } }

        //public string CoachForNationals { get { return _CoachForNationals; } set { _CoachForNationals = value; } }

        public bool SelectedForNextEvent { get { return _SelectedForNextEvent; } set { _SelectedForNextEvent = value; Save ( ); } }

        private int PersonalBest_RawValue { get { return _PersonalBest_RawValue; } set { _PersonalBest_RawValue = value; Save ( ); } }

        //private int PersonalBest_ValueType { get { return _PersonalBest_ValueType; } set { _PersonalBest_ValueType = value; } }

        public ResultValue PersonalBest
        {
            get
            {
                return new ResultValue ( )
                {
                    RawValue = PersonalBest_RawValue ,
                    ValueType = CompetingIn.ResultsDisplayDescription 
                };
            }
            set
            {
                PersonalBest_RawValue = value.RawValue;
                //PersonalBest_ValueType = (int)value.ValueType;
            }
        }

        public string PersonalBestStr
        {
            get
            {
                if (PersonalBest == null) return string.Empty;

                return PersonalBest.getResultString();
            }
        }

        public static bool operator ==(ACompetitor x, ACompetitor y)
        {
            if (((object)x) == null && ((object)y) == null) return true;
            if (((object)x) == null) return false;
            if (((object)y) == null) return false;

            return x.ID == y.ID;
        }

        public static bool operator !=(ACompetitor x, ACompetitor y)
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
            ACompetitor c = obj as ACompetitor;
            if ((System.Object)c == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (ID == c.ID);
        }

        public bool Equals(ACompetitor c)
        {
            // If parameter is null return false:
            if ((object)c == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (ID == c.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        [Obsolete ("Use bool isAvilableFor(string Championship) instead",true)]
        public bool isAvilableForSW()
        {
            //if (AvilableForSW.HasValue)
            //    return AvilableForSW.Value;
            //return false;
           return AvilableForSW;
        }

        public bool isAvilableFor ( string Championship )
        {
            if ( this is Competitor )
                if ( ((Competitor)this ).Athlete != null )
                    return ( (Competitor)this ).Athlete.isAvilableFor ( Championship );
                else
                    return false;
            else
                return false;
        }

        #region Lanes

        public int LaneNumber { get { return _LaneNumber; } set { _LaneNumber = value; Save ( ); } }

        public bool hasLaneNumber()
        {
            return LaneNumber > 0;
        }

        public int getLaneNumber()
        {
            return LaneNumber;
        }

        public void setLaneNumber(int Number)
        {
            LaneNumber = Number;
        }

        #endregion

        #region CustomData

        protected ACustomDataValue[] CustomDataStore { get { return _CustomDataStore.ToArray(); } }

        public void addCustomDataValue ( ACustomDataValue Value )
        {
            if ( Value != null )
            {
                Value.Competitor = this;
                DState.IO.Add <ACustomDataValue> ( Value );
                //_CustomDataStore.Add ( Value );
            }
        }

        public void removeCustomDataValue ( ACustomDataValue Value )
        {
            if ( Value != null )
                //!**!
                DState.IO.Delete<ACustomDataValue> ( Value );
                //_CustomDataStore.Remove ( Value );
        }

        public void createIntField(string key)
        {
            ACustomDataValue cdv = CustomData.createIntField(CustomDataStore, this, key);
            //cdv.ACompetitor_ID = this.ID;
            //cdv.Competitor = this;
        }

        public void createStringField(string key)
        {
            ACustomDataValue cdv = CustomData.createStringField(CustomDataStore, this, key);
            //cdv.ACompetitor_ID = this.ID;
            //cdv.Competitor = this;
        }

        public void deleteField(string key)
        {
            removeCustomDataValue ( CustomData.deleteField ( CustomDataStore , this , key ) );
        }

        public bool customFieldExists(string key)
        {
            return CustomData.customFieldExists(CustomDataStore, key);
        }

        public object getValue(string key)
        {
            return CustomData.getValue(CustomDataStore, key);
        }

        public void setValue(string key, object value)
        {
            __CustomDataStore.refresh( );
            ACustomDataValue cdv = CustomData.setValue(CustomDataStore, key, value, this);
            cdv.Competitor = this;
        }

        public void clearAllFields ( )
        {
            foreach ( var field in CustomDataStore )
                deleteField ( field.key );
        }

        #endregion

    }

    public partial class Competitor : ACompetitor
    {
        //protected int? Athlete_ID { get { return _Athlete_ID; } set { _Athlete_ID = value; } }

        // Competitor will be deleted if Athlete is set to null
        public Athlete Athlete { get { return _Athlete; } set { _Athlete = value; } }

        #region Constructors

        public Competitor(Athlete athlete, VestNumber Vest, AEvent inEvent)
        {
            this.Athlete = athlete;
            this.Vest = Vest;
            this.CompetingIn = inEvent;
        }

        public Competitor(Athlete athlete) 
        {
            Athlete = athlete;
        }

        public Competitor() {  }

        #endregion

        #region ACompetitor


        public override string getName()
        {
            return Athlete.PrintName();
        }

        public override bool isAthlete(Athlete athlete)
        {
            return Athlete == athlete;
        }

        /// <summary>
        /// Used to decide if this competitor is an orphan, if it is it will be deleted.
        /// </summary>
        /// <returns></returns>
        public override bool isValid()
        {
            bool athleteValid, eventValid;

            // if there is an athlete 
            athleteValid = ( Athlete != null );

            eventValid   = ( CompetingIn != null);

            return (athleteValid && eventValid);
        }

        #endregion

        public override Team getTeam()
        {
            return Athlete.getTeam(this.CompetingIn.Championship);
        }

        public override object checkParameter(string Parameter)
        {
            // Check this competitor object

            System.Reflection.PropertyInfo prop = this.GetType().GetProperties().Where(f => f.Name == Parameter).FirstOrDefault();

            if (prop != null)
                if (prop.CanRead)
                    return prop.GetValue(this, null);


            // Now check the parent Athlete object

            if (Athlete == null)
                return null;

            prop = Athlete.GetType().GetProperties().Where(f => f.Name == Parameter).FirstOrDefault();

            if (prop != null)
                if (prop.CanRead)
                    return prop.GetValue(Athlete, null);

            return null;
        }

    }

    public partial class StudentCompetitor : Competitor
    {
        #region Constructors

        public StudentCompetitor(Athlete athlete, VestNumber Vest, AEvent inEvent):base(athlete,Vest,inEvent)
        {
        }

        public StudentCompetitor(): base()
        {
        }

        #endregion

        static public int CalculateYearGroup ( Athlete Athlete, Championship Championship )
        {
            if ( Athlete == null )
                return 0;

            if ( !Athlete.DateOfBirth.HasValue )
                return 0;

            // ToDo is the minus 5 really constant here!? 2016-11-06
            if ( Championship.AgeDateReference.HasValue )
                return Athlete.getAge( Championship.AgeDateReference.Value ).Years - 5;

            return 0;

        }

        public int YearGroup
        {
            get => CalculateYearGroup( Athlete, CompetingIn.Championship );
        }

        public string School
        {
            get
            {
                if (Athlete == null)
                    return string.Empty;

                return printParameter("Attends").ToString();
            }
        }

        public string formatYearGroup()
        {
            string yG = YearGroup.ToString();
            if (yG == "0")
            {
                return " ";
            }
            else
            {
                return yG;
            }
        }

    }

    public partial class StudentHeatedCompetitor : StudentCompetitor, IHeatedCompetitor , ILanedHeatedCompetitor 
    {
        #region Constructors

        public StudentHeatedCompetitor(Athlete athlete, VestNumber Vest, AEvent inEvent)
        {
            this.Athlete = athlete;
            this.Vest = Vest;
            this.CompetingIn = inEvent;
        }

        public StudentHeatedCompetitor()
        {
        }

        #endregion

        //protected int? HeatEvent_ID { get { return _HeatEvent_ID; } set { _HeatEvent_ID = value; } }

        public IndividualTimedHeatEvent HeatEvent { get { return (IndividualTimedHeatEvent) _HeatEvent; } set { _HeatEvent = value; } }

        public override bool InFinal { get { return _InFinal; } set { _InFinal = value; } }

        public override AResult getResult()
        {
            AResult temp = base.getResult();

            //if (temp == null)
                // there isn't a result for the final, so lets check the heat.
                //if ( this.HeatEvent.hasResultFor ( this ) )
                    // there is a result so lets get it an return it.
                    //return this.HeatEvent.Results.Where ( r => r.Competitor == this ).First ( );

            return temp;
        }

        public AResult getHeatResult()
        {
            if ( this.HeatEvent?.hasResultFor ( this ) == true )
                // there is a result so lets get it an return it.
                return this.HeatEvent.Results.Where ( r => r.Competitor == this ).First ( );
            else
                return null;
        }

        // 2016-06-09 needs more thought
        public AResult HeatResult
        {
            get
            {
                //if ( !( this is StudentHeatedCompetitor ) )
                    //return null;
                //return ( (StudentHeatedCompetitor)this ).getHeatResult ( );
                return getHeatResult ( );
            }
        }

        public bool isInFinal()
        {
            return InFinal;
        }

        public void promoteToFinal()
        {
            InFinal = true;
            Save ( );
        }

        /// <summary>
        /// Will throw an exception if the competitor has a result in the final
        /// </summary>
        public void demoteFromFinal()
        {
            if (this.Result == null)
                InFinal = false;
            else
                throw new ArgumentException("Can not remove from the final as this competitor has a result in the final");
        }

        #region Lanes

        public int HeatLaneNumber { get { return _HeatLaneNumber; } set { _HeatLaneNumber = value; Save ( );  }  }

        public bool hasHeatLaneNumber()
        {
            return HeatLaneNumber > 0;
        }

        public int getHeatLaneNumber()
        {
            return HeatLaneNumber;
        }

        public void setHeatLaneNumber(int Number)
        {
            HeatLaneNumber = Number;
        }

        #endregion
    }

    public partial class Squad : ACompetitor
    {
        public string SquadName { get { return printTeam; } }

        public Athlete Competitor1 { get { return _Competitor1; } set { _Competitor1 = value; } }
        public Athlete Competitor2 { get { return _Competitor2; } set { _Competitor2 = value; } }
        public Athlete Competitor3 { get { return _Competitor3; } set { _Competitor3 = value; } }
        public Athlete Competitor4 { get { return _Competitor4; } set { _Competitor4 = value; } }

        public override Team Team
        {
            get
            {
                foreach (Athlete a in members)
                {
                    if (a == null) continue;

                    return a.getTeam(CompetingIn.Championship);
                }

                return null;
            }
        }

        public Athlete[] members { get { return new Athlete[] { Competitor1, Competitor2, Competitor3, Competitor4 }; } }

        #region ACompetitor

        public override string getName()
        {
            if (!string.IsNullOrWhiteSpace(SquadName))
                if ( ! isSquadEmpty () )
                {
                    string t = "";

                    t = SquadName + " - ";

                    t += printSquadInitials();

                    return t;
                }

           if (!string.IsNullOrWhiteSpace(SquadName))
                return SquadName;
            else
                return printSquadInitials();
        }

        private string printSquadInitials()
        {
            string temp = "";

            foreach (Athlete a in members)
            {
                if (a == null) continue;
                temp += a.PrintInitials();
                temp += ", ";
            }

            return temp.Trim().Trim(',');
        }

        /// <summary>
        /// Will check if this athlete is within this squad
        /// </summary>
        /// <param name="athlete">Athlete to check for.</param>
        /// <returns>True if the Athlete is within this squad</returns>
        public override bool isAthlete(Athlete athlete)
        {
            foreach (Athlete a in members)
                if (a == athlete)
                    return true;

            return false;
        }

        public bool addToSquad(Athlete athlete)
        {
            int slot;

            if (getEmptySlot(out slot))
            {
                switch (slot)
                {
                    case 0:
                        Competitor1 = athlete;
                        break;
                    case 1:
                        Competitor2 = athlete;
                        break;
                    case 2:
                        Competitor3 = athlete;
                        break;
                    case 3:
                        Competitor4 = athlete;
                        break;
                    default:
                        return false;
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool removeFromSquad(Athlete athlete)
        {

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] == athlete)
                {
                    members[i] = null ;
                    return true ;
                }
            }

            return false;
        }

        private bool getEmptySlot( out int slot )
        {

            for ( int i = 0 ; i < members.Length ; i++)
            {
                if (members[i] == null)
                {
                    slot = i;
                    return true;
                }
            }

            slot = 0;
            return false;
        }

        public bool isSquadFull()
        {
            int i;
            return !(getEmptySlot(out i));
        }

        public bool isSquadEmpty()
        {
            for (int i = 0; i < members.Length; i++)
                if (members[i] != null)
                    return false;

            return true;
        }

        #endregion


        public override Team getTeam()
        {
            return Team;
        }

        /// Used to decide if this competitor is an orphan, if it is it will be deleted.
        public override bool isValid()
        {
            bool athleteValid, eventValid;

            athleteValid = false;

            // if there is a single athlete in this event the return true.
            foreach (Athlete a in members)
            {
                if (a != null) 
                    athleteValid = true;
            }

            eventValid = (CompetingIn != null);

            return (athleteValid && eventValid);
        }

        public override object checkParameter(string Parameter)
        {
            // Check this competitor object

            System.Reflection.PropertyInfo prop = this.GetType().GetProperties().Where(f => f.Name == Parameter).FirstOrDefault();

            if (prop != null)
                if (prop.CanRead)
                    return prop.GetValue(this, null);

            return null;
            // as a squad I have made an active decision to not support checking the athletes
            //foreach (AAthlete a in members)
            //{
            //    if (a == null)
            //        return null;
            //}

            //prop = Athlete.GetType().GetProperties().Where(f => f.Name == Parameter).FirstOrDefault();

            //if (prop != null)
            //    if (prop.CanRead)
            //        return prop.GetValue(Athlete, null);

            //return null;
        }

    }
}
