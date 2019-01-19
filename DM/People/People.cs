/*
 *  Filename         : Names.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : FConn Ltd 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :
 * 
 */


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Data.Linq;

namespace ChampionshipSolutions.DM
{

	interface IPrintableName
	{
		string PrintName();
	}

	public class Age
	{
		public int Days, Months, Years;

		public Age(int Days, int Months, int Years)
		{
			this.Days = Days;
			this.Months = Months;
			this.Years = Years;
		}

		public Age(DateTime age)
		{
			this.Years = age.Year;
			this.Months = age.Month;
			this.Days = age.Day;
		}

		public string PrintAge()
		{
			return string.Format("{0:00}.{1:00}", this.Years, this.Months); 
		}
	}

	public enum Gender
	{
		NotStated, 
		Male, 
		Female
	}

	public partial class Person : IPrintableName , IContacts
	{

		#region Properties

		//public int ID { get { return _ID; } set { _ID = value; } }

		public string Discriminator { get { return _Discriminator; } set { _Discriminator = value; } }

		public string FirstName { get { return _FirstName; } set { _FirstName = value; } }

		public string MiddleName { get { return _MiddleName; } set { _MiddleName = value; } }

		public string LastName { get { return _LastName; } set { _LastName = value; } }

		public string PreferredName
		{
			get
			{
				if  (! string.IsNullOrWhiteSpace(_PreferredName))
					return _PreferredName;

				return Fullname;
			}
			set
			{
				if ( value == Fullname )
					_PreferredName = null;
				else
					_PreferredName = value;
			}
		}
		
		public string Title { get { return _Title; } set { _Title = value; } }

		public string Suffix { get { return _Suffix; } set { _Suffix = value; } }

		public Gender Gender
		{
			get {return (Gender)_Gender;}
			set { _Gender = (int)value; }
		}

		public DateTime? DateOfBirth { get { return _DateOfBirth; } set { _DateOfBirth = value; } }

		#endregion

		#region Constructors

		///// <summary>
		///// Copy constructor
		///// </summary>
		///// <param name="name">The name to be copied for the new object</param>
		public Person (Person copyPerson) 
			: this ( copyPerson.FirstName, 
			copyPerson.LastName, 
			copyPerson.MiddleName, 
			copyPerson.Title, 
			copyPerson.Suffix,copyPerson.DateOfBirth )
		{}


		/// <summary>
		/// Explicit Constructor
		/// </summary>
		/// <param name="FirstName">First Name</param>
		/// <param name="LastName">Last Name</param>
		/// <param name="MiddleName">Optional Middle Name</param>
		/// <param name="Title">Optional Middle Name</param>
		/// <param name="Suffux">Optional Middle Name</param>
		/// <param name="DateOfBirth">Optional Date of Birth</param>
		public Person(string FirstName, string LastName, string MiddleName = "", string Title = "", string Suffux = "", DateTime? DateOfBirth = null) :this()
		{
			this.FirstName = FirstName;
			this.MiddleName = MiddleName;
			this.LastName = LastName;
			this.Title = Title;
			this.Suffix = Suffux;

			this.DateOfBirth = DateOfBirth;
		}

		public Person() { DState = null; }

		#endregion

		#region Name

		public string Fullname { get
			{
				string printName = "";

				if ( !String.IsNullOrEmpty ( Title ) )
				{
					printName += Title + " ";
				}

				printName += FirstName + " ";

				if ( !String.IsNullOrEmpty ( MiddleName ) )
				{
					printName += MiddleName + " ";
				}

				printName += LastName + " ";

				if ( !String.IsNullOrEmpty ( Suffix ) )
				{
					printName += Suffix;
				}

				return printName.Trim ( );
			}
		}

		public string PrintName()
		{
			if ( _PreferredName == null )
			{
				return Fullname;
			}
			else
				return PreferredName;
		}

		public string PrintInitials()
		{
			return string.Format("{0}{1}", StringHelper.FirstLetterToUpper(FirstName).Remove(1), StringHelper.FirstLetterToUpper(LastName).Remove(1));
		}

		public void setFullName(string FullName)
		{
			if (FullName == null)
			{
				return;
			}

			string[] nameArray = FullName.Split(' ');

			switch (nameArray.Count())
			{
				case 0:
					throw new ArgumentException();
				case 1:
					FirstName = FullName;
					MiddleName = string.Empty;
					LastName = string.Empty;
					break;
				case 2:
					FirstName = nameArray[0];
					MiddleName = string.Empty;
					LastName = nameArray[1];
					break;
				case 3:
					FirstName = nameArray[0];
					MiddleName = nameArray[1];
					LastName = nameArray[2];
					break;
				case 4:
					FirstName = nameArray.First();
					LastName = nameArray.Last();
					for (int i = 1; i < nameArray.Count(); i++)
					{
						MiddleName += nameArray[i];
					}
					break;
				default:
					break;
			}
		}

		#endregion

		#region Date of birth

		public string PrintDoBUniversal()
		{
			if ( DateOfBirth.HasValue )
				return string.Format( "{0:yyyy}-{0:MM}-{0:dd}" , DateOfBirth );
			return "";
		}

		public string PrintDoBShort()
		{
			//return string.Format("{0:dd}/{0:MM}/{0:yyyy}", DateOfBirth);
			if ( DateOfBirth.HasValue)
				return DateOfBirth.Value.ToShortDateString( ) ;
			return "";
		}
	
		public string PrintDoBLong()
		{
			// return string.Format( "{0:dd} {0:MMMM} {0:yyyy}" ,  );
			if ( DateOfBirth.HasValue )
				return DateOfBirth.Value.ToLongDateString( );
			return "";
		}

		#endregion

		#region Age

		/// <summary>
		/// Check to see if a person's Date of Birth is between two dates
		/// </summary>
		/// <param name="low">Start of range</param>
		/// <param name="high">End of range</param>
		/// <returns>True if date of birth is within rage</returns>
		public bool isDateWithin(DateTime low, DateTime high)
		{
			if (DateOfBirth == null)
			{
				return false;
			}

			return (DateOfBirth > low && DateOfBirth < high);
		}

		public Age getAge()
		{
			return getAge(DateTime.Today);
		}

		public Age getAge(DateTime onDate)
		{
			if (DateOfBirth == null)
				return null;

			if (onDate == null)
				onDate = DateTime.Today;

			int day, month, year;

			DateHelper.DateDifference(onDate, DateOfBirth, out day, out month, out year);

			return new Age(day, month, year);
		}

		public string printAge(DateTime onDate)
		{
			return getAge(onDate).PrintAge();
		}

		public string printAge()
		{
			return printAge(DateTime.Today);
		}

		#endregion

		#region Contact Details

		//private ContactList _contacts
		//{
		//    get { return new ContactList(_Contacts.ToList()); }
		//}

		public virtual List<AContactDetail> Contacts 
		{ 
			get { return _Contacts.ToList(); } 
		}

		public List<AContactDetail> getAllContacts ( )
		{
			return Contacts;
		}

		public void removeContact ( AContactDetail obj )
		{
			DState.IO.Delete<AContactDetail> ( obj );
			__Contacts.refresh ( );
			//_Contacts.Remove ( obj );
			
			// Will delete the contact detail record
			//obj.Person = null;
		}


		//public AddressContactDetail getPrimaryAddress() { return _contacts.getPrimaryAddress(); }

		//public MobileContactDetail getPrimaryMobile()  { return _contacts.getPrimaryMobile(); }

		//public PhoneContactDetail getPrimaryPhone() { return _contacts.getPrimaryPhone(); }

		public void AddContact(AContactDetail newContact)
		{
			newContact.Person = this;
			if ( newContact.ContactName == null ) newContact.ContactName = this.Fullname;

			DState.IO.Add<AContactDetail> ( newContact );
			__Contacts.refresh ( );
			//_Contacts.Add(newContact);
		}

		// 2016-01-19 Bodge to add a new phone number via python web script.
		[Obsolete]
		public void addNewPhoneNumber(string Number)
		{
			AContactDetail aCD = new PhoneContactDetail()
			{
				phoneNumber = Number,
				ContactName = this.Fullname,
			};

			AddContact(aCD);
			//aCD.makePrimary();
		}

		#endregion

		#region Notes

		public ConfidentialNote[] Notes { get { return _Notes.ToArray ( ); } }

		public ConfidentialNote AddNote ( ConfidentialNote Note )
		{
			// checks for duplicate key, if any exists they are deleted.
			if ( ! string.IsNullOrWhiteSpace( Note.Key ) ) // 2017-03-26 don't delete check for key integrity if the key is blank.
				foreach ( ConfidentialNote note in Notes.Where( c => c.Key == Note.Key ) )
					DState.IO.Delete<ConfidentialNote> ( note );

			Note._Athlete_ID = this.ID;

			DState.IO.Add<ConfidentialNote>( Note );


			__Notes.refresh ( );

			return Note;
		}

		public void RemoveNote ( ConfidentialNote Note )
		{
			DState.IO.Delete<ConfidentialNote> ( Note );
			__Notes.refresh ( );
			//_Notes.Remove ( Note );
			//Note.Person = null; // Marks note for deletion
		}

		public List<PublicNote> GetPublicNotes ( )
		{
			return ( from note in Notes where note.GetType ( ) == typeof ( PublicNote ) orderby note.EnteredDate select (PublicNote)note ).ToList ( );
		}

		public List<ConfidentialNote> GetConfidentialNotes ( )
		{
			return ( from note in Notes where note.GetType ( ) == typeof ( ConfidentialNote ) orderby note.EnteredDate select (ConfidentialNote)note ).ToList ( );
		}

		public List<DeclaredAvailibilityInformation> GetAvailibilityNotes ( )
		{
			return ( from note in Notes where note.GetType ( ) == typeof ( DeclaredAvailibilityInformation ) orderby note.EnteredDate select (DeclaredAvailibilityInformation)note ).ToList ( );
		}

		public List<PreviousResult> GetPreviousResultsNotes ( )
		{
			return ( from note in Notes where note.GetType ( ) == typeof ( PreviousResult ) || note.GetType ( ) == typeof ( PowerOfTenResult ) orderby note.EnteredDate select (PreviousResult)note ).ToList ( );
		}

		public PublicNote CreatePublicNote ( )
		{
			return (PublicNote)AddNote ( new PublicNote ( this , DateTime.Now ) );
		}

		public ConfidentialNote CreateConfidentialNote ( )
		{
			return AddNote ( new ConfidentialNote ( this , DateTime.Now ) );
		}

		public PreviousResult CreatePreviousResult ( string Championship , string Event , string Rank , string ResultValue , DateTime EventDate )
		{
			return (PreviousResult)AddNote ( new PreviousResult ( this , DateTime.Now , Championship , Event , Rank , ResultValue , EventDate ) );
		}

		#endregion

		public virtual bool CanBeDeleted
		{
			get
			{
				// A person can always be deleted but an Athlete might have restrictions
				return true;
			}
		}

		public override string ToString()
		{
			return PrintName();
		}

		#region ISearchableField

		static string[] SearchableFields = new string[] { "Name", "FirstName", "LastName", "DoB", "Age", "ID", "Gender" };

		public virtual bool SearchAllFields(string Criteria)
		//public static virtual bool SearchAllFields(Person person, string Criteria)
		{
			foreach (string f in SearchableFields)
				if (SearchField(Criteria, f))
					return true;
			return false;
		}

		//public static virtual bool SearchField(Person person, string criteria, string Field)
		public virtual bool SearchField(string criteria, string Field)
		{
			if (HasField(Field))
			{
				switch (Field)
				{
					case "Name":
						if (PrintName().Contains(criteria))
							return true;
						break;
					case "FirstName":
						if (FirstName.Contains(criteria))
							return true;
						break;
					case "LastName":
						if (LastName.Contains(criteria))
							return true;
						break;
					case "DoB":
						throw new NotImplementedException();
					//break;
					case "Age":
						throw new NotImplementedException();
					//break;
					case "ID":
						throw new NotImplementedException();
					//break;
					case "Gender":
						throw new NotImplementedException();
					//break;
					default:
						break;
				} // end switch
			} // end if
		 
			return false;
		}

		public virtual bool HasField(string Field)
		{
			return SearchableFields.Contains(Field);
		}

		#endregion

		public override bool Equals ( object obj )
		{
			try
			{
				return ( (Person)obj ).ID == this.ID;
			}
			catch 
			{
				return base.Equals ( obj );
			}
		}

		public override int GetHashCode ( )
		{
			return base.GetHashCode ( );
		}

		public static bool operator == ( Person x , Person y )
		{
			if ( ( (object)x ) == null && ( (object)y ) == null ) return true;
			if ( ( (object)x ) == null ) return false;
			if ( ( (object)y ) == null ) return false;

			return x.ID == y.ID;
		}

		public static bool operator != ( Person x , Person y )
		{
			return !( x == y );
		}



	}

}
