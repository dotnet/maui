using System;
#if __IOS__
using NativeView = UIKit.UIView;
#elif __MACOS__
using NativeView = AppKit.NSView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Xamarin.Platform.Handlers
{
	public partial class ViewHandler
	{
		public static PropertyMapper<IView> ViewMapper = new PropertyMapper<IView>
		{
			[nameof(IView.BackgroundColor)] = MapBackgroundColor,
			[nameof(IView.Frame)] = MapPropertyFrame,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
		};

		public static void MapPropertyFrame(IViewHandler handler, IView view)
			=> handler?.SetFrame(view.Frame);

		public static void MapIsEnabled(IViewHandler handler, IView view)
			=> (handler.NativeView as NativeView).UpdateIsEnabled(view);

		public static void MapBackgroundColor(IViewHandler handler, IView view)
			=> (handler.NativeView as NativeView).UpdateBackgroundColor(view);
	}
}