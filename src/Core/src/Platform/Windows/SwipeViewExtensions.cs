using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;
using WSwipeMode = Microsoft.UI.Xaml.Controls.SwipeMode;
using WSwipeBehaviorOnInvoked = Microsoft.UI.Xaml.Controls.SwipeBehaviorOnInvoked;


namespace Microsoft.Maui.Platform
{
	public static partial class SwipeViewExtensions
	{
		public static void UpdateBackground(this WSwipeItem nativeControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateProperty(WSwipeItem.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToPlatform());

		internal static void UpdateProperty(this WSwipeItem nativeControl, DependencyProperty property, object? value)
		{
			if (value == null)
				nativeControl.ClearValue(property);
			else
				nativeControl.SetValue(property, value);
		}

		public static WSwipeMode ToPlatform(this SwipeMode swipeMode)
		{
			switch (swipeMode)
			{
				case SwipeMode.Execute:
					return WSwipeMode.Execute;
				case SwipeMode.Reveal:
					return WSwipeMode.Reveal;
			}

			return WSwipeMode.Reveal;
		}

		public static void UpdateTextColor(this WSwipeItem nativeControl, ISwipeItemMenuItem view)
		{
			var textColor = view.TextColor ?? view.GetTextColor();

			if (textColor != null)
				nativeControl.Foreground = textColor.ToPlatform();
		}

		public static WSwipeBehaviorOnInvoked ToPlatform(this SwipeBehaviorOnInvoked swipeBehaviorOnInvoked)
		{
			switch (swipeBehaviorOnInvoked)
			{
				case SwipeBehaviorOnInvoked.Auto:
					return WSwipeBehaviorOnInvoked.Auto;
				case SwipeBehaviorOnInvoked.Close:
					return WSwipeBehaviorOnInvoked.Close;
				case SwipeBehaviorOnInvoked.RemainOpen:
					return WSwipeBehaviorOnInvoked.RemainOpen;
			}

			return WSwipeBehaviorOnInvoked.Auto;
		}
	}
}
