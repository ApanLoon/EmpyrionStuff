
using System.Collections.ObjectModel;
using System.Windows;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Logic
{
    public class LogicNodeViewModel : ViewModelBase
    {

        public ObservableCollection<ConnectionPointViewModel> Inputs { get => _Inputs; protected set => Set (ref _Inputs, value); }
        protected ObservableCollection<ConnectionPointViewModel> _Inputs = new ObservableCollection<ConnectionPointViewModel>();
        public ObservableCollection<ConnectionPointViewModel> Outputs { get => _Outputs; protected set => Set(ref _Outputs, value); }
        protected ObservableCollection<ConnectionPointViewModel> _Outputs = new ObservableCollection<ConnectionPointViewModel>();

        public string NodeType { get => _NodeType; set => Set(ref _NodeType, value); }
        protected string _NodeType;

        public double X { get => _X;
            set
            {
                Set(ref _X, value);
                //RaisePropertyChanged("Centre");
                //RaisePropertyChanged("CentreX");
                //RaisePropertyChanged("CentreY");
            }
        }
        protected double _X;

        public double Y
        {
            get => _Y;
            set
            {
                Set(ref _Y, value);
                //RaisePropertyChanged("Centre");
                //RaisePropertyChanged("CentreX");
                //RaisePropertyChanged("CentreY");
            }
        }
        protected double _Y;



        public Point Centre
        {
            get => _Centre;
            set => Set( ref _Centre, value);
        }
        protected Point _Centre;



/*
        public Point Centre
        {
            get { return new Point(X + Size.Width / 2.0, Y + Size.Height / 2.0); }
        }
        public double CentreX
        {
            get { return X + Size.Width / 2.0; }
        }
        public double CentreY
        {
            get { return Y + Size.Height / 2.0; }
        }
*/

        public Size Size
        {
            get => _Size;
            set
            {
                Set( ref _Size, value);
                //RaisePropertyChanged("Size");
                //RaisePropertyChanged("Centre");
                //RaisePropertyChanged("CentreX");
                //RaisePropertyChanged("CentreY");
            }
        }
        protected Size _Size;
    }
}
