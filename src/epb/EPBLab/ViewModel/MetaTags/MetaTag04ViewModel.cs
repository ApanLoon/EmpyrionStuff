
using System;
using System.Linq;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTag04ViewModel : MetaTagViewModel
    {
        private MetaTag04 MyTag => Tag as MetaTag04;

        public override string Value
        {
            get => $"{BitConverter.ToString(MyTag.Value).Replace("-", "")}";
            set
            {
                int n = value.Length;
                if (n == 26)
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

        public MetaTag04ViewModel(MetaTag04 tag) : base(tag)
        {
        }
    }
}
