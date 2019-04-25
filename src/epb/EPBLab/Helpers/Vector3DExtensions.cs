
using System;
using System.Windows.Media.Media3D;

namespace EPBLab.Helpers
{
    public static class Vector3DExtensions
    {
        public static Vector3D Cartesian2Spherical(this Vector3D v)
        {
            double r = v.Length;
            double t = Math.Atan2(-v.Z, v.X);
            double f = Math.Acos(v.Y / r);
            return new Vector3D(r, t, f);
        }

        public static Vector3D Spherical2Cartesian(this Vector3D v)
        {
            double x = v.X * Math.Cos(v.Y) * Math.Sin(v.Z);
            double z = -v.X * Math.Sin(v.Y) * Math.Sin(v.Z);
            double y = v.X * Math.Cos(v.Z);
            return new Vector3D(x, y, z);
        }
    }
}
