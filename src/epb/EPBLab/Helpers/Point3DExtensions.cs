
using System;
using System.Windows.Media.Media3D;

namespace EPBLab.Helpers
{
    public static class Point3DExtensions
    {
        public static Point3D Add(this Point3D p0, Point3D p1)
        {
            return new Point3D(p0.X + p1.X, p0.Y + p1.Y, p0.Z += p1.Z);
        }
        public static Point3D Add(this Point3D p0, Vector3D v)
        {
            return new Point3D(p0.X + v.X, p0.Y + v.Y, p0.Z += v.Z);
        }
        public static Point3D Scale(this Point3D p0, double scale)
        {
            return new Point3D(p0.X * scale, p0.Y * scale, p0.Z * scale);
        }

        public static Vector3D VectorTo(this Point3D p0, Point3D p1)
        {
            return new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
        }
    }
}
