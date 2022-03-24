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

	public static partial class VersionTracking
	{
		static IVersionTracking? defaultImplementation;

		public static IVersionTracking Default =>
			defaultImplementation ??= new VersionTrackingImplementation(Preferences.Default, AppInfo.Current);

		internal static void SetDefault(IVersionTracking? implementation) =>
			defaultImplementation = implementation;
	}

	class VersionTrackingImplementation : IVersionTracking
	{
		const string versionTrailKey = "VersionTracking.Trail";
		const string versionsKey = "VersionTracking.Versions";
		const string buildsKey = "VersionTracking.Builds";

		static readonly string sharedName = Preferences.GetPrivatePreferencesSharedName("versiontracking");

		readonly IPreferences preferences;
		readonly IAppInfo appInfo;

		Dictionary<string, List<string>> versionTrail = null!;

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