using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Essentials.Gtk;
using Microsoft.Maui.Networking;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

public sealed class TempDir : IDisposable
{
	public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "maui-gtk-" + Guid.NewGuid().ToString("N"));

	public TempDir() => Directory.CreateDirectory(Path);

	public string File(string relative, string? content = null, bool overwrite = false)
	{
		var full = System.IO.Path.Combine(Path, relative);
		Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
		if (content is not null)
			System.IO.File.WriteAllText(full, content);
		return full;
	}

	public void Dispose()
	{
		try
		{
			Directory.Delete(Path, true);
		}
		catch (IOException)
		{
		}
	}
}

public class OsReleaseTests
{
	const string Debian = "PRETTY_NAME=\"Debian GNU/Linux 13 (trixie)\"\nNAME=\"Debian GNU/Linux\"\nVERSION_ID=\"13\"\n# comment\nHOME_URL='https://www.debian.org/'\n";

	[Fact]
	public void ParsesQuotedValuesAndSkipsComments()
	{
		var os = OsRelease.Parse(Debian);

		Assert.Equal("Debian GNU/Linux 13 (trixie)", os.Name);
		Assert.Equal("13", os.VersionId);
		Assert.Equal("https://www.debian.org/", os["HOME_URL"]);
		Assert.Null(os["MISSING"]);
	}

	[Fact]
	public void HandlesEscapesAndRollingReleases()
	{
		var os = OsRelease.Parse("NAME=\"Arch \\\"Linux\\\"\"\r\nID=arch\r\n");

		Assert.Equal("Arch \"Linux\"", os.Name);
		Assert.Null(os.VersionId);
		Assert.Equal("arch", os["ID"]);
	}

	[Fact]
	public void LoadPrefersFirstExistingFileAndFallsBackToEmpty()
	{
		using var dir = new TempDir();
		var second = dir.File("usr-lib-os-release", "NAME=Second\nVERSION_ID=2\n");

		Assert.Equal("Second", OsRelease.Load(System.IO.Path.Combine(dir.Path, "missing"), second).Name);
		Assert.Null(OsRelease.Load(System.IO.Path.Combine(dir.Path, "missing")).Name);
	}
}

public class XdgPathsTests
{
	static XdgPaths With(Dictionary<string, string> env, string home = "/home/u") =>
		new(k => env.TryGetValue(k, out var v) ? v : null, home);

	[Fact]
	public void UsesDefaultsUnderHome()
	{
		var xdg = With(new(), System.IO.Path.GetFullPath("/home/u"));

		Assert.Equal(System.IO.Path.Combine(System.IO.Path.GetFullPath("/home/u"), ".local", "share"), xdg.DataHome);
		Assert.Equal(System.IO.Path.Combine(System.IO.Path.GetFullPath("/home/u"), ".config"), xdg.ConfigHome);
		Assert.Equal(System.IO.Path.Combine(System.IO.Path.GetFullPath("/home/u"), ".cache"), xdg.CacheHome);
	}

	[Fact]
	public void HonorsAbsoluteVariablesAndIgnoresRelativeOnes()
	{
		var abs = System.IO.Path.GetFullPath("/custom/data");
		var xdg = With(new() { ["XDG_DATA_HOME"] = abs, ["XDG_CONFIG_HOME"] = "relative/config", ["XDG_CACHE_HOME"] = "" });

		Assert.Equal(abs, xdg.DataHome);
		Assert.EndsWith(".config", xdg.ConfigHome, StringComparison.Ordinal);
		Assert.EndsWith(".cache", xdg.CacheHome, StringComparison.Ordinal);
	}
}

public class DmiInfoTests
{
	[Fact]
	public void ReadsModelAndVendor()
	{
		using var dir = new TempDir();
		dir.File("product_name", "ThinkPad X1\n");
		dir.File("sys_vendor", "LENOVO\n");

		var dmi = DmiInfo.Read(dir.Path);

		Assert.Equal("ThinkPad X1", dmi.ProductName);
		Assert.Equal("LENOVO", dmi.Vendor);
		Assert.False(dmi.IsVirtual);
	}

	[Theory]
	[InlineData("QEMU", "Standard PC (Q35 + ICH9, 2009)")]
	[InlineData("VMware, Inc.", "VMware Virtual Platform")]
	[InlineData("Microsoft Corporation", "Virtual Machine")]
	[InlineData("innotek GmbH", "VirtualBox")]
	public void DetectsHypervisors(string vendor, string product) =>
		Assert.True(new DmiInfo(product, vendor).IsVirtual);

	[Fact]
	public void MissingFilesGiveNulls()
	{
		using var dir = new TempDir();
		var dmi = DmiInfo.Read(dir.Path);

		Assert.Null(dmi.ProductName);
		Assert.Null(dmi.Vendor);
	}
}

public class DeviceInfoTests
{
	[Fact]
	public void ReportsLinuxDesktopFromOsReleaseAndDmi()
	{
		var info = new GtkDeviceInfo(
			OsRelease.Parse("VERSION_ID=\"24.04\"\n"),
			new DmiInfo("Latitude 7420", "Dell Inc."),
			() => "6.8.0.0",
			() => "host1");

		Assert.Equal("Linux", info.Platform.ToString());
		Assert.Equal(DeviceIdiom.Desktop, info.Idiom);
		Assert.Equal(DeviceType.Physical, info.DeviceType);
		Assert.Equal("Latitude 7420", info.Model);
		Assert.Equal("Dell Inc.", info.Manufacturer);
		Assert.Equal("host1", info.Name);
		Assert.Equal("24.04", info.VersionString);
		Assert.Equal(new Version(24, 4), info.Version);
	}

	[Fact]
	public void FallsBackToKernelVersionOnRollingReleases()
	{
		var info = new GtkDeviceInfo(OsRelease.Empty, new DmiInfo(null, "QEMU"), () => "6.12.1.0", () => "h");

		Assert.Equal("6.12.1.0", info.VersionString);
		Assert.Equal(new Version(6, 12, 1, 0), info.Version);
		Assert.Equal("Unknown", info.Model);
		Assert.Equal(DeviceType.Virtual, info.DeviceType);
	}
}

public class AppIdentityTests
{
	[Fact]
	public void OptionsOverrideAssemblyValues()
	{
		var id = AppIdentity.Resolve(typeof(AppIdentityTests).Assembly, "com.contoso.notes", "Notes");

		Assert.Equal("com.contoso.notes", id.ApplicationId);
		Assert.Equal("Notes", id.Name);
	}

	[Fact]
	public void FallsBackToAssemblyName()
	{
		var id = AppIdentity.Resolve(typeof(AppIdentityTests).Assembly);

		Assert.Equal("Microsoft.Maui.Gtk.UnitTests", id.ApplicationId);
		Assert.NotNull(id.Version);
	}

	[Fact]
	public void NullAssemblyStillResolves() =>
		Assert.Equal("app", AppIdentity.Resolve(null).ApplicationId);

	[Theory]
	[InlineData("1.2.3", 1, 2, 3)]
	[InlineData("2.0.1-beta.4+abc", 2, 0, 1)]
	[InlineData("7", 7, 0, 0)]
	public void ParsesLeadingNumericVersion(string text, int major, int minor, int build)
	{
		var v = AppIdentity.ParseVersion(text)!;
		Assert.Equal(major, v.Major);
		Assert.Equal(minor, v.Minor);
		Assert.Equal(Math.Max(build, 0), Math.Max(v.Build, 0));
	}

	[Theory]
	[InlineData("rolling")]
	[InlineData("")]
	[InlineData(null)]
	public void RejectsNonNumericVersions(string? text) =>
		Assert.Null(AppIdentity.ParseVersion(text));

	[Fact]
	public void DirectoryNameIsSafe()
	{
		Assert.Equal("a_b", AppIdentity.ToDirectoryName("a/b"));
		Assert.Equal("app", AppIdentity.ToDirectoryName(".."));
	}
}

public class FileSystemTests
{
	[Fact]
	public void CreatesPerAppXdgDirectories()
	{
		using var dir = new TempDir();
		var xdg = new XdgPaths(k => k switch
		{
			"XDG_DATA_HOME" => System.IO.Path.Combine(dir.Path, "data"),
			"XDG_CACHE_HOME" => System.IO.Path.Combine(dir.Path, "cache"),
			_ => null,
		}, dir.Path);
		var fs = new GtkFileSystem("com.contoso.app", xdg, dir.Path);

		Assert.Equal(System.IO.Path.Combine(dir.Path, "data", "com.contoso.app"), fs.AppDataDirectory);
		Assert.Equal(System.IO.Path.Combine(dir.Path, "cache", "com.contoso.app"), fs.CacheDirectory);
		Assert.True(Directory.Exists(fs.AppDataDirectory));
		Assert.True(Directory.Exists(fs.CacheDirectory));
	}

	[Fact]
	public async System.Threading.Tasks.Task PackageFilesComeFromTheBaseDirectory()
	{
		using var dir = new TempDir();
		dir.File("Resources/Raw/hello.txt", "hi");
		var fs = new GtkFileSystem("app", new XdgPaths(_ => null, dir.Path), dir.Path);

		Assert.True(await fs.AppPackageFileExistsAsync("Resources/Raw/hello.txt"));
		Assert.True(await fs.AppPackageFileExistsAsync("/Resources\\Raw/hello.txt"));
		Assert.False(await fs.AppPackageFileExistsAsync("nope.txt"));

		using var reader = new StreamReader(await fs.OpenAppPackageFileAsync("Resources/Raw/hello.txt"));
		Assert.Equal("hi", await reader.ReadToEndAsync());
	}
}

public class PreferencesTests
{
	[Fact]
	public void RoundTripsEverySupportedType()
	{
		using var dir = new TempDir();
		var p = new GtkPreferences(dir.Path);
		var dt = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc);
		var dto = new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.FromHours(2));

		p.Set("s", "text");
		p.Set("b", true);
		p.Set("i", 42);
		p.Set("l", 1L << 40);
		p.Set("f", 1.5f);
		p.Set("d", 2.0);
		p.Set("dt", dt);
		p.Set("dto", dto);

		Assert.Equal("text", p.Get("s", ""));
		Assert.True(p.Get("b", false));
		Assert.Equal(42, p.Get("i", 0));
		Assert.Equal(1L << 40, p.Get("l", 0L));
		Assert.Equal(1.5f, p.Get("f", 0f));
		Assert.Equal(2.0, p.Get("d", 0.0));
		Assert.Equal(dt, p.Get("dt", DateTime.MinValue));
		Assert.Equal(DateTimeKind.Utc, p.Get("dt", DateTime.MinValue).Kind);
		Assert.Equal(dto, p.Get("dto", DateTimeOffset.MinValue));
	}

	[Fact]
	public void PersistsAcrossInstances()
	{
		using var dir = new TempDir();
		new GtkPreferences(dir.Path).Set("k", "v");

		Assert.Equal("v", new GtkPreferences(dir.Path).Get("k", ""));
		Assert.True(File.Exists(System.IO.Path.Combine(dir.Path, "preferences.json")));
	}

	[Fact]
	public void SharedNamesAreIsolated()
	{
		using var dir = new TempDir();
		var p = new GtkPreferences(dir.Path);
		p.Set("k", "default");
		p.Set("k", "other", "my/shared");

		Assert.Equal("default", p.Get("k", ""));
		Assert.Equal("other", p.Get("k", "", "my/shared"));
		Assert.True(File.Exists(System.IO.Path.Combine(dir.Path, "preferences.my_shared.json")));
		p.Clear("my/shared");
		Assert.False(p.ContainsKey("k", "my/shared"));
		Assert.True(p.ContainsKey("k"));
	}

	[Fact]
	public void RemoveClearAndNullBehaveLikeOtherPlatforms()
	{
		using var dir = new TempDir();
		var p = new GtkPreferences(dir.Path);
		p.Set("a", 1);
		p.Set("b", 2);
		p.Set<string?>("c", "x");

		p.Remove("a");
		p.Set<string?>("c", null);

		Assert.False(p.ContainsKey("a"));
		Assert.False(p.ContainsKey("c"));
		p.Clear();
		Assert.False(p.ContainsKey("b"));
		Assert.Equal(7, p.Get("b", 7));
	}

	[Fact]
	public void MismatchedTypeAndCorruptFileReturnDefaults()
	{
		using var dir = new TempDir();
		var p = new GtkPreferences(dir.Path);
		p.Set("s", "not a number");

		Assert.Equal(5, p.Get("s", 5));

		File.WriteAllText(System.IO.Path.Combine(dir.Path, "preferences.json"), "{ not json");
		var fresh = new GtkPreferences(dir.Path);
		Assert.Equal("d", fresh.Get("s", "d"));
		fresh.Set("s", "ok");
		Assert.Equal("ok", new GtkPreferences(dir.Path).Get("s", ""));
	}

	[Fact]
	public void NonFiniteDoublesSurvive()
	{
		using var dir = new TempDir();
		var p = new GtkPreferences(dir.Path);
		p.Set("nan", double.NaN);
		p.Set("inf", double.PositiveInfinity);

		Assert.True(double.IsNaN(new GtkPreferences(dir.Path).Get("nan", 0.0)));
		Assert.True(double.IsPositiveInfinity(new GtkPreferences(dir.Path).Get("inf", 0.0)));
	}

	[Fact]
	public void UnsupportedTypesThrow()
	{
		using var dir = new TempDir();
		Assert.Throws<NotSupportedException>(() => new GtkPreferences(dir.Path).Set("x", new object()));
	}
}

public class SecureStorageTests
{
	sealed class FakeBackend : ISecretBackend
	{
		public Dictionary<(string, string), string> Items { get; } = new();

		public string? Lookup(string service, string key) => Items.TryGetValue((service, key), out var v) ? v : null;

		public void Store(string service, string key, string label, string secret) => Items[(service, key)] = secret;

		public bool Clear(string service, string key) => Items.Remove((service, key));

		public void ClearAll(string service)
		{
			foreach (var k in Items.Keys.Where(k => k.Item1 == service).ToList())
				Items.Remove(k);
		}
	}

	[Fact]
	public async System.Threading.Tasks.Task StoresPerApplication()
	{
		var backend = new FakeBackend();
		var a = new GtkSecureStorage("app.a", backend);
		var b = new GtkSecureStorage("app.b", backend);

		await a.SetAsync("token", "secret-a");
		await b.SetAsync("token", "secret-b");

		Assert.Equal("secret-a", await a.GetAsync("token"));
		Assert.Equal("secret-b", await b.GetAsync("token"));
		Assert.Null(await a.GetAsync("missing"));
		Assert.True(a.Remove("token"));
		Assert.False(a.Remove("token"));
		a.RemoveAll();
		Assert.Equal("secret-b", await b.GetAsync("token"));
	}

	[Fact]
	public async System.Threading.Tasks.Task NullArgumentsAreRejected()
	{
		var s = new GtkSecureStorage("a", new FakeBackend());

		await Assert.ThrowsAsync<ArgumentNullException>(() => s.SetAsync("k", null!));
		Assert.Throws<ArgumentNullException>(() => s.Remove(null!));
	}

	[Fact]
	public async System.Threading.Tasks.Task MissingLibsecretIsAFeatureNotSupportedError()
	{
		if (OperatingSystem.IsLinux())
			return; // libsecret may exist on this machine

		Assert.Null(LibSecret.TryCreate("test.schema"));
		var s = new GtkSecureStorage("a");
		await Assert.ThrowsAsync<FeatureNotSupportedException>(() => s.GetAsync("k"));
	}
}

public class PortalSettingsTests
{
	[Theory]
	[InlineData("(<<uint32 1>>,)", 1)]
	[InlineData("(<<uint32 2>>,)", 2)]
	[InlineData("(<<0>>,)", 0)]
	[InlineData("(<<uint32 12>>,)", 12)]
	public void ParsesPrintedVariant(string printed, int expected) =>
		Assert.Equal(expected, PortalSettings.ParsePrintedUInt(printed));

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("(<<'dark'>>,)")]
	public void RejectsUnparsableVariants(string? printed) =>
		Assert.Null(PortalSettings.ParsePrintedUInt(printed));

	[Fact]
	public void MapsColorSchemeToTheme()
	{
		Assert.Equal(AppTheme.Dark, PortalSettings.ToAppTheme(1));
		Assert.Equal(AppTheme.Light, PortalSettings.ToAppTheme(2));
		Assert.Equal(AppTheme.Unspecified, PortalSettings.ToAppTheme(0));
		Assert.Equal(AppTheme.Unspecified, PortalSettings.ToAppTheme(null));
	}

	[Fact]
	public void AppInfoUsesThemeReaderAndIdentity()
	{
		var info = new GtkAppInfo(AppIdentity.Resolve(null, "com.x", "X"), () => AppTheme.Dark);

		Assert.Equal(AppTheme.Dark, info.RequestedTheme);
		Assert.Equal("com.x", info.PackageName);
		Assert.Equal("X", info.Name);
		Assert.Equal(AppPackagingModel.Packaged, info.PackagingModel);
		Assert.Throws<FeatureNotSupportedException>(() => info.ShowSettingsUI());
	}
}

public class BatteryTests
{
	static GtkBattery With(TempDir dir) => new(dir.Path);

	[Fact]
	public void NoBatteryMeansNotPresent()
	{
		using var dir = new TempDir();
		dir.File("AC/type", "Mains");
		dir.File("AC/online", "1");

		var b = With(dir);

		Assert.Equal(BatteryState.NotPresent, b.State);
		Assert.Equal(1.0, b.ChargeLevel);
		Assert.Equal(BatteryPowerSource.AC, b.PowerSource);
	}

	[Fact]
	public void ReadsDischargingBattery()
	{
		using var dir = new TempDir();
		dir.File("BAT0/type", "Battery");
		dir.File("BAT0/capacity", "42");
		dir.File("BAT0/status", "Discharging");
		dir.File("AC/type", "Mains");
		dir.File("AC/online", "0");

		var b = With(dir);

		Assert.Equal(0.42, b.ChargeLevel, 3);
		Assert.Equal(BatteryState.Discharging, b.State);
		Assert.Equal(BatteryPowerSource.Battery, b.PowerSource);
	}

	[Theory]
	[InlineData("Charging", BatteryState.Charging)]
	[InlineData("Full", BatteryState.Full)]
	[InlineData("Not charging", BatteryState.NotCharging)]
	[InlineData("weird", BatteryState.Unknown)]
	[InlineData(null, BatteryState.Unknown)]
	public void MapsKernelStatus(string? status, BatteryState expected) =>
		Assert.Equal(expected, GtkBattery.ParseStatus(status));

	[Fact]
	public void IgnoresPeripheralBatteries()
	{
		using var dir = new TempDir();
		dir.File("hidpp_battery_0/type", "Battery");
		dir.File("hidpp_battery_0/scope", "Device");
		dir.File("hidpp_battery_0/capacity", "10");

		Assert.Equal(BatteryState.NotPresent, With(dir).State);
	}

	[Fact]
	public void PollRaisesOnlyOnChange()
	{
		using var dir = new TempDir();
		dir.File("BAT0/type", "Battery");
		dir.File("BAT0/capacity", "50");
		dir.File("BAT0/status", "Discharging");
		var b = With(dir);
		var raised = new List<BatteryInfoChangedEventArgs>();
		b.BatteryInfoChanged += (_, e) => raised.Add(e);

		b.Poll();
		Assert.Empty(raised);

		dir.File("BAT0/capacity", "49", overwrite: true);
		b.Poll();
		Assert.Single(raised);
		Assert.Equal(0.49, raised[0].ChargeLevel, 3);
	}
}

public class ConnectivityTests
{
	[Theory]
	[InlineData(false, GLib.NetworkConnectivity.Full, NetworkAccess.None)]
	[InlineData(true, GLib.NetworkConnectivity.Full, NetworkAccess.Internet)]
	[InlineData(true, GLib.NetworkConnectivity.Portal, NetworkAccess.ConstrainedInternet)]
	[InlineData(true, GLib.NetworkConnectivity.Limited, NetworkAccess.Local)]
	[InlineData(true, GLib.NetworkConnectivity.Local, NetworkAccess.Local)]
	public void MapsNetworkMonitorState(bool available, GLib.NetworkConnectivity c, NetworkAccess expected) =>
		Assert.Equal(expected, GtkConnectivity.ToNetworkAccess(available, c));

	[Fact]
	public void ClassifiesInterfacesFromSysfs()
	{
		using var dir = new TempDir();
		dir.File("lo/operstate", "unknown");
		dir.File("lo/type", "772");
		dir.File("wlp3s0/operstate", "up");
		dir.File("wlp3s0/type", "1");
		Directory.CreateDirectory(System.IO.Path.Combine(dir.Path, "wlp3s0", "wireless"));
		dir.File("enp2s0/operstate", "up");
		dir.File("enp2s0/type", "1");
		dir.File("wwan0/operstate", "up");
		dir.File("eth9/operstate", "down");
		dir.File("eth9/type", "1");

		var profiles = GtkConnectivity.ReadProfiles(dir.Path).ToList();

		Assert.Contains(ConnectionProfile.WiFi, profiles);
		Assert.Contains(ConnectionProfile.Ethernet, profiles);
		Assert.Contains(ConnectionProfile.Cellular, profiles);
		Assert.Equal(3, profiles.Count);
	}

	[Fact]
	public void MissingSysfsGivesNoProfiles() =>
		Assert.Empty(GtkConnectivity.ReadProfiles(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid())));
}

public class LaunchUriTests
{
	[Fact]
	public void BuildsMailtoWithEscaping()
	{
		var message = new EmailMessage("Hello & welcome", "Line 1\nLine 2", "a@x.org", "b@y.org")
		{
			Cc = new List<string> { "c@z.org" },
		};

		Assert.Equal(
			"mailto:a@x.org,b@y.org?cc=c@z.org&subject=Hello%20%26%20welcome&body=Line%201%0ALine%202",
			LaunchUris.Mailto(message));
	}

	[Fact]
	public void MailtoWithoutRecipientsOrFields() =>
		Assert.Equal("mailto:", LaunchUris.Mailto(new EmailMessage()));

	[Fact]
	public void BuildsGeoUrisInvariantly()
	{
		var old = System.Globalization.CultureInfo.CurrentCulture;
		try
		{
			System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
			Assert.Equal("geo:48.8584,2.2945", LaunchUris.Geo(48.8584, 2.2945));
			Assert.Equal("geo:-1.5,2?q=-1.5,2%28Tour%20Eiffel%29", LaunchUris.Geo(-1.5, 2, "Tour Eiffel"));
		}
		finally
		{
			System.Globalization.CultureInfo.CurrentCulture = old;
		}
	}

	[Fact]
	public void PlacemarkWithoutLocationBecomesASearch()
	{
		var uri = LaunchUris.GeoQuery(new Placemark { Thoroughfare = "Main St", SubThoroughfare = "5", Locality = "Springfield" });

		Assert.Equal("geo:0,0?q=" + Uri.EscapeDataString("5 Main St, Springfield"), uri);
		Assert.Null(LaunchUris.GeoQuery(new Placemark()));
	}

	[Fact]
	public void FileUriIsAbsolute() =>
		Assert.StartsWith("file:///", LaunchUris.FileUri(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "a b.txt")), StringComparison.Ordinal);
}

public class FilePickerSpecTests
{
	[Fact]
	public void ClassifiesMimeSuffixAndPatternEntries()
	{
		var spec = FileFilterSpec.Parse(new[] { "image/png", "image/*", "*.jpg", ".gif", "webp", "data*.bin", "*", "  " });

		Assert.Equal(new[] { "image/png", "image/*" }, spec.MimeTypes);
		Assert.Equal(new[] { "jpg", "gif", "webp" }, spec.Suffixes);
		Assert.Equal(new[] { "data*.bin" }, spec.Patterns);
	}

	[Fact]
	public void MatchAllEntriesMeanNoFilter()
	{
		Assert.True(FileFilterSpec.Parse(new[] { "*/*", "*.*" }).IsEmpty);
		Assert.True(FileFilterSpec.Parse(null).IsEmpty);
	}

	[Theory]
	[InlineData("a.PNG", "image/png")]
	[InlineData("a.mkv", "video/x-matroska")]
	[InlineData("a.unknownext", "application/octet-stream")]
	public void FallbackContentTypeTable(string name, string expected) =>
		Assert.Equal(expected, GtkFileResult.FromExtension(System.IO.Path.GetExtension(name)));

	[Fact]
	public async System.Threading.Tasks.Task FileResultOpensTheLocalFile()
	{
		using var dir = new TempDir();
		var path = dir.File("pic.png", "data");

		var result = new GtkFileResult(path);

		Assert.Equal("pic.png", result.FileName);
		using var reader = new StreamReader(await result.OpenReadAsync());
		Assert.Equal("data", await reader.ReadToEndAsync());
		Assert.False(string.IsNullOrEmpty(result.ContentType));
	}
}
