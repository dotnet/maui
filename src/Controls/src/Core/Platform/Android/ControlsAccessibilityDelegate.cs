// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Controls.Platform;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ControlsAccessibilityDelegate : AccessibilityDelegateCompatWrapper
	{
		public IViewHandler Handler { get; }

		public ControlsAccessibilityDelegate(AccessibilityDelegateCompat? originalDelegate, IViewHandler viewHandler)
			: base(originalDelegate)
		{
			Handler = viewHandler;
		}

		public override void OnInitializeAccessibilityNodeInfo(PlatformView host, AccessibilityNodeInfoCompat info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);

			if (Handler?.VirtualView is View v)
				v.UpdateSemanticNodeInfo(info);
		}
	}
}
