using System;
#if __IOS__
using NativeView = UIKit.UIButton;
#elif __MACOS__
using NativeView = AppKit.NSButton;
#elif MONOANDROID
using NativeView = AndroidX.AppCompat.Widget.AppCompatCheckBox;
#else
using NativeView = System.Object;
#endif

namespace System.Maui.Platform {
	public partial class CheckBoxRenderer
	{
		protected override NativeView CreateView()
		{
			var checkBox = new NativeView();
			return checkBox;
		}
	}
}
