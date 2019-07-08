using System;
using System.Collections.Generic;

namespace ECFLib
{
    internal class Entity
    {
        public string Name { get; set; }
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        public Entity(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            string s = "";

            s += Name + Environment.NewLine;
            s += "{" + Environment.NewLine;
            foreach (string key in Properties.Keys)
            {
                s += $"    {key}: {Properties[key]}" + Environment.NewLine;
            }
            s += "}" + Environment.NewLine;
            return s;
        }
    }

}
