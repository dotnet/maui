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
		private static ApplicationExecutionState s_state;

		//TODO WINUI3 This is set by main page currently because
		// it's only a single window
		public static UI.Xaml.Window MainWindow { get; set; }

		public static bool IsInitialized { get; private set; }
		public static IMauiContext MauiContext { get; private set; }

		public static void Init(IActivationState state)
		{
			SetupInit(state.Context, state.LaunchActivatedEventArgs, null, null);
		}

		public static void Init(
			UI.Xaml.LaunchActivatedEventArgs launchActivatedEventArgs,
			WindowsBasePage mainWindow,
			IEnumerable<Assembly> rendererAssemblies = null)
		{
			SetupInit(new MauiContext(), launchActivatedEventArgs, mainWindow, rendererAssemblies);
		}

		public static void Init(InitializationOptions options) =>
			SetupInit(new MauiContext(), options.LaunchActivatedEventArgs, null, null, options);

		static void SetupInit(
			IMauiContext mauiContext,
			UI.Xaml.LaunchActivatedEventArgs launchActivatedEventArgs,
			WindowsBasePage mainWindow,
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

			if (!UI.Xaml.Application.Current.Resources.ContainsKey("RootContainerStyle"))
			{
				UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(GetTabletResources());
			}

			try
			{
				UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(new UI.Xaml.Controls.XamlControlsResources());
			}
			catch
			{
				Log.Warning("Resources", "Unable to load WinUI resources. Try adding Microsoft.Maui.Controls.Compatibility nuget to UWP project");
			}

			Device.SetIdiom(TargetIdiom.Tablet);
			Device.SetFlowDirection(GetFlowDirection());

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
			s_state = launchActivatedEventArgs.UWPLaunchActivatedEventArgs.PreviousExecutionState;

			var dispatcher = mainWindow?.DispatcherQueue ?? System.DispatcherQueue.GetForCurrentThread();

			var platformServices = new WindowsPlatformServices(dispatcher);

			Device.PlatformServices = platformServices;
			Device.PlatformInvalidator = platformServices;

			if (maybeOptions?.Flags.HasFlag(InitializationFlags.SkipRenderers) != true)
				RegisterCompatRenderers();

			if (mainWindow != null)
			{
				MainWindow = mainWindow;

				//TODO WINUI3
				Platform.UWP.Platform.SubscribeAlertsAndActionSheets();

				mainWindow.LoadApplication(mainWindow.CreateApplication());
				mainWindow.Activate();
			}

			IsInitialized = true;
		}

		static bool IsInitializedRenderers;

		internal static void RegisterCompatRenderers()
		{
			if (IsInitializedRenderers)
				return;

			IsInitializedRenderers = true;

			// Only need to do this once
			Registrar.RegisterAll(new[]
			{
				typeof(ExportRendererAttribute),
				typeof(ExportCellAttribute),
				typeof(ExportImageSourceHandlerAttribute),
				typeof(ExportFontAttribute)
			});
		}

		internal static void RegisterCompatRenderers(
			Assembly[] assemblies,
			Assembly defaultRendererAssembly,
			Action<Type> viewRegistered)
		{
			if (IsInitializedRenderers)
				return;

			IsInitializedRenderers = true;

			// Only need to do this once
			Controls.Internals.Registrar.RegisterAll(
				assemblies,
				defaultRendererAssembly,
				new[] {
						typeof(ExportRendererAttribute),
						typeof(ExportCellAttribute),
						typeof(ExportImageSourceHandlerAttribute),
						typeof(ExportFontAttribute)
					}, default(InitializationFlags),
				viewRegistered);
		}

		static FlowDirection GetFlowDirection()
		{
			string resourceFlowDirection = "LTR"; //TODO WINUI3 ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
			if (resourceFlowDirection == "LTR")
				return FlowDirection.LeftToRight;
			else if (resourceFlowDirection == "RTL")
				return FlowDirection.RightToLeft;

			return FlowDirection.MatchParent;
		}

		internal static UI.Xaml.ResourceDictionary GetTabletResources()
		{
			var dict = new UI.Xaml.ResourceDictionary
			{
				Source = new Uri("ms-appx:///Microsoft.Maui.Controls.Compatibility/WinUI/Resources.xbf")
			};

			return dict;
		}
	}
}