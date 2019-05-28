using System.Collections.ObjectModel;
using System.Windows;
using EPBLib;
using EPBLib.Logic;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Logic
{
    public class SignalSourceViewModel : LogicNodeViewModel
    {
        protected EpbSignalSource Source;

        public string Name
        {
            get => Source.Name;
            set => Source.Name = value;
        }

        public string Pos => Source.Pos.ToString();


        public SignalSourceViewModel(EpBlueprint blueprint, EpbSignalSource source, Point initialPosition)
        {
            Source = source;
            X = initialPosition.X;
            Y = initialPosition.Y;

            EpbBlockPos pos = source.Pos;
            EpbBlock block = blueprint.Blocks[pos.X, pos.Y, pos.Z];
            NodeType = block != null ? block.BlockType.ToString() : "Source";

            Outputs.Add(new ConnectionPointViewModel() { Name = "0", Type = ConnectionPointViewModel.ConnectorType.OutputLast });
        }
    }
}
