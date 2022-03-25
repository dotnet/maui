using System;
using System.Globalization;
using Windows.ApplicationModel;
#if WINDOWS
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.Maui.ApplicationModel
{
	public class AppInfoImplementation : IAppInfo
	{
		static Lazy<bool> _isPackagedAppLazy = new Lazy<bool>(() =>
		{
			try
			{
				if (Package.Current != null)
					return true;
			}
			catch
			{
				// no-op
			}

			return false;
		});

		public string PackageName => Package.Current.Id.Name;

		public string Name => Package.Current.DisplayName;

		public System.Version Version => Utils.ParseVersion(VersionString);

		public string VersionString
		{
			get
			{
				var version = Package.Current.Id.Version;
				return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
			}
		}

		public string BuildString =>
			Package.Current.Id.Version.Build.ToString(CultureInfo.InvariantCulture);

		public void ShowSettingsUI() =>
			global::Windows.System.Launcher.LaunchUriAsync(new global::System.Uri("ms-settings:appsfeatures-app")).WatchForError();

		public AppTheme RequestedTheme =>
			Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;

		public AppPackagingModel PackagingModel => _isPackagedAppLazy.Value
			? AppPackagingModel.Packaged
			: AppPackagingModel.Unpackaged;

		public LayoutDirection RequestedLayoutDirection =>
			CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? LayoutDirection.RightToLeft : LayoutDirection.LeftToRight;
	}
}