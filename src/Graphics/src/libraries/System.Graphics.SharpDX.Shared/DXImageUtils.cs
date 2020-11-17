using System.IO;
using SharpDX.WIC;

namespace System.Graphics.SharpDX
{
    public static class DXImageUtils
    {
#if !NETFX_CORE
        /*public static Bitmap LoadFromStream(Stream stream)
        {
            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(stream, true, true))
            {
                return new Bitmap(DXGraphicsService.FactoryImaging, bitmap, BitmapAlphaChannelOption.UsePremultipliedAlpha);
            }
        }*/
#endif

        public static void WriteJpegToStream(Bitmap bitmap, Stream stream)
        {
            int width = bitmap.Size.Width;
            int height = bitmap.Size.Height;

            // ------------------------------------------------------
            // Encode a JPEG image
            // ------------------------------------------------------

            // Create a WIC outputstream 
            var wicStream = new WICStream(DXGraphicsService.FactoryImaging, stream);

            // Initialize a Jpeg encoder with this stream
            var encoder = new JpegBitmapEncoder(DXGraphicsService.FactoryImaging);
            encoder.Initialize(wicStream);

            // Create a Frame encoder
            var bitmapFrameEncode = new BitmapFrameEncode(encoder);
            bitmapFrameEncode.Options.CompressionQuality = .8f;
            bitmapFrameEncode.Initialize();
            bitmapFrameEncode.SetSize(width, height);
            Guid guid = PixelFormat.Format24bppRGB;
            bitmapFrameEncode.SetPixelFormat(ref guid);

            bitmapFrameEncode.WriteSource(bitmap);

            // Commit changes
            bitmapFrameEncode.Commit();
            encoder.Commit();

            // Cleanup
            bitmapFrameEncode.Options.Dispose();
            bitmapFrameEncode.Dispose();
            encoder.Dispose();
            wicStream.Dispose();
        }

        public static void WritePngToStream(Bitmap bitmap, Stream stream)
        {
            int width = bitmap.Size.Width;
            int height = bitmap.Size.Height;

            // ------------------------------------------------------
            // Encode a PNG image
            // ------------------------------------------------------

            // Create a WIC outputstream 
            var wicStream = new WICStream(DXGraphicsService.FactoryImaging, stream);

            // Initialize a Jpeg encoder with this stream
            var encoder = new PngBitmapEncoder(DXGraphicsService.FactoryImaging);
            encoder.Initialize(wicStream);

            // Create a Frame encoder
            var bitmapFrameEncode = new BitmapFrameEncode(encoder);
            bitmapFrameEncode.Initialize();
            bitmapFrameEncode.SetSize(width, height);
            Guid guid = PixelFormat.Format32bppRGBA;
            bitmapFrameEncode.SetPixelFormat(ref guid);

            bitmapFrameEncode.WriteSource(bitmap);

            // Commit changes
            bitmapFrameEncode.Commit();
            encoder.Commit();

            // Cleanup
            bitmapFrameEncode.Options.Dispose();
            bitmapFrameEncode.Dispose();
            encoder.Dispose();
            wicStream.Dispose();
        }

        public static Bitmap LoadPngFromStream(Stream stream)
        {
            // Read input
            var wicStream = new WICStream(DXGraphicsService.FactoryImaging, stream);
            var decoder = new PngBitmapDecoder(DXGraphicsService.FactoryImaging);
            decoder.Initialize(wicStream, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode bitmapFrameDecode = decoder.GetFrame(0);

            int width = bitmapFrameDecode.Size.Width;
            int height = bitmapFrameDecode.Size.Height;

            //Convert WIC pixel format to D2D1 format
            var formatConverter = new FormatConverter(DXGraphicsService.FactoryImaging);
            formatConverter.Initialize(bitmapFrameDecode, PixelFormat.Format32bppBGRA, BitmapDitherType.None, null, 0f,
                BitmapPaletteType.MedianCut);

            // Bitmaps and render target settings
            var wicBitmap = new Bitmap(
                DXGraphicsService.FactoryImaging, width, height,
                PixelFormat.Format32bppBGRA,
                BitmapCreateCacheOption.CacheOnLoad);

            bitmapFrameDecode.Dispose();
            decoder.Dispose();
            wicStream.Dispose();

            return wicBitmap;
        }
    }
}