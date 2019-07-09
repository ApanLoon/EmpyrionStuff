using System;
using System.IO;
using System.Text.RegularExpressions;
using ECFLib.Attributes;

namespace ECFLib.IO
{
    public static class StreamReaderExtensions
    {
        public static Config ReadEcf(this StreamReader reader)
        {
            try
            {
                Config config = new Config();
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
                        config.Version = int.Parse(s.Substring(8).Trim());
                    }

                    if (s.StartsWith("{"))
                    {
                        Match m = Regex.Match(s, "{\\s*(?<type>\\S+)\\s*(?<props>.*)$");
                        if (m.Success)
                        {
                            string type      = m.Groups["type"].Value;

                            int id = -1;
                            string name = null;
                            string reference = null;

                            foreach (string p in m.Groups["props"].Value.Split(','))
                            {
                                Match mp = Regex.Match(p, "\\s*(?<key>[^:]+):\\s*(?<value>.*)\\s*");
                                if (mp.Success)
                                {
                                    string key = mp.Groups["key"].Value;
                                    string value = mp.Groups["value"].Value;
                                    switch (key)
                                    {
                                        case "Id":
                                            id = int.Parse(value);
                                            break;
                                        case "Name":
                                            name = value;
                                            break;
                                        case "Ref":
                                            reference = value;
                                            break;
                                        default:
                                            Console.WriteLine($"Unknown attribute: {key} = {value}");
                                            break;
                                    }
                                }
                            }

                            switch (type)
                            {
                                case "Block":
                                    config.BlockTypes.Add(reader.ReadBlockType(id, name, reference));
                                    break;
                                case "Item":
                                    config.ItemTypes.Add(reader.ReadItemType(id, name, reference));
                                    break;
                                case "Entity":
                                    config.EntityTypes.Add(reader.ReadEntityType(id, name, reference));
                                    break;
                                case "Template":
                                    config.TemplateTypes.Add(reader.ReadTemplateType(id, name, reference));
                                    break;
                                default:
                                    Console.WriteLine($"Unknkown type ({type})");
                                    break;
                            }
                        }
                    }
                }

                config.ConnectBlockTypeReferences();
                return config;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading ECF file", ex);
            }
        }

        public static BlockType ReadBlockType(this StreamReader reader, int id, string name, string reference)
        {
            try
            {
                BlockType blockType = new BlockType(id, name, reference);
                reader.ReadAttributes(blockType, (key, value) =>
                {
                    EcfAttribute a = null;
                        switch (key)
                        {
                            // bool
                            case "AboveTerrainCheck":
                            case "IsAccessible":
                            case "IsIgnoreLC":
                            case "IsLockable":
                            case "ShowBlockName":
                            case "TurretTargetIgnore":
                                a = new AttributeBool(value == "true");
                                break;

                            // int
                            case "BlastDamage":
                            case "BlastRadius":
                            case "CPUIn":
                            case "CPUOut":
                            case "EnergyIn":
                            case "EnergyInIdle":
                            case "EnergyOut":
                            case "FuelCapacity":
                            case "HitPoints":
                            case "MaxCount":
                            case "O2Capacity":
                            case "PanelAngle":
                            case "RotSpeed":
                            case "ShieldCapacity":
                            case "ShieldCooldown":
                            case "ShieldPerCrystal":
                            case "ShieldRecharge":
                            case "StackSize":
                            case "Temperature":
                            case "TemperatureGain":
                            case "ThrusterForce":
                            case "Torque":
                            case "UnlockCost":
                            case "UnlockLevel":
                            case "XpFactor":
                                a = new AttributeInt(int.Parse(value));
                                break;

                            // colour
                            case "BlockColor":
                                Match cm = Regex.Match(value, "(?<red>\\d+)\\s*,\\s*(?<green>\\d+)\\s*,\\s*(?<blue>\\d+)");
                                if (cm.Success)
                                {
                                    byte r = byte.Parse(cm.Groups["red"].Value);
                                    byte g = byte.Parse(cm.Groups["green"].Value);
                                    byte b = byte.Parse(cm.Groups["blue"].Value);
                                    a = new AttributeColour(r, g, b);
                                }
                                else
                                {
                                    throw new Exception("Unable to parse BlockColor attribute.");
                                }
                                break;

                            // string
                            case "Category":
                            case "Group":
                            case "Info":
                            case "IsOxygenTight":
                            case "Material":
                            case "TechTreeParent":
                            case "TemplateRoot":
                            case "WeaponItem":
                                a = new AttributeString(value);
                                break;

                            // string[]
                            case "ChildBlocks":
                            case "FuelAccept":
                            case "O2Accept":
                            case "TechTreeNames":
                                a = new AttributeStringArray(value != null ? Regex.Split(value, "\\s*,\\s*") : null);
                                break;

                            // float
                            case "HVEngineDampCoef":
                            case "HVEngineDampPow":
                            case "Mass":
                            case "MaxVolumeCapacity":
                            case "Radiation":
                            case "ReturnFactor":
                            case "SolarPanelEfficiency":
                            case "Volume":
                            case "VolumeCapacity":
                            case "Zoom":
                                a = new AttributeFloat(float.Parse(value));
                                break;

                            default:
                                Console.WriteLine($"Unknown block attribute: {key}");
                                break;
                        }

                    return a;
                }, null);

                return blockType;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading item", ex);
            }
        }

        public static ItemType ReadItemType(this StreamReader reader, int id, string name, string reference)
        {
            try
            {
                ItemType itemType = new ItemType(id, name, reference);

                reader.ReadAttributes(itemType, (key, value) =>
                {
                    EcfAttribute a = null;
                    switch (key)
                    {
                        // int
                        case "Armor":
                        case "BlastDamage":
                        case "BlastRadius":
                        case "Cold":
                        case "Durability":
                        case "FoodDecayTime":
                        case "Heat":
                        case "NrSlots":
                        case "Oxygen":
                        case "Radiation":
                        case "StackSize":
                        case "UnlockCost":
                        case "UnlockLevel":
                            a = new AttributeInt(int.Parse(value));
                            break;

                        // string[]
                        case "AllowAt":
                        case "TechTreeNames":
                            a = new AttributeStringArray(value != null ? Regex.Split(value, "\\s*,\\s*") : null);
                            break;

                        // bool
                        case "PickupToToolbar":
                        case "RadialMenu":
                            a = new AttributeBool(value.ToLower() == "true");
                            break;

                        // string
                        case "Category":
                        case "TechTreeParent":
                            a = new AttributeString(value);
                            break;

                        // float
                        case "DegradationProb":
                        case "FallDamageFac":
                        case "FoodFac":
                        case "JetpackFac":
                        case "JumpFac":
                        case "Mass":
                        case "PowerFac":
                        case "Range":
                        case "SpeedFac":
                        case "StaminaFac":
                        case "Volume":
                        case "VolumeCapacity":
                            a = new AttributeFloat(float.Parse(value));
                            break;

                        default:
                            Console.WriteLine($"Unknown item attribute: {key}");
                            break;
                    }
                    return a;
                }, (s, o) =>
                {
                    OperationMode child = reader.ReadOperationMode(-1, null, null);
                    ((ItemType)o).OperationModes.Add(child);
                });

                return itemType;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading item", ex);
            }
        }

        public static OperationMode ReadOperationMode(this StreamReader reader, int id, string name, string reference)
        {
            try
            {
                OperationMode operationMode = new OperationMode();
                reader.ReadAttributes(operationMode, (key, value) =>
                {
                    EcfAttribute a = null;
                    switch (key)
                    {
                        // int
                        case "AddFood":
                        case "AddHealth":
                        case "AddOxygen":
                        case "AddStamina":
                        case "AmmoCapacity":
                        case "BlastDamage":
                        case "BlastRadius":
                        case "BulletsPerShot":
                        case "CameraShake":
                        case "Damage":
                        case "NoiseStrength":
                        case "RangeSpace":
                        case "Speed":
                        case "SpeedSpace":
                        case "TracerPerBullet":
                            a = new AttributeInt(int.Parse(value));
                            break;

                        // string[]
                        case "AllowAt":
                            a = new AttributeStringArray(Regex.Split(value, "\\s*,\\s*"));
                            break;

                        // bool
                        case "AllowRemote":
                        case "Automatic":
                        case "Ballistic":
                        case "DecoMode":
                        case "HarvestSupport":
                        case "IgnoreAtmo":
                        case "InfoPopup":
                        case "TerrainMode":
                            a = new AttributeBool(value.ToLower() == "true");
                            break;

                        // string
                        case "AmmoType":
                        case "RadialDesc":
                        case "RadialIcon":
                        case "RadialText":
                        case "Tracer":
                            a = new AttributeString(value);
                            break;

                        // float
                        case "BlastDamageMultiplier_1":
                        case "BlastDamageMultiplier_2":
                        case "BlastDamageMultiplier_3":
                        case "BlastDamageMultiplier_4":
                        case "BulletSpread":
                        case "DamageMultiplier_1":
                        case "DamageMultiplier_2":
                        case "DamageMultiplier_3":
                        case "DamageMultiplier_4":
                        case "DamageMultiplier_5":
                        case "DamageMultiplier_6":
                        case "DamageMultiplier_7":
                        case "DamageMultiplier_8":
                        case "DamageMultiplier_9":
                        case "HomingSpeed":
                        case "Radius":
                        case "Range":
                        case "RaySpread":
                        case "Recoil":
                        case "ReloadDelay":
                        case "ReturnFactor":
                        case "ROF":
                            a = new AttributeFloat(float.Parse(value));
                            break;

                        default:
                            Console.WriteLine($"Unknown item scope attribute: {key}");
                            break;
                    }
                    return a;
                }, null);

                return operationMode;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading operation mode for item", ex);
            }
        }

        public static EntityType ReadEntityType(this StreamReader reader, int id, string name, string reference)
        {
            try
            {
                EntityType entityType = new EntityType(id, name, reference);
                reader.ReadAttributes(entityType, (key, value) =>
                {
                    EcfAttribute a = null;
                    switch (key)
                    {
                        // bool
                        case "IsEnemy":
                            a = new AttributeBool(value == "true");
                            break;

                        // int
                        case "MaxHealth":
                            a = new AttributeInt(int.Parse(value));
                            break;

                        default:
                            Console.WriteLine($"Unknown entity attribute: {key}");
                            break;
                    }
                    return a;
                }, null);

                return entityType;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading entity", ex);
            }
        }

        public static TemplateType ReadTemplateType(this StreamReader reader, int id, string name, string reference)
        {
            try
            {
                TemplateType templateType = new TemplateType(id, name, reference);

                reader.ReadAttributes(templateType, (key, value) =>
                {
                    EcfAttribute a = null;
                    switch (key)
                    {
                        // int
                        case "CraftTime":
                        case "OutputCount":
                            a = new AttributeInt(int.Parse(value));
                            break;

                        // string[]
                        case "Target":
                            a = new AttributeStringArray(value != null ? Regex.Split(value, "\\s*,\\s*") : null);
                            break;

                        // bool
                        case "BaseItem":
                            a = new AttributeBool(value.ToLower() == "true");
                            break;

                        // string
                        case "DeconOverride":
                            a = new AttributeString(value);
                            break;

                        default:
                            Console.WriteLine($"Unknown template attribute: {key}");
                            break;
                    }
                    return a;
                }, (s, o) =>
                {
                    EcfObject child = new EcfObject(-1, null, null);
                    reader.ReadAttributes(child, (key, value) =>
                    {
                        int v = int.Parse(value);
                        TemplateType t = (TemplateType) o;
                        if (t.Inputs.ContainsKey(key))
                        {
                            t.Inputs[key] += v;
                        }
                        else
                        {
                            t.Inputs.Add(key, v);
                        }
                        return null;
                    }, null);
                });
                return templateType;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading template", ex);
            }
        }

        public static void ReadAttributes(this StreamReader reader,
                                          EcfObject o,
                                          Func<string, string, EcfAttribute> createAttribute,
                                          Action<string, EcfObject> readChild)
        {
            try
            {
                while (true)
                {
                    string s = reader.ReadLine();
                    if (s == null)
                    {
                        throw new Exception("Unexpected end of file when reading attributes.");
                    }

                    s = s.Trim();
                    if (s == String.Empty || s.StartsWith("#"))
                    {
                        continue;
                    }

                    if (s.StartsWith("}"))
                    {
                        break;
                    }

                    if (s.StartsWith("{") && readChild != null)
                    {
                        readChild(s, o);
                        continue;
                    }

                    Match m = Regex.Match(s, "\\s*(?<key>[^:]+):\\s*(?:(?:\"(?<value>.+?)\")|(?<value>[^,]+))?\\s*(?<props>.*)$");
                    if (m.Success)
                    {
                        string key = m.Groups["key"].Value;
                        string value = m.Groups["value"].Success ? m.Groups["value"].Value : null;

                        string type = null;
                        string display = null;
                        string formatter = null;
                        string data = null;

                        foreach (string p in m.Groups["props"].Value.Split(','))
                        {
                            Match mp = Regex.Match(p, "\\s*(?<pKey>[^:]+):\\s*(?<pValue>.*)\\s*");
                            if (mp.Success)
                            {
                                string pKey = mp.Groups["pKey"].Value;
                                string pValue = mp.Groups["pValue"].Value;
                                switch (pKey)
                                {
                                    case "type":
                                        type = pValue;
                                        break;
                                    case "data":
                                        data = pValue;
                                        break;
                                    case "display":
                                        display = pValue;
                                        break;
                                    case "formatter":
                                        formatter = pValue;
                                        break;
                                    default:
                                        Console.WriteLine($"Unknown attribute property: {key} = {pValue}");
                                        break;
                                }
                            }
                        }

                        EcfAttribute a = createAttribute(key, value);
                        if (a != null)
                        {
                            a.Key = key;
                            a.AttributeType = type;
                            a.Display = display;
                            a.Formatter = formatter;
                            a.Data = data;
                            o.Attributes.Add(a.Key, a);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading attributes", ex);
            }
        }

    }
}
