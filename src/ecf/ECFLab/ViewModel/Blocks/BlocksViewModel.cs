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
        #endregion Properties

        private Config _config;
        private Dictionary<string, GroupNode> _categoryVms = new Dictionary<string, GroupNode>();
        private Dictionary<string, BlockViewModel> _blockVms = new Dictionary<string, BlockViewModel>();

        public BlocksViewModel(Config config)
        {
            _config = config;

            foreach (BlockType blockType in _config.BlockTypes)
            {
                _blockVms[blockType.Name] = new BlockViewModel(blockType);
                if (blockType.Category != null && !_categoryVms.ContainsKey(blockType.Category))
                {
                    _categoryVms.Add(blockType.Category, new GroupNode(blockType.Category));
                }
            }

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
