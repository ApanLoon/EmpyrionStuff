using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTagUInt32ViewModel : MetaTagViewModel
    {
        public override string Value
        {
            get => (Tag as MetaTagUInt32).Value.ToString();
            set
            {
                (Tag as MetaTagUInt32).Value = UInt32.Parse(value);
                RaisePropertyChanged();
            }
        }

        public MetaTagUInt32ViewModel(MetaTagUInt32 tag) : base(tag)
        {
        }
    }
}
