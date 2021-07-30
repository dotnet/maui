using System;
using System.Collections.Generic;
using System.Text;
#if __ANDROID__
using AndroidX.Core.View.Accessibility;
#endif

namespace Microsoft.Maui
{
#if __ANDROID__
	public record SemanticInfoRequest(AccessibilityNodeInfoCompat? NodeInfo);
#else
	public record SemanticInfoRequest();
#endif
}
