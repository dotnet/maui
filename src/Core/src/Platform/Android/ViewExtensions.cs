using Google.Android.Material.TextField;
using AView = Android.Views.View;
using ATextView = Android.Widget.TextView;
using AEditText = Android.Widget.EditText;
using System;
<<<<<<< HEAD
using AAccessibilityDelegate = Android.Views.View.AccessibilityDelegate;
=======
>>>>>>> 01a52bb2284ab127341c3d8e7547e9cb4158dbb8

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

<<<<<<< HEAD
		class MauiAccessibilityDelegate : AAccessibilityDelegate
		{
			public IView? View { get; set; }

			public MauiAccessibilityDelegate(IView view)
			{
				View = view;
			}

			public override void OnInitializeAccessibilityNodeInfo(AView? host, Android.Views.Accessibility.AccessibilityNodeInfo? info)
            {
				if  (View == null)
					return;

				base.OnInitializeAccessibilityNodeInfo(host, info);

				var semantics = View.Semantics;
				if (semantics == null)
					return;

				if (info == null)
					return;

				if (!string.IsNullOrEmpty(semantics.Hint))
				{
					info.HintText = semantics.Hint;
				}
			}
		}

		public static void UpdateSemantics(this AView nativeView, IView view)
		{
			// this code is wrong
			var semantics = view.Semantics;
			if (semantics == null)
				return;

			nativeView.ContentDescription = semantics.Description;

			if (!string.IsNullOrEmpty(semantics.Hint))
			{
				if (nativeView.GetAccessibilityDelegate() is MauiAccessibilityDelegate mad)
					mad.View = view;
				else
					nativeView.SetAccessibilityDelegate(new MauiAccessibilityDelegate(view));
			}

			nativeView.AccessibilityHeading = semantics.IsHeading;
=======
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
>>>>>>> 01a52bb2284ab127341c3d8e7547e9cb4158dbb8
		}
	}
}