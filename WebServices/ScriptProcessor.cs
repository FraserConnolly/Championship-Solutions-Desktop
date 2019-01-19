using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDjango;
using NDjango.Interfaces;
using NHttp;

namespace ChampionshipSolutions.WebServices
{
    public class WebServerManager
    {
        public WebServerManager(string WorkingDirectory)
        {
            this.WorkingDirectory = WorkingDirectory;
        }

        //private static WebServerManager _instance;

        //public static WebServerManager GetServer()
        //{
        //    if (_instance == null)
        //        _instance = new WebServerManager();
        //    return _instance;
        //}

        private string redirectURL;
        private string errorCode;
        private string errorMessage;
        public string WorkingDirectory { get; internal set; }

        internal bool isRedirecting(out string URL)
        {
            URL = redirectURL;
            return (!string.IsNullOrWhiteSpace(redirectURL));
        }


        public void Redirect(string URL)
        {
            redirectURL = URL;
        }

        internal bool isError(out string errorCode, out string errorMessage)
        {
            errorCode = this.errorCode;
            errorMessage = this.errorMessage;
            return (!string.IsNullOrWhiteSpace(errorCode));
        }

        public void Error(string Code, string Message)
        {
            this.errorCode = Code;
            this.errorMessage = Message;
        }

        public string UseNDjando(string Template, IronPython.Runtime.PythonDictionary Context)
        {
            return BuildNJangoPage(WorkingDirectory, Template, Context);
        }

        internal string BuildNJangoPage(string workingDirectory, string templateFile, IronPython.Runtime.PythonDictionary pyContext)
        {
            TemplateManagerProvider templateProvider = new TemplateManagerProvider();
            ITemplateManager manager = templateProvider.GetNewManager();

            Dictionary<string, object> context = new Dictionary<string, object>();

            foreach (object str in pyContext.Keys)
            {
                context.Add(str.ToString(), pyContext[str]);
            }

            if (File.Exists(workingDirectory + @"\" + templateFile.ToString()))
            {
                TextReader reader = manager.RenderTemplate(
                    workingDirectory + @"\" + templateFile.ToString(),
                    (Dictionary<string, Object>)context);

                return reader.ReadToEnd();
            }
            else
            {
                return "Template not found";
            }
        }
    }

    public static class ScriptProcessor
    {
        internal static string rootWebPageDirectory = "";
        internal static Dictionary<string, object> ApplicationScopeObjects = new Dictionary<string, object>();

        public static void addApplicationScopeObject <T> (string name, T obj)
        {
            if (ApplicationScopeObjects.ContainsKey(name))
                ApplicationScopeObjects[name] = obj;
            else
                ApplicationScopeObjects.Add(name, obj);
        }

        /// <param name="File">Absolute file path</param>
        internal static bool ProcessScriptFile( string File, HttpRequestEventArgs e )
        {
            AWebScript script = null;

            if ( File.EndsWith( ".py" ) )
                script = new PythonWebScript( File, e.Request.GetQueryStringArguments() );

            if ( script == null )
            {
                Debug.WriteLine( $"Error reading script {File}");
                return true;
            }

            foreach ( string name in ApplicationScopeObjects.Keys )
            {
                script.addObject( ApplicationScopeObjects [ name ], name );
            }

            AWebScript.ReturnType result = script.excuteScript( out string HTMLOutput );

            switch ( result )
            {
                case AWebScript.ReturnType.HTML:
                    e.Response.OutputStream.WriteString ( HTMLOutput );

                    return true;

                case AWebScript.ReturnType.Redirect:

                    e.Redirect( script.getRedirectURL( ) );
                    return false;

                case AWebScript.ReturnType.Error:

                    StringBuilder sb = new StringBuilder( );
                    sb.Append( "Error " );
                    sb.AppendLine( script.getErrorCode( ) );
                    sb.AppendLine( script.getErrorMessage( ) );

                    e.Response.OutputStream.WriteString( sb.ToString() );

                    return true;
            }

            return true;

        }
    }

    internal abstract class AWebScript
    {
        public enum ReturnType
        {
            Error,
            HTML,
            Redirect
        }

        protected string filePath;
        protected Dictionary<string, string> arguments;
        protected WebServerManager WebServerManager;

        protected AWebScript ()
        {
            this.filePath = string.Empty;
            this.arguments = new Dictionary<string, string>();
        }

        protected AWebScript (string filePath, Dictionary<string,string> arguments)
        {
            this.filePath = filePath;

            this.arguments = new Dictionary<string, string>();

            foreach (string key in arguments.Keys)
            {
                this.arguments.Add(key, arguments[key]);
            }
        }

        public abstract string Language { get; }

        protected void addArguement(string argument, string value)
        {
            if (this.arguments.ContainsKey(argument))
                this.arguments[argument] = value;
            else
                this.arguments.Add(argument, value);
        }
        
        public bool FileExists()
        {
            return File.Exists(filePath);
        }

        public abstract void addObject(object obj, string name);

        public abstract ReturnType excuteScript(out string HTMLOutput);

        public string getRedirectURL()
        {
            string URL;
            if (WebServerManager.isRedirecting(out URL))
            {
                if (URL[0] == '/')
                    return URL;
                else
                    return '/' + URL;
            }
            else
                return null;
        }

        public string getErrorCode()
        {
            string errorCode, errorMessage;

            if (WebServerManager.isError(out errorCode, out errorMessage))
                return errorCode;
            else
                return "";

        }

        public string getErrorMessage()
        {
            string errorCode, errorMessage;

            if (WebServerManager.isError(out errorCode, out errorMessage))
                return errorMessage;
            else
                return "";
        }
    }

    internal class PythonWebScript : AWebScript 
    {
        protected ScriptEngine pythonEngine;
        protected ScriptScope pythonScope;
        protected ScriptRuntime pythonRuntime;

        private bool compiled;
        private ScriptSource source;
        private CompiledCode compiledScript;

        internal PythonWebScript() : base()
        {
            WebServerManager = new WebServerManager(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory));
            pythonEngine = Python.CreateEngine();
            pythonScope = pythonEngine.CreateScope();
            pythonRuntime = pythonEngine.Runtime;
            ICollection<string> paths = pythonEngine.GetSearchPaths();
            paths.Add(ScriptProcessor.rootWebPageDirectory);
            pythonEngine.SetSearchPaths(paths);
            addObject(WebServerManager, "Server");
        }

        internal PythonWebScript(string filePath, Dictionary<string, string> arguemtents) : base(filePath, arguemtents)
        {
            WebServerManager = new WebServerManager(Path.GetDirectoryName(filePath));
            pythonEngine = Python.CreateEngine();
            pythonScope = pythonEngine.CreateScope();
            pythonRuntime = pythonEngine.Runtime;
            ICollection<string> paths = pythonEngine.GetSearchPaths();
            paths.Add(ScriptProcessor.rootWebPageDirectory);
            pythonEngine.SetSearchPaths(paths);
            addObject( WebServerManager, "Server" );
        }

        public override string Language { get { return "Python"; } }

        public override void addObject(object obj, string name)
        {
            pythonScope.SetVariable(name, obj);
        }

        private void compileScript()
        {
            source = pythonEngine.CreateScriptSourceFromFile(this.filePath);
            compiledScript = source.Compile();
            foreach (string key in arguments.Keys)
                pythonScope.SetVariable(key, arguments[key]);

            // in order to use methods you have to execute the script here.
            //compiledScript.Execute(pythonScope);

            compiled = true;
        }

        public override ReturnType excuteScript( out string HTMLOutput )
        {
            //HTMLOutput = "";

            try
            {
                if (!compiled)
                    compileScript();

                TextWriter tw = new StringWriter();
                MemoryStream ms = new MemoryStream();

                pythonRuntime.IO.SetOutput(ms, tw);

                var result = compiledScript.Execute(pythonScope);

                string errorCode, errorMessage, redirectURL;

                if (WebServerManager.isError(out errorCode, out errorMessage))
                {
                    HTMLOutput = "";
                    return ReturnType.Error;
                }

                if (WebServerManager.isRedirecting(out redirectURL))
                {
                    HTMLOutput = "";
                    return ReturnType.Redirect;
                }

                HTMLOutput = tw.ToString();
                return ReturnType.HTML;
                
            }
            catch (Exception ex)
            {
                HTMLOutput = ex.Message;
                return ReturnType.HTML;
            }
        }

        // I dropped the call method mechanism in favour of the Server object concept.
        //protected dynamic callMethod (string method)
        //{
        //    return callMethod(method, null);
        //}

        //protected dynamic callMethod(string method, dynamic[] arguments)
        //{
        //    try
        //    {

        //        if (!compiled)
        //            compileScript();

        //        var methodPtr = pythonScope.GetVariable(method);

        //        if (methodPtr == null)
        //            return null;

        //        if (arguments == null)
        //            return methodPtr();
        //        else
        //            return methodPtr(arguments);
        //    }
        //    catch (Exception ex)
        //    {
        //        Diagnostics.Diagnostics.WriteLine(ex.Message);
        //    }

        //    return null;
        //}

    }

}
