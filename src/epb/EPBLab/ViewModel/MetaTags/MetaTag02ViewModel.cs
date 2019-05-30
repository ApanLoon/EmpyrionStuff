using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTag02ViewModel : MetaTagViewModel
    {
        public override string Value
        {
            get => (Tag as EpMetaTag02).Value.ToString();
            set
            {
                (Tag as EpMetaTag02).Value = UInt32.Parse(value);
                RaisePropertyChanged();
            }
        }

        public MetaTag02ViewModel(EpMetaTag02 tag) : base(tag)
        {
        }
    }
}
