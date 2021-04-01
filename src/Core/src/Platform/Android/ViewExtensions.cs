using AndroidX.Core.View;
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

		public static void UpdateBackgroundColor(this AView nativeView, IView view)
		{
			var backgroundColor = view.BackgroundColor;
			if (!backgroundColor.IsDefault)
				nativeView?.SetBackgroundColor(backgroundColor.ToNative());
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

		public static void UpdateSemantics(this AView nativeView, IView view)
		{
			var semantics = view.Semantics;
			if (semantics == null)
				return;

			nativeView.ContentDescription = semantics.Description;

			// Set the delegate here?
			//if (!string.IsNullOrEmpty(semantics.Hint))
			//{
			//	var accessibilityDelegate = ViewCompat.GetAccessibilityDelegate(nativeView);
			//	if (accessibilityDelegate is MauiAccessibilityDelegate mad)
			//		mad.View = view;
			//	else if (accessibilityDelegate == null)
			//		ViewCompat.SetAccessibilityDelegate(nativeView, new MauiAccessibilityDelegate(view));
			//}

			ViewCompat.SetAccessibilityHeading(nativeView, semantics.IsHeading);
		}
	}
}