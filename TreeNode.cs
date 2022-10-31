using System.Collections.ObjectModel;
using Windows.Storage;

namespace imageSorter4
{
    public class TreeNode
    {      
        public TreeNode(StorageFolder folder)
        {
            Folder = folder;
            Name = folder.DisplayName;
        }

        public StorageFolder Folder { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; } = new ObservableCollection<TreeNode>();
        public string Name { get; }
    }
}
