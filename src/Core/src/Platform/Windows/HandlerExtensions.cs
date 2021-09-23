using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static FrameworkElement ToNative(this IElement view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			//This is how MVU works. It collapses views down
			if (view is IReplaceableView ir)
				view = ir.ReplacedView;

			var handler = view.Handler;

			if (handler == null)
				handler = context.Handlers.GetHandler(view.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for view {view}.");

			handler.SetMauiContext(context);

			view.Handler = handler;

			if (handler.VirtualView != view)
				handler.SetVirtualView(view);

			if (((INativeViewHandler)handler).NativeView is not FrameworkElement result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(FrameworkElement)}");
			}

			return result;
		}

		public static void SetApplicationHandler(this UI.Xaml.Application nativeApplication, IApplication application, IMauiApplicationContext context) =>
			SetHandler(nativeApplication, application, context);

		public static void SetWindowHandler(this UI.Xaml.Window nativeWindow, IWindow window, IMauiWindowContext context) =>
			SetHandler(nativeWindow, window, context);

		static void SetHandler(this WinRT.IWinRTObject nativeElement, IElement element, IMauiContext context)
		{
			_ = nativeElement ?? throw new ArgumentNullException(nameof(nativeElement));
			_ = element ?? throw new ArgumentNullException(nameof(element));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = element.Handler;
			if (handler == null)
				handler = context.Handlers.GetHandler(element.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for window {element}.");

			handler.SetMauiContext(context);

			element.Handler = handler;

			if (handler.VirtualView != element)
				handler.SetVirtualView(element);
		}
	}
}