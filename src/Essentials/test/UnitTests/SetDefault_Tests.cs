#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using Xunit;

namespace Tests
{
	[CollectionDefinition("SetDefault", DisableParallelization = true)]
	public class SetDefaultCollection
	{
	}

	[Collection("SetDefault")]
	public class SetDefault_Tests
	{
		[Fact]
		public void AllEssentialsTypes_ExposePublicSetDefault()
		{
			var essentialsAssembly = typeof(Preferences).Assembly;
			var missingSetDefaultMethods = new List<string>();

			var typesWithInterfaceDefault = essentialsAssembly
				.GetTypes()
				.Where(t => t.IsClass && t.IsAbstract && t.IsSealed)
				.Where(t => t.Namespace?.StartsWith("Microsoft.Maui.", StringComparison.Ordinal) == true)
				.Select(t => new
				{
					Type = t,
					DefaultProperty = t.GetProperty("Default", BindingFlags.Public | BindingFlags.Static),
				})
				.Where(x =>
					x.DefaultProperty is not null &&
					x.DefaultProperty.PropertyType.IsInterface &&
					x.DefaultProperty.GetMethod?.IsPublic == true)
				.OrderBy(x => x.Type.FullName, StringComparer.Ordinal);

			foreach (var entry in typesWithInterfaceDefault)
			{
				var setDefaultMethod = entry.Type.GetMethod(
					"SetDefault",
					BindingFlags.Public | BindingFlags.Static,
					binder: null,
					types: new[] { entry.DefaultProperty!.PropertyType },
					modifiers: null);

				if (setDefaultMethod is null || setDefaultMethod.ReturnType != typeof(void))
				{
					missingSetDefaultMethods.Add($"{entry.Type.FullName}.SetDefault({entry.DefaultProperty!.PropertyType.FullName})");
				}
			}

			Assert.True(
				missingSetDefaultMethods.Count == 0,
				"Missing or invalid public static SetDefault methods:\n" + string.Join("\n", missingSetDefaultMethods));
		}

		// --- Geocoding SetDefault tests ---

		[Fact]
		public void Geocoding_SetDefault_SetsCustomImplementation()
		{
			var custom = new MockGeocoding();
			try
			{
				Geocoding.SetDefault(custom);
				Assert.Same(custom, Geocoding.Default);
			}
			finally
			{
				Geocoding.SetDefault(null);
			}
		}

		[Fact]
		public void Geocoding_SetDefault_Null_ResetsToDefault()
		{
			var custom = new MockGeocoding();
			Geocoding.SetDefault(custom);
			Geocoding.SetDefault(null);

			// After reset, Default should return a new platform default (not our mock)
			Assert.NotSame(custom, Geocoding.Default);
		}

		[Fact]
		public async Task Geocoding_SetDefault_CustomImplementation_IsUsed()
		{
			var custom = new MockGeocoding();
			try
			{
				Geocoding.SetDefault(custom);
				var results = await Geocoding.GetLocationsAsync("test address");
				Assert.Empty(results);
				Assert.True(custom.GetLocationsCalled);
			}
			finally
			{
				Geocoding.SetDefault(null);
			}
		}

		// --- Preferences SetDefault tests ---

		[Fact]
		public void Preferences_SetDefault_SetsCustomImplementation()
		{
			var custom = new MockPreferences();
			try
			{
				Preferences.SetDefault(custom);
				Assert.Same(custom, Preferences.Default);
			}
			finally
			{
				Preferences.SetDefault(null);
			}
		}

		[Fact]
		public void Preferences_SetDefault_Null_ResetsToDefault()
		{
			var custom = new MockPreferences();
			Preferences.SetDefault(custom);
			Preferences.SetDefault(null);

			Assert.NotSame(custom, Preferences.Default);
		}

		// --- FilePicker SetDefault tests ---

		[Fact]
		public void FilePicker_SetDefault_SetsCustomImplementation()
		{
			var custom = new MockFilePicker();
			try
			{
				FilePicker.SetDefault(custom);
				Assert.Same(custom, FilePicker.Default);
			}
			finally
			{
				FilePicker.SetDefault(null);
			}
		}

		[Fact]
		public void FilePicker_SetDefault_Null_ResetsToDefault()
		{
			var custom = new MockFilePicker();
			FilePicker.SetDefault(custom);
			FilePicker.SetDefault(null);

			Assert.NotSame(custom, FilePicker.Default);
		}

		// --- Clipboard SetDefault tests ---

		[Fact]
		public void Clipboard_SetDefault_SetsCustomImplementation()
		{
			var custom = new MockClipboard();
			try
			{
				Clipboard.SetDefault(custom);
				Assert.Same(custom, Clipboard.Default);
			}
			finally
			{
				Clipboard.SetDefault(null);
			}
		}

		[Fact]
		public async Task Clipboard_SetDefault_CustomImplementation_IsUsed()
		{
			var custom = new MockClipboard();
			try
			{
				Clipboard.SetDefault(custom);
				await Clipboard.SetTextAsync("test");
				Assert.True(custom.SetTextCalled);
			}
			finally
			{
				Clipboard.SetDefault(null);
			}
		}

		// --- SecureStorage SetDefault tests ---

		[Fact]
		public void SecureStorage_SetDefault_SetsCustomImplementation()
		{
			var custom = new MockSecureStorage();
			try
			{
				SecureStorage.SetDefault(custom);
				Assert.Same(custom, SecureStorage.Default);
			}
			finally
			{
				SecureStorage.SetDefault(null);
			}
		}

		[Fact]
		public async Task SecureStorage_SetDefault_CustomImplementation_IsUsed()
		{
			var custom = new MockSecureStorage();
			try
			{
				SecureStorage.SetDefault(custom);
				await SecureStorage.GetAsync("key");
				Assert.True(custom.GetAsyncCalled);
			}
			finally
			{
				SecureStorage.SetDefault(null);
			}
		}

		// --- Battery SetDefault tests ---

		[Fact]
		public void Battery_SetDefault_SetsCustomImplementation()
		{
			var custom = new MockBattery();
			try
			{
				Battery.SetDefault(custom);
				Assert.Same(custom, Battery.Default);
			}
			finally
			{
				Battery.SetDefault(null);
			}
		}

		[Fact]
		public void Battery_SetDefault_CustomImplementation_IsUsed()
		{
			var custom = new MockBattery();
			try
			{
				Battery.SetDefault(custom);
				Assert.Equal(0.5, Battery.ChargeLevel);
				Assert.Equal(BatteryState.Discharging, Battery.State);
			}
			finally
			{
				Battery.SetDefault(null);
			}
		}

		// --- Double-set test ---

		[Fact]
		public void Geocoding_SetDefault_SecondCallOverridesFirst()
		{
			var first = new MockGeocoding();
			var second = new MockGeocoding();
			try
			{
				Geocoding.SetDefault(first);
				Assert.Same(first, Geocoding.Default);

				Geocoding.SetDefault(second);
				Assert.Same(second, Geocoding.Default);
			}
			finally
			{
				Geocoding.SetDefault(null);
			}
		}

		// --- Mock implementations ---

		class MockGeocoding : IGeocoding
		{
			public bool GetLocationsCalled { get; private set; }

			public Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
				=> Task.FromResult<IEnumerable<Placemark>>(Array.Empty<Placemark>());

			public Task<IEnumerable<Location>> GetLocationsAsync(string address)
			{
				GetLocationsCalled = true;
				return Task.FromResult<IEnumerable<Location>>(Array.Empty<Location>());
			}
		}

		class MockPreferences : IPreferences
		{
			public bool ContainsKey(string key, string? sharedName = null) => false;
			public void Remove(string key, string? sharedName = null) { }
			public void Clear(string? sharedName = null) { }
			public void Set<T>(string key, T value, string? sharedName = null) { }
			public T Get<T>(string key, T defaultValue, string? sharedName = null) => defaultValue;
		}

		class MockFilePicker : IFilePicker
		{
			public Task<FileResult?> PickAsync(PickOptions? options = null)
				=> Task.FromResult<FileResult?>(null);

			public Task<IEnumerable<FileResult>?> PickMultipleAsync(PickOptions? options = null)
				=> Task.FromResult<IEnumerable<FileResult>?>(Array.Empty<FileResult>());
		}

		class MockClipboard : IClipboard
		{
			public bool HasText => false;
			public bool SetTextCalled { get; private set; }

			public Task SetTextAsync(string? text)
			{
				SetTextCalled = true;
				return Task.CompletedTask;
			}

			public Task<string?> GetTextAsync() => Task.FromResult<string?>(null);

			public event EventHandler<EventArgs>? ClipboardContentChanged;

			// Suppress unused event warning in mock
			internal void OnClipboardContentChanged() => ClipboardContentChanged?.Invoke(this, EventArgs.Empty);
		}

		class MockSecureStorage : ISecureStorage
		{
			public bool GetAsyncCalled { get; private set; }

			public Task<string?> GetAsync(string key)
			{
				GetAsyncCalled = true;
				return Task.FromResult<string?>(null);
			}

			public Task SetAsync(string key, string value) => Task.CompletedTask;
			public bool Remove(string key) => false;
			public void RemoveAll() { }
		}

		class MockBattery : IBattery
		{
			public double ChargeLevel => 0.5;
			public BatteryState State => BatteryState.Discharging;
			public BatteryPowerSource PowerSource => BatteryPowerSource.Battery;
			public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;

			public event EventHandler<BatteryInfoChangedEventArgs>? BatteryInfoChanged;
			public event EventHandler<EnergySaverStatusChangedEventArgs>? EnergySaverStatusChanged;

			internal void OnBatteryInfoChanged() => BatteryInfoChanged?.Invoke(this, new BatteryInfoChangedEventArgs(ChargeLevel, State, PowerSource));
			internal void OnEnergySaverStatusChanged() => EnergySaverStatusChanged?.Invoke(this, new EnergySaverStatusChangedEventArgs(EnergySaverStatus));
		}
	}
}
