
using System;
using System.Windows;
using EPBLib;
using EPBLib.Logic;

namespace EPBLab.ViewModel.Logic
{
    public class SignalTargetViewModel : LogicNodeViewModel
    {
        protected EpBlueprint Blueprint;
        protected EpbSignalTarget Target;

        public string Name
        {
            get
            {
                int x = Target.Pos.X;
                int y = Target.Pos.Y;
                int z = Target.Pos.Z;
                string s = "";
                bool found = false;
                foreach (EpbDeviceGroup group in Blueprint.DeviceGroups)
                {
                    foreach (EpbDeviceGroupEntry entry in group.Entries)
                    {
                        if (entry.Pos.X == x && entry.Pos.Y == y && entry.Pos.Z == z)
                        {
                            s = entry.Name;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
                return s;
            }
        }
        public string SignalName => Target.SignalName;
        public UInt32 Func => Target.Func;
        public EpbSignalTarget.Behaviour Beh => Target.Beh;
        public bool Inv => Target.Inv;
        public string Pos => Target.Pos.ToString();


        public SignalTargetViewModel(EpBlueprint blueprint, EpbSignalTarget target, Point initialPosition)
        {
            Blueprint = blueprint;
            Target = target;
            X = initialPosition.X;
            Y = initialPosition.Y;

            EpbBlockPos pos = target.Pos;
            EpbBlock block = blueprint.Blocks[pos.X, pos.Y, pos.Z];
            NodeType = block != null ? block.BlockType.ToString() : "Target";

            Inputs.Add(new ConnectionPointViewModel() { Name = "0", Type = ConnectionPointViewModel.ConnectorType.InputLast });
        }
    }
}
