using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM.DataAccess
{
    public partial class Database
    {

        //public IList<Template> GetAllTemplates()
        //{
        //    return GetAll<DM.Template> ( );
        //}

        //public bool Update ( Template entity )
        //{
        //    if ( entity.ID == 0 )
        //        Add<Template> ( entity );


        //    using ( CSDB_Templates context = new CSDB_Templates ( Connection ) )
        //    {
        //        Template origonal = GetSingle<Template>(entity.ID, context);

        //        //// Just grabbing this to get hold of the type name:
        //        //var type = entity.GetType();

        //        //// Get the PropertyInfo object:
        //        //var properties = entity.GetType().GetProperties();
        //        //Console.WriteLine ( "Finding properties for {0} ..." , type.Name );

        //        MakeObjectsEqual ( origonal , entity );

        //        context.SubmitChanges ( );
        //        return true;
        //    }
        //}
    }

}
