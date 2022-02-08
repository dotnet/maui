using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Internals;
#if __MOBILE__
using ObjCRuntime;
using UIKit;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.PlatformValueConverterService))]
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using UIView = AppKit.NSView;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.MacOS.PlatformValueConverterService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	[Preserve(AllMembers = true)]
	class PlatformValueConverterService : IPlatformValueConverterService
	{
		public bool ConvertTo(object value, Type toType, out object platformValue)
		{
			platformValue = null;
			if (typeof(UIView).IsInstanceOfType(value) && toType.IsAssignableFrom(typeof(View)))
			{
				platformValue = ((UIView)value).ToView();
				return true;
			}
			return false;
		}
	}
}