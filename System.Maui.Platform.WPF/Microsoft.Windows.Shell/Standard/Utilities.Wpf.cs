// This file contains general utilities to aid in development.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal static partial class Utility
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static byte[] GetBytesFromBitmapSource(BitmapSource bmp)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * ((bmp.Format.BitsPerPixel + 7) / 8);

            var pixels = new byte[height * stride];

            bmp.CopyPixels(pixels, stride, 0);

            return pixels;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapSource GenerateBitmapSource(ImageSource img)
        {
            return GenerateBitmapSource(img, img.Width, img.Height);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapSource GenerateBitmapSource(ImageSource img, double renderWidth, double renderHeight)
        {
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawImage(img, new Rect(0, 0, renderWidth, renderHeight));
            }
            var bmp = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapSource GenerateBitmapSource(UIElement element, double renderWidth, double renderHeight, bool performLayout)
        {
            if (performLayout)
            {
                element.Measure(new Size(renderWidth, renderHeight));
                element.Arrange(new Rect(new Size(renderWidth, renderHeight)));
            }

            var bmp = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(element), null, new Rect(0, 0, renderWidth, renderHeight));
            }
            bmp.Render(dv);
            return bmp;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void SaveToPng(BitmapSource source, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using (FileStream stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        // This can be cached.  It's not going to change under reasonable circumstances.
        private static int s_bitDepth; // = 0;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _GetBitDepth()
        {
            if (s_bitDepth == 0)
            {
                using (SafeDC dc = SafeDC.GetDesktop())
                {
                    s_bitDepth = NativeMethods.GetDeviceCaps(dc, DeviceCap.BITSPIXEL) * NativeMethods.GetDeviceCaps(dc, DeviceCap.PLANES);
                }
            }
            return s_bitDepth;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static BitmapFrame GetBestMatch(IList<BitmapFrame> frames, int width, int height)
        {
            return _GetBestMatch(frames, _GetBitDepth(), width, height);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _MatchImage(BitmapFrame frame, int bitDepth, int width, int height, int bpp)
        {
            int score = 2 * _WeightedAbs(bpp, bitDepth, false) +
                    _WeightedAbs(frame.PixelWidth, width, true) +
                    _WeightedAbs(frame.PixelHeight, height, true);

            return score;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static int _WeightedAbs(int valueHave, int valueWant, bool fPunish)
        {
            int diff = (valueHave - valueWant);

            if (diff < 0)
            {
                diff = (fPunish ? -2 : -1) * diff;
            }

            return diff;
        }

        /// From a list of BitmapFrames find the one that best matches the requested dimensions.
        /// The methods used here are copied from Win32 sources.  We want to be consistent with
        /// system behaviors.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static BitmapFrame _GetBestMatch(IList<BitmapFrame> frames, int bitDepth, int width, int height)
        {
            int bestScore = int.MaxValue;
            int bestBpp = 0;
            int bestIndex = 0;

            bool isBitmapIconDecoder = frames[0].Decoder is IconBitmapDecoder;

            for (int i = 0; i < frames.Count && bestScore != 0; ++i)
            {
                int currentIconBitDepth = isBitmapIconDecoder ? frames[i].Thumbnail.Format.BitsPerPixel : frames[i].Format.BitsPerPixel;

                if (currentIconBitDepth == 0)
                {
                    currentIconBitDepth = 8;
                }

                int score = _MatchImage(frames[i], bitDepth, width, height, currentIconBitDepth);
                if (score < bestScore)
                {
                    bestIndex = i;
                    bestBpp = currentIconBitDepth;
                    bestScore = score;
                }
                else if (score == bestScore)
                {
                    // Tie breaker: choose the higher color depth.  If that fails, choose first one.
                    if (bestBpp < currentIconBitDepth)
                    {
                        bestIndex = i;
                        bestBpp = currentIconBitDepth;
                    }
                }
            }

            return frames[bestIndex];
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int RGB(Color c)
        {
            return c.B | (c.G << 8) | (c.R << 16);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static int AlphaRGB(Color c)
        {
            return c.B | (c.G << 8) | (c.R << 16) | (c.A << 24);
        }

        /// <summary>Convert a native integer that represent a color with an alpha channel into a Color struct.</summary>
        /// <param name="color">The integer that represents the color.  Its bits are of the format 0xAARRGGBB.</param>
        /// <returns>A Color representation of the parameter.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Color ColorFromArgbDword(uint color)
        {
            return Color.FromArgb(
                (byte)((color & 0xFF000000) >> 24),
                (byte)((color & 0x00FF0000) >> 16),
                (byte)((color & 0x0000FF00) >> 8),
                (byte)((color & 0x000000FF) >> 0));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool AreImageSourcesEqual(ImageSource left, ImageSource right)
        {
            if (null == left)
            {
                return right == null;
            }
            if (null == right)
            {
                return false;
            }

            BitmapSource leftBmp = GenerateBitmapSource(left);
            BitmapSource rightBmp = GenerateBitmapSource(right);

            byte[] leftPixels = GetBytesFromBitmapSource(leftBmp);
            byte[] rightPixels = GetBytesFromBitmapSource(rightBmp);

            if (leftPixels.Length != rightPixels.Length)
            {
                return false;
            }

            return MemCmp(leftPixels, rightPixels, leftPixels.Length);
        }

        // Caller is responsible for destroying the HICON
        // Caller is responsible to ensure that GDI+ has been initialized.
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IntPtr GenerateHICON(ImageSource image, Size dimensions)
        {
            if (image == null)
            {
                return IntPtr.Zero;
            }

            // If we're getting this from a ".ico" resource, then it comes through as a BitmapFrame.
            // We can use leverage this as a shortcut to get the right 16x16 representation
            // because DrawImage doesn't do that for us.
            var bf = image as BitmapFrame;
            if (bf != null)
            {
                bf = GetBestMatch(bf.Decoder.Frames, (int)dimensions.Width, (int)dimensions.Height);
            }
            else
            {
                // Constrain the dimensions based on the aspect ratio.
                var drawingDimensions = new Rect(0, 0, dimensions.Width, dimensions.Height);

                // There's no reason to assume that the requested image dimensions are square.
                double renderRatio = dimensions.Width / dimensions.Height;
                double aspectRatio = image.Width / image.Height;

                // If it's smaller than the requested size, then place it in the middle and pad the image.
                if (image.Width <= dimensions.Width && image.Height <= dimensions.Height)
                {
                    drawingDimensions = new Rect((dimensions.Width - image.Width) / 2, (dimensions.Height - image.Height) / 2, image.Width, image.Height);
                }
                else if (renderRatio > aspectRatio)
                {
                    double scaledRenderWidth = (image.Width / image.Height) * dimensions.Width;
                    drawingDimensions = new Rect((dimensions.Width - scaledRenderWidth) / 2, 0, scaledRenderWidth, dimensions.Height);
                }
                else if (renderRatio < aspectRatio)
                {
                    double scaledRenderHeight = (image.Height / image.Width) * dimensions.Height;
                    drawingDimensions = new Rect(0, (dimensions.Height - scaledRenderHeight) / 2, dimensions.Width, scaledRenderHeight);
                }

                var dv = new DrawingVisual();
                DrawingContext dc = dv.RenderOpen();
                dc.DrawImage(image, drawingDimensions);
                dc.Close();

                var bmp = new RenderTargetBitmap((int)dimensions.Width, (int)dimensions.Height, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(dv);
                bf = BitmapFrame.Create(bmp);
            }

            // Using GDI+ to convert to an HICON.
            // I'd rather not duplicate their code.
            using (MemoryStream memstm = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(bf);
                enc.Save(memstm);

                using (var istm = new ManagedIStream(memstm))
                {
                    // We are not bubbling out GDI+ errors when creating the native image fails.
                    IntPtr bitmap = IntPtr.Zero;
                    try
                    {
                        Status gpStatus = NativeMethods.GdipCreateBitmapFromStream(istm, out bitmap);
                        if (Status.Ok != gpStatus)
                        {
                            return IntPtr.Zero;
                        }

                        IntPtr hicon;
                        gpStatus = NativeMethods.GdipCreateHICONFromBitmap(bitmap, out hicon);
                        if (Status.Ok != gpStatus)
                        {
                            return IntPtr.Zero;
                        }

                        // Caller is responsible for freeing this.
                        return hicon;
                    }
                    finally
                    {
                        Utility.SafeDisposeImage(ref bitmap);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void AddDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
        {
            if (component == null)
            {
                return;
            }
            Assert.IsNotNull(property);
            Assert.IsNotNull(listener);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(property, component.GetType());
            dpd.AddValueChanged(component, listener);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void RemoveDependencyPropertyChangeListener(object component, DependencyProperty property, EventHandler listener)
        {
            if (component == null)
            {
                return;
            }
            Assert.IsNotNull(property);
            Assert.IsNotNull(listener);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(property, component.GetType());
            dpd.RemoveValueChanged(component, listener);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsNonNegative(this Thickness thickness)
        {
            if (!thickness.Top.IsFiniteAndNonNegative())
            {
                return false;
            }

            if (!thickness.Left.IsFiniteAndNonNegative())
            {
                return false;
            }

            if (!thickness.Bottom.IsFiniteAndNonNegative())
            {
                return false;
            }

            if (!thickness.Right.IsFiniteAndNonNegative())
            {
                return false;
            }

            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsValid(this CornerRadius cornerRadius)
        {
            if (!cornerRadius.TopLeft.IsFiniteAndNonNegative())
            {
                return false;
            }

            if (!cornerRadius.TopRight.IsFiniteAndNonNegative())
            {
                return false;
            }

            if (!cornerRadius.BottomLeft.IsFiniteAndNonNegative())
            {
                return false;
            }

            if (!cornerRadius.BottomRight.IsFiniteAndNonNegative())
            {
                return false;
            }

            return true;
        }
    }
}
