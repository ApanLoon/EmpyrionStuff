
using System;
using System.Linq;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTag03ViewModel : MetaTagViewModel
    {
        private MetaTag03 MyTag => Tag as MetaTag03;

        public override string Value
        {
            get => $"{BitConverter.ToString(MyTag.Value).Replace("-", "")}"; 
            set
            {
                int n = value.Length;
                if (n == 10)
                {
                    byte[] buf = Enumerable.Range(0, n)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(value.Substring(x, 2), 16))
                        .ToArray();
                    MyTag.Value = buf;
                }
                RaisePropertyChanged();
            }
        }

        public MetaTag03ViewModel(MetaTag03 tag) : base(tag)
        {
        }
    }
}
