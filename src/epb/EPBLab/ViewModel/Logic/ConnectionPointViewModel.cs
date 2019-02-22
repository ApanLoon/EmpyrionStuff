
using System;
using System.Windows;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Logic
{
    public class ConnectionPointViewModel : LogicNodeViewModel
    {
        public string Name { get => _Name; set => Set (ref _Name, value); }
        protected string _Name;

        public enum ConnectorType
        {
            Input,
            InputLast,
            Output,
            OutputLast
        }

        public ConnectorType Type { get => _ConnectorType; set => Set (ref _ConnectorType, value); }
        protected ConnectorType _ConnectorType;

    }
}
