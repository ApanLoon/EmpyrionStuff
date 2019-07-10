
using System;
using System.Collections.Generic;
using ECFLib.Attributes;

namespace ECFLib
{
    public class EcfObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RefName { get; set; }
        internal EcfObject Ref { get; set; }

        public Dictionary<string, EcfAttribute> Attributes = new Dictionary<string, EcfAttribute>();

        public EcfObject(int id, string name, string reference)
        {
            Id = id;
            Name = name;
            RefName = reference;
        }

        protected T GetAttribute<T>(string name) where T : EcfAttribute
        {
            if (Attributes.ContainsKey(name) && Attributes[name] is T)
            {
                return (T)Attributes[name];
            }
            return Ref?.GetAttribute<T>(name);
        }
    }
}
