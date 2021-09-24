using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility
{
	public struct InitializationOptions
	{
		public InitializationOptions(UI.Xaml.LaunchActivatedEventArgs args)
		{
			this = default(InitializationOptions);
			LaunchActivatedEventArgs = args;
		}
		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs;
		public InitializationFlags Flags;
	}

	public static partial class Forms
	{
		const string LogFormat = "[{0}] {1}";

		//TODO WINUI3 This is set by main page currently because
		// it's only a single window
		public static UI.Xaml.Window MainWindow { get; set; }

		public static bool IsInitialized { get; private set; }

		public static IMauiContext MauiContext { get; private set; }

		public static void Init(IActivationState state, InitializationOptions? options = null)
		{
			SetupInit(state.Context, state.Context.Window, maybeOptions: options);
		}

		public static void Init(
			UI.Xaml.Window mainWindow,
			IEnumerable<Assembly> rendererAssemblies = null)
		{
			SetupInit(new MauiContext(), mainWindow, rendererAssemblies);
		}

		public static void Init(InitializationOptions options) =>
			SetupInit(new MauiContext(), null, null, options);

		static void SetupInit(
			IMauiContext mauiContext,
			UI.Xaml.Window mainWindow,
			IEnumerable<Assembly> rendererAssemblies = null,
			InitializationOptions? maybeOptions = null)
		{
			MauiContext = mauiContext;
			Registrar.RegisterRendererToHandlerShim(RendererToHandlerShim.CreateShim);

			var accent = (WSolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["SystemColorControlAccentBrush"];
			KnownColor.SetAccent(accent.ToColor());

			if (!IsInitialized)
			{
				Log.Listeners.Add(new DelegateLogListener((c, m) => Debug.WriteLine(LogFormat, c, m)));

			}

			Device.SetIdiom(TargetIdiom.Tablet);
			Device.SetFlowDirection(mauiContext.GetFlowDirection());

			Device.SetFlags(s_flags);
			Device.Info = new WindowsDeviceInfo();

			//TODO WINUI3
			//// use field and not property to avoid exception in getter
			//if (Device.info != null)
			//{
			//	Device.info.Dispose();
			//	Device.info = null;
			//}
			//Device.Info = new WindowsDeviceInfo();

			//TODO WINUI3
			//switch (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily)
			//{
			//	case "Windows.Desktop":
			//		if (Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode ==
			//			Windows.UI.ViewManagement.UserInteractionMode.Touch)
			//			Device.SetIdiom(TargetIdiom.Tablet);
			//		else
			//			Device.SetIdiom(TargetIdiom.Desktop);
			//		break;
			//	case "Windows.Mobile":
			//		Device.SetIdiom(TargetIdiom.Phone);
			//		break;
			//	case "Windows.Xbox":
			//		Device.SetIdiom(TargetIdiom.TV);
			//		break;
			//	default:
			//		Device.SetIdiom(TargetIdiom.Unsupported);
			//		break;
			//}

			ExpressionSearch.Default = new WindowsExpressionSearch();

			Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();

			var dispatcher = mainWindow?.DispatcherQueue ?? UI.Dispatching.DispatcherQueue.GetForCurrentThread();

			var platformServices = new WindowsPlatformServices(dispatcher);

			Device.PlatformServices = platformServices;
			Device.PlatformInvalidator = platformServices;

			if (mainWindow != null)
			{
				MainWindow = mainWindow;

				//if (mainWindow is WindowsBasePage windowsPage)
				//{
				//	windowsPage.LoadApplication(windowsPage.CreateApplication());
				//	windowsPage.Activate();
				//}
			}

			IsInitialized = true;
		}
	}
}