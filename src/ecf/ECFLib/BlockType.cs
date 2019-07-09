
using ECFLib.Attributes;
using System.Collections.Generic;

namespace ECFLib
{
    public class BlockType : EcfObject
    {
        //private string _category = null;
        //public string Category
        //{
        //    get
        //    {
        //        if (_category != null)
        //        {
        //            return _category;
        //        }

        //        if (Ref != null)
        //        {
        //            return Ref.Category;
        //        }

        //        if (TemplateRoot != null && TemplateRoot != this) // TODO: This is probably not correct, but it does make the category matching better.
        //        {
        //            return TemplateRoot.Category;
        //        }

        //        return null;
        //    }
        //    set => _category = value;
        //}

        #region AttributeShortcuts
        public bool AboveTerrainCheck
        {
            get => Attributes.ContainsKey("AboveTerrainCheck") && ((AttributeBool)Attributes["AboveTerrainCheck"]).Value;
            set
            {
                if (Attributes.ContainsKey("AboveTerrainCheck"))
                {
                    ((AttributeBool)Attributes["AboveTerrainCheck"]).Value = value;
                }
                else
                {
                    Attributes.Add("AboveTerrainCheck", new AttributeBool(value) {Key = "AboveTerrainCheck" });
                }
            }
        }

        public int BlastDamage
        {
            get => Attributes.ContainsKey("BlastDamage") ? ((AttributeInt)Attributes["BlastDamage"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("BlastDamage"))
                {
                    ((AttributeInt)Attributes["BlastDamage"]).Value = value;
                }
                else
                {
                    Attributes.Add("BlastDamage", new AttributeInt(value) {Key = "BlastDamage" });
                }
            }
        }

        public int BlastRadius
        {
            get => Attributes.ContainsKey("BlastRadius") ? ((AttributeInt)Attributes["BlastRadius"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("BlastRadius"))
                {
                    ((AttributeInt)Attributes["BlastRadius"]).Value = value;
                }
                else
                {
                    Attributes.Add("BlastRadius", new AttributeInt(value) {Key = "BlastRadius" });
                }
            }
        }

        //public int BlockColor
        //{
        //    get => Attributes.ContainsKey("BlockColor") ? ((AttributeColour)Attributes["BlockColor"]).Value : 0;
        //    set
        //    {
        //        if (Attributes.ContainsKey("BlockColor"))
        //        {
        //            ((AttributeColour)Attributes["BlockColor"]).Value = value;
        //        }
        //        else
        //        {
        //            Attributes.Add("BlockColor", new AttributeColour(value) { Key = "BlockColor" });
        //        }
        //    }
        //}

        public string Category
        {
            get => Attributes.ContainsKey("Category") ? ((AttributeString)Attributes["Category"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("Category"))
                {
                    ((AttributeString)Attributes["Category"]).Value = value;
                }
                else
                {
                    Attributes.Add("Category", new AttributeString(value) {Key = "Category" });
                }
            }
        }

        public string[] ChildBlocks
        {
            get => Attributes.ContainsKey("ChildBlocks") ? ((AttributeStringArray)Attributes["ChildBlocks"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("ChildBlocks"))
                {
                    ((AttributeStringArray)Attributes["ChildBlocks"]).Value = value;
                }
                else
                {
                    Attributes.Add("ChildBlocks", new AttributeStringArray(value) {Key = "ChildBlocks" });
                }
            }
        }

        public int CPUIn
        {
            get => Attributes.ContainsKey("CPUIn") ? ((AttributeInt)Attributes["CPUIn"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("CPUIn"))
                {
                    ((AttributeInt)Attributes["CPUIn"]).Value = value;
                }
                else
                {
                    Attributes.Add("CPUIn", new AttributeInt(value){Key = "CPUIn" });
                }
            }
        }

        public int CPUOut
        {
            get => Attributes.ContainsKey("CPUOut") ? ((AttributeInt)Attributes["CPUOut"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("CPUOut"))
                {
                    ((AttributeInt)Attributes["CPUOut"]).Value = value;
                }
                else
                {
                    Attributes.Add("CPUOut", new AttributeInt(value){Key = "CPUOut" });
                }
            }
        }

        public int EnergyIn
        {
            get => Attributes.ContainsKey("EnergyIn") ? ((AttributeInt)Attributes["EnergyIn"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("EnergyIn"))
                {
                    ((AttributeInt)Attributes["EnergyIn"]).Value = value;
                }
                else
                {
                    Attributes.Add("EnergyIn", new AttributeInt(value){Key = "EnergyIn" });
                }
            }
        }

        public int EnergyInIdle
        {
            get => Attributes.ContainsKey("EnergyInIdle") ? ((AttributeInt)Attributes["EnergyInIdle"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("EnergyInIdle"))
                {
                    ((AttributeInt)Attributes["EnergyInIdle"]).Value = value;
                }
                else
                {
                    Attributes.Add("EnergyInIdle", new AttributeInt(value){Key = "EnergyInIdle" });
                }
            }
        }

        public int EnergyOut
        {
            get => Attributes.ContainsKey("EnergyOut") ? ((AttributeInt)Attributes["EnergyOut"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("EnergyOut"))
                {
                    ((AttributeInt)Attributes["EnergyOut"]).Value = value;
                }
                else
                {
                    Attributes.Add("EnergyOut", new AttributeInt(value){Key = "EnergyOut" });
                }
            }
        }

        public string[] FuelAccept
        {
            get => Attributes.ContainsKey("FuelAccept") ? ((AttributeStringArray)Attributes["FuelAccept"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("FuelAccept"))
                {
                    ((AttributeStringArray)Attributes["FuelAccept"]).Value = value;
                }
                else
                {
                    Attributes.Add("FuelAccept", new AttributeStringArray(value){Key = "FuelAccept" });
                }
            }
        }

        public int FuelCapacity
        {
            get => Attributes.ContainsKey("FuelCapacity") ? ((AttributeInt)Attributes["FuelCapacity"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("FuelCapacity"))
                {
                    ((AttributeInt)Attributes["FuelCapacity"]).Value = value;
                }
                else
                {
                    Attributes.Add("FuelCapacity", new AttributeInt(value){Key = "FuelCapacity" });
                }
            }
        }

        public string Group
        {
            get => Attributes.ContainsKey("Group") ? ((AttributeString)Attributes["Group"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("Group"))
                {
                    ((AttributeString)Attributes["Group"]).Value = value;
                }
                else
                {
                    Attributes.Add("Group", new AttributeString(value){Key = "Group" });
                }
            }
        }

        public int HitPoints
        {
            get => Attributes.ContainsKey("HitPoints") ? ((AttributeInt)Attributes["HitPoints"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("HitPoints"))
                {
                    ((AttributeInt)Attributes["HitPoints"]).Value = value;
                }
                else
                {
                    Attributes.Add("HitPoints", new AttributeInt(value){Key = "HitPoints" });
                }
            }
        }

        public float HVEngineDampCoef
        {
            get => Attributes.ContainsKey("HVEngineDampCoef") ? ((AttributeFloat)Attributes["HVEngineDampCoef"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("HVEngineDampCoef"))
                {
                    ((AttributeFloat)Attributes["HVEngineDampCoef"]).Value = value;
                }
                else
                {
                    Attributes.Add("HVEngineDampCoef", new AttributeFloat(value){Key = "HVEngineDampCoef" });
                }
            }
        }

        public float HVEngineDampPow
        {
            get => Attributes.ContainsKey("HVEngineDampPow") ? ((AttributeFloat)Attributes["HVEngineDampPow"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("HVEngineDampPow"))
                {
                    ((AttributeFloat)Attributes["HVEngineDampPow"]).Value = value;
                }
                else
                {
                    Attributes.Add("HVEngineDampPow", new AttributeFloat(value){Key = "HVEngineDampPow" });
                }
            }
        }

        public string Info
        {
            get => Attributes.ContainsKey("Info") ? ((AttributeString)Attributes["Info"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("Info"))
                {
                    ((AttributeString)Attributes["Info"]).Value = value;
                }
                else
                {
                    Attributes.Add("Info", new AttributeString(value){Key = "Info" });
                }
            }
        }

        public bool IsAccessible
        {
            get => Attributes.ContainsKey("IsAccessible") && ((AttributeBool)Attributes["IsAccessible"]).Value;
            set
            {
                if (Attributes.ContainsKey("IsAccessible"))
                {
                    ((AttributeBool)Attributes["IsAccessible"]).Value = value;
                }
                else
                {
                    Attributes.Add("IsAccessible", new AttributeBool(value){Key = "IsAccessible" });
                }
            }
        }

        public bool IsIgnoreLC
        {
            get => Attributes.ContainsKey("IsIgnoreLC") && ((AttributeBool)Attributes["IsIgnoreLC"]).Value;
            set
            {
                if (Attributes.ContainsKey("IsIgnoreLC"))
                {
                    ((AttributeBool)Attributes["IsIgnoreLC"]).Value = value;
                }
                else
                {
                    Attributes.Add("IsIgnoreLC", new AttributeBool(value){Key = "IsIgnoreLC" });
                }
            }
        }

        public bool IsLockable
        {
            get => Attributes.ContainsKey("IsLockable") && ((AttributeBool)Attributes["IsLockable"]).Value;
            set
            {
                if (Attributes.ContainsKey("IsLockable"))
                {
                    ((AttributeBool)Attributes["IsLockable"]).Value = value;
                }
                else
                {
                    Attributes.Add("IsLockable", new AttributeBool(value){Key = "IsLockable" });
                }
            }
        }

        public string IsOxygenTight
        {
            get => Attributes.ContainsKey("IsOxygenTight") ? ((AttributeString)Attributes["IsOxygenTight"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("IsOxygenTight"))
                {
                    ((AttributeString)Attributes["IsOxygenTight"]).Value = value;
                }
                else
                {
                    Attributes.Add("IsOxygenTight", new AttributeString(value){Key = "IsOxygenTight" });
                }
            }
        }

        public float Mass
        {
            get => Attributes.ContainsKey("Mass") ? ((AttributeFloat)Attributes["Mass"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("Mass"))
                {
                    ((AttributeFloat)Attributes["Mass"]).Value = value;
                }
                else
                {
                    Attributes.Add("Mass", new AttributeFloat(value){Key = "Mass" });
                }
            }
        }

        public string Material
        {
            get => Attributes.ContainsKey("Material") ? ((AttributeString)Attributes["Material"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("Material"))
                {
                    ((AttributeString)Attributes["Material"]).Value = value;
                }
                else
                {
                    Attributes.Add("Material", new AttributeString(value){Key = "Material" });
                }
            }
        }

        public int MaxCount
        {
            get => Attributes.ContainsKey("MaxCount") ? ((AttributeInt)Attributes["MaxCount"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("MaxCount"))
                {
                    ((AttributeInt)Attributes["MaxCount"]).Value = value;
                }
                else
                {
                    Attributes.Add("MaxCount", new AttributeInt(value){Key = "MaxCount" });
                }
            }
        }

        public float MaxVolumeCapacity
        {
            get => Attributes.ContainsKey("MaxVolumeCapacity") ? ((AttributeFloat)Attributes["MaxVolumeCapacity"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("MaxVolumeCapacity"))
                {
                    ((AttributeFloat)Attributes["MaxVolumeCapacity"]).Value = value;
                }
                else
                {
                    Attributes.Add("MaxVolumeCapacity", new AttributeFloat(value){Key = "MaxVolumeCapacity" });
                }
            }
        }

        public string[] O2Accept
        {
            get => Attributes.ContainsKey("O2Accept") ? ((AttributeStringArray)Attributes["O2Accept"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("O2Accept"))
                {
                    ((AttributeStringArray)Attributes["O2Accept"]).Value = value;
                }
                else
                {
                    Attributes.Add("O2Accept", new AttributeStringArray(value){Key = "O2Accept" });
                }
            }
        }

        public int O2Capacity
        {
            get => Attributes.ContainsKey("O2Capacity") ? ((AttributeInt)Attributes["O2Capacity"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("O2Capacity"))
                {
                    ((AttributeInt)Attributes["O2Capacity"]).Value = value;
                }
                else
                {
                    Attributes.Add("O2Capacity", new AttributeInt(value){Key = "O2Capacity" });
                }
            }
        }

        public int PanelAngle
        {
            get => Attributes.ContainsKey("PanelAngle") ? ((AttributeInt)Attributes["PanelAngle"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("PanelAngle"))
                {
                    ((AttributeInt)Attributes["PanelAngle"]).Value = value;
                }
                else
                {
                    Attributes.Add("PanelAngle", new AttributeInt(value){Key = "PanelAngle" });
                }
            }
        }

        public float Radiation
        {
            get => Attributes.ContainsKey("Radiation") ? ((AttributeFloat)Attributes["Radiation"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("Radiation"))
                {
                    ((AttributeFloat)Attributes["Radiation"]).Value = value;
                }
                else
                {
                    Attributes.Add("Radiation", new AttributeFloat(value){Key = "Radiation" });
                }
            }
        }

        public float ReturnFactor
        {
            get => Attributes.ContainsKey("ReturnFactor") ? ((AttributeFloat)Attributes["ReturnFactor"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("ReturnFactor"))
                {
                    ((AttributeFloat)Attributes["ReturnFactor"]).Value = value;
                }
                else
                {
                    Attributes.Add("ReturnFactor", new AttributeFloat(value){Key = "ReturnFactor" });
                }
            }
        }

        public int RotSpeed
        {
            get => Attributes.ContainsKey("RotSpeed") ? ((AttributeInt)Attributes["RotSpeed"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("RotSpeed"))
                {
                    ((AttributeInt)Attributes["RotSpeed"]).Value = value;
                }
                else
                {
                    Attributes.Add("RotSpeed", new AttributeInt(value){Key = "RotSpeed" });
                }
            }
        }

        public int ShieldCapacity
        {
            get => Attributes.ContainsKey("ShieldCapacity") ? ((AttributeInt)Attributes["ShieldCapacity"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("ShieldCapacity"))
                {
                    ((AttributeInt)Attributes["ShieldCapacity"]).Value = value;
                }
                else
                {
                    Attributes.Add("ShieldCapacity", new AttributeInt(value){Key = "ShieldCapacity" });
                }
            }
        }

        public int ShieldCooldown
        {
            get => Attributes.ContainsKey("ShieldCooldown") ? ((AttributeInt)Attributes["ShieldCooldown"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("ShieldCooldown"))
                {
                    ((AttributeInt)Attributes["ShieldCooldown"]).Value = value;
                }
                else
                {
                    Attributes.Add("ShieldCooldown", new AttributeInt(value){Key = "ShieldCooldown" });
                }
            }
        }

        public int ShieldPerCrystal
        {
            get => Attributes.ContainsKey("ShieldPerCrystal") ? ((AttributeInt)Attributes["ShieldPerCrystal"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("ShieldPerCrystal"))
                {
                    ((AttributeInt)Attributes["ShieldPerCrystal"]).Value = value;
                }
                else
                {
                    Attributes.Add("ShieldPerCrystal", new AttributeInt(value){Key = "ShieldPerCrystal" });
                }
            }
        }

        public int ShieldRecharge
        {
            get => Attributes.ContainsKey("ShieldRecharge") ? ((AttributeInt)Attributes["ShieldRecharge"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("ShieldRecharge"))
                {
                    ((AttributeInt)Attributes["ShieldRecharge"]).Value = value;
                }
                else
                {
                    Attributes.Add("ShieldRecharge", new AttributeInt(value){Key = "ShieldRecharge" });
                }
            }
        }

        public bool ShowBlockName
        {
            get => Attributes.ContainsKey("ShowBlockName") && ((AttributeBool)Attributes["ShowBlockName"]).Value;
            set
            {
                if (Attributes.ContainsKey("ShowBlockName"))
                {
                    ((AttributeBool)Attributes["ShowBlockName"]).Value = value;
                }
                else
                {
                    Attributes.Add("ShowBlockName", new AttributeBool(value){Key = "ShowBlockName" });
                }
            }
        }

        public float SolarPanelEfficiency
        {
            get => Attributes.ContainsKey("SolarPanelEfficiency") ? ((AttributeFloat)Attributes["SolarPanelEfficiency"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("SolarPanelEfficiency"))
                {
                    ((AttributeFloat)Attributes["SolarPanelEfficiency"]).Value = value;
                }
                else
                {
                    Attributes.Add("SolarPanelEfficiency", new AttributeFloat(value){Key = "SolarPanelEfficiency" });
                }
            }
        }

        public int StackSize
        {
            get => Attributes.ContainsKey("StackSize") ? ((AttributeInt)Attributes["StackSize"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("StackSize"))
                {
                    ((AttributeInt)Attributes["StackSize"]).Value = value;
                }
                else
                {
                    Attributes.Add("StackSize", new AttributeInt(value){Key = "StackSize" });
                }
            }
        }

        public string[] TechTreeNames
        {
            get => Attributes.ContainsKey("TechTreeNames") ? ((AttributeStringArray)Attributes["TechTreeNames"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("TechTreeNames"))
                {
                    ((AttributeStringArray)Attributes["TechTreeNames"]).Value = value;
                }
                else
                {
                    Attributes.Add("TechTreeNames", new AttributeStringArray(value){Key = "TechTreeNames" });
                }
            }
        }

        public string TechTreeParent
        {
            get => Attributes.ContainsKey("TechTreeParent") ? ((AttributeString)Attributes["TechTreeParent"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("TechTreeParent"))
                {
                    ((AttributeString)Attributes["TechTreeParent"]).Value = value;
                }
                else
                {
                    Attributes.Add("TechTreeParent", new AttributeString(value){Key = "TechTreeParent" });
                }
            }
        }

        public int Temperature
        {
            get => Attributes.ContainsKey("Temperature") ? ((AttributeInt)Attributes["Temperature"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("Temperature"))
                {
                    ((AttributeInt)Attributes["Temperature"]).Value = value;
                }
                else
                {
                    Attributes.Add("Temperature", new AttributeInt(value){Key = "Temperature" });
                }
            }
        }

        public int TemperatureGain
        {
            get => Attributes.ContainsKey("TemperatureGain") ? ((AttributeInt)Attributes["TemperatureGain"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("TemperatureGain"))
                {
                    ((AttributeInt)Attributes["TemperatureGain"]).Value = value;
                }
                else
                {
                    Attributes.Add("TemperatureGain", new AttributeInt(value){Key = "TemperatureGain" });
                }
            }
        }

        public string TemplateRoot
        {
            get => Attributes.ContainsKey("TemplateRoot") ? ((AttributeString)Attributes["TemplateRoot"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("TemplateRoot"))
                {
                    ((AttributeString)Attributes["TemplateRoot"]).Value = value;
                }
                else
                {
                    Attributes.Add("TemplateRoot", new AttributeString(value){Key = "TemplateRoot" });
                }
            }
        }

        public int ThrusterForce
        {
            get => Attributes.ContainsKey("ThrusterForce") ? ((AttributeInt)Attributes["ThrusterForce"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("ThrusterForce"))
                {
                    ((AttributeInt)Attributes["ThrusterForce"]).Value = value;
                }
                else
                {
                    Attributes.Add("ThrusterForce", new AttributeInt(value){Key = "ThrusterForce" });
                }
            }
        }

        public int Torque
        {
            get => Attributes.ContainsKey("Torque") ? ((AttributeInt)Attributes["Torque"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("Torque"))
                {
                    ((AttributeInt)Attributes["Torque"]).Value = value;
                }
                else
                {
                    Attributes.Add("Torque", new AttributeInt(value){Key = "Torque" });
                }
            }
        }

        public bool TurretTargetIgnore
        {
            get => Attributes.ContainsKey("TurretTargetIgnore") && ((AttributeBool)Attributes["TurretTargetIgnore"]).Value;
            set
            {
                if (Attributes.ContainsKey("TurretTargetIgnore"))
                {
                    ((AttributeBool)Attributes["TurretTargetIgnore"]).Value = value;
                }
                else
                {
                    Attributes.Add("TurretTargetIgnore", new AttributeBool(value){Key = "TurretTargetIgnore" });
                }
            }
        }

        public int UnlockCost
        {
            get => Attributes.ContainsKey("UnlockCost") ? ((AttributeInt)Attributes["UnlockCost"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("UnlockCost"))
                {
                    ((AttributeInt)Attributes["UnlockCost"]).Value = value;
                }
                else
                {
                    Attributes.Add("UnlockCost", new AttributeInt(value){Key = "UnlockCost" });
                }
            }
        }

        public int UnlockLevel
        {
            get => Attributes.ContainsKey("UnlockLevel") ? ((AttributeInt)Attributes["UnlockLevel"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("UnlockLevel"))
                {
                    ((AttributeInt)Attributes["UnlockLevel"]).Value = value;
                }
                else
                {
                    Attributes.Add("UnlockLevel", new AttributeInt(value){Key = "UnlockLevel" });
                }
            }
        }

        public float Volume
        {
            get => Attributes.ContainsKey("Volume") ? ((AttributeFloat)Attributes["Volume"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("Volume"))
                {
                    ((AttributeFloat)Attributes["Volume"]).Value = value;
                }
                else
                {
                    Attributes.Add("Volume", new AttributeFloat(value){Key = "Volume" });
                }
            }
        }

        public float VolumeCapacity
        {
            get => Attributes.ContainsKey("VolumeCapacity") ? ((AttributeFloat)Attributes["VolumeCapacity"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("VolumeCapacity"))
                {
                    ((AttributeFloat)Attributes["VolumeCapacity"]).Value = value;
                }
                else
                {
                    Attributes.Add("VolumeCapacity", new AttributeFloat(value){Key = "VolumeCapacity" });
                }
            }
        }

        public string WeaponItem
        {
            get => Attributes.ContainsKey("WeaponItem") ? ((AttributeString)Attributes["WeaponItem"]).Value : null;
            set
            {
                if (Attributes.ContainsKey("WeaponItem"))
                {
                    ((AttributeString)Attributes["WeaponItem"]).Value = value;
                }
                else
                {
                    Attributes.Add("WeaponItem", new AttributeString(value){Key = "WeaponItem" });
                }
            }
        }

        public int XpFactor
        {
            get => Attributes.ContainsKey("XpFactor") ? ((AttributeInt)Attributes["XpFactor"]).Value : 0;
            set
            {
                if (Attributes.ContainsKey("XpFactor"))
                {
                    ((AttributeInt)Attributes["XpFactor"]).Value = value;
                }
                else
                {
                    Attributes.Add("XpFactor", new AttributeInt(value){Key = "XpFactor" });
                }
            }
        }

        public float Zoom
        {
            get => Attributes.ContainsKey("Zoom") ? ((AttributeFloat)Attributes["Zoom"]).Value : 0f;
            set
            {
                if (Attributes.ContainsKey("Zoom"))
                {
                    ((AttributeFloat)Attributes["Zoom"]).Value = value;
                }
                else
                {
                    Attributes.Add("Zoom", new AttributeFloat(value){Key = "Zoom" });
                }
            }
        }
        #endregion AttributeShortcuts


        public BlockType (int id, string name, string reference) : base (id, name, reference)
        {
        }
    }
}
