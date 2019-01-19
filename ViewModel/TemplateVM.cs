using ChampionshipSolutions.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using static ChampionshipSolutions.FileIO.FConnFile;
using System.Windows.Input;
using System.Windows;
using ChampionshipSolutions.ControlRoom;
using ChampionshipSolutions.Reporting;

namespace ChampionshipSolutions.ViewModel
{
    public class TemplateVM : ObservableObject
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a Template  View Model
        /// </summary>
        public TemplateVM ( )
        {
        }

        public TemplateVM ( Template Template , ChampionshipVM Championship)
        {
            _template = Template;
            _championship = Championship;
        }



        #endregion

        #region Members

        Template _template;
        ChampionshipVM _championship;
        
        #endregion

        private void saveTemplate()
        {
            GetFileDetails ( ).IO.Update<Template> ( Template );
            Championship.updateTemplates ( );
        }

        #region Properties

        public Template Template
        {
            get { return _template; }
            private set
            {
                if ( value == null )
                    // reject - I'm not sure why this is happening when Championships is being updated??
                    return;

                if ( Template != value )
                {
                    //Championship.SaveToDB ( );
                    _template = value;
                }
            }
        }

        public ChampionshipVM Championship
        {
            get
            {
                return _championship;
            }
        }

        public string Name
        {
            get
            {
                if ( Template == null ) return "(None)";
                return Template.TemplateName;
            }
            set
            {
                if ( Template == null ) return;
                if ( Template.TemplateName != value )
                {
                    Template.TemplateName  = value;
                    saveTemplate ( );
                    RaisePropertyChanged ( "Name" );
                }
            }
        }

        #endregion

        #region Commands

        //public ICommand Save { get { return new RelayCommand ( SaveToDB , CanSaveToDB ); } }

        //void SaveToDB ( )
        //{
        //    if ( !GetFileDetails ( ).isOpen ) return;

        //    SaveChanges ( );
        //    clearHasChanges ( );
        //}

        //bool CanSaveToDB ( )
        //{
        //    if ( !GetFileDetails ( ).isOpen ) return false;

        //    // Does the record need updating?
        //    if ( hasChanges ) return true;

        //    return false;
        //}


        public ICommand Edit { get { return new RelayCommand ( editTemplate , canDelete ); } }

        private void editTemplate()
        {
            ReportEditor re = new ReportEditor();
            re.LoadReportFromApplication( AReport.LoadTemplate ( this.Template.Instructions , this.Template.TemplateFile ));
            re.ShowDialog ( );

            //this.Template.Instructions = re.report.rawXML;
            //ReportLibrary.addTemplatesToDB ( re.report );

            if ( re.DialogResult == true )
            {
                this.Template.Instructions = re.report.rawXML;
                this.Template.TemplateFile = re.report.GetTemplate();
                Name = re.report.Name;
            }

            saveTemplate ( );

        }

        public ICommand OpenTemplate { get { return new RelayCommand ( openTemplate , canOpenTemplate ); } }

        private bool canOpenTemplate()
        {
            return ( Template.TemplateFile != null );
        }

        private void openTemplate()
        {
            if ( Template.TemplateFile == null ) return;

            //WinFormPDFViewer viewer = new WinFormPDFViewer();
            //viewer.LoadPDF ( this.Template.TemplateFile );

            PDFViewer.OpenOnSTAThread( Template.TemplateFile );
        }

        public ICommand Delete { get { return new RelayCommand ( deleteFromDatabase , canDelete ); } }

        private bool canDelete ( ) { return Championship.canModify ( ); }

        private void deleteFromDatabase ( )
        {
            if ( MessageBox.Show ( "Are you sure you want to delete " + Name + "? This cannot be undone." , "Are you sure" ,
                MessageBoxButton.YesNo , MessageBoxImage.Exclamation , MessageBoxResult.No ) == MessageBoxResult.Yes )
            {
                //CSDB context = FileIO.FConnFile.getContext();

                //if ( context == null )
                    //throw new Exception ( "File error when deleting team" );

                //Template temp = Template;

                GetFileDetails().IO.Delete<Template> ( Template );

                //!!context.Templates.DeleteOnSubmit ( temp );

                //SaveChanges ( );

                Championship.updateTemplates ( );
            }
        }

        #endregion

        public override bool Equals ( object obj )
        {
            // If parameter is null return false.
            if ( obj == null )
                return false;

            // If parameter cannot be cast to Point return false.
            TemplateVM v = obj as TemplateVM;
            if ( (System.Object)v == null )
                return false;

            // Return true if the fields match:
            return ( Template.ID == v.Template.ID && Championship.Championship.ID == v.Championship.Championship.ID );
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public override string ToString ( )
        {
            return this.Name;
        }

    }
}
