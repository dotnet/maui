namespace System.Graphics
{
    public interface IFontFamily
    {
        string Name { get; }
        IFontStyle[] GetFontStyles();
    }
}