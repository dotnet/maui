using System.Text.Json.Serialization;
using Maui.HybridApp.UI;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace Maui.HybridApp.Essentials.FilePicker;

/// <summary>Configurable native file-resource/message budgets, never a whole-file selection size limit.</summary>
public sealed class HybridFileLimits
{
	/// <summary>Creates bounded selected-file/reader/chunk budgets.</summary>
	/// <param name="selectedFiles">Maximum simultaneous user-selected file handles.</param>
	/// <param name="readers">Maximum simultaneous open read streams.</param>
	/// <param name="chunkBytes">Maximum raw bytes in one base64 transport response.</param>
	public HybridFileLimits(int selectedFiles = 128, int readers = 16, int chunkBytes = 65536)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(selectedFiles);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(readers);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkBytes);
		if (chunkBytes > 65536) throw new ArgumentOutOfRangeException(nameof(chunkBytes), "A read message must fit the bounded native transport.");
		SelectedFiles = selectedFiles; Readers = readers; ChunkBytes = chunkBytes;
	}
	/// <summary>Gets the simultaneous selected-file resource budget.</summary>
	public int SelectedFiles { get; }
	/// <summary>Gets the simultaneous open-reader resource budget.</summary>
	public int Readers { get; }
	/// <summary>Gets the per-read transport budget, not file capacity.</summary>
	public int ChunkBytes { get; }
}

/// <summary>System selection with JS-supplied MAUI options and document-owned FileResult/read-stream resources.</summary>
public sealed class FilePickerProvider : IHybridCommandProvider, IDisposable
{
	readonly IFilePicker _picker;
	readonly HybridViewContext _context;
	readonly NativeDispatcher _dispatcher;
	readonly HybridFileLimits _limits;
	readonly object _gate = new();
	readonly Dictionary<string, FileResult> _files = new(StringComparer.Ordinal);
	readonly Dictionary<string, Reader> _readers = new(StringComparer.Ordinal);
	bool _disposed;
	sealed class Reader(string fileId, Stream stream) : IDisposable
	{
		internal string FileId { get; } = fileId;
		internal Stream Stream { get; } = stream;
		internal SemaphoreSlim Lock { get; } = new(1, 1);
		internal CancellationTokenSource Lifetime { get; } = new();
		public void Dispose() { Lifetime.Cancel(); Stream.Dispose(); }
	}
	/// <summary>Creates a picker/resource provider using actual existing Essentials interfaces.</summary>
	/// <param name="picker">Actual native system picker.</param>
	/// <param name="context">Calling view/document lifetime boundary.</param>
	/// <param name="dispatcher">Native UI thread boundary.</param>
	/// <param name="limits">Optional host resource/message budgets, not file capacity.</param>
	public FilePickerProvider(IFilePicker picker, HybridViewContext context, NativeDispatcher dispatcher, HybridFileLimits? limits = null)
	{
		_picker = picker; _context = context; _dispatcher = dispatcher; _limits = limits ?? new HybridFileLimits();
		context.DocumentEnded += Clear;
	}
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = FilePickerJsonContext.Default;
		registry.Register("filePicker.pick", json.PickerOptions, json.PickResult, (args, op) =>
		{
			var options = Options(args);
			return _dispatcher.RunAsync(() => _context.InteractAsync(async () =>
			{
#if DEBUG && (IOS || MACCATALYST || ANDROID || WINDOWS)
				Console.WriteLine($"HYBRIDWEBAPP_PICKER phase=before-await mainThread={Microsoft.Maui.ApplicationModel.MainThread.IsMainThread} cancellation={op.Cancellation.IsCancellationRequested} ownerAttached={_context.Window.Handler is not null} pageOwned={ReferenceEquals(_context.Page.Window, _context.Window)} picker={_picker.GetType().FullName}");
#endif
				var file = await _picker.PickAsync(options);
#if DEBUG && (IOS || MACCATALYST || ANDROID || WINDOWS)
				Console.WriteLine($"HYBRIDWEBAPP_PICKER phase=after-await selected={file is not null} cancellation={op.Cancellation.IsCancellationRequested} disposed={_disposed}");
#endif
				op.Cancellation.ThrowIfCancellationRequested();
				var result = new PickResult(file is null ? null : AddFiles([file]).Single());
#if DEBUG && (IOS || MACCATALYST || ANDROID || WINDOWS)
				Console.WriteLine($"HYBRIDWEBAPP_PICKER phase=return selected={result.File is not null} cancellation={op.Cancellation.IsCancellationRequested}");
#endif
				return result;
			}, op.Cancellation), op.Cancellation);
		}, interactive: true);
		registry.Register("filePicker.pickMultiple", json.PickerOptions, json.PickMultipleResult, (args, op) =>
		{
			var options = Options(args);
			return _dispatcher.RunAsync(() => _context.InteractAsync(async () =>
			{
				var files = await _picker.PickMultipleAsync(options);
				op.Cancellation.ThrowIfCancellationRequested();
				return new PickMultipleResult(files is null ? [] : AddFiles(files.ToArray()));
			}, op.Cancellation), op.Cancellation);
		}, interactive: true);
		registry.Register("file.openRead", json.FileIdArgs, json.ReaderMetadata, (args, op) => _dispatcher.RunAsync(async () =>
		{
			FileResult file;
			lock (_gate)
			{
				if (_disposed || !_files.TryGetValue(args.Id, out file!)) throw InvalidFile();
				if (_readers.Count >= _limits.Readers) throw new CommandException("resource_limit", "The host's open file-reader budget was reached.");
			}
			var stream = await file.OpenReadAsync();
			try
			{
				op.Cancellation.ThrowIfCancellationRequested();
				lock (_gate)
				{
					if (_disposed || !_files.ContainsKey(args.Id)) throw InvalidFile();
					if (_readers.Count >= _limits.Readers) throw new CommandException("resource_limit", "The host's open file-reader budget was reached.");
					var id = Guid.NewGuid().ToString("D");
					_readers.Add(id, new Reader(args.Id, stream));
					return new ReaderMetadata(id, stream.CanSeek, _limits.ChunkBytes);
				}
			}
			catch { stream.Dispose(); throw; }
		}, op.Cancellation));
		registry.Register("file.readBytes", json.ReadArgs, json.ReadResult, ReadAsync);
		registry.Register("file.seek", json.SeekArgs, json.FileAccepted, async (args, op) =>
		{
			if (args.Position < 0 || args.Position > 9007199254740991)
				throw new CommandException("invalid_arguments", "Use a nonnegative safe-integer byte position.");
			var reader = GetReader(args.Id);
			using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(op.Cancellation, reader.Lifetime.Token);
			await reader.Lock.WaitAsync(cancellation.Token);
			try
			{
				if (!reader.Stream.CanSeek) throw new NotSupportedException("This selected native stream does not support seeking.");
				reader.Stream.Seek(args.Position, SeekOrigin.Begin);
				return new FileAccepted(true);
			}
			finally { reader.Lock.Release(); }
		});
		registry.Register("file.closeReader", json.FileIdArgs, json.FileAccepted, (args, _) =>
		{
			Reader? reader;
			lock (_gate)
				if (!_readers.Remove(args.Id, out reader)) throw InvalidFile();
			reader.Dispose();
			return Task.FromResult(new FileAccepted(true));
		});
		registry.Register("file.release", json.FileIdArgs, json.FileAccepted, (args, _) =>
		{
			lock (_gate)
			{
				if (!_files.Remove(args.Id)) throw InvalidFile();
				foreach (var (id, reader) in _readers.Where(pair => pair.Value.FileId == args.Id).ToArray())
				{
					_readers.Remove(id); reader.Dispose();
				}
			}
			return Task.FromResult(new FileAccepted(true));
		});
	}
	static CommandException InvalidFile() => new("invalid_file", "This selected file/reader does not belong to the calling live document.");
	Reader GetReader(string id)
	{
		lock (_gate)
			return !_disposed && _readers.TryGetValue(id, out var reader) ? reader : throw InvalidFile();
	}
	async Task<ReadResult> ReadAsync(ReadArgs args, Operation op)
	{
		if (args.Count <= 0 || args.Count > _limits.ChunkBytes)
			throw new CommandException("invalid_arguments", $"Read count must be between 1 and the {_limits.ChunkBytes}-byte transport chunk budget.");
		var reader = GetReader(args.Id);
		using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(op.Cancellation, reader.Lifetime.Token);
		await reader.Lock.WaitAsync(cancellation.Token);
		try
		{
			var result = await ReadChunkAsync(reader.Stream, args.Count, cancellation.Token);
			return new(Convert.ToBase64String(result.Bytes), result.Eof);
		}
		catch (ObjectDisposedException) when (reader.Lifetime.IsCancellationRequested) { throw new OperationCanceledException(cancellation.Token); }
		finally { reader.Lock.Release(); }
	}
	/// <summary>Reads one bounded raw chunk; no text conversion, whole-file allocation or silent truncation.</summary>
	/// <param name="stream">The actual selected file stream, owned by its resource.</param>
	/// <param name="count">Desired per-message bytes, from 1 through 65536.</param>
	/// <param name="cancellation">Owning reader/document cancellation.</param>
	/// <returns>Actual partial bytes and native EOF state.</returns>
	public static async Task<(byte[] Bytes, bool Eof)> ReadChunkAsync(Stream stream, int count, CancellationToken cancellation)
	{
		if (count is < 1 or > 65536) throw new CommandException("invalid_arguments", "Use a bounded native read chunk.");
		var buffer = new byte[count];
		var read = await stream.ReadAsync(buffer.AsMemory(), cancellation);
		cancellation.ThrowIfCancellationRequested();
		var eof = read == 0 || (stream.CanSeek && stream.Position >= stream.Length);
		return (read == buffer.Length ? buffer : buffer.AsSpan(0, read).ToArray(), eof);
	}
	FileMetadata[] AddFiles(FileResult[] files)
	{
		lock (_gate)
		{
			if (_disposed) throw InvalidFile();
			if (files.Length > _limits.SelectedFiles - _files.Count)
				throw new CommandException("resource_limit", "The host's selected-file handle budget was reached. Release unused handles.");
			return files.Select(file =>
			{
				var id = Guid.NewGuid().ToString("D");
				_files.Add(id, file);
				return new FileMetadata(id, file.FileName, file.ContentType);
			}).ToArray();
		}
	}
	static PickOptions Options(PickerOptions args)
	{
		if (args.FileTypes?.Count > 4 || args.FileTypes?.Any(pair => pair.Value.Length > 64 ||
			pair.Value.Any(value => string.IsNullOrWhiteSpace(value) || value.Length > 256)) == true)
			throw new CommandException("invalid_arguments", "Use bounded native platform file-type filters.");
		var options = new PickOptions { PickerTitle = args.PickerTitle };
		if (args.FileTypes is not null)
		{
			var types = new Dictionary<DevicePlatform, IEnumerable<string>>();
			foreach (var (platform, values) in args.FileTypes)
				types.Add(platform switch
				{
					"android" => DevicePlatform.Android, "ios" => DevicePlatform.iOS, "maccatalyst" => DevicePlatform.MacCatalyst,
					"windows" => DevicePlatform.WinUI,
					_ => throw new CommandException("invalid_arguments", "Unknown picker platform filter."),
				}, values);
			options.FileTypes = new FilePickerFileType(types);
		}
		return options;
	}
	void Clear()
	{
		lock (_gate)
		{
			foreach (var reader in _readers.Values) reader.Dispose();
			_readers.Clear(); _files.Clear();
		}
	}
	/// <summary>Releases all selected-file/read-stream resources on owning scope destruction.</summary>
	public void Dispose() { _context.DocumentEnded -= Clear; lock (_gate) { _disposed = true; Clear(); } }
}
sealed record PickerOptions(string? PickerTitle = null, Dictionary<string, string[]>? FileTypes = null);
sealed record FileMetadata(string Id, string Name, string? ContentType);
sealed record PickResult(FileMetadata? File);
sealed record PickMultipleResult(FileMetadata[] Files);
sealed record FileIdArgs(string Id);
sealed record ReaderMetadata(string Id, bool CanSeek, int MaximumChunkBytes);
sealed record ReadArgs(string Id, int Count);
sealed record SeekArgs(string Id, long Position);
sealed record ReadResult(string Base64, bool Eof);
sealed record FileAccepted(bool Accepted);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(PickerOptions))]
[JsonSerializable(typeof(PickResult))]
[JsonSerializable(typeof(PickMultipleResult))]
[JsonSerializable(typeof(FileIdArgs))]
[JsonSerializable(typeof(ReaderMetadata))]
[JsonSerializable(typeof(ReadArgs))]
[JsonSerializable(typeof(SeekArgs))]
[JsonSerializable(typeof(ReadResult))]
[JsonSerializable(typeof(FileAccepted))]
partial class FilePickerJsonContext : JsonSerializerContext;
