using System;
using System.Drawing;
using Windows.Foundation;

namespace Microsoft.Maui.Essentials
{
    public static class RectangleExtensions
    {
        public static Rectangle ToSystemRectangle(this Rect rect)
        {
            if (rect.X > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(rect.X));

            if (rect.Y > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(rect.Y));

            if (rect.Width > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(rect.Width));

            if (rect.Height > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(rect.Height));

            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        public static RectangleF ToSystemRectangleF(this Rect rect) =>
            new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

        public static Rect ToPlatformRectangle(this Rectangle rect) =>
            new Rect(rect.X, rect.Y, rect.Width, rect.Height);

        public static Rect ToPlatformRectangle(this RectangleF rect) =>
            new Rect(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
