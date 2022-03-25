#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="Type[@FullName='Microsoft.Maui.Essentials.VersionTracking']/Docs" />
	public static partial class VersionTracking
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='Track']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static void Track()
			=> Default.Track();

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchEver']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static bool IsFirstLaunchEver
			=> Default.IsFirstLaunchEver;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentVersion']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static bool IsFirstLaunchForCurrentVersion
			=> Default.IsFirstLaunchForCurrentVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentBuild']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static bool IsFirstLaunchForCurrentBuild
			=> Default.IsFirstLaunchForCurrentBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentVersion']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static string CurrentVersion
			=> Default.CurrentVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentBuild']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static string CurrentBuild
			=> Default.CurrentBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousVersion']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static string? PreviousVersion
			=> Default.PreviousVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousBuild']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static string? PreviousBuild
			=> Default.PreviousBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledVersion']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static string? FirstInstalledVersion
			=> Default.FirstInstalledVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledBuild']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static string? FirstInstalledBuild
			=> Default.FirstInstalledBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='VersionHistory']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static IEnumerable<string> VersionHistory
			=> Default.VersionHistory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='BuildHistory']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static IEnumerable<string> BuildHistory
			=> Default.BuildHistory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForVersion']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static bool IsFirstLaunchForVersion(string version)
			=> Default.IsFirstLaunchForVersion(version);

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForBuild']/Docs" />
		[Obsolete($"Use {nameof(VersionTracking)}.{nameof(Default)} instead.", true)]
		public static bool IsFirstLaunchForBuild(string build)
			=> Default.IsFirstLaunchForBuild(build);
	}
}
