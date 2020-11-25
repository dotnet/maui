namespace System.Graphics
{
    public class VirtualFontService : AbstractFontService
    {
        private static readonly IFontFamily[] EmptyArray = { };

        public override IFontFamily[] GetFontFamilies()
        {
            return EmptyArray;
        }
    }
}