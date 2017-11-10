using Gdk;
using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class ImageControl : Gtk.HBox, IDesiredSizeProvider
    {
        private Gtk.Image _image;
        private Pixbuf _original;
        private ImageAspect _aspect;

        private Gdk.Rectangle _lastAllocation = Gdk.Rectangle.Zero;

        public ImageControl()
        {
            _aspect = ImageAspect.AspectFill;
            BuildImageControl();
        }

        public ImageAspect Aspect
        {
            get
            {
                return _aspect;
            }

            set
            {
                _aspect = value;
                QueueDraw();
            }
        }

        public Pixbuf Pixbuf
        {
            get
            {
                return _image.Pixbuf;
            }
            set
            {
                _lastAllocation = Gdk.Rectangle.Zero;
                _original = value;
                _image.Pixbuf = value;
            }
        }

        public void SetAlpha(double opacity)
        {
            if (_image != null && _original != null)
            {
                _image.Pixbuf = Pixbuf.AddAlpha(
                    true,
                    ((byte)(255 * opacity)),
                    ((byte)(255 * opacity)),
                    ((byte)(255 * opacity)));
            }
        }

        public Gdk.Size GetDesiredSize()
        {
            return _original != null
                ? new Gdk.Size(_original.Width, _original.Height)
                : Gdk.Size.Empty;
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            if (_image.Pixbuf != null && _lastAllocation != allocation)
            {
                _lastAllocation = allocation;
                UpdatePixBufWithAllocation(allocation);
            }
        }

        private void BuildImageControl()
        {
            CanFocus = true;

            _image = new Gtk.Image();

            PackStart(_image, true, true, 0);
        }

        private static Pixbuf GetAspectFitPixBuf(Pixbuf original, Gdk.Rectangle allocation)
        {
            var widthRatio = (float)allocation.Width / original.Width;
            var heigthRatio = (float)allocation.Height / original.Height;

            var fitRatio = Math.Min(widthRatio, heigthRatio);
            var finalWidth = (int)(original.Width * fitRatio);
            var finalHeight = (int)(original.Height * fitRatio);

            return original.ScaleSimple(finalWidth, finalHeight, InterpType.Bilinear);
        }

        private static Pixbuf GetAspectFillPixBuf(Pixbuf original, Gdk.Rectangle allocation)
        {
            var widthRatio = (float)allocation.Width / original.Width;
            var heigthRatio = (float)allocation.Height / original.Height;

            var fitRatio = Math.Max(widthRatio, heigthRatio);
            var finalWidth = (int)(original.Width * fitRatio);
            var finalHeight = (int)(original.Height * fitRatio);

            return original.ScaleSimple(finalWidth, finalHeight, InterpType.Bilinear);
        }

        private static Pixbuf GetFillPixBuf(Pixbuf original, Gdk.Rectangle allocation)
        {
            return original.ScaleSimple(allocation.Width, allocation.Height, InterpType.Bilinear);
        }

        private void UpdatePixBufWithAllocation(Gdk.Rectangle allocation)
        {
            var srcWidth = _original.Width;
            var srcHeight = _original.Height;

            Pixbuf newPixBuf = null;

            // Differents modes in which the image will be scaled to fit the display area.
            switch (Aspect)
            {
                case ImageAspect.AspectFit:
                    newPixBuf = GetAspectFitPixBuf(_original, allocation);
                    break;
                case ImageAspect.AspectFill:
                    newPixBuf = GetAspectFillPixBuf(_original, allocation);
                    break;
                case ImageAspect.Fill:
                    newPixBuf = GetFillPixBuf(_original, allocation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Aspect));
            }

            if (newPixBuf != null)
            {
                _image.Pixbuf = newPixBuf;
                newPixBuf.Dispose();    // Important: Image should adapt to window size. To maintain memory consuption, we make Pixbuf dispose (Unref is deprecated).
                System.GC.Collect();
            }
        }
    }

    public enum ImageAspect
    {
        AspectFit,
        AspectFill,
        Fill
    }
}