using System.Security.Cryptography.X509Certificates;

namespace ECFLib.Attributes
{
    public abstract class EcfAttribute
    {
        public string Key { get; set; }
        public string AttributeType { get; set; }
        public string Display { get; set; }
        public string Formatter { get; set; }
        public string Data { get; set; }

        public virtual string ValueString
        {
            get => "";
            set
            {
                var s = value;
            }
        }
    }
}
