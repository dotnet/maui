using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps.Gtk;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Maps
{
	/// <summary>A stub tile server: serves bytes from a function, counts the requests, can be held back.</summary>
	public sealed class StubTileFetcher : IGtkMapTileFetcher
	{
		readonly Func<string, byte[]?> _serve;
		readonly ConcurrentQueue<string> _requests = new();
		public SemaphoreSlim? Gate;

		public StubTileFetcher(Func<string, byte[]?> serve) => _serve = serve;

		public IReadOnlyCollection<string> Requests => _requests;

		public async Task<byte[]?> FetchAsync(string url, CancellationToken cancellationToken)
		{
			_requests.Enqueue(url);
			if (Gate is not null)
				await Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
			return _serve(url);
		}
	}

	/// <summary>The tile store, the disk cache, the HTTP fetcher and the geocoder, against stubs (no network).</summary>
	public class MapTileTests : IDisposable
	{
		readonly string _directory = Path.Combine(Path.GetTempPath(), "maui-gtk-maps-" + Guid.NewGuid().ToString("N"));

		public void Dispose()
		{
			try
			{
				if (Directory.Exists(_directory))
					Directory.Delete(_directory, true);
			}
			catch (IOException)
			{
			}
		}

		static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);

		static TileStore NewStore(IGtkMapTileFetcher fetcher, TileDiskCache? disk = null, int concurrent = 4) =>
			new(fetcher, disk, concurrent, action => action());

		static bool WaitFor(Func<bool> condition, int timeoutMs = 5000)
		{
			var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
			while (DateTime.UtcNow < deadline)
			{
				if (condition())
					return true;
				Thread.Sleep(5);
			}

			return condition();
		}

		[Fact]
		public void DiskCacheStoresAndReadsAnEntry()
		{
			var cache = new TileDiskCache(_directory, TimeSpan.FromDays(7));
			Assert.Null(cache.TryRead("https://t/1/2/3.png"));

			cache.TryWrite("https://t/1/2/3.png", Bytes("tile"));

			Assert.Equal(Bytes("tile"), cache.TryRead("https://t/1/2/3.png"));
			Assert.Null(cache.TryRead("https://t/1/2/4.png"));
		}

		[Fact]
		public void DiskCachePathIsStableAndFannedOut()
		{
			var cache = new TileDiskCache(_directory, TimeSpan.FromDays(7));
			var path = cache.PathFor("https://t/1/2/3.png");

			Assert.Equal(path, cache.PathFor("https://t/1/2/3.png"));
			Assert.NotEqual(path, cache.PathFor("https://t/1/2/4.png"));
			Assert.StartsWith(_directory, path);
			Assert.EndsWith(".tile", path);

			var relative = Path.GetRelativePath(_directory, path).Split(Path.DirectorySeparatorChar);
			Assert.Equal(3, relative.Length);
			Assert.Equal(2, relative[0].Length);
			Assert.Equal(2, relative[1].Length);
		}

		[Fact]
		public void DiskCacheIgnoresExpiredEntries()
		{
			var cache = new TileDiskCache(_directory, TimeSpan.FromDays(7));
			cache.TryWrite("u", Bytes("old"));
			File.SetLastWriteTimeUtc(cache.PathFor("u"), DateTime.UtcNow.AddDays(-8));

			Assert.Null(cache.TryRead("u"));

			// A zero max age never expires.
			Assert.Equal(Bytes("old"), new TileDiskCache(_directory, TimeSpan.Zero).TryRead("u"));
		}

		[Fact]
		public void DiskCacheOverwritesAndLeavesNoTemporaryFiles()
		{
			var cache = new TileDiskCache(_directory, TimeSpan.FromDays(7));
			cache.TryWrite("u", Bytes("one"));
			cache.TryWrite("u", Bytes("two"));

			Assert.Equal(Bytes("two"), cache.TryRead("u"));
			Assert.Empty(Directory.GetFiles(_directory, "*.tmp", SearchOption.AllDirectories));
		}

		[Fact]
		public void DiskCacheTrimDeletesTheOldestEntriesFirst()
		{
			var cache = new TileDiskCache(_directory, TimeSpan.Zero);
			for (int i = 0; i < 4; i++)
			{
				cache.TryWrite("u" + i, new byte[100]);
				File.SetLastWriteTimeUtc(cache.PathFor("u" + i), DateTime.UtcNow.AddMinutes(-10 + i));
			}

			long deleted = cache.Trim(250);

			Assert.Equal(200, deleted);
			Assert.Null(cache.TryRead("u0"));
			Assert.Null(cache.TryRead("u1"));
			Assert.NotNull(cache.TryRead("u2"));
			Assert.NotNull(cache.TryRead("u3"));
			Assert.Equal(0, cache.Trim(250));
			Assert.Equal(0, new TileDiskCache(Path.Combine(_directory, "missing"), TimeSpan.Zero).Trim(1));
		}

		[Fact]
		public void StoreFetchesAndDeliversTheBytes()
		{
			var fetcher = new StubTileFetcher(url => Bytes("data:" + url));
			var store = NewStore(fetcher);
			byte[]? received = null;

			store.Request("u1", this, b => received = b);

			Assert.True(WaitFor(() => received is not null));
			Assert.Equal(Bytes("data:u1"), received);
			Assert.True(WaitFor(() => store.InFlight == 0));
		}

		[Fact]
		public void StoreSharesOneDownloadBetweenRequestersAndKeepsOneCallbackPerOwner()
		{
			var fetcher = new StubTileFetcher(url => Bytes("x")) { Gate = new SemaphoreSlim(0) };
			var store = NewStore(fetcher);
			int first = 0, second = 0;
			var ownerA = new object();
			var ownerB = new object();

			store.Request("u", ownerA, _ => first++);
			store.Request("u", ownerA, _ => first++); // a repaint asking again: replaces, does not add
			store.Request("u", ownerB, _ => second++);
			Assert.True(WaitFor(() => fetcher.Requests.Count == 1));
			fetcher.Gate.Release();

			Assert.True(WaitFor(() => first == 1 && second == 1));
			Assert.Single(fetcher.Requests);
		}

		[Fact]
		public void StoreReportsFailuresAndLeavesTheUrlAloneForAWhile()
		{
			var fetcher = new StubTileFetcher(url => null);
			var store = NewStore(fetcher);
			bool completed = false;
			byte[]? bytes = Bytes("sentinel");

			store.Request("u", this, b => { bytes = b; completed = true; });
			Assert.True(WaitFor(() => completed));

			Assert.Null(bytes);
			Assert.True(store.HasFailedRecently("u"));

			// A new request inside the retry window is ignored: no second download.
			bool again = false;
			store.Request("u", this, _ => again = true);
			Thread.Sleep(50);
			Assert.False(again);
			Assert.Single(fetcher.Requests);
		}

		[Fact]
		public void StoreTreatsEmptyBodiesAsFailuresAndMarkFailedBlocksRetries()
		{
			var store = NewStore(new StubTileFetcher(url => Array.Empty<byte>()));
			bool completed = false;
			byte[]? bytes = Bytes("sentinel");
			store.Request("empty", this, b => { bytes = b; completed = true; });

			Assert.True(WaitFor(() => completed));
			Assert.Null(bytes);

			Assert.False(store.HasFailedRecently("decode"));
			store.MarkFailed("decode");
			Assert.True(store.HasFailedRecently("decode"));
		}

		[Fact]
		public void StoreSurvivesAFetcherThatThrows()
		{
			var store = NewStore(new StubTileFetcher(url => throw new InvalidOperationException("boom")));
			bool completed = false;
			byte[]? bytes = Bytes("sentinel");

			store.Request("u", this, b => { bytes = b; completed = true; });

			Assert.True(WaitFor(() => completed));
			Assert.Null(bytes);
			Assert.True(WaitFor(() => store.InFlight == 0));
		}

		[Fact]
		public void StoreLimitsConcurrentDownloadsAndServesTheNewestRequestFirst()
		{
			var fetcher = new StubTileFetcher(url => Bytes(url)) { Gate = new SemaphoreSlim(0) };
			var store = NewStore(fetcher, concurrent: 2);
			var done = new ConcurrentBag<string>();

			foreach (var url in new[] { "a", "b", "c", "d", "e" })
				store.Request(url, this, _ => done.Add(url));

			// Two downloads run; the rest wait for a slot.
			Assert.True(WaitFor(() => fetcher.Requests.Count == 2));
			Assert.Equal(2, store.InFlight);
			Assert.Equal(3, store.Queued);

			fetcher.Gate.Release(5);
			Assert.True(WaitFor(() => done.Count == 5));

			// "a" and "b" took the free slots; the waiting ones are served newest first.
			var order = fetcher.Requests.ToArray();
			Assert.Equal(new[] { "a", "b" }, order.Take(2).OrderBy(u => u).ToArray());
			Assert.Equal(new[] { "d", "e" }, order.Skip(2).Take(2).OrderBy(u => u).ToArray());
			Assert.Equal("c", order[4]);
		}

		[Fact]
		public void StoreUsesTheDiskCacheBeforeTheFetcherAndFillsItAfter()
		{
			var disk = new TileDiskCache(_directory, TimeSpan.FromDays(7));
			disk.TryWrite("cached", Bytes("from-disk"));
			var fetcher = new StubTileFetcher(url => Bytes("from-net"));
			var store = NewStore(fetcher, disk);

			byte[]? cached = null, fresh = null;
			store.Request("cached", this, b => cached = b);
			Assert.True(WaitFor(() => cached is not null));
			Assert.Equal(Bytes("from-disk"), cached);
			Assert.Empty(fetcher.Requests);

			store.Request("fresh", this, b => fresh = b);
			Assert.True(WaitFor(() => fresh is not null));
			Assert.Equal(Bytes("from-net"), fresh);
			Assert.Equal(Bytes("from-net"), disk.TryRead("fresh"));
		}

		[Fact]
		public void StoreDeliversThroughThePostFunction()
		{
			var posted = new ConcurrentQueue<Action>();
			var store = new TileStore(new StubTileFetcher(url => Bytes("x")), null, 2, posted.Enqueue);
			int delivered = 0;

			store.Request("u", this, _ => delivered++);

			Assert.True(WaitFor(() => !posted.IsEmpty));
			Assert.Equal(0, delivered);
			posted.TryDequeue(out var action);
			action!();
			Assert.Equal(1, delivered);
		}

		[Fact]
		public async Task HttpFetcherSendsTheUserAgentAndReturnsTheBody()
		{
			var handler = new StubHandler(request => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Bytes("png")) });
			using var fetcher = new HttpGtkMapTileFetcher("MyApp/1.0 (contact)", handler);

			var bytes = await fetcher.FetchAsync("https://t.example/1/2/3.png", CancellationToken.None);

			Assert.Equal(Bytes("png"), bytes);
			Assert.Equal("MyApp/1.0 (contact)", handler.Requests.Single().Headers.UserAgent.ToString());
			Assert.Equal("https://t.example/1/2/3.png", handler.Requests.Single().RequestUri!.ToString());
		}

		[Fact]
		public async Task HttpFetcherReturnsNullForErrorsAndNetworkFailures()
		{
			using var notFound = new HttpGtkMapTileFetcher("a", new StubHandler(r => new HttpResponseMessage(HttpStatusCode.NotFound)));
			using var offline = new HttpGtkMapTileFetcher("a", new StubHandler(r => throw new HttpRequestException("offline")));

			Assert.Null(await notFound.FetchAsync("u", CancellationToken.None));
			Assert.Null(await offline.FetchAsync("u", CancellationToken.None));
		}

		[Fact]
		public async Task GeocoderParsesSearchResultsAndSendsTheUserAgent()
		{
			var handler = new StubHandler(r => Json("[{\"lat\":\"48.8566\",\"lon\":\"2.3522\",\"display_name\":\"Paris\"},{\"lat\":\"x\",\"lon\":\"1\"},{\"lat\":45.5,\"lon\":9.2}]"));
			using var geocoder = new NominatimGeocoding(new GtkMapOptions { UserAgent = "Test/1", NominatimUrl = "https://nom.example/" }, handler) { MinimumInterval = TimeSpan.Zero };

			var locations = (await geocoder.GetLocationsAsync("Paris & more")).ToList();

			Assert.Equal(2, locations.Count);
			Assert.Equal(48.8566, locations[0].Latitude, 6);
			Assert.Equal(2.3522, locations[0].Longitude, 6);
			Assert.Equal(45.5, locations[1].Latitude, 6);
			var request = handler.Requests.Single();
			Assert.StartsWith("https://nom.example/search?format=jsonv2", request.RequestUri!.AbsoluteUri);
			Assert.Contains("q=Paris%20%26%20more", request.RequestUri.AbsoluteUri);
			Assert.Equal("Test/1", request.Headers.UserAgent.ToString());
		}

		[Fact]
		public async Task GeocoderMapsTheReverseAddressToAPlacemark()
		{
			var handler = new StubHandler(r => Json(
				"{\"name\":\"Tour Eiffel\",\"address\":{\"house_number\":\"5\",\"road\":\"Avenue Anatole France\",\"suburb\":\"Gros-Caillou\",\"city\":\"Paris\"," +
				"\"county\":\"Paris\",\"state\":\"Ile-de-France\",\"postcode\":\"75007\",\"country\":\"France\",\"country_code\":\"fr\"}}"));
			using var geocoder = new NominatimGeocoding(new GtkMapOptions(), handler) { MinimumInterval = TimeSpan.Zero };

			var placemark = (await geocoder.GetPlacemarksAsync(48.8584, 2.2945)).Single();

			Assert.Equal("France", placemark.CountryName);
			Assert.Equal("FR", placemark.CountryCode);
			Assert.Equal("Ile-de-France", placemark.AdminArea);
			Assert.Equal("Paris", placemark.SubAdminArea);
			Assert.Equal("Paris", placemark.Locality);
			Assert.Equal("Gros-Caillou", placemark.SubLocality);
			Assert.Equal("Avenue Anatole France", placemark.Thoroughfare);
			Assert.Equal("5", placemark.SubThoroughfare);
			Assert.Equal("75007", placemark.PostalCode);
			Assert.Equal("Tour Eiffel", placemark.FeatureName);
			Assert.Equal(48.8584, placemark.Location.Latitude, 6);
			Assert.Contains("lat=48.8584&lon=2.2945", handler.Requests.Single().RequestUri!.Query);
		}

		[Fact]
		public async Task GeocoderReturnsEmptyResultsForFailures()
		{
			using var empty = new NominatimGeocoding(new GtkMapOptions(), new StubHandler(r => Json("[]"))) { MinimumInterval = TimeSpan.Zero };
			using var error = new NominatimGeocoding(new GtkMapOptions(), new StubHandler(r => Json("{\"error\":\"Unable to geocode\"}"))) { MinimumInterval = TimeSpan.Zero };
			using var offline = new NominatimGeocoding(new GtkMapOptions(), new StubHandler(r => throw new HttpRequestException("offline"))) { MinimumInterval = TimeSpan.Zero };
			using var garbage = new NominatimGeocoding(new GtkMapOptions(), new StubHandler(r => Json("<html>"))) { MinimumInterval = TimeSpan.Zero };
			using var throttled = new NominatimGeocoding(new GtkMapOptions(), new StubHandler(r => new HttpResponseMessage(HttpStatusCode.TooManyRequests))) { MinimumInterval = TimeSpan.Zero };

			Assert.Empty(await empty.GetLocationsAsync("nowhere"));
			Assert.Empty(await empty.GetLocationsAsync("  "));
			Assert.Empty(await error.GetPlacemarksAsync(0, 0));
			Assert.Empty(await offline.GetLocationsAsync("x"));
			Assert.Empty(await offline.GetPlacemarksAsync(1, 1));
			Assert.Empty(await garbage.GetLocationsAsync("x"));
			Assert.Empty(await throttled.GetLocationsAsync("x"));
		}

		[Fact]
		public async Task GeocoderSpacesItsRequests()
		{
			var handler = new StubHandler(r => Json("[]"));
			using var geocoder = new NominatimGeocoding(new GtkMapOptions(), handler) { MinimumInterval = TimeSpan.FromMilliseconds(250) };

			var started = DateTime.UtcNow;
			await geocoder.GetLocationsAsync("a");
			await geocoder.GetLocationsAsync("b");
			await geocoder.GetLocationsAsync("c");

			// Three requests need two gaps.
			Assert.True(DateTime.UtcNow - started >= TimeSpan.FromMilliseconds(450), "the requests were not spaced");
			Assert.Equal(3, handler.Requests.Count);
		}

		static HttpResponseMessage Json(string body) =>
			new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

		sealed class StubHandler : HttpMessageHandler
		{
			readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
			readonly ConcurrentQueue<HttpRequestMessage> _requests = new();

			public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

			public IReadOnlyCollection<HttpRequestMessage> Requests => _requests;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				_requests.Enqueue(request);
				return Task.FromResult(_respond(request));
			}
		}
	}
}
