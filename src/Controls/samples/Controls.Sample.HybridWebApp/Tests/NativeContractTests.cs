using System.Text.Json;
using System.Text.Json.Serialization;
using Maui.HybridApp;
using Maui.HybridApp.Essentials;
using Maui.HybridApp.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

[assembly: HybridJsonContext(typeof(DevicePlatform), typeof(Maui.Controls.Sample.HybridWebApp.Tests.PlatformJsonContext))]

namespace Maui.Controls.Sample.HybridWebApp.Tests;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	AllowDuplicateProperties = false,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true,
	Converters = [typeof(PlatformConverter)])]
[JsonSerializable(typeof(DevicePlatform))]
internal partial class PlatformJsonContext : JsonSerializerContext;

internal sealed class PlatformConverter : JsonConverter<DevicePlatform>
{
	public override DevicePlatform Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
	{
		var text = reader.GetString();
		return !string.IsNullOrWhiteSpace(text) && text.Length <= 64 ? DevicePlatform.Create(text) : throw new JsonException();
	}
	public override void Write(Utf8JsonWriter writer, DevicePlatform value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}

internal static class PlatformContractActions
{
	[HybridAction("contract.platform")]
	internal static DevicePlatform Echo(DevicePlatform platform) => platform;
}

public sealed class NativeContractTests
{
	[Theory]
	[InlineData("""{"request":{}}""")]
	[InlineData("""{"request":{"timeout":"00:00:00"}}""")]
	[InlineData("""{"request":{"timeout":"-00:00:01"}}""")]
	[InlineData("""{"request":{"timeout":"01:00:00"}}""")]
	[InlineData("""{"request":{"timeout":null}}""")]
	[InlineData("""[{"timeout":"00:02:01"}]""")]
	public void NativeGeolocationPolicyRejectsInvalidTimeoutsBeforeInvocation(string arguments)
	{
		using var services = new ServiceCollection().BuildServiceProvider();
		Assert.Equal("invalid_arguments", Assert.Throws<CommandException>(() =>
			HybridNativePolicy.Validate(services, "geolocation.location", Json(arguments))).Code);
	}

	[Theory]
	[InlineData(AltitudeReferenceSystem.Unspecified, "unspecified")]
	[InlineData(AltitudeReferenceSystem.Terrain, "terrain")]
	[InlineData(AltitudeReferenceSystem.Ellipsoid, "ellipsoid")]
	[InlineData(AltitudeReferenceSystem.Geoid, "geoid")]
	[InlineData(AltitudeReferenceSystem.Surface, "surface")]
	public void NativeLocationContextUsesDeclaredCamelCaseEnumNames(AltitudeReferenceSystem reference, string wire)
	{
		var location = new Location { AltitudeReferenceSystem = reference };
		var json = JsonSerializer.Serialize(location, NativeLocationJsonContext.Default.Location);
		using var document = JsonDocument.Parse(json);
		Assert.Equal(wire, document.RootElement.GetProperty("altitudeReferenceSystem").GetString());
		Assert.Equal(reference, JsonSerializer.Deserialize(json, NativeLocationJsonContext.Default.Location)!.AltitudeReferenceSystem);
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
			"""{"altitudeReferenceSystem":0}""", NativeLocationJsonContext.Default.Location));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
			"""{"altitudeReferenceSystem":"0"}""", NativeLocationJsonContext.Default.Location));
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
			"""{"altitudeReferenceSystem":"unknown"}""", NativeLocationJsonContext.Default.Location));
	}

	static HybridViewContext Context()
	{
		var context = new HybridViewContext();
		var page = new ContentPage();
		context.Bind(page, new Window(page), "index.html");
		return context;
	}
	static JsonElement Json(string json) { using var document = JsonDocument.Parse(json); return document.RootElement.Clone(); }

	[Fact]
	public async Task ConsumerConverterPreservesCustomPlatformsWithoutClosedSwitches()
	{
		var registry = TestComposition.CreateRegistry();
		var result = await registry.DispatchAsync("contract.platform", Json("""{"platform":"CustomDesktop"}"""),
			new Operation("session", "request", default, (_, _) => { }));
		Assert.Equal("CustomDesktop", result.GetString());
		Assert.Equal("invalid_arguments", (await Assert.ThrowsAsync<CommandException>(() =>
			registry.DispatchAsync("contract.platform", Json("""{"platform":""}"""), new Operation("session", "request", default, (_, _) => { })))).Code);
	}

	[Theory]
	[InlineData("""{}""")]
	[InlineData("""{"title":"ambiguous"}""")]
	[InlineData("""{"text":"hello","file":{"id":"00000000-0000-0000-0000-000000000000"}}""")]
	[InlineData("""{"files":[]}""")]
	[InlineData("""{"file":null}""")]
	[InlineData("""{"text":""}""")]
	[InlineData("""{"text":"hello","unknown":1}""")]
	[InlineData("""{"text":"one","text":"two"}""")]
	[InlineData("""{"file":{"fullPath":"/private/not-authorized.pdf"}}""")]
	[InlineData("""null""")]
	public async Task NativeUnionRejectsEmptyAmbiguousUnknownAndUnauthorizedPathShapes(string request)
	{
		using var context = Context();
		var fake = new FakeShare();
		var root = TestDirectory();
		try
		{
			using var provider = new ShareProvider(fake, context, new(root, TimeProvider.System), new("share."));
			var error = await Assert.ThrowsAsync<CommandException>(() => provider.Request(Json(request), default));
			Assert.Equal("invalid_arguments", error.Code);
			Assert.Empty(fake.Requests);
		}
		finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
	}

	[Fact]
	public async Task NativeTextFileAndMultiFileCasesUseOpaqueResourcesAndRetainAfterChooserLaunch()
	{
		using var context = Context();
		using var otherContext = Context();
		var root = TestDirectory();
		var fake = new FakeShare();
		var time = new Clock();
		var cache = new HybridShareCache(root, time);
		var provider = new ShareProvider(fake, context, cache, new("share."));
		using var other = new ShareProvider(fake, otherContext, cache, new("share."));
		try
		{
			var first = provider.Stage([0, 1, 255], "browser.pdf", "application/pdf", default);
			var second = provider.Stage([2, 3], "notes.txt", "text/plain", default);
			var id = JsonSerializer.Deserialize(first, ShareJsonContext.Default.StagedShareFile)!.Id;
			var id2 = JsonSerializer.Deserialize(second, ShareJsonContext.Default.StagedShareFile)!.Id;
			Assert.DoesNotContain("fullPath", System.Text.Encoding.UTF8.GetString(first), StringComparison.OrdinalIgnoreCase);
			Assert.Equal("invalid_resource", (await Assert.ThrowsAsync<CommandException>(() =>
				other.Request(Json($$$"""{"file":{"id":"{{{id}}}"}}"""), default))).Code);
			Assert.True((await provider.Request(Json("""{"text":"native text","title":"Share explicitly"}"""), default)).Accepted);
			Assert.IsType<ShareTextRequest>(fake.Requests[0]);
			await provider.Request(Json($$$"""{"file":{"id":"{{{id}}}"}}"""), default);
			var single = Assert.IsType<ShareFileRequest>(fake.Requests[1]);
			Assert.Equal(new byte[] { 0, 1, 255 }, File.ReadAllBytes(single.File!.FullPath));
			await provider.Request(Json($$"""{"files":[{"id":"{{id}}"},{"id":"{{id2}}"}]}"""), default);
			Assert.Equal(2, Assert.IsType<ShareMultipleFilesRequest>(fake.Requests[2]).Files!.Count);
			provider.Release(id);
			Assert.Equal("invalid_resource", (await Assert.ThrowsAsync<CommandException>(() =>
				provider.Request(Json($$$"""{"file":{"id":"{{{id}}}"}}"""), default))).Code);
			Assert.True(File.Exists(single.File.FullPath), "Releasing authority must not delete a file the OS may still read.");
			provider.Dispose();
			Assert.True(File.Exists(single.File.FullPath), "Android chooser task completion is not file consumption.");
			time.Advance(HybridShareCache.Retention - TimeSpan.FromSeconds(1));
			cache.Prune();
			Assert.True(File.Exists(single.File.FullPath));
			time.Advance(TimeSpan.FromSeconds(1));
			cache.Prune();
			Assert.False(File.Exists(single.File.FullPath));
			Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
		}
		finally { provider.Dispose(); if (Directory.Exists(root)) Directory.Delete(root, true); }
	}

	[Fact]
	public async Task UnsharedFilesAreCleanedAndActiveSharesAreNotPrunedDuringAsyncNativeUi()
	{
		using var context = Context();
		var root = TestDirectory();
		var time = new Clock();
		var cache = new HybridShareCache(root, time);
		var fake = new FakeShare { Wait = new(TaskCreationOptions.RunContinuationsAsynchronously) };
		var provider = new ShareProvider(fake, context, cache, new("share."));
		try
		{
			provider.Stage([3], "unshared.txt", "text/plain", default);
			var json = provider.Stage([4], "shared.txt", "text/plain", default);
			var id = JsonSerializer.Deserialize(json, ShareJsonContext.Default.StagedShareFile)!.Id;
			var pending = provider.Request(Json($$$"""{"file":{"id":"{{{id}}}"}}"""), default);
			var path = Assert.IsType<ShareFileRequest>(Assert.Single(fake.Requests)).File!.FullPath;
			provider.Dispose();
			Assert.Single(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
			time.Advance(HybridShareCache.Retention + TimeSpan.FromMinutes(1));
			cache.Prune();
			Assert.True(File.Exists(path));
			fake.Wait.SetResult();
			await pending;
			cache.Prune();
			Assert.True(File.Exists(path), "Retention must extend from actual native completion, not expire while the UI was still awaiting.");
			time.Advance(HybridShareCache.Retention);
			cache.Prune();
			Assert.False(File.Exists(path));
		}
		finally { provider.Dispose(); if (Directory.Exists(root)) Directory.Delete(root, true); }
	}

	[Fact]
	public void StagingBudgetsCancellationAndFilenamePolicyCannotBecomeAnArbitraryWriter()
	{
		using var context = Context();
		var root = TestDirectory();
		try
		{
			using var provider = new ShareProvider(new FakeShare(), context, new(root, TimeProvider.System), new("share."));
			foreach (var name in new[] { "../escape.pdf", "/absolute.pdf", "sub/file.pdf", "a\\file.pdf", ".hidden", "" })
				Assert.Equal("invalid_arguments", Assert.Throws<CommandException>(() => provider.Stage([1], name, "application/pdf", default)).Code);
			Assert.Throws<CommandException>(() => provider.Stage([], "empty.pdf", "application/pdf", default));
			Assert.Throws<CommandException>(() => provider.Stage([1], "code.js", "application/javascript", default));
			Assert.ThrowsAny<OperationCanceledException>(() => provider.Stage([1], "cancelled.pdf", "application/pdf", new(true)));
			var ids = new List<string>();
			for (var i = 0; i < 8; i++)
				ids.Add(JsonSerializer.Deserialize(provider.Stage([1], "file.pdf", "application/pdf", default), ShareJsonContext.Default.StagedShareFile)!.Id);
			Assert.Equal("resource_limit", Assert.Throws<CommandException>(() => provider.Stage([1], "full.pdf", "application/pdf", default)).Code);
			provider.Release(ids[0]);
			provider.Stage([1], "replacement.pdf", "application/pdf", default);
			provider.Dispose();
			Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
		}
		finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
	}

	[Fact]
	public async Task RetiringADocumentRevokesItsHandlesAndCleansUnsharedBytes()
	{
		using var context = Context();
		var root = TestDirectory();
		try
		{
			using var provider = new ShareProvider(new FakeShare(), context, new(root, TimeProvider.System), new("share."));
			var json = provider.Stage([1], "old.txt", "text/plain", default);
			var id = JsonSerializer.Deserialize(json, ShareJsonContext.Default.StagedShareFile)!.Id;
			context.RetireDocument();
			Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
			Assert.Equal("invalid_resource", (await Assert.ThrowsAsync<CommandException>(() =>
				provider.Request(Json($$$"""{"file":{"id":"{{{id}}}"}}"""), default))).Code);
			provider.Stage([2], "new.txt", "text/plain", default);
			Assert.Single(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
		}
		finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
	}

	[Fact]
	public void RetainedCacheBudgetsRejectNewFilesInsteadOfDeletingYoungRecipientsFiles()
	{
		var root = TestDirectory();
		var time = new Clock();
		var cache = new HybridShareCache(root, time);
		try
		{
			for (var i = 0; i < HybridShareCache.MaximumFiles; i++) cache.Stage("retained.pdf", [1]);
			Assert.Equal("resource_limit", Assert.Throws<CommandException>(() => cache.Stage("too-many.pdf", [1])).Code);
			time.Advance(HybridShareCache.Retention);
			cache.Prune();
			var bytes = new byte[HybridShareCache.MaximumFileBytes];
			for (var i = 0; i < HybridShareCache.MaximumTotalBytes / bytes.Length; i++) cache.Stage("retained.pdf", bytes);
			Assert.Equal("resource_limit", Assert.Throws<CommandException>(() => cache.Stage("too-large.pdf", [1])).Code);
		}
		finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
	}

	[Fact]
	public async Task RealGeolocationRequestAndLocationUseNativeContractsAndExplicitPermissionFailures()
	{
		var services = TestComposition.Services();
		using var context = Context();
		var geo = new FakeGeolocation();
		services.AddSingleton<IGeolocation>(geo);
		using var root = services.BuildServiceProvider();
		using var hostScope = root.CreateScope();
		using var local = hostScope.ServiceProvider.CreateHybridAppProvider();
		local.GetRequiredService<HybridViewContext>().Bind(context.Page, context.Window, "index.html");
		var registry = local.GetRequiredService<CommandRegistry>();
		var args = Json("""{"request":{"desiredAccuracy":"high","timeout":"00:00:20","requestFullAccuracy":false}}""");
		async Task<JsonElement> Call() => await registry.DispatchAsync("geolocation.location", args, new Operation("session", "request", default, (_, _) => { }));
		var result = await Call();
		Assert.Equal(GeolocationAccuracy.High, geo.Request!.DesiredAccuracy);
		Assert.Equal(TimeSpan.FromSeconds(20), geo.Request.Timeout);
		Assert.Equal(geo.Location.Latitude, result.GetProperty("latitude").GetDouble());
		Assert.Equal(geo.Location.Longitude, result.GetProperty("longitude").GetDouble());
		Assert.Equal("unspecified", result.GetProperty("altitudeReferenceSystem").GetString());
		geo.Error = new PermissionException("OS denied");
		Assert.Equal("permission_denied", (await Assert.ThrowsAsync<CommandException>(Call)).Code);
		geo.Error = new FeatureNotEnabledException();
		Assert.Equal("configuration_required", (await Assert.ThrowsAsync<CommandException>(Call)).Code);
		geo.Error = new FeatureNotSupportedException();
		await Assert.ThrowsAsync<FeatureNotSupportedException>(Call);
	}

	static string TestDirectory() => Path.Combine(AppContext.BaseDirectory, "share-tests", Guid.NewGuid().ToString("N"));
	sealed class Clock : TimeProvider
	{
		DateTimeOffset _now = new(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);
		public override DateTimeOffset GetUtcNow() => _now;
		internal void Advance(TimeSpan elapsed) => _now += elapsed;
	}
	sealed class FakeShare : IShare
	{
		internal List<ShareRequestBase> Requests { get; } = new();
		internal TaskCompletionSource? Wait;
		Task Request(ShareRequestBase request) { Requests.Add(request); return Wait?.Task ?? Task.CompletedTask; }
		public Task RequestAsync(ShareTextRequest request) => Request(request);
		public Task RequestAsync(ShareFileRequest request) => Request(request);
		public Task RequestAsync(ShareMultipleFilesRequest request) => Request(request);
	}
	sealed class FakeGeolocation : IGeolocation
	{
		internal GeolocationRequest? Request;
		internal Location Location { get; } = new(51.5, -0.12) { Accuracy = 8, Timestamp = DateTimeOffset.UtcNow };
		internal Exception? Error;
		public Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellation)
		{
			Request = request;
			cancellation.ThrowIfCancellationRequested();
			if (Error is not null) throw Error;
			return Task.FromResult<Location?>(Location);
		}
		public Task<Location?> GetLastKnownLocationAsync() => throw new NotSupportedException();
		public bool IsListeningForeground => false;
		public bool IsEnabled => true;
		public event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged { add { } remove { } }
		public event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed { add { } remove { } }
		public Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request) => throw new NotSupportedException();
		public void StopListeningForeground() { }
	}
}
