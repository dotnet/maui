#if WINDOWS_UWP
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WindowActivationState = Windows.UI.Core.CoreWindowActivationState;
#elif WINDOWS
using Microsoft.UI.Xaml;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class Platform
	{
		internal static Window CurrentWindow
		{
			get => _currentWindow ?? Window.Current;
			set => _currentWindow = value;
		}

		internal const string AppManifestFilename = "AppxManifest.xml";
		internal const string AppManifestXmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
		internal const string AppManifestUapXmlns = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
		private static Window _currentWindow;

		public static string MapServiceToken { get; set; }

		public static async void OnLaunched(LaunchActivatedEventArgs e)
			=> await AppActions.OnLaunched(e);

		public static void OnActivated(Window window, WindowActivatedEventArgs args)
		{
			if (args.WindowActivationState != WindowActivationState.Deactivated)
				CurrentWindow = window;
		}
	}
}
