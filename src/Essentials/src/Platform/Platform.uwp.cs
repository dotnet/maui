#if WINDOWS_UWP
using Windows.ApplicationModel.Activation;
#elif WINDOWS
using Microsoft.UI.Xaml;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class Platform
	{
		internal const string AppManifestFilename = "AppxManifest.xml";
		internal const string AppManifestXmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
		internal const string AppManifestUapXmlns = "http://schemas.microsoft.com/appx/manifest/uap/windows10";

		public static string MapServiceToken { get; set; }

		public static async void OnLaunched(LaunchActivatedEventArgs e)
			=> await AppActions.OnLaunched(e);
	}
}
