#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Services;

[Category(TestCategory.Application)]
[Collection(EssentialsStaticStateCollection.Name)]
public class EssentialsDIBridgeTests
{
	[Fact]
	public void WebAuthenticatorBridgeRequiresWindowsLifecycleContract()
	{
		var original = WebAuthenticator.Default;

		try
		{
			using (var app = BuildApp<IWebAuthenticator>(new StubWebAuthenticator()))
				Assert.Same(original, WebAuthenticator.Default);

			var replacement = new StubPlatformWebAuthenticator();
			using (var app = BuildApp<IWebAuthenticator>(replacement))
				Assert.Same(replacement, WebAuthenticator.Default);
		}
		finally
		{
			using var app = BuildApp<IWebAuthenticator>(original);
		}

		Assert.Same(original, WebAuthenticator.Default);
	}

	[Fact]
	public void MauiAppBuildWithoutMapTokenDoesNotInitializeGeocoding()
	{
		var field = GetGeocodingBackingField();
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
	public void NullMapServiceTokenBehavesAsUnconfigured()
	{
		var field = GetGeocodingBackingField();
		var original = field.GetValue(null);

		try
		{
			field.SetValue(null, null);

			var builder = MauiApp.CreateBuilder();
			builder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(null!));
			using var app = builder.Build();

			Assert.Null(field.GetValue(null));
		}
		finally
		{
			field.SetValue(null, original);
		}
	}

	[Fact]
	public void RestoreGeocodingReplacesLeakedFacade()
	{
		var field = GetGeocodingBackingField();
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;

		try
		{
			field.SetValue(null, new StubGeocoding());

			RestoreGeocoding(original, originalToken);

			Assert.Same(original, field.GetValue(null));
		}
		finally
		{
			field.SetValue(null, original);

			if (original is IPlatformGeocoding platformGeocoding)
				platformGeocoding.MapServiceToken = originalToken;
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

	[Fact]
	public void ThrowingMapServiceTokenSetterDoesNotLeakAssignment()
	{
		const string originalPlatformToken = "original-platform-token";
		const string configuredToken = "throw-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var initialAssignmentCount = GetMapTokenAssignmentCount();
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var replacement = new ThrowingPlatformGeocoding();

		try
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IGeocoding>(replacement);
			builder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(configuredToken));

			var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

			Assert.Equal("map token failed", exception.Message);
			Assert.Equal(initialAssignmentCount, GetMapTokenAssignmentCount());
			Assert.Equal(originalPlatformToken, platformToken.Value);
			Assert.Same(original, Geocoding.Default);
		}
		finally
		{
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public async Task FailedMapServiceTokenApplyRestoresPlatformTokenAfterPredecessorDisposes()
	{
		const string originalInstanceToken = "original-instance-token";
		const string originalPlatformToken = "original-platform-token";
		const string firstToken = "first-token";
		const string secondToken = "second-token";
		var timeout = TimeSpan.FromSeconds(30);
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var initialAssignmentCount = GetMapTokenAssignmentCount();
		var initialStateCount = GetMapTokenImplementationStateCount();
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var secondSetterEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var releaseSecondSetter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var first = new StubPlatformGeocoding { MapServiceToken = originalInstanceToken };
		var second = new CallbackPlatformGeocoding(value =>
		{
			if (!string.Equals(value, secondToken, StringComparison.Ordinal))
				return;

			platformToken.Value = secondToken;
			secondSetterEntered.TrySetResult(true);
			Assert.True(
				releaseSecondSetter.Task.Wait(timeout),
				"Timed out waiting to release the failing map-token setter.");
			throw new InvalidOperationException("map token failed");
		});
		MauiApp? firstApp = null;
		Task<MauiApp>? secondBuild = null;

		try
		{
			var firstBuilder = MauiApp.CreateBuilder(useDefaults: false);
			firstBuilder.Services.AddSingleton<IGeocoding>(first);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();
			platformToken.Value = firstToken;

			var secondBuilder = MauiApp.CreateBuilder(useDefaults: false);
			secondBuilder.Services.AddSingleton<IGeocoding>(second);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondBuild = Task.Run(secondBuilder.Build);

			await secondSetterEntered.Task.WaitAsync(timeout);

			firstApp.Dispose();
			firstApp = null;

			releaseSecondSetter.TrySetResult(true);
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(
				() => secondBuild.WaitAsync(timeout));

			Assert.Equal("map token failed", exception.Message);
			Assert.Equal(originalPlatformToken, platformToken.Value);
			Assert.Equal(initialAssignmentCount, GetMapTokenAssignmentCount());
			Assert.Equal(initialStateCount, GetMapTokenImplementationStateCount());
			Assert.Same(original, Geocoding.Default);
		}
		finally
		{
			releaseSecondSetter.TrySetResult(true);
			if (secondBuild is not null)
				await Record.ExceptionAsync(() => secondBuild.WaitAsync(timeout));

			firstApp?.Dispose();
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public async Task ConcurrentBuildAppliesMapServiceTokenToOwningGeocoder()
	{
		const string token = "first-token";
		var timeout = TimeSpan.FromSeconds(30);
		var field = GetGeocodingBackingField();
		var original = field.GetValue(null);
		var firstTokenSetterEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var releaseFirstTokenSetter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var first = new CallbackPlatformGeocoding(value =>
		{
			if (!string.Equals(value, token, StringComparison.Ordinal))
				return;

			firstTokenSetterEntered.TrySetResult(true);
			Assert.True(
				releaseFirstTokenSetter.Task.Wait(timeout),
				"Timed out waiting to release the map-token setter.");
		});
		var second = new StubPlatformGeocoding();
		Task<MauiApp>? firstBuild = null;
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			field.SetValue(null, null);

			var firstBuilder = MauiApp.CreateBuilder(useDefaults: false);
			firstBuilder.Services.AddSingleton<IGeocoding>(first);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(token));

			// The map-token setter (rather than EssentialsCleanup resolution) is the right
			// place to stall the first build: it runs after Geocoding.Default has already
			// been bridged to `first`, so the assertion below observes the state the real
			// initialization pipeline produces instead of racing ahead of it.
			firstBuild = Task.Run(firstBuilder.Build);
			await firstTokenSetterEntered.Task.WaitAsync(timeout);
			Assert.Same(first, Geocoding.Default);

			var secondBuilder = MauiApp.CreateBuilder(useDefaults: false);
			secondBuilder.Services.AddSingleton<IGeocoding>(second);
			secondBuilder.ConfigureEssentials();
			secondApp = secondBuilder.Build();
			Assert.Same(second, Geocoding.Default);

			releaseFirstTokenSetter.TrySetResult(true);
			firstApp = await firstBuild.WaitAsync(timeout);

			Assert.Equal(token, first.MapServiceToken);
			Assert.Null(second.MapServiceToken);
			Assert.Same(second, Geocoding.Default);
		}
		finally
		{
			releaseFirstTokenSetter.TrySetResult(true);
			if (firstBuild is not null && firstApp is null)
				firstApp = await firstBuild.WaitAsync(timeout);

			secondApp?.Dispose();
			firstApp?.Dispose();
			field.SetValue(null, original);
		}
	}

	[Fact]
	public async Task MapServiceTokenSetterDoesNotBlockConcurrentAppBuild()
	{
		const string firstToken = "first-token";
		const string secondToken = "second-token";
		var timeout = TimeSpan.FromSeconds(30);
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var firstSetterEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondSetterCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var first = new CallbackPlatformGeocoding(value =>
		{
			if (!string.Equals(value, firstToken, StringComparison.Ordinal))
				return;

			firstSetterEntered.TrySetResult(true);
			Assert.True(
				secondSetterCompleted.Task.Wait(timeout),
				"The first map-token setter blocked the second app build under the global bookkeeping lock.");
		});
		var second = new CallbackPlatformGeocoding(value =>
		{
			if (string.Equals(value, secondToken, StringComparison.Ordinal))
				secondSetterCompleted.TrySetResult(true);
		});
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			// Isolate the Essentials initializer from the default Windows MauiCoreInitializer,
			// which requires the UI application dispatcher and cannot be built on Task.Run.
			var firstBuilder = MauiApp.CreateBuilder(useDefaults: false);
			firstBuilder.Services.AddSingleton<IGeocoding>(first);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			var firstBuild = Task.Run(firstBuilder.Build);

			await firstSetterEntered.Task.WaitAsync(timeout);

			var secondBuilder = MauiApp.CreateBuilder(useDefaults: false);
			secondBuilder.Services.AddSingleton<IGeocoding>(second);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			var secondBuild = Task.Run(secondBuilder.Build);

			var apps = await Task.WhenAll(firstBuild, secondBuild).WaitAsync(timeout);
			firstApp = apps[0];
			secondApp = apps[1];

			Assert.Equal(firstToken, first.MapServiceToken);
			Assert.Equal(secondToken, second.MapServiceToken);
		}
		finally
		{
			secondApp?.Dispose();
			firstApp?.Dispose();
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public void MapServiceTokenCleanupRestoresPlatformTokenWhenGeocoderRestoreThrows()
	{
		const string originalInstanceToken = "original-instance-token";
		const string originalPlatformToken = "original-platform-token";
		const string configuredToken = "configured-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var initialAssignmentCount = GetMapTokenAssignmentCount();
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var replacement = new ThrowingRestorePlatformGeocoding(originalInstanceToken);
		MauiApp? app = null;

		try
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IGeocoding>(replacement);
			builder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(configuredToken));
			app = builder.Build();
			platformToken.Value = configuredToken;
			replacement.ThrowOnToken = originalInstanceToken;

			var exception = Assert.Throws<InvalidOperationException>(() => app.Dispose());
			app = null;

			Assert.Equal("geocoder map token restore failed", exception.Message);
			Assert.Equal(configuredToken, replacement.MapServiceToken);
			Assert.Equal(originalPlatformToken, platformToken.Value);
			Assert.Equal(initialAssignmentCount, GetMapTokenAssignmentCount());
			Assert.Same(original, Geocoding.Default);
		}
		finally
		{
			replacement.ThrowOnToken = null;
			app?.Dispose();
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public void MapServiceTokenCleanupAggregatesGeocoderAndPlatformRestoreFailures()
	{
		const string originalInstanceToken = "original-instance-token";
		const string originalPlatformToken = "original-platform-token";
		const string configuredToken = "configured-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var initialAssignmentCount = GetMapTokenAssignmentCount();
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var replacement = new ThrowingRestorePlatformGeocoding(originalInstanceToken);
		MauiApp? app = null;

		try
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IGeocoding>(replacement);
			builder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(configuredToken));
			app = builder.Build();
			platformToken.Value = configuredToken;
			replacement.ThrowOnToken = originalInstanceToken;
			platformToken.ThrowOnToken = originalPlatformToken;

			var aggregate = Assert.Throws<AggregateException>(() => app.Dispose());
			app = null;

			Assert.Collection(
				aggregate.InnerExceptions,
				exception => Assert.Equal("geocoder map token restore failed", exception.Message),
				exception => Assert.Equal("platform map token restore failed", exception.Message));
			Assert.Equal(configuredToken, replacement.MapServiceToken);
			Assert.Equal(configuredToken, platformToken.Value);
			Assert.Equal(initialAssignmentCount, GetMapTokenAssignmentCount());
			Assert.Same(original, Geocoding.Default);
		}
		finally
		{
			replacement.ThrowOnToken = null;
			platformToken.ThrowOnToken = null;
			app?.Dispose();
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public void NewerMapServiceTokenCleanupRestoresOlderUnpushedPlatformToken()
	{
		const string originalInstanceToken = "original-instance-token";
		const string originalPlatformToken = "original-platform-token";
		const string firstToken = "first-token";
		const string secondToken = "second-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var shared = new StubPlatformGeocoding { MapServiceToken = originalInstanceToken };
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IGeocoding>(shared);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();

			Assert.Equal(firstToken, shared.MapServiceToken);
			Assert.Equal(originalPlatformToken, platformToken.Value);

			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IGeocoding>(shared);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondApp = secondBuilder.Build();
			platformToken.Value = secondToken;

			secondApp.Dispose();
			secondApp = null;

			Assert.Equal(firstToken, shared.MapServiceToken);
			Assert.Equal(firstToken, platformToken.Value);

			firstApp.Dispose();
			firstApp = null;

			Assert.Equal(originalInstanceToken, shared.MapServiceToken);
			Assert.Equal(originalPlatformToken, platformToken.Value);
		}
		finally
		{
			secondApp?.Dispose();
			firstApp?.Dispose();
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
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var shared = new StubPlatformGeocoding { MapServiceToken = originalInstanceToken };
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IGeocoding>(shared);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();
			platformToken.Value = firstToken;

			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IGeocoding>(shared);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondApp = secondBuilder.Build();
			platformToken.Value = secondToken;

			if (disposeOlderFirst)
			{
				firstApp.Dispose();
				firstApp = null;
				Assert.Equal(secondToken, shared.MapServiceToken);
				Assert.Equal(secondToken, platformToken.Value);

				secondApp.Dispose();
				secondApp = null;
			}
			else
			{
				secondApp.Dispose();
				secondApp = null;
				Assert.Equal(firstToken, shared.MapServiceToken);
				Assert.Equal(firstToken, platformToken.Value);

				firstApp.Dispose();
				firstApp = null;
			}

			Assert.Equal(originalInstanceToken, shared.MapServiceToken);
			Assert.Equal(originalPlatformToken, platformToken.Value);
		}
		finally
		{
			secondApp?.Dispose();
			firstApp?.Dispose();
			RestoreGeocoding(original, originalToken);
		}
	}

	[Fact]
	public async Task ConcurrentSharedMapServiceTokenCleanupRestoresOriginalToken()
	{
		const string originalInstanceToken = "original-instance-token";
		const string firstToken = "first-token";
		const string secondToken = "second-token";
		var timeout = TimeSpan.FromSeconds(30);
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		var initialAssignmentCount = GetMapTokenAssignmentCount();
		var initialStateCount = GetMapTokenImplementationStateCount();
		var restoreEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var releaseRestore = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var blockRestore = 0;
		var restoreBlocked = 0;
		var shared = new CallbackPlatformGeocoding(value =>
		{
			if (Volatile.Read(ref blockRestore) == 1 &&
				string.Equals(value, firstToken, StringComparison.Ordinal) &&
				Interlocked.CompareExchange(ref restoreBlocked, 1, 0) == 0)
			{
				restoreEntered.TrySetResult(true);
				Assert.True(releaseRestore.Task.Wait(timeout));
			}
		})
		{
			MapServiceToken = originalInstanceToken,
		};
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IGeocoding>(shared);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();

			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IGeocoding>(shared);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondApp = secondBuilder.Build();

			Volatile.Write(ref blockRestore, 1);
			var secondDispose = Task.Run(secondApp.Dispose);
			await restoreEntered.Task.WaitAsync(timeout);

			var firstDispose = Task.Run(firstApp.Dispose);
			await firstDispose.WaitAsync(timeout);
			firstApp = null;

			releaseRestore.TrySetResult(true);
			await secondDispose.WaitAsync(timeout);
			secondApp = null;

			Assert.Equal(originalInstanceToken, shared.MapServiceToken);
			Assert.Equal(initialAssignmentCount, GetMapTokenAssignmentCount());
			Assert.Equal(initialStateCount, GetMapTokenImplementationStateCount());
		}
		finally
		{
			releaseRestore.TrySetResult(true);
			secondApp?.Dispose();
			firstApp?.Dispose();
			RestoreGeocoding(original, originalToken);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void OlderMapServiceTokenCleanupAppliesLatestUnappliedSuccessorToken(bool disposeSecondFirst)
	{
		const string originalInstanceToken = "original-instance-token";
		const string originalPlatformToken = "original-platform-token";
		const string firstToken = "first-token";
		const string secondToken = "second-token";
		const string thirdToken = "third-token";
		var original = Geocoding.Default;
		var originalToken = (original as IPlatformGeocoding)?.MapServiceToken;
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var shared = new StubPlatformGeocoding { MapServiceToken = originalInstanceToken };
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;
		MauiApp? thirdApp = null;

		try
		{
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IGeocoding>(shared);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();
			platformToken.Value = firstToken;

			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IGeocoding>(shared);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondApp = secondBuilder.Build();

			var thirdBuilder = MauiApp.CreateBuilder();
			thirdBuilder.Services.AddSingleton<IGeocoding>(shared);
			thirdBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(thirdToken));
			thirdApp = thirdBuilder.Build();

			Assert.Equal(thirdToken, shared.MapServiceToken);
			Assert.Equal(firstToken, platformToken.Value);

			firstApp.Dispose();
			firstApp = null;

			Assert.Equal(thirdToken, shared.MapServiceToken);
			Assert.Equal(thirdToken, platformToken.Value);

			if (disposeSecondFirst)
			{
				secondApp.Dispose();
				secondApp = null;
				Assert.Equal(thirdToken, shared.MapServiceToken);
				Assert.Equal(thirdToken, platformToken.Value);

				thirdApp.Dispose();
				thirdApp = null;
			}
			else
			{
				thirdApp.Dispose();
				thirdApp = null;
				Assert.Equal(secondToken, shared.MapServiceToken);
				Assert.Equal(secondToken, platformToken.Value);

				secondApp.Dispose();
				secondApp = null;
			}

			Assert.Equal(originalInstanceToken, shared.MapServiceToken);
			Assert.Equal(originalPlatformToken, platformToken.Value);
		}
		finally
		{
			thirdApp?.Dispose();
			secondApp?.Dispose();
			firstApp?.Dispose();
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
		using var platformToken = new WindowsMapServiceTokenScope(originalPlatformToken);
		var first = new StubPlatformGeocoding { MapServiceToken = "first-original-token" };
		var second = new StubPlatformGeocoding { MapServiceToken = "second-original-token" };
		MauiApp? firstApp = null;
		MauiApp? secondApp = null;

		try
		{
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IGeocoding>(first);
			firstBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(firstToken));
			firstApp = firstBuilder.Build();
			platformToken.Value = firstToken;

			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IGeocoding>(second);
			secondBuilder.ConfigureEssentials(essentials => essentials.UseMapServiceToken(secondToken));
			secondApp = secondBuilder.Build();
			platformToken.Value = secondToken;

			if (disposeOlderFirst)
			{
				firstApp.Dispose();
				firstApp = null;
				Assert.Equal("first-original-token", first.MapServiceToken);
				Assert.Equal(secondToken, platformToken.Value);

				secondApp.Dispose();
				secondApp = null;
			}
			else
			{
				secondApp.Dispose();
				secondApp = null;
				Assert.Equal("second-original-token", second.MapServiceToken);
				Assert.Equal(firstToken, platformToken.Value);

				firstApp.Dispose();
				firstApp = null;
			}

			Assert.Equal("first-original-token", first.MapServiceToken);
			Assert.Equal("second-original-token", second.MapServiceToken);
			Assert.Equal(originalPlatformToken, platformToken.Value);
		}
		finally
		{
			secondApp?.Dispose();
			firstApp?.Dispose();
			RestoreGeocoding(original, originalToken);
		}
	}

	static int GetMapTokenAssignmentCount()
	{
		var field = typeof(EssentialsExtensions).GetField(
			"s_mapTokenAssignments",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
			?? throw new InvalidOperationException("Map-token assignment field was not found.");

		return ((System.Collections.ICollection)field.GetValue(null)!).Count;
	}

	static int GetMapTokenImplementationStateCount()
	{
		var field = typeof(EssentialsExtensions).GetField(
			"s_mapTokenImplementationStates",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
			?? throw new InvalidOperationException("Map-token implementation-state field was not found.");

		return ((System.Collections.ICollection)field.GetValue(null)!).Count;
	}

	sealed class WindowsMapServiceTokenScope : IDisposable
	{
		readonly Func<string?> _originalGetter = EssentialsExtensions.WindowsMapServiceTokenGetter;
		readonly Action<string?> _originalSetter = EssentialsExtensions.WindowsMapServiceTokenSetter;

		public WindowsMapServiceTokenScope(string? value)
		{
			Value = value;
			EssentialsExtensions.WindowsMapServiceTokenGetter = () => Value;
			EssentialsExtensions.WindowsMapServiceTokenSetter = token =>
			{
				if (ThrowOnToken is not null &&
					string.Equals(token, ThrowOnToken, StringComparison.Ordinal))
				{
					throw new InvalidOperationException("platform map token restore failed");
				}

				Value = token;
			};
		}

		public string? ThrowOnToken { get; set; }

		public string? Value { get; set; }

		public void Dispose()
		{
			EssentialsExtensions.WindowsMapServiceTokenGetter = _originalGetter;
			EssentialsExtensions.WindowsMapServiceTokenSetter = _originalSetter;
		}
	}

	static void RestoreGeocoding(IGeocoding original, string? originalToken)
	{
		GetGeocodingBackingField().SetValue(null, original);

		if (original is IPlatformGeocoding platformGeocoding)
			platformGeocoding.MapServiceToken = originalToken;
	}

	static System.Reflection.FieldInfo GetGeocodingBackingField() =>
		typeof(Geocoding).GetField(
			"defaultImplementation",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
		?? throw new InvalidOperationException("Geocoding backing field was not found.");

	static MauiApp BuildApp<TService>(TService service)
		where TService : class
	{
		var builder = MauiApp.CreateBuilder();
		builder.Services.AddSingleton(service);
		return builder.Build();
	}

	class StubWebAuthenticator : IWebAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
			Task.FromException<WebAuthenticatorResult>(new NotSupportedException());

		public Task<WebAuthenticatorResult> AuthenticateAsync(
			WebAuthenticatorOptions webAuthenticatorOptions,
			CancellationToken cancellationToken) =>
			Task.FromException<WebAuthenticatorResult>(new NotSupportedException());
	}

	sealed class StubPlatformWebAuthenticator : StubWebAuthenticator, IPlatformWebAuthenticatorCallback
	{
		public bool OnAppInstanceActivatedCallback(Microsoft.Windows.AppLifecycle.AppActivationArguments args) => true;
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

	sealed class CallbackPlatformGeocoding : StubGeocoding, IPlatformGeocoding
	{
		readonly Action<string?> _onSet;
		string? _mapServiceToken;

		public CallbackPlatformGeocoding(Action<string?> onSet)
		{
			_onSet = onSet;
		}

		public string? MapServiceToken
		{
			get => _mapServiceToken;
			set
			{
				_onSet(value);
				_mapServiceToken = value;
			}
		}
	}

	sealed class ThrowingRestorePlatformGeocoding : StubGeocoding, IPlatformGeocoding
	{
		string? _mapServiceToken;

		public ThrowingRestorePlatformGeocoding(string? mapServiceToken)
		{
			_mapServiceToken = mapServiceToken;
		}

		public string? ThrowOnToken { get; set; }

		public string? MapServiceToken
		{
			get => _mapServiceToken;
			set
			{
				if (ThrowOnToken is not null &&
					string.Equals(value, ThrowOnToken, StringComparison.Ordinal))
				{
					throw new InvalidOperationException("geocoder map token restore failed");
				}

				_mapServiceToken = value;
			}
		}
	}

	sealed class ThrowingPlatformGeocoding : StubGeocoding, IPlatformGeocoding
	{
		string? _mapServiceToken;

		public string? MapServiceToken
		{
			get => _mapServiceToken;
			set
			{
				if (value == "throw-token")
					throw new InvalidOperationException("map token failed");

				_mapServiceToken = value;
			}
		}
	}
}
