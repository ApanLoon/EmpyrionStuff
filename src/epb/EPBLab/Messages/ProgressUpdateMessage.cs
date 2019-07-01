using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.Messages
{
    public class ProgressUpdateData
    {
        public string Description;
        public float Progress;

        public ProgressUpdateData(string description, float progress)
        {
            Description = description;
            Progress = progress;
        }
    }

    public class ProgressUpdateMessage : GenericMessage<ProgressUpdateData>
    {
        public ProgressUpdateMessage(string description, float progress) : base(new ProgressUpdateData(description, progress))
        {
            Identifier = string.Empty;
        }

        public string Identifier { get; set; }
    }
}
