#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="Type[@FullName='Microsoft.Maui.Essentials.VersionTracking']/Docs" />
	public static class VersionTracking
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='Track']/Docs" />
		public static void Track()
			=> Current.Track();

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchEver']/Docs" />
		public static bool IsFirstLaunchEver
			=> Current.IsFirstLaunchEver;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentVersion']/Docs" />
		public static bool IsFirstLaunchForCurrentVersion
			=> Current.IsFirstLaunchForCurrentVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentBuild']/Docs" />
		public static bool IsFirstLaunchForCurrentBuild
			=> Current.IsFirstLaunchForCurrentBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentVersion']/Docs" />
		public static string CurrentVersion
			=> Current.CurrentVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentBuild']/Docs" />
		public static string CurrentBuild
			=> Current.CurrentBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousVersion']/Docs" />
		public static string? PreviousVersion
			=> Current.PreviousVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousBuild']/Docs" />
		public static string? PreviousBuild
			=> Current.PreviousBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledVersion']/Docs" />
		public static string? FirstInstalledVersion
			=> Current.FirstInstalledVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledBuild']/Docs" />
		public static string? FirstInstalledBuild =>
			Current.FirstInstalledBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='VersionHistory']/Docs" />
		public static IEnumerable<string> VersionHistory
			=> Current.VersionHistory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='BuildHistory']/Docs" />
		public static IEnumerable<string> BuildHistory
			=> Current.BuildHistory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForVersion']/Docs" />
		public static bool IsFirstLaunchForVersion(string version)
			=> Current.IsFirstLaunchForVersion(version);

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForBuild']/Docs" />
		public static bool IsFirstLaunchForBuild(string build)
			=> Current.IsFirstLaunchForBuild(build);

		static IVersionTracking Current => ApplicationModel.VersionTracking.Current;
	}
}
