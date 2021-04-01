using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class MauiAccessibilityDelegate : View.AccessibilityDelegate
	{
		public IView? View { get; set; }

		public MauiAccessibilityDelegate(IView view)
		{
			View = view;
		}

		public override void OnInitializeAccessibilityNodeInfo(AView? host, Android.Views.Accessibility.AccessibilityNodeInfo? info)
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
			}
		}
	}

}
