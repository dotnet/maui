using Pango;

namespace Xamarin.Forms.Platform.GTK.Helpers
{
    internal static class FontDescriptionHelper
    {
        internal static FontDescription CreateFontDescription(double fontSize, string fontFamily, FontAttributes attributes)
        {
            FontDescription fontDescription = new FontDescription();
            fontDescription.Size = (int)(fontSize * Scale.PangoScale);
            fontDescription.Family = fontFamily;
            fontDescription.Weight = attributes == FontAttributes.Bold ? Weight.Bold : Weight.Normal;
            fontDescription.Style = attributes == FontAttributes.Italic ? Pango.Style.Italic : Pango.Style.Normal;

            return fontDescription;
        }
    }
}