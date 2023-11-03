using Astralis.UserManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Astralis.Logic
{
    internal class ImageManager
    {

        private static ImageManager instance;
        private Dictionary<int, Bitmap> imageMap;

        private ImageManager() 
        {
            imageMap = new Dictionary<int, Bitmap>()
            {
                {1, ImageResource.Window},
                {2, ImageResource.Piranhas},
                {3, ImageResource.Pumpkin},
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
            return imageMap.Count;
        }

        public BitmapImage GetImage(int imageId)
        {
            Bitmap image = imageMap[imageId];
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
