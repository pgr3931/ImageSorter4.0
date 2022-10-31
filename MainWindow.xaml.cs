using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace imageSorter4
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeTreeViewAsync();
            InitializeImagesAsync();
        }

        private ObservableCollection<TreeNode> FolderStructure { get; } = new();

        public ObservableCollection<ImageFileInfo> Images { get; } = new();

        public Paging Paging { get; set; } = new();

        static readonly string[] mediaExtensions = {
            ".png", ".JPG", ".JPEG", ".BMP", ".GIF", ".TIF", ".TIFF", ".JFIF",
            ".AVI", ".MP4", ".DIVX", ".WMV",
        };

        static bool IsVideoFile(string path)
        {
            return Array.IndexOf(new string[] { ".AVI", ".MP4", ".DIVX", ".WMV" }, Path.GetExtension(path).ToUpperInvariant()) != -1;
        }

        private async Task InitializeImagesAsync()
        {
            var picturesFolder = await StorageFolder.GetFolderFromPathAsync("E:\\Bilder\\Anime\\ImageDump");
            var result = picturesFolder.CreateFileQueryWithOptions(new QueryOptions(CommonFileQuery.DefaultQuery, mediaExtensions));
            IReadOnlyList<StorageFile> imageFiles = await result.GetFilesAsync();

            foreach (StorageFile file in imageFiles)
            {
                Images.Add(new ImageFileInfo(file));
            }

            Paging.Size = imageFiles.Count;
            if (Images.Count != 0)
                await SetImage(Images.FirstOrDefault());
        }

        private async Task InitializeTreeViewAsync()
        {
            var rootFolder = await StorageFolder.GetFolderFromPathAsync("E:\\Bilder\\Anime\\Images");
            var node = new TreeNode(rootFolder);
            await FillTreeNode(node);
            FolderStructure.Add(node);
            progress.Visibility = Visibility.Collapsed;
        }

        private async Task FillTreeNode(TreeNode node)
        {
            IReadOnlyList<StorageFolder> subFolders = await node.Folder.GetFoldersAsync();
            foreach (var folder in subFolders)
            {
                var newNode = new TreeNode(folder);
                await FillTreeNode(newNode);
                node.Children.Add(newNode);
            }
        }

        private async Task SetImage(ImageFileInfo img)
        {
            if (IsVideoFile(img.ImageFile.Path))
            {
                video.Source = MediaSource.CreateFromUri(new Uri(img.ImageFile.Path));
                video.MediaPlayer.IsLoopingEnabled = true;
                video.Visibility = Visibility.Visible;
                image.Visibility = Visibility.Collapsed;
                image.Source = null;
            }
            else
            {
                image.Source = await img.GetImageSourceAsync();
                image.Visibility = Visibility.Visible;
                video.Visibility = Visibility.Collapsed;
                video.Source = null;              
            }
        }

        private void Grid_KeyDown(object _, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Right)
            {
                Paging.Next();
                SetImage(Images[Paging.Page - 1]);
            }
            else if (e.Key == Windows.System.VirtualKey.Left)
            {
                Paging.Prev();
                SetImage(Images[Paging.Page - 1]);
            }
            else if (e.Key == Windows.System.VirtualKey.Delete)
            {
                //Delete file
            }
        }

        private void TreeView_ItemInvoked(TreeView _, TreeViewItemInvokedEventArgs args)
        {
            var node = args.InvokedItem as TreeNode;

            // Move file
        }

        private void RefreshButton_Click(object a, RoutedEventArgs b)
        {
            progress.Visibility = Visibility.Visible;
            FolderStructure.Clear();
            InitializeTreeViewAsync();
        }
    }
}
