#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class EssentialsDIBridgeTests : IDisposable
	{
		public void Dispose()
		{
			// Reset all Essentials static backing fields after each test via reflection.
			// SetDefault/SetCurrent are internal, so we clear the backing fields directly.
			ResetStaticField(typeof(Accelerometer), "defaultImplementation");
			ResetStaticField(typeof(Barometer), "defaultImplementation");
			ResetStaticField(typeof(Battery), "defaultImplementation");
			ResetStaticField(typeof(Browser), "defaultImplementation");
			ResetStaticField(typeof(Clipboard), "defaultImplementation");
			ResetStaticField(typeof(Compass), "defaultImplementation");
			ResetStaticField(typeof(Contacts), "defaultImplementation");
			ResetStaticField(typeof(Email), "defaultImplementation");
			ResetStaticField(typeof(FilePicker), "defaultImplementation");
			ResetStaticField(typeof(Flashlight), "defaultImplementation");
			ResetStaticField(typeof(Geolocation), "defaultImplementation");
			ResetStaticField(typeof(Gyroscope), "defaultImplementation");
			ResetStaticField(typeof(HapticFeedback), "defaultImplementation");
			ResetStaticField(typeof(Launcher), "defaultImplementation");
			ResetStaticField(typeof(Magnetometer), "defaultImplementation");
			ResetStaticField(typeof(Map), "defaultImplementation");
			ResetStaticField(typeof(MediaPicker), "defaultImplementation");
			ResetStaticField(typeof(OrientationSensor), "defaultImplementation");
			ResetStaticField(typeof(PhoneDialer), "defaultImplementation");
			ResetStaticField(typeof(Preferences), "defaultImplementation");
			ResetStaticField(typeof(Screenshot), "defaultImplementation");
			ResetStaticField(typeof(SecureStorage), "defaultImplementation");
			ResetStaticField(typeof(SemanticScreenReader), "defaultImplementation");
			ResetStaticField(typeof(Share), "defaultImplementation");
			ResetStaticField(typeof(Sms), "defaultImplementation");
			ResetStaticField(typeof(TextToSpeech), "defaultImplementation");
			ResetStaticField(typeof(VersionTracking), "defaultImplementation");
			ResetStaticField(typeof(Vibration), "defaultImplementation");
			ResetStaticField(typeof(WebAuthenticator), "defaultImplementation");
			ResetStaticField(typeof(AppleSignInAuthenticator), "defaultImplementation");

			ResetStaticField(typeof(AppActions), "currentImplementation");
			ResetStaticField(typeof(AppInfo), "currentImplementation");
			ResetStaticField(typeof(Connectivity), "currentImplementation");
			ResetStaticField(typeof(DeviceDisplay), "currentImplementation");
			ResetStaticField(typeof(DeviceInfo), "currentImplementation");
			ResetStaticField(typeof(FileSystem), "currentImplementation");
			// Geocoding is a special case: uses SetCurrent but the backing field is defaultImplementation
			ResetStaticField(typeof(Geocoding), "defaultImplementation");
		}

		static void ResetStaticField(Type type, string fieldName)
		{
			var field = type.GetField(fieldName,
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			field?.SetValue(null, null);
		}

		[Fact]
		public void DIRegisteredPreferences_BridgedToStaticFacade()
		{
			var mock = new StubPreferences();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Preferences.Default);
		}

		[Fact]
		public void DIRegisteredBattery_BridgedToStaticFacade()
		{
			var mock = new StubBattery();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IBattery>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Battery.Default);
		}

		[Fact]
		public void DIRegisteredAppInfo_BridgedToStaticFacade()
		{
			var mock = new StubAppInfo();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppInfo>(mock);

			using var app = builder.Build();

			Assert.Same(mock, AppInfo.Current);
		}

		[Fact]
		public void DIRegisteredConnectivity_BridgedToStaticFacade()
		{
			var mock = new StubConnectivity();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IConnectivity>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Connectivity.Current);
		}

		[Fact]
		public void NoDIRegistration_StaticFacadeUnchanged()
		{
			// When nothing is registered in DI, the backing field should remain null
			// (the lazy ??= will create the platform default on first access).
			var builder = MauiApp.CreateBuilder();
			using var app = builder.Build();

			// Verify the setter was NOT called — field is still null.
			// Accessing .Default would trigger lazy init, so we use reflection.
			var field = typeof(Preferences).GetField("defaultImplementation",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			Assert.NotNull(field);
			Assert.Null(field!.GetValue(null));
		}

		[Fact]
		public void MultipleDIRegistrations_AllBridged()
		{
			var mockPrefs = new StubPreferences();
			var mockBattery = new StubBattery();
			var mockAppInfo = new StubAppInfo();

			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mockPrefs);
			builder.Services.AddSingleton<IBattery>(mockBattery);
			builder.Services.AddSingleton<IAppInfo>(mockAppInfo);

			using var app = builder.Build();

			Assert.Same(mockPrefs, Preferences.Default);
			Assert.Same(mockBattery, Battery.Default);
			Assert.Same(mockAppInfo, AppInfo.Current);
		}

		[Fact]
		public void DIBridge_WorksWithConfigureEssentials()
		{
			// Ensure bridge works even when ConfigureEssentials is also called
			var mock = new StubPreferences();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mock);
			builder.ConfigureEssentials();

			using var app = builder.Build();

			Assert.Same(mock, Preferences.Default);
		}

		[Fact]
		public void DIRegisteredGeocoding_BridgedToDefaultProperty()
		{
			// Geocoding is unique: uses SetCurrent internally but exposes Default property,
			// and its backing field is named defaultImplementation (not currentImplementation).
			var mock = new StubGeocoding();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IGeocoding>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Geocoding.Default);
		}

		[Fact]
		public void NoDIRegistration_SetCurrentTypeUnchanged()
		{
			// Verify SetCurrent types are also unset when nothing is registered in DI.
			var builder = MauiApp.CreateBuilder();
			using var app = builder.Build();

			var field = typeof(AppInfo).GetField("currentImplementation",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			Assert.NotNull(field);
			Assert.Null(field!.GetValue(null));
		}

		[Fact]
		public void TransientDIRegistration_StillBridged()
		{
			// Even with transient lifetime, the bridge resolves and sets the instance.
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IPreferences, StubPreferences>();

			using var app = builder.Build();

			Assert.IsType<StubPreferences>(Preferences.Default);
		}

		// Stub implementations for testing
		class StubPreferences : IPreferences
		{
			public bool ContainsKey(string key, string? sharedName = null) => false;
			public void Remove(string key, string? sharedName = null) { }
			public void Clear(string? sharedName = null) { }
			public void Set<T>(string key, T value, string? sharedName = null) { }
			public T Get<T>(string key, T defaultValue, string? sharedName = null) => defaultValue;
		}

		class StubBattery : IBattery
		{
			public double ChargeLevel => 1.0;
			public BatteryState State => BatteryState.Full;
			public BatteryPowerSource PowerSource => BatteryPowerSource.AC;
			public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;
			public event EventHandler<BatteryInfoChangedEventArgs>? BatteryInfoChanged { add { } remove { } }
			public event EventHandler<EnergySaverStatusChangedEventArgs>? EnergySaverStatusChanged { add { } remove { } }
		}

		class StubAppInfo : IAppInfo
		{
			public string PackageName => "test.package";
			public string Name => "Test";
			public string VersionString => "1.0.0";
			public Version Version => new Version(1, 0, 0);
			public string BuildString => "1";
			public AppTheme RequestedTheme => AppTheme.Light;
			public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;
			public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;
			public void ShowSettingsUI() { }
		}

		class StubConnectivity : IConnectivity
		{
			public NetworkAccess NetworkAccess => NetworkAccess.Internet;
			public IEnumerable<ConnectionProfile> ConnectionProfiles => Array.Empty<ConnectionProfile>();
			public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged { add { } remove { } }
		}

		class StubGeocoding : IGeocoding
		{
			public Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
				Task.FromResult<IEnumerable<Placemark>>(Array.Empty<Placemark>());
			public Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
				Task.FromResult<IEnumerable<Location>>(Array.Empty<Location>());
		}
	}
}
