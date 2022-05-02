using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Platform
{
	public static class AlignmentExtensions
	{
		public static HorizontalAlignment ToPlatformHorizontalAlignment(this TextAlignment alignment)
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

		public static VerticalAlignment ToPlatformVerticalAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return VerticalAlignment.Center;
				case TextAlignment.End:
					return VerticalAlignment.Bottom;
				default:
					return VerticalAlignment.Top;
			}
		}

		public static UI.Xaml.TextAlignment ToPlatform(this TextAlignment alignment, bool isLtr = true)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return UI.Xaml.TextAlignment.Center;
				case TextAlignment.End:
					if (isLtr)
						return UI.Xaml.TextAlignment.Right;
					else
						return UI.Xaml.TextAlignment.Left;
				default:
					if (isLtr)
						return UI.Xaml.TextAlignment.Left;
					else
						return UI.Xaml.TextAlignment.Right;
			}
		}
	}
}
