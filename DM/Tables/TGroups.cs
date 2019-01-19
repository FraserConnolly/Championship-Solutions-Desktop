using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;

namespace ChampionshipSolutions.DM
{
    [Table ( Name = "Groups" )]
    [InheritanceMapping ( Code = "Group" , Type = typeof ( Group ) , IsDefault = true )]
    [InheritanceMapping ( Code = "GenderRestriction" , Type = typeof ( GenderRestriction ) )]
    [InheritanceMapping ( Code = "AgeRestriction" , Type = typeof ( AgeRestriction ) )]
    [InheritanceMapping ( Code = "DoBRestriction" , Type = typeof ( DoBRestriction ) )]
    public partial class Group
    {

        [Column ( IsPrimaryKey = true , Name = "ID" )]
        public int ID { get; set; }

        [Column ( IsDiscriminator = true , Name = "Discriminator" )]
        public string Discriminator { get; set; }

        [Column ( Name = "Championship_ID" )]
        internal int _Championship_ID { get; set; }

        [Column ( Name = "Name" )]
        internal string _Name { get; set; }

        [Column ( Name = "ShortName" )]
        internal string _ShortName { get; set; }

        //internal EntityRef<Championship> _ChampionshipStorage = new EntityRef<Championship>();
        //[Association ( IsForeignKey = true , ThisKey = "_Championship_ID" , Storage = "_ChampionshipStorage" , DeleteOnNull = true )]
        //internal Championship _Championship { get { return _ChampionshipStorage.Entity; } set { _ChampionshipStorage.Entity = value; } }

        private DataState _DState = new DataState();

        public virtual DataState DState
        {
            get
            {
                return _DState;
            }
            set
            {
                _DState = value;
                __Championship = new LazyRef<Championship> ( DState , ( ) => _Championship_ID , v => { _Championship_ID = v; } );
                __Events = new LazyList<EventGroups> ( DState , c => c._Group_ID == ID );
            }
        }

        internal LazyRef<Championship> __Championship;
        internal Championship _Championship { get { return __Championship.Store; } set { __Championship.Store = value; } }

        internal LazyList<EventGroups> __Events { get; set; }
        internal EventGroups[] _Events { get { return __Events.ToArray( ); } }

        public virtual void voidStorage ( bool softRefresh = true )
        {
            __Championship.refresh ( );
            __Events.refresh ( );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<Group> ( this );
        }


    }

    public partial class GenderRestriction : Group
    {

        [Column ( Name = "Male" )]
        internal bool _Male { get; set; }

        [Column ( Name = "Female" )]
        internal bool _Female { get; set; }

    }

    public partial class AgeRestriction : Group
    {

        [Column ( Name = "minAge" )]
        internal int _minAge { get; set; }

        [Column ( Name = "maxAge" )]
        internal int _maxAge { get; set; }

        [Column ( Name = "dateReference" )]
        internal DateTime _dateReference { get; set; }

    }

    public partial class DoBRestriction : Group
    {
        /// <summary>
        /// Start of date range
        /// </summary>
        [Column ( Name = "StartDate" )]
        internal DateTime _StartDate { get; set; }

        /// <summary>
        /// End of date range
        /// </summary>
        [Column ( Name = "EndDate" )]
        internal DateTime _EndDate { get; set; }

    }
}
