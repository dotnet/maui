using System;
using Tizen.Applications;
using ElmSharp;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		internal static EvasObject? GetNative(this IElement view, bool returnWrappedIfPresent)
		{
			if (view.Handler is INativeViewHandler nativeHandler && nativeHandler.NativeView != null)
				return nativeHandler.NativeView;

			return (view.Handler?.NativeView as EvasObject);
		}

		internal static EvasObject ToNative(this IElement view, IMauiContext context, bool returnWrappedIfPresent)
		{
			var nativeView = view.ToNative(context);

			if (view.Handler is INativeViewHandler nativeHandler && nativeHandler.NativeView != null)
				return nativeHandler.NativeView;

			return nativeView;
		}

		public static EvasObject ToContainerView(this IElement view, IMauiContext context) =>
			new ContainerView(context) { CurrentView = view };

		public static EvasObject ToNative(this IElement view, IMauiContext context)
		{
			var handler = view.ToHandler(context);

			if (handler.NativeView is not EvasObject result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(EvasObject)}");
			}

			return result;
		}

		public static INativeViewHandler ToHandler(this IElement view, IMauiContext context)
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

			return (INativeViewHandler)handler;
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