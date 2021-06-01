using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.View;
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
			nativeView.Enabled = view.IsEnabled;
		}

		public static void UpdateVisibility(this AView nativeView, IView view)
		{
			nativeView.Visibility = view.Visibility.ToNativeVisibility();
		}

		public static ViewStates ToNativeVisibility(this Visibility visibility)
		{
			return visibility switch
			{
				Visibility.Hidden => ViewStates.Invisible,
				Visibility.Collapsed => ViewStates.Gone,
				_ => ViewStates.Visible,
			};
		}

		public static void UpdateBackground(this AView nativeView, IView view, Drawable? defaultBackground = null)
		{
			// Remove previous background gradient if any
			if (nativeView.Background is MauiDrawable mauiDrawable)
			{
				nativeView.Background = null;
				mauiDrawable.Dispose();
			}

			var paint = view.Background;

			if (paint.IsNullOrEmpty())
				nativeView.Background = defaultBackground;
			else
				nativeView.Background = paint!.ToDrawable();
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
				AutomationTagId = Resource.Id.automation_tag_id;
			}

			nativeView.SetTag(AutomationTagId, view.AutomationId);
		}

		public static void UpdateSemantics(this AView nativeView, IView view)
		{
			var semantics = view.Semantics;

			if (semantics == null)
				return;

			nativeView.ContentDescription = semantics.Description;
			ViewCompat.SetAccessibilityHeading(nativeView, semantics.IsHeading);
		}

		public static void InvalidateMeasure(this AView nativeView, IView view)
		{
			nativeView.RequestLayout();
		}

		public static void UpdateWidth(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Width into account during the layout
			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}

		public static void UpdateHeight(this AView nativeView, IView view)
		{
			// GetDesiredSize will take the specified Height into account during the layout
			if (!nativeView.IsInLayout)
			{
				nativeView.RequestLayout();
			}
		}
	}
}