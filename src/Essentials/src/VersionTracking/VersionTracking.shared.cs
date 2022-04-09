#nullable enable
using System;
using System.Collections.Generic;
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

		string? PreviousVersion { get; }

		string? PreviousBuild { get; }

		string? FirstInstalledVersion { get; }

		string? FirstInstalledBuild { get; }

		IReadOnlyList<string> VersionHistory { get; }

		IReadOnlyList<string> BuildHistory { get; }

		bool IsFirstLaunchForVersion(string version);

		bool IsFirstLaunchForBuild(string build);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="Type[@FullName='Microsoft.Maui.Essentials.VersionTracking']/Docs" />
	public static partial class VersionTracking
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='Track']/Docs" />
		public static void Track()
			=> Default.Track();

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchEver']/Docs" />
		public static bool IsFirstLaunchEver
			=> Default.IsFirstLaunchEver;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentVersion']/Docs" />
		public static bool IsFirstLaunchForCurrentVersion
			=> Default.IsFirstLaunchForCurrentVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForCurrentBuild']/Docs" />
		public static bool IsFirstLaunchForCurrentBuild
			=> Default.IsFirstLaunchForCurrentBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentVersion']/Docs" />
		public static string CurrentVersion
			=> Default.CurrentVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='CurrentBuild']/Docs" />
		public static string CurrentBuild
			=> Default.CurrentBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousVersion']/Docs" />
		public static string? PreviousVersion
			=> Default.PreviousVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='PreviousBuild']/Docs" />
		public static string? PreviousBuild
			=> Default.PreviousBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledVersion']/Docs" />
		public static string? FirstInstalledVersion
			=> Default.FirstInstalledVersion;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='FirstInstalledBuild']/Docs" />
		public static string? FirstInstalledBuild
			=> Default.FirstInstalledBuild;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='VersionHistory']/Docs" />
		public static IEnumerable<string> VersionHistory
			=> Default.VersionHistory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='BuildHistory']/Docs" />
		public static IEnumerable<string> BuildHistory
			=> Default.BuildHistory;

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForVersion']/Docs" />
		public static bool IsFirstLaunchForVersion(string version)
			=> Default.IsFirstLaunchForVersion(version);

		/// <include file="../../docs/Microsoft.Maui.Essentials/VersionTracking.xml" path="//Member[@MemberName='IsFirstLaunchForBuild']/Docs" />
		public static bool IsFirstLaunchForBuild(string build)
			=> Default.IsFirstLaunchForBuild(build);

		static IVersionTracking? defaultImplementation;

		public static IVersionTracking Default =>
			defaultImplementation ??= new VersionTrackingImplementation(Preferences.Default, AppInfo.Current);

		internal static void SetDefault(IVersionTracking? implementation) =>
			defaultImplementation = implementation;

		internal static void InitVersionTracking() =>
			(Default as VersionTrackingImplementation)?.InitVersionTracking();
	}

	class VersionTrackingImplementation : IVersionTracking
	{
		const string versionsKey = "VersionTracking.Versions";
		const string buildsKey = "VersionTracking.Builds";

		static readonly string sharedName = Preferences.GetPrivatePreferencesSharedName("versiontracking");

		readonly IPreferences preferences;
		readonly IAppInfo appInfo;

		Dictionary<string, List<string>> versionTrail = null!;

		string LastInstalledVersion => versionTrail[versionsKey]?.LastOrDefault() ?? string.Empty;

        string LastInstalledBuild => versionTrail[buildsKey]?.LastOrDefault() ?? string.Empty;

		public VersionTrackingImplementation(IPreferences preferences, IAppInfo appInfo)
		{
			this.preferences = preferences;
			this.appInfo = appInfo;

			Track();
		}

		public void Track()
		{
			if (versionTrail != null)
				return;

			InitVersionTracking();
		}

		/// <summary>
        /// Initialize VersionTracking module, load data and track current version
        /// </summary>
        /// <remarks>
        /// For internal use. Usually only called once in production code, but multiple times in unit tests
        /// </remarks>
        internal void InitVersionTracking()
		{
			IsFirstLaunchEver = !preferences.ContainsKey(versionsKey, sharedName) || !preferences.ContainsKey(buildsKey, sharedName);
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

			IsFirstLaunchForCurrentVersion = !versionTrail[versionsKey].Contains(CurrentVersion) || CurrentVersion != LastInstalledVersion;
			if (IsFirstLaunchForCurrentVersion)
			{
				// Avoid duplicates and move current version to end of list if already present
                versionTrail[versionsKey].RemoveAll(v => v == CurrentVersion);
				versionTrail[versionsKey].Add(CurrentVersion);
			}

			IsFirstLaunchForCurrentBuild = !versionTrail[buildsKey].Contains(CurrentBuild) || CurrentBuild != LastInstalledBuild;
			if (IsFirstLaunchForCurrentBuild)
			{
				// Avoid duplicates and move current build to end of list if already present
                versionTrail[buildsKey].RemoveAll(b => b == CurrentBuild);
				versionTrail[buildsKey].Add(CurrentBuild);
			}

			if (IsFirstLaunchForCurrentVersion || IsFirstLaunchForCurrentBuild)
			{
				WriteHistory(versionsKey, versionTrail[versionsKey]);
				WriteHistory(buildsKey, versionTrail[buildsKey]);
			}
		}

		public bool IsFirstLaunchEver { get; private set; }

		public bool IsFirstLaunchForCurrentVersion { get; private set; }

		public bool IsFirstLaunchForCurrentBuild { get; private set; }

		public string CurrentVersion => appInfo.VersionString;

		public string CurrentBuild => appInfo.BuildString;

		public string? PreviousVersion => GetPrevious(versionsKey);

		public string? PreviousBuild => GetPrevious(buildsKey);

		public string? FirstInstalledVersion => versionTrail[versionsKey].FirstOrDefault();

		public string? FirstInstalledBuild => versionTrail[buildsKey].FirstOrDefault();

		public IReadOnlyList<string> VersionHistory => versionTrail[versionsKey].ToArray();

		public IReadOnlyList<string> BuildHistory => versionTrail[buildsKey].ToArray();

		public bool IsFirstLaunchForVersion(string version)
			=> CurrentVersion == version && IsFirstLaunchForCurrentVersion;

		public bool IsFirstLaunchForBuild(string build)
			=> CurrentBuild == build && IsFirstLaunchForCurrentBuild;

		public string GetStatus()
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
			=> preferences.Get<string?>(key, null, sharedName)?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

		void WriteHistory(string key, IEnumerable<string> history)
			=> preferences.Set(key, string.Join("|", history), sharedName);

		string? GetPrevious(string key)
		{
			var trail = versionTrail[key];
			return (trail.Count >= 2) ? trail[trail.Count - 2] : null;
		}
	}
}