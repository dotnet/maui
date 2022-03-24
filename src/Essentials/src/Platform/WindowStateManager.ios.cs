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

			var window = UIApplication.SharedApplication.KeyWindow;

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				viewController = window.RootViewController;

			if (viewController == null)
			{
				window = UIApplication.SharedApplication
					.Windows
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
			var window = UIApplication.SharedApplication.KeyWindow;

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				return window;

			if (window == null)
			{
				window = UIApplication.SharedApplication
					.Windows
					.OrderByDescending(w => w.WindowLevel)
					.FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);
			}

			return window;
		}
	}
}
