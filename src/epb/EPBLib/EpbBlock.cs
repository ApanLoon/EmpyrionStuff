
using System;
using System.Collections.Generic;
using System.Linq;
using EPBLib.BlockData;

namespace EPBLib
{
    public class EpbBlock
    {
        #region Types

        public enum EpbBlockRotation
        {
            // FwdUp : P=Positive, N=Negative
            PzPy, PxPy, NzPy, NxPy, PzPx, PyPx, NzPx, NyPx, NzNy, NxNy, PzNy, PxNy, PzNx, PyNx, NzNx, NyNx, PyNz, PxNz, NyNz, NxNz, NxPz, NyPz, PxPz, PyPz
        }

        public class EpbBlockTypeDefinition
        {
            public EpbBlockType Type;
            public string Name;
            public string[] VariantNames;
        }

        public enum EpbBlockType
        {

            // BuildingBlocks
            Window_crsd1x1ThickInv          =  1000, // Window_v1x1Thick
            Window_sd1x2V2ThickInv          =  1001, // Window_v1x1Thick
            TrussLargeBlocks                =  1075, //
            ScifiSofa                       =  1076, // DecoTemplate
            ScifiStorage                    =  1077, // DecoTemplate
            ScifiTable                      =  1078, // DecoTemplate
            ScifiShower                     =  1079, // DecoTemplate
            ScifiPlant                      =  1080, // IndoorPlant01
            ScifiContainer1                 =  1081, // ContainerMS01
            ScifiContainer2                 =  1082, // ContainerMS01
            ScifiContainerEnergy            =  1083, // DecoTemplate
            ScifiContainerPower             =  1084, // ContainerMS01
            ScifiChair                      =  1085, // DecoTemplate
            ScifiTableV2                    =  1086, // DecoTemplate
            ScifiComputerTable              =  1087, // DecoTemplate
            ScifiMediaCenter                =  1088, // DecoTemplate
            HangarDoor01Small3              =  1089, // HangarDoor01Medium
            HangarDoor01Medium3             =  1090, // HangarDoor01Medium
            CockpitMS03                     =  1091, // CockpitMS01
            StairShapes                     =  1125, //
            StairShapesLong                 =  1126, //
            WindowLargeBlocks               =  1128, //
            WindowArmoredLargeBlocks        =  1129, //
            HoverEngineLarge                =  1130, // HoverEngineSmall
            RepairBayCV                     =  1131, // RepairBayBA
            WingBlocks                      =  1135, //
            Wing6x9a                        =  1139, //
            Wing6x5a                        =  1140, // Wing6x9a
            Wing12x9a                       =  1141, // Wing6x9a
            RailingVertGlass                =  1191, //
            RailingVertGlassInv             =  1192, // RailingVertGlass
            RailingRoundGlass               =  1193, // RailingVertGlass
            RailingRoundGlassInv            =  1194, // RailingVertGlass
            RailingLGlass                   =  1195, // RailingVertGlass
            RailingLGlassInv                =  1196, // RailingVertGlass
            Window_crctw1x1                 =  1197, // Window_v1x1
            Window_creA1x1                  =  1198, // Window_v1x1
            Window_creB1x1                  =  1199, // Window_v1x1
            Window_crl1x1                   =  1200, // Window_v1x1
            Window_crse1x1                  =  1201, // Window_v1x1
            Window_cc1x1                    =  1202, // Window_v1x1
            Window_crctw1x1Thick            =  1203, // Window_v1x1Thick
            Window_creA1x1Thick             =  1204, // Window_v1x1Thick
            Window_creB1x1Thick             =  1205, // Window_v1x1Thick
            Window_crl1x1Thick              =  1206, // Window_v1x1Thick
            Window_crse1x1Thick             =  1207, // Window_v1x1Thick
            Window_cc1x1Thick               =  1208, // Window_v1x1Thick
            Window_creA1x1Inv               =  1209, // Window_v1x1
            Window_crctw1x1Inv              =  1210, // Window_v1x1
            Window_creB1x1Inv               =  1211, // Window_v1x1
            Window_crl1x1Inv                =  1212, // Window_v1x1
            Window_crse1x1Inv               =  1213, // Window_v1x1
            Window_cc1x1Inv                 =  1214, // Window_v1x1
            Window_crctw1x1ThickInv         =  1215, // Window_v1x1Thick
            Window_creA1x1ThickInv          =  1216, // Window_v1x1Thick
            Window_creB1x1ThickInv          =  1217, // Window_v1x1Thick
            Window_crl1x1ThickInv           =  1218, // Window_v1x1Thick
            Window_crse1x1ThickInv          =  1219, // Window_v1x1Thick
            Window_cc1x1ThickInv            =  1220, // Window_v1x1Thick
            RailingSlopeGlassRight          =  1221, // RailingVertGlass
            RailingSlopeGlassRightInv       =  1222, // RailingVertGlass
            RailingSlopeGlassLeft           =  1223, // RailingVertGlass
            RailingSlopeGlassLeftInv        =  1224, // RailingVertGlass
            RailingDiagonalGlass            =  1225, // RailingVertGlass
            RailingDiagonalGlassInv         =  1226, // RailingVertGlass
            LeverSV                         =  1227, //
            LightBarrierSV                  =  1228, //
            MotionSensorSV                  =  1229, //
            ConcreteArmoredBlocks           =  1322, //
            ConcreteArmoredFull             =  1323, //
            ConcreteArmoredThin             =  1324, // ConcreteArmoredFull
            HullLargeDestroyedBlocks        =  1386, //
            HullFullLargeDestroyed          =  1387, //
            HullThinLargeDestroyed          =  1388, // HullFullLargeDestroyed
            HullSmallDestroyedBlocks        =  1389, //
            HullFullSmallDestroyed          =  1390, //
            HullThinSmallDestroyed          =  1391, // HullFullSmallDestroyed
            ConcreteDestroyedBlocks         =  1392, //
            ConcreteFullDestroyed           =  1393, //
            ConcreteThinDestroyed           =  1394, // ConcreteFullDestroyed
            AlienLargeBlocks                =  1395, // AlienBlocks
            AlienFullLarge                  =  1396, // AlienFull
            AlienThinLarge                  =  1397, // AlienThin
            ReceptionTableThin              =  1398, // DecoTemplate
            ReceptionTableCornerThin        =  1399, // DecoTemplate
            PlasticSmallBlocks              =  1478, //
            PlasticFullSmall                =  1479, //
            PlasticThinSmall                =  1480, // PlasticFullSmall
            PlasticLargeBlocks              =  1481, //
            PlasticFullLarge                =  1482, //
            PlasticThinLarge                =  1483, // PlasticFullLarge
            HeavyWindowBlocks               =  1549, //
            HeavyWindowA                    =  1550, //
            HeavyWindowB                    =  1551, // HeavyWindowA
            HeavyWindowC                    =  1552, // HeavyWindowA
            HeavyWindowD                    =  1553, // HeavyWindowA
            HeavyWindowE                    =  1554, // HeavyWindowA
            HeavyWindowF                    =  1555, // HeavyWindowA
            HeavyWindowAInv                 =  1556, // HeavyWindowA
            HeavyWindowBInv                 =  1557, // HeavyWindowA
            HeavyWindowCInv                 =  1558, // HeavyWindowA
            HeavyWindowDInv                 =  1559, // HeavyWindowA
            HeavyWindowEInv                 =  1560, // HeavyWindowA
            HeavyWindowFInv                 =  1561, // HeavyWindowA
            HullCombatSmallBlocks           =  1594, //
            HullCombatFullSmall             =  1595, //
            HullCombatThinSmall             =  1596, // HullCombatFullSmall
            ModularWingTaperedL             =  1605, //
            ModularWingTaperedM             =  1606, // ModularWingTaperedL
            ModularWingTaperedS             =  1607, // ModularWingTaperedL
            ModularWingStraightL            =  1608, // ModularWingTaperedL
            ModularWingStraightM            =  1609, // ModularWingTaperedL
            ModularWingStraightS            =  1610, // ModularWingTaperedL
            ModularWingDeltaL               =  1611, // ModularWingTaperedL
            ModularWingDeltaM               =  1612, // ModularWingTaperedL
            ModularWingDeltaS               =  1613, // ModularWingTaperedL
            ModularWingSweptL               =  1614, // ModularWingTaperedL
            ModularWingSweptM               =  1615, // ModularWingTaperedL
            ModularWingSweptS               =  1616, // ModularWingTaperedL
            ModularWingLongL                =  1617, // ModularWingTaperedL
            ModularWingLongM                =  1618, // ModularWingTaperedL
            ModularWingLongS                =  1619, // ModularWingTaperedL
            ModularWingAngledTaperedL       =  1620, // ModularWingTaperedL
            ModularWingAngledTaperedM       =  1621, // ModularWingTaperedL
            ModularWingAngledTaperedS       =  1622, // ModularWingTaperedL
            ModularWingTConnectorL          =  1623, // ModularWingTaperedL
            ModularWingTConnectorM          =  1624, // ModularWingTaperedL
            ModularWingPylon                =  1625, // ModularWingTaperedL
            ModularWingBlocks               =  1626, //
            WalkwaySmallBlocks              =  1690, //
            WalkwayLargeBlocks              =  1691, //
            RailingDiagonal                 =   333, //
            RailingVert                     =   334, // RailingDiagonal
            HullSmallBlocks                 =   380, //
            HullFullSmall                   =   381, //
            HullThinSmall                   =   382, // HullFullSmall
            HullArmoredFullSmall            =   383, //
            HullArmoredThinSmall            =   384, // HullArmoredFullSmall
            HullArmoredSmallBlocks          =   393, //
            WoodBlocks                      =   396, //
            WoodFull                        =   397, //
            WoodThin                        =   398, // WoodFull
            ConcreteBlocks                  =   399, //
            ConcreteFull                    =   400, //
            ConcreteThin                    =   401, // ConcreteFull
            HullLargeBlocks                 =   402, //
            HullFullLarge                   =   403, //
            HullThinLarge                   =   404, // HullFullLarge
            HullArmoredLargeBlocks          =   405, //
            HullArmoredFullLarge            =   406, //
            HullArmoredThinLarge            =   407, // HullArmoredFullLarge
            AlienBlocks                     =   408, //
            AlienFull                       =   409, //
            AlienThin                       =   410, // AlienFull
            HullCombatLargeBlocks           =   411, //
            HullCombatFullLarge             =   412, //
            HullCombatThinLarge             =   413, // HullCombatFullLarge
            TrussCube                       =   416, //
            StairsMS                        =   461, //
            GrowingPot                      =   462, //
            WindowShutterLargeBlocks        =   545, //
            StairsWedge                     =   672, //
            StairsWedgeLong                 =   673, //
            WalkwaySlope                    =   676, //
            RailingL                        =   681, // RailingDiagonal
            RailingRound                    =   682, // RailingDiagonal
            Window_v1x1                     =   770, //
            Window_s1x1                     =   771, // Window_v1x1
            WindowSmallBlocks               =   836, //
            TrussSmallBlocks                =   837, //
            WalkwayBlocks                   =   838, //
            StairsBlocks                    =   839, //
            WalkwayVertNew                  =   884, //
            WalkwaySlopeNew                 =   885, // WalkwayVertNew
            Window_v1x1Thick                =   966, //
            Window_v1x2Thick                =   967, // Window_v1x1Thick
            Window_v2x2Thick                =   968, // Window_v1x1Thick
            WindowVertShutterArmored        =   969, //
            WindowSlopedShutterArmored      =   970, // WindowVertShutterArmored
            WindowSloped2ShutterArmored     =   971, // WindowVertShutterArmored
            WindowVertShutterTransArmored   =   972, // WindowVertShutterArmored
            WindowSlopedShutterTransArmored =   973, // WindowVertShutterArmored
            WindowArmoredSmallBlocks        =   974, //
            WindowShutterSmallBlocks        =   976, //
            Window_s1x1Thick                =   977, // Window_v1x1Thick
            Window_s1x2Thick                =   978, // Window_v1x1Thick
            Window_sd1x1Thick               =   979, // Window_v1x1Thick
            Window_sd1x2Thick               =   980, // Window_v1x1Thick
            Window_c1x1Thick                =   981, // Window_v1x1Thick
            Window_c1x2Thick                =   982, // Window_v1x1Thick
            Window_cr1x1Thick               =   983, // Window_v1x1Thick
            Window_crc1x1Thick              =   984, // Window_v1x1Thick
            Window_crsd1x1Thick             =   985, // Window_v1x1Thick
            Window_sd1x2V2Thick             =   986, // Window_v1x1Thick
            HangarDoor01Large               =   987, // HangarDoor01Medium
            HangarDoor01Small               =   988, // HangarDoor01Medium
            Window_v1x1ThickInv             =   989, // Window_v1x1Thick
            Window_v1x2ThickInv             =   990, // Window_v1x1Thick
            Window_v2x2ThickInv             =   991, // Window_v1x1Thick
            Window_s1x1ThickInv             =   992, // Window_v1x1Thick
            Window_s1x2ThickInv             =   993, // Window_v1x1Thick
            Window_sd1x1ThickInv            =   994, // Window_v1x1Thick
            Window_sd1x2ThickInv            =   995, // Window_v1x1Thick
            Window_c1x1ThickInv             =   996, // Window_v1x1Thick
            Window_c1x2ThickInv             =   997, // Window_v1x1Thick
            Window_cr1x1ThickInv            =   998, // Window_v1x1Thick
            Window_crc1x1ThickInv           =   999, // Window_v1x1Thick

            // Deco Blocks
            DecoStoneTemplate               =  1300, //
            StoneBarbarian                  =  1301, // DecoStoneTemplate
            CelticCross                     =  1302, // DecoStoneTemplate
            DemonHead                       =  1303, // DecoStoneTemplate
            DemonicStatue                   =  1305, // DecoStoneTemplate
            GothicFountain                  =  1306, // DecoStoneTemplate
            GreekHead                       =  1307, // DecoStoneTemplate
            MayanStatueSnake                =  1308, // DecoStoneTemplate
            SnakeStatue                     =  1309, // DecoStoneTemplate
            StatueSkull                     =  1310, // DecoStoneTemplate
            TigerStatue                     =  1311, // DecoStoneTemplate
            AncientStatue                   =  1312, // DecoStoneTemplate
            DecoVesselBlocks                =  1719, //
            SVDecoAeroblister01             =  1720, //
            SVDecoAirbrake01                =  1721, // SVDecoAeroblister01
            SVDecoAntenna01                 =  1722, // SVDecoAeroblister01
            SVDecoAntenna02                 =  1723, // SVDecoAeroblister01
            SVDecoArmor1x                   =  1724, // SVDecoAeroblister01
            SVDecoArmor2x                   =  1725, // SVDecoAeroblister01
            SVDecoFin01                     =  1726, // SVDecoAeroblister01
            SVDecoFin02                     =  1727, // SVDecoAeroblister01
            SVDecoFin03                     =  1728, // SVDecoAeroblister01
            SVDecoGreeble01                 =  1729, // SVDecoAeroblister01
            SVDecoGreeble02                 =  1730, // SVDecoAeroblister01
            SVDecoGreeble03                 =  1731, // SVDecoAeroblister01
            SVDecoIntake01                  =  1732, // SVDecoAeroblister01
            SVDecoIntake02                  =  1733, // SVDecoAeroblister01
            SVDecoLightslot2x               =  1734, // SVDecoAeroblister01
            SVDecoLightslot3x               =  1735, // SVDecoAeroblister01
            SVDecoStrake01                  =  1736, // SVDecoAeroblister01
            SVDecoStrake02                  =  1737, // SVDecoAeroblister01
            SVDecoVent01                    =  1738, // SVDecoAeroblister01
            DecoTribalBlocks                =  1739, // DecoBlocks
            TribalBarrels                   =  1740, // DecoTemplate
            TribalBarrow                    =  1741, // DecoTemplate
            TribalBaskets                   =  1742, // DecoTemplate
            TribalBed1                      =  1743, // DecoTemplate
            TribalBed2                      =  1744, // DecoTemplate
            TribalBookcase1                 =  1745, // DecoTemplate
            TribalBookcase2                 =  1746, // DecoTemplate
            TribalBuckets                   =  1747, // DecoTemplate
            TribalCabinet1                  =  1748, // DecoTemplate
            TribalCabinet2                  =  1749, // DecoTemplate
            TribalCauldron                  =  1750, // DecoTemplate
            TribalDryFish                   =  1751, // DecoTemplate
            TribalLoom                      =  1752, // DecoTemplate
            TribalOven                      =  1753, // DecoTemplate
            TribalSacks                     =  1754, // DecoTemplate
            TribalTable1                    =  1755, // DecoTemplate
            TribalTable2                    =  1756, // DecoTemplate
            TribalWoodSaw                   =  1757, // DecoTemplate
            TribalTrunkAxe                  =  1758, // DecoTemplate
            TribalTorch                     =  1759, // DecoTemplate
            TribalFirepit                   =  1760, // DecoTemplate
            TribalFirewood                  =  1761, // DecoTemplate
            TribalBoat                      =  1762, // DecoTemplate
            TribalChair                     =  1763, // DecoTemplate
            TribalAnvil                     =  1764, // DecoTemplate
            TribalCauldron2                 =  1765, // DecoTemplate
            TribalHearth                    =  1766, // DecoTemplate
            TribalTub                       =  1767, // DecoTemplate
            TribalBox                       =  1768, // DecoTemplate
            ContainerMS01Small              =  1769, // ContainerMS01
            ScifiContainer1Small            =  1770, // ContainerMS01
            ScifiContainer2Small            =  1771, // ContainerMS01
            ScifiContainerPowerSmall        =  1772, // ContainerMS01
            TurretEnemyBallista             =  1773, // TurretIONCannon
            ThrusterGVRoundNormalT2         =  1774, // ThrusterGVRoundNormal
            ThrusterGVRoundLarge            =  1775, // ThrusterGVRoundNormal
            ThrusterGVRoundLargeT2          =  1776, // ThrusterGVRoundNormal
            CargoPalette01                  =  1777, // ContainerMS01
            CargoPalette02                  =  1778, // ContainerMS01
            CargoPalette03                  =  1779, // ContainerMS01
            CargoPalette04                  =  1780, // ContainerMS01
            WoodExtended                    =  1782, // WoodFull
            ConcreteExtended                =  1783, // ConcreteFull
            ConcreteArmoredExtended         =  1784, // ConcreteArmoredFull
            PlasticExtendedLarge            =  1785, // PlasticFullLarge
            HullExtendedLarge               =  1786, // HullFullLarge
            HullArmoredExtendedLarge        =  1787, // HullArmoredFullLarge
            HullCombatExtendedLarge         =  1788, // HullCombatFullLarge
            AlienExtended                   =  1789, // AlienFull
            PlasticExtendedSmall            =  1790, // PlasticFullSmall
            HullExtendedSmall               =  1791, // HullFullSmall
            HullArmoredExtendedSmall        =  1792, // HullArmoredFullSmall
            HullCombatExtendedSmall         =  1793, // HullCombatFullSmall
            CapacitorMS                     =   256, //
            ConsoleMS01                     =   261, //
            Antenna                         =   262, //
            TurretRadar                     =   289, //
            NPCAlienTemplate                =   328, //
            NPCHumanTemplate                =   329, //
            AntennaBlocks                   =   330, //
            ContainerUltraRare              =   331, // ContainerMS01
            IndoorPlant01                   =   629, //
            IndoorPlant02                   =   630, // IndoorPlant01
            IndoorPlant03                   =   631, // IndoorPlant01
            ConsoleSmallMS01                =   635, //
            ConsoleLargeMS01                =   636, //
            ConsoleLargeMS02                =   637, // ConsoleLargeMS01
            ConsoleMapMS01                  =   638, // ConsoleLargeMS01
            DecoBlocks                      =   927, //
            ConsoleBlocks                   =   928, //
            IndoorPlants                    =   929, //
            HoloScreen01                    =   950, //
            HoloScreen02                    =   951, // HoloScreen01
            HoloScreen03                    =   952, // HoloScreen01
            HoloScreen04                    =   953, // HoloScreen01
            HoloScreen05                    =   954, // HoloScreen01

            // Devices
            DoorBlocks                      =  1002, //
            DoorInterior01                  =  1003, //
            DoorInterior02                  =  1004, // DoorInterior01
            HangarDoor01Small2              =  1005, // HangarDoor01Medium
            HangarDoor01Medium2             =  1006, // HangarDoor01Medium
            HangarDoor01Large2              =  1007, // HangarDoor01Medium
            HangarDoorBlocks                =  1008, //
            CockpitOpenSV                   =  1009, //
            ShutterDoor1x1                  =  1011, //
            ShutterDoor2x2                  =  1012, // ShutterDoor1x1
            ShutterDoor3x3                  =  1013, // ShutterDoor1x1
            ShutterDoor4x4                  =  1014, // ShutterDoor1x1
            ShutterDoor5x5                  =  1015, // ShutterDoor1x1
            ShutterDoorLargeBlocks          =  1016, //
            ShutterDoor1x1SV                =  1017, //
            ShutterDoor2x2SV                =  1018, // ShutterDoor1x1SV
            ShutterDoor3x3SV                =  1019, // ShutterDoor1x1SV
            ShutterDoorSmallBlocks          =  1020, //
            ShutterDoor3x4SV                =  1021, // ShutterDoor1x1SV
            Ramp3x1x1                       =  1022, // RampTemplate
            Ramp3x2x1                       =  1023, // RampTemplate
            Ramp3x3x1                       =  1024, // RampTemplate
            Ramp3x4x2                       =  1025, // RampTemplate
            Ramp3x5x3                       =  1026, // RampTemplate
            Ramp1x1x1                       =  1027, // RampTemplate
            Ramp1x2x1                       =  1028, // RampTemplate
            Ramp1x3x1                       =  1029, // RampTemplate
            Ramp1x4x2                       =  1030, // RampTemplate
            RampBlocks                      =  1031, //
            GeneratorMST2                   =  1034, // GeneratorMS
            FuelTankMSLargeT2               =  1035, // FuelTankMSLarge
            ShutterDoor1x2                  =  1036, // ShutterDoor1x1
            ShutterDoor2x3                  =  1037, // ShutterDoor1x1
            LandinggearHeavySV              =  1064, // LandinggearSV
            ScifiBed                        =  1072, // DecoTemplate
            ScifiLargeSofa                  =  1073, // DecoTemplate
            ScifiNightstand                 =  1074, // DecoTemplate
            CockpitOpen2SV                  =  1092, //
            CockpitBlocksSV                 =  1093, //
            CockpitSV04                     =  1094, //
            LCDScreenBlocks                 =  1095, //
            LCDNoFrame1x1                   =  1096, //
            LCDFrame1x1                     =  1097, // LCDNoFrame1x1
            LCDNoFrame1x2                   =  1098, // LCDNoFrame1x1
            LCDFrame1x2                     =  1099, // LCDNoFrame1x1
            LCDNoFrame05x1                  =  1100, // LCDNoFrame1x1
            LCDNoFrame02x1                  =  1101, // LCDNoFrame1x1
            LCDNoFrame05x05                 =  1102, // LCDNoFrame1x1
            LCDNoFrame02x05                 =  1103, // LCDNoFrame1x1
            TurretGVTool                    =  1104, // TurretDrillTemplate
            TurretMSTool                    =  1105, // TurretDrillTemplate
            ThrusterGVRoundArmored          =  1106, // ThrusterGVRoundNormal
            ThrusterGVRoundBlocks           =  1107, //
            AutoMiningDeviceT1              =  1108, //
            AutoMiningDeviceT2              =  1109, // AutoMiningDeviceT1
            AutoMiningDeviceT3              =  1110, // AutoMiningDeviceT1
            RepairBayBA                     =  1111, //
            DoorArmoredBlocks               =  1112, //
            DoorVertical                    =  1113, // DoorMS01
            DoorVerticalGlass               =  1114, // DoorInterior01
            DoorVerticalArmored             =  1115, // DoorArmored
            LandinggearSingleShort          =  1116, // LandinggearSV
            LandinggearDoubleShort          =  1117, // LandinggearSV
            LandinggearBlocksSV             =  1118, //
            LandinggearBlocksHeavySV        =  1119, // LandinggearBlocksSV
            LandinggearBlocksCV             =  1120, //
            LandinggearSingleCV             =  1121, // LandinggearMSHeavy
            LandinggearSingleShortCV        =  1122, // LandinggearMSHeavy
            LandinggearDoubleCV             =  1123, // LandinggearMSHeavy
            LandinggearDoubleShortCV        =  1124, // LandinggearMSHeavy
            HoverEngineSmall                =  1127, //
            Furnace                         =  1132, //
            TradingStation                  =  1133, //
            ATM                             =  1134, //
            SensorTriggerBlocksSV           =  1230, //
            RepairBayBAT2                   =  1231, // RepairBayBA
            Closet                          =  1232, // DecoTemplate
            DoorVerticalGlassArmored        =  1233, // DoorArmored
            DoorInterior01Armored           =  1234, // DoorArmored
            DoorInterior02Armored           =  1235, // DoorArmored
            DrillAttachmentT2               =  1236, // DrillAttachment
            CockpitSV06                     =  1237, // CockpitSV07
            GrowingPotConcrete              =  1238, // GrowingPot
            GrowingPotWood                  =  1239, // GrowingPot
            ScifiTableNPC2                  =  1240, // NPCAlienTemplate
            ScifiTableNPC3                  =  1241, // NPCAlienTemplate
            ScifiLargeSofaNPC               =  1242, // NPCAlienTemplate
            ConsoleSmallNPC                 =  1243, // NPCAlienTemplate
            ScifiTableV2NPC                 =  1244, // NPCAlienTemplate
            SofaNPC                         =  1245, // NPCAlienTemplate
            StandingNPC                     =  1246, // NPCAlienTemplate
            ControlStationNPC               =  1247, // NPCAlienTemplate
            ReceptionTableNPC               =  1248, // NPCAlienTemplate
            ScifiSofaNPC                    =  1249, // NPCAlienTemplate
            ScifiTableNPC                   =  1250, // NPCAlienTemplate
            StandingNPC2                    =  1251, // NPCAlienTemplate
            ConsoleSmallHuman               =  1252, // NPCHumanTemplate
            CockpitBlocksCV                 =  1253, //
            AlienDeviceBlocks               =  1254, //
            SensorTriggerBlocks             =  1257, //
            TrapDoor                        =  1258, //
            LightBarrier                    =  1259, //
            MotionSensor                    =  1260, //
            Lever                           =  1262, //
            ExplosiveBlocks                 =  1263, //
            TrapDoorAnim                    =  1264, //
            TriggerPlate                    =  1265, //
            LightLantern                    =  1272, // LightMS01
            LightMS01Corner                 =  1273, // LightMS01
            LightMS01Offset                 =  1274, // LightMS01
            LightMS02                       =  1275, // LightMS01
            LightMS03                       =  1276, // LightMS01
            LightMS04                       =  1277, // LightMS01
            LightLargeBlocks                =  1278, //
            ReceptionTable                  =  1279, // DecoTemplate
            SmallTable                      =  1280, // DecoTemplate
            DecoBlocks2                     =  1281, // DecoBlocks
            Level4Prop2                     =  1282, // DecoTemplate
            Level4Prop3                     =  1283, // DecoTemplate
            Freezer                         =  1284, // FridgeMS02
            Level5FreezerOpened             =  1285, // DecoTemplate
            LabTable1                       =  1286, // DecoTemplate
            LabTable2                       =  1287, // DecoTemplate
            LabTable3                       =  1288, // DecoTemplate
            LockerWShelves                  =  1289, // DecoTemplate
            OperationTableWDrawers          =  1290, // DecoTemplate
            Props6BoxLarge1                 =  1291, // DecoTemplate
            Props6BoxLarge2                 =  1292, // DecoTemplate
            Props6BoxMedium1                =  1293, // DecoTemplate
            ScannerBase1                    =  1294, // DecoTemplate
            Scanner2                        =  1295, // DecoTemplate
            Scanner3                        =  1296, // DecoTemplate
            Tank1                           =  1297, // DecoTemplate
            Tank2                           =  1298, // DecoTemplate
            Console4                        =  1299, // DecoTemplate
            ArmorLocker                     =  1370, //
            Deconstructor                   =  1371, //
            RepairStation                   =  1372, //
            Portal                          =  1373, //
            PlayerSpawner                   =  1374, //
            DoorBlocksSV                    =  1375, //
            DoorInterior01SV                =  1376, // DoorSS01
            Teleporter                      =  1377, //
            ArmorLockerSV                   =  1380, //
            PlayerSpawnerPlateThin          =  1385, // PlayerSpawner
            Ventilator                      =  1405, //
            TrussWall                       =  1406, // TrussCube
            TrussCylinder                   =  1407, // TrussCube
            TrussHalfRound                  =  1408, // TrussCube
            TrussQuarterRound               =  1409, // TrussCube
            TrussQuarterRoundInv            =  1410, // TrussCube
            TrussCurveOutSlope              =  1411, // TrussCube
            TrussWedgeThin                  =  1412, // TrussCube
            TrussQuarterRoundThin           =  1413, // TrussCube
            TrussCornerThin                 =  1414, // TrussCube
            TrussCornerRoundThin            =  1415, // TrussCube
            TrussCornerRoundThin2           =  1416, // TrussCube
            ThrusterGVJetRound1x3x1         =  1417, // ThrusterGVRoundNormal
            ElderberryBushDeco              =  1418, // IndoorPlant01
            ElderberryBushBlueDeco          =  1419, // IndoorPlant01
            AlienPalmTreeDeco               =  1420, // IndoorPlant01
            AlienTentacleDeco               =  1421, // IndoorPlant01
            HollywoodJuniperDeco            =  1422, // IndoorPlant01
            BallTreeDeco                    =  1423, // IndoorPlant01
            BallFlower01Deco                =  1424, // IndoorPlant01
            OnionFlowerDeco                 =  1425, // IndoorPlant01
            FantasyPlant1Deco               =  1426, // IndoorPlant01
            AkuaFernDeco                    =  1427, // IndoorPlant01
            GlowTube01Deco                  =  1428, // IndoorPlant01
            DoorInterior01SlimSV            =  1429, // DoorSS01
            DoorSS01Slim                    =  1430, // DoorSS01
            WarpDriveSV                     =  1435, // WarpDrive
            LandClaimDevice                 =  1436, //
            WarpDriveTankSV                 =  1437, //
            StairsBlocksConcrete            =  1440, // StairsBlocks
            StairShapesShortConcrete        =  1441, // StairShapes
            StairShapesLongConcrete         =  1442, // StairShapes
            StairsBlocksWood                =  1443, // StairsBlocks
            StairShapesShortWood            =  1444, // StairShapes
            StairShapesLongWood             =  1445, // StairShapes
            ConstructorSV                   =  1446, // ConstructorSmallV2
            ConstructorHV                   =  1447, // ConstructorSmallV2
            StandingHuman                   =  1453, // NPCHumanTemplate
            StandingHuman2                  =  1454, // NPCHumanTemplate
            ControlStationHuman             =  1455, // NPCHumanTemplate
            ReceptionTableHuman             =  1456, // NPCHumanTemplate
            ControlStationHuman2            =  1457, // NPCHumanTemplate
            ScifiTableHuman                 =  1458, // NPCHumanTemplate
            ScifiLargeSofaHuman             =  1459, // NPCHumanTemplate
            TacticalOfficer                 =  1460, // NPCHumanTemplate
            CommandingOfficer               =  1461, // NPCHumanTemplate
            SecurityGuard                   =  1462, // NPCHumanTemplate
            OperatorPilot                   =  1463, // NPCHumanTemplate
            EngineerMainStation             =  1464, // NPCHumanTemplate
            AlienNPCBlocks                  =  1465, //
            HumanNPCBlocks                  =  1466, //
            CommandingOfficer2              =  1467, // NPCHumanTemplate
            SecurityGuard2                  =  1468, // NPCHumanTemplate
            CommandingOfficerAlien          =  1469, // NPCAlienTemplate
            SecurityGuardAlien              =  1470, // NPCAlienTemplate
            StandingAlienAssassin           =  1472, // NPCAlienTemplate
            StandingHexapod                 =  1473, // NPCAlienTemplate
            DancingHuman1                   =  1474, // NPCHumanTemplate
            DancingHuman2                   =  1475, // NPCHumanTemplate
            DancingHuman3                   =  1476, // NPCHumanTemplate
            DancingAlien1                   =  1477, // NPCHumanTemplate
            HoverEngineThruster             =  1484, //
            MobileAirCon                    =  1485, //
            RepairBayCVT2                   =  1486, // RepairBayCV
            CaptainChair01                  =  1487, //
            RepairBayConsole                =  1490, //
            LightCorner                     =  1491, // LightMS01
            LightCorner02                   =  1492, // LightMS01
            BunkBed02                       =  1493, // DecoTemplate
            SolarPanelBlocks                =  1494, //
            SolarGenerator                  =  1495, //
            SolarPanelSlope                 =  1496, //
            SolarPanelHorizontal            =  1497, // SolarPanelSlope
            SolarPanelHorizontal2           =  1498, // SolarPanelSlope
            SolarPanelHorizontalMount       =  1499, // SolarPanelSlope
            ForcefieldEmitterBlocks         =  1500, //
            ForcefieldEmitter1x1            =  1501, //
            ForcefieldEmitter1x2            =  1502, // ForcefieldEmitter1x1
            ForcefieldEmitter1x3            =  1503, // ForcefieldEmitter1x1
            ForcefieldEmitter3x5            =  1504, // ForcefieldEmitter1x1
            ForcefieldEmitter3x9            =  1505, // ForcefieldEmitter1x1
            ForcefieldEmitter5x11           =  1506, // ForcefieldEmitter1x1
            ForcefieldEmitter7x14           =  1507, // ForcefieldEmitter1x1
            SolarPanelSlope2                =  1510, // SolarPanelSlope
            SolarPanelSlope3                =  1511, // SolarPanelSlope
            SolarPanelHorizontalStand       =  1512, // SolarPanelSlope
            SolarPanelSmallBlocks           =  1513, //
            SolarPanelSlope3Small           =  1514, //
            SolarPanelSlopeSmall            =  1515, // SolarPanelSlope3Small
            SolarPanelHorizontalSmall       =  1516, // SolarPanelSlope3Small
            TurretEnemyRocket               =  1517, // TurretIONCannon
            TurretEnemyArtillery            =  1518, // TurretIONCannon
            SurvivalTent01                  =  1535, //
            MedicalStationBlocks            =  1571, //
            DetectorHVT1                    =  1575, //
            DetectorHVT2                    =  1576, // DetectorHVT1
            ExplosiveBlockFull              =  1577, //
            ExplosiveBlockThin              =  1578, // ExplosiveBlockFull
            ExplosiveBlock2Full             =  1579, //
            ExplosiveBlock2Thin             =  1580, // ExplosiveBlock2Full
            CloneChamberHV                  =  1583, //
            MedicStationHV                  =  1584, //
            ThrusterJetRound2x5x2V2         =  1585, // ThrusterJetRound2x5x2
            DroneSpawner                    =  1586, //
            DroneSpawner2                   =  1587, // DroneSpawner
            FridgeBlocks                    =  1588, //
            HoverEngineSmallDeco            =  1589, // DecoTemplate
            HoverEngineLargeDeco            =  1590, // DecoTemplate
            ThrusterJetRound3x10x3V2        =  1591, // ThrusterJetRound3x10x3
            ThrusterJetRound3x13x3V2        =  1592, // ThrusterJetRound3x13x3
            RemoteConnection                =  1627, //
            ConstructorT0                   =  1628, // ConstructorT1V2
            HeavyWindowG                    =  1629, // HeavyWindowA
            HeavyWindowGInv                 =  1630, // HeavyWindowA
            HeavyWindowH                    =  1631, // HeavyWindowA
            HeavyWindowHInv                 =  1632, // HeavyWindowA
            HeavyWindowI                    =  1633, // HeavyWindowA
            HeavyWindowIInv                 =  1634, // HeavyWindowA
            HeavyWindowJ                    =  1635, // HeavyWindowA
            HeavyWindowJInv                 =  1636, // HeavyWindowA
            CargoContainerSmall             =  1676, //
            CargoContainerMedium            =  1677, // CargoContainerSmall
            CargoContainerSV                =  1678, // CargoContainerSmall
            ContainerControllerLarge        =  1682, //
            ContainerExtensionLarge         =  1683, //
            ContainerControllerSmall        =  1684, // ContainerControllerLarge
            ContainerExtensionSmall         =  1685, // ContainerExtensionLarge
            ContainerHarvestControllerSmall =  1686, // ContainerControllerLarge
            ContainerHarvestControllerLarge =  1687, // ContainerControllerLarge
            ContainerAmmoControllerSmall    =  1688, // ContainerControllerLarge
            ContainerAmmoControllerLarge    =  1689, // ContainerControllerLarge
            RampLargeBlocks                 =  1692, //
            RampSmallBlocks                 =  1693, //
            LandingGearSVRetDouble          =  1694, // LandinggearSV
            LandingGearSVSideStrut          =  1695, // LandinggearSV
            LandingGearSVSingle             =  1696, // LandinggearSV
            LandingGearSVRetSkid            =  1697, // LandinggearSV
            LandingGearSVRetLargeDouble     =  1698, // LandinggearSV
            LandingGearSVRetLargeDoubleV2   =  1699, // LandinggearSV
            LandingGearSVRetLargeDoubleV3   =  1700, // LandinggearSV
            LandingGearSVRetLargeSingle     =  1701, // LandinggearSV
            LandingGearSVRetLargeSingleV2   =  1702, // LandinggearSV
            LandingGearSVRetLargeSingleV3   =  1703, // LandinggearSV
            LandingGearCVRetLargeDouble     =  1704, // LandinggearMSHeavy
            LandingGearCVRetLargeSingle     =  1705, // LandinggearMSHeavy
            BoardingRampBlocks              =  1706, //
            BoardingRamp1x2x3               =  1707, // RampTemplate
            BoardingRamp2x2x3               =  1708, // RampTemplate
            BoardingRamp3x2x3               =  1709, // RampTemplate
            BoardingRamp3x3x5               =  1710, // RampTemplate
            ContainerLargeBlocks            =  1711, //
            ContainerSmallBlocks            =  1712, //
            ContainerMS01Large              =  1713, // ContainerMS01
            ScifiContainer1Large            =  1714, // ContainerMS01
            ScifiContainer2Large            =  1715, // ContainerMS01
            ScifiContainerPowerLarge        =  1716, // ContainerMS01
            CockpitMS01                     =   257, //
            FuelTankMSSmall                 =   259, //
            FuelTankMSLarge                 =   260, // FuelTankMSSmall
            OxygenTankMS                    =   263, //
            PassengerSeatMS                 =   266, //
            CockpitMS02                     =   267, //
            MedicinelabMS                   =   270, //
            RCSBlockSV                      =   272, //
            ContainerMS01                   =   273, //
            GravityGeneratorMS              =   278, //
            LightSS01                       =   279, //
            LightMS01                       =   280, //
            DoorMS01                        =   281, //
            OxygenStation                   =   291, //
            OfflineProtector                =   335, //
            WarpDriveTank                   =   336, //
            SurvivalTent                    =   339, //
            LandinggearSV                   =   417, //
            GeneratorSV                     =   418, //
            FuelTankSV                      =   419, //
            RCSBlockMS                      =   420, //
            OxygenTankSV                    =   422, //
            FridgeSV                        =   423, //
            LandinggearMSHeavy              =   445, //
            RampSteep                       =   446, // Hull
            ThrusterSVRoundNormal           =   449, //
            ThrusterSVRoundArmored          =   450, // ThrusterSVRoundNormal
            ThrusterSVRoundSlant            =   451, // ThrusterSVRoundNormal
            ThrusterMSRoundArmored          =   453, //
            ThrusterMSRoundSlant            =   454, // ThrusterMSRoundArmored
            ThrusterMSRoundNormal           =   455, // ThrusterMSRoundArmored
            ThrusterSVDirectional           =   456, //
            ThrusterMSDirectional           =   457, //
            ThrusterMSRoundNormal2x2        =   458, //
            CockpitSV_ShortRange            =   459, //
            DoorSS01                        =   460, //
            ElevatorMS                      =   468, //
            GeneratorMS                     =   469, //
            WeaponSV05Homing                =   489, // WeaponSV05
            ThrusterMSRoundNormal3x3        =   497, //
            GeneratorBA                     =   498, //
            ContainerSpecialEvent           =   514, // ContainerMS01
            ContainerBlocks                 =   535, //
            ThrusterSVRoundBlocks           =   536, //
            ThrusterMSRoundSlant2x2         =   537, // ThrusterMSRoundNormal2x2
            ThrusterMSRoundArmored2x2       =   538, // ThrusterMSRoundNormal2x2
            ThrusterMSRoundSlant3x3         =   539, // ThrusterMSRoundNormal3x3
            ThrusterMSRoundArmored3x3       =   540, // ThrusterMSRoundNormal3x3
            AlienContainer                  =   541, //
            AlienContainerRare              =   542, // AlienContainer
            AlienContainerVeryRare          =   543, // AlienContainer
            AlienContainerUltraRare         =   544, // AlienContainer
            OxygenGenerator                 =   554, //
            SpotlightSSCube                 =   556, //
            Core                            =   558, //
            CoreNPC                         =   560, //
            LightPlant01                    =   564, //
            LightWork                       =   569, //
            FridgeMS02                      =   583, //
            FridgeMS                        =   584, // FridgeMS02
            WaterGenerator                  =   588, //
            ThrusterGVDirectional           =   589, //
            ThrusterGVRoundNormal           =   590, //
            HoverBooster                    =   603, //
            RCSBlockGV                      =   604, //
            Bed                             =   612, // DecoTemplate
            Sofa                            =   613, // DecoTemplate
            KitchenCounter                  =   614, // DecoTemplate
            KitchenTable                    =   615, // DecoTemplate
            Bookshelf                       =   617, // DecoTemplate
            ControlStation                  =   618, // DecoTemplate
            BathroomCounter                 =   619, // DecoTemplate
            Toilet                          =   620, // DecoTemplate
            Shower                          =   621, // DecoTemplate
            LightInterior01                 =   622, // LightMS01
            LightInterior02                 =   623, // LightMS01
            CockpitSV02                     =   632, //
            CockpitSV05                     =   633, // CockpitSV02
            LightWork02                     =   652, //
            Flare                           =   653, //
            EntitySpawner1                  =   658, //
            EntitySpawnerPlateThin          =   668, // EntitySpawner1
            ContainerPersonal               =   686, //
            CockpitSV07New                  =   688, // CockpitSV02
            CockpitSV05New                  =   689, // CockpitSV02
            CockpitSV02New                  =   690, // CockpitSV02
            RailingSlopeLeft                =   691, // RailingDiagonal
            RailingSlopeRight               =   692, // RailingDiagonal
            ThrusterJetRound3x7x3           =   694, //
            ThrusterJetRound3x10x3          =   695, // ThrusterJetRound3x7x3
            ThrusterJetRound3x13x3          =   696, // ThrusterJetRound3x7x3
            ThrusterJetRound1x3x1           =   697, // ThrusterJetRound3x7x3
            ThrusterJetRound2x5x2           =   698, // ThrusterJetRound3x7x3
            ConstructorSurvival             =   711, //
            PassengerSeatSV                 =   712, //
            ConstructorT2                   =   714, //
            PassengerSeat2SV                =   715, //
            OxygenTankSmallMS               =   717, //
            WarpDrive                       =   720, //
            OxygenStationSV                 =   721, //
            LandinggearShort                =   722, //
            LandinggearMSLight              =   723, // LandinggearMSHeavy
            ContainerAmmoLarge              =   724, //
            ConsoleLargeMS01a               =   727, // ConsoleLargeMS01
            ContainerAmmoSmall              =   728, // ContainerAmmoLarge
            DockingPad                      =   730, // LandinggearShort
            ContainerHarvest                =   732, //
            ThrusterGVRoundSlant            =   768, // ThrusterGVRoundNormal
            TurretMSArtillery               =   769, // TurretMSArtilleryRetract
            ThrusterMSRoundBlocks           =   772, //
            ThrusterMSRound2x2Blocks        =   778, //
            LandinggearSingle               =   779, //
            CloneChamber                    =   781, //
            Window_v1x1Inv                  =   795, // Window_v1x1
            Window_v1x2                     =   796, // Window_v1x1
            Window_v1x2Inv                  =   797, // Window_v1x1
            Window_v2x2                     =   798, // Window_v1x1
            Window_v2x2Inv                  =   799, // Window_v1x1
            Window_s1x1Inv                  =   800, // Window_v1x1
            Window_s1x2                     =   801, // Window_v1x1
            Window_s1x2Inv                  =   802, // Window_v1x1
            Window_sd1x1                    =   803, // Window_v1x1
            Window_sd1x1Inv                 =   804, // Window_v1x1
            Window_sd1x2                    =   805, // Window_v1x1
            Window_sd1x2Inv                 =   806, // Window_v1x1
            Window_c1x1                     =   807, // Window_v1x1
            Window_c1x1Inv                  =   808, // Window_v1x1
            Window_c1x2                     =   809, // Window_v1x1
            Window_c1x2Inv                  =   810, // Window_v1x1
            Window_cr1x1                    =   811, // Window_v1x1
            Window_cr1x1Inv                 =   812, // Window_v1x1
            Window_crc1x1                   =   813, // Window_v1x1
            Window_crc1x1Inv                =   814, // Window_v1x1
            Window_crsd1x1                  =   815, // Window_v1x1
            Window_crsd1x1Inv               =   816, // Window_v1x1
            Window_sd1x2V2                  =   817, // Window_v1x1
            Window_sd1x2V2Inv               =   818, // Window_v1x1
            RampTemplate                    =   819, //
            ThrusterMSRound3x3Blocks        =   835, //
            RCSBlockMS_T2                   =   934, //
            ConstructorT1V2                 =   960, //
            FoodProcessorV2                 =   962, //
            CockpitSV01                     =   963, // CockpitSV02
            OxygenGeneratorSmall            =   964, //
            DoorArmored                     =   965, //
            HangarDoor01Medium              =   975, //

            // Farming
            PlantDead                       =  1313, //
            Trader                          =  1314, //
            PlantDead2                      =  1316, // PlantDead
            ExplosiveBlocks2                =  1318, // ExplosiveBlocks
            SpotlightSlope                  =  1319, // SpotlightSSCube
            SpotlightSlopeHorizontal        =  1320, // SpotlightSSCube
            SpotlightBlocks                 =  1321, // SpotlightSSCube
            SproutDead                      =  1330, //
            DemonicStatueSmall              =  1331, // DecoStoneTemplate
            MayanStatueSnakeSmall           =  1332, // DecoStoneTemplate
            SnakeStatueSmall                =  1333, // DecoStoneTemplate
            TigerStatueSmall                =  1334, // DecoStoneTemplate
            Runestone                       =  1335, // DecoStoneTemplate
            DecoStoneBlocks                 =  1336, // DecoBlocks
            ReceptionTableCorner            =  1338, // DecoTemplate
            CoreNPCAdmin                    =  1360, // CoreNPC
            CorePlayerAdmin                 =  1361, // Core
            Antenna01                       =  1362, // Antenna
            Antenna02                       =  1363, // Antenna
            Antenna03                       =  1364, // Antenna
            Antenna04                       =  1365, // Antenna
            Antenna05                       =  1366, // Antenna

            // Weapons/Items
            TurretMSPulseLaser              =  1142, //
            TurretBaseCannon                =  1143, //
            TurretBaseRocket                =  1144, //
            TurretMSCannon                  =  1145, //
            TurretMSFlak                    =  1146, //
            TurretBaseMinigun               =  1147, //
            TurretBasePulseLaser            =  1148, //
            TurretBaseArtillery             =  1149, //
            Wing12x9b                       =  1150, // Wing6x9a
            Wing12x9c                       =  1151, // Wing6x9a
            Wing9x6a                        =  1152, // Wing6x9a
            Wing9x6b                        =  1153, // Wing6x9a
            Wing9x6c                        =  1154, // Wing6x9a
            Wing6x9b                        =  1155, // Wing6x9a
            Wing6x9c                        =  1156, // Wing6x9a
            Wing6x5b                        =  1157, // Wing6x9a
            Wing6x5c                        =  1158, // Wing6x9a
            Wing6x5d                        =  1159, // Wing6x9a
            Wing6x5e                        =  1160, // Wing6x9a
            Wing6x9d                        =  1161, // Wing6x9a
            Wing6x9e                        =  1162, // Wing6x9a
            Wing12x9d                       =  1163, // Wing6x9a
            Wing12x9e                       =  1164, // Wing6x9a
            Wing9x6d                        =  1165, // Wing6x9a
            Wing9x6e                        =  1166, // Wing6x9a
            Window_3side1x1                 =  1183, // Window_v1x1
            Window_3side1x1Inv              =  1184, // Window_v1x1
            Window_L1x1                     =  1185, // Window_v1x1
            Window_L1x1Inv                  =  1186, // Window_v1x1
            Window_3side1x1Thick            =  1187, // Window_v1x1Thick
            Window_3side1x1ThickInv         =  1188, // Window_v1x1Thick
            Window_L1x1Thick                =  1189, // Window_v1x1Thick
            Window_L1x1ThickInv             =  1190, // Window_v1x1Thick
            DrillAttachmentCV               =  1582, //
            TurretMSProjectileBlocks        =  1637, //
            TurretMSRocketBlocks            =  1638, //
            TurretMSLaserBlocks             =  1639, //
            TurretMSToolBlocks              =  1640, //
            TurretMSArtilleryBlocks         =  1641, //
            DetectorSVT1                    =  1642, // DetectorHVT1
            DancingAlien2                   =  1645, // NPCHumanTemplate
            DancingAlien3                   =  1646, // NPCHumanTemplate
            DancingAlien4                   =  1647, // NPCHumanTemplate
            TurretBaseProjectileBlocks      =  1648, //
            TurretBaseRocketBlocks          =  1649, //
            TurretBaseLaserBlocks           =  1650, //
            TurretBaseArtilleryBlocks       =  1651, //
            TurretBaseFlakRetract           =  1652, // TurretBaseFlak
            TurretBasePlasmaRetract         =  1653, // TurretBasePlasma
            TurretBaseCannonRetract         =  1654, // TurretBaseCannon
            TurretBaseRocketRetract         =  1655, // TurretBaseRocket
            TurretBaseMinigunRetract        =  1656, // TurretBaseMinigun
            TurretBasePulseLaserRetract     =  1657, // TurretBasePulseLaser
            TurretBaseArtilleryRetract      =  1658, // TurretBaseArtillery
            TurretGVMinigunBlocks           =  1659, //
            TurretGVRocketBlocks            =  1660, //
            TurretGVPlasmaBlocks            =  1661, //
            TurretGVArtilleryBlocks         =  1662, //
            TurretGVToolBlocks              =  1663, //
            TurretGVMinigunRetract          =  1664, // TurretGVMinigun
            TurretGVRocketRetract           =  1665, // TurretGVRocket
            TurretGVPlasmaRetract           =  1666, // TurretGVPlasma
            TurretGVArtilleryRetract        =  1667, // TurretGVArtillery
            TurretGVDrillRetract            =  1668, // TurretGVDrill
            TurretGVToolRetract             =  1669, // TurretGVTool
            SentryGun03Retract              =  1670, // SentryGun03
            SentryGun05                     =  1671, // SentryGun03
            SentryGun05Retract              =  1672, // SentryGun03
            SentryGunBlocks                 =  1673, //
            SentryGun01Retract              =  1674, // SentryGun01
            SentryGun02Retract              =  1675, // SentryGun02
            TurretTemplate                  =   282, //
            TurretMSMinigunRetract          =   283, // TurretTemplate
            TurretMSRocketRetract           =   284, // TurretTemplate
            TurretMSMinigun                 =   287, // TurretMSMinigunRetract
            TurretMSRocket                  =   288, // TurretMSRocketRetract
            TurretDrillTemplate             =   320, //
            TurretMSDrillRetract            =   321, // TurretDrillTemplate
            TurretMSToolRetract             =   322, // TurretDrillTemplate
            TurretMSPulseLaserRetract       =   323, // TurretTemplate
            TurretMSPlasmaRetract           =   324, // TurretTemplate
            TurretMSFlakRetract             =   325, // TurretTemplate
            TurretMSCannonRetract           =   326, // TurretTemplate
            TurretMSArtilleryRetract        =   327, // TurretTemplate
            WeaponSV01                      =   428, //
            WeaponSV02                      =   429, //
            WeaponSV03                      =   430, //
            WeaponSV04                      =   431, //
            WeaponSV05                      =   432, //
            TurretIONCannon                 =   491, //
            TurretEnemyLaser                =   492, // TurretIONCannon
            TurretIONCannon2                =   555, //
            SentryGun01                     =   565, //
            SentryGun02                     =   566, //
            SentryGun03                     =   567, //
            WeaponMS01                      =   646, //
            WeaponMS02                      =   647, //
            TurretGVMinigun                 =   648, //
            TurretGVRocket                  =   649, //
            TurretGVPlasma                  =   650, //
            BunkBed                         =   651, // DecoTemplate
            SawAttachment                   =   669, //
            CockpitSV03                     =   670, // CockpitSV02
            CockpitSV07                     =   671, // CockpitSV02
            DrillAttachment                 =   683, //
            TurretGVDrill                   =   684, // TurretDrillTemplate
            TurretMSDrill                   =   685, // TurretDrillTemplate
            TurretBaseFlak                  =   700, //
            TurretBasePlasma                =   701, //
            TurretMSPlasma                  =   702, //
            OxygenHydrogenGenerator         =   706, // OxygenGenerator
            TurretGVArtillery               =   716, //
        }


        public enum FaceIndex
        {
            All    = -1,
            Top    = 0,
            Bottom = 1,
            Front  = 2,
            Left   = 3,
            Back   = 4,
            Right  = 5
        }
        public enum SymbolRotation
        {
            Up, Left, Down, Right
        }
        #endregion Types

        public static readonly Dictionary<EpbBlockType, EpbBlockTypeDefinition> BlockTypeDefinitions = new Dictionary<EpbBlockType, EpbBlockTypeDefinition>()
        {
            // Building Blocks:
            {(EpbBlockType)0x018d, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x018d, Name = "Wood Blocks",              VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x018e, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x018e, Name = "Wood Blocks",              VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},

            {(EpbBlockType)0x0190, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0190, Name = "Concrete Blocks",          VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x0191, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0191, Name = "Concrete Blocks",          VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},

            {EpbBlockType.HullFullLarge,     new EpbBlockTypeDefinition() { Type = EpbBlockType.HullFullLarge,     Name = "Steel Blocks L",           VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {EpbBlockType.HullThinLarge,     new EpbBlockTypeDefinition() { Type = EpbBlockType.HullThinLarge,     Name = "Steel Blocks L",           VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},
            {EpbBlockType.HullExtendedLarge, new EpbBlockTypeDefinition() { Type = EpbBlockType.HullExtendedLarge, Name = "Steel Blocks L",           VariantNames = new string[] {"Corner Small A", "Corner Small C", "CornerLongF", "CornerB", "WallCornerRound", "NotchedA", "NotchedB", "NotchedC", "CubeQuarter", "CylinderThinXJoint", "RampWedgeTopB", "CutCornerB" }}},
            
            {(EpbBlockType)0x0196, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0196, Name = "Hardened Steel Blocks L",  VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x0197, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0197, Name = "Hardened Steel Blocks L",  VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},

            {(EpbBlockType)0x0199, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0199, Name = "Alien Building Blocks",    VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x019a, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x019a, Name = "Alien Building Blocks",    VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},

            {(EpbBlockType)0x019c, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x019c, Name = "Combat Steel Blocks L",    VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x019d, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x019d, Name = "Combat Steel Blocks L",    VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},

            {(EpbBlockType)0x01ce, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x01ce, Name = "Growing Plot Steel",       VariantNames = new string[] {"Growing Plot Steel"}}},

            {(EpbBlockType)0x03c9, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x03c9, Name = "Shutter Window Blocks",    VariantNames = new string[] {"Unkown", "Vertical (non-transparent)"}}},
            {(EpbBlockType)0x03ca, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x03ca, Name = "Shutter Window Blocks",    VariantNames = new string[] {"Unkown", "Slope (non-transparent)"}}},
            {(EpbBlockType)0x03cb, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x03cb, Name = "Shutter Window Blocks",    VariantNames = new string[] {"Unkown", "Slope Inv (non-transparent)"}}},
            {(EpbBlockType)0x03cc, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x03cc, Name = "Shutter Window Blocks",    VariantNames = new string[] {"Unkown", "Vertical (transparent)"}}},
            {(EpbBlockType)0x03cd, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x03cd, Name = "Shutter Window Blocks",    VariantNames = new string[] {"Unkown", "Slope (transparent)" }}},

            {(EpbBlockType)0x0374, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0374, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Walkway" }}},
            {(EpbBlockType)0x0375, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0375, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Walkway Slope 2" }}},
            {(EpbBlockType)0x02a4, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02a4, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Walkway Slope" }}},
            {(EpbBlockType)0x014e, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x014e, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Railing" }}},
            {(EpbBlockType)0x014d, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x014d, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Railing Diagonal" }}},
            {(EpbBlockType)0x02b3, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02b3, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Railing Slope (left)" }}},
            {(EpbBlockType)0x02b4, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02b4, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Railing Slope (right)" }}},
            {(EpbBlockType)0x02a9, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02a9, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Railing L-Shape" }}},
            {(EpbBlockType)0x02aa, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02aa, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Railing Round" }}},
            {(EpbBlockType)0x04a7, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04a7, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Glass Railing" }}},
            {(EpbBlockType)0x04c9, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04c9, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Glass Railing Diagonal" }}},
            {(EpbBlockType)0x04c7, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04c7, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Glass Railing Slope (left)" }}},
            {(EpbBlockType)0x04c5, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04c5, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Glass Railing Slope (right)" }}},
            {(EpbBlockType)0x04ab, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04ab, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Glass Railing L-Shape" }}},
            {(EpbBlockType)0x04a9, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04a9, Name = "Walkway & Railing Blocks",    VariantNames = new string[] {"Glass Railing Round" }}},

            {(EpbBlockType)0x02a0, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02a0, Name = "Stairs Blocks",               VariantNames = new string[] {"Stairs Wedge" }}},
            {(EpbBlockType)0x02a1, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02a1, Name = "Stairs Blocks",               VariantNames = new string[] {"Stairs Wedge Long" }}},
            {(EpbBlockType)0x01cd, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x01cd, Name = "Stairs Blocks",               VariantNames = new string[] {"Stairs Freestanding" }}},
            {(EpbBlockType)0x0465, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0465, Name = "Stairs Blocks",               VariantNames = new string[] {"Stairs Wedge (texturable)", "Stairs Corner - right (texturable)", "Stairs Corner - left (texturable)" }}},
            {(EpbBlockType)0x0466, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0466, Name = "Stairs Blocks",               VariantNames = new string[] {"Stairs Wedge 2 (texturable)" }}},

            // In the block count list "Truss Blocks" variants all count as block type 0x433!
            {(EpbBlockType)0x01a0, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x01a0, Name = "Truss Blocks",                VariantNames = new string[] {"Cube" }}},
            {(EpbBlockType)0x02c1, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02c1, Name = "Truss Blocks",                VariantNames = new string[] {"Corner" }}},
            {(EpbBlockType)0x02c0, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x02c0, Name = "Truss Blocks",                VariantNames = new string[] {"Slope" }}},
            {(EpbBlockType)0x0583, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0583, Name = "Truss Blocks",                VariantNames = new string[] {"Curved Corner" }}},
            {(EpbBlockType)0x0580, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0580, Name = "Truss Blocks",                VariantNames = new string[] {"Round Corner" }}},
            {(EpbBlockType)0x0581, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0581, Name = "Truss Blocks",                VariantNames = new string[] {"Round Slope" }}},
            {(EpbBlockType)0x057f, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x057f, Name = "Truss Blocks",                VariantNames = new string[] {"Cylinder" }}},
            {(EpbBlockType)0x0582, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0582, Name = "Truss Blocks",                VariantNames = new string[] {"Inward Round Slope" }}},
            {(EpbBlockType)0x057e, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x057e, Name = "Truss Blocks",                VariantNames = new string[] {"Wall" }}},
            {(EpbBlockType)0x0584, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0584, Name = "Truss Blocks",                VariantNames = new string[] {"Thin Slope" }}},
            {(EpbBlockType)0x0585, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0585, Name = "Truss Blocks",                VariantNames = new string[] {"Round Slope Thin" }}},
            {(EpbBlockType)0x0586, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0586, Name = "Truss Blocks",                VariantNames = new string[] {"Thin Corner" }}},
            {(EpbBlockType)0x0587, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0587, Name = "Truss Blocks",                VariantNames = new string[] {"Round Slope Thin" }}},
            {(EpbBlockType)0x0588, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0588, Name = "Truss Blocks",                VariantNames = new string[] {"Corner Round Thin" }}},

            // In the block count list "Window Blocks L" variants all count as block type 0x468!
            {(EpbBlockType)0x0302, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0302, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Vertical 1x1" }}},
            {(EpbBlockType)0x031c, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x031c, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Vertical 1x2" }}},
            {(EpbBlockType)0x031e, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x031e, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Vertical 2x2" }}},
            {(EpbBlockType)0x0303, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0303, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Slope 1x1" }}},
            {(EpbBlockType)0x0321, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0321, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Slope 1x2" }}},
            {(EpbBlockType)0x0323, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0323, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Side 1x1" }}},
            {(EpbBlockType)0x0325, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0325, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Side 1x2" }}},
            {(EpbBlockType)0x0331, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0331, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Side 2 1x2" }}},
            {(EpbBlockType)0x0327, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0327, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Corner 1x1" }}},
            {(EpbBlockType)0x0329, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0329, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Corner 1x2" }}},
            {(EpbBlockType)0x032b, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x032b, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Round Vertical" }}},
            {(EpbBlockType)0x032d, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x032d, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Round Corner" }}},
            {(EpbBlockType)0x032f, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x032f, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Round Side" }}},
            {(EpbBlockType)0x04a1, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04a1, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Vertical L-Shape" }}},
            {(EpbBlockType)0x049f, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x049f, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Vertical Corner" }}},
            {(EpbBlockType)0x04ad, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04ad, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Round Corner Thin" }}},
            {(EpbBlockType)0x04ae, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04ae, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Connector A" }}},
            {(EpbBlockType)0x04af, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04af, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Connector B" }}},
            {(EpbBlockType)0x04b0, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04b0, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Round Corner Long" }}},
            {(EpbBlockType)0x04b1, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04b1, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Round Corner Edge" }}},
            {(EpbBlockType)0x04b2, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x04b2, Name = "Window Blocks L",             VariantNames = new string[] {"Unknown", "Corner Thin" }}},

            {(EpbBlockType)0x052b, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x052b, Name = "Armored Concrete Blocks",    VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x052c, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x052c, Name = "Armored Concrete Blocks",    VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},

            {(EpbBlockType)0x056b, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x056b, Name = "Steel Blocks L - Destroyed", VariantNames = new string[] {"Cube - Destroyed", "Cut Corner - Destroyed", "Corner Long A - Destroyed", "Corner Long B - Destroyed", "Corner Long C - Destroyed", "Corner Long D - Destroyed", "Corner Large A - Destroyed", "Corner - Destroyed", "Ramp Bottom - Destroyed", "Ramp Top - Destroyed", "Slope - Destroyed", "Curved Corner - Destroyed", "Round Cut Corner - Destroyed", "Round Corner - Destroyed", "Round Corner Long - Destroyed", "Round Slope - Destroyed", "Cylinder - Destroyed", "Inward Corner - Destroyed", "Inward Round Slope - Destroyed", "Inward Curved Corner - Destroyed", "Round Slope Edge Inward - Destroyed", "Cylinder End A - Destroyed", "Cylinder End B - Destroyed", "Cylinder End C - Destroyed", "Ramp Wedge Top - Destroyed", "Round 4 Way Connector - Destroyed", "Round Slope Edge - Destroyed", "Corner Large B - Destroyed", "Corner Large C - Destroyed", "Corner Large D - Destroyed", "Corner Long E - Destroyed", "Pyramid A - Destroyed" }}},
            {(EpbBlockType)0x056c, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x056c, Name = "Steel Blocks L - Destroyed", VariantNames = new string[] {"Wall - Destroyed", "Wall L-shape - Destroyed", "Thin Slope - Destroyed", "Thin Corner - Destroyed", "Sloped Wall - Destroyed", "Sloped Wall Bottom (right) - Destroyed", "Sloped Wall Top (right) - Destroyed", "Sloped Wall Bottom (left) - Destroyed", "Sloped Wall Top (left) - Destroyed", "Round Corner Thin - Destroyed", "Round Slope Thin - Destroyed", "Round Cut Corner Thin - Destroyed", "Round Slope Edge Thin - Destroyed", "Round Corner Long Thin - Destroyed", "Corner Round Thin 2 - Destroyed", "Corner Thin 2 - Destroyed", "Wall 3 Corner - Destroyed", "Wall Half - Destroyed", "Cube Half - Destroyed", "Ramp Top Double - Destroyed", "Ramp Bottom A - Destroyed", "Ramp Bottom B - Destroyed", "Ramp Bottom C - Destroyed", "Ramp Wedge Bottom - Destroyed", "Beam - Destroyed", "Cylinder Thin - Destroyed", "Cylinder Thin T Joint - Destroyed", "Cylinder Thin Curved - Destroyed", "Cylinder Fence Bottom - Destroyed", "Cylinder Fence Top - Destroyed", "Slope Half - Destroyed" }}},

            {(EpbBlockType)0x0571, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0571, Name = "Concrete Blocks L - Destroyed", VariantNames = new string[] {"Cube - Destroyed", "Cut Corner - Destroyed", "Corner Long A - Destroyed", "Corner Long B - Destroyed", "Corner Long C - Destroyed", "Corner Long D - Destroyed", "Corner Large A - Destroyed", "Corner - Destroyed", "Ramp Bottom - Destroyed", "Ramp Top - Destroyed", "Slope - Destroyed", "Curved Corner - Destroyed", "Round Cut Corner - Destroyed", "Round Corner - Destroyed", "Round Corner Long - Destroyed", "Round Slope - Destroyed", "Cylinder - Destroyed", "Inward Corner - Destroyed", "Inward Round Slope - Destroyed", "Inward Curved Corner - Destroyed", "Round Slope Edge Inward - Destroyed", "Cylinder End A - Destroyed", "Cylinder End B - Destroyed", "Cylinder End C - Destroyed", "Ramp Wedge Top - Destroyed", "Round 4 Way Connector - Destroyed", "Round Slope Edge - Destroyed", "Corner Large B - Destroyed", "Corner Large C - Destroyed", "Corner Large D - Destroyed", "Corner Long E - Destroyed", "Pyramid A - Destroyed" }}},
            {(EpbBlockType)0x0572, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0572, Name = "Concrete Blocks L - Destroyed", VariantNames = new string[] {"Wall - Destroyed", "Wall L-shape - Destroyed", "Thin Slope - Destroyed", "Thin Corner - Destroyed", "Sloped Wall - Destroyed", "Sloped Wall Bottom (right) - Destroyed", "Sloped Wall Top (right) - Destroyed", "Sloped Wall Bottom (left) - Destroyed", "Sloped Wall Top (left) - Destroyed", "Round Corner Thin - Destroyed", "Round Slope Thin - Destroyed", "Round Cut Corner Thin - Destroyed", "Round Slope Edge Thin - Destroyed", "Round Corner Long Thin - Destroyed", "Corner Round Thin 2 - Destroyed", "Corner Thin 2 - Destroyed", "Wall 3 Corner - Destroyed", "Wall Half - Destroyed", "Cube Half - Destroyed", "Ramp Top Double - Destroyed", "Ramp Bottom A - Destroyed", "Ramp Bottom B - Destroyed", "Ramp Bottom C - Destroyed", "Ramp Wedge Bottom - Destroyed", "Beam - Destroyed", "Cylinder Thin - Destroyed", "Cylinder Thin T Joint - Destroyed", "Cylinder Thin Curved - Destroyed", "Cylinder Fence Bottom - Destroyed", "Cylinder Fence Top - Destroyed", "Slope Half - Destroyed" }}},

            {(EpbBlockType)0x0574, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0574, Name = "Xeno Steel Blocks",           VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x0575, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0575, Name = "Xeno Steel Blocks",           VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},
        };


        public static EpbBlockType GetBlockType(string name, string variantName)
        {
            EpbBlockTypeDefinition def = BlockTypeDefinitions.Values.FirstOrDefault(d => d.Name == name && Array.FindIndex(d.VariantNames, vName => vName == variantName) != -1);
            return def == null ? 0x0000 : def.Type;
        }
        public static string GetBlockTypeName(EpbBlockType type)
        {
            string s = "";
            if (BlockTypeDefinitions.ContainsKey(type))
            {
                s = $"\"{BlockTypeDefinitions[type].Name}\"";
            }
            else
            {
                s = $"{type.ToString()}";
            }

            return $"{s} (0x{(UInt16)type:x4}={(UInt16)type})";

        }

        public static byte GetVariant(EpbBlockType type, string variantName)
        {
            if (!BlockTypeDefinitions.ContainsKey(type))
            {
                return 0x00;
            }

            int i = Array.FindIndex(BlockTypeDefinitions[type].VariantNames, s => s == variantName);
            if (i == -1)
            {
                return 0x00;
            }

            return (byte)i;
        }

        public static string GetVariantName(EpbBlockType type, byte variant)
        {
            string s = "";
            if (BlockTypeDefinitions.ContainsKey(type) && variant < BlockTypeDefinitions[type].VariantNames.Length)
            {
                s = $"\"{BlockTypeDefinitions[type].VariantNames[variant]}\"";
            }
            return $"{s} (0x{variant:x2}={variant})";
        }


        public EpbBlockType BlockType { get; set; } // 11 bit
        public EpbBlockRotation Rotation { get; set; }
        public UInt16 Unknown00 { get; set; }
        public byte Variant { get; set; }
        public string VariantName
        {
            get => GetVariantName(BlockType, Variant);
            set => Variant = GetVariant(BlockType, value);
        }

        public EpbColour[] Colours = new EpbColour[6];     // 5 bit colour index
        public byte[] Textures = new byte[6];        // 6 bit texture index
        public bool[] TextureFlips = new bool[6];
        public byte   SymbolPage { get; set; }       // 2 bit page index
        public byte[] Symbols = new byte[6];         // 5 bit symbol index
        public SymbolRotation[] SymbolRotations = new SymbolRotation[6]; // 2 bit symbol rotation


        public void SetColour(EpbColour colour, FaceIndex face = FaceIndex.All)
        {
            if ((int)face < -1 || (int)face >= 6 || (byte)colour > 0x1f)
            {
                return;
            }

            if (face == FaceIndex.All)
            {
                for (int i = 0; i < 6; i++)
                {
                    Colours[i] = colour;
                }
            }
            else
            {
                Colours[(byte)face] = colour;
            }
        }
        public void SetTexture(byte texture, bool flip = false, FaceIndex face = FaceIndex.All)
        {
            if ((int)face < -1 || (int)face >= 6 || texture > 0x3f)
            {
                return;
            }

            if (face == FaceIndex.All)
            {
                for (int i = 0; i < 6; i++)
                {
                    Textures[i] = texture;
                    TextureFlips[i] = flip;
                }
            }
            else
            {
                Textures[(byte)face] = texture;
                TextureFlips[(byte)face] = flip;
            }
        }
        public void SetSymbol(byte symbol, SymbolRotation rotation = SymbolRotation.Up, FaceIndex face = FaceIndex.All)
        {
            if ((int)face < -1 || (int)face >= 6 || symbol > 0x1f)
            {
                return;
            }

            if (face == FaceIndex.All)
            {
                for (int i = 0; i < 6; i++)
                {
                    Symbols[i] = symbol;
                    SymbolRotations[i] = rotation;
                }
            }
            else
            {
                Symbols[(byte)face] = symbol;
                SymbolRotations[(byte)face] = rotation;
            }
        }

        public EpbBlock()
        {
            BlockType = GetBlockType("Steel Blocks L", "Cube");
            Variant = GetVariant(BlockType, "Cube");
            Rotation = EpbBlock.EpbBlockRotation.PzPy;
            Unknown00 = 0x0000;
        }

    }
}
