
using System;
using System.ComponentModel;
using System.Windows;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Logic
{
    public class ConnectorViewModel : ViewModelBase
    {
        #region Fields
        protected Point _lineEndPoint;
        protected Point _endEdgePoint;
        protected double _endArrowAngle;
        #endregion

        public LogicNodeViewModel StartVm { get; set; }
        public LogicNodeViewModel EndVm { get; set; }

        public ConnectorViewModel(LogicNodeViewModel startVm, LogicNodeViewModel endVm)
        {
            StartVm = startVm;
            EndVm = endVm;
            StartVm.PropertyChanged += OnPropertyChanged;
            EndVm.PropertyChanged += OnPropertyChanged;
        }

        #region Properties

        /// <summary>
        /// Dummy to make the listbox binding happy.
        /// </summary>
        public double X => 0.0;

        /// <summary>
        /// Dummy to make the listbox binding happy.
        /// </summary>
        public double Y => 0.0;


        public Point EndEdgePoint
        {
            get
            {
                return _endEdgePoint;
            }
            set
            {
                _endEdgePoint = value;
                RaisePropertyChanged("EndEdgePoint");
                RaisePropertyChanged("EndEdgePointX");
                RaisePropertyChanged("EndEdgePointY");
            }
        }
        public double EndEdgePointX
        {
            get { return EndEdgePoint.X; }
        }
        public double EndEdgePointY
        {
            get { return EndEdgePoint.Y; }
        }

        public Point LineEndPoint
        {
            get { return _lineEndPoint; }
            set
            {
                _lineEndPoint = value;
                RaisePropertyChanged("LineEndPoint");
                RaisePropertyChanged("LineEndPointX");
                RaisePropertyChanged("LineEndPointY");
            }
        }
        public double LineEndPointX
        {
            get { return LineEndPoint.X; }
        }
        public double LineEndPointY
        {
            get { return LineEndPoint.Y; }
        }

        public double EndArrowAngle
        {
            get { return _endArrowAngle; }
            set
            {
                _endArrowAngle = value;
                RaisePropertyChanged("EndArrowAngle");
            }
        }

        #endregion Properties

        #region Event handlers
        public void OnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                EndArrowAngle = computeEndArrowAngle();
                EndEdgePoint = computeEndEdgePoint();
                LineEndPoint = computeLineEndPoint();
            }
        }
        #endregion

        #region Methods
        protected Point computeEndEdgePoint()
        {
            Point startCentre = StartVm.Centre;
            Point endCentre = EndVm.Centre;
            Size endSize = EndVm.Size;
            Vector endHalfSize = new Vector(endSize.Width / 2.0, endSize.Height / 2.0);
            Line l = new Line(startCentre, endCentre);

            Nullable<Point> p = null;

            // Check the top line:
            p = l.lineIntersection(new Line(Point.Add(endCentre, new Vector(-endHalfSize.X, -endHalfSize.Y)),
                                              Point.Add(endCentre, new Vector(endHalfSize.X, -endHalfSize.Y))));
            if (p == null)
            {
                // Check the bottom line:
                p = l.lineIntersection(new Line(Point.Add(endCentre, new Vector(-endHalfSize.X, endHalfSize.Y)),
                                                  Point.Add(endCentre, new Vector(endHalfSize.X, endHalfSize.Y))));
            }

            if (p == null)
            {
                // Check the left line:
                p = l.lineIntersection(new Line(Point.Add(endCentre, new Vector(-endHalfSize.X, -endHalfSize.Y)),
                                                  Point.Add(endCentre, new Vector(-endHalfSize.X, endHalfSize.Y))));
            }

            if (p == null)
            {
                // Check the right line:
                p = l.lineIntersection(new Line(Point.Add(endCentre, new Vector(endHalfSize.X, -endHalfSize.Y)),
                                                  Point.Add(endCentre, new Vector(endHalfSize.X, endHalfSize.Y))));
            }

            if (p == null)
            {
                return endCentre;
            }
            else
            {
                return p.Value;
            }
        }
        protected Point computeLineEndPoint()
        {
            Line l = new Line(StartVm.Centre, EndEdgePoint);
            return l.pointByDistanceFromStart(l.Length - 8.0); ///TODO: 8.0 is half the width of the arrow head, this should be defined elsewhere
        }
        protected double computeEndArrowAngle()
        {
            double dX = EndVm.Centre.X - StartVm.Centre.X;
            double dY = EndVm.Centre.Y - StartVm.Centre.Y;
            double a = dX != 0 ? Math.Atan(dY / dX) : 0;
            if (dX < 0)
            {
                a += Math.PI;
            }
            return a * (180.0 / Math.PI);
        }
        #endregion
    }
}
