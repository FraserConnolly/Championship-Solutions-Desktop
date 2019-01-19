#define ForceSingeInstance

using ChampionshipSolutions.ControlRoom.Scanning;
using ChampionshipSolutions.Controls;
using ChampionshipSolutions.FileIO;
using ChampionshipSolutions.Reports;
using ChampionshipSolutions.ViewModel;
using MicroMvvm;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using Xceed.Wpf.Toolkit;
using static ChampionshipSolutions.FileIO.FConnFile;

namespace ChampionshipSolutions
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public ChampionshipVM CurrentChampionship { get; set; }
        public ObservableCollection<MainPageControl> controls;
        public MRUManager mruManager;
        public CSReportLibrary ReportLibrary;// = CSReportLibrary.getLibrary();
        public EventsPage EventPage { get; set; }

        public ICommand OpenFile { get { return new RelayCommand(openFile, canOpenFile); } }
        public ICommand CloseFile { get { return new RelayCommand(closeFile, canCloseFile); } }
        public ICommand NewSingleFile { get { return new RelayCommand(newSingleFile, canOpenFile); } }
        //public ICommand NewMultipleFile { get { return new RelayCommand(newMultipleFile, canOpenFile); } }

        // OpenScanner and associated functions are no longer required as scanner now has it's own MainPage.
            //public ICommand OpenScanner { get { return new RelayCommand(openScanner); } }

            //private static Scanning.Scanning scanner;

            //private void openScanner()
            //{
            //    if (scanner == null)
            //        scanner = new Scanning.Scanning ( );

            //    scanner.Show();
            //}

        private void newSingleFile()
        {
            string newFilePath = getFilePath();

            if (newFilePath == null) return;

            CreateSingleChampionshipFile(newFilePath);

            openFile ( newFilePath);
        }

        //private void newMultipleFile()
        //{
        //    string newFilePath = getFilePath();

        //    if (newFilePath == null) return;

        //    CreateMultipleChampionshipFile(newFilePath);

        //    openFile ( newFilePath);
        //}

        /// <summary>
        /// Opens the championship file. There is a background worker for the really hard stuff.
        /// I.e. loading the events data.
        /// </summary>
        /// <param name="FilePath"></param>
        internal void openFile ( string FilePath )
        {
            BackgroundWorker worker = new BackgroundWorker();

            Stopwatch watch = new Stopwatch();

            if ( !System.IO.File.Exists ( FilePath ) )
                return;

            if ( FilePath.ToLower( ).Contains( "appdata" ) )
            {
                MessageBoxResult msg = MessageBox.Show("The file you are trying to open is stored in a temporary location. " +
                    "\nWe strongly recommend that you move this file to your Desktop or your Documents directory before opening it." +
                    "\nDo you wish to continue to open this file? ", 
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if ( msg == MessageBoxResult.No )
                    return; // abort the load process

            }

            if ( FilePath.ToLower( ).Contains( ".zip" ) || FilePath.ToLower( ).Contains( ".rar" ) )
            {
                MessageBoxResult msg = MessageBox.Show("The file you are trying to open is inside a compressed archive." +
                    "\nWe strongly recommend that you extract the file to your Desktop or your Documents directory before opening it." +
                    "\nDo you wish to continue? ", 
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if ( msg == MessageBoxResult.No )
                    return; // abort the load process
            }


            watch.Start ( );

            ChampionshipSolutions.MainWindow.BusyIndicator.IsBusy = true;

            if ( FConnFile.OpenFile ( FilePath ) )
            {

                if ( mruManager != null )
                {
                    Dispatcher.Invoke ( new Action ( ( ) =>
                    {
                        mruManager.AddRecentFile ( FilePath );
                        mruManager.IsEnabled = false;
                    } ) , null );
                }

                controls.Clear ( );

                ( (App)App.Current ).CurrentChampionship = new ChampionshipVM ( );
                //CurrentChampionship.Championship.database = GetFileDetails ( ).IO;
                this.EventPage = new EventsPage ( );


                worker.DoWork += ( o , a ) =>
                {
                    Console.WriteLine ( "Loading Events" );

                    this.EventPage.ReloadPage ( );
                    Dispatcher.Invoke ( new Action ( ( ) =>
                        {
                        } ) , null );
                };


                worker.RunWorkerCompleted += ( o , a ) =>
                {
                    controls.Add ( new MainPageControl ( ) { Content = "Championship" , isEnabled = true , DisplayPage = new ChampionshipPage ( ) } );
                    Console.WriteLine ( "Loading Teams" );
                    controls.Add ( new MainPageControl ( ) { Content = "Teams" , isEnabled = true , DisplayPage = new TeamsPage ( ) } );
                    Console.WriteLine ( "Loading Schools" );
                    controls.Add ( new MainPageControl ( ) { Content = "Schools & Clubs" , isEnabled = true , DisplayPage = new SchoolsPage ( ) } );
                    Console.WriteLine ( "Loading Athletes" );
                    controls.Add ( new MainPageControl ( ) { Content = "Athletes" , isEnabled = true , DisplayPage = new AthltesPage ( ) } );
                    Console.WriteLine ( "Loading Groups" );
                    controls.Add ( new MainPageControl ( ) { Content = "Groups" , isEnabled = true , DisplayPage = new GroupsPage ( ) } );
                    controls.Add ( new MainPageControl ( ) { Content = "Events" , isEnabled = true , DisplayPage = EventPage } );
                    controls.Add ( new MainPageControl ( ) { Content = "Import Results" , isEnabled = !IsEntryForm( ) , DisplayPage = new ImportResultsPage ( ) } );
                    controls.Add ( new MainPageControl ( ) { Content = "Scan Results" , isEnabled = !IsEntryForm( ) , DisplayPage = new ScannerPage ( ) } );
                    controls.Add ( new MainPageControl ( ) { Content = "Reports" , isEnabled = true , DisplayPage = new ReportsPage ( ) } );
                    //controls.Add ( new MainPageControl ( ) { Content = "Exports" , isEnabled = false , DisplayPage = new ExportsPage ( ) } );
                    controls.Add ( new MainPageControl ( ) { Content = "Web Server" , isEnabled = !IsEntryForm( ) , DisplayPage = new WebServerPage ( ) } );
                    //controls.Add ( new MainPageControl ( ) { Content = "Scripts" , isEnabled = false , DisplayPage = new ScriptsPage ( ) } );
                    Console.WriteLine ( "Loading Templates" );
                    controls.Add ( new MainPageControl ( ) { Content = "Templates" , isEnabled = !IsEntryForm( ) , DisplayPage = new TemplatesPage ( ) } );


                    ReportLibrary = CSReportLibrary.getLibrary ( );


                    ChampionshipSolutions.MainWindow.Frame.Navigate ( new OpenedPage ( ) );
                    watch.Stop ( );

                    ChampionshipSolutions.MainWindow.BusyIndicator.IsBusy = false ;

                    Console.WriteLine ( "**************************" );
                    Console.WriteLine ( "*****Loading complete*****" );
                    Console.WriteLine ( "*File open took " + watch.Elapsed.TotalSeconds + " seconds.**" );
                    Console.WriteLine ( "********DB Reads = " + GetFileDetails().IO.DBReadCounter + "*****" );
                    Console.WriteLine ( "*****Cache Reads = " + GetFileDetails().IO.CacheReadCounter + "***" );
                    Console.WriteLine ( "**************************" );

                    ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Opened File at " + FilePath + " in " + watch.Elapsed.TotalSeconds + " seconds." , ChampionshipSolutions.Diag.MessagePriority.Infomation );
                };

                worker.RunWorkerAsync ( );

            }
            else
            {
                ChampionshipSolutions.MainWindow.BusyIndicator.IsBusy = false ;
            }

        }

        [Obsolete ("This opens the file whilst doing the hard work on the UI thread")]
        internal void openFileSync ( string FilePath )
        {
            if ( !System.IO.File.Exists ( FilePath ) )
                return;

            Stopwatch watch = new Stopwatch();
            watch.Start ( );

            ChampionshipSolutions.MainWindow.BusyIndicator.IsBusy = true;

            if ( FConnFile.OpenFile ( FilePath ) )
            {
                if ( mruManager != null )
                {
                    mruManager.AddRecentFile ( FilePath );
                    mruManager.IsEnabled = false;
                }

                controls.Clear ( );

                ( (App)App.Current ).CurrentChampionship = new ChampionshipVM ( );
                //CurrentChampionship.Championship.database = GetFileDetails ( ).IO;

                Console.WriteLine ( "Loading Events" );

                this.EventPage = new EventsPage ( );
                this.EventPage.ReloadPage ( );

                controls.Add ( new MainPageControl ( ) { Content = "Championship" , isEnabled = true , DisplayPage = new ChampionshipPage ( ) } );
                Console.WriteLine ( "Loading Teams" );
                controls.Add ( new MainPageControl ( ) { Content = "Teams" , isEnabled = true , DisplayPage = new TeamsPage ( ) } );
                Console.WriteLine ( "Loading Schools" );
                controls.Add ( new MainPageControl ( ) { Content = "Schools & Clubs" , isEnabled = true , DisplayPage = new SchoolsPage ( ) } );
                Console.WriteLine ( "Loading Athletes" );
                controls.Add ( new MainPageControl ( ) { Content = "Athletes" , isEnabled = true , DisplayPage = new AthltesPage ( ) } );
                Console.WriteLine ( "Loading Groups" );
                controls.Add ( new MainPageControl ( ) { Content = "Groups" , isEnabled = true , DisplayPage = new GroupsPage ( ) } );
                controls.Add ( new MainPageControl ( ) { Content = "Events" , isEnabled = true , DisplayPage = EventPage } );
                controls.Add ( new MainPageControl ( ) { Content = "Import Results" , isEnabled = true , DisplayPage = new ImportResultsPage ( ) } );
                controls.Add ( new MainPageControl ( ) { Content = "Web Server" , isEnabled = true , DisplayPage = new WebServerPage ( ) } );
                controls.Add ( new MainPageControl ( ) { Content = "Reports" , isEnabled = true , DisplayPage = new ReportsPage ( ) } );
                controls.Add ( new MainPageControl ( ) { Content = "Exports" , isEnabled = true , DisplayPage = new ExportsPage ( ) } );
                controls.Add ( new MainPageControl ( ) { Content = "Scanner" , isEnabled = true , DisplayPage = new ScannerPage ( ) } );
                Console.WriteLine ( "Loading Templates" );
                controls.Add ( new MainPageControl ( ) { Content = "Templates" , isEnabled = true , DisplayPage = new TemplatesPage ( ) } );


                ReportLibrary = CSReportLibrary.getLibrary ( );
                ChampionshipSolutions.MainWindow.Frame.Navigate ( new OpenedPage ( ) );
                ChampionshipSolutions.MainWindow.BusyIndicator.IsBusy = false;

            }

            watch.Stop ( );

#if ( DEBUG )
            MessageBox.Show ( "File open took " + watch.Elapsed.TotalSeconds + " seconds." );
#endif
        }

        private void openFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Championship Solutions Database (.csdb)|*.csdb";

            if (openFileDialog.ShowDialog() != true)
                return;

            openFile ( openFileDialog.FileName);
        }

        private bool canOpenFile()
        {
            return ! FConnFile.isFileOpen();
        }

        private void closeFile()
        {
            if ( CurrentChampionship != null )
                if ( CurrentChampionship.WebServer != null )
                {
                    CurrentChampionship.WebServer.stop ( );
                    CurrentChampionship.WebServer = null;
                }

            FileIO.FConnFile.CloseFile();
            ( (App)App.Current).CurrentChampionship = new ChampionshipVM();

            controls.Clear();
            mruManager.IsEnabled = true;

            ReportLibrary.closeLibrary ( );

            ChampionshipSolutions.MainWindow.Frame.Navigate ( new DefaultPage ( ) );

        }

        private bool canCloseFile()
        {
            return FConnFile.isFileOpen();
        }

        private string onLoadFile;

        static Mutex mutex = new Mutex(true, "{9A8FC093-2B3F-483C-AFE5-1E1BB8B9DD36}");

        protected override void OnStartup ( StartupEventArgs e )
        {
                // To Do Disabled 2017-06-22 maybe this shouldn't be disabled
            //if ( mutex.WaitOne ( TimeSpan.Zero , true ) )
            if ( true )
            {
                //mutex.ReleaseMutex ( );

                CurrentChampionship = new ChampionshipVM ( );

                base.OnStartup ( e );

                controls = new ObservableCollection<MainPageControl> ( );

                if ( e.Args.Count ( ) > 0 )
                {
                    if ( e.Args[0].ToLower ( ).EndsWith ( ".csdb" ) )
                    {
                        string OpenFilePath = e.Args[0];

                        onLoadFile = OpenFilePath;
                    }
                    else
                    {
                        ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Tried to open invalid file" , ChampionshipSolutions.Diag.MessagePriority.Critical );
                        ChampionshipSolutions.Diag.Diagnostics.LogLine ( e.Args[0].ToString ( ) , ChampionshipSolutions.Diag.MessagePriority.Critical );
                    }

                }
            }

            //else {

            //    if ( e.Args.Count ( ) > 0 )
            //    {
            //        ChampionshipSolutions.Diag.Diagnostics.LogLine ( "Tried to open file on non main application" , ChampionshipSolutions.Diag.MessagePriority.Critical );
            //        foreach ( string s in e.Args )
            //            ChampionshipSolutions.Diag.Diagnostics.LogLine ( s , ChampionshipSolutions.Diag.MessagePriority.Critical );
            //    }

            //    // send our Win32 message to make the currently running instance
            //    // jump on top of all the other windows
            //    NativeMethods.PostMessage(
            //        (IntPtr) NativeMethods.HWND_BROADCAST ,
            //        NativeMethods.WM_SHOWME ,
            //        IntPtr.Zero ,
            //        IntPtr.Zero );
            //    Application.Current.Shutdown( );
            //}

            // here you take control
        }

        internal void PostLoadEvent ( )
        {
            ( (App)Application.Current ).mruManager = new MRUManager (
            ChampionshipSolutions.MainWindow.MRUMenu ,
            "Championship Solutions" ,
            ( (App)App.Current ).myOwnRecentFileGotClicked_handler );


            if ( onLoadFile != null )
            {
                openFile ( onLoadFile );
            }
            else
            {
                ChampionshipSolutions.MainWindow.Frame.Navigate ( new DefaultPage ( ) );
            }
        }

        protected override void OnExit ( ExitEventArgs e )
        {
            if (CurrentChampionship != null)
                if (CurrentChampionship.WebServer != null)
                    CurrentChampionship.WebServer.stop ( );

            //if ( scanner != null )
            //    ((Window)scanner).Close() ;

            base.OnExit ( e );
        }

        internal void myOwnRecentFileGotClicked_handler ( object obj , EventArgs evt )
        {
            string fName = (obj as MenuItem).Header as string;
            if ( !System.IO.File.Exists ( fName ) )
            {
                if ( MessageBox.Show ( string.Format ( "{0} doesn't exist. Remove from recent workspaces?" , fName ) , "File not found" , MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
                    ( (App)Application.Current ).mruManager.RemoveRecentFile ( fName );
                return;
            }

            openFile ( fName );
        }

    }

#if ( ForceSingeInstance )
    // this class just wraps some Win32 stuff that we're going to use to force single instance.
    internal class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport ( "user32" )]
        public static extern bool PostMessage ( IntPtr hwnd , int msg , IntPtr wparam , IntPtr lparam );

        [DllImport ( "user32" )]
        public static extern int RegisterWindowMessage ( string message );
    }
#endif 

    public struct MainPageControl
    {
        public string Content { get; set; }
        public bool isEnabled { get; set; }
        public Page DisplayPage { get; set; }
    }


}
