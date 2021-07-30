#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View : IAccessibilityNodeInfoListener
	{
		void IAccessibilityNodeInfoListener.InitializeAccessibilityNodeInfo(AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat? nodeInfo) =>
			InitializeAccessibilityNodeInfo(nodeInfo);

		protected virtual void InitializeAccessibilityNodeInfo(AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat? nodeInfo) =>
			this.UpdateSemanticNodeInfo(nodeInfo);
	}
}
