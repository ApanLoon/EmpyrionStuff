using System;
using System.IO;
using GalaSoft.MvvmLight;
using ECFLab.Model;
using ECFLab.ViewModel.Blocks;
using ECFLab.ViewModel.Entities;
using ECFLab.ViewModel.Items;
using ECFLab.ViewModel.Templates;
using ECFLib;
using ECFLib.IO;

namespace ECFLab.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        #region Properties
        #region Window
        public const string MainWindowTitlePropertyName = "MainWindowTitle";
        private string _mainWindowTitle = string.Empty;
        public string MainWindowTitle
        {
            get => _mainWindowTitle;
            set => Set(ref _mainWindowTitle, value);
        }
        #endregion Window
        #region Tabs
        public const string BlocksPropertyName = "Blocks";
        private BlocksViewModel _blocks = null;
        public BlocksViewModel Blocks
        {
            get => _blocks;
            set => Set(ref _blocks, value);
        }

        public const string ItemsPropertyName = "Items";
        private ItemsViewModel _items = null;
        public ItemsViewModel Items
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public const string EntitiesPropertyName = "Entities";
        private EntitiesViewModel _entities = null;
        public EntitiesViewModel Entities
        {
            get => _entities;
            set => Set(ref _entities, value);
        }

        public const string TemplatesPropertyName = "Templates";
        private TemplatesViewModel _templates = null;
        public TemplatesViewModel Templates
        {
            get => _templates;
            set => Set(ref _templates, value);
        }
        #endregion Tabs
        #endregion Properties


        public MainViewModel(IDataService dataService)
        {
            SetMainWindowTitle();
            Config = OpenECF("G:\\Steam\\steamapps\\common\\Empyrion - Galactic Survival\\Content\\Configuration\\Config_Example.ecf");
            Blocks = new BlocksViewModel(Config);
            Items = new ItemsViewModel(Config);
            Entities = new EntitiesViewModel(Config);
            Templates = new TemplatesViewModel(Config);
        }

        protected Config Config;

        private static Config OpenECF(string path)
        {
            using (StreamReader reader = new StreamReader(File.OpenRead(path)))
            {
                try
                {
                    return reader.ReadEcf();
                }
                catch (System.Exception ex)
                {
                    throw new Exception("Failed reading ECF file", ex);
                }
            }
        }


        private void SetMainWindowTitle()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName name = assembly.GetName();
            System.Version version = name.Version;
            MainWindowTitle = $"{name.Name} v{version.Major}.{version.Minor}";
        }

    }
}