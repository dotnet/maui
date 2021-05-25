using System;
using Microsoft.Maui.Hosting;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static AView ToContainerView(this IView view, IMauiContext context) => new ContainerView(context) { CurrentView = view };
		public static AView ToNative(this IView view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			//This is how MVU works. It collapses views down
			if (view is IReplaceableView ir)
				view = ir.ReplacedView;

			var handler = view.Handler;

			if (handler?.MauiContext != null &&
				handler.MauiContext != context)
			{
				view.Handler = null;
				handler = null;
			}

			if (handler == null)
			{
				handler = context.Handlers.GetHandler(view.GetType());

				if (handler == null)
					throw new Exception($"Handler not found for view {view}");

				handler.SetMauiContext(context);

				view.Handler = handler;
			}

			handler.SetVirtualView(view);

			if (((INativeViewHandler)handler).NativeView is not AView result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(AView)}");
			}

			return result;
		}
	}
}