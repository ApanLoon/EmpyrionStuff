
using System.Collections.Generic;
using System.IO;
using ECFLib.Attributes;

namespace ECFLib.IO
{
    public static class StreamWriterExtensions
    {
        public static void EcfWrite(this StreamWriter writer, Config config)
        {
            if (config.Version != -1)
            {
                writer.Write($"VERSION: {config.Version}\r\n");
            }

            foreach (BlockType blockType in config.BlockTypes)
            {
                writer.EcfWrite(blockType);
            }

            if (config.ItemTypes.Count > 0)
            {
                writer.Write("\r\n");
                foreach (ItemType itemType in config.ItemTypes)
                {
                    writer.EcfWrite(itemType);
                }
            }

            if (config.EntityTypes.Count > 0)
            {
                writer.Write("\r\n");
                foreach (EntityType entityType in config.EntityTypes)
                {
                    writer.EcfWrite(entityType);
                }
            }

            if (config.TemplateTypes.Count > 0)
            {
                writer.Write("\r\n");
                foreach (TemplateType templateType in config.TemplateTypes)
                {
                    writer.EcfWrite(templateType);
                }
            }

            if (config.TabGroupTypes.Count > 0)
            {
                writer.Write("\r\n");
                foreach (TabGroupType tabGroupType in config.TabGroupTypes)
                {
                    writer.EcfWrite(tabGroupType);
                }
            }

            writer.Flush();
        }

        #region Config
        public static void EcfWrite(this StreamWriter writer, BlockType blockType)
        {
            writer.Write($"{{ Block Id: {blockType.Id}, Name: {blockType.Name}");
            if (blockType.RefName != null)
            {
                writer.Write($", Ref: {blockType.RefName}");
            }
            writer.Write("\r\n");
            foreach (EcfAttribute attribute in blockType.Attributes.Values)
            {
                writer.EcfWrite(attribute);
            }
            writer.Write("}\r\n");
        }

        public static void EcfWrite(this StreamWriter writer, ItemType itemType)
        {
            writer.Write($"{{ Item Id: {itemType.Id}, Name: {itemType.Name}");
            if (itemType.RefName != null)
            {
                writer.Write($", Ref: {itemType.RefName}");
            }
            writer.Write("\r\n");
            foreach (EcfAttribute attribute in itemType.Attributes.Values)
            {
                writer.EcfWrite(attribute);
            }

            foreach (EcfObject o in itemType.OperationModes)
            {
                writer.Write("  {\r\n");
                foreach (EcfAttribute attribute in o.Attributes.Values)
                {
                    writer.Write("  ");
                    writer.EcfWrite(attribute);
                }
                writer.Write("  }\r\n");
            }
            writer.Write("}\r\n");
        }

        public static void EcfWrite(this StreamWriter writer, EntityType entityType)
        {
            writer.Write($"{{ Entity Name: {entityType.Name}");
            if (entityType.RefName != null)
            {
                writer.Write($", Ref: {entityType.RefName}");
            }
            writer.Write("\r\n");
            foreach (EcfAttribute attribute in entityType.Attributes.Values)
            {
                writer.EcfWrite(attribute);
            }
            writer.Write("}\r\n");
        }

        public static void EcfWrite(this StreamWriter writer, TemplateType templateType)
        {
            writer.Write($"{{ Template Name: {templateType.Name}\r\n");
            foreach (EcfAttribute attribute in templateType.Attributes.Values)
            {
                writer.EcfWrite(attribute);
            }

            writer.Write("  { Child Inputs\r\n");
            foreach (KeyValuePair<string, int> input in templateType.Inputs)
            {
                writer.Write($"    {input.Key}: {input.Value}\r\n");
            }
            writer.Write("  }\r\n");
            writer.Write("}\r\n");
        }
        #endregion Config

        #region BlockShapesWindow
        public static void EcfWrite(this StreamWriter writer, TabGroupType tabGroupType)
        {
            writer.Write($"{{ TabGroup Id: {tabGroupType.Id}\r\n");
            foreach (EcfAttribute attribute in tabGroupType.Attributes.Values)
            {
                writer.EcfWrite(attribute);
            }

            foreach (TabGroupGridType o in tabGroupType.Grids)
            {
                writer.Write($"  {{ Child {o.Name}\r\n");
                foreach (EcfAttribute attribute in o.Attributes.Values)
                {
                    writer.Write("  ");
                    writer.EcfWrite(attribute);
                }
                writer.Write("  }\r\n");
            }
            writer.Write("}\r\n");
        }
        #endregion BlockShapesWindow

        #region Generic
        public static void EcfWrite(this StreamWriter writer, EcfAttribute attribute)
        {
            string v = attribute.ValueString();
            writer.Write($"  {attribute.Key}: {v}");
            if (attribute.AttributeType != null)
            {
                writer.Write($", type: {attribute.AttributeType}");
            }

            if (attribute.Data != null)
            {
                writer.Write($", data: {attribute.Data}");
            }

            if (attribute.Display != null)
            {
                writer.Write($", display: {attribute.Display}");
            }

            if (attribute.Formatter != null)
            {
                writer.Write($", formatter: {attribute.Formatter}");
            }
            writer.Write("\r\n");
        }
        #endregion Generic
    }
}

