using System;

namespace Microsoft.Maui.Essentials
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
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppInfo']/Docs" />
	public static class AppInfo
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='PackageName']/Docs" />
		public static string PackageName => Default.PackageName;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='Name']/Docs" />
		public static string Name => Default.Name;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='VersionString']/Docs" />
		public static string VersionString => Default.VersionString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='Version']/Docs" />
		public static Version Version => Default.Version;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='BuildString']/Docs" />
		public static string BuildString => Default.BuildString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='ShowSettingsUI']/Docs" />
		public static void ShowSettingsUI() => Default.ShowSettingsUI();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='RequestedTheme']/Docs" />
		public static AppTheme RequestedTheme => Default.RequestedTheme;


		static Lazy<IAppInfo> current = new Lazy<IAppInfo>(() => new AppInfoImplementation());

		public static IAppInfo Current => current.Value;
	}
}
