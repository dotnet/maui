#nullable disable
using Android.Graphics.Drawables;
using AndroidX.Core.View;
using AView = Android.Views.View;
using AViewAccessibility = Android.Views.Accessibility;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ViewExtensions
	{
		public static void SetBackground(this AView view, Drawable drawable)
		{
			view.Background = drawable;
		}

		public static void EnsureId(this AView view)
		{
			if (!view.IsAlive())
			{
				return;
			}

			if (view.Id == AView.NoId)
			{
				view.Id = AView.GenerateViewId();
			}
		}

		public static bool SetElevation(this AView view, float value)
		{
			if (!view.IsAlive())
				return false;

			view.Elevation = value;
			return true;
		}

		internal static void MaybeRequestLayout(this AView view)
		{
			if (!view.IsInLayout && !view.IsLayoutRequested)
				view.RequestLayout();
		}

		internal static void AddRecyclerItemViewAccessibility(this AView view, bool shouldBehaveLikeButton)
		{
			if (view is null)
				return;

			var currentDelegate = ViewCompat.GetAccessibilityDelegate(view);

			// If delegate already set correctly, skip
			if (currentDelegate is ControlsAccessibilityDelegate sid && sid.ShouldBehaveLikeButton == shouldBehaveLikeButton)
				return;

			// Set new delegate based on existing state
			if (currentDelegate is ControlsAccessibilityDelegate cad)
			{
				cad.ShouldBehaveLikeButton = shouldBehaveLikeButton;
			}
			else
			{
				var controlsDelegate = new ControlsAccessibilityDelegate(currentDelegate)
				{
					ShouldBehaveLikeButton = shouldBehaveLikeButton
				};
				ViewCompat.SetAccessibilityDelegate(view, controlsDelegate);
			}

			// Inform the system that accessibility state has changed
			view.SendAccessibilityEvent(AViewAccessibility.EventTypes.WindowStateChanged);
		}
	}
}
