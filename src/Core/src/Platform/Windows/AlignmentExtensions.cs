using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public static class AlignmentExtensions
	{
		public static HorizontalAlignment ToNativeHorizontalAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return HorizontalAlignment.Center;
				case TextAlignment.End:
					return HorizontalAlignment.Right;
				default:
					return HorizontalAlignment.Left;
			}
		}
	}
}