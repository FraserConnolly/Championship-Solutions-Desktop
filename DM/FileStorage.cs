using System;

namespace ChampionshipSolutions.DM
{
    public partial class FileStorage : IIdentity
    {
        //private int? Event_ID { get { return _Event_ID; } set { _Event_ID = value; } }

        public AEvent Event { get { return _Event; } set { _Event = value; } }

        public string Name { get { return _Name; } set { _Name = value; } }

        /// <summary>
        /// Get or Set the short name of this team.
        /// By default this is the first 4 characters without any spaces.
        /// </summary>
        public string ShortName
        {
            get
            {
                if (_ShortName != null)
                    return _ShortName;

                if (Name.Length > 4)
                    return Name.Replace(" ", string.Empty).Substring(0, 4).Trim();
                else
                    return Name.Trim();
            }
            set
            {
                _ShortName = value;
            }
        }

        public DateTime CreatedOn { get { return _CreatedOn; } set { _CreatedOn = value; } }

        public string Extension { get { return _Extension; } set { _Extension = value; } }

        public byte[] FileData { get { return _FileData; } set { _FileData = value; } }

        public FileStorage(string FileName, string Extension, byte[] File) : this ()
        {
            this.Name = FileName;
            this.Extension = Extension;
            FileData = File;
            CreatedOn = System.DateTime.Now;
        }

        public FileStorage() { DState = null; }

    }
}
