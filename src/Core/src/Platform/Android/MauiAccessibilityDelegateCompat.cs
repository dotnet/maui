using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Handlers;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public class MauiAccessibilityDelegateCompat : AccessibilityDelegateCompatWrapper
	{
		public IViewHandler? Handler { get; set; }

		public MauiAccessibilityDelegateCompat() : this(null)
		{
		}

		public MauiAccessibilityDelegateCompat(AccessibilityDelegateCompat? originalDelegate) : base(originalDelegate)
		{
		}

		public override void OnInitializeAccessibilityNodeInfo(PlatformView? host, AccessibilityNodeInfoCompat? info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);

			if (Handler?.PlatformView is PlatformView platformView && Handler?.VirtualView != null)
				platformView.UpdateSemanticNodeInfo(Handler.VirtualView, info);
		}
	}
}
