using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM
{
    [Table ( Name = "ContactDetails" )]
    [InheritanceMapping ( Code = "AddressContactDetail" , Type = typeof ( AddressContactDetail ) , IsDefault = true )]
    [InheritanceMapping ( Code = "PhoneContactDetail" , Type = typeof ( PhoneContactDetail ) )]
    [InheritanceMapping ( Code = "MobileContactDetail" , Type = typeof ( MobileContactDetail ) )]
    [InheritanceMapping ( Code = "EmailContactDetail" , Type = typeof ( EmailContactDetail ) )]
    public abstract partial class AContactDetail :IID 
    {

        [Column(IsPrimaryKey = true, Name = "ID")]
        public int ID { get; set; }

        [Column(IsDiscriminator = true, Name = "Discriminator")]
        public string Discriminator { get; set; }

        // removed in V3-0
        //[Column(Name = "School_ID", CanBeNull = true)]
        //internal int? School_ID;

        [Column(Name = "Person_ID", CanBeNull = false)]
        internal int _Person_ID { get; set; }

        [Column (Name ="Primary")]
        internal bool _Primary { get; set; }

        [Column ( Name = "ContactName" )]
        internal string _ContactName { get; set; }

        //private EntityRef<Person> _PersonStorage = new EntityRef<Person>();
        //[Association ( ThisKey = "_Person_ID" , IsForeignKey = true, DeleteOnNull = true, Storage = "_PersonStorage" )]
        //internal Person _Person { get { return _PersonStorage.Entity; } set { _PersonStorage.Entity = value; } }

        private DataState _DState ;

        public virtual DataState DState
        {
            get
            {
                return _DState;
            }
            set
            {
                _DState = value;
                __Person = new LazyRef<Person> ( DState , ( ) => _Person_ID , v => { _Person_ID = v; } );
            }
        }

        private LazyRef<Person> __Person;
        internal Person _Person { get { return __Person.Store; } set { __Person.Store = value; } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Person.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<AContactDetail> ( this );
        }


    }

    public partial class AddressContactDetail : AContactDetail
    {
        [Column (Name = "FirstLine" )]
        internal string _FirstLine { get; set; }

        [Column ( Name = "SecondLine" )]
        internal string _SecondLine { get; set; }

        [Column ( Name = "ThirdLine" )]
        internal string _ThirdLine { get; set; }

        [Column ( Name = "FourthLine" )]
        internal string _FourthLine { get; set; }

        [Column ( Name = "PostCode" )]
        internal string _PostCode { get; set; }

    }

    public partial class PhoneContactDetail : AContactDetail
    {
        [Column (Name ="phoneNumber")]
        internal string _phoneNumber { get; set; }

    }

    public partial class MobileContactDetail : PhoneContactDetail
    {

    }

    public partial class EmailContactDetail : AContactDetail
    {
        [Column (Name = "EmailAddress")]
        internal string _EmailAddress { get; set; }

    }


}
