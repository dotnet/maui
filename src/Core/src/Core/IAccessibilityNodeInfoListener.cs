using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
#if __ANDROID__
	using AndroidX.Core.View.Accessibility;
	public interface IAccessibilityNodeInfoListener
	{
		void InitializeAccessibilityNodeInfo(AccessibilityNodeInfoCompat? nodeInfo);
	}
#endif
}
