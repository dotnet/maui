using System;
using System.Reflection;

namespace Microsoft.Maui.ApplicationModel
{

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppInfo']/Docs" />
	class AppInfoImplementation : IAppInfo
	{

		static readonly Assembly _launchingAssembly = Assembly.GetEntryAssembly();

		public string PackageName => _launchingAssembly.GetAppInfoValue("PackageName") ?? _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

		public string Name => throw ExceptionUtils.NotSupportedOrImplementedException;

		public System.Version Version => Utils.ParseVersion(VersionString);

		public string VersionString => throw ExceptionUtils.NotSupportedOrImplementedException;

		public string BuildString => throw ExceptionUtils.NotSupportedOrImplementedException;

		public void ShowSettingsUI() => throw ExceptionUtils.NotSupportedOrImplementedException;

		public AppTheme RequestedTheme => AppTheme.Unspecified;

		public AppPackagingModel PackagingModel => AppPackagingModel.Unpackaged;

		// Returning the Unknown value for LayoutDirection so that unit tests can work
		public LayoutDirection RequestedLayoutDirection => LayoutDirection.Unknown;

		internal static string PublisherName => _launchingAssembly.GetAppInfoValue("PublisherName") ?? _launchingAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;

	}

	static class AppInfoUtils
	{

		static readonly Lazy<bool> _isPackagedAppLazy = new Lazy<bool>(() =>
		{

			return false;
		});

		public static bool IsPackagedApp => _isPackagedAppLazy.Value;

		public static Version GetAppInfoVersionValue(this Assembly assembly, string name)
		{
			if (assembly.GetAppInfoValue(name) is string value && !string.IsNullOrEmpty(value))
				return Version.Parse(value);

			return null;
		}

		public static string GetAppInfoValue(this Assembly assembly, string name) =>
			assembly.GetMetadataAttributeValue("Microsoft.Maui.ApplicationModel.AppInfo." + name);

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