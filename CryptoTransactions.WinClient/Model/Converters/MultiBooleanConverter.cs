using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CryptoTransactions.WinClient.Model.Converters
{
    public class MultiBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in values)
                if ((bool)item)
                    return true;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
