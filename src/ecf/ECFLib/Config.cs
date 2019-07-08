using System.Collections.Generic;
using System.Linq;

namespace ECFLib
{
    public class Config
    {
        public string Path { get; set; }
        public int Version { get; set; }

        public List<BlockType> BlockTypes = new List<BlockType>();
        public List<ItemType> ItemTypes = new List<ItemType>();


        public Config()
        {
        }

        public void ConnectBlockTypeReferences()
        {
            foreach (BlockType block in BlockTypes)
            {
                if (block.RefName != "")
                {
                    block.Ref = BlockTypes.FirstOrDefault(b => b.Name == block.RefName);
                }
                //if (block.TemplateRootName != "")
                //{
                //    block.TemplateRoot = BlockTypes.FirstOrDefault(b => b.Name == block.TemplateRootName);
                //}
                //if (block.TechTreeParentName != "")
                //{
                //    block.TechTreeParent = BlockTypes.FirstOrDefault(b => b.TechTreeParentName == block.TechTreeParentName);
                //}
            }
        }
    }



}
