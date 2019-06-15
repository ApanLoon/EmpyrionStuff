using System.Collections.ObjectModel;
using System.Windows;
using EPBLib;
using EPBLib.Logic;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Logic
{
    public class SignalSourceViewModel : LogicNodeViewModel
    {
        protected SignalSource Source;

        public string Name
        {
            get => Source.Name;
            set
            {
                Source.Name = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }
        public static readonly string NamePropertyName = "Name";

        public string Pos => Source.Pos.ToString();


        public SignalSourceViewModel(Blueprint blueprint, SignalSource source, Point initialPosition)
        {
            Source = source;
            X = initialPosition.X;
            Y = initialPosition.Y;

            BlockPos pos = source.Pos;
            Block block = blueprint.Blocks[pos.X, pos.Y, pos.Z];
            NodeType = block != null ? block.BlockType.ToString() : "Source";

            Outputs.Add(new ConnectionPointViewModel() { Name = "0", Type = ConnectionPointViewModel.ConnectorType.OutputLast });
        }
    }
}
