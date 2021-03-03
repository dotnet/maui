using System;

namespace Microsoft.Maui.Essentials
{
	public static partial class AppInfo
	{
		public static string PackageName => PlatformGetPackageName();

		public static string Name => PlatformGetName();

		public static string VersionString => PlatformGetVersionString();

		public static Version Version => Utils.ParseVersion(VersionString);

		public static string BuildString => PlatformGetBuild();

		public static void ShowSettingsUI() => PlatformShowSettingsUI();

		public static AppTheme RequestedTheme => PlatformRequestedTheme();
	}
}
