using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeView
	{
		internal static new void RemapForControls()
		{
			// Adjusted the mapping to preserve SwipeView.Entry legacy behavior
			SwipeViewHandler.Mapper.AppendToMapping<SwipeView, ISwipeViewHandler>(nameof(Background), MapBackground);
		}

		static void MapBackground(ISwipeViewHandler handler, SwipeView swipeView)
		{
			if (swipeView.Content is not null)
			{
				var contentBackgroundIsNull = Brush.IsNullOrEmpty(swipeView.Content.Background);
#pragma warning disable CS0618 // BackgroundColor — SwipeView backward compatibility
				var contentBackgroundColorIsNull = swipeView.Content.BackgroundColor == null;
#pragma warning restore CS0618

				if (contentBackgroundIsNull && contentBackgroundColorIsNull)
				{
					if (!Brush.IsNullOrEmpty(swipeView.Background))
					{
						swipeView.Content.Background = swipeView.Background;
					}
#pragma warning disable CS0618 // BackgroundColor — SwipeView backward compatibility
					else if (swipeView.BackgroundColor != null)
					{
						swipeView.Content.BackgroundColor = swipeView.BackgroundColor;
					}
#pragma warning restore CS0618
				}
			}
		}
	}
}
