using System;
using System.Windows;

namespace Xamarin.Forms.Platform.WPF
{
	internal static class AlignmentExtensions
	{
		internal static System.Windows.TextAlignment ToNativeTextAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return System.Windows.TextAlignment.Center;
				case TextAlignment.End:
					return System.Windows.TextAlignment.Right;
				default:
					return System.Windows.TextAlignment.Left;
			}
		}

		internal static VerticalAlignment ToNativeVerticalAlignment(this LayoutOptions alignment)
		{
			switch (alignment.Alignment)
			{
				case LayoutAlignment.Start:
					return VerticalAlignment.Top;
				case LayoutAlignment.Center:
					return VerticalAlignment.Center;
				case LayoutAlignment.End:
					return VerticalAlignment.Bottom;
				case LayoutAlignment.Fill:
					return VerticalAlignment.Stretch;
				default:
					return VerticalAlignment.Stretch;
			}
		}

		internal static HorizontalAlignment ToNativeHorizontalAlignment(this LayoutOptions alignment)
		{
			switch (alignment.Alignment)
			{
				case LayoutAlignment.Start:
					return HorizontalAlignment.Left;
				case LayoutAlignment.Center:
					return HorizontalAlignment.Center;
				case LayoutAlignment.End:
					return HorizontalAlignment.Right;
				case LayoutAlignment.Fill:
					return HorizontalAlignment.Stretch;
				default:
					return HorizontalAlignment.Stretch;
			}
		}
	}
}
