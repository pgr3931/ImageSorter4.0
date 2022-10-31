using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace imageSorter4
{
    public class ImageFileInfo
    {
        public ImageFileInfo(StorageFile imageFile)
        {
            ImageFile = imageFile;
        }

        public StorageFile ImageFile { get; }

        public async Task<BitmapImage> GetImageSourceAsync()
        {
            using IRandomAccessStream fileStream = await ImageFile.OpenReadAsync();

            // Create a bitmap to be the image source.
            BitmapImage bitmapImage = new();
            bitmapImage.SetSource(fileStream);

            return bitmapImage;
        }
    }
}