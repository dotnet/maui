using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace System.Graphics.GDI
{
    public class GDIGraphicsService : IGraphicsService
    {
        public static GDIGraphicsService Instance = new GDIGraphicsService();

        public string SystemFontName => "Arial";
        public string BoldSystemFontName => "Arial-Bold";
        
        public SizeF GetStringSize(string aString, string aFontName, float fontSize)
        {
            var fontEntry = GDIFontManager.GetMapping(aFontName);
            var font = new Font(fontEntry.Name, fontSize * .75f);
            var size = TextRenderer.MeasureText(aString, font);
            font.Dispose();
            return new SizeF(size.Width, size.Height);
        }


        public SizeF GetStringSize(string aString, string aFontName, float fontSize, HorizontalAlignment aHorizontalAlignment, VerticalAlignment aVerticalAlignment)
        {
            var fontEntry = GDIFontManager.GetMapping(aFontName);
            var font = new Font(fontEntry.Name, fontSize * .75f);
            var size = TextRenderer.MeasureText(aString, font);
            font.Dispose();
            return new SizeF(size.Width, size.Height);
        }

        public IImage LoadImageFromStream(Stream stream, ImageFormat format = ImageFormat.Png)
        {
            var bitmap = new Bitmap(stream);
            return new GDIImage(bitmap);
        }

        public BitmapExportContext CreateBitmapExportContext(int width, int height, float displayScale = 1)
        {
            return new GDIBitmapExportContext(width, height, displayScale);
        }
    }
}