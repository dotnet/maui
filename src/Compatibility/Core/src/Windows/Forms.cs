using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources.Core;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static partial class Forms
	{
		const string LogFormat = "[{0}] {1}";
		private static ApplicationExecutionState s_state;

		//TODO WINUI3 This is set by main page currently because
		// it's only a single window
		public static Window MainWindow { get; set; }

		public static bool IsInitialized { get; private set; }
		
		public static void Init(
			Microsoft.UI.Xaml.LaunchActivatedEventArgs launchActivatedEventArgs,
			WindowsBasePage mainWindow,
			IEnumerable<Assembly> rendererAssemblies = null)
		{
			if (IsInitialized)
				return;
			
			var accent = (WSolidColorBrush)Microsoft.UI.Xaml.Application.Current.Resources["SystemColorControlAccentBrush"];
			Color.SetAccent(accent.ToFormsColor());

#if !UWP_16299
			Log.Listeners.Add(new DelegateLogListener((c, m) => Debug.WriteLine(LogFormat, c, m)));
#else
			Log.Listeners.Add(new DelegateLogListener((c, m) => Trace.WriteLine(m, c)));
#endif
			if (!Microsoft.UI.Xaml.Application.Current.Resources.ContainsKey("RootContainerStyle"))
			{
				Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(GetTabletResources());
			}

			try
			{
				Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
			}
			catch
			{
				Log.Warning("Resources", "Unable to load WinUI resources. Try adding Microsoft.Maui.Controls.Compatibility nuget to UWP project");
			}

			Device.SetIdiom(TargetIdiom.Tablet);
			Device.SetFlowDirection(GetFlowDirection());

			Device.SetFlags(s_flags);
			Device.Info = new WindowsDeviceInfo();

			switch (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily)
			{
				case "Windows.Desktop":
					if (Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode ==
						Windows.UI.ViewManagement.UserInteractionMode.Touch)
						Device.SetIdiom(TargetIdiom.Tablet);
					else
						Device.SetIdiom(TargetIdiom.Desktop);
					break;
				case "Windows.Mobile":
					Device.SetIdiom(TargetIdiom.Phone);
					break;
				case "Windows.Xbox":
					Device.SetIdiom(TargetIdiom.TV);
					break;
				default:
					Device.SetIdiom(TargetIdiom.Unsupported);
					break;
			}

			ExpressionSearch.Default = new WindowsExpressionSearch();

			Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();
			s_state = launchActivatedEventArgs.UWPLaunchActivatedEventArgs.PreviousExecutionState;


			MainWindow = mainWindow;
			Microsoft.Maui.Controls.Compatibility.Forms.InitDispatcher(mainWindow.DispatcherQueue);
			mainWindow.LoadApplication(mainWindow.CreateApplication());
			mainWindow.Activate();
		}

#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		public static void InitDispatcher(Microsoft.System.DispatcherQueue dispatcher)
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		{
			var platformServices = new WindowsPlatformServices(dispatcher);

			Device.PlatformServices = platformServices;
			Device.PlatformInvalidator = platformServices;

			Registrar.RegisterAll(new[] { typeof(ExportRendererAttribute), typeof(ExportCellAttribute), typeof(ExportImageSourceHandlerAttribute), typeof(ExportFontAttribute) });

			IsInitialized = true;

			Platform.UWP.Platform.SubscribeAlertsAndActionSheets();
		}

		static FlowDirection GetFlowDirection()
		{
			string resourceFlowDirection = ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
			if (resourceFlowDirection == "LTR")
				return FlowDirection.LeftToRight;
			else if (resourceFlowDirection == "RTL")
				return FlowDirection.RightToLeft;

			return FlowDirection.MatchParent;
		}

		internal static Microsoft.UI.Xaml.ResourceDictionary GetTabletResources()
		{
			return new Microsoft.UI.Xaml.ResourceDictionary {
				Source = new Uri("ms-appx:///Microsoft.Maui.Controls.Compatibility.Platform.UAP/Resources.xbf")
			};
		}
	}
}
