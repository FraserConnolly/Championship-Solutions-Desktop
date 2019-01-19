using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionshipSolutions.DM.DataAccess
{

    internal class DataBaseCache
    {
        private enum ObjectState
        {
            NotLoaded,
            NoData,         // not implemented
            NeedsUpdating,  // not implemented
            UpToDate,       
            Deleted,
            NeedsSaving     // not implemented
        }

        private class StoredData
        {
            public Type type;
            public IID item;
            public ObjectState state;
        }

        private Dictionary<Type,Hashtable> cacheStore;
        private Dictionary<Type,ObjectState> tableState; // Not implemented

        internal DataBaseCache ()
        {
            cacheStore = new Dictionary<Type , Hashtable> ( );
            tableState = new Dictionary<Type , ObjectState> ( ); // Not implemented
        }

        //internal void AddTableToCache (Type tableType, IID[] ItemData)
        //{
            //tableState[tableType] = ObjectState.UpToDate;

            //foreach ( var item in collection )
            //{

            //}
        //}

        internal void AddToCache(Type objectType, IID Item)
        {
            // Create the hash table for this data type.
            if (! cacheStore.ContainsKey( objectType ) )
                cacheStore.Add ( objectType , new Hashtable ( ) );

            cacheStore[objectType][Item.ID] = new StoredData ( ) { type = objectType , item = Item , state = ObjectState.UpToDate } ;
        }

        internal void AddTableToCache ( Type tableType , IList<IID> temp )
        {
            if ( !tableState.ContainsKey ( tableType ) )
                tableState.Add ( tableType , ObjectState.UpToDate );

            foreach ( IID item in temp )
                AddToCache ( tableType , item );

            if (temp.Count == 0)
            {
                // Table is empty
                cacheStore.Add ( tableType , new Hashtable ( ) );
            }
        }

        internal IID GetFromCache(Type objectType, int ItemRef, out bool Deleted)
        {
            Deleted = false;

            if ( !cacheStore.ContainsKey ( objectType ) )
                // no data of this type has been entered into the cache.
                return null;
            object raw = cacheStore[objectType][ItemRef];

            if ( raw == null ) return null;

            StoredData data = (StoredData) cacheStore[objectType][ItemRef];

            if ( data.state == ObjectState.Deleted )
            {
                // this item has previously been deleted so don't ask the data store for it.
                Deleted = true;
                return null;
            }
            else if ( data.state == ObjectState.UpToDate )
            {
                //Console.WriteLine ( "Data retrieved from cache from " + objectType.ToString ( ) );
                return data.item;
            }

            return null;
        }

        internal IList<IID> GetTableFromCache ( Type tableType )
        {
            if ( !tableState.ContainsKey ( tableType ) )
                return null;

            if ( tableState[tableType] == ObjectState.UpToDate )
            {
                //Console.WriteLine ( "Table Data retrieved from cache from " + tableType.ToString ( ) );

                if ( ! cacheStore.ContainsKey ( tableType ) ) return null;

                IList < IID > temp =  ( cacheStore[tableType].Values.Cast<StoredData> ( )
                    .Where ( i => i.state == ObjectState.UpToDate )
                    .Select ( i => i.item ).ToList() );

                return temp;
            }
            else
            {
                return null;
            }
        }

        internal void DeleteItemFromCache ( Type objectType, int ItemRef)
        {
            if ( !cacheStore.ContainsKey ( objectType ) )
                // no data of this type has been entered into the cache.
                return ;

            //StoredData data = ((StoredData)cacheStore[objectType][ItemRef]).state = ObjectState.Deleted ;
            ((StoredData)cacheStore[objectType][ItemRef]).state = ObjectState.Deleted ;
            ((StoredData)cacheStore[objectType][ItemRef]).item = null ;
            //data.item = null;
            //data.state = ObjectState.Deleted;
        }

        internal void Clear( )
        {
            foreach ( var table in cacheStore )
            {
                // each table
                foreach ( DictionaryEntry record in table.Value )
                {
                    // each record
                    if ( ( (StoredData)record.Value )?.item != null )
                        ( ((StoredData)record.Value)?.item).DState = null;
                }

                table.Value.Clear( );
            }
            cacheStore.Clear( );
        }
    }


}
