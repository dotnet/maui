using System;
using Microsoft.Maui.Hosting;
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
			{
				handler = context.Handlers.GetHandler(view.GetType());

				if (handler == null)
					throw new Exception($"Handler not found for view {view}");

				handler.SetMauiContext(context);

				view.Handler = handler;
			}

			handler.SetVirtualView(view);

			if (((INativeViewHandler)handler).NativeView is not FrameworkElement result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(FrameworkElement)}");
			}

			return result;
		}
	}
}