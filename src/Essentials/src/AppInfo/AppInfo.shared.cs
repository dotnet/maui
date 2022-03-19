#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IAppInfo
	{
		string PackageName { get; }

		string Name { get; }

		string VersionString { get; }

		Version Version { get; }

		string BuildString { get; }

		void ShowSettingsUI();

		AppTheme RequestedTheme { get; }

		AppPackagingModel PackagingModel { get; }

		LayoutDirection RequestedLayoutDirection { get; }
	}

	public static class AppInfo
	{
		static IAppInfo? currentImplementation;

		public static IAppInfo Current =>
			currentImplementation ??= new AppInfoImplementation();

		internal static void SetCurrent(IAppInfo? implementation) =>
			currentImplementation = implementation;
	}

	public enum AppPackagingModel
	{
		Packaged,
		Unpackaged,
	}
}
