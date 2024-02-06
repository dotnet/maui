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

		public static void UpdateScrollBarVisibility(this ScrollViewer scrollViewer, ScrollOrientation orientation,
			ScrollBarVisibility horizontalScrollBarVisibility)
		{
			if (horizontalScrollBarVisibility == ScrollBarVisibility.Default)
			{
				// If the user has not explicitly set a horizontal scroll bar visibility, then the orientation will
				// determine what the horizontal scroll bar does

				scrollViewer.HorizontalScrollBarVisibility = orientation switch
				{
					ScrollOrientation.Horizontal or ScrollOrientation.Both => WScrollBarVisibility.Auto,
					_ => WScrollBarVisibility.Disabled,
				};

				return;
			}

			// If the user _has_ set a horizontal scroll bar visibility preference, then convert that preference to the native equivalent
			// if the orientation allows for it

			scrollViewer.HorizontalScrollBarVisibility = orientation switch
			{
				ScrollOrientation.Horizontal or ScrollOrientation.Both => horizontalScrollBarVisibility.ToWindowsScrollBarVisibility(),
				_ => WScrollBarVisibility.Disabled,
			};

			// TODO ezhart 2021-07-08 RE: the note below - do we actually need to be accounting for Neither in the measurement code?
			// Could we just disable the scroll bars entirely?
			// Accounting for Neither in the xplat measurement code is a leftover from Forms, it may be easier to do that on the native side here.

			// Note that the Orientation setting of "Neither" is covered by the measurement code (the size of the content is limited
			// so that no scrolling is possible) and the xplat scrolling code (the ScrollTo methods are disabled when Orientation=Neither)
		}

		public static void UpdateContent(this ScrollViewer scrollViewer, IView? content, IMauiContext context)
		{
			scrollViewer.Content = content?.ToPlatform(context);
		}
	}
}