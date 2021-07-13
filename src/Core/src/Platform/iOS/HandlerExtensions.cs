using System;
using UIKit;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static UIViewController ToUIViewController(this IElement view, IMauiContext context)
		{
			var nativeView = view.ToNative(context);
			if (view?.Handler is INativeViewHandler nvh && nvh.ViewController != null)
				return nvh.ViewController;

			return new ContainerViewController { CurrentView = view, Context = context };
		}

		public static UIView ToNative(this IElement view, IMauiContext context)
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

			if (((INativeViewHandler)handler).NativeView is not UIView result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(UIView)}");
			}

			return result;
		}

		public static void SetWindow(this UIWindow nativeWindow, IWindow window, IMauiContext mauiContext)
		{
			_ = nativeWindow ?? throw new ArgumentNullException(nameof(nativeWindow));
			_ = window ?? throw new ArgumentNullException(nameof(window));
			_ = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));

			var handler = window.Handler;
			if (handler == null)
				handler = mauiContext.Handlers.GetHandler(window.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for window {window}.");

			handler.SetMauiContext(mauiContext);

			window.Handler = handler;

			if (handler.VirtualView != window)
				handler.SetVirtualView(window);
		}
	}
}