using Microsoft.UI.Xaml.Controls;
using WScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;

namespace Microsoft.Maui.Platform
{
	public static class ScrollViewerExtensions
	{
		public static WScrollBarVisibility ToWindowsScrollBarVisibility(this ScrollBarVisibility visibility)
		{
			return visibility switch
			{
				ScrollBarVisibility.Always => WScrollBarVisibility.Visible,
				ScrollBarVisibility.Default => WScrollBarVisibility.Auto,
				ScrollBarVisibility.Never => WScrollBarVisibility.Hidden,
				_ => WScrollBarVisibility.Auto,
			};
		}

		public static void UpdateScrollBarVisibility(this ScrollViewer scrollViewer, ScrollOrientation orientation, ScrollBarVisibility visibility)
		{
			if (orientation == ScrollOrientation.Neither)
			{
				scrollViewer.HorizontalScrollBarVisibility = scrollViewer.VerticalScrollBarVisibility = WScrollBarVisibility.Disabled;
				return;
			}

			if (visibility == ScrollBarVisibility.Default)
			{
				// If the user has not explicitly set a horizontal scroll bar visibility, then the orientation will
				// determine what the horizontal scroll bar does
				scrollViewer.HorizontalScrollBarVisibility = orientation switch
				{
					ScrollOrientation.Horizontal or ScrollOrientation.Both => WScrollBarVisibility.Auto,
					_ => WScrollBarVisibility.Disabled,
				};

				scrollViewer.VerticalScrollBarVisibility = orientation switch
				{
					ScrollOrientation.Vertical or ScrollOrientation.Both => WScrollBarVisibility.Auto,
					_ => WScrollBarVisibility.Disabled,
				};
			}
			else
			{
				// If the user _has_ set a horizontal scroll bar visibility preference, then convert that preference to the native equivalent
				// if the orientation allows for it
				scrollViewer.HorizontalScrollBarVisibility = orientation switch
				{
					ScrollOrientation.Horizontal or ScrollOrientation.Both => visibility.ToWindowsScrollBarVisibility(),
					_ => WScrollBarVisibility.Disabled,
				};

				scrollViewer.VerticalScrollBarVisibility = orientation switch
				{
					ScrollOrientation.Vertical or ScrollOrientation.Both => visibility.ToWindowsScrollBarVisibility(),
					_ => WScrollBarVisibility.Disabled,
				};
			}
		}

		public static void UpdateContent(this ScrollViewer scrollViewer, IView? content, IMauiContext context)
		{
			scrollViewer.Content = content?.ToPlatform(context);
		}
	}
}