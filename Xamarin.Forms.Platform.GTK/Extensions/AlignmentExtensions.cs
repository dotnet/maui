namespace Xamarin.Forms.Platform.GTK.Extensions
{
    internal static class AlignmentExtensions
    {
        internal static float ToNativeValue(this TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Start:
                    return 0f;
                case TextAlignment.End:
                    return 1f;
                default:
                    return 0.5f;
            }
        }
    }
}