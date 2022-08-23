using Gtk;
using System;

namespace Microsoft.Maui
{

	public static class HandlerExtensions
	{

		public static Widget ToPlatform(this IView view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = view.Handler ?? context.Handlers.GetHandler(view.GetType()) as IViewHandler;

			if (handler == null)
				throw new Exception($"Handler not found for view {view}");

			handler.SetMauiContext(context);

			view.Handler = handler;

			handler.SetVirtualView(view);

			if (handler.PlatformView is not Widget result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(Widget)}");
			}

			return result;
		}

	}

}