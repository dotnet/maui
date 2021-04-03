using System.IO;

namespace Microsoft.Maui.Graphics
{
    public delegate void LayoutLine(PointF aPoint, ITextAttributes aTextual, string aText, float aAscent, float aDescent, float aLeading);

    public interface IGraphicsService
    {
        string SystemFontName { get; }
        string BoldSystemFontName { get; }
        
        SizeF GetStringSize(string value, string fontName, float textSize);
        SizeF GetStringSize(string value, string fontName, float textSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment);
        
        IImage LoadImageFromStream(Stream stream, ImageFormat format = ImageFormat.Png);
        BitmapExportContext CreateBitmapExportContext(int width, int height, float displayScale = 1);
    }
}
