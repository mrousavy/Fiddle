using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Fiddle.Compilers;

namespace Fiddle.UI.Converter {
    public class LanguageToFriendlyConverter : IValueConverter {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Language[] values = (Language[])value;
            IEnumerable<string> descriptions = values?.Select(v => v.GetDescription());
            return descriptions;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Enum.GetValues(typeof(Language)).Cast<Language>();
        }
    }
}
