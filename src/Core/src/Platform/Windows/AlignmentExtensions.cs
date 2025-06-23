using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Platform
{
	public static class AlignmentExtensions
	{
		public static HorizontalAlignment ToPlatformHorizontalAlignment(this TextAlignment alignment)
		{
			return alignment switch
			{
				TextAlignment.Center => HorizontalAlignment.Center,
				TextAlignment.End => HorizontalAlignment.Right,
				TextAlignment.Justify => HorizontalAlignment.Stretch,
				_ => HorizontalAlignment.Left,
			};
		}

		public static VerticalAlignment ToPlatformVerticalAlignment(this TextAlignment alignment)
		{
			return alignment switch
			{
				TextAlignment.Center => VerticalAlignment.Center,
				TextAlignment.End => VerticalAlignment.Bottom,
				_ => VerticalAlignment.Top,
			};
		}

		public static UI.Xaml.TextAlignment ToPlatform(this TextAlignment alignment, bool isLtr = true)
		{
			return alignment switch
			{
				TextAlignment.Center => UI.Xaml.TextAlignment.Center,
				TextAlignment.Justify => UI.Xaml.TextAlignment.Justify,
				TextAlignment.End => isLtr ? UI.Xaml.TextAlignment.Right : UI.Xaml.TextAlignment.Left,
				_ => isLtr ? UI.Xaml.TextAlignment.Left : UI.Xaml.TextAlignment.Right,
			};
		}
	}
}
