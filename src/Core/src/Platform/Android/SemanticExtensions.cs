using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;

namespace Microsoft.Maui.Platform
{
	public static partial class SemanticExtensions
	{
		public static void UpdateSemanticNodeInfo(this View nativeView, IView virtualView, AccessibilityNodeInfoCompat? info)
		{
			if (info == null || virtualView == null)
				return;

			var semantics = virtualView.Semantics;
			var desc = semantics?.Description;
			var hint = semantics?.Hint;

			string? newText = null;
			string? newContentDescription = null;

			if (!string.IsNullOrEmpty(desc))
			{
				// Edit Text fields won't read anything for the content description
				if (nativeView is EditText et)
				{
					if (!string.IsNullOrEmpty(et.Text))
						newText = $"{desc}, {et.Text}";
					else
						newText = $"{desc}";
				}
				else
					newContentDescription = desc;
			}

			if (!string.IsNullOrEmpty(hint))
			{
				// info HintText won't read anything back when using TalkBack pre API 26
				if (PlatformVersion.IsAtLeast(26))
				{
					info.HintText = hint;

					if (nativeView is EditText)
						info.ShowingHintText = false;
				}
				else
				{
					if (nativeView is EditText et)
					{
						newText = newText ?? et.Text;
						newText = $"{newText}, {hint}";
					}
					else if (nativeView is TextView tv)
					{
						if (newContentDescription != null)
						{
							newText = $"{newContentDescription}, {hint}";
						}
						else if (!string.IsNullOrEmpty(tv.Text))
						{
							newText = $"{tv.Text}, {hint}";
						}
						else
						{
							newText = $"{hint}";
						}
					}
					else
					{
						if (newContentDescription != null)
						{
							newText = $"{newContentDescription}, {hint}";
						}
						else
						{
							newText = $"{hint}";
						}
					}

					newContentDescription = null;
				}
			}

			if (!string.IsNullOrWhiteSpace(newContentDescription))
				info.ContentDescription = newContentDescription;
			else if (info.ContentDescription == virtualView.AutomationId)
				info.ContentDescription = null;

			if (!string.IsNullOrWhiteSpace(newText))
				info.Text = newText;
		}

		public static void UpdateSemantics(this View nativeView, IView view)
		{
			var semantics = view.Semantics;

			if (semantics == null)
				return;

			ViewCompat.SetAccessibilityHeading(nativeView, semantics.IsHeading);
		}

		internal static View GetSemanticNativeElement(this View nativeView)
		{
			return ViewHelper.GetSemanticNativeElement(nativeView)!;
		}
	}
}
