#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
			public bool ContainsKey(string key) => false;
			public bool ContainsKey(string key, string? sharedName) => false;
			public void Remove(string key) { }
			public void Remove(string key, string? sharedName) { }
			public void Clear() { }
			public void Clear(string? sharedName) { }
			public void Set<T>(string key, T value) { }
			public void Set<T>(string key, T value, string? sharedName) { }
			public T Get<T>(string key, T defaultValue) => defaultValue;
			public T Get<T>(string key, T defaultValue, string? sharedName) => defaultValue;
		}

		class MockFilePicker : IFilePicker
		{
			public Task<FileResult?> PickAsync(PickOptions? options = null)
				=> Task.FromResult<FileResult?>(null);

			public Task<IEnumerable<FileResult>?> PickMultipleAsync(PickOptions? options = null)
				=> Task.FromResult<IEnumerable<FileResult>?>(Array.Empty<FileResult>());
		}
	}
}
