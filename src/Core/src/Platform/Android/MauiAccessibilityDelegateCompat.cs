using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Handlers;
using NativeView = Android.Views.View;

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

		public override void OnInitializeAccessibilityNodeInfo(NativeView? host, AccessibilityNodeInfoCompat? info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);

			if (Handler?.NativeView is NativeView nativeView && Handler?.VirtualView != null)
				nativeView.UpdateSemanticNodeInfo(Handler.VirtualView, info);
		}
	}
}
