using System.Collections.Generic;
using ECFLib.Attributes;

namespace ECFLib
{
    public class TabGroupType : EcfObject
    {
        public List<TabGroupGridType> Grids = new List<TabGroupGridType>();

        #region AttributeShortcuts
        public string Icon
        {
            get => GetAttribute<AttributeString>("Icon")?.Value;
            set => ((AttributeString)Attributes["Icon"]).Value = value;
        }
        public string NameAttribute
        {
            get => GetAttribute<AttributeString>("Name")?.Value;
            set => ((AttributeString)Attributes["Name"]).Value = value;
        }
        #endregion AttributeShortcuts

        public TabGroupType(int id) : base(id, null, null)
        {
        }
    }
}
