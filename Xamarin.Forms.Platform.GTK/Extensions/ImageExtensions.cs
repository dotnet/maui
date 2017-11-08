using Gdk;
using System;
using System.IO;

namespace Xamarin.Forms.Platform.GTK.Extensions
{
    public static class ImageExtensions
    {
        public static Pixbuf ToPixbuf(this ImageSource imagesource)
        {
            return ToPixbufAux(imagesource, null);
        }

        public static Pixbuf ToPixbuf(this ImageSource imagesource, Size size)
        {
            return ToPixbufAux(imagesource, size);
        }

        private static Pixbuf ToPixbufAux(this ImageSource imagesource, Size? size)
        {
            try
            {
                Pixbuf image = null;

                var filesource = imagesource as FileImageSource;

                if (filesource != null)
                {
                    var file = filesource.File;

                    if (!string.IsNullOrEmpty(file))
                    {
                        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);

                        image = size.HasValue
                            ? new Pixbuf(imagePath, (int)size.Value.Width, (int)size.Value.Height)
                            : new Pixbuf(imagePath);
                    }
                }

                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}