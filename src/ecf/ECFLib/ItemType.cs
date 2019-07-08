using System.Collections.Generic;
using ECFLib.Attributes;

namespace ECFLib
{
    public class ItemType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RefName { get; set; }
        public BlockType Ref { get; set; }

        public Dictionary<string, EcfAttribute> Attributes = new Dictionary<string, EcfAttribute>();

        #region AttributeShortcuts
        #endregion AttributeShortcuts

        public ItemType(int id, string name, string reference)
        {
            Id = id;
            Name = name;
            RefName = reference;
        }

    }
}
