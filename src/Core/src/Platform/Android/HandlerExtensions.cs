using System;
using Microsoft.Maui.Hosting;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static AView ToNative(this IView view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = view.Handler;

			if (handler == null)
			{

				handler = context.Handlers.GetHandler(view.GetType());

				if (handler == null)
					throw new Exception($"Handler not found for view {view}");

				if (handler is IAndroidViewHandler ahandler)
					ahandler.SetContext(context.Context);

				view.Handler = handler;
			}

			handler.SetVirtualView(view);

			if (!(handler.NativeView is AView result))
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(AView)}");
			}

			return result;
		}
	}
}