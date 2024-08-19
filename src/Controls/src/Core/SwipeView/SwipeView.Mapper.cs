using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeView
	{
		[Obsolete("Use SwipeViewHandler.Mapper instead.")]
		internal static IPropertyMapper<ISwipeView, ISwipeViewHandler> ControlsSwipeMapper =
			new ControlsMapper<SwipeView, SwipeViewHandler>(SwipeViewHandler.Mapper);

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
