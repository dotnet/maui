using System;
using Tizen.Applications;
using ElmSharp;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static EvasObject ToNative(this IElement view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			//This is how MVU works. It collapses views down
			if (view is IReplaceableView ir)
				view = ir.ReplacedView;

			var handler = view.Handler;
			if (handler?.MauiContext != null && handler.MauiContext != context)
				handler = null;

			if (handler == null)
				handler = context.Handlers.GetHandler(view.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for view {view}.");

			handler.SetMauiContext(context);

			view.Handler = handler;

			if (handler.VirtualView != view)
				handler.SetVirtualView(view);

			if (((INativeViewHandler)handler).NativeView is not EvasObject result)
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(EvasObject)}");

			return result;
		}

		public static void SetWindow(this Window nativeWindow, IWindow window, IMauiContext mauiContext)
		{
			_ = nativeWindow ?? throw new ArgumentNullException(nameof(nativeWindow));
			_ = window ?? throw new ArgumentNullException(nameof(window));
			_ = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));

			var handler = window.Handler;
			if (handler == null)
				handler = mauiContext.Handlers.GetHandler(window.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for view {window}.");

			handler.SetMauiContext(mauiContext);

			window.Handler = handler;

			if (handler.VirtualView != window)
				handler.SetVirtualView(window);
		}
	}
}