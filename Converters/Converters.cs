using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChampionshipSolutions.Converters
{
    public class SelectedTemplateConverter : IValueConverter
    {
        public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
        {
            return value;
        }

        public object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture )
        {
            if ( value is ViewModel.TemplateVM )
                return value;
            if ( value is string )
                if ( value.ToString() == "(None)" )
                    return new ViewModel.TemplateVM ( ) ;
            return null;
        }
    }

    public class SelectedTeamConverter : IValueConverter
    {
        public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
        {
            return value;
        }

        public object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture )
        {
            if ( value is ViewModel.TeamVM )
                return value;
            if ( value is string )
                if ( value.ToString ( ) == "(None)" )
                    return new ViewModel.TeamVM ( );
            return null;
        }
    }

    public class SelectedSchoolConverter : IValueConverter
    {
        public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
        {
            return value;
        }

        public object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture )
        {
            if ( value is ViewModel.SchoolVM )
                return value;
            if ( value is string )
                if ( value.ToString ( ) == "(None)" )
                    return new ViewModel.SchoolVM ( new DM.School("No School"), null ) ;
            return null;
        }
    }

}
