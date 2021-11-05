using System;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using Tizen.Applications;
using EWindow = ElmSharp.Window;

namespace Microsoft.Maui
{
	internal static class CoreUIAppExtensions
	{
		public static IWindow GetWindow(this CoreUIApplication application)
		{
			var nativeWindow = CoreUIAppContext.GetInstance(application)?.MainWindow;

			foreach (var window in MauiApplication.Current.Application.Windows)
			{
				if (window?.Handler?.NativeView is EWindow win && win == nativeWindow)
					return window;
			}

			throw new InvalidOperationException("Window Not Found");
		}

		public static void RequestNewWindow(this CoreUIApplication nativeApplication, IApplication application, OpenWindowRequest? args)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var state = args?.State;
			var bundle = state.ToBundle();

			//TODO : Need to implementation
		}

		public static void CreateNativeWindow(this CoreUIApplication nativeApplication, IApplication application)
		{
			if (application.Handler?.MauiContext is not IMauiContext applicationContext)
				return;

			var context = CoreUIAppContext.GetInstance(nativeApplication);
			var mauiContext = applicationContext.MakeScoped(context);

			applicationContext.Services.InvokeLifecycleEvents<TizenLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var tizenWindow = mauiContext.Context?.MainWindow;

			if (tizenWindow == null)
				throw new InvalidOperationException($"The {nameof(tizenWindow)} instance was not found.");

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
	}
}
