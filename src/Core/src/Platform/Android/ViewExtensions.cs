using Google.Android.Material.TextField;
using AView = Android.Views.View;
using ATextView = Android.Widget.TextView;
using AEditText = Android.Widget.EditText;
using System;

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

			// this code is wrong			
			var semantics = view.Semantics;
			if  (semantics.Hint == null && semantics.Description == null)
				return;
			
			// Need to test Pre API 26 talkback (verify on all TextView based controls)
			if (nativeView is TextInputLayout til)
			{
				til.ContentDescription = semantics.Description;
				til.Hint = view.Semantics.Hint;
			}
			else if (
				nativeView is ATextView tvPreApi26 &&

				// TODO IS this right?
				view is ILabel &&
				//!(nativeView is AEditText) &&
				!NativeVersion.IsAtLeast(26))
			{
				string? contentDescription;

				if (string.IsNullOrEmpty(semantics.Description))
					contentDescription = tvPreApi26.Text;
				else
					contentDescription = semantics.Description;

				tvPreApi26.ContentDescription = $"{contentDescription}, {semantics.Hint}";
				tvPreApi26.Hint = semantics.Hint;
			}
			else if (nativeView is ATextView tv)
			{
				tv.ContentDescription = semantics.Description;
				tv.Hint = semantics.Hint;
			}
			else
            {
				// TODO IS this right?
				//var text = $"{semantics.Description}, {semantics.Hint}";
				nativeView.ContentDescription = semantics.Description;
			}
		}
	}
}