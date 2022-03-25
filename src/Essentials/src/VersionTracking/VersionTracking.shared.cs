using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IVersionTracking
	{
		void Track();

		bool IsFirstLaunchEver { get; }

		bool IsFirstLaunchForCurrentVersion { get; }

		bool IsFirstLaunchForCurrentBuild { get; }

		string CurrentVersion { get; }

		string CurrentBuild { get; }

		string PreviousVersion { get; }

		string PreviousBuild { get; }

		string FirstInstalledVersion { get; }

		string FirstInstalledBuild { get; }

		IReadOnlyList<string> VersionHistory { get; }

		IReadOnlyList<string> BuildHistory { get; }

		bool IsFirstLaunchForVersion(string version);

		bool IsFirstLaunchForBuild(string build);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="Type[@FullName='Microsoft.Maui.Essentials.VersionTracking']/Docs" />
	public static class VersionTracking
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='Track']/Docs" />
		[Preserve]
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
		public static string PreviousVersion
			=> Current.PreviousVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousBuild']/Docs" />
		public static string PreviousBuild
			=> Current.PreviousBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledVersion']/Docs" />
		public static string FirstInstalledVersion
			=> Current.FirstInstalledVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledBuild']/Docs" />
		public static string FirstInstalledBuild =>
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

		//internal static string GetStatus()
		//	=> Current.GetStatus();

#nullable enable
		static IVersionTracking? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IVersionTracking Current =>
			currentImplementation ??= new VersionTrackingImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IVersionTracking? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}

namespace Microsoft.Maui.ApplicationModel
{
	public class VersionTrackingImplementation : IVersionTracking
	{
		const string versionTrailKey = "VersionTracking.Trail";
		const string versionsKey = "VersionTracking.Versions";
		const string buildsKey = "VersionTracking.Builds";

		readonly string sharedName = Preferences.GetPrivatePreferencesSharedName("versiontracking");

		readonly Dictionary<string, List<string>> versionTrail;

		public VersionTrackingImplementation()
		{
			IsFirstLaunchEver = !Preferences.ContainsKey(versionsKey, sharedName) || !Preferences.ContainsKey(buildsKey, sharedName);
			if (IsFirstLaunchEver)
			{
				versionTrail = new Dictionary<string, List<string>>
				{
					{ versionsKey, new List<string>() },
					{ buildsKey, new List<string>() }
				};
			}
			else
			{
				versionTrail = new Dictionary<string, List<string>>
				{
					{ versionsKey, ReadHistory(versionsKey).ToList() },
					{ buildsKey, ReadHistory(buildsKey).ToList() }
				};
			}

			IsFirstLaunchForCurrentVersion = !versionTrail[versionsKey].Contains(CurrentVersion);
			if (IsFirstLaunchForCurrentVersion)
			{
				versionTrail[versionsKey].Add(CurrentVersion);
			}

			IsFirstLaunchForCurrentBuild = !versionTrail[buildsKey].Contains(CurrentBuild);
			if (IsFirstLaunchForCurrentBuild)
			{
				versionTrail[buildsKey].Add(CurrentBuild);
			}

			if (IsFirstLaunchForCurrentVersion || IsFirstLaunchForCurrentBuild)
			{
				WriteHistory(versionsKey, versionTrail[versionsKey]);
				WriteHistory(buildsKey, versionTrail[buildsKey]);
			}
		}

		[Preserve]
		public void Track()
		{
		}

		public bool IsFirstLaunchEver { get; private set; }

		public bool IsFirstLaunchForCurrentVersion { get; private set; }

		public bool IsFirstLaunchForCurrentBuild { get; private set; }

		public string CurrentVersion => AppInfo.VersionString;

		public string CurrentBuild => AppInfo.BuildString;

		public string PreviousVersion => GetPrevious(versionsKey);

		public string PreviousBuild => GetPrevious(buildsKey);

		public string FirstInstalledVersion => versionTrail[versionsKey].FirstOrDefault();

		public string FirstInstalledBuild => versionTrail[buildsKey].FirstOrDefault();

		public IReadOnlyList<string> VersionHistory => versionTrail[versionsKey].ToArray();

		public IReadOnlyList<string> BuildHistory => versionTrail[buildsKey].ToArray();

		public bool IsFirstLaunchForVersion(string version)
			=> CurrentVersion == version && IsFirstLaunchForCurrentVersion;

		public bool IsFirstLaunchForBuild(string build)
			=> CurrentBuild == build && IsFirstLaunchForCurrentBuild;

		string GetStatus()
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine("VersionTracking");
			sb.AppendLine($"  IsFirstLaunchEver:              {IsFirstLaunchEver}");
			sb.AppendLine($"  IsFirstLaunchForCurrentVersion: {IsFirstLaunchForCurrentVersion}");
			sb.AppendLine($"  IsFirstLaunchForCurrentBuild:   {IsFirstLaunchForCurrentBuild}");
			sb.AppendLine();
			sb.AppendLine($"  CurrentVersion:                 {CurrentVersion}");
			sb.AppendLine($"  PreviousVersion:                {PreviousVersion}");
			sb.AppendLine($"  FirstInstalledVersion:          {FirstInstalledVersion}");
			sb.AppendLine($"  VersionHistory:                 [{string.Join(", ", VersionHistory)}]");
			sb.AppendLine();
			sb.AppendLine($"  CurrentBuild:                   {CurrentBuild}");
			sb.AppendLine($"  PreviousBuild:                  {PreviousBuild}");
			sb.AppendLine($"  FirstInstalledBuild:            {FirstInstalledBuild}");
			sb.AppendLine($"  BuildHistory:                   [{string.Join(", ", BuildHistory)}]");
			return sb.ToString();
		}

		string[] ReadHistory(string key)
			=> Preferences.Get(key, null, sharedName)?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

		void WriteHistory(string key, IEnumerable<string> history)
			=> Preferences.Set(key, string.Join("|", history), sharedName);

		string GetPrevious(string key)
		{
			var trail = versionTrail[key];
			return (trail.Count >= 2) ? trail[trail.Count - 2] : null;
		}
	}
}