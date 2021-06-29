using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static FrameworkElement ToNative(this IView view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			//This is how MVU works. It collapses views down
			if (view is IReplaceableView ir)
				view = ir.ReplacedView;

			var handler = view.Handler;

			if (handler == null)
				handler = context.Handlers.GetHandler(view.GetType()) as IViewHandler;

			if (handler == null)
				throw new Exception($"Handler not found for view {view}");

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

		public static void SetWindow(this UI.Xaml.Window nativeWindow, IWindow window, MauiWinUIApplication mauiApp, IMauiContext context)
		{
			_ = nativeWindow ?? throw new ArgumentNullException(nameof(nativeWindow));
			_ = mauiApp ?? throw new ArgumentNullException(nameof(mauiApp));
			_ = window ?? throw new ArgumentNullException(nameof(window));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = window.Handler as IWindowHandler;
			if (handler == null)
				handler = context.Handlers.GetHandler(window.GetType()) as IWindowHandler;

			if (handler == null)
				throw new Exception($"Handler not found for view {window} or was not {nameof(IWindowHandler)}.");

			handler.SetMauiContext(context);

			window.Handler = handler;

			if (handler.VirtualView != window)
				handler.SetVirtualView(window);

			var nativeContent = window.View.ToNative(context);

			// TODO WINUI should this be some other known constant or via some mechanism? Or done differently?
			mauiApp.Resources.TryGetValue("MauiRootContainerStyle", out object style);

			var root = new RootPanel
			{
				Style = style as UI.Xaml.Style,
				Children =
				{
					nativeContent
				}
			};

			nativeWindow.Content = root;
		}
	}
}