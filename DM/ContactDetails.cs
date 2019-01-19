/*
 *  Filename         : ContactDetails.cs
 *  Author           : Fraser Connolly
 *  Date started     : 2014-07-05
 *  Copyright        : Fraser Connolly 2014
 *  Summery          :
 *  
 * 
 * Revision Notes   :  ContactList concept has been pulled May 2016.
 * 
 */

using System;
using System.Collections.Generic;
using System.Windows;

namespace ChampionshipSolutions.DM
{
    public interface IContacts
    {
        List<AContactDetail> getAllContacts();

        //AddressContactDetail getPrimaryAddress();

        //MobileContactDetail getPrimaryMobile();

        //PhoneContactDetail getPrimaryPhone();

        void AddContact(AContactDetail newContact);
    }

    //internal class ContactList
    //{
    //    private ICollection<AContactDetail> _contacts;

    //    public ContactList()
    //    {
    //        _contacts = new List<AContactDetail>();
    //    }

    //    public ContactList(ICollection<AContactDetail> contacts)
    //    {
    //        //_contacts = new List<AContactDetail>();

    //        //if (contacts != null)
    //        //    _contacts.AddRange(contacts);
    //        _contacts = contacts;
    //    }

    //    public List<AContactDetail> getAllContacts()
    //    {
    //        return _contacts.ToList();
    //    }

    //    //public AddressContactDetail getPrimaryAddress()
    //    //{
    //    //    foreach (AContactDetail cd in _contacts)
    //    //    {
    //    //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.AddressContactDetail")
    //    //        {
    //    //            if (cd.isPrimary())
    //    //            {
    //    //                return (AddressContactDetail) cd;
    //    //            }
    //    //        }
    //    //    }
    //    //    return null;
    //    //}

    //    //public MobileContactDetail getPrimaryMobile()
    //    //{
    //    //    foreach (AContactDetail cd in _contacts)
    //    //    {
    //    //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.MobileContactDetail")
    //    //        {
    //    //            if (cd.isPrimary())
    //    //            {
    //    //                return (MobileContactDetail)cd;
    //    //            }
    //    //        }
    //    //    }
    //    //    return null;
    //    //}

    //    //public PhoneContactDetail getPrimaryPhone()
    //    //{
    //    //    foreach (AContactDetail cd in _contacts)
    //    //    {
    //    //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.PhoneContactDetail")
    //    //        {
    //    //            if (cd.isPrimary())
    //    //            {
    //    //                return (PhoneContactDetail)cd;
    //    //            }
    //    //        }
    //    //    }
    //    //    return null;
    //    //}

    //    public void addContact(AContactDetail newContact, IContacts Person)
    //    {
    //        _contacts.Add(newContact);
    //        //newContact.addToList(_contacts);
    //        //newContact.Owner = Person;

    //        //if (Person is DM.Person)
    //        //{
    //            //newContact.Person_ID = ((IID)Person).ID;
    //        //}
    //        //else if (Person is DM.School)
    //        //{
    //            //newContact.School_ID = ((IID)Person).ID;
    //        //}
    //    }
    //}


    public abstract partial class AContactDetail
    {
        public AContactDetail()
        {
            DState = null;
        }

        //public int ID { get { return _ID; } set { _ID = value; } }

        //public string Discriminator { get { return _Discriminator; } set { _Discriminator = value; } }

        public Person Person { get { return _Person; } set { _Person = value; } }

        //// to do How the hell do I map this in SQLite
        //public IContacts Owner { get; set; }

        //// to do How the hell do I map this in SQLite
        //protected ICollection<AContactDetail> inList;

        //public void addToList(ICollection<AContactDetail> list)
        //{
        //    inList = list;

        //    if (isOnlyOfType())
        //        makePrimary();
        //}

        //public bool isPrimary() { return _primary; }

        //public abstract void makePrimary();

        //public bool Primary { get { return _Primary; } set { _Primary = value; } }
        //{
        //    get
        //    {
        //        return isPrimary();
        //    }
        //    set
        //    {
        //        _primary = value;
        //    }
        //}

        //protected void dropPrimary() { _primary = false; }

        //protected void setPrimary() { _primary = true; }

        //protected bool isOnlyOfType()
        //{
        //    return (inList.Where(x => x.GetType() == this.GetType()).Count() == 1);
        //}

        public string ContactName { get { return _ContactName; } set { _ContactName = value; } }

        public abstract string printValue {get;}

        //public List<EmailContactDetail> getAllEmaill()
        //{
        //    List<EmailContactDetail> temp = new List<EmailContactDetail>();

        //    foreach (AContactDetail cd in inList)
        //    {
        //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.EmailContactDetail")
        //        {
        //            temp.Add((EmailContactDetail)cd);
        //        }
        //    }
        //    return temp;

        //}

        public Visibility ShowAddress
        {
            get
            {
                if ( this is AddressContactDetail )
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ShowPhoneNumber
        {
            get
            {
                if ( this is PhoneContactDetail )
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ShowEmail
        {
            get
            {
                if ( this is EmailContactDetail )
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }
        
    }

    public partial class AddressContactDetail : AContactDetail
    {
        public string FirstLine { get { return _FirstLine; } set { _FirstLine = value; } }

        public string SecondLine { get { return _SecondLine; } set { _SecondLine = value; } }

        public string ThirdLine { get { return _ThirdLine; } set { _ThirdLine = value; } }

        public string FourthLine { get { return _FourthLine; } set { _FourthLine = value; } }

        public string PostCode { get { return _PostCode; } set { _PostCode = value; } }

        //public override void makePrimary()
        //{
        //    foreach (AContactDetail cd in inList)
        //    {
        //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.AddressContactDetail")
        //        {
        //            ((AddressContactDetail)cd).dropPrimary();
        //        }
        //    }
        //    setPrimary();
        //}

        public AddressContactDetail(string ContactName, string FirstLine, string SecondLine, string ThirdLine, string FourthLine, string PostCode)
        {
            this.ContactName = ContactName;
            this.FirstLine = FirstLine;
            this.SecondLine = SecondLine;
            this.ThirdLine = ThirdLine;
            this.FourthLine = FourthLine;
            this.PostCode = PostCode;
        }

        public AddressContactDetail()
        {
        }

        public override string printValue
        {
            get { return string.Format( "{0},\n{1},\n{2},\n{3},\n{4},\n{5}" , ContactName,FirstLine,SecondLine,ThirdLine,FourthLine,PostCode); }
        }
    }

    public partial class PhoneContactDetail : AContactDetail
    {
        public string phoneNumber { get { return _phoneNumber; } set { if (setPhoneNumber(value) ) _phoneNumber = value; } }
     
        private bool setPhoneNumber(string value)
        {
            if ( value == null )
                return true;

            if ( value.Length < 10 || value.Length > 14 )
                throw new ArgumentOutOfRangeException ( "phoneNumber" , "Phone number is either not long enough or too long." );
            else
                return true;
        }

        //public override void makePrimary()
        //{
        //    foreach (AContactDetail cd in inList)
        //    {
        //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.PhoneContactDetail")
        //        {
        //            ((PhoneContactDetail)cd).dropPrimary();
        //        }
        //    }
        //    setPrimary();
        //}

        public override string printValue
        {
            get { return phoneNumber ?? "" ; }
        }

    }

    public partial class MobileContactDetail : PhoneContactDetail
    {
        //public override void makePrimary()
        //{
        //    foreach (AContactDetail cd in inList)
        //    {
        //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.MobileContactDetail")
        //        {
        //            ((MobileContactDetail)cd).dropPrimary();
        //        }
        //    }
        //    setPrimary();


        //}
    }

    public partial class EmailContactDetail : AContactDetail
    {
        #region Constructors
        public EmailContactDetail()
        {
        }

        public EmailContactDetail(string EmailAddress)
        {
            this.EmailAddress = EmailAddress;
        }
        
        public EmailContactDetail(string EmailAddress,string ContactName)
        {
            this.EmailAddress = EmailAddress;
            this.ContactName = ContactName;
        }
        #endregion

        public string EmailAddress { get { return _EmailAddress; } set { _EmailAddress = value; } }
        //{
        //    get { return _EmailAdress; }
        //    set
        //    {
        //        //if (Validator.ValidateEmail(value))
        //        //    _EmailAdress = value;
        //        //else
        //        //    _EmailAdress = null;
        //        _EmailAdress = value;
        //    }
        //}

        //public override void makePrimary()
        //{
        //    foreach (AContactDetail cd in inList)
        //    {
        //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.EmailContactDetail")
        //        {
        //            ((EmailContactDetail)cd).dropPrimary();
        //        }
        //    }
        //    setPrimary();
        //}

        public override string printValue
        {
            get
            {
                if ( ContactName != null )
                    return string.Format ( "\"{0}\" <{1}>" , ContactName , EmailAddress );
                else
                    return string.Format ( "{0}" , EmailAddress );
            }
        }

        //public string printAllEmails()
        //{
        //    string emailList = string.Empty;

        //    foreach (AContactDetail cd in inList)
        //    {
        //        if (cd.GetType().ToString() == "ChampionshipSolutions.DM.EmailContactDetail")
        //        {
        //            emailList += ((EmailContactDetail)cd).printValue;
        //            emailList += ", ";
        //        }
        //    }

        //    // remove the last comma.
        //    emailList = emailList.Remove(emailList.Length - 2);

        //    return emailList;
        //}
    }
}
