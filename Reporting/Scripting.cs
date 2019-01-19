using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ChampionshipSolutions.Reporting.Template;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace ChampionshipSolutions.Reporting.Template
{
    public interface IReportTemplate
    {
        string getFileName();

        /// <summary>
        /// Can this template work with the proposed host object.
        /// </summary>
        /// <returns>True if compatible</returns>
        bool isCompatibleHostObject(int TemplateIndex, object Host);

        /// <summary>
        /// Returns an array of descriptions for the Report Templates
        /// </summary>
        string[] getTemplateNames();

        /// <summary>
        /// Populates a DataSet for a host object
        /// </summary>
        /// <param name="TemplateIndex">Index of the Report Template to use.</param>
        DataSet getDataSet(int TemplateIndex, object Host, string Where = null);

        ///// <summary>
        ///// Returns the entire contents of the original XML file.
        ///// </summary>
        //string getXMLFile();
    }
}

namespace ChampionshipSolutions.Reporting
{
    public interface IScripting
    {
        IScriptApplication ScriptApplication { get; }

        string Language { get; }

        void AddTemplate(string Name, string Host, string Code);
        void RemoveTemplate(string Name);
        string[] getTemplates();
        bool hasTemplate(string Name);

        string getTemplateCode(string Name);
        void setTemplateCode(string Name, string Code);

        string getHostObject(string Name);

        string Code { get; }
    }

    internal struct ScriptTemplate
    {
        public string Host;
        public string Name;
        public string Code;
    }

    public abstract class AScripting : IScripting
    {

        public abstract string Language { get; }
        public string UsingStatements { get; set; }
        public string HeaderCode { get; set; }
        public string AdditionalMethods { get; set; }
        internal Dictionary<string, ScriptTemplate> Templates;

        public void AddTemplate(string Name, string Host, string Code)
        {
            if (!Templates.ContainsKey(Name))
                Templates.Add(Name, new ScriptTemplate() { Name = Name, Host = Host, Code = Code });
        }

        public void RemoveTemplate(string Name)
        {
            if (Templates.ContainsKey(Name))
                Templates.Remove(Name);
        }

        public AScripting()
        {
            Templates = new Dictionary<string, ScriptTemplate>();
        }

        public string[] getTemplates()
        {
            return Templates.Keys.ToArray();
        }

        public string Code { get { return buildCode(); } }

        public abstract IScriptApplication ScriptApplication { get; set; }

        protected abstract string buildCode();

        public static List<string> ReferencedAssemblies = new List<string>();

        //public static Assembly CompileCode(string code, string FileName, out CompilerResults result, string[] resourceFilePaths = null)

        //public static IReportTemplate ConstructReportTemplate(Assembly script)

        public string getTemplateCode(string Name)
        {
            if (Templates.ContainsKey(Name))
                return Templates[Name].Code;
            else
                return string.Empty;
        }

        public void setTemplateCode(string Name, string Code)
        {
            if (Templates.ContainsKey(Name))
            {
                ScriptTemplate st = Templates[Name];
                st.Code = Code;
                Templates[Name] = st;
            }
        }

        public string getHostObject(string Name)
        {
            if (Templates.ContainsKey(Name))
                return Templates[Name].Host;
            else
                return string.Empty;
        }

        public bool hasTemplate(string Name)
        {
            return Templates.ContainsKey(Name);
        }

    }

    public class CSharpScripting : AScripting, IScripting
    {
        #region Template Code

        public const string TemplateClass =
@"using System;
using System.Data;
using FConn.Reporting;
{0}

namespace FConn.Reporting.Template
{{
    public class {1} : IReportTemplate
    {{
        string[] Descriptions = new string[] 
        {{
            {2}
        }};

        public string getFileName()
        {{
            return ""{3}"";
        }}

        public bool isCompatibleHostObject(int TemplateIndex, object Host)
        {{
            {4}

            return false;
        }}

        public string[] getTemplateNames()
        {{
            return Descriptions;
        }}

        public DataSet getDataSet(int TemplateIndex, object Host, string Where = null)
        {{
            {5}

            {6}

            return null;
        }}

        {7}

    }}
}}
";

        #endregion 

        public override string Language { get { return "CSharp"; } }

        // todo
        public override IScriptApplication ScriptApplication
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        protected override string buildCode()
        {
            string templateNames = "";
            string hostCompatibility = "";
            //string usingStatements = ScriptInstructions.SelectSingleNode("/FConnTemplate/Report/Script/UsingStatements").InnerText;
            //string scriptHeader = ScriptInstructions.SelectSingleNode("/FConnTemplate/Report/Script/Header").InnerText;
            //string additionalMethods = ScriptInstructions.SelectSingleNode("/FConnTemplate/Report/Script/AdditionalMethods").InnerText;
            string script = "";
            //string XML = instructions.InnerXml;

            if (Templates.Count > 0)
            {
                hostCompatibility =
@"     switch (TemplateIndex)
       {
";
                script =
@"     switch (TemplateIndex)
       {
";
            }

            int c = 0;

            // for each template name in the report
            foreach (ScriptTemplate s in Templates.Values)
            {
                templateNames += @"""" + s.Name + @""",";

                hostCompatibility += "\tcase " + c + ":\n";
                hostCompatibility += "\t\tif(Host is " + s.Host + ")\n";
                hostCompatibility += "\t\t\treturn true;\n";
                hostCompatibility += "\t\tbreak;\n";

                script += "\tcase " + c + ":\n";
                script += s.Code;
                script += "\n"; //"\t\tbreak;\n";

                c++;
            }

            templateNames = templateNames.TrimEnd(new char[] { ',' });

            hostCompatibility +=
@"    default:
        break;
}
";

            script +=
@"    default:
        break;
}
";


            return string.Format(CSharpScripting.TemplateClass,
                UsingStatements,    // using statements
                "FConnReportTemplate", // class name
                templateNames,
                "",                 // filename
                hostCompatibility,  // host compatibility
                HeaderCode,       // Script Header
                script,             // Script
                AdditionalMethods  // Additional Methods
                );

            // 0 - using statements
            // 1 - Class Name
            // 2 - Descriptions
            // 3 - FileName
            // 4 - host compatibility switch
            // 5 - Script Header
            // 6 - Indexed Script
            // 7 - Additional Methods
            // 8 - Inner XML

        }

        public static Assembly CompileCode(string code, string FileName, out CompilerResults result, string[] resourceFilePaths = null )
        {
            Microsoft.CSharp.CSharpCodeProvider csProvider = new Microsoft.CSharp.CSharpCodeProvider();

            CompilerParameters options = new CompilerParameters();

            options.GenerateExecutable = false; // we want a Dll (or "Class Library" as its called in .Net)

            if (FileName != null)
            {
                options.OutputAssembly = FileName;
            }
            else
            {
                options.GenerateInMemory = true;
            }

            // Add any references you want the users to be able to access, be warned that giving them access to some classes can allow
            // harmful code to be written and executed. I recommend that you write your own Class library that is the only reference it allows
            // thus they can only do the things you want them to.
            // (though things like "System.Xml.dll" can be useful, just need to provide a way users can read a file to pass in to it)
            // Just to avoid bloating this example to much, we will just add THIS program to its references, that way we don't need another
            // project to store the interfaces that both this class and the other uses. Just remember, this will expose ALL public classes to
            // the "script"

            //var assemblies = AppDomain.CurrentDomain
            //                .GetAssemblies()
            //                .Where(a => !a.IsDynamic)
            //                .Select(a => a.Location);

            //options.ReferencedAssemblies.AddRange(assemblies.ToArray());

            options.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            ReferencedAssemblies.ForEach(x => options.ReferencedAssemblies.Add(x));
            //options.ReferencedAssemblies.Add("System.Data.dll");
            //options.ReferencedAssemblies.Add("System.dll");
            //options.ReferencedAssemblies.Add("System.Xml.dll");
            //options.ReferencedAssemblies.Add("System.Data.Linq.dll");
            //options.ReferencedAssemblies.Add("ChampionshipSolutions.DM.dll");
            //options.ReferencedAssemblies.Add("mscorlib.dll");
            //options.ReferencedAssemblies.Add("System.Core.dll");

            if (resourceFilePaths != null)
                foreach (string filePath in resourceFilePaths)
                    if ( System.IO.File.Exists (filePath))
                        options.EmbeddedResources.Add(filePath);

            // Compile our code
            //CompilerResults result;
            result = csProvider.CompileAssemblyFromSource(options, code);

            if (result.Errors.HasErrors)
            {
                // TODO: report back to the user that the script has errored
                return null;
            }

            if (result.Errors.HasWarnings)
            {
                // TODO: tell the user about the warnings, might want to prompt them if they want to continue
                // running the "script"
            }

            return result.CompiledAssembly;
        }

        public static IReportTemplate ConstructReportTemplate(Assembly script)
        {
            // Now that we have a compiled script, lets run them
            foreach (Type type in script.GetExportedTypes())
            {
                foreach (Type iface in type.GetInterfaces())
                {
                    if (iface == typeof(ChampionshipSolutions.Reporting.Template.IReportTemplate))
                    {
                        // Get the constructor for the current type
                        ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                        if (constructor != null && constructor.IsPublic)
                        {
                            ChampionshipSolutions.Reporting.Template.IReportTemplate template = constructor.Invoke(null) as ChampionshipSolutions.Reporting.Template.IReportTemplate;
                            if (template != null)
                            {
                                return template;
                            }
                            else
                            {
                                // hmmm, for some reason it didn't create the object
                                // this shouldn't happen, as we have been doing checks all along, but we should
                                // inform the user something bad has happened, and possibly request them to send
                                // you the script so you can debug this problem
                            }
                        }
                        else
                        {
                            // and even more friendly and explain that there was no valid constructor
                            // found and that's why this script object wasn't run
                        }
                    }
                }
            }
            return null;
        }

    }

    public class PythonScripting : AScripting, IScripting
    {
        ScriptEngine pythonEngine; 
        ScriptScope pythonScope;
        ScriptRuntime pythonRuntime;

        public PythonScripting():base()
        {
            pythonEngine = Python.CreateEngine();
            pythonScope = pythonEngine.CreateScope();
            pythonRuntime = pythonEngine.Runtime;
        }

        public override string Language { get { return "Python"; } }

        private IScriptApplication _ScriptApplication;

        public override IScriptApplication ScriptApplication
        {
            get { return _ScriptApplication; }
            set
            {
                _ScriptApplication = value;
                pythonScope.SetVariable("ScriptApplication", value);
            }
        }

        /// <summary>
        /// HeaderCode is currently ignored in Python
        /// </summary>
        protected override string buildCode()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine(UsingStatements);

            str.AppendLine("");

            str.AppendLine(AdditionalMethods);

            str.AppendLine("");

            foreach (ScriptTemplate t in Templates.Values)
            {
                str.Append("def ");
                str.Append(t.Name);
                str.Append("( ");
                str.Append(t.Host);
                str.AppendLine(" Host )");

                foreach (string line in t.Code.Split('\n'))
                {
                    str.Append('\t');
                    str.AppendLine(line.TrimEnd());
                }
                str.AppendLine("");
            }

            return str.ToString();

        }            
        
        public static string CleanPythonTabs ( string code )
        {
            List<string> lines = code.Split('\r').ToList();

            string firstline;
            int firstLineTabs;

            for (; string.IsNullOrWhiteSpace(lines.First());)
            {
                if (lines.Count == 0) return "";
                lines.RemoveAt(0);
            }

            firstline = lines.First().TrimStart(' ');

            firstLineTabs = CountTabs(firstline);

            StringBuilder str = new StringBuilder();

            foreach (string line in lines)
            {
                int tabCount = CountTabs(line);
                if (tabCount == firstLineTabs)
                {
                    str.AppendLine(line.Trim().TrimStart('\t'));
                }
                else if (tabCount < firstLineTabs)
                {
                    string t = line.Trim().TrimStart('t');
                    str.AppendLine(t);
                }
                else if (tabCount > firstLineTabs)
                {
                    // remove all tabs
                    string t = line.Trim().TrimStart('t');

                    // Add back in the correct number of tabs
                    for (int i = 0; i < tabCount - firstLineTabs; i++)
                        t = t.Insert(0, "\t");

                    str.AppendLine(t);
                }
            }

            return str.ToString();
        }

        private static int CountTabs(string line)
        {
            int tabCount =0;
            foreach (char c in line)
                if (c == '\t')
                    tabCount++;
            return tabCount;
        }


        //ScriptSource source = pythonEngine.CreateScriptSourceFromString(;
        //CompiledCode compiled = source.Compile();
        //compiled.Execute(pythonScope);

    }
}
