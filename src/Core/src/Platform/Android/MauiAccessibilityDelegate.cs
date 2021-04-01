using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class MauiAccessibilityDelegate : AccessibilityDelegateCompat
	{
		public IView? View { get; set; }

		public MauiAccessibilityDelegate(IView view)
		{
			View = view;
		}

		public override void OnInitializeAccessibilityNodeInfo(AView? host, AccessibilityNodeInfoCompat? info)
		{
			if (View == null)
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

				if (host is EditText)
					info.ShowingHintText = false;
			}
		}
	}

}
