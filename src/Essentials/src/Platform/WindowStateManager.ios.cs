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

	class WindowStateManagerImplementation : IWindowStateManager
	{
		Func<UIViewController?>? getCurrentController;

		public void Init(Func<UIViewController?>? getCurrentUIViewController) =>
			getCurrentController = getCurrentUIViewController;

		public UIViewController? GetCurrentUIViewController() =>
			GetCurrentViewController(false);

		public UIWindow? GetCurrentUIWindow() =>
			GetCurrentWindow(false);

		internal UIViewController? GetCurrentViewController(bool throwIfNull = true)
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

				if (window == null && throwIfNull)
					throw new InvalidOperationException("Could not find current view controller.");
				else
					viewController = window?.RootViewController;
			}

			while (viewController?.PresentedViewController != null)
				viewController = viewController.PresentedViewController;

			if (throwIfNull && viewController == null)
				throw new InvalidOperationException("Could not find current view controller.");

			return viewController;
		}

		internal UIWindow? GetCurrentWindow(bool throwIfNull = true)
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

			if (throwIfNull && window == null)
				throw new InvalidOperationException("Could not find current window.");

			return window;
		}
	}
}
