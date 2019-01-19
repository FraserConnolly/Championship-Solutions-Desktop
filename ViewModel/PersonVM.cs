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

namespace ChampionshipSolutions.ViewModel
{
    public class PersonVM : ObservableObject
    {

        #region Construction
        /// <summary>
        /// Constructs the default instance of a Team View Model
        /// </summary>
        public PersonVM ( )
        {
        }

        #endregion

        #region Members

        Person _person;

        #endregion

        #region Properties

        public Person Person
        {
            get { return _person; }
            set
            {
                if ( value == null )
                    // reject - I'm not sure why this is happening ??
                    return;

                if ( Person == null )
                {
                    _person = value;
                }
                else
                {
                    if ( Person != value )
                    {
                        _person = value;
                        SaveToDB ( );
                        RaisePropertyChanged ( "FirstName" );
                        RaisePropertyChanged ( "LastName" );
                        RaisePropertyChanged ( "MiddleName" );
                        RaisePropertyChanged ( "PreferredName" );
                    }
                }
            }
        }

        public string FirstName
        {
            get { return Person.FirstName ; }
            set
            {
                if ( Person.FirstName != value )
                {
                    Person.FirstName = value;
                    RaisePropertyChanged ( "FirstName" );
                    RaisePropertyChanged ( "PreferredName" );
                }
            }
        }

        public string MiddleName
        {
            get { return Person.MiddleName; }
            set
            {
                if ( Person.MiddleName != value )
                {
                    Person.MiddleName = value;
                    RaisePropertyChanged ( "MiddleName" );
                    RaisePropertyChanged ( "PreferredName" );
                }
            }
        }

        public string LastName
        {
            get { return Person.LastName; }
            set
            {
                if ( Person.LastName != value )
                {
                    Person.LastName = value;
                    RaisePropertyChanged ( "LastName" );
                    RaisePropertyChanged ( "PreferredName" );
                }
            }
        }

        public string PreferredName
        {
            get { return Person.PreferredName; }
            set
            {
                if ( Person.PreferredName != value )
                {
                    Person.PreferredName = value;
                    RaisePropertyChanged ( "PreferredName" );
                }
            }
        }

        public string YearGroup
        {
            get
            {
                if ( Person is Athlete Athlete )
                {
                    int y = StudentCompetitor.CalculateYearGroup( Athlete , 
                        ((App) App.Current).CurrentChampionship.Championship );

                    if ( y != 0 )
                        return $"{y:00}";
                }

                return string.Empty;

            }
        }

        public DateTime? DateOfBirth
        {
            get { return Person.DateOfBirth; }
            set
            {
                if ( Person.DateOfBirth != value )
                {
                    Person.DateOfBirth = value;
                    RaisePropertyChanged ( "DateOfBirth" );

                    if ( Person is Athlete )
                        RaisePropertyChanged ( "AvailableEvents" );
                }
            }
        }

        public Gender Gender
        {
            get { return Person.Gender; }
            set
            {
                if ( Person.Gender != value )
                {
                    Person.Gender = value;
                    RaisePropertyChanged ( "Gender" );

                    if (Person is Athlete )
                        RaisePropertyChanged ( "AvailableEvents" );
                }
            }
        }

        public List<AContactDetail> Contacts
        {
            get
            {
                if ( Person == null ) return new List<AContactDetail>();

                return Person.Contacts;
            }
        }

        public Visibility ShowAthlete
        {
            get
            {
                if ( Person is Athlete ) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
        }

        public List<ConfidentialNote> Notes
        {
            get
            {
                if ( Person == null ) return new List<ConfidentialNote> ( );

                List<ConfidentialNote> temp = new List<ConfidentialNote> ( );

                temp.AddRange ( Person.GetConfidentialNotes ( ) );
                temp.AddRange ( Person.GetPublicNotes ( ) );
                temp.AddRange ( Person.GetAvailibilityNotes ( ) );
                temp.AddRange ( Person.GetPreviousResultsNotes( ) );

                temp = temp.OrderBy ( o => o.EnteredDate ).ToList();

                return temp;
            }
        }


        #endregion

        #region Commands

        private bool canModify()
        {
            if ( IsEntryForm( ) ) return true;

            if ( (( App )App.Current).CurrentChampionship == null ) return false;

            return ( (App)App.Current ).CurrentChampionship.canModify ( );
        }

        public ICommand Save { get { return new RelayCommand ( SaveToDB , CanSaveToDB ); } }

        protected void SaveToDB ( )
        {
            if ( !GetFileDetails ( ).isOpen ) return;

            //!*!
            GetFileDetails ( ).IO.Update<Person> ( Person );

            //SaveChanges ( );
            clearHasChanges ( );
        }

        protected bool CanSaveToDB ( )
        {
            if ( !GetFileDetails ( ).isOpen ) return false;

            // Does the record need updating?
            if ( hasChanges ) return true;

            return false;
        }

        #region Notes

        public ICommand AddConfidentialNote { get { return new RelayCommand ( addConfidentialNote , canModify ); } }
        public ICommand AddPublicNote { get { return new RelayCommand ( addPublicNote , canModify ); } }
        public ICommand AddAvailibilityNote { get { return new RelayCommand ( addAvailibilityNote , canModify ); } }
        public ICommand AddPreviousResult { get { return new RelayCommand ( addPreviousResult , canModify ); } }

        public ICommand EditNote { get { return new RelayCommand ( editNote , canModify ); } }
        public ICommand DeleteNote { get { return new RelayCommand ( deleteNote , canModify ); } }

        private void editNote ( object obj )
        {
            if ( obj is ConfidentialNote )
            {
                DM.Dialogs.NoteDialogue cd = new DM.Dialogs.NoteDialogue( )
                {
                    DataContext = obj
                };
                cd.ShowDialog ( );

                ((IID)obj).Save ( );

                //SaveToDB ( );
                RaisePropertyChanged ( "Notes" );
            }
        }

        private void deleteNote ( object obj )
        {
            if ( canModify ( ) )
            {
                if ( obj is ConfidentialNote )
                {
                    Person.RemoveNote ( (ConfidentialNote)obj );
                    //SaveToDB ( );
                    RaisePropertyChanged ( "Notes" );
                }
            }
        }

        private void addConfidentialNote ( )
        {
            addNote ( new ConfidentialNote ( Person, DateTime.Now ) );
        }

        private void addPublicNote ( )
        {
            addNote ( new PublicNote ( Person , DateTime.Now ) );
        }

        private void addAvailibilityNote ( )
        {
            addNote ( new DeclaredAvailibilityInformation ( Person , DateTime.Now ) );
        }

        private void addPreviousResult ( )
        {
            addNote ( new PreviousResult ( Person , DateTime.Now ) );
        }

        private void addNote ( ConfidentialNote note )
        {

            DM.Dialogs.NoteDialogue d = new DM.Dialogs.NoteDialogue( )
            {
                DataContext = note
            };
            d.ShowDialog ( );

            Person.AddNote ( note );

            //SaveToDB ( );
            RaisePropertyChanged ( "Notes" );
        }

        #endregion

        #region Contacts


        public ICommand AddPhoneNumber { get { return new RelayCommand ( addPhoneNumber , canModify ); } }
        public ICommand AddEmailAddress { get { return new RelayCommand ( addEmailAddress , canModify ); } }
        public ICommand AddAddress { get { return new RelayCommand ( addAddress , canModify ); } }

        private void addPhoneNumber()
        {
            addContact ( new PhoneContactDetail ( ) );
        }

        private void addEmailAddress ( )
        {
            addContact ( new EmailContactDetail ( ) );
        }

        private void addAddress ( )
        {
            addContact ( new AddressContactDetail ( ) );
        }

        private void addContact(AContactDetail contact)
        {

            contact.ContactName = this.PreferredName;

            DM.Dialogs.ContactDialogue cd = new DM.Dialogs.ContactDialogue( )
            {
                DataContext = contact
            };
            cd.ShowDialog ( );

            Person.AddContact ( contact );

            SaveToDB ( );
            RaisePropertyChanged ( "Contacts" );
        }

        public ICommand EditContact { get { return new RelayCommand ( editContact , canModify ); } }
        public ICommand DeleteContact { get { return new RelayCommand ( deleteContact , canModify ); } }

        private void editContact ( object obj )
        {
            if ( obj is AContactDetail )
            {
                DM.Dialogs.ContactDialogue cd = new DM.Dialogs.ContactDialogue( )
                {
                    DataContext = obj
                };
                cd.ShowDialog ( );

                SaveToDB ( );
                RaisePropertyChanged ( "Contacts" );
            }
        }

        private void deleteContact ( object obj )
        {
            if ( canModify ( ) )
            {
                if ( obj is AContactDetail )
                {

                    Person.removeContact ( ( AContactDetail )obj );
                    //SaveToDB ( );
                    RaisePropertyChanged ( "Contacts" );
                }
            }
        }


        #endregion

        #endregion

        public override bool Equals ( object obj )
        {
            // If parameter is null return false.
            if ( obj == null )
                return false;

            // If parameter cannot be cast to Point return false.
            PersonVM v = obj as PersonVM;
            if ( (System.Object)v == null )
                return false;

            // Return true if the fields match:
            return ( Person.ID == v.Person.ID );
        }

        public override int GetHashCode ( )
        {
            return base.GetHashCode ( );
        }

        public override string ToString ( )
        {
            return this.PreferredName;
        }

        internal bool CanDelete ( )
        {
            if ( Person == null ) return false ;

            return Person.CanBeDeleted;
        }
    }
}
