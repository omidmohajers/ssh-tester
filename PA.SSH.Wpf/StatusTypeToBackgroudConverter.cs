using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PA.SSH.Wpf
{
    public class StatusTypeToBackgroudConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StatusType state = (StatusType)value;
            switch (state)
            {
                case StatusType.Done:
                    return new SolidColorBrush(Colors.CadetBlue);
                case StatusType.PingOK:
                    return new SolidColorBrush(Colors.LightSteelBlue);
                case StatusType.Error:
                case StatusType.Exception:
                case StatusType.PingError:
                    return new SolidColorBrush(Colors.Salmon);
                case StatusType.ReplyButNoAthenticate:
                    return new SolidColorBrush(Colors.Gold);
                case StatusType.Message:
                default:
                    return new SolidColorBrush(Colors.WhiteSmoke);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
