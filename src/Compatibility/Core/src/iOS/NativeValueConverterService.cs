using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Internals;
#if __MOBILE__
using UIKit;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.NativeValueConverterService))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using UIView = AppKit.NSView;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.MacOS.NativeValueConverterService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	[Preserve(AllMembers = true)]
	class NativeValueConverterService : INativeValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object nativeValue)
		{
			nativeValue = null;
			if (typeof(UIView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				nativeValue = ((UIView)value).ToView();
				return true;
			}
			return false;
		}
	}
}