
using System.Collections.Generic;
using ECFLib.Attributes;

namespace ECFLib
{
    public class EcfObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RefName { get; set; }
        public BlockType Ref { get; set; }

        public Dictionary<string, EcfAttribute> Attributes = new Dictionary<string, EcfAttribute>();

        public EcfObject(int id, string name, string reference)
        {
            Id = id;
            Name = name;
            RefName = reference;
        }

    }
}
