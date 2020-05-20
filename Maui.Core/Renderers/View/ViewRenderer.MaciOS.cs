using System;
#if __MOBILE__
using NativeColor = UIKit.UIColor;
using NativeControl = UIKit.UIControl;
using NativeView = UIKit.UIView;

#else
using NativeView = AppKit.NSView;
using NativeColor = CoreGraphics.CGColor;
using NativeControl = AppKit.NSControl;

#endif

namespace System.Maui.Platform
{
	public partial class ViewRenderer
	{
		public static void MapPropertyIsEnabled(IViewRenderer renderer, IView view)
		{
			var uiControl = renderer.NativeView as NativeControl;
			if (uiControl == null)
				return;
			uiControl.Enabled = view.IsEnabled;
		}
		public static void MapBackgroundColor (IViewRenderer renderer, IView view)
		{
			var nativeView = (NativeView)renderer.NativeView;
			var color = view.BackgroundColor;
			if (color != null && !color.IsDefault)
				nativeView.SetBackgroundColor(color.ToNativeColor ());
		}
	}
}
