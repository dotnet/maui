using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Controls.Platform;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ControlsAccessibilityDelegate : AccessibilityDelegateCompatWrapper
	{
		public IViewHandler Handler { get; }
		internal bool ShouldBehaveLikeButton { get; set; }

		public ControlsAccessibilityDelegate(AccessibilityDelegateCompat? originalDelegate, IViewHandler viewHandler)
			: base(originalDelegate)
		{
			Handler = viewHandler;
		}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		internal ControlsAccessibilityDelegate(AccessibilityDelegateCompat? originalDelegate)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
			: base(originalDelegate)
		{
		}

		public override void OnInitializeAccessibilityNodeInfo(PlatformView host, AccessibilityNodeInfoCompat info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);

			if (Handler?.VirtualView is View v)
				v.UpdateSemanticNodeInfo(info);

			if (ShouldBehaveLikeButton)
			{
				info.ClassName = "android.widget.Button";
			}
			else
			{
				info.ClassName = null;
			}
		}
	}
}
