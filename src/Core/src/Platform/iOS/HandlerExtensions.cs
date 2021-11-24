using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class HandlerExtensions
	{
		const string UIApplicationSceneManifestKey = "UIApplicationSceneManifest";

		internal static UIView? GetNative(this IElement view, bool returnWrappedIfPresent)
		{
			if (view.Handler is INativeViewHandler nativeHandler && nativeHandler.NativeView != null)
				return nativeHandler.NativeView;

			return (view.Handler?.NativeView as UIView);

		}

		internal static UIView ToNative(this IElement view, IMauiContext context, bool returnWrappedIfPresent)
		{
			var nativeView = view.ToNative(context);

			if (view.Handler is INativeViewHandler nativeHandler && nativeHandler.NativeView != null)
				return nativeHandler.NativeView;

			return nativeView;

		}

		public static UIViewController ToUIViewController(this IElement view, IMauiContext context)
		{
			var nativeView = view.ToNative(context);
			if (view?.Handler is INativeViewHandler nvh && nvh.ViewController != null)
				return nvh.ViewController;

			return new ContainerViewController { CurrentView = view, Context = context };
		}

		public static UIView ToNative(this IElement view, IMauiContext context)
		{
			var handler = view.ToHandler(context);

			if (handler.NativeView is not UIView result)
			{
				throw new InvalidOperationException($"Unable to convert {view} to {typeof(UIView)}");
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

		// If < iOS 13 or the Info.plist does not have a scene manifest entry we need to assume no multi window, and no UISceneDelegate.
		// We cannot check for iPads/Mac because even on the iPhone it uses the scene delegate if one is specified in the manifest.
		public static bool HasSceneManifest(this UIApplicationDelegate nativeApplication) =>
			UIDevice.CurrentDevice.CheckSystemVersion(13, 0) &&
			NSBundle.MainBundle.InfoDictionary.ContainsKey(new NSString(UIApplicationSceneManifestKey));

		public static void SetApplicationHandler(this UIApplicationDelegate nativeApplication, IApplication application, IMauiContext context) =>
			SetHandler(nativeApplication, application, context);

		public static void SetWindowHandler(this UIWindow nativeWindow, IWindow window, IMauiContext context) =>
			SetHandler(nativeWindow, window, context);

		static void SetHandler(this NSObject nativeElement, IElement element, IMauiContext mauiContext)
		{
			_ = nativeElement ?? throw new ArgumentNullException(nameof(nativeElement));
			_ = element ?? throw new ArgumentNullException(nameof(element));
			_ = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));

			var handler = element.Handler;
			if (handler == null)
				handler = mauiContext.Handlers.GetHandler(element.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for window {element}.");

			handler.SetMauiContext(mauiContext);

			element.Handler = handler;

			if (handler.VirtualView != element)
				handler.SetVirtualView(element);
		}
	}
}
