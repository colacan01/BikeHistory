using System.Globalization;

namespace BikeHistory.Mobile.Converters
{
    public class BoolToExpandCollapseIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // true�� ���� ������ (���� ȭ��ǥ), false�� ��ġ�� ������ (�Ʒ��� ȭ��ǥ)
                return boolValue ? "&#xf077;" : "&#xf078;"; // chevron-up : chevron-down
            }
            return "&#xf078;"; // �⺻���� �Ʒ��� ȭ��ǥ
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}