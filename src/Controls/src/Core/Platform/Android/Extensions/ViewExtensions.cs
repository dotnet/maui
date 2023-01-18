#nullable disable
using Android.Graphics.Drawables;
using AView = Android.Views.View;

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
	}
}
