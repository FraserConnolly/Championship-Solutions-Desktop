//
// Adapted from http://www.codeproject.com/Articles/407513/Add-Most-Recently-Used-Files-MRU-List-to-Windows by Fraser Connolly 2016-04-23
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows;

namespace ChampionshipSolutions.Controls
{
    public class MRUManager
    {
        #region Private members
        private string NameOfProgram;
        private string SubKeyName;
        private MenuItem ParentMenuItem;
        private Action<object, RoutedEventArgs> OnRecentFileClick;
        private Action<object, RoutedEventArgs> OnClearRecentFilesClick;

        private void _onClearRecentFiles_Click ( object obj , RoutedEventArgs evt )
        {
            try
            {
                RegistryKey rK = Registry.CurrentUser.OpenSubKey(this.SubKeyName, true);
                if ( rK == null )
                    return;
                string[] values = rK.GetValueNames();
                foreach ( string valueName in values )
                    rK.DeleteValue ( valueName , true );
                rK.Close ( );
                this.ParentMenuItem.Items.Clear ( );
                this.ParentMenuItem.IsEnabled = false;
            }
            catch ( Exception ex )
            {
                Console.WriteLine ( ex.ToString ( ) );
            }
            if ( OnClearRecentFilesClick != null )
                this.OnClearRecentFilesClick ( obj , evt );
        }

        private List<string> _getRegistryKeys()
        {
            RegistryKey rK;

            try
            {
                rK = Registry.CurrentUser.OpenSubKey ( this.SubKeyName , false );
                if ( rK == null )
                {
                    this.ParentMenuItem.IsEnabled = false;
                    return new List<string> ( );
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine ( "Cannot open recent files registry key:\n" + ex.ToString ( ) );
                return new List<string> ( );
            }

            string[] valueNames = rK.GetValueNames ( );
            List<string> files = new List<string>();

            foreach ( string valueName in valueNames )
            {
                string fileName = rK.GetValue ( valueName , null ) as string;

                if ( fileName != null )
                    files.Add ( fileName );
            }

            return files;
        }

        private void _refreshRecentFilesMenu ( )
        {
            MenuItem tSI;

            this.ParentMenuItem.Items.Clear ( );

            foreach ( string valueName in _getRegistryKeys ( ) )
            {
                tSI = new MenuItem ( );
                tSI.Header = valueName;

                tSI.Click += TSI_Click; 

                this.ParentMenuItem.Items.Add ( tSI );
            }

            if ( this.ParentMenuItem.Items.Count == 0 )
            {
                this.ParentMenuItem.IsEnabled = false;
                return;
            }

            this.ParentMenuItem.Items.Add ( new Separator () );
            tSI = new MenuItem ( );
            tSI.Header = "Clear list";

            tSI.Click += _onClearRecentFiles_Click;
            this.ParentMenuItem.Items.Add ( tSI );
            this.ParentMenuItem.IsEnabled = true;
        }

        private void TSI_Click ( object sender , RoutedEventArgs e )
        {
            if ( OnRecentFileClick != null )
                this.OnRecentFileClick ( sender , e );
        }

        #endregion

        #region Public members

        public void AddRecentFile ( string fileNameWithFullPath )
        {
            string s;
            try
            {
                RegistryKey rK = Registry.CurrentUser.CreateSubKey(this.SubKeyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                for ( int i = 0 ; true ; i++ )
                {
                    s = rK.GetValue ( i.ToString ( ) , null ) as string;
                    if ( s == null )
                    {
                        rK.SetValue ( i.ToString ( ) , fileNameWithFullPath );
                        rK.Close ( );
                        break;
                    }
                    else if ( s == fileNameWithFullPath )
                    {
                        rK.Close ( );
                        break;
                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine ( ex.ToString ( ) );
            }
            this._refreshRecentFilesMenu ( );
        }

        public void RemoveRecentFile ( string fileNameWithFullPath )
        {
            try
            {
                RegistryKey rK = Registry.CurrentUser.OpenSubKey(this.SubKeyName, true);
                string[] valuesNames = rK.GetValueNames();
                foreach ( string valueName in valuesNames )
                {
                    if ( ( rK.GetValue ( valueName , null ) as string ) == fileNameWithFullPath )
                    {
                        rK.DeleteValue ( valueName , true );
                        this._refreshRecentFilesMenu ( );
                        break;
                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine ( ex.ToString ( ) );
            }
            this._refreshRecentFilesMenu ( );
        }

        public List<string> RecentFiles ( ) { return _getRegistryKeys ( ); }

        public bool IsEnabled
        {
            get
            {
                return ParentMenuItem.IsEnabled;
            }
            set
            {
                ParentMenuItem.IsEnabled = value;
            }
        }

		#endregion

		/// <exception cref="ArgumentException">If anything is null or nameOfProgram contains a forward slash or is empty.</exception>
		public MRUManager(MenuItem parentMenuItem, string nameOfProgram, Action<object, RoutedEventArgs> onRecentFileClick, Action<object, RoutedEventArgs> onClearRecentFilesClick = null)
		{
			if(parentMenuItem == null || onRecentFileClick == null ||
				nameOfProgram == null || nameOfProgram.Length == 0 || nameOfProgram.Contains("\\"))
				throw new ArgumentException("Bad argument.");

			this.ParentMenuItem = parentMenuItem;
			this.NameOfProgram = nameOfProgram;
			this.OnRecentFileClick = onRecentFileClick;
			this.OnClearRecentFilesClick = onClearRecentFilesClick;
			this.SubKeyName = string.Format("Software\\{0}\\MRU", this.NameOfProgram);

			this._refreshRecentFilesMenu();
		}
	}
}
