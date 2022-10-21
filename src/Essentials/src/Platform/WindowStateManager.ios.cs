#nullable enable
using System;
using System.Linq;
using UIKit;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IWindowStateManager
	{
		void Init(Func<UIViewController?>? getCurrentUIViewController);

		UIViewController? GetCurrentUIViewController();

		UIWindow? GetCurrentUIWindow();
	}

	public static class WindowStateManager
	{
		static IWindowStateManager? defaultImplementation;

		public static IWindowStateManager Default =>
			defaultImplementation ??= new WindowStateManagerImplementation();

		internal static void SetDefault(IWindowStateManager? implementation) =>
			defaultImplementation = implementation;
	}

	static class WindowStateManagerExtensions
	{
		public static UIViewController? GetCurrentUIViewController(this IWindowStateManager manager, bool throwOnNull)
		{
			var vc = manager.GetCurrentUIViewController();
			if (throwOnNull && vc == null)
				throw new NullReferenceException("The current view controller can not be detected.");

			return vc;
		}
		public static UIWindow? GetCurrentUIWindow(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetCurrentUIWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The current window can not be detected.");

			return window;
		}
	}

	class WindowStateManagerImplementation : IWindowStateManager
	{
		Func<UIViewController?>? getCurrentController;

		public void Init(Func<UIViewController?>? getCurrentUIViewController) =>
			getCurrentController = getCurrentUIViewController;

		public UIViewController? GetCurrentUIViewController()
		{
			var viewController = getCurrentController?.Invoke();

			if (viewController != null)
				return viewController;

			var window = GetKeyWindow();

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				viewController = window.RootViewController;

			if (viewController == null)
			{
				window = GetWindows()?
					.OrderByDescending(w => w.WindowLevel)
					.FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);

				viewController = window?.RootViewController;
			}

			while (viewController?.PresentedViewController != null)
				viewController = viewController.PresentedViewController;

			return viewController;
		}

		public UIWindow? GetCurrentUIWindow()
		{
			var window = GetKeyWindow();

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				return window;

			if (window == null)
			{
				window = GetWindows()?
					.OrderByDescending(w => w.WindowLevel)
					.FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);
			}

			return window;
		}

		static UIWindow? GetKeyWindow()
		{
			// if we have scene support, use that
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				var scenes = UIApplication.SharedApplication.ConnectedScenes;
				var windowScene = scenes.ToArray<UIWindowScene>().FirstOrDefault();
				return windowScene?.Windows.FirstOrDefault();
			}

			// use the windows property (up to 13.0)
			return UIApplication.SharedApplication.KeyWindow;
		}

		static UIWindow[]? GetWindows()
		{
			// if we have scene support, use that
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				var scenes = UIApplication.SharedApplication.ConnectedScenes;
				var windowScene = scenes.ToArray<UIWindowScene>().FirstOrDefault();
				return windowScene?.Windows;
			}

			// use the windows property (up to 15.0)
			return UIApplication.SharedApplication.Windows;
		}
	}
}
