using System;
using Android.Text;
using Android.Views;
using Android.Views.Accessibility;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(this IView element)
		{
			if (element?.Handler?.NativeView is not View view)
				throw new NullReferenceException("Can't access view from a null handler");

			view.SendAccessibilityEvent(EventTypes.ViewFocused);
		}

		public static void UpdateSemanticNodeInfo(this View nativeView, IView virtualView, AccessibilityNodeInfoCompat? info)
		{
			if (info == null)
				return;

			var semantics = virtualView?.Semantics;

			if (semantics == null)
				return;

			string? newText = null;
			string? newContentDescription = null;

			var desc = semantics.Description;
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

			var hint = semantics.Hint;
			if (!string.IsNullOrEmpty(hint))
			{
				// info HintText won't read anything back when using TalkBack pre API 26
				if (NativeVersion.IsAtLeast(26))
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
	}
}
