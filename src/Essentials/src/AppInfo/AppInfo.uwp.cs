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
	class AppInfoImplementation : IAppInfo
	{
		ApplicationTheme? _applicationTheme;
		public AppInfoImplementation()
		{
			if (MainThread.IsMainThread && Application.Current != null)
				_applicationTheme = Application.Current.RequestedTheme;
		}

		public string PackageName => Package.Current.Id.Name;

		public string Name => Package.Current.DisplayName;

		public Version Version => Utils.ParseVersion(VersionString);

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

		internal void ThemeChanged() =>
			_applicationTheme = Application.Current.RequestedTheme;

		public AppTheme RequestedTheme
		{
			get
			{
				if (MainThread.IsMainThread && Application.Current != null)
					_applicationTheme = Application.Current.RequestedTheme;
				else if (_applicationTheme == null)
					return AppTheme.Unspecified;

				return _applicationTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
			}
		}

		public AppPackagingModel PackagingModel => AppInfoUtils.IsPackagedApp
			? AppPackagingModel.Packaged
			: AppPackagingModel.Unpackaged;

		public LayoutDirection RequestedLayoutDirection =>
			CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? LayoutDirection.RightToLeft : LayoutDirection.LeftToRight;
	}

	static class AppInfoUtils
	{
		static readonly Lazy<bool> _isPackagedAppLazy = new Lazy<bool>(() =>
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

		public static bool IsPackagedApp => _isPackagedAppLazy.Value;
	}
}
