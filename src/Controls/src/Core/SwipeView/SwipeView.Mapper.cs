using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class SwipeView
	{
		static SwipeView()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
			#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(SwipeView), typeof(VisualElement));
			#endif
			VisualElement.s_forceStaticConstructor = true;

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
