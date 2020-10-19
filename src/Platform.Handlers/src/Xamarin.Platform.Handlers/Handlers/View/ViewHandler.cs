using System;
using System.Drawing;
using System.Runtime.CompilerServices;
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
			[nameof(IView.Frame)] = MapFrame,
			[nameof(IView.IsEnabled)] = MapIsEnabled
		};

		public static void MapFrame(IViewHandler handler, IView view)
		{
			CheckParameters(handler, view);
			handler.SetFrame(view.Frame);
		}

		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			CheckParameters(handler, view);
			(handler.NativeView as NativeView)?.UpdateIsEnabled(view);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			CheckParameters(handler, view);
			(handler.NativeView as NativeView)?.UpdateBackgroundColor(view);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void CheckParameters(IViewHandler handler, IView view)
		{
			_ = handler ?? throw new ArgumentNullException(nameof(handler));
			_ = view ?? throw new ArgumentNullException(nameof(view));
		}
	}
}