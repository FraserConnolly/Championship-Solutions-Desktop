using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace ChampionshipSolutions.DM
{
    [Table(Name = "Templates")]
    public class Template : IID 
    {
        //public Template ( DM.Template entity ) 
        //{
        //    ID = entity.ID;
        //    InsertedDate = entity.InsertedDate;
        //    TemplateName = entity.TemplateName;
        //    Description = entity.Description;
        //    Instructions = entity.Instructions;
        //    TemplateFile = entity.TemplateFile;
        //}
        public DataState DState { get; set; }

        [Column(IsPrimaryKey = true)]
        public int ID { get; set; }

        public string Discriminator { get; set; }

        [Column (Name = "InsertedDate")]
        public DateTime InsertedDate { get; set; }

        [Column (Name = "TemplateName")]
        public string TemplateName { get; set; }

        [Column (Name = "Description")]
        public string Description { get; set; }

        [Column (Name = "Instructions")]
        public string Instructions { get; set; }

        [Column (Name = "TemplateFile")]
        public byte[] TemplateFile { get; set; }

        public override bool Equals ( object obj )
        {
            try
            {
                return ( (Template)obj ).ID == this.ID;
            }
            catch 
            {
                return base.Equals ( obj );
            }
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public void voidStorage ( bool softRefresh = true )
        {
            // do nothing as there are no relationships to refresh.
        }

        public static bool operator == ( Template x , Template y )
        {
            if ( ( (object)x ) == null && ( (object)y ) == null ) return true;
            if ( ( (object)x ) == null ) return false;
            if ( ( (object)y ) == null ) return false;

            return x.ID == y.ID;
        }

        public static bool operator != ( Template x , Template y )
        {
            return !( x == y );
        }

        public void Save ( )
        {
            if ( DState == null ) return;
            if ( DState.IO == null ) return;

            DState.IO.Update<Template> ( this );
        }

    }
}

//namespace ChampionshipSolutions.DM
//{
//    public class Template 
//    {
//        public Template ( Tables.Template entity )
//        {
//            Description = entity.Description;
//            ID = entity.ID;
//            InsertedDate = entity.InsertedDate;
//            Instructions = entity.Instructions;
//            TemplateName = entity.TemplateName;
//            TemplateFile = entity.TemplateFile;
//        }

//        public int ID { get; set; }

//        public DateTime InsertedDate { get; set; }

//        public string TemplateName { get; set; }

//        public string Description { get; set; }

//        public string Instructions { get; set; }

//        public byte[] TemplateFile { get; set; }
//    }
//}
