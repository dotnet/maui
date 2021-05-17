using System;
using ElmSharp;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static EvasObject ToNative(this IView view, IMauiContext context, bool isRoot = true)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			//This is how MVU works. It collapses views down
			if (view is IReplaceableView ir)
				view = ir.ReplacedView;

			var handler = view.Handler;

			if (handler == null)
			{
				handler = context.Handlers.GetHandler(view.GetType());

				if (handler == null)
					throw new Exception($"Handler not found for view {view}");

				handler.SetMauiContext(context);

				view.Handler = handler;
			}

			handler.SetVirtualView(view);

			if (!(handler.NativeView is EvasObject result))
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(EvasObject)}");
			}

			// Root content view should register to LayoutUpdated() callback.
			if (isRoot && handler is LayoutHandler layoutHandler)
			{
				layoutHandler.RegisterOnLayoutUpdated();
			}

			return result;
		}
	}
}