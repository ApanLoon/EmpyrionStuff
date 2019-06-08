
using EPBLib.BlockData;
using System;
using System.Collections.Generic;

namespace EPBLib
{
    public class EpbBlock
    {
        #region static

        public enum EpbBlockRotation
        {
            // FwdUp : P=Positive, N=Negative
            PzPy, PxPy, NzPy, NxPy, PzPx, PyPx, NzPx, NyPx, NzNy, NxNy, PzNy, PxNy, PzNx, PyNx, NzNx, NyNx, PyNz, PxNz, NyNz, NxNz, NxPz, NyPz, PxPz, PyPz
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


        #endregion static

        public EpbBlockPos Position { get; protected set; }

        public EpbBlockType BlockType { get; set; }
        public EpbBlockRotation Rotation { get; set; }
        public UInt16 Unknown00 { get; set; }
        public byte Variant { get; set; }
        public string VariantName
        {
            get => EpbBlockType.GetVariantName(BlockType.Id, Variant);
            set => Variant = EpbBlockType.GetVariant(BlockType.Id, value);
        }

        public UInt16 DamageState {get; set;}
        
        public EpbColourIndex[] Colours = new EpbColourIndex[6];     // 5 bit colour index
        public byte[] Textures = new byte[6];        // 6 bit texture index
        public bool[] TextureFlips = new bool[6];
        public byte   SymbolPage { get; set; }       // 2 bit page index
        public byte[] Symbols = new byte[6];         // 5 bit symbol index
        public SymbolRotation[] SymbolRotations = new SymbolRotation[6]; // 2 bit symbol rotation


        public void SetColour(EpbColourIndex colour, FaceIndex face = FaceIndex.All)
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


        public Dictionary<string, EpbBlockTag> Tags = new Dictionary<string, EpbBlockTag>();

        public void AddTag(EpbBlockTag tag)
        {
            Tags.Add(tag.Name, tag);
        }

        public EpbBlockTag GetTag(string name)
        {
            return Tags.ContainsKey(name) ? Tags[name] : null;
        }

        public EpbBlock(EpbBlockPos position)
        {
            Position = position;
            BlockType = EpbBlockType.GetBlockType("HullFullLarge", "Cube");
            Variant = EpbBlockType.GetVariant(BlockType.Id, "Cube");
            Rotation = EpbBlock.EpbBlockRotation.PzPy;
            Unknown00 = 0x0000;
        }
    }
}
