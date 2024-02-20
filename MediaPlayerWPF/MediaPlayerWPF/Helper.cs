using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MediaPlayerWPF
{
    public  static class Helper
    {
        public static BitmapImage MyToImage(byte[] array)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(array))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        //public BitmapImage ToImageLocal()
        //{
        //    byte[] imgdata = System.IO.File.ReadAllBytes("7years.png");
        //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imgdata))
        //    {
        //        BitmapImage image = new BitmapImage();
        //        image.BeginInit();
        //        image.StreamSource = ms;
        //        image.EndInit();
        //        return image;
        //    }
        //}

        public static System.Drawing.Image MyByteArrayToImage(byte[] byteArrayIn)
        {
            System.Drawing.Image returnImage = null;
            try
            {
                MemoryStream ms = new MemoryStream(byteArrayIn, 0, byteArrayIn.Length);
                ms.Write(byteArrayIn, 0, byteArrayIn.Length);
                returnImage = System.Drawing.Image.FromStream(ms, true);//Exception occurs here
            }
            catch { }
            return returnImage;
        }

        public static ImageSource MyToImageSource(this System.Drawing.Image image)
        {
            var bitmap = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            return bitmap;
        }
    }
}

