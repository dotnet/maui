using System;
using System.Collections.Generic;
using System.Text;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif __ANDROID__
using NativeView = Android.Views.View;
using AndroidX.Core.View.Accessibility;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui
{
#if __ANDROID__
	public record SemanticInfoRequest(NativeView view, AccessibilityNodeInfoCompat? info);
#else
	public record SemanticInfoRequest(NativeView view);
#endif
}
