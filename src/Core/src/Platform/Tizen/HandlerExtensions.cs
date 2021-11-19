using System;
using Tizen.Applications;
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
				handler = context.Handlers.GetHandler(view.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for view {view}.");

			handler.SetMauiContext(context);

				handler.SetMauiContext(context);

				view.Handler = handler;
			}

			handler.SetVirtualView(view);

			if (!(handler.NativeView is EvasObject result))
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(EvasObject)}");

			// Root content view should register to LayoutUpdated() callback.
			if (isRoot && handler is LayoutHandler layoutHandler)
			{
				layoutHandler.RegisterOnLayoutUpdated();
			}

			return result;
		}

		public static void SetApplicationHandler(this CoreUIApplication nativeApplication, IApplication application, IMauiContext context) =>
			SetHandler(application, context);

		public static void SetWindowHandler(this Window nativeWindow, IWindow window, IMauiContext context) =>
			SetHandler(window, context);

		static void SetHandler(IElement element, IMauiContext context)
		{
			_ = element ?? throw new ArgumentNullException(nameof(element));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = element.Handler;
			if (handler == null)
				handler = context.Handlers.GetHandler(element.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for view {element}.");

			handler.SetMauiContext(context);

			element.Handler = handler;

			if (handler.VirtualView != element)
				handler.SetVirtualView(element);
		}
	}
}