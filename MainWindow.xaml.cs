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
using Microsoft.VisualBasic.FileIO;
using Microsoft.UI;
using Windows.ApplicationModel;

namespace imageSorter4
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "ImageSorter 4.0";    
            InitializeTreeViewAsync();
            InitializeImagesAsync();
        }

        private ObservableCollection<TreeNode> FolderStructureFiltered { get; } = new();
        private TreeNode FolderStructureFull { get; set; }

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
            FolderStructureFiltered.Add(node);
            FolderStructureFull = CopyTree(node);
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

        public TreeNode CopyTree(TreeNode origin)
        {
            if (origin == null)
            {
                return null;
            }

            var result = new TreeNode(origin.Folder);
            result.Children = origin.Children != null ? origin.Children.Select(x => CopyTree(x)).ToList() : null;
            return result;
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
                if (Paging.Next())
                    SetImage(Images[Paging.Page - 1]);
            }
            else if (e.Key == Windows.System.VirtualKey.Left)
            {
                if (Paging.Prev())
                    SetImage(Images[Paging.Page - 1]);
            }
            else if (e.Key == Windows.System.VirtualKey.Delete)
            {
                if (Images.Any())
                {
                    var image = Images[Paging.Page - 1];
                    try
                    {
                        FileSystem.DeleteFile(image.ImageFile.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        Images.RemoveAt(Paging.Page - 1);
                        Paging.Size--;
                        SetImage(Images[Paging.Page - 1]);
                    }
                    catch (Exception ex)
                    {
                        var dialog = new ContentDialog
                        {

                            Title = "Error",
                            Content = ex.Message,
                            CloseButtonText = "Ok",
                            XamlRoot = this.Content.XamlRoot
                        };
                        dialog.ShowAsync();
                    }
                }
            }
        }

        private void TreeView_ItemInvoked(TreeView _, TreeViewItemInvokedEventArgs args)
        {
            if (Images.Any())
            {
                var node = args.InvokedItem as TreeNode;
                var image = Images[Paging.Page - 1];
                try
                {
                    File.Move(image.ImageFile.Path, Path.Combine(node.Folder.Path, image.ImageFile.Name));
                    Images.RemoveAt(Paging.Page - 1);
                    Paging.Size--;
                    SetImage(Images[Paging.Page - 1]);
                }
                catch (Exception ex)
                {
                    var dialog = new ContentDialog
                    {

                        Title = "Error",
                        Content = ex.Message,
                        CloseButtonText = "Ok",
                        XamlRoot = this.Content.XamlRoot
                    };
                    dialog.ShowAsync();
                }
            }
        }

        private void Filtered_NameChanged(object _, TextChangedEventArgs __)
        {
            FolderStructureFiltered.Clear();
            FolderStructureFiltered.Add(CopyTree(FolderStructureFull));

            if (!string.IsNullOrWhiteSpace(FilterByName.Text))
            {
                Filter(FolderStructureFiltered[0], FilterByName.Text);
            }
        }

        private bool Filter(TreeNode node, string search)
        {
            //remove children
            foreach (var child in node.Children.Where(c => !Filter(c, search)).ToArray())
                node.Children.Remove(child);
            //return if should be retained
            return node.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase) || node.Children.Count > 0;
        }

        private void RefreshButton_Click(object a, RoutedEventArgs b)
        {
            progress.Visibility = Visibility.Visible;
            FilterByName.TextChanged -= Filtered_NameChanged;
            FilterByName.Text = null;
            FilterByName.TextChanged += Filtered_NameChanged;
            FolderStructureFiltered.Clear();
            InitializeTreeViewAsync();
        }
    }
}
