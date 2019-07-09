using System.Collections.Generic;
using ECFLib.Attributes;

namespace ECFLib
{
    public class ItemType : EcfObject
    {
        public List<EcfObject> OperationModes = new List<EcfObject>();

        #region AttributeShortcuts
        #endregion AttributeShortcuts

        public ItemType(int id, string name, string reference) : base(id, name, reference)
        {
        }

    }
}
