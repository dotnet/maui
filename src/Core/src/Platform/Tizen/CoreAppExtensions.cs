using System;
using System.Reflection;
using ElmSharp;
using Microsoft.Maui.LifecycleEvents;
using Tizen.Applications;
using EWindow = ElmSharp.Window;

namespace Microsoft.Maui.Platform
{
	internal static class CoreAppExtensions
	{
		public static Window? MainWindow { get; set; }

		public static IWindow GetWindow(this CoreApplication application)
		{
			foreach (var window in MauiApplication.Current.Application.Windows)
			{
				if (window?.Handler?.PlatformView is EWindow win && win == MainWindow)
					return window;
			}

			throw new InvalidOperationException("Window Not Found");
		}

		public static void RequestNewWindow(this CoreApplication platformApplication, IApplication application, OpenWindowRequest? args)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var state = args?.State;
			var bundle = state.ToBundle();

			//TODO : Need to implementation
		}

		public static void CreateNativeWindow(this CoreApplication platformApplication, IApplication application)
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
					userInfo.AddItem(pair.Key, pair.Value);
				}
			}

			return userInfo;
		}

		public static EWindow GetDefaultWindow()
		{
			if (MainWindow != null)
				return MainWindow;

			return MainWindow = GetPreloadedWindow() ?? new EWindow("MauiDefaultWindow");
		}

		static EWindow? GetPreloadedWindow()
		{
			var type = typeof(EWindow);
			// Use reflection to avoid breaking compatibility. ElmSharp.Window.CreateWindow() is has been added since API6.
			var methodInfo = type.GetMethod("CreateWindow", BindingFlags.NonPublic | BindingFlags.Static);

			return (EWindow?)methodInfo?.Invoke(null, new object[] { "FormsWindow" });
		}
	}
}
