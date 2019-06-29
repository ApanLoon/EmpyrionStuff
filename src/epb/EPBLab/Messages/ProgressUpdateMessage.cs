using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.Messages
{
    public class ProgressUpdateData
    {
        public string Description;
        public int Goal;
        public int Current;

        public ProgressUpdateData(string description, int goal, int current)
        {
            Description = description;
            Goal = goal;
            Current = current;
        }
    }

    public class ProgressUpdateMessage : GenericMessage<ProgressUpdateData>
    {
        public ProgressUpdateMessage(string description, int goal, int current) : base(new ProgressUpdateData(description, goal, current))
        {
            Identifier = string.Empty;
        }

        public string Identifier { get; set; }
    }
}
