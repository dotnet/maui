using System;
using ElmSharp;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static EvasObject ToNative(this IView view, IMauiContext context)
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
				handler.SetVirtualView(view);
				view.Handler = handler;
			}

			if (((INativeViewHandler)handler).NativeView is not EvasObject result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(EvasObject)}");
			}

			return result;
		}
	}
}