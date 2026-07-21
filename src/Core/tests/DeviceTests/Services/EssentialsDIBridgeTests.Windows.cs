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

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void SharedMapServiceTokenRestoresAcrossOverlappingApps(bool disposeOlderFirst)
	{
		const string originalInstanceToken = "original-instance-token";
		const string originalPlatformToken = "original-platform-token";
		const string firstToken = "first-token";
		const string secondToken = "second-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var originalWindowsToken = Windows.Services.Maps.MapService.ServiceToken;
		var shared = new StubPlatformGeocoding { MapServiceToken = originalInstanceToken };
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			Windows.Services.Maps.MapService.ServiceToken = originalPlatformToken;

			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IGeocoding>(shared);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();
			Windows.Services.Maps.MapService.ServiceToken = firstToken;

			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IGeocoding>(shared);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondApp = secondBuilder.Build();
			Windows.Services.Maps.MapService.ServiceToken = secondToken;

			if (disposeOlderFirst)
			{
				firstApp.Dispose();
				firstApp = null;
				Assert.Equal(secondToken, shared.MapServiceToken);
				Assert.Equal(secondToken, Windows.Services.Maps.MapService.ServiceToken);

				secondApp.Dispose();
				secondApp = null;
			}
			else
			{
				secondApp.Dispose();
				secondApp = null;
				Assert.Equal(firstToken, shared.MapServiceToken);
				Assert.Equal(firstToken, Windows.Services.Maps.MapService.ServiceToken);

				firstApp.Dispose();
				firstApp = null;
			}

			Assert.Equal(originalInstanceToken, shared.MapServiceToken);
			Assert.Equal(originalPlatformToken, Windows.Services.Maps.MapService.ServiceToken);
		}
		finally
		{
			secondApp?.Dispose();
			firstApp?.Dispose();
			Windows.Services.Maps.MapService.ServiceToken = originalWindowsToken;
			RestoreGeocoding(original, originalToken);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void DistinctGeocodingMapServiceTokenRestoresAcrossOverlappingApps(bool disposeOlderFirst)
	{
		const string originalPlatformToken = "original-platform-token";
		const string firstToken = "first-token";
		const string secondToken = "second-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var originalWindowsToken = Windows.Services.Maps.MapService.ServiceToken;
		var first = new StubPlatformGeocoding { MapServiceToken = "first-original-token" };
		var second = new StubPlatformGeocoding { MapServiceToken = "second-original-token" };
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			Windows.Services.Maps.MapService.ServiceToken = originalPlatformToken;

			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IGeocoding>(first);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();
			Windows.Services.Maps.MapService.ServiceToken = firstToken;

			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IGeocoding>(second);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondApp = secondBuilder.Build();
			Windows.Services.Maps.MapService.ServiceToken = secondToken;

			if (disposeOlderFirst)
			{
				firstApp.Dispose();
				firstApp = null;
				Assert.Equal("first-original-token", first.MapServiceToken);
				Assert.Equal(secondToken, Windows.Services.Maps.MapService.ServiceToken);

				secondApp.Dispose();
				secondApp = null;
			}
			else
			{
				secondApp.Dispose();
				secondApp = null;
				Assert.Equal("second-original-token", second.MapServiceToken);
				Assert.Equal(firstToken, Windows.Services.Maps.MapService.ServiceToken);

				firstApp.Dispose();
				firstApp = null;
			}

			Assert.Equal("first-original-token", first.MapServiceToken);
			Assert.Equal("second-original-token", second.MapServiceToken);
			Assert.Equal(originalPlatformToken, Windows.Services.Maps.MapService.ServiceToken);
		}
		finally
		{
			secondApp?.Dispose();
			firstApp?.Dispose();
			Windows.Services.Maps.MapService.ServiceToken = originalWindowsToken;
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
