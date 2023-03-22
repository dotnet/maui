using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using WSwipeBehaviorOnInvoked = Microsoft.UI.Xaml.Controls.SwipeBehaviorOnInvoked;
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;
using WSwipeMode = Microsoft.UI.Xaml.Controls.SwipeMode;


namespace Microsoft.Maui.Platform
{
	public static partial class SwipeViewExtensions
	{
		public static void UpdateBackground(this WSwipeItem platformControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			platformControl.UpdateProperty(WSwipeItem.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToPlatform());

		internal static void UpdateProperty(this WSwipeItem platformControl, DependencyProperty property, object? value)
		{
			if (value == null)
				platformControl.ClearValue(property);
			else
				platformControl.SetValue(property, value);
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

		public static void UpdateTextColor(this WSwipeItem platformControl, ISwipeItemMenuItem view)
		{
			var textColor = view.TextColor ?? view.GetTextColor();

			if (textColor != null)
				platformControl.Foreground = textColor.ToPlatform();
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
