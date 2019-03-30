using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using EPBLab.Behaviours;
using EPBLab.Messages;
using GalaSoft.MvvmLight;
using EPBLab.Model;
using GalaSoft.MvvmLight.Command;
using EPBLib;
using EPBLib.BlockData;
using EPBLib.Helpers;
using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        //private readonly IDataService _dataService;


        #region Properties

        public const string MainWindowTitlePropertyName = "MainWindowTitle";
        private string _mainWindowTitle = string.Empty;
        public string MainWindowTitle
        {
            get => _mainWindowTitle;
            set => Set(ref _mainWindowTitle, value);
        }

        public const string BlueprintsPropertyName = "Blueprints";
        private ObservableCollection<BlueprintViewModel> _blueprints = new ObservableCollection<BlueprintViewModel>();
        public ObservableCollection<BlueprintViewModel> Blueprints
        {
            get => _blueprints;
            set => Set (ref _blueprints, value);
        }

        public const string SelectedBlueprintIndexPropertyName = "SelectedBlueprintIndex";
        private int _selectedBlueprintIndex;
        public int SelectedBlueprintIndex
        {
            get => _selectedBlueprintIndex;
            set => Set(ref _selectedBlueprintIndex, value);
        }

        /*
        public const string BlueprintIsLoadedPropertyName = "BlueprintIsLoaded";
        private bool _blueprintIsLoaded = false;
        public bool BlueprintIsLoaded
        {
            get
            {
                return _blueprintIsLoaded;
            }
            set
            {
                Set(ref _blueprintIsLoaded, value);
                CommandSave.RaiseCanExecuteChanged();
            }
        }

        */

        #endregion Properties

        #region Commands

        #region Command_New

        public RelayCommand CommandNew
        {
            get { return _commandNew ?? (_commandNew = new RelayCommand(() => { NewBlueprint(); }));}
        }

        private RelayCommand _commandNew;

        #endregion Command_New

        #region Command_Save

        public RelayCommand CommandSave
        {
            get
            {
                return _commandSave ?? (_commandSave = new RelayCommand(() =>
                {
                    //if (!BlueprintIsLoaded)
                    //{
                    //    return;
                    //}
                    SaveBlueprint();
                }//,
//                () => BlueprintIsLoaded
                ));
            }
        }

        private RelayCommand _commandSave;

        #endregion Command_Save

        #endregion Commands


        protected void NewBlueprint()
        {
            EpBlueprint blueprint = new EpBlueprint(EpBlueprint.EpbType.Base, 64, 1, 4);
            EpbBlock block;

            byte x = 0;
            byte z = 0;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Cube", blueprint);
            block.SetColour(EpbColourIndex.Red);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Cut Corner", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Long A", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Long B", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Long C", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Long D", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Large A", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Ramp Bottom", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Ramp Top", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Slope", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Large B", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Large C", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Large D", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Corner Long E", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullFullLarge", "Pyramid A", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            x = 0;
            z += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Wall", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Wall L-shape", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Sloped Wall", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Sloped Wall Bottom (right)", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Sloped Wall Top (right)", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Sloped Wall Bottom (left)", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Sloped Wall Top (left)", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Wall 3 Corner", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Wall Half", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Cube Half", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Ramp Top Double", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Ramp Bottom A", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Ramp Bottom B", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Ramp Bottom C", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Ramp Wedge Bottom", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Beam", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            block = CreateBlock(x, 0, z, "HullThinLarge", "Slope Half", blueprint);
            block.SetColour(EpbColourIndex.Pink);
            x += 2;

            BlueprintViewModel vm = new BlueprintViewModel("New", blueprint);
            Blueprints.Add(vm);
            SelectedBlueprintIndex = Blueprints.Count - 1;
        }

        protected EpbBlock CreateBlock(byte x, byte y, byte z, string typeName, string variantName, EpBlueprint blueprint)
        {
            EpbBlock.EpbBlockType t = EpbBlock.GetBlockType(typeName, variantName);
            byte v = EpbBlock.GetVariant(t.Id, variantName);
            EpbBlock block = new EpbBlock(new EpbBlockPos() { X = x, Y = y, Z = z }) { BlockType = t, Variant = v };
            blueprint.SetBlock(block, x, y, z);
            return block;
        }

        protected void OpenBlueprints(FilesOpenedMessage m)
        {
            if (m.Identifier != "OpenBlueprints")
            {
                return;
            }

            foreach (string fileName in m.Content)
            {
                EpBlueprint blueprint = OpenEpb(fileName);
                BlueprintViewModel vm = new BlueprintViewModel(fileName, blueprint);
                Blueprints.Add(vm);
                SelectedBlueprintIndex = Blueprints.Count - 1;
            }
        }
        protected EpBlueprint OpenEpb(string path)
        {
            EpBlueprint blueprint = null;

            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                try
                {
                    long bytesLeft = reader.BaseStream.Length;
                    blueprint = reader.ReadEpBlueprint(ref bytesLeft);
                }
                catch (System.Exception ex)
                {
                    throw new Exception("Failed reading EPB file", ex);
                }
            }
            return blueprint;
        }

        protected void SaveBlueprint()
        {

        }


        protected void CloseBlueprint(CloseBlueprintMessage m)
        {
            if (Blueprints.Contains(m.Content))
            {
                Blueprints.Remove(m.Content);
            }
        }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            SetMainWindowTitle();
            /*
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    // Update local properties with information from the data item.
                    //WelcomeTitle = item.Title;
                });
            */
            Messenger.Default.Register<FilesOpenedMessage>(this, OpenBlueprints);
            Messenger.Default.Register<CloseBlueprintMessage>(this, CloseBlueprint);

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                //Messenger.Default.Send(new FilesOpenedMessage(new string[]{ "BA_Logic.epb" }) { Identifier = "OpenBlueprints" });
                EpBlueprint blueprint = OpenEpb("C:\\Users\\fredrik\\Documents\\Projects\\Empyrion\\EmpyrionStuff\\Research\\SampleData\\v22\\BA_Logic.epb");
                BlueprintViewModel vm = new BlueprintViewModel("BA_Logic.epb", blueprint);
                Blueprints.Add(vm);
                SelectedBlueprintIndex = Blueprints.Count - 1;
            }
            else
            {
            }
        }

        private void SetMainWindowTitle()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName name = assembly.GetName();
            System.Version version = name.Version;
            MainWindowTitle = $"{name.Name} v{version.Major}.{version.Minor}";
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}