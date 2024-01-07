using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Astralis.Logic
{
    internal class ImageManager
    {

        private static ImageManager instance;
        private readonly Dictionary<int, Bitmap> _imageMap;

        private ImageManager() 
        {
            _imageMap = new Dictionary<int, Bitmap>()
            {
                {1, ImageResource.Window},
                {2, ImageResource.Piranhas},
                {3, ImageResource.Pumpkin},
                {4, ImageResource.Falling},
                {5, ImageResource.Fox},
                {6, ImageResource.Eye },
                {7, ImageResource.Gangster},
                {8, ImageResource.Duck},
                {9, ImageResource.Mondongo},
                {10, ImageResource.DogeCoin},
                {11, ImageResource.Patrick}
            };
        }

        public static ImageManager Instance()
        {
            if (instance == null)
            {
                instance = new ImageManager();
            }

            return instance;
        }

        public int GetImageCount()
        {
            return _imageMap.Count;
        }

        public BitmapImage GetImage(int imageId)
        {
            Bitmap image = _imageMap[imageId];
            return ConvertBitmapToBitmapImage(image);
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
