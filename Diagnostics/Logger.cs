using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ChampionshipSolutions.Diag
{

    public enum MessagePriority
    {
        Critical, Error, Warning, Infomation
    }

    public class Diagnostics
    {
        private static Diagnostics _instance;
        private static MessagePriority logLevel ;

        public static Diagnostics Logger
        {
            get
            {
                if (_instance == null)
                    _instance = new Diagnostics();

                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        ~Diagnostics()
        {
            if (logStream != null)
            {
                try
                {
                LogLine("Logging finished");
                logStream.Flush();
                logStream.Close();
                _instance = null;
                }
                catch (Exception)
                {
                }
            }
        }

        public Diagnostics getLogger() { return Logger; }

        public static bool ready
        {
            get
            {
                if (logStream == null)
                    return false;
                return logStream.BaseStream.CanWrite;
            }
            
        }

        public static string FileName { get; private set; }

        public delegate void errorLoggedDelegate(string errorMessage);

        public static errorLoggedDelegate errorLogged;

        private static StreamWriter logStream;

        public static List<string> Logs;

        private Diagnostics()
        {
            //try
            //{
            //    logLevel = (MessagePriority)Properties.Settings.Default.LogLevel;
            //}
            //catch 
            //{
            //    logLevel = MessagePriority.Warning ;
            //}

            logLevel = MessagePriority.Infomation;
            Logs = new List<string>();

            try
            {
                string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FConn Diagnostics");

                if ( !Directory.Exists ( tempPath ) )
                    Directory.CreateDirectory ( tempPath );

                FileName = tempPath + @"\" + DateTime.Now.ToString().Replace(':', '-') + ".log";
                logStream = File.AppendText(FileName);
                logStream.AutoFlush = true;
                logStream.WriteLine("Log file created at {0}", DateTime.Now.ToString());
            }
            catch (Exception)
            {
            }
        }

        private static object WriteLock = new object();

        public static bool WriteLine(string value)
        {
            if (!ready)
                Logger.getLogger();

            if (!ready)
                return false;

            lock ( WriteLock )
            {
                try
                {
                    Logs.Add ( value );
                    Diagnostics.logStream.WriteLine ( value );
                    System.Diagnostics.Debug.WriteLine ( value );
                    return true;
                }
                catch ( Exception )
                {
                    return false;
                }
            }
        }

        public List<string> getLogs()
        {
            return Logs;
        }

        public static bool WriteLine(string format, params object[] arg)
        {
            return WriteLine(String.Format(format, arg));
        }

        public static bool LogLine(string message, MessagePriority priority = MessagePriority.Infomation)
        {
            switch (priority)
            {
                case MessagePriority.Critical:
                    break;
                case MessagePriority.Error:
                    break;
                case MessagePriority.Warning:
                    errorLogged?.Invoke( message );
                    break;
                case MessagePriority.Infomation:
                    break;
                default:
                    break;
            }

            // ToDo fix this so that the log level check works.
            return WriteLine(String.Format("{0} \t{1} \t{2}", DateTime.Now.ToString(), priority.ToString(""), message));
        
            //    if (priority <= logLevel)
            //        return WriteLine(String.Format("{0} \t{1} \t{2}", DateTime.Now.ToString(), priority.ToString(""), message));
            //    else
            //        return false;
        }

    }
}
