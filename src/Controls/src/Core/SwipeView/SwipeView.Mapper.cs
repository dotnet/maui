using System;
using System.Threading;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeView
	{
		static int s_remappedForControls;

		internal new static void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			VisualElement.RemapForControls();

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
