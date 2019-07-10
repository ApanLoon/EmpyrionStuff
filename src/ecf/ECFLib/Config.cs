using System;
using System.Collections.Generic;
using System.Linq;

namespace ECFLib
{
    public class Config
    {
        public int Version { get; set; }

        public List<BlockType>    BlockTypes    = new List<BlockType>();
        public List<ItemType>     ItemTypes     = new List<ItemType>();
        public List<EntityType>   EntityTypes   = new List<EntityType>();
        public List<TemplateType> TemplateTypes = new List<TemplateType>();
        public List<TabGroupType> TabGroupTypes = new List<TabGroupType>();

        public Config()
        {
            Version = -1;
        }

        public void ConnectReferences()
        {
            ConnectBlockTypeReferences();
            ConnectItemTypeReferences();
            ConnectEntityTypeReferences();
        }

        public void ConnectBlockTypeReferences()
        {
            foreach (BlockType blockType in BlockTypes)
            {
                if (blockType.RefName != null)
                {
                    BlockType refBlock = BlockTypes.FirstOrDefault(b => b.Name == blockType.RefName);
                    if (refBlock == null)
                    {
                        Console.WriteLine($"Warning: Ref \"{blockType.RefName}\" does not exist. (Block Id: {blockType.Id} Name: {blockType.Name})");
                    }
                    blockType.Ref = refBlock; // Store even if null
                }

                if (blockType.TemplateRootName != null)
                {
                    TemplateType templateRoot = TemplateTypes.FirstOrDefault(t => t.Name == blockType.TemplateRootName);
                    if (templateRoot == null)
                    {
                        Console.WriteLine($"Warning: TemplateRoot \"{blockType.TemplateRootName}\" does not exist. (Block Id: {blockType.Id} Name: {blockType.Name})");
                    }
                    blockType.TemplateRoot = templateRoot;
                }

                blockType.ChildBlocks.Clear();
                if (blockType.ChildBlockNames != null)
                {
                    foreach (string name in blockType.ChildBlockNames)
                    {
                        BlockType child = BlockTypes.FirstOrDefault(b => b.Name == name);
                        if (child == null)
                        {
                            Console.WriteLine($"Warning: ChildBlock \"{name}\" does not exist. (Block Id: {blockType.Id} Name: {blockType.Name})");
                        }
                        blockType.ChildBlocks.Add(child); // Insert even if null to maintain indices with name array
                    }
                }

                blockType.FuelAccept.Clear();
                if (blockType.FuelAcceptNames != null)
                {
                    foreach (string name in blockType.FuelAcceptNames)
                    {
                        ItemType item = ItemTypes.FirstOrDefault(i => i.Name == name);
                        if (item == null)
                        {
                            Console.WriteLine($"Warning: FuelAccept \"{name}\" does not exist. (Block Id: {blockType.Id} Name: {blockType.Name})");
                        }
                        blockType.FuelAccept.Add(item); // Insert even if null to maintain indices with name array
                    }
                }

                blockType.O2Accept.Clear();
                if (blockType.O2AcceptNames != null)
                {
                    foreach (string name in blockType.O2AcceptNames)
                    {
                        ItemType item = ItemTypes.FirstOrDefault(i => i.Name == name);
                        if (item == null)
                        {
                            Console.WriteLine($"Warning: O2Accept \"{name}\" does not exist. (Block Id: {blockType.Id} Name: {blockType.Name})");
                        }
                        blockType.O2Accept.Add(item); // Insert even if null to maintain indices with name array
                    }
                }

                if (blockType.WeaponItemName != null)
                {
                    ItemType item = ItemTypes.FirstOrDefault(i => i.Name == blockType.WeaponItemName);
                    if (item == null)
                    {
                        Console.WriteLine($"Warning: WeaponItem \"{blockType.WeaponItemName}\" does not exist. (Block Id: {blockType.Id}, Name: {blockType.Name})");
                    }
                    blockType.WeaponItem = item;
                }
            }
        }

        public void ConnectItemTypeReferences()
        {
            foreach (ItemType itemType in ItemTypes)
            {
                if (itemType.RefName != null)
                {
                    ItemType refItem = ItemTypes.FirstOrDefault(i => i.Name == itemType.RefName);
                    if (refItem == null)
                    {
                        Console.WriteLine(
                            $"Warning: Ref \"{itemType.RefName}\" does not exist. (Item Id: {itemType.Id}, Name: {itemType.Name})");
                    }
                    itemType.Ref = refItem;
                }
            }
        }

        public void ConnectEntityTypeReferences()
        {
            foreach (EntityType entityType in EntityTypes)
            {
                if (entityType.RefName != null)
                {
                    EntityType refEntity = EntityTypes.FirstOrDefault(i => i.Name == entityType.RefName);
                    if (refEntity == null)
                    {
                        Console.WriteLine(
                            $"Warning: Ref \"{entityType.RefName}\" does not exist. (Entity Name: {entityType.Name})");
                    }
                    entityType.Ref = refEntity;
                }
            }
        }
    }



}
