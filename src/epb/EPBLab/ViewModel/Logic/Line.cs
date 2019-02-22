using System;
using System.Windows;

namespace EPBLab.ViewModel.Logic
{
    public class Line
    {
        #region Fields
        public Point p0;
        public Point p1;
        #endregion

        #region Constructor
        public Line(Point p0, Point p1)
        {
            this.p0 = p0;
            this.p1 = p1;
        }
        #endregion

        #region Properties
        public double Length
        {
            get { return Point.Subtract(p1, p0).Length; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calculates the intersection point between this line and the given
        /// line. TGhis code was taken from
        /// http://dotnetbyexample.blogspot.nl/2013/09/utility-classes-to-check-if-lines-andor.html
        /// I should probably replace it with code that I understand.
        /// </summary>
        /// <param name="l">Line to check for intersection with.</param>
        /// <returns>The Point or null if the lines do not intersect.</returns>
        public Point? lineIntersection(Line l)
        {
            double l1DY = p1.Y - p0.Y;
            double l1DX = p0.X - p1.X;
            double c1 = p1.X * p0.Y - p0.X * p1.Y;

            double r3 = l1DY * l.p0.X + l1DX * l.p0.Y + c1;
            double r4 = l1DY * l.p1.X + l1DX * l.p1.Y + c1;

            /* Check signs of r3 and r4.  If both point 3 and point 4 lie on
             * same side of line 1, the line segments do not intersect.
             */

            if (r3 != 0 && r4 != 0 && Math.Sign(r3) == Math.Sign(r4))
            {
                return null; // DONT_INTERSECT
            }

            double l2DY = l.p1.Y - l.p0.Y;
            double l2DX = l.p0.X - l.p1.X;
            double c2 = l.p1.X * l.p0.Y - l.p0.X * l.p1.Y;

            double r1 = l2DY * p0.X + l2DX * p0.Y + c2;
            double r2 = l2DY * p1.X + l2DX * p1.Y + c2;

            /* Check signs of r1 and r2.  If both point 1 and point 2 lie
             * on same side of second line segment, the line segments do
             * not intersect.
             */
            if (r1 != 0 && r2 != 0 && Math.Sign(r1) == Math.Sign(r2))
            {
                return (null); // DONT_INTERSECT
            }

            /* Line segments intersect: compute intersection point. 
             */

            double denom = l1DY * l2DX - l2DY * l1DX;
            if (denom == 0)
            {
                return null; //( COLLINEAR );
            }
            double offset = denom < 0 ? -denom / 2 : denom / 2;

            /* The denom/2 is to get rounding instead of truncating.  It
             * is added or subtracted to the numerator, depending upon the
             * sign of the numerator.
             */

            double num = l1DX * c2 - l2DX * c1;
            double x = (num < 0 ? num - offset : num + offset) / denom;

            num = l2DY * c1 - l1DY * c2;
            double y = (num < 0 ? num - offset : num + offset) / denom;
            return new Point(x, y);
        }


        public Point pointByDistanceFromStart(double distance)
        {
            Vector v = Point.Subtract(p1, p0);
            v.Normalize();
            v = Vector.Multiply(distance, v);
            return Point.Add(p0, v);
        }
        #endregion
    }
}
