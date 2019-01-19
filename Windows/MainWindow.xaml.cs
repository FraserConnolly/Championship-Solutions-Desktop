using ChampionshipSolutions.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Xceed.Wpf.Toolkit;
using static ChampionshipSolutions.FileIO.FConnFile;

namespace ChampionshipSolutions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Frame Frame;
        public static MenuItem MRUMenu;
        public static BusyIndicator BusyIndicator;
        private static ListBox Controls;

        public static void Lock ()
        {
            Controls.IsEnabled = false;
            Controls.Visibility = Visibility.Collapsed;
        }

        public static void Unlock ( )
        {
            Controls.IsEnabled = true;
            Controls.Visibility = Visibility.Visible;
        }

        private void checkSpellingDictoinaries ( )
        {
            try
            {
                var dictionariesKeyName = "SOFTWARE\\Microsoft\\Spelling\\Dictionaries";
                var dictionariesKey = Registry.CurrentUser.OpenSubKey(dictionariesKeyName , true);

                if ( dictionariesKey != null )
                {
                    var dictionaries = (string[])dictionariesKey.GetValue("_GLOBAL_");

                    var validDictionaries = new List<string>();

                    foreach ( var dictionary in dictionaries )
                    {
                        if ( File.Exists ( dictionary ) )
                        {
                            validDictionaries.Add ( dictionary );
                        }
                    }

                    dictionariesKey.SetValue ( "_GLOBAL_" , validDictionaries.ToArray ( ) );
                }

                dictionariesKey.Close ( );
            }
            catch (Exception ex)
            {
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( "An error occurred whilst trying to check the spelling dictionaries." , ChampionshipSolutions.Diag.MessagePriority.Error  );
                ChampionshipSolutions.Diag.Diagnostics.LogLine ( ex.Message , ChampionshipSolutions.Diag.MessagePriority.Error );
            }
        }

        protected override void OnSourceInitialized ( EventArgs e )
        {
            base.OnSourceInitialized ( e );
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook ( WndProc );
        }

        private IntPtr WndProc ( IntPtr hwnd , int msg , IntPtr wParam , IntPtr lParam , ref bool handled )
        {
            if ( msg == NativeMethods.WM_SHOWME )
            {
                ShowMe ( );
            }

            return IntPtr.Zero;
        }

        private void ShowMe ( )
        {
            if ( WindowState == System.Windows.WindowState.Minimized ) // FormWindowState.Minimized )
            {
                WindowState = System.Windows.WindowState.Normal; // FormWindowState.Normal;
            }
            // get our current "TopMost" value (ours will always be false though)
            bool top = this.Topmost; // TopMost;
            // make our form jump to the top of everything
            Topmost = true;
            // set it back to whatever it was
            Topmost = top;
        }

        public MainWindow()
        {
            checkSpellingDictoinaries ( );

            InitializeComponent();

            this.MainPageControlsList.ItemsSource = ((App)App.Current).controls;
            Frame = MainFrame;
            MRUMenu = mruMenu;
            BusyIndicator = busyIndicator;
            Controls = MainPageControlsList;
        }

        private void MainPageControlsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPageControlsList.SelectedItem == null)
                this.MainFrame.Content = null;
            else
                this.MainFrame.Navigate(((MainPageControl)MainPageControlsList.SelectedItem).DisplayPage);
        }

        private void Window_Loaded ( object sender , RoutedEventArgs e )
        {
            ( ( App ) Application.Current ).PostLoadEvent ( );
        }

        private void Help_Click ( object sender , RoutedEventArgs e )
        {
            System.Diagnostics.Process.Start ( "http://championship.solutions/DesktopHelp" );
        }

        private void About_Click ( object sender , RoutedEventArgs e )
        {
            new AboutBox ( ).ShowDialog ( );
        }

    }

}
