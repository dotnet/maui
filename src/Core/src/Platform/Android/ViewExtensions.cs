using Android.Graphics.Drawables;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		const int DefaultAutomationTagId = -1;
		public static int AutomationTagId { get; set; } = DefaultAutomationTagId;

		public static void UpdateIsEnabled(this AView nativeView, IView view)
		{
			if (nativeView != null)
				nativeView.Enabled = view.IsEnabled;
		}

		public static void UpdateBackground(this AView nativeView, IView view)
		{
			if (view == null)
				return;

			// Remove previous background gradient if any
			if (view.Background is MauiDrawable mauiDrawable)
			{
				nativeView.Background = null;
				mauiDrawable.Dispose();
			}

			var brush = view.Background;
			if (brush.IsNullOrEmpty())
				return;

			nativeView.Background = brush?.ToDrawable();
		}

		public static void SetBackground(this AView view, Drawable? drawable)
		{
			view.Background = drawable;
		}

		public static bool GetClipToOutline(this AView view)
		{
			return view.ClipToOutline;
		}

		public static void SetClipToOutline(this AView view, bool value)
		{
			view.ClipToOutline = value;
		}

		public static void UpdateAutomationId(this AView nativeView, IView view)
		{
			if (AutomationTagId == DefaultAutomationTagId)
			{
				AutomationTagId = Microsoft.Maui.Resource.Id.automation_tag_id;
			}

			nativeView.SetTag(AutomationTagId, view.AutomationId);
		}
	}
}