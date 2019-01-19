using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChampionshipSolutions.Reporting.Template;
using System.IO;
using Ionic.Zip;

namespace ChampionshipSolutions.Reporting
{
    //public class ReportLibrary
    //{
    //    private static List<IReportTemplate> Templates;

    //    public static void SeachForTemplates(string Directory, string TemplateExtension)
    //    {
    //        string workingDirectory = Directory;
    //        string extention = TemplateExtension;

    //        if ( string.IsNullOrWhiteSpace (workingDirectory) )
    //            workingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

    //        if (string.IsNullOrWhiteSpace(extention))
    //            extention = "fcr";

    //        string[] allFiles = System.IO.Directory.GetFiles(workingDirectory);

    //        string[] matchingFiles = allFiles.Where(f => f.EndsWith(extention)).ToArray();
    //    }
    //}
}
