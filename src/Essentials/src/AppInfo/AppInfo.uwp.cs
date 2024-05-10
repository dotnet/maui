using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace Microsoft.Maui.ApplicationModel
{
	class AppInfoImplementation : IAppInfo
	{
		static readonly Assembly _launchingAssembly = Assembly.GetEntryAssembly();

		const string SettingsUri = "ms-settings:appsfeatures-app";

		ApplicationTheme? _applicationTheme;

		readonly ActiveWindowTracker _activeWindowTracker;

		/// <summary>
		/// Intializes a new <see cref="AppInfoImplementation"/> object with default values.
		/// </summary>
		public AppInfoImplementation()
		{
			_activeWindowTracker = new(WindowStateManager.Default);
			_activeWindowTracker.Start();
			_activeWindowTracker.WindowMessage += OnWindowMessage;

			if (MainThread.IsMainThread)
				OnActiveWindowThemeChanged();
		}

		public string PackageName => AppInfoUtils.IsPackagedApp
			? Package.Current.Id.Name
			: _launchingAssembly.GetAppInfoValue("PackageName") ?? _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

		public static string PublisherName => AppInfoUtils.IsPackagedApp
			? Package.Current.PublisherDisplayName
			: _launchingAssembly.GetAppInfoValue("PublisherName") ?? _launchingAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;

		public string Name => AppInfoUtils.IsPackagedApp
			? Package.Current.DisplayName
			: _launchingAssembly.GetAppInfoValue("Name") ?? _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

		public Version Version => AppInfoUtils.IsPackagedApp
			? Package.Current.Id.Version.ToVersion()
			: _launchingAssembly.GetAppInfoVersionValue("Version") ?? _launchingAssembly.GetName().Version;

		public string VersionString => Version.ToString();

		public string BuildString => Version.Revision.ToString(CultureInfo.InvariantCulture);

		public void ShowSettingsUI()
		{
			if (AppInfoUtils.IsPackagedApp)
				global::Windows.System.Launcher.LaunchUriAsync(new Uri(SettingsUri)).WatchForError();
			else
				Process.Start(new ProcessStartInfo { FileName = SettingsUri, UseShellExecute = true });
		}

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

		void OnWindowMessage(object sender, WindowMessageEventArgs e)
		{
			if (e.MessageId == PlatformMethods.MessageIds.WM_SETTINGCHANGE ||
				e.MessageId == PlatformMethods.MessageIds.WM_THEMECHANGE)
				OnActiveWindowThemeChanged();
		}

		void OnActiveWindowThemeChanged()
		{
			if (Application.Current is Application app)
				_applicationTheme = app.RequestedTheme;
		}
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

		/// <summary>
		/// Gets if this app is a packaged app.
		/// </summary>
		public static bool IsPackagedApp => _isPackagedAppLazy.Value;

		static readonly Lazy<string> platformGetFullAppPackageFilePath = new Lazy<string>(() =>
		{
			return IsPackagedApp
				? Package.Current.InstalledLocation.Path
				: AppContext.BaseDirectory;
		});

		/// <summary>
		/// Gets full application path.
		/// </summary>
		public static string PlatformGetFullAppPackageFilePath => platformGetFullAppPackageFilePath.Value;

		/// <summary>
		/// Converts a <see cref="PackageVersion"/> object to a <see cref="Version"/> object.
		/// </summary>
		/// <param name="version">The <see cref="PackageVersion"/> to convert.</param>
		/// <returns>A new <see cref="Version"/> object with the version information of this app.</returns>
		public static Version ToVersion(this PackageVersion version) =>
			new Version(version.Major, version.Minor, version.Build, version.Revision);

		/// <summary>
		/// Gets the version information for this app.
		/// </summary>
		/// <param name="assembly">The assembly to retrieve the version information for.</param>
		/// <param name="name">The key that is used to retrieve the version information from the metadata.</param>
		/// <returns><see langword="null"/> if <paramref name="name"/> is <see langword="null"/> or empty, or no version information could be found with the value of <paramref name="name"/>.</returns>
		public static Version GetAppInfoVersionValue(this Assembly assembly, string name)
		{
			if (assembly.GetAppInfoValue(name) is string value && !string.IsNullOrEmpty(value))
				return Version.Parse(value);

			return null;
		}

		/// <summary>
		/// Gets the app info from this apps' metadata.
		/// </summary>
		/// <param name="assembly">The assembly to retrieve the app info for.</param>
		/// <param name="name">The key of the metadata to be retrieved (e.g. PackageName, PublisherName or Name).</param>
		/// <returns>The value that corresponds to the given key in <paramref name="name"/>.</returns>
		public static string GetAppInfoValue(this Assembly assembly, string name) =>
			assembly.GetMetadataAttributeValue("Microsoft.Maui.ApplicationModel.AppInfo." + name);

		/// <summary>
		/// Gets the value for a given key from the assembly metadata.
		/// </summary>
		/// <param name="assembly">The assembly to retrieve the information for.</param>
		/// <param name="key">The key of the metadata to be retrieved (e.g. PackageName, PublisherName or Name).</param>
		/// <returns>The value that corresponds to the given key in <paramref name="key"/>.</returns>
		public static string GetMetadataAttributeValue(this Assembly assembly, string key)
		{
			foreach (var attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
			{
				if (attr.Key == key)
					return attr.Value;
			}

			return null;
		}
	}
}
