#if __IOS__
using NativeView = UIKit.UIView;
#elif __MACOS__
using NativeView = AppKit.NSView;
#elif MONOANDROID
using NativeView = Microsoft.Maui.MauiView;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		public static PropertyMapper<IView> ViewMapper = new PropertyMapper<IView>
		{
			[nameof(IView.AutomationId)] = MapAutomationId,
			[nameof(IView.BackgroundColor)] = MapBackgroundColor,
			[nameof(IView.Clip)] = MapClip,
			[nameof(IView.Frame)] = MapFrame,
			[nameof(IView.IsEnabled)] = MapIsEnabled
		};

		public static void MapAutomationId(IViewHandler handler, IView view)
		{
			(handler.NativeView as NativeView)?.UpdateAutomationId(view);
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			(handler.NativeView as NativeView)?.UpdateBackgroundColor(view);
		}

		public static void MapClip(IViewHandler handler, IView view)
		{
			(handler.NativeView as NativeView)?.UpdateClip(view);
		}

		public static void MapFrame(IViewHandler handler, IView view)
		{
			handler.SetFrame(view.Frame);
		}

		public static void MapIsEnabled(IViewHandler handler, IView view)
		{
			(handler.NativeView as NativeView)?.UpdateIsEnabled(view);
		}
	}
}