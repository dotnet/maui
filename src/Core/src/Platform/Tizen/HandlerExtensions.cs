using System;
using Tizen.Applications;
using NView = Tizen.NUI.BaseComponents.View;
using Tizen.NUI;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static NView ToNative(this IElement view, IMauiContext context)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			Console.WriteLine($"HandlerExtension::ToNative - VirtualView {view} - start");

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

			if (((INativeViewHandler)handler).NativeView is not NView result)
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(NView)}");

			Console.WriteLine($"HandlerExtension::ToNative - VirtualView {view}, Handler {handler} - End");

			return result;
		}

		public static void SetApplicationHandler(this CoreApplication nativeApplication, IApplication application, IMauiContext context)
		{
			_ = nativeApplication ?? throw new ArgumentNullException(nameof(nativeApplication));
			SetHandler(application, context);
		}

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