using System.Collections.Generic;

namespace ECFLib
{
    public class TabGroupType : EcfObject
    {
        public List<TabGroupGridType> Grids = new List<TabGroupGridType>();

        #region AttributeShortcuts
        #endregion AttributeShortcuts

        public TabGroupType(int id) : base(id, null, null)
        {
        }
    }
}
