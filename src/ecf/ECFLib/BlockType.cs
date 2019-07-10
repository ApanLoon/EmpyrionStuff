
using ECFLib.Attributes;
using System.Collections.Generic;

namespace ECFLib
{
    public class BlockType : EcfObject
    {
        public TemplateType    TemplateRoot;
        public List<BlockType> ChildBlocks = new List<BlockType>();
        public List<ItemType>  FuelAccept  = new List<ItemType>();
        public List<ItemType>  O2Accept    = new List<ItemType>();
        public ItemType        WeaponItem;

        #region AttributeShortcuts
        public bool AboveTerrainCheck
        {
            get => GetAttribute<AttributeBool>("AboveTerrainCheck")?.Value ?? false;
            set => ((AttributeBool)Attributes["AboveTerrainCheck"]).Value = value;
        }

        public int BlastDamage
        {
            get => GetAttribute<AttributeInt>("BlastDamage")?.Value ?? 0;
            set => ((AttributeInt)Attributes["BlastDamage"]).Value = value;
        }

        public int BlastRadius
        {
            get => GetAttribute<AttributeInt>("BlastRadius")?.Value ?? 0;
            set => ((AttributeInt)Attributes["BlastRadius"]).Value = value;
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
            // The Category getter for the old C# generation code also had this after checking the Ref:
            //
            //        if (TemplateRoot != null && TemplateRoot != this) // TODO: This is probably not correct, but it does make the category matching better.
            //        {
            //            return TemplateRoot.Category;
            //        }
            //
            get => GetAttribute<AttributeString>("Category")?.Value;
            set => ((AttributeString)Attributes["Category"]).Value = value;
        }

        public string[] ChildBlockNames
        {
            get => GetAttribute<AttributeStringArray>("ChildBlocks")?.Value;
            set => ((AttributeStringArray)Attributes["ChildBlocks"]).Value = value;
        }

        public int CPUIn
        {
            get => GetAttribute<AttributeInt>("CPUIn")?.Value ?? 0;
            set => ((AttributeInt)Attributes["CPUIn"]).Value = value;
        }

        public int CPUOut
        {
            get => GetAttribute<AttributeInt>("CPUOut")?.Value ?? 0;
            set => ((AttributeInt)Attributes["CPUOut"]).Value = value;
        }

        public int EnergyIn
        {
            get => GetAttribute<AttributeInt>("EnergyIn")?.Value ?? 0;
            set => ((AttributeInt)Attributes["EnergyIn"]).Value = value;
        }

        public int EnergyInIdle
        {
            get => GetAttribute<AttributeInt>("EnergyInIdle")?.Value ?? 0;
            set => ((AttributeInt)Attributes["EnergyInIdle"]).Value = value;
        }

        public int EnergyOut
        {
            get => GetAttribute<AttributeInt>("EnergyOut")?.Value ?? 0;
            set => ((AttributeInt)Attributes["EnergyOut"]).Value = value;
        }

        public string[] FuelAcceptNames
        {
            get => GetAttribute<AttributeStringArray>("FuelAccept")?.Value;
            set => ((AttributeStringArray)Attributes["FuelAccept"]).Value = value;
        }

        public int FuelCapacity
        {
            get => GetAttribute<AttributeInt>("FuelCapacity")?.Value ?? 0;
            set => ((AttributeInt)Attributes["FuelCapacity"]).Value = value;
        }

        public string Group
        {
            get => GetAttribute<AttributeString>("Group")?.Value;
            set => ((AttributeString)Attributes["Group"]).Value = value;
        }

        public int HitPoints
        {
            get => GetAttribute<AttributeInt>("HitPoints")?.Value ?? 0;
            set => ((AttributeInt)Attributes["HitPoints"]).Value = value;
        }

        public float HVEngineDampCoef
        {
            get => GetAttribute<AttributeFloat>("HVEngineDampCoef")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["HVEngineDampCoef"]).Value = value;
        }

        public float HVEngineDampPow
        {
            get => GetAttribute<AttributeFloat>("HVEngineDampPow")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["HVEngineDampPow"]).Value = value;
        }

        public string Info
        {
            get => GetAttribute<AttributeString>("Info")?.Value;
            set => ((AttributeString)Attributes["Info"]).Value = value;
        }

        public bool IsAccessible
        {
            get => GetAttribute<AttributeBool>("IsAccessible")?.Value ?? false;
            set => ((AttributeBool)Attributes["IsAccessible"]).Value = value;
        }

        public bool IsIgnoreLC
        {
            get => GetAttribute<AttributeBool>("IsIgnoreLC")?.Value ?? false;
            set => ((AttributeBool)Attributes["IsIgnoreLC"]).Value = value;
        }

        public bool IsLockable
        {
            get => GetAttribute<AttributeBool>("IsLockable")?.Value ?? false;
            set => ((AttributeBool)Attributes["IsLockable"]).Value = value;
        }

        public string IsOxygenTight
        {
            get => GetAttribute<AttributeString>("IsOxygenTight")?.Value;
            set => ((AttributeString)Attributes["IsOxygenTight"]).Value = value;
        }

        public float Mass
        {
            get => GetAttribute<AttributeFloat>("Mass")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["Mass"]).Value = value;
        }

        public string Material
        {
            get => GetAttribute<AttributeString>("Material")?.Value;
            set => ((AttributeString)Attributes["Material"]).Value = value;
        }

        public int MaxCount
        {
            get => GetAttribute<AttributeInt>("MaxCount")?.Value ?? 0;
            set => ((AttributeInt)Attributes["MaxCount"]).Value = value;
        }

        public float MaxVolumeCapacity
        {
            get => GetAttribute<AttributeFloat>("MaxVolumeCapacity")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["MaxVolumeCapacity"]).Value = value;
        }

        public string[] O2AcceptNames
        {
            get => GetAttribute<AttributeStringArray>("O2Accept")?.Value;
            set => ((AttributeStringArray)Attributes["O2Accept"]).Value = value;
        }

        public int O2Capacity
        {
            get => GetAttribute<AttributeInt>("O2Capacity")?.Value ?? 0;
            set => ((AttributeInt)Attributes["O2Capacity"]).Value = value;
        }

        public int PanelAngle
        {
            get => GetAttribute<AttributeInt>("PanelAngle")?.Value ?? 0;
            set => ((AttributeInt)Attributes["PanelAngle"]).Value = value;
        }

        public float Radiation
        {
            get => GetAttribute<AttributeFloat>("Radiation")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["Radiation"]).Value = value;
        }

        public float ReturnFactor
        {
            get => GetAttribute<AttributeFloat>("ReturnFactor")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["ReturnFactor"]).Value = value;
        }

        public int RotSpeed
        {
            get => GetAttribute<AttributeInt>("RotSpeed")?.Value ?? 0;
            set => ((AttributeInt)Attributes["RotSpeed"]).Value = value;
        }

        public int ShieldCapacity
        {
            get => GetAttribute<AttributeInt>("ShieldCapacity")?.Value ?? 0;
            set => ((AttributeInt)Attributes["ShieldCapacity"]).Value = value;
        }

        public int ShieldCooldown
        {
            get => GetAttribute<AttributeInt>("ShieldCooldown")?.Value ?? 0;
            set => ((AttributeInt)Attributes["ShieldCooldown"]).Value = value;
        }

        public int ShieldPerCrystal
        {
            get => GetAttribute<AttributeInt>("ShieldPerCrystal")?.Value ?? 0;
            set => ((AttributeInt)Attributes["ShieldPerCrystal"]).Value = value;
        }

        public int ShieldRecharge
        {
            get => GetAttribute<AttributeInt>("ShieldRecharge")?.Value ?? 0;
            set => ((AttributeInt)Attributes["ShieldRecharge"]).Value = value;
        }

        public bool ShowBlockName
        {
            get => GetAttribute<AttributeBool>("ShowBlockName")?.Value ?? false;
            set => ((AttributeBool)Attributes["ShowBlockName"]).Value = value;
        }

        public float SolarPanelEfficiency
        {
            get => GetAttribute<AttributeFloat>("SolarPanelEfficiency")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["SolarPanelEfficiency"]).Value = value;
        }

        public int StackSize
        {
            get => GetAttribute<AttributeInt>("StackSize")?.Value ?? 0;
            set => ((AttributeInt)Attributes["StackSize"]).Value = value;
        }

        public string[] TechTreeNames
        {
            get => GetAttribute<AttributeStringArray>("TechTreeNames")?.Value;
            set => ((AttributeStringArray)Attributes["TechTreeNames"]).Value = value;
        }

        public string TechTreeParent
        {
            get => GetAttribute<AttributeString>("TechTreeParent")?.Value;
            set => ((AttributeString)Attributes["TechTreeParent"]).Value = value;
        }

        public int Temperature
        {
            get => GetAttribute<AttributeInt>("Temperature")?.Value ?? 0;
            set => ((AttributeInt)Attributes["Temperature"]).Value = value;
        }

        public int TemperatureGain
        {
            get => GetAttribute<AttributeInt>("TemperatureGain")?.Value ?? 0;
            set => ((AttributeInt)Attributes["TemperatureGain"]).Value = value;
        }

        public string TemplateRootName
        {
            get => GetAttribute<AttributeString>("TemplateRoot")?.Value;
            set => ((AttributeString)Attributes["TemplateRoot"]).Value = value;
        }

        public int ThrusterForce
        {
            get => GetAttribute<AttributeInt>("ThrusterForce")?.Value ?? 0;
            set => ((AttributeInt)Attributes["ThrusterForce"]).Value = value;
        }

        public int Torque
        {
            get => GetAttribute<AttributeInt>("Torque")?.Value ?? 0;
            set => ((AttributeInt)Attributes["Torque"]).Value = value;
        }

        public bool TurretTargetIgnore
        {
            get => GetAttribute<AttributeBool>("TurretTargetIgnore")?.Value ?? false;
            set => ((AttributeBool)Attributes["TurretTargetIgnore"]).Value = value;
        }

        public int UnlockCost
        {
            get => GetAttribute<AttributeInt>("UnlockCost")?.Value ?? 0;
            set => ((AttributeInt)Attributes["UnlockCost"]).Value = value;
        }

        public int UnlockLevel
        {
            get => GetAttribute<AttributeInt>("UnlockLevel")?.Value ?? 0;
            set => ((AttributeInt)Attributes["UnlockLevel"]).Value = value;
        }

        public float Volume
        {
            get => GetAttribute<AttributeFloat>("Volume")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["Volume"]).Value = value;
        }

        public float VolumeCapacity
        {
            get => GetAttribute<AttributeFloat>("VolumeCapacity")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["VolumeCapacity"]).Value = value;
        }

        public string WeaponItemName
        {
            get => GetAttribute<AttributeString>("WeaponItem")?.Value;
            set => ((AttributeString)Attributes["WeaponItem"]).Value = value;
        }

        public int XpFactor
        {
            get => GetAttribute<AttributeInt>("XpFactor")?.Value ?? 1;
            set => ((AttributeInt)Attributes["XpFactor"]).Value = value;
        }

        public float Zoom
        {
            get => GetAttribute<AttributeFloat>("Zoom")?.Value ?? 0f;
            set => ((AttributeFloat)Attributes["Zoom"]).Value = value;
        }
        #endregion AttributeShortcuts


        public BlockType (int id, string name, string reference) : base (id, name, reference)
        {
        }

    }
}
