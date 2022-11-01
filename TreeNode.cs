using System.Collections.Generic;
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
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
        public string Name { get; }
    }
}
