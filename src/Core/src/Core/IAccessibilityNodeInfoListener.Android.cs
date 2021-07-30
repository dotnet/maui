using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Core.View.Accessibility;

namespace Microsoft.Maui.Platform
{
	public interface IAccessibilityNodeInfoListener
	{
		void InitializeAccessibilityNodeInfo(AccessibilityNodeInfoCompat? nodeInfo);
	}
}
