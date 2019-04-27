using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using EPBLab.Helpers;

namespace EPBLab.View
{
    public class PositionAimPoint2UpDirectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            {
                return null;
            }
            Point3D position = (Point3D)values[0];
            Point3D aimPoint = (Point3D)values[1];
            Vector3D lookDirection = new Vector3D(aimPoint.X - position.X, aimPoint.Y - position.Y, aimPoint.Z - position.Z);
            Vector3D rightDirection = lookDirection.Cross(new Vector3D(0, 1, 0));
            Vector3D upDirection = rightDirection.Cross(lookDirection);
            upDirection.Normalize();
            return upDirection;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
