using AppKit;

namespace Microsoft.Maui.Graphics.Native
{
    public static class CoreGraphicsExtensions
    {
        public static Paint AsPaint(this NSImage target)
        {
            if (target == null)
                return null;

            var image = new NativeImage(target);
            var paint = new Paint {Image = image};
            return paint;
        }

        public static NSColor AsNSColor(this Color color)
        {
            return NSColor.FromDeviceRgba(color.Red, color.Green, color.Blue, color.Alpha);
        }
    }
}