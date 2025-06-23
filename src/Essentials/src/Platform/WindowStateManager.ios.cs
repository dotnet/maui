#nullable enable
using System;
using System.Linq;
using UIKit;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Manager object that manages window states on iOS and macOS.
	/// </summary>
	public interface IWindowStateManager
	{
		/// <summary>
		/// Initializes this <see cref="IWindowStateManager"/> instance.
		/// </summary>
		/// <param name="getCurrentUIViewController">The function task to retrieve the current <see cref="UIViewController"/>.</param>
		void Init(Func<UIViewController?>? getCurrentUIViewController);

		/// <summary>
		/// Gets the currently presented <see cref="UIViewController"/>.
		/// </summary>
		/// <returns>The <see cref="UIViewController"/> object that is currently presented.</returns>
		UIViewController? GetCurrentUIViewController();

		/// <summary>
		/// Gets the currently active <see cref="UIWindow"/>.
		/// </summary>
		/// <returns>The <see cref="UIWindow"/> object that is currently active.</returns>
		UIWindow? GetCurrentUIWindow();
	}

	/// <summary>
	/// Manager object that manages window states on iOS and macOS.
	/// </summary>
	public static class WindowStateManager
	{
		static IWindowStateManager? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IWindowStateManager Default =>
			defaultImplementation ??= new WindowStateManagerImplementation();

		internal static void SetDefault(IWindowStateManager? implementation) =>
			defaultImplementation = implementation;
	}

	static class WindowStateManagerExtensions
	{
		/// <summary>
		/// Gets the currently presented <see cref="UIViewController"/>.
		/// </summary>
		/// <param name="manager">The object to invoke this method on.</param>
		/// <param name="throwOnNull">Throws an exception if no current <see cref="UIViewController"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see langword="null"/>.</param>
		/// <returns>The <see cref="UIViewController"/> object that is currently presented.</returns>
		/// <exception cref="NullReferenceException">Thrown if no current <see cref="UIViewController"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
		public static UIViewController? GetCurrentUIViewController(this IWindowStateManager manager, bool throwOnNull)
		{
			var vc = manager.GetCurrentUIViewController();
			if (throwOnNull && vc == null)
				throw new NullReferenceException("The current view controller cannot be detected.");

			return vc;
		}

		/// <summary>
		/// Gets the currently active <see cref="UIWindow"/>.
		/// </summary>
		/// <param name="manager">The object to invoke this method on.</param>
		/// <param name="throwOnNull">Throws an exception if no current <see cref="UIWindow"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see langword="null"/>.</param>
		/// <returns>The <see cref="UIWindow"/> object that is currently active.</returns>
		/// <exception cref="NullReferenceException">Thrown if no current <see cref="UIWindow"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
		public static UIWindow? GetCurrentUIWindow(this IWindowStateManager manager, bool throwOnNull)
		{
			var window = manager.GetCurrentUIWindow();
			if (throwOnNull && window == null)
				throw new NullReferenceException("The current window cannot be detected.");

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
				try
				{
					using var scenes = UIApplication.SharedApplication.ConnectedScenes;
					var windowScene = scenes.ToArray().OfType<UIWindowScene>().FirstOrDefault(scene =>
						scene.Session.Role == UIWindowSceneSessionRole.Application);
					return windowScene?.Windows.FirstOrDefault();
				}
				catch (InvalidCastException)
				{
					// HACK: Workaround for https://github.com/xamarin/xamarin-macios/issues/13704
					//       This only throws if the collection is empty.
					return null;
				}
			}

			// use the windows property (up to 13.0)
			return UIApplication.SharedApplication.KeyWindow;
		}

		static UIWindow[]? GetWindows()
		{
			// if we have scene support, use that
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				try
				{
					using var scenes = UIApplication.SharedApplication.ConnectedScenes;
					var windowScene = scenes.ToArray().OfType<UIWindowScene>().FirstOrDefault(scene =>
						scene.Session.Role == UIWindowSceneSessionRole.Application);
					return windowScene?.Windows;
				}
				catch (InvalidCastException)
				{
					// HACK: Workaround for https://github.com/xamarin/xamarin-macios/issues/13704
					//       This only throws if the collection is empty.
					return null;
				}
			}

			// use the windows property (up to 15.0)
			return UIApplication.SharedApplication.Windows;
		}
	}
}
