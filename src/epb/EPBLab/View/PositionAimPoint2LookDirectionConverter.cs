using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace EPBLab.View
{
    public class PositionAimPoint2LookDirectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Point3D position = (Point3D)values[0];
            Point3D aimPoint = (Point3D)values[1];
            Vector3D lookDirection = new Vector3D(aimPoint.X - position.X, aimPoint.Y - position.Y, aimPoint.Z - position.Z);
            lookDirection.Normalize();
            return lookDirection;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
