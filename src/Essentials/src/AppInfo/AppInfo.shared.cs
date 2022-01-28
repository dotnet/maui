using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppInfo']/Docs" />
	public static partial class AppInfo
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='PackageName']/Docs" />
		public static string PackageName => PlatformGetPackageName();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='Name']/Docs" />
		public static string Name => PlatformGetName();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='VersionString']/Docs" />
		public static string VersionString => PlatformGetVersionString();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='Version']/Docs" />
		public static Version Version => Utils.ParseVersion(VersionString);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='BuildString']/Docs" />
		public static string BuildString => PlatformGetBuild();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='ShowSettingsUI']/Docs" />
		public static void ShowSettingsUI() => PlatformShowSettingsUI();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='RequestedTheme']/Docs" />
		public static AppTheme RequestedTheme => PlatformRequestedTheme();
	}
}
