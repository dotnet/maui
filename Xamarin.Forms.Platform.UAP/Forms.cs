using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

namespace Xamarin.Forms
{
	public static partial class Forms
	{
		const string LogFormat = "[{0}] {1}";

		static ApplicationExecutionState s_state;

		public static bool IsInitialized { get; private set; }
		
		public static void Init(IActivatedEventArgs launchActivatedEventArgs, IEnumerable<Assembly> rendererAssemblies = null)
		{
			if (IsInitialized)
				return;

			var accent = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["SystemColorControlAccentBrush"];
			Color.SetAccent(accent.ToFormsColor());

#if UWP_14393
			Log.Listeners.Add(new DelegateLogListener((c, m) => Debug.WriteLine(LogFormat, c, m)));
#else
			Log.Listeners.Add(new DelegateLogListener((c, m) => Trace.WriteLine(m, c)));
#endif
			if (!Windows.UI.Xaml.Application.Current.Resources.ContainsKey("RootContainerStyle"))
			{
				Windows.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(GetTabletResources());
			}

			try
			{
				Windows.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
			}
			catch
			{
				Log.Warning("Resources", "Unable to load WinUI resources. Try adding Xamarin.Forms nuget to UWP project");
			}

			Device.SetIdiom(TargetIdiom.Tablet);
			Device.SetFlowDirection(GetFlowDirection());
			Device.PlatformServices = new WindowsPlatformServices(Window.Current.Dispatcher);
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

			Registrar.RegisterAll(new[] { typeof(ExportRendererAttribute), typeof(ExportCellAttribute), typeof(ExportImageSourceHandlerAttribute), typeof(ExportFontAttribute) });

			IsInitialized = true;
			s_state = launchActivatedEventArgs.PreviousExecutionState;

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

		internal static Windows.UI.Xaml.ResourceDictionary GetTabletResources()
		{
			return new Windows.UI.Xaml.ResourceDictionary {
				Source = new Uri("ms-appx:///Xamarin.Forms.Platform.UAP/Resources.xbf")
			};
		}
	}
}
