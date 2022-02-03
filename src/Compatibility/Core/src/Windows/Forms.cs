using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;

using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
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
		//TODO WINUI3 This is set by main page currently because
		// it's only a single window
		public static UI.Xaml.Window MainWindow { get; set; }

		public static bool IsInitialized { get; private set; }

		public static IMauiContext MauiContext { get; private set; }

		public static void Init(IActivationState state, InitializationOptions? options = null)
		{
			SetupInit(state.Context, state.Context.GetOptionalNativeWindow(), maybeOptions: options);
		}

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

			TargetIdiom currentIdiom = TargetIdiom.Unsupported;

			switch (AnalyticsInfo.VersionInfo.DeviceFamily)
			{
				case "Windows.Mobile":
					currentIdiom = TargetIdiom.Phone;
					break;
				case "Windows.Universal":
				case "Windows.Desktop":
					{
						try
						{
							if (mainWindow != null)
							{
								var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
								var settings = UIViewSettingsInterop.GetForWindow(windowHandle);

								var uiMode = settings.UserInteractionMode;
								currentIdiom = uiMode == UserInteractionMode.Mouse ? TargetIdiom.Desktop : TargetIdiom.Tablet;
							}
						}
						catch (Exception ex)
						{
							Debug.WriteLine($"Unable to get device . {ex.Message}");
						}
					}
					break;
				case "Windows.Xbox":
				case "Windows.Team":
					currentIdiom = TargetIdiom.TV;
					break;
				case "Windows.IoT":
				default:
					currentIdiom = TargetIdiom.Unsupported;
					break;
			}

			Device.SetIdiom(currentIdiom);

			Device.SetFlowDirection(mauiContext.GetFlowDirection());

			ExpressionSearch.Default = new WindowsExpressionSearch();

			Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();

			var platformServices = new WindowsPlatformServices();

			Device.PlatformServices = platformServices;
			Device.PlatformInvalidator = platformServices;

			if (mainWindow != null)
			{
				MainWindow = mainWindow;
			}

			IsInitialized = true;
		}
	}
}