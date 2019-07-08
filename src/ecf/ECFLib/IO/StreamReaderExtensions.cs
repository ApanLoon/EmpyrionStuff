using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ECFLib.Attributes;

namespace ECFLib.IO
{
    public static class StreamReaderExtensions
    {
        public static Config ReadECF(this StreamReader reader)
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
                            string type = m.Groups["type"].Value;
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
                                case "Child":
                                    break;
                                case "Entity":
                                    break;
                                case "Item":
                                    config.ItemTypes.Add(reader.ReadItemType(id, name, reference));
                                    break;
                                case "Template":
                                    break;
                                default:
                                    Console.WriteLine($"Unknkown type ({type})");
                                    break;
                            }
                        }

                        continue;
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
                foreach (EcfAttribute attribute in reader.ReadAttributes((key, value) =>
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
                                a = new AttributeStringArray(Regex.Split(value, "\\s*,\\s*"));
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

                }))
                {
                    blockType.Attributes.Add(attribute.Key, attribute);
                }

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

                foreach (EcfAttribute attribute in reader.ReadAttributes((key, value) =>
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
                        case "Armor":
                        case "BlastDamage":
                        case "BlastRadius":
                        case "BulletsPerShot":
                        case "Cold":
                        case "Damage":
                        case "Durability":
                        case "CameraShake":
                        case "FoodDecayTime":
                        case "Heat":
                        case "NoiseStrength":
                        case "NrSlots":
                        case "Oxygen":
                        case "Radiation":
                        case "Speed":
                        case "RangeSpace":
                        case "SpeedSpace":
                        case "StackSize":
                        case "TracerPerBullet":
                        case "UnlockCost":
                        case "UnlockLevel":
                            a = new AttributeInt(int.Parse(value));
                            break;

                        // string[]
                        case "AllowAt":
                        case "TechTreeNames":
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
                        case "PickupToToolbar":
                        case "RadialMenu":
                        case "TerrainMode":
                            a = new AttributeBool(value == "true");
                            break;

                        // string
                        case "AmmoType":
                        case "Category":
                        case "RadialDesc":
                        case "RadialIcon":
                        case "RadialText":
                        case "TechTreeParent":
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
                        case "DegradationProb":
                        case "FallDamageFac":
                        case "FoodFac":
                        case "HomingSpeed":
                        case "JetpackFac":
                        case "JumpFac":
                        case "Mass":
                        case "PowerFac":
                        case "Radius":
                        case "Range":
                        case "RaySpread":
                        case "Recoil":
                        case "ReloadDelay":
                        case "ReturnFactor":
                        case "ROF":
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

                }))
                {
                    itemType.Attributes.Add(attribute.Key, attribute);
                }

                return itemType;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading item", ex);
            }

        }



        public static List<EcfAttribute> ReadAttributes(this StreamReader reader, Func<string, string, EcfAttribute> createAttribute)
        {
            List<EcfAttribute> list = new List<EcfAttribute>();
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

                    //TODO: How to deal with nested attribute lists? Scopes?

                    Match m = Regex.Match(s, "\\s*(?<key>[^:]+):\\s*(?:(?:\"(?<value>.+?)\")|(?<value>[^,]+))(\\s*,\\s*type:\\s*(?<type>[^,]+))?(\\s*,\\s*display:\\s*(?<display>[^,]+))?(\\s*,\\s*formatter:\\s*(?<formatter>[^,]+))?(\\s*,\\s*data:\\s*(?<data>[^,]+))?");
                    if (m.Success)
                    {
                        string key = m.Groups["key"].Value;
                        string value = m.Groups["value"].Value;
                        string type = m.Groups["type"].Success ? m.Groups["type"].Value : null;
                        string display = m.Groups["display"].Success ? m.Groups["display"].Value : null;
                        string formatter = m.Groups["formatter"].Success ? m.Groups["formatter"].Value : null;
                        string data = m.Groups["data"].Success ? m.Groups["data"].Value : null;
                        EcfAttribute a = createAttribute(key, value);
                        if (a != null)
                        {
                            a.Key = key;
                            a.AttributeType = type;
                            a.Display = display;
                            a.Formatter = formatter;
                            a.Data = data;
                            list.Add(a);
                        }
                    }
                }

                return list;
            }
            catch (System.Exception ex)
            {
                throw new Exception("Failed reading attributes", ex);
            }
        }

    }
}
