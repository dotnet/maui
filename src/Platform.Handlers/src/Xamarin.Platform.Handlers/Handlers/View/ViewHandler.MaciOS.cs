using System;

#if __IOS__
using NativeColor = UIKit.UIColor;
using NativeControl = UIKit.UIControl;
using NativeView = UIKit.UIView;
#else
using NativeColor = CoreGraphics.CGColor;
using NativeControl = AppKit.NSControl;
using NativeView = AppKit.NSView;
#endif

namespace Xamarin.Platform.Handlers
{
	public partial class ViewHandler
	{
		public static void MapPropertyIsEnabled(IViewHandler handler, IView view)
		{
			if (!(handler.NativeView is NativeControl uiControl))
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			var nativeView = (NativeView)handler.NativeView;
			var color = view.BackgroundColor;

			if (color != null && !color.IsDefault)
				nativeView.SetBackgroundColor(color.ToNative());
		}
	}
}