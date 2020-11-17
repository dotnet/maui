namespace System.Graphics
{
    public interface IFontService
    {
        IFontFamily[] GetFontFamilies();
        IFontStyle GetFontStyleById(string id);
        IFontStyle GetDefaultFontStyle();
    }
}