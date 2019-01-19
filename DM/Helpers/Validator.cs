using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Text.RegularExpressions;

namespace ChampionshipSolutions.DM.Helpers
{
    static partial class Validator
    {
        public static bool ValidateEmail(string email)
        {
            if (email == null)
                return false;

            throw new NotImplementedException("This function is not implemented for dll Class Library, convert back to Portable Library - FC");

            //Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            //Match match = regex.Match(email);
            
            //if (match.Success)
            //    return true;
            //else
            //    return false;
        }

    }
}
