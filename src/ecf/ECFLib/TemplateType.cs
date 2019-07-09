using System.Collections.Generic;

namespace ECFLib
{
    public class TemplateType : EcfObject
    {
        public Dictionary<string, int> Inputs = new Dictionary<string, int>();

        #region AttributeShortcuts
        #endregion AttributeShortcuts

        public TemplateType(int id, string name, string reference) : base(id, name, reference)
        {
        }
    }
}
