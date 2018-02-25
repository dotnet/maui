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

			Log.Listeners.Add(new DelegateLogListener((c, m) => Debug.WriteLine(LogFormat, c, m)));

			Windows.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(GetTabletResources());

			Device.SetIdiom(TargetIdiom.Tablet);
			Device.SetFlowDirection(GetFlowDirection());
			Device.PlatformServices = new WindowsPlatformServices(Window.Current.Dispatcher);
			Device.SetFlags(s_flags);
			Device.Info = new WindowsDeviceInfo();

			switch (DetectPlatform())
			{
				case Windows.Foundation.Metadata.Platform.Windows:
					Device.SetIdiom(TargetIdiom.Desktop);
					break;
				case Windows.Foundation.Metadata.Platform.WindowsPhone:
					Device.SetIdiom(TargetIdiom.Phone);
					break;
				default:
					Device.SetIdiom(TargetIdiom.Tablet);
					break;
			}
			ExpressionSearch.Default = new WindowsExpressionSearch();

			Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();

			Registrar.RegisterAll(new[] { typeof(ExportRendererAttribute), typeof(ExportCellAttribute), typeof(ExportImageSourceHandlerAttribute) });

			IsInitialized = true;
			s_state = launchActivatedEventArgs.PreviousExecutionState;

			SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
			Platform.UWP.Platform.SubscribeAlertsAndActionSheets();
		}

		static Windows.Foundation.Metadata.Platform DetectPlatform()
		{
			bool isHardwareButtonsAPIPresent = ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

			if (isHardwareButtonsAPIPresent)
				return Windows.Foundation.Metadata.Platform.WindowsPhone;
			return Windows.Foundation.Metadata.Platform.Windows;
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

		static Windows.UI.Xaml.ResourceDictionary GetTabletResources()
		{
			return new Windows.UI.Xaml.ResourceDictionary {
				Source = new Uri("ms-appx:///Xamarin.Forms.Platform.UAP/Resources.xbf")
			};
		}

		static void OnBackRequested(object sender, BackRequestedEventArgs e)
		{
			Application app = Application.Current;
			if (app == null)
				return;

			Page page = app.MainPage;
			if (page == null)
				return;

			var platform = page.Platform as Platform.UWP.Platform;
			if (platform == null)
				return;

			e.Handled = platform.BackButtonPressed();
		}
	}
}