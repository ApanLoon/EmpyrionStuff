using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ECFLib
{
    public class Config
    {
        public string Path { get; set; }
        public int Version { get; set; }

        public List<Entity> Entities = new List<Entity>();
        public List<Block> Blocks = new List<Block>();

        public IEnumerable<Entity> BlockEntities
        {
            get { return Entities.Where(e => e.Name == "Block"); }
        }

        public Config(string path)
        {
            Path = path;

            using (StreamReader reader = new StreamReader(File.OpenRead(path)))
            {
                try
                {
                    Entity entity = null;

                    while (!reader.EndOfStream)
                    {
                        string s = reader.ReadLine();
                        if (s == null)
                        {
                            break;
                        }

                        s = s.Trim();
                        if (s == String.Empty || s.StartsWith("#"))
                        {
                            continue;
                        }

                        if (s.StartsWith("VERSION:"))
                        {
                            Version = int.Parse(s.Substring(8).Trim());
                        }

                        if (s.StartsWith("{"))
                        {
                            Match m = Regex.Match(s, "{\\s*(?<name>\\S+)\\s*(?<props>.*)$");
                            if (m.Success)
                            {
                                entity = new Entity(m.Groups["name"].Value);
                                foreach (string p in m.Groups["props"].Value.Split(','))
                                {
                                    Match mp = Regex.Match(p, "\\s*(?<key>[^:]+):\\s*(?<value>.*)\\s*");
                                    if (mp.Success)
                                    {
                                        entity.Properties.Add(mp.Groups["key"].Value, mp.Groups["value"].Value);
                                    }
                                }
                            }
                            continue;
                        }

                        if (s.StartsWith("}") && entity != null)
                        {
                            Entities.Add(entity);

                            if (entity.Name == "Block")
                            {
                                Blocks.Add(new Block(entity));
                            }

                            entity = null;
                            continue;
                        }

                        if (entity != null)
                        {
                            Match m = Regex.Match(s, "\\s*(?<key>[^:]+):\\s*(?<value>.*)\\s*");
                            if (m.Success)
                            {
                                entity.Properties.Add(m.Groups["key"].Value, m.Groups["value"].Value);
                            }
                        }
                    }

                    ConnectBlockReferences();

                }
                catch (System.Exception ex)
                {
                    throw new Exception("Failed reading ECF file", ex);
                }
            }

        }

        private void ConnectBlockReferences()
        {
            foreach (Block block in Blocks)
            {
                if (block.RefName != "")
                {
                    block.Ref = Blocks.FirstOrDefault(b => b.Name == block.RefName);
                }
                if (block.TemplateRootName != "")
                {
                    block.TemplateRoot = Blocks.FirstOrDefault(b => b.Name == block.TemplateRootName);
                }
                if (block.TechTreeParentName != "")
                {
                    block.TechTreeParent = Blocks.FirstOrDefault(b => b.TechTreeParentName == block.TechTreeParentName);
                }
            }
        }
    }

    public class Entity
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

    public class Block
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RefName { get; set; }
        public Block Ref { get; set; }

        private string _category = null;
        public string Category
        {
            get
            {
                if (_category != null)
                {
                    return _category;
                }

                if (Ref != null)
                {
                    return Ref.Category;
                }

                if (TemplateRoot != null && TemplateRoot != this) // TODO: This is probably not correct, but it does make the category matching better.
                {
                    return TemplateRoot.Category;
                }

                return null;
            }
            set => _category = value;
        }

        public int BlastRadius { get; set; }
        public int BlastDamage { get; set; }
        public string TemplateRootName { get; set; }
        public Block TemplateRoot { get; set; }
        public string TechTreeParentName { get; set; }
        public Block TechTreeParent { get; set; }
        public string TechTreeNames { get; set; }


        private BlockProperty<string> Info { get; set; }
        private BlockProperty<int> HitPoints { get; set; }
        private BlockProperty<float> Volume { get; set; }
        private BlockProperty<float> VolumeCapacity { get; set; }
        private BlockProperty<float> Mass { get; set; }
        private BlockProperty<bool> IsOxygenTight { get; set; }
        private BlockProperty<float> ThrusterForce { get; set; }
        private BlockProperty<int> EnergyIn { get; set; }
        private BlockProperty<int> EnergyOut { get; set; }
        private BlockProperty<int> CpuIn { get; set; }
        private BlockProperty<int> Temperature { get; set; }
        public string WeaponItem { get; set; }
        private BlockProperty<int> RotSpeed { get; set; }
        private BlockProperty<int> UnlockCost { get; set; }
        private BlockProperty<int> UnlockLevel { get; set; }

        public Block(Entity entity)
        {
            foreach (string key in entity.Properties.Keys)
            {
                switch (key)
                {
                    case "Id":
                        Id = int.Parse(entity.Properties[key]);
                        break;
                    case "Name":
                        Name = entity.Properties[key];
                        break;
                    case "Ref":
                        RefName = entity.Properties[key];
                        break;
                    case "Category":
                        Category = entity.Properties[key];
                        break;
                    case "TemplateRoot":
                        TemplateRootName = entity.Properties[key];
                        break;
                    case "TechTreeParent":
                        TechTreeParentName = entity.Properties[key];
                        break;
                    case "TechTreeNames":
                        TechTreeNames = entity.Properties[key];
                        break;
                }
            }
        }
    }

    public class BlockProperty<T>
    {
        public T Value { get; set; }
        public bool Display { get; set; }
        public string Formatter { get; set; }
    }
}
