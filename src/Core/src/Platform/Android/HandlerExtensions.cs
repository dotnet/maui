using System;
using Android.App;
using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class HandlerExtensions
	{
		public static AView ToContainerView(this IElement view, IMauiContext context) =>
			new ContainerView(context) { CurrentView = view };

		public static AView ToNative(this IElement view, IMauiContext context)
		{
			var handler = view.ToHandler(context);

			if (handler.NativeView is not AView result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(AView)}");
			}
			return result;
		}

		public static IElementHandler ToHandler(this IElement view, IMauiContext context)
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

			return (IElementHandler)handler;
		}

		public static void SetApplicationHandler(this Application nativeApplication, IApplication application, IMauiContext context) =>
			SetHandler(nativeApplication, application, context);

		public static void SetWindowHandler(this Activity activity, IWindow window, IMauiContext context) =>
			SetHandler(activity, window, context);

		static void SetHandler(this Context nativeElement, IElement element, IMauiContext context)
		{
			_ = nativeElement ?? throw new ArgumentNullException(nameof(nativeElement));
			_ = element ?? throw new ArgumentNullException(nameof(element));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = element.Handler;
			if (handler?.MauiContext != null && handler.MauiContext != context)
				handler = null;

			if (handler == null)
				handler = context.Handlers.GetHandler(element.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for window {element}.");

			handler.SetMauiContext(context);

			element.Handler = handler;

			if (handler.VirtualView != element)
				handler.SetVirtualView(element);
		}
	}
}