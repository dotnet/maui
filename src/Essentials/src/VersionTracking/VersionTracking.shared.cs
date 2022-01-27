using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="Type[@FullName='Microsoft.Maui.Essentials.VersionTracking']/Docs" />
	public static class VersionTracking
	{
		const string versionTrailKey = "VersionTracking.Trail";
		const string versionsKey = "VersionTracking.Versions";
		const string buildsKey = "VersionTracking.Builds";

		static readonly string sharedName = Preferences.GetPrivatePreferencesSharedName("versiontracking");

		static readonly Dictionary<string, List<string>> versionTrail;

		static VersionTracking()
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='Track']/Docs" />
		[Preserve]
		public static void Track()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchEver']/Docs" />
		public static bool IsFirstLaunchEver { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentVersion']/Docs" />
		public static bool IsFirstLaunchForCurrentVersion { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentBuild']/Docs" />
		public static bool IsFirstLaunchForCurrentBuild { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentVersion']/Docs" />
		public static string CurrentVersion => AppInfo.VersionString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentBuild']/Docs" />
		public static string CurrentBuild => AppInfo.BuildString;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousVersion']/Docs" />
		public static string PreviousVersion => GetPrevious(versionsKey);

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousBuild']/Docs" />
		public static string PreviousBuild => GetPrevious(buildsKey);

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledVersion']/Docs" />
		public static string FirstInstalledVersion => versionTrail[versionsKey].FirstOrDefault();

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledBuild']/Docs" />
		public static string FirstInstalledBuild => versionTrail[buildsKey].FirstOrDefault();

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='VersionHistory']/Docs" />
		public static IEnumerable<string> VersionHistory => versionTrail[versionsKey].ToArray();

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='BuildHistory']/Docs" />
		public static IEnumerable<string> BuildHistory => versionTrail[buildsKey].ToArray();

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForVersion']/Docs" />
		public static bool IsFirstLaunchForVersion(string version)
			=> CurrentVersion == version && IsFirstLaunchForCurrentVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForBuild']/Docs" />
		public static bool IsFirstLaunchForBuild(string build)
			=> CurrentBuild == build && IsFirstLaunchForCurrentBuild;

		internal static string GetStatus()
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

		static string[] ReadHistory(string key)
			=> Preferences.Get(key, null, sharedName)?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

		static void WriteHistory(string key, IEnumerable<string> history)
			=> Preferences.Set(key, string.Join("|", history), sharedName);

		static string GetPrevious(string key)
		{
			var trail = versionTrail[key];
			return (trail.Count >= 2) ? trail[trail.Count - 2] : null;
		}
	}
}
