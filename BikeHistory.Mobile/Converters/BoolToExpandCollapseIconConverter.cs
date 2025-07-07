using System.Globalization;

namespace BikeHistory.Mobile.Converters
{
    public class BoolToExpandCollapseIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // true면 접기 아이콘 (위쪽 화살표), false면 펼치기 아이콘 (아래쪽 화살표)
                return boolValue ? "&#xf077;" : "&#xf078;"; // chevron-up : chevron-down
            }
            return "&#xf078;"; // 기본값은 아래쪽 화살표
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}