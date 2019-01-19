#define UseCache


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ChampionshipSolutions.DM;
using ChampionshipSolutions.DM.DataAccess;



namespace ChampionshipSolutions.DM.DataAccess
{


    public partial class Database 
    {
        public int DBReadCounter, CacheReadCounter;

        internal DataBaseCache cache;

        internal DataState getDataState ( )
        {

            DataState ds = new DataState();

            ds.IO = this;
            ds.NeedsUpdating = true;

            return ds;
        }

        public Database( SQLiteConnection Connection )
        {
            this.Connection = Connection;
            cache = new DataBaseCache ( );
        }

        internal SQLiteConnection Connection {get;set;}

        #region Insertions

        [DebuggerStepThrough]
        public bool Add<TEntity> ( IID entity ) where TEntity : class
        {
            if ( entity.ID != 0 )
                throw new ArgumentException ( "ID must be 0 to be inserted" );

            entity.DState = getDataState ( );

            return add<TEntity> ( (TEntity) entity );
        }

        public bool Add<TEntity> ( IID[] entityRange ) where TEntity : class
        {
            foreach ( var entity in entityRange )
                if ( entity.ID != 0 )
                    throw new ArgumentException ( "ID must be 0 to be inserted" );
                else
                    entity.DState = getDataState ( );

            return addRange<TEntity> ( (TEntity[]) entityRange );
        }

        private bool add<TEntity> ( TEntity entity ) where TEntity : class
        {
            if ( Connection == null )
                throw new Exception ( "No connection to database" );

            try
            {

                TEntity newEntity;

                // determine type here
                var type = entity.GetType();

                Type baseType =  typeof( TEntity );
                TableAttribute ta = null;

                while ( true )
                {
                    ta = (TableAttribute)Attribute.GetCustomAttribute ( baseType , typeof ( TableAttribute ) );

                    if ( ta != null )
                        break;

                    baseType = baseType.BaseType;

                    if ( baseType == null )
                        break;
                }

                if ( ta == null )
                    throw new ArgumentException ( "Failed to find Table of " + typeof ( TEntity ).ToString ( ) );


                // create an object of the type
                newEntity = (TEntity)Activator.CreateInstance ( type );

                MakeObjectsEqual ( newEntity , entity );

                using ( CSDB context = new CSDB ( Connection ) )
                {
                    context.GetTable<TEntity> ( ).InsertOnSubmit ( newEntity );
                    context.SubmitChanges ( );
                }

                ( (IID)entity ).ID = ( (IID)newEntity ).ID;
                ( (IID)entity ).Discriminator = ( (IID)newEntity ).Discriminator;

                cache.AddToCache ( baseType , (IID)entity );

                return true;
            }
            catch ( Exception ex )
            {
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Error inserting into database of type " + typeof( TEntity ).ToString() );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
                return false;
            }
        }

        private bool addRange<TEntity> ( TEntity[] entities ) where TEntity : class
        {
            if ( Connection == null )
                throw new Exception ( "No connection to database" );

            try
            {
                ArrayList newEntities = new ArrayList(entities.Count());
                TEntity newEntity;

                // determine type here
                var type = typeof( TEntity ); //.GetType(); // entities.GetType();

                foreach ( IID entity in entities )
                {
                    // create an object of the type
                    newEntity = (TEntity)Activator.CreateInstance ( type );

                    MakeObjectsEqual ( newEntity , entity );

                    newEntities.Add ( newEntity );
                }


                using ( CSDB context = new CSDB ( Connection ) )
                {
                    TEntity[] array = (TEntity[]) newEntities.ToArray();
                    context.GetTable<TEntity> ( ).InsertAllOnSubmit ( array );
                    context.SubmitChanges ( );
                }

                for ( int i = 0 ; i < entities.Count ( ) ; i++ )
                {
                    ( (IID)entities[i] ).ID = ( (IID)newEntities[i] ).ID;
                    ( (IID)entities[i] ).Discriminator = ( (IID)newEntities[i] ).Discriminator;
                    cache.AddToCache ( type , (IID)entities[i] );
                }

                return true;


            }
            catch (Exception ex)
            {
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Error inserting range into database of type " + typeof ( TEntity ).ToString ( ) );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
                return false;
            }


        }

        public void End( )
        {
            cache.Clear( );
        }

        #endregion

        #region Reads

        public IList<TEntity> GetAll<TEntity> ( ) where TEntity : class
        {
            if ( Connection == null )
                throw new Exception ( "No connection to database" );

            // try to get the Table (base) type instead of T

            Type baseType =  typeof( TEntity );
            TableAttribute ta = null;

            while ( true )
            {
                ta = (TableAttribute)Attribute.GetCustomAttribute ( baseType , typeof ( TableAttribute ) );

                if ( ta != null )
                    break;

                baseType = baseType.BaseType;

                if ( baseType == null )
                    break;
            }

            if ( ta == null )
                throw new ArgumentException ( "Failed to find Table of " + typeof ( TEntity ).ToString ( ) );

#if(UseCache)
            // Try to get the data from the cache first
            var cacheTable = cache.GetTableFromCache ( baseType );

            if ( cacheTable != null )
            {
                CacheReadCounter++;

                // Casting list makes sure that only valid items are returned.
                // For example if TEntity is Athlete then we need to remove all
                // objects of type person.
                List<TEntity> castingList = new List<TEntity>();

                foreach ( var i in cacheTable )
                    if ( i is TEntity )
                        castingList.Add ( (TEntity)i );

                    return castingList.Cast<TEntity> ( ).ToList ( );
            }

            // could not find this item in the cache or it needs updating.
#endif 
            using ( CSDB context = new CSDB ( Connection ) )
            {
                context.DeferredLoadingEnabled = false;


                IList<TEntity> temp = context.GetTable(baseType).OfType<TEntity>().ToList( );

                if ( typeof ( IID ).IsAssignableFrom ( typeof ( TEntity ) ) )
                {
                    foreach ( TEntity ent in temp )
                        ( (IID)ent ).DState = getDataState ( );

#if(UseCache)
                    cache.AddTableToCache ( baseType , temp.Cast<IID>().ToList() );
#endif
                }

                    DBReadCounter++;
                return temp;
            }
        }

        public TEntity GetID<TEntity> ( int ? ID ) where TEntity : class
        {
            if ( ID == null ) return null;

            if ( Connection == null )
                throw new Exception ( "No connection to database" );


            Type baseType =  typeof( TEntity );
            TableAttribute ta = null;

            while ( true )
            {
                ta = (TableAttribute)Attribute.GetCustomAttribute ( baseType , typeof ( TableAttribute ) );

                if ( ta != null )
                    break;

                baseType = baseType.BaseType;

                if ( baseType == null )
                    break;
            }

            if ( ta == null )
                throw new ArgumentException ( "Failed to find Table of " + typeof ( TEntity ).ToString ( ) );
#if(UseCache)

            // Try to get the data from the cache first
            bool deleted;
            TEntity cacheObj = (TEntity)cache.GetFromCache ( baseType , ID.Value, out deleted);

            if ( deleted ) return null;

            if ( cacheObj != null )
            {
                CacheReadCounter++;
                return cacheObj;
            }
#endif 
            // could not find this item in the cache or it needs updating.

            using ( CSDB context = new CSDB ( Connection ) )
            {
                TEntity temp =  context.GetTable(baseType).OfType<TEntity>().Where ( t => ((IID)t).ID == ID ).ToList ( ).FirstOrDefault();

                if ( temp != null )
                    if ( typeof ( IID ).IsAssignableFrom ( typeof ( TEntity ) ) )
                    {
                        ( (IID)temp ).DState = getDataState ( );
#if(UseCache)
                        cache.AddToCache ( baseType , (IID)temp );
#endif
                    }

                        DBReadCounter++;
                return temp;
            }
        }

        /// <summary>
        /// Used in the deletion and updating process to get the currently stored item to compare against.
        /// </summary>
        private TEntity GetSingle <TEntity> ( int ID , CSDB_BASE context ) where TEntity : class
        {
            if ( Connection == null )
                throw new Exception ( "No connection to database" );

            if ( typeof ( IID ).IsAssignableFrom ( typeof ( TEntity ) ) )
            {
                try
                {
                    return context.GetTable<TEntity> ( ).Where ( r => ( (IID)r ).ID == ID ).ToArray().FirstOrDefault ( );
                }
                catch ( Exception ex )
                {
                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Error selecting single entity from database of type " + typeof ( TEntity ).ToString ( ) );
                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
                    return null;
                    throw;
                }
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Deletion

        public bool Delete<TEntity> ( IID entity ) where TEntity : class
        {
            if ( Connection == null )
                throw new Exception ( "No connection to database" );

            try
            {
                Type baseType =  typeof( TEntity );
                TableAttribute ta = null;

                while ( true )
                {
                    ta = (TableAttribute)Attribute.GetCustomAttribute ( baseType , typeof ( TableAttribute ) );

                    if ( ta != null )
                        break;

                    baseType = baseType.BaseType;

                    if ( baseType == null )
                        break;
                }

                if ( ta == null )
                    throw new ArgumentException ( "Failed to find Table of " + typeof ( TEntity ).ToString ( ) );


                using ( CSDB context = new CSDB ( Connection ) )
                {
#if (DEBUG)
                    TextWriter log =  new StringWriter();
                    context.Log = log;
#endif
                    TEntity t = GetSingle<TEntity> ( entity.ID , context );

                    context.GetTable( baseType ).DeleteOnSubmit ( t );
                    context.SubmitChanges ( );
#if (UseCache)
                    cache.DeleteItemFromCache ( baseType , ( (IID)t ).ID );
#endif                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Error deleting entity from database of type " + typeof ( TEntity ).ToString ( ) );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
                return false;
            }
        }

        public bool DeleteRange<TEntity> ( TEntity[] entities ) where TEntity : class
        {
            if ( Connection == null )
                throw new Exception ( "No connection to database" );

            try
            {
                using ( CSDB context = new CSDB ( Connection ) )
                {
                    // retrieve entities from context

                    HashSet<TEntity> ToBeDeleted = new HashSet<TEntity>();

                    foreach ( IID entity in entities )
                    {
                        TEntity t = GetSingle<TEntity> ( entity.ID , context );
                        if ( t != null )
                            ToBeDeleted.Add( t );
                    }
                    if ( ToBeDeleted.Count == 0 )
                        return false;

                    context.GetTable<TEntity> ( ).DeleteAllOnSubmit ( ToBeDeleted );
                    context.SubmitChanges ( );

                    foreach ( IID entity in entities )
                        cache.DeleteItemFromCache ( typeof ( TEntity ) , ( (IID)entity ).ID );

                    return true;
                }
            }
            catch (Exception ex)
            {
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Error deleting range from database of type " + typeof ( TEntity ).ToString ( ) );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message );
                return false;
            }
        }

        #endregion

        #region Updating

        public bool Update <TEntity> ( IID entity ) where TEntity :class
        {
            if ( entity == null ) return false;

            if ( entity.ID == 0 )
                return Add<TEntity> ( entity );

            using ( CSDB context = new CSDB ( Connection ) )
            {
                TEntity origonal = GetSingle<TEntity>(entity.ID, context);

                MakeObjectsEqual ( origonal , entity );

                context.SubmitChanges ( );

                cache.AddToCache ( typeof ( TEntity ) , entity );

                return true;
            }

        }
        
        #endregion

        #region Update Helpers

        /// <summary>
        /// Compares the properties of two objects of the same type and returns if all properties are equal.
        /// </summary>
        /// <param name="objectA">The first object to compare.</param>
        /// <param name="objectB">The second object to compare.</param>
        /// <returns><c>true</c> if all property values are equal, otherwise <c>false</c>.</returns>
        public static bool MakeObjectsEqual ( object objectA , object objectB )
        {
            bool result;

            if ( objectA != null && objectB != null )
            {
                Type objectType;

                objectType = objectA.GetType ( );

                result = true; // assume by default they are equal

                var properties = objectType.GetProperties(BindingFlags.NonPublic
                    | BindingFlags.Instance 
                    | BindingFlags.Public);

                foreach ( var property in properties )
                {
                    var attributes = property.GetCustomAttributes(false);

                    var columnMapping = attributes.FirstOrDefault(a => a.GetType() == typeof(ColumnAttribute));

                    if ( columnMapping == null ) continue;

                    object valueA = property.GetValue ( objectA , null );
                    object valueB = property.GetValue ( objectB , null );

                    if ( CanDirectlyCompare ( property.PropertyType ) )
                    {
                        if ( !AreValuesEqual ( valueA , valueB ) )
                        {
                            property.SetValue ( objectA , valueB );
                            result = false;
                        }
                    }
                    // if it implements IEnumerable, then scan any items
                    else if ( typeof ( IEnumerable ).IsAssignableFrom ( property.PropertyType ) )
                    {

                        IEnumerable <object> collectionItems1;
                        IEnumerable <object> collectionItems2;
                        int collectionItemsCount1;
                        int collectionItemsCount2;

                        // null check
                        if ( valueA == null && valueB != null || valueA != null && valueB == null )
                        {
                            Console.WriteLine ( "Mismatch with property '{0}.{1}' found." , objectType.FullName , property.Name );
                            property.SetValue ( objectA , valueB );
                            result = false;
                        }
                        else if ( valueA != null && valueB != null )
                        {
                            if ( typeof ( byte[] ) == ( property.PropertyType ) )
                                // this is a byte array (binary data)
                            {
                                if ( ! ( (byte[])valueA ).SequenceEqual ( (byte[])valueB ))
                                    // the binary data has changed
                                    property.SetValue ( objectA , valueB );

                                // move onto next parameter
                                continue;
                            }

                            collectionItems1 = ( (IEnumerable)valueA ).Cast<object> ( );
                            collectionItems2 = ( (IEnumerable)valueB ).Cast<object> ( );
                            collectionItemsCount1 = collectionItems1.Count ( );
                            collectionItemsCount2 = collectionItems2.Count ( );

                            // check the counts to ensure they match
                            if ( collectionItemsCount1 != collectionItemsCount2 )
                            {
                                Console.WriteLine ( "Collection counts for property '{0}.{1}' do not match." , objectType.FullName , property.Name );
                                result = false;
                            }
                            // and if they do, compare each item... this assumes both collections have the same order
                            else
                            {
                                for ( int i = 0 ; i < collectionItemsCount1 ; i++ )
                                {
                                    object collectionItem1;
                                    object collectionItem2;
                                    Type collectionItemType;

                                    collectionItem1 = collectionItems1.ElementAt ( i );
                                    collectionItem2 = collectionItems2.ElementAt ( i );
                                    collectionItemType = collectionItem1.GetType ( );

                                    if ( CanDirectlyCompare ( collectionItemType ) )
                                    {
                                        if ( !AreValuesEqual ( collectionItem1 , collectionItem2 ) )
                                        {
                                            Console.WriteLine ( "Item {0} in property collection '{1}.{2}' does not match." , i , objectType.FullName , property.Name );
                                            result = false;
                                        }
                                    }
                                    else if ( !MakeObjectsEqual ( collectionItem1 , collectionItem2 ) )
                                    {
                                        Console.WriteLine ( "Item {0} in property collection '{1}.{2}' does not match." , i , objectType.FullName , property.Name );
                                        result = false;
                                    }
                                }
                            }
                        }
                        else if ( property.PropertyType.IsClass )
                        {
                            if ( !MakeObjectsEqual ( property.GetValue ( objectA , null ) , property.GetValue ( objectB , null ) ) )
                            {
                                Console.WriteLine ( "Mismatch with property '{0}.{1}' found." , objectType.FullName , property.Name );
                                result = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine ( "Cannot compare property '{0}.{1}'." , objectType.FullName , property.Name );
                            result = false;
                        }
                    }
                }
            }
            else
                result = object.Equals ( objectA , objectB );

            return result;
        }



        /// <summary>
        /// Determines whether value instances of the specified type can be directly compared.
        ///</summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	true if this value instances of the specified type can be directly compared; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanDirectlyCompare ( Type type )
        {
            return typeof ( IComparable ).IsAssignableFrom ( type ) || type.IsPrimitive || type.IsValueType;
        }

        /// <summary>
        /// Compares two values and returns if they are the same.
        /// </summary>
        /// <param name="valueA">The first value to compare.</param>
        /// <param name="valueB">The second value to compare.</param>
        /// <returns><c>true</c> if both values match, otherwise <c>false</c>.<</returns>
        private static bool AreValuesEqual ( object valueA , object valueB )
        {
            bool result;
            IComparable selfValueComparer;

            selfValueComparer = valueA as IComparable;

            if ( valueA == null && valueB != null || valueA != null && valueB == null )
                result = false; // one of the values is null
            else if ( selfValueComparer != null && selfValueComparer.CompareTo ( valueB ) != 0 )
                result = false; // the comparison using IComparable failed
            else if ( !object.Equals ( valueA , valueB ) )
                result = false; // the comparison using Equals failed
            else
                result = true; // match

            return result;
        }

        #endregion

    }
}
