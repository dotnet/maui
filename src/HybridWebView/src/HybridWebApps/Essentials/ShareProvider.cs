using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls.HybridWebApps.UI;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials;

[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
internal union NativeShareRequest(ShareTextRequest, ShareFileRequest, ShareMultipleFilesRequest);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	AllowDuplicateProperties = false,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(NativeShareRequest))]
[JsonSerializable(typeof(StagedShareFile))]
internal partial class ShareJsonContext : JsonSerializerContext;

internal sealed record StagedShareFile(string Id, string FileName, string ContentType, int Size);

internal sealed class ShareProvider : IDisposable
{
	readonly IShare _share;
	readonly HybridViewContext _context;
	readonly HybridShareCache _cache;
	readonly HybridAppOptions _options;
	readonly ShareJsonContext _json;
	readonly object _gate = new();
	readonly Dictionary<string, Entry> _files = new(StringComparer.Ordinal);
	bool _disposed;
	long _bytes;
	sealed record Entry(string Path, string ContentType, int Size) { internal bool Retained; }

	public ShareProvider(IShare share, HybridViewContext context, HybridShareCache cache, HybridAppOptions options)
	{
		_share = share;
		_context = context;
		_cache = cache;
		_options = options;
		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
			AllowDuplicateProperties = false,
			RespectNullableAnnotations = true,
			RespectRequiredConstructorParameters = true,
		};
		jsonOptions.Converters.Add(new AuthorizedShareFileConverter(this));
		_json = new ShareJsonContext(jsonOptions);
		context.DocumentEnded += Retire;
		cache.Prune();
	}

	[HybridAction("share.stage")]
	internal byte[] Stage(byte[] bytes, string fileName, string contentType, CancellationToken cancellation)
	{
		if (contentType is not ("application/pdf" or "text/plain"))
			throw new CommandException("invalid_arguments", "Only PDF and plain-text staging is enabled by this prototype.");
		cancellation.ThrowIfCancellationRequested();
		lock (_gate)
		{
			if (_disposed)
				throw InvalidHandle();
			if (_files.Count >= 8 || _bytes + bytes.LongLength > 8 * 1024 * 1024)
				throw new CommandException("resource_limit", "This document's staged share resource budget was reached.");
			var path = _cache.Stage(fileName, bytes);
			if (cancellation.IsCancellationRequested)
			{ _cache.RemoveUnshared(path); cancellation.ThrowIfCancellationRequested(); }
			var id = Guid.NewGuid().ToString("D");
			_files.Add(id, new(path, contentType, bytes.Length));
			_bytes += bytes.Length;
			return JsonSerializer.SerializeToUtf8Bytes(new StagedShareFile(id, fileName, contentType, bytes.Length), _json.StagedShareFile);
		}
	}

	[HybridAction("share.request", MainThread = true, Interactive = true)]
	internal Task<Ack> Request(JsonElement request, CancellationToken cancellation)
	{
		_cache.Prune();
		if (request.ValueKind != JsonValueKind.Object)
			throw InvalidRequest();
		NativeShareRequest native;
		try
		{ native = request.Deserialize(_json.NativeShareRequest); }
		catch (Exception error) when (error is JsonException or NotSupportedException) { throw InvalidRequest(); }
		return _context.InteractAsync(async () =>
		{
			cancellation.ThrowIfCancellationRequested();
			var paths = new List<string>();
			Task Invoke()
			{
				switch (native)
				{
					case ShareTextRequest text:
						if (string.IsNullOrWhiteSpace(text.Text) && string.IsNullOrWhiteSpace(text.Uri))
							throw InvalidRequest();
						if (text.Text?.Length > _options.MaximumTextLength || text.Subject?.Length > 256 || text.Title?.Length > 256 ||
							text.Uri is not null && (!Uri.TryCreate(text.Uri, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo)))
							throw InvalidRequest();
						if (text.PreviewImage is not null)
							throw InvalidRequest();
						Prepare(paths);
						return _share.RequestAsync(text);
					case ShareFileRequest file:
						if (file.File is null || file.Title?.Length > 256)
							throw InvalidRequest();
						Authorize(file.File, paths);
						Prepare(paths);
						return _share.RequestAsync(file);
					case ShareMultipleFilesRequest files:
						if (files.Files is not { Count: >= 1 and <= 8 } || files.Title?.Length > 256)
							throw InvalidRequest();
						foreach (var item in files.Files)
						{ if (item is null) throw InvalidRequest(); Authorize(item, paths); }
						if (paths.Distinct(StringComparer.Ordinal).Count() != paths.Count)
							throw InvalidRequest();
						Prepare(paths);
						return _share.RequestAsync(files);
					default:
						throw InvalidRequest();
				}
			}
			try
			{ await Invoke(); return new Ack(true); }
			finally { _cache.EndShare(paths); }
		}, cancellation);
	}

	[HybridAction("share.release")]
	internal Ack Release(string id)
	{
		lock (_gate)
		{
			if (_disposed || !_files.Remove(id, out var entry))
				throw InvalidHandle();
			_bytes -= entry.Size;
			if (!entry.Retained)
				_cache.RemoveUnshared(entry.Path);
			return new(true);
		}
	}

	void Authorize(ShareFile file, List<string> paths)
	{
		lock (_gate)
		{
			var entry = _files.Values.FirstOrDefault(entry => entry.Path == file.FullPath);
			if (_disposed || entry is null || !File.Exists(entry.Path))
				throw InvalidHandle();
			paths.Add(entry.Path);
		}
	}
	void Prepare(List<string> paths)
	{
		lock (_gate)
		{
			if (_disposed || paths.Any(path => !_files.Values.Any(entry => entry.Path == path)))
				throw InvalidHandle();
			_cache.BeginShare(paths);
			foreach (var entry in _files.Values.Where(entry => paths.Contains(entry.Path, StringComparer.Ordinal)))
				entry.Retained = true;
		}
	}
	ShareFile Resolve(string id)
	{
		lock (_gate)
		{
			if (_disposed || !_files.TryGetValue(id, out var entry) || !File.Exists(entry.Path))
				throw InvalidHandle();
			return new ShareFile(entry.Path, entry.ContentType);
		}
	}
	void Retire()
	{
		lock (_gate)
		{
			foreach (var entry in _files.Values.Where(entry => !entry.Retained))
				_cache.RemoveUnshared(entry.Path);
			_files.Clear();
			_bytes = 0;
		}
		_cache.Prune();
	}
	public void Dispose()
	{
		lock (_gate)
		{ if (_disposed) return; _disposed = true; }
		_context.DocumentEnded -= Retire;
		Retire();
	}
	static CommandException InvalidRequest() => new("invalid_arguments", "Select exactly one nonempty native share request shape using authorized staged handles.");
	static CommandException InvalidHandle() => new("invalid_resource", "This staged file handle is unavailable in the current view/document.");

	sealed class AuthorizedShareFileConverter(ShareProvider owner) : JsonConverter<ShareFile>
	{
		public override ShareFile Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var value = document.RootElement;
			if (value.ValueKind != JsonValueKind.Object || value.EnumerateObject().Count() != 1 ||
				!value.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String ||
				!Guid.TryParseExact(id.GetString(), "D", out _))
				throw new JsonException("Use only an authorized opaque staged handle.");
			return owner.Resolve(id.GetString()!);
		}
		public override void Write(Utf8JsonWriter writer, ShareFile value, JsonSerializerOptions options) =>
			throw new JsonException("Native paths must never be serialized to a document.");
	}
}
