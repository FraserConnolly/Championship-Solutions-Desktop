using System;

using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading;
using static System.Diagnostics.Debug;
using System.Collections.Generic;
using NHttp;
using ChampionshipSolutions.WebServices;
using System.Collections.Specialized;
using System.Linq;

namespace ChampionshipSolutions
{
    public delegate MemoryStream DynamicFileRequestedEventHandler( object sender, DynamicFileRequestedEventArgs e );

    public class WebServer
    {
        private readonly HttpServer server;
        private readonly string ServerDirectory;

        public event DynamicFileRequestedEventHandler DynamicFileRequestedEvent;

        public WebServer( string path, int Port = 80 )
        {
            server = new HttpServer
            {
                EndPoint = new IPEndPoint( IPAddress.Any, Port )
            };

            ServerDirectory = path;
            ScriptProcessor.rootWebPageDirectory = path;

            server.RequestReceived += Server_RequestReceived;

            server.Start( );
        }

        private void Server_RequestReceived( object sender, HttpRequestEventArgs e )
        {
            WriteLine( $"Raw URL: {e.Request.RawUrl}" );
            WriteLine( $"URL: {e.Request.Url}" );
            WriteLine( $"Path: {e.Request.Path}" );

            string ErrorMessage = "";
            string DirectoryName = e.Request.Directory();
            string FileName = e.Request.FileName();
            string Path = e.Request.Path;
            string [ ] splitPath = Path.Split( '/' );

            if ( !Directory.Exists(ServerDirectory))
            {
                ErrorMessage = "<H2>Error!! Requested Directory does not exists</H2><Br>";
                WriteLine( "HTTP folder not found: " + ServerDirectory + DirectoryName );

                BuildHeader( e, 404, "404 Not Found" );

                e.Response.OutputStream.WriteString( ErrorMessage );
                return;
            }

            if ( e.Request.Path.EndsWith("/") )
            {
                // we need to find the default file

                FileName = DefaultFileName( );
            }

            MemoryStream virtualFileStream = null;

            DynamicFileRequestedEventArgs fe = new DynamicFileRequestedEventArgs( e );
            if ( DynamicFileRequestedEvent != null )
            {
                virtualFileStream = DynamicFileRequestedEvent( this, fe );
            }

            if ( virtualFileStream != null )
            {

                // send virtual file
                e.Response.ContentType
                    = mimeTypeMappings.TryGetValue(
                        System.IO.Path.GetExtension( fe.FileName ), out string mime )
                        ? mime : "application/octet-stream";

                e.Response.CacheControl = "no-cache";

                virtualFileStream.CopyTo( e.Response.OutputStream );

                return;
            }


            string filePath =
                DirectoryName == "/" ? System.IO.Path.Combine( ServerDirectory, FileName ) :
                System.IO.Path.Combine( ServerDirectory, DirectoryName, FileName );

            if ( FileName.EndsWith( ".py" ) )
            {
                ScriptProcessor.ProcessScriptFile( filePath, e );
                return;
            }
            else
            {
                // look for a static file


                if ( File.Exists( filePath ) )
                {
                    try
                    {
                        // set mime type

                        e.Response.ContentType 
                            = mimeTypeMappings.TryGetValue( 
                                System.IO.Path.GetExtension( FileName ), out string mime ) 
                                ? mime : "application/octet-stream";

                        if ( mime.Contains( "image" ) || mime.Contains( "javascript" ) || mime.Contains( "css" ) )
                        {
                            // allow images, css and javascript to cache
                            e.Response.CacheControl = "max-age=36000";
                        }
                        else
                        {
                            e.Response.CacheControl = "no-cache";
                        }

                        Stream fs = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.Read );

                        fs.CopyTo( e.Response.OutputStream );

                        fs.Close( );
                        return;
                    }
                    catch
                    {

                    }
                }
                else
                {
                    // return 404
                    ErrorMessage = "<H2>404 Error! File Does Not Exist...</H2>";
                    BuildHeader( e, 404, "404 Not Found" );
                    e.Response.OutputStream.WriteString( ErrorMessage );
                }
            }
        }

        private void BuildHeader ( HttpRequestEventArgs e, int statusCode,  string status, string mime = "text/html" )
        {
            e.Response.StatusCode = statusCode;
            e.Response.Status = status;
            // set MIME type
            // set status code
            // set HTTP version
            // set Cache
        }

        public void ShutDown ( )
        {
            server?.Stop( );
        }

        private static readonly string [ ] defaultFiles = new string [ ]
        {
                "index.htm",
                "index.html",
                "index.py",
                "default.htm",
                "default.html",
                "default.py",
        };

        private static IDictionary<string, string> mimeTypeMappings = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase ) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".py",  "text/html" },
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };


        private string DefaultFileName()
        {
            foreach ( string f in defaultFiles )
            {
                //Look for the default file in the web server root folder
                if ( File.Exists( ServerDirectory + f ) == true )
                {
                    return f;
                }
            }

            return "";
        }

    }

    public class DynamicFileRequestedEventArgs : EventArgs
    {
        public HttpRequestEventArgs request;

        public DynamicFileRequestedEventArgs( HttpRequestEventArgs args )
        {
            request = args;
        }

        public string FileName => request.Request.FileName( );
        public string FilePath => request.Request.Directory( );
        public string FullPath => Path.Combine( FilePath, FileName );
        public NameValueCollection Arguments => request.Request.QueryString; 
    }

    static class HTTPExtensions
    {
        public static void WriteString ( this HttpOutputStream stream, string body )
        {
            byte[] eBody = Encoding.UTF8.GetBytes( body ?? "" );

            stream.Write( eBody, 0, eBody.Length );
        }

        public static string FileName ( this HttpRequest request )
        {
            return System.Web.HttpUtility.UrlDecode(request.Path.Split( '/' ).Last( ));
        }

        public static string Directory( this HttpRequest request )
        {

            string dir = @"/";
            string [ ] splitPath = System.Web.HttpUtility.UrlDecode(request.Path).Split( '/' );

            if ( splitPath.Length > 1 )
            {
                for ( int i = 1 ; i < splitPath.Length - 1 ; i++ )
                {
                    dir += splitPath [ i ] + "/";
                }
            }

            return dir;
        }

        public static Dictionary<string,string> GetQueryStringArguments( this HttpRequest request )
        {
            Dictionary<string, string> d = new Dictionary<string, string>( );

            foreach ( string name in request.QueryString.AllKeys )
            {
                d.Add( name, request.QueryString[name] );
            }

            return d;
        }

        public static void Redirect( this HttpRequestEventArgs e, string url )
        {
            StringBuilder sb = new StringBuilder( );

            sb.AppendLine( "<!doctype html>" );
            sb.AppendLine( "<html>" );
            sb.AppendLine( "<head>" );
            sb.AppendLine( @"<meta charset = ""utf -8""> " );
            sb.AppendLine( @"<meta http-equiv=""refresh"" content=""0;URL=" + url + @""" />" );
            sb.AppendLine( "</head>" );
            sb.AppendLine( "<body>" );
            sb.AppendLine( "</body>" );
            sb.AppendLine( "</html>" );

            e.Response.OutputStream.WriteString( sb.ToString( ) );
        }


    }

}
