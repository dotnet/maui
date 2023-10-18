using System;
using Microsoft.Maui.LifecycleEvents;
using Tizen.Applications;
using Tizen.NUI;

namespace Microsoft.Maui.Platform
{
	internal static class CoreAppExtensions
	{
		public static IWindow GetWindow(this CoreApplication application)
		{
			foreach (var window in IPlatformApplication.Current?.Application?.Windows ?? Array.Empty<IWindow>())
			{
				if (window?.Handler?.PlatformView is Window win && win == GetDefaultWindow())
					return window;
			}

			throw new InvalidOperationException("Window Not Found");
		}

		public static IWindow? GetWindow(this Window? platformWindow)
		{
			if (platformWindow == null)
				return null;

			foreach (var window in IPlatformApplication.Current?.Application?.Windows ?? Array.Empty<IWindow>())
			{
				if (window?.Handler?.PlatformView is Window win && win == platformWindow)
					return window;
			}

			return null;
		}

		public static void RequestNewWindow(this CoreApplication platformApplication, IApplication application, OpenWindowRequest? args)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var state = args?.State;
			var bundle = state.ToBundle();

			//TODO : Need to implementation
		}

		public static void CreatePlatformWindow(this CoreApplication platformApplication, IApplication application)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var tizenWindow = GetDefaultWindow();
			if (tizenWindow == null)
				throw new InvalidOperationException($"The {nameof(tizenWindow)} instance was not found.");

			var mauiContext = applicationContext.MakeWindowScope(tizenWindow, out var windowScope);

			tizenWindow.SetWindowCloseRequestHandler(() => platformApplication.Exit());

			applicationContext.Services.InvokeLifecycleEvents<TizenLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext);
			var window = application.CreateWindow(activationState);

			tizenWindow.SetWindowHandler(window, mauiContext);
		}

		public static Bundle ToBundle(this IPersistedState? state)
		{
			var userInfo = new Bundle();

			var keyset = userInfo.Keys;
			if (keyset != null)
			{
				foreach (var k in keyset)
				{
					userInfo?.GetItem<string>(k);
				}
			}

			if (state is not null)
			{
				foreach (var pair in state)
				{
					userInfo?.AddItem(pair.Key, pair.Value);
				}
			}

			return userInfo!;
		}

		public static Window GetDefaultWindow()
		{
			return Window.Instance;
		}
	}
}
