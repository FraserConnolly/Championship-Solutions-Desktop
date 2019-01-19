using ChampionshipSolutions.DM.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM
{
    public class DataState
    {
        internal bool NeedsUpdating;
        internal Database IO;
    }

    internal enum LazyState
    {
        NotInitialised,
        Initialised,
        UpToDate,
        NeedsUpdating
    }

    public class LazyList<T> : IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T> where T : class
    {

        private LazyState State;

        #region List Functions

        private IList<T> List;

        public T this[int index]
        {
            get
            {
                if ( State == LazyState.NotInitialised ) return default ( T );
                checkState ( );
                if ( List == null ) return null;
                return List[index];
            }
        }

        public int Count
        {
            get
            {
                if ( State == LazyState.NotInitialised ) return 0;
                checkState ( );
                if ( List == null ) return 0;
                return List.Count;
            }
        }

        public bool IsReadOnly { get { return true; } }

        public bool Contains ( T item )
        {
            if ( State == LazyState.NotInitialised ) return false;
            checkState ( );
            if ( List == null ) return false ;
            return List.Contains ( item );
        }

        public void CopyTo ( T[] array , int arrayIndex )
        {
            if ( State == LazyState.NotInitialised ) return;
            checkState ( );
            if ( List == null ) return ;
            List.CopyTo ( array , arrayIndex );
        }

        public IEnumerator<T> GetEnumerator ( )
        {
            if ( State == LazyState.NotInitialised ) return new List<T> ( ).GetEnumerator ( );
            checkState ( );
            if ( List == null ) return new List<T>().GetEnumerator();
            return List.GetEnumerator ( );
        }

        public int IndexOf ( T item )
        {
            if ( State == LazyState.NotInitialised ) return -1;
            checkState ( );
            if ( List == null ) return -1;
            return List.IndexOf ( item );
        }

        IEnumerator IEnumerable.GetEnumerator ( )
        {
            if ( State == LazyState.NotInitialised ) return new List<T> ( ).GetEnumerator ( );
            checkState ( );
            if ( List == null ) return new List<T> ( ).GetEnumerator ( );
            return List.GetEnumerator ( );
        }

        #endregion

        #region Constructors

        public LazyList ( )
        {
            State = LazyState.NotInitialised;
        }

        public LazyList ( DataState ds , Func<T , bool> predicate )
        {
            dataState = ds;
            pre = predicate;
            State = LazyState.Initialised;
        }

        #endregion

        private DataState dataState;
        private Func<T,bool> pre;

        private void checkState ( )
        {
            switch ( State )
            {
                case LazyState.NotInitialised:
                    return;
                case LazyState.Initialised:
                    UpdateData ( );
                    return;
                case LazyState.UpToDate:
                    return;
                case LazyState.NeedsUpdating:
                    UpdateData ( );
                    return;
                default:
                    return;
            }
        }

        private void UpdateData ( )
        {
            try
            {
                List = dataState.IO.GetAll<T> ( ).Where ( pre ).ToList ( );
                //Console.WriteLine ( "Updated Lazy List " + typeof ( T ).Name.ToString ( ) + " for " + pre.Target.ToString ( ) );
                State = LazyState.UpToDate;
            }
            catch ( Exception ex )
            {
                State = LazyState.NotInitialised;
                error = ex;
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Lazy loading error of type " + typeof ( T ).ToString ( ) );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
            }
        }

        public void refresh ( )
        {
            State = LazyState.NeedsUpdating;
        }

        public Exception error { get; set; }
    }

    public class LazyRef<T> where T : class
    {

        private LazyState State;
        private T Storage;

        public T Store
        {
            get
            {
                if ( State == LazyState.NotInitialised ) return null;
                checkState ( );
                return Storage;
            }
            set {
                Storage = value;
                if ( value == null )
                    StoreRef = 0; //Default value;
                else
                    StoreRef = ( (IID)value ).ID;
            }
        }

        public int StoreRef
        {
            get
            {
                return getter ( );
            }
            set { setter ( value ); State = LazyState.Initialised; }
        }

        #region Constructors

        public LazyRef ( )
        {
            State = LazyState.NotInitialised;
        }

        public LazyRef ( DataState ds , Func<int> getter , Action<int> setter )
        {
            dataState = ds;
            this.getter = getter;
            this.setter = setter;
            if ( dataState != null )
                State = LazyState.Initialised;
        }

        #endregion

        private Func<int> getter;
        private Action<int> setter;

        private DataState dataState;

        private void checkState ( )
        {
            switch ( State )
            {
                case LazyState.NotInitialised:
                    return;
                case LazyState.Initialised:
                    UpdateData ( );
                    return;
                case LazyState.UpToDate:
                    return;
                case LazyState.NeedsUpdating:
                    UpdateData ( );
                    return;
                default:
                    return;
            }
        }

        private void UpdateData ( )
        {
            try
            {
                if ( dataState != null )
                {
                    Storage = dataState.IO.GetID<T> ( StoreRef );
                    //Console.WriteLine ( "Updated Lazy Ref " + " for " + Storage.ToString ( ) );
                    State = LazyState.UpToDate;
                }
            }
            catch ( Exception ex )
            {
                State = LazyState.NotInitialised;
                error = ex;
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Lazy loading error of type " + typeof ( T ).ToString ( ) );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
            }
        }

        public void refresh ( )
        {
            State = LazyState.NeedsUpdating;
        }

        public Exception error { get; set; }
    }

    public class LazyRefN<T> where T : class
    {

        private LazyState State;
        private T Storage;

        public T Store
        {
            get
            {
                if ( State == LazyState.NotInitialised ) return null;
                checkState ( );
                return Storage;
            }
            set {
                Storage = value;
                if ( value == null )
                    StoreRef = null;
                else
                    StoreRef = ( (IID)value ).ID;
            }
        }

        public int? StoreRef
        {
            get
            {
                return getter ( );
            }
            set { setter ( value ); State = LazyState.Initialised; }
        }

        #region Constructors

        public LazyRefN ( )
        {
            State = LazyState.NotInitialised;
        }

        public LazyRefN ( DataState ds , Func<int?> getter , Action<int?> setter ) : this ( )
        {
            dataState = ds;
            this.getter = getter;
            this.setter = setter;
            if ( dataState != null )
                State = LazyState.Initialised;
        }

        #endregion

        private Func<int?> getter;
        private Action<int?> setter;

        private DataState dataState;

        private void checkState ( )
        {
            switch ( State )
            {
                case LazyState.NotInitialised:
                    return;
                case LazyState.Initialised:
                    UpdateData ( );
                    return;
                case LazyState.UpToDate:
                    return;
                case LazyState.NeedsUpdating:
                    UpdateData ( );
                    return;
                default:
                    return;
            }
        }

        private void UpdateData ( )
        {
            try
            {
                if ( StoreRef == null )
                {
                    State = LazyState.Initialised;
                    Storage = null;
                    return;
                }

                if ( dataState != null )
                {
                    Storage = dataState.IO.GetID<T> ( StoreRef );
                    //Console.WriteLine ( "Updated Lazy Ref " + " for " + Storage.ToString ( ) );
                    State = LazyState.UpToDate;
                }
            }
            catch ( Exception ex )
            {
                State = LazyState.NotInitialised;
                error = ex;
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Lazy loading error of type " + typeof ( T ).ToString ( ) );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
            }
        }

        public void refresh ( )
        {
            State = LazyState.NeedsUpdating;
        }

        public Exception error { get; set; }
    }
}
