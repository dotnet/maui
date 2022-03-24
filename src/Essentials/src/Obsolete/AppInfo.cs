#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppInfo']/Docs" />
	public static partial class AppInfo
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='PackageName']/Docs" />
		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static string PackageName => Current.PackageName;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='Name']/Docs" />
		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static string Name => Current.Name;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='VersionString']/Docs" />
		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static string VersionString => Current.VersionString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='Version']/Docs" />
		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static Version Version => Current.Version;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='BuildString']/Docs" />
		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static string BuildString => Current.BuildString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='ShowSettingsUI']/Docs" />
		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static void ShowSettingsUI() => Current.ShowSettingsUI();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AppInfo.xml" path="//Member[@MemberName='RequestedTheme']/Docs" />
		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static AppTheme RequestedTheme => Current.RequestedTheme;

		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static AppPackagingModel PackagingModel => Current.PackagingModel;

		[Obsolete($"Use {nameof(AppInfo)}.{nameof(Current)} instead.", true)]
		public static LayoutDirection RequestedLayoutDirection => Current.RequestedLayoutDirection;
	}
}