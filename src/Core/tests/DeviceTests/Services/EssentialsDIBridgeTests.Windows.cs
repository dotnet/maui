#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Services;

[Category(TestCategory.Application)]
[Collection(EssentialsStaticStateCollection.Name)]
public class EssentialsDIBridgeTests
{
	[Fact]
	public void MauiAppBuildWithoutMapTokenDoesNotInitializeGeocoding()
	{
		var field = typeof(Geocoding).GetField(
			"defaultImplementation",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
			?? throw new InvalidOperationException("Geocoding backing field was not found.");
		var original = field.GetValue(null);

		try
		{
			field.SetValue(null, null);

			var builder = MauiApp.CreateBuilder();
			using var app = builder.Build();

			Assert.Null(field.GetValue(null));
		}
		finally
		{
			field.SetValue(null, original);
		}
	}

	[Fact]
	public void ConfiguredMapServiceTokenIsForwardedToPlatform()
	{
		const string token = "test-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;

		try
		{
			var builder = MauiApp.CreateBuilder();
			builder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(token));

			using var app = builder.Build();

			Assert.Equal(token, Microsoft.Maui.ApplicationModel.Platform.MapServiceToken);
		}
		finally
		{
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public void MapServiceTokenIsForwardedToDIPlatformGeocoding()
	{
		const string configuredToken = "configured-token";
		const string existingToken = "existing-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;

		try
		{
			var configured = new StubPlatformGeocoding();
			var configuredBuilder = MauiApp.CreateBuilder();
			configuredBuilder.Services.AddSingleton<IGeocoding>(configured);
			configuredBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(configuredToken));

			using (var app = configuredBuilder.Build())
			{
				Assert.Same(configured, Geocoding.Default);
				Assert.Equal(configuredToken, configured.MapServiceToken);
			}

			Microsoft.Maui.ApplicationModel.Platform.MapServiceToken = existingToken;
			var existing = new StubPlatformGeocoding();
			var existingBuilder = MauiApp.CreateBuilder();
			existingBuilder.Services.AddSingleton<IGeocoding>(existing);

			using (var app = existingBuilder.Build())
			{
				Assert.Same(existing, Geocoding.Default);
				Assert.Equal(existingToken, existing.MapServiceToken);
			}
		}
		finally
		{
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public void ConfiguredMapServiceTokenIsNotAppliedWithoutPlatformContract()
	{
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var replacement = new StubGeocoding();

		try
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IGeocoding>(replacement);
			builder.ConfigureEssentials(essentials => essentials.UseMapServiceToken("test-token"));

			using var app = builder.Build();

			Assert.Same(replacement, Geocoding.Default);
		}
		finally
		{
			RestoreGeocoding(original, originalToken);
		}
	}

	static void RestoreGeocoding(IGeocoding original, string? originalToken)
	{
		var builder = MauiApp.CreateBuilder();
		builder.Services.AddSingleton(original);

		using var app = builder.Build();

		if (original is IPlatformGeocoding platformGeocoding)
			platformGeocoding.MapServiceToken = originalToken;
	}

	class StubGeocoding : IGeocoding
	{
		public Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
			Task.FromResult<IEnumerable<Placemark>>(Array.Empty<Placemark>());

		public Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
			Task.FromResult<IEnumerable<Location>>(Array.Empty<Location>());
	}

	sealed class StubPlatformGeocoding : StubGeocoding, IPlatformGeocoding
	{
		public string? MapServiceToken { get; set; }
	}
}
