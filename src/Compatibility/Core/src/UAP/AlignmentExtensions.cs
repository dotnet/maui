using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal static class AlignmentExtensions
	{
		internal static Microsoft.UI.Xaml.TextAlignment ToNativeTextAlignment(this TextAlignment alignment, EffectiveFlowDirection flowDirection = default(EffectiveFlowDirection))
		{
			var isLtr = flowDirection.IsLeftToRight();
			switch (alignment)
			{
				case TextAlignment.Center:
					return Microsoft.UI.Xaml.TextAlignment.Center;
				case TextAlignment.End:
					if (isLtr)
						return Microsoft.UI.Xaml.TextAlignment.Right;
					else
						return Microsoft.UI.Xaml.TextAlignment.Left;
				default:
					if (isLtr)
						return Microsoft.UI.Xaml.TextAlignment.Left;
					else
						return Microsoft.UI.Xaml.TextAlignment.Right;
			}
		}

		internal static VerticalAlignment ToNativeVerticalAlignment(this TextAlignment alignment)
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

		internal static HorizontalAlignment ToNativeHorizontalAlignment(this TextAlignment alignment)
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