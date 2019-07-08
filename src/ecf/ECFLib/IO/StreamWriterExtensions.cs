
using System.IO;
using ECFLib.Attributes;

namespace ECFLib.IO
{
    public static class StreamWriterExtensions
    {
        public static void EcfWrite(this StreamWriter writer, Config config)
        {
            writer.Write($"VERSION: {config.Version}\r\n");

            foreach (BlockType blockType in config.BlockTypes)
            {
                writer.EcfWrite(blockType);
            }
            writer.Write("\r\n");
            foreach (ItemType itemType in config.ItemTypes)
            {
                writer.EcfWrite(itemType);
            }
        }

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
            writer.Write("}\r\n");
        }

        public static void EcfWrite(this StreamWriter writer, EcfAttribute attribute)
        {
            string v = attribute.ValueString();
            if (v.IndexOf(',') != -1)
            {
                v = $"\"{v}\"";
            }
            writer.Write($"  {attribute.Key}: {v}");
            if (attribute.AttributeType != null)
            {
                writer.Write($", type: {attribute.AttributeType}");
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
    }
}

