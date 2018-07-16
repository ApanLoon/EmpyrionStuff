
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
        //    // Devices:
        //    CvCockpit     = 0x0101,
        //    CvFuelTankT1  = 0x0103,
        //    VesselCore    = 0x022e,
        //    BaseCore      = 0x0a2e,

        //    // Building Blocks:
        //    WoodBlocks_A           = 0x018d, // Variants 0x00-0x1f
        //    WoodBlocks_B           = 0x018e, // Variants 0x00-0x1e
        //    ConcreteBlocks_A       = 0x0190, // Variants 0x00-0x1f
        //    ConcreteBlocks_B       = 0x0191, // Variants 0x00-0x1e
        //    SteelBlocksL_A         = 0x0193, // Variants 0x00-0x1f
        //    SteelBlocksL_B         = 0x0194, // Variants 0x00-0x1e
        //    HardenedSteelBlocksL_A = 0x0196, // Variants 0x00-0x1f
        //    HardenedSteelBlocksL_B = 0x0197, // Variants 0x00-0x1e
        //    AlienBuildingBlocks_A  = 0x0199, // Variants 0x00-0x1f
        //    AlienBuildingBlocks_B  = 0x019a, // Variants 0x00-0x1e
        //    CombatSteelBlocksL_A   = 0x019c, // Variants 0x00-0x1f
        //    CombatSteelBlocksL_B   = 0x019d, // Variants 0x00-0x1e
        //    GrowingPlotSteel       = 0x01ce, // Variants 0x00
        //    ShutterWindowBlocks    = 0x03c9, // Variants 0x00-0x04
        //    WalkwayRailingBlocks   = 0x0374, // Variants 0x00-0x0e
        //    StairsBlocks           = 0x02a0, // Variants 0x00-0x06
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

            {(EpbBlockType)0x0193, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0193, Name = "Steel Blocks L",           VariantNames = new string[] {"Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C", "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top", "Slope", "Curved Corner", "Round Cut Corner", "Round Corner", "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner", "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward", "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top", "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C", "Corner Large D", "Corner Long E", "Pyramid A" }}},
            {(EpbBlockType)0x0194, new EpbBlockTypeDefinition() { Type = (EpbBlockType)0x0194, Name = "Steel Blocks L",           VariantNames = new string[] {"Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall", "Sloped Wall Bottom (right)", "Sloped Wall Top (right)", "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin", "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin", "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2", "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A", "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin", "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom", "Cylinder Fence Top", "Slope Half" }}},

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
