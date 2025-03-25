#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// The VersionTracking API provides an easy way to track an app's version on a device.
	/// </summary>
	public interface IVersionTracking
	{
		/// <summary>
		/// Starts tracking version information.
		/// </summary>
		void Track();

		/// <summary>
		/// Gets a value indicating whether this is the first time this app has ever been launched on this device.
		/// </summary>
		bool IsFirstLaunchEver { get; }

		/// <summary>
		/// Gets a value indicating if this is the first launch of the app for the current version number.
		/// </summary>
		bool IsFirstLaunchForCurrentVersion { get; }

		/// <summary>
		/// Gets a value indicating if this is the first launch of the app for the current build number.
		/// </summary>
		bool IsFirstLaunchForCurrentBuild { get; }

		/// <summary>
		/// Gets the current version number of the app.
		/// </summary>
		string CurrentVersion { get; }

		/// <summary>
		/// Gets the current build of the app.
		/// </summary>
		string CurrentBuild { get; }

		/// <summary>
		/// Gets the version number for the previously run version.
		/// </summary>
		string? PreviousVersion { get; }

		/// <summary>
		/// Gets the build number for the previously run version.
		/// </summary>
		string? PreviousBuild { get; }

		/// <summary>
		/// Gets the version number of the first version of the app that was installed on this device.
		/// </summary>
		string? FirstInstalledVersion { get; }

		/// <summary>
		/// Gets the build number of first version of the app that was installed on this device.
		/// </summary>
		string? FirstInstalledBuild { get; }

		/// <summary>
		/// Gets the collection of version numbers of the app that ran on this device.
		/// </summary>
		IReadOnlyList<string> VersionHistory { get; }

		/// <summary>
		/// Gets the collection of build numbers of the app that ran on this device.
		/// </summary>
		IReadOnlyList<string> BuildHistory { get; }

		/// <summary>
		/// Determines if this is the first launch of the app for a specified version number.
		/// </summary>
		/// <param name="version">The version number.</param>
		/// <returns><see langword="true"/> if this is the first launch of the app for the specified version number; otherwise <see langword="false"/>.</returns>
		bool IsFirstLaunchForVersion(string version);

		/// <summary>
		/// Determines if this is the first launch of the app for a specified build number.
		/// </summary>
		/// <param name="build">The build number.</param>
		/// <returns><see langword="true"/> if this is the first launch of the app for the specified build number; otherwise <see langword="false"/>.</returns>
		bool IsFirstLaunchForBuild(string build);
	}

	/// <summary>
	/// The VersionTracking API provides an easy way to track an app's version on a device.
	/// </summary>
	public static partial class VersionTracking
	{
		/// <summary>
		/// Starts tracking version information.
		/// </summary>
		public static void Track()
			=> Default.Track();

		/// <summary>
		/// Gets a value indicating whether this is the first time this app has ever been launched on this device.
		/// </summary>
		public static bool IsFirstLaunchEver
			=> Default.IsFirstLaunchEver;

		/// <summary>
		/// Gets a value indicating if this is the first launch of the app for the current version number.
		/// </summary>
		public static bool IsFirstLaunchForCurrentVersion
			=> Default.IsFirstLaunchForCurrentVersion;

		/// <summary>
		/// Gets a value indicating if this is the first launch of the app for the current build number.
		/// </summary>
		public static bool IsFirstLaunchForCurrentBuild
			=> Default.IsFirstLaunchForCurrentBuild;

		/// <summary>
		/// Gets the current version number of the app.
		/// </summary>
		public static string CurrentVersion
			=> Default.CurrentVersion;

		/// <summary>
		/// Gets the current build of the app.
		/// </summary>
		public static string CurrentBuild
			=> Default.CurrentBuild;

		/// <summary>
		/// Gets the version number for the previously run version.
		/// </summary>
		public static string? PreviousVersion
			=> Default.PreviousVersion;

		/// <summary>
		/// Gets the build number for the previously run version.
		/// </summary>
		public static string? PreviousBuild
			=> Default.PreviousBuild;

		/// <summary>
		/// Gets the version number of the first version of the app that was installed on this device.
		/// </summary>
		public static string? FirstInstalledVersion
			=> Default.FirstInstalledVersion;

		/// <summary>
		/// Gets the build number of first version of the app that was installed on this device.
		/// </summary>
		public static string? FirstInstalledBuild
			=> Default.FirstInstalledBuild;

		/// <summary>
		/// Gets the collection of version numbers of the app that ran on this device.
		/// </summary>
		public static IEnumerable<string> VersionHistory
			=> Default.VersionHistory;

		/// <summary>
		/// Gets the collection of build numbers of the app that ran on this device.
		/// </summary>
		public static IEnumerable<string> BuildHistory
			=> Default.BuildHistory;

		/// <summary>
		/// Determines if this is the first launch of the app for a specified version number.
		/// </summary>
		/// <param name="version">The version number.</param>
		/// <returns><see langword="true"/> if this is the first launch of the app for the specified version number; otherwise <see langword="false"/>.</returns>
		public static bool IsFirstLaunchForVersion(string version)
			=> Default.IsFirstLaunchForVersion(version);

		/// <summary>
		/// Determines if this is the first launch of the app for a specified build number.
		/// </summary>
		/// <param name="build">The build number.</param>
		/// <returns><see langword="true"/> if this is the first launch of the app for the specified build number; otherwise <see langword="false"/>.</returns>
		public static bool IsFirstLaunchForBuild(string build)
			=> Default.IsFirstLaunchForBuild(build);

		static IVersionTracking? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
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
				versionTrail = new(StringComparer.Ordinal)
				{
					{ versionsKey, new List<string>() },
					{ buildsKey, new List<string>() }
				};
			}
			else
			{
				versionTrail = new(StringComparer.Ordinal)
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
			=> preferences.Get<string?>(key, null, sharedName)?.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

		void WriteHistory(string key, IEnumerable<string> history)
			=> preferences.Set(key, string.Join("|", history), sharedName);

		string? GetPrevious(string key)
		{
			var trail = versionTrail[key];
			return (trail.Count >= 2) ? trail[trail.Count - 2] : null;
		}
	}
}