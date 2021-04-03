using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Graphics.Canvas;

namespace Microsoft.Maui.Graphics.Win2D
{
    public class W2DGraphicsService : IGraphicsService
    {
        public static W2DGraphicsService Instance = new W2DGraphicsService();

        private static ICanvasResourceCreator _globalCreator;
        private static readonly ThreadLocal<ICanvasResourceCreator> _threadLocalCreator = new ThreadLocal<ICanvasResourceCreator>();

        public static ICanvasResourceCreator GlobalCreator
        {
            get => _globalCreator;
            set => _globalCreator = value;
        }

        public static ICanvasResourceCreator ThreadLocalCreator
        {
            set => _threadLocalCreator.Value = value;
        }

        private static ICanvasResourceCreator Creator
        {
            get
            {
                var value = _threadLocalCreator.Value;
                if (value == null)
                    return _globalCreator;

                return value;
            }

        }

        public string SystemFontName => "Arial";
        public string BoldSystemFontName => "Arial-Bold";

        public List<PathF> ConvertToPaths(PathF aPath, string text, ITextAttributes textAttributes, float ppu, float zoom)
        {
            return new List<PathF>();
        }

        public SizeF GetStringSize(string value, string fontName, float textSize)
        {
            return new SizeF(value.Length * 10, textSize + 2);
        }

        public SizeF GetStringSize(string value, string fontName, float textSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            return new SizeF(value.Length * 10, textSize + 2);
        }

        public IImage LoadImageFromStream(Stream stream, ImageFormat format = ImageFormat.Png)
        {
            var creator = Creator;
            if (creator == null)
                throw new Exception("No resource creator has been registered globally or for this thread.");

            var bitmap = AsyncPump.Run(async () => await CanvasBitmap.LoadAsync(creator, stream.AsRandomAccessStream()));
            return new W2DImage(Creator, bitmap);
        }

        public BitmapExportContext CreateBitmapExportContext(int width, int height, float displayScale)
        {
            return null;
        }
    }
}
