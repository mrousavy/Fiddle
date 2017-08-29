using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Fiddle.Compilers;

namespace Fiddle.UI.Converter {
    public class FriendlyToLanguageConverter : IValueConverter {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Language language = (Language?) value ?? Language.CSharp;
            return language.GetDescription();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Enum.GetValues(typeof(Language)).Cast<Language>()
                .First(e => e.GetDescription() == value?.ToString());
        }
    }
}