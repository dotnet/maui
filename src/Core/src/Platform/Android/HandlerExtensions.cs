using System;
using Android.App;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static AView ToContainerView(this IView view, IMauiContext context) =>
			new ContainerView(context) { CurrentView = view };

		public static AView ToNative(this IView view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			// This is how MVU works. It collapses views down
			if (view is IReplaceableView ir)
				view = ir.ReplacedView;

			var handler = view.Handler;
			if (handler?.MauiContext != null && handler.MauiContext != context)
				handler = null;

			if (handler == null)
				handler = context.Handlers.GetHandler(view.GetType()) as IViewHandler;

			if (handler == null)
				throw new Exception($"Handler not found for view {view} or was not {nameof(IViewHandler)}.");

			handler.SetMauiContext(context);

			view.Handler = handler;

			if (handler.VirtualView != view)
				handler.SetVirtualView(view);

			if (((INativeViewHandler)handler).NativeView is not AView result)
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(AView)}");

			return result;
		}

		public static void SetWindow(this Activity activity, IWindow window, IMauiContext context)
		{
			_ = activity ?? throw new ArgumentNullException(nameof(activity));
			_ = window ?? throw new ArgumentNullException(nameof(window));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = window.Handler as IWindowHandler;
			if (handler?.MauiContext != null && handler.MauiContext != context)
				handler = null;

			if (handler == null)
				handler = context.Handlers.GetHandler(window.GetType()) as IWindowHandler;

			if (handler == null)
				throw new Exception($"Handler not found for view {window} or was not {nameof(IWindowHandler)}.");

			handler.SetMauiContext(context);

			window.Handler = handler;

			if (handler.VirtualView != window)
				handler.SetVirtualView(window);
		}
	}
}