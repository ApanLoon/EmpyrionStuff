using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using ECFLab.ViewModel.Tree;
using ECFLib;
using GalaSoft.MvvmLight;

namespace ECFLab.ViewModel.Blocks
{
    public class BlocksViewModel : ViewModelBase
    {
        #region Properties
        public const string RootBlockTypesPropertyName = "RootBlockTypes";
        private ObservableCollection<ITreeNode> _rootBlockTypes = null;
        public ObservableCollection<ITreeNode> RootBlockTypes
        {
            get => _rootBlockTypes;
            set => Set(ref _rootBlockTypes, value);
        }

        public const string SelectedBlockTypesPropertyName = "SelectedBlockTypes";
        private ObservableCollection<ITreeNode> _selectedBlockTypes = null;
        public ObservableCollection<ITreeNode> SelectedBlockTypes
        {
            get => _selectedBlockTypes;
            set => Set(ref _selectedBlockTypes, value);
        }
        #endregion Properties

        private Config _config;
        private Dictionary<string, GroupNode> _categoryVms = new Dictionary<string, GroupNode>();
        private Dictionary<string, BlockViewModel> _blockVms = new Dictionary<string, BlockViewModel>();

        public BlocksViewModel(Config config)
        {
            _config = config;

            // Create VMs:
            foreach (BlockType blockType in _config.BlockTypes)
            {
                _blockVms[blockType.Name] = new BlockViewModel(blockType);
                if (blockType.Category != null && !_categoryVms.ContainsKey(blockType.Category))
                {
                    _categoryVms.Add(blockType.Category, new GroupNode(blockType.Category));
                }
            }

            // Organise child block types:
            foreach (BlockType blockType in _config.BlockTypes)
            {
                BlockViewModel parentVm = _blockVms[blockType.Name];
                foreach (BlockType childBlock in blockType.ChildBlocks)
                {
                    if (childBlock == null)
                    {
                        continue;
                    }

                    BlockViewModel childVm = _blockVms[childBlock.Name];
                    if (childVm != null)
                    {
                        parentVm.Children.Add(childVm);
                        childVm.Parent = parentVm;
                    }
                }
            }

            //// Organise Refs: TODO: This makes the same block type appear under multiple parents
            //foreach (BlockType blockType in _config.BlockTypes)
            //{
            //    if (string.IsNullOrEmpty(blockType.RefName) || !_blockVms.ContainsKey(blockType.RefName))
            //    {
            //        continue;
            //    }
            //    BlockViewModel vm = _blockVms[blockType.Name];
            //    BlockViewModel refVm = _blockVms[blockType.RefName];
            //    vm.Parent = refVm;
            //    refVm.Children.Add(vm);
            //}

            // Organise categories:
            ObservableCollection<ITreeNode> roots = new ObservableCollection<ITreeNode>(_categoryVms.Values);

            foreach (BlockViewModel blockVm in _blockVms.Values.Where(x=>x.Parent == null))
            {
                if (blockVm.BlockType.Category != null && _categoryVms.ContainsKey(blockVm.BlockType.Category))
                {
                    _categoryVms[blockVm.BlockType.Category].Children.Add(blockVm);
                }
                else
                {
                    roots.Add(blockVm);
                }
            }

            RootBlockTypes = roots;
        }
    }
}
