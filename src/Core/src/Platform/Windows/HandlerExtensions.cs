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

			var handler = view.Handler;

			if (handler == null)
			{
				handler = context.Handlers.GetHandler(view.GetType());

				if (handler == null)
					throw new Exception($"Handler not found for view {view}");

				view.Handler = handler;
			}

			handler.SetVirtualView(view);

			if (!(handler.NativeView is FrameworkElement result))
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(FrameworkElement)}");
			}

			return result;
		}
	}
}