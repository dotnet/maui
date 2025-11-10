using System;

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
				var contentBackgroundColorIsNull = swipeView.Content.BackgroundColor == null;

				if (contentBackgroundIsNull && contentBackgroundColorIsNull)
				{
					if (!Brush.IsNullOrEmpty(swipeView.Background))
					{
						swipeView.Content.Background = swipeView.Background;
					}
					else if (swipeView.BackgroundColor != null)
					{
						swipeView.Content.BackgroundColor = swipeView.BackgroundColor;
					}
				}
			}
		}
	}
}
