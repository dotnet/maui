using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("VersionTracking")]
	public class VersionTracking_Tests
	{
		// We cannot mock the app version but it should be constant value
#if WINDOWS_UWP || WINDOWS
		const string currentVersion = "1.0.0.1";
		const string currentBuild = "1";
#else
		const string currentVersion = "1.0";
		const string currentBuild = "1";
#endif

		const string versionsKey = "VersionTracking.Versions";
		const string buildsKey = "VersionTracking.Builds";
		static readonly string sharedName = Preferences.GetPrivatePreferencesSharedName("versiontracking");

		readonly ITestOutputHelper output;

		public VersionTracking_Tests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void First_Launch_Ever()
		{
			VersionTracking.Track();
			Preferences.Clear(sharedName);

			VersionTracking.InitVersionTracking();

			Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
			Assert.True(VersionTracking.IsFirstLaunchEver);
			Assert.True(VersionTracking.IsFirstLaunchForCurrentVersion);
			Assert.True(VersionTracking.IsFirstLaunchForCurrentBuild);
		}

		[Fact]
		public void First_Launch_For_Version()
		{
			VersionTracking.Track();
			Preferences.Set(versionsKey, string.Join("|", new string[] { "0.8.0", "0.9.0", "1.0.0" }), sharedName);
			Preferences.Set(buildsKey, string.Join("|", new string[] { currentBuild }), sharedName);

			VersionTracking.InitVersionTracking();

			Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
			Assert.Equal("1.0.0", VersionTracking.PreviousVersion);
			Assert.Equal("0.8.0", VersionTracking.FirstInstalledVersion);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.True(VersionTracking.IsFirstLaunchForCurrentVersion);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);

			VersionTracking.InitVersionTracking();

			Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
			Assert.Equal("1.0.0", VersionTracking.PreviousVersion);
			Assert.Equal("0.8.0", VersionTracking.FirstInstalledVersion);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);
		}

		[Fact]
		public void First_Launch_For_Build()
		{
			VersionTracking.Track();
			Preferences.Set(versionsKey, string.Join("|", new string[] { currentVersion }), sharedName);
			Preferences.Set(buildsKey, string.Join("|", new string[] { "10", "20" }), sharedName);

			VersionTracking.InitVersionTracking();

			Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
			Assert.Equal("20", VersionTracking.PreviousBuild);
			Assert.Equal("10", VersionTracking.FirstInstalledBuild);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
			Assert.True(VersionTracking.IsFirstLaunchForCurrentBuild);

			VersionTracking.InitVersionTracking();

			Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
			Assert.Equal("20", VersionTracking.PreviousBuild);
			Assert.Equal("10", VersionTracking.FirstInstalledBuild);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);
		}

		[Fact]
		public void First_Launch_After_Downgrade()
		{
			VersionTracking.Track();
			Preferences.Set(versionsKey, string.Join("|", new string[] { currentVersion, "1.0.2", "1.0.3" }), sharedName);

			VersionTracking.InitVersionTracking();
			output.WriteLine((VersionTracking.Default as VersionTrackingImplementation)?.GetStatus());

			Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
			Assert.Equal("1.0.3", VersionTracking.PreviousVersion);
			Assert.Equal("1.0.2", VersionTracking.FirstInstalledVersion);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.True(VersionTracking.IsFirstLaunchForCurrentVersion);

			VersionTracking.InitVersionTracking();

			Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
			Assert.Equal("1.0.3", VersionTracking.PreviousVersion);
			Assert.Equal("1.0.2", VersionTracking.FirstInstalledVersion);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
		}

		[Fact]
		public void First_Launch_After_Build_Downgrade()
		{
			VersionTracking.Track();
			Preferences.Set(versionsKey, string.Join("|", new string[] { currentVersion }), sharedName);
			Preferences.Set(buildsKey, string.Join("|", new string[] { currentBuild, "10", "20" }), sharedName);

			VersionTracking.InitVersionTracking();
			output.WriteLine((VersionTracking.Default as VersionTrackingImplementation)?.GetStatus());

			Assert.Equal(currentBuild, VersionTracking.CurrentBuild);
			Assert.Equal("20", VersionTracking.PreviousBuild);
			Assert.Equal("10", VersionTracking.FirstInstalledBuild);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
			Assert.True(VersionTracking.IsFirstLaunchForCurrentBuild);

			VersionTracking.InitVersionTracking();

			Assert.Equal(currentBuild, VersionTracking.CurrentBuild);
			Assert.Equal("20", VersionTracking.PreviousBuild);
			Assert.Equal("10", VersionTracking.FirstInstalledBuild);
			Assert.False(VersionTracking.IsFirstLaunchEver);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
			Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);
		}
	}
}
