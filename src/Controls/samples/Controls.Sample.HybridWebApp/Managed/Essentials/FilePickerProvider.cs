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
	/// <param name="chunkBytes">Maximum raw bytes in one binary transport response.</param>
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
public sealed class FilePickerProvider : IDisposable
{
	readonly IFilePicker _picker;
	readonly HybridViewContext _context;
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
	/// <param name="limits">Optional host resource/message budgets, not file capacity.</param>
	public FilePickerProvider(IFilePicker picker, HybridViewContext context, HybridFileLimits? limits = null)
	{
		_picker = picker; _context = context; _limits = limits ?? new HybridFileLimits();
		context.DocumentEnded += Clear;
	}
	[HybridAction("filePicker.pick", MainThread = true, Interactive = true)]
	internal Task<FileMetadata?> Pick(CancellationToken cancellation, string? pickerTitle = null, Dictionary<string, string[]>? fileTypes = null)
	{
		var options = Options(pickerTitle, fileTypes);
		// The native API cannot be force-dismissed. InteractAsync retains the OS interaction slot until dismissal,
		// while request/document cancellation prevents the late selection from becoming a document-owned resource.
		return _context.InteractAsync(async () =>
		{
#if DEBUG && (IOS || MACCATALYST || ANDROID || WINDOWS)
			Console.WriteLine($"HYBRIDWEBAPP_PICKER phase=before-await mainThread={Microsoft.Maui.ApplicationModel.MainThread.IsMainThread} cancellation={cancellation.IsCancellationRequested} ownerAttached={_context.Window.Handler is not null} pageOwned={ReferenceEquals(_context.Page.Window, _context.Window)} picker={_picker.GetType().FullName}");
#endif
			var file = await _picker.PickAsync(options);
#if DEBUG && (IOS || MACCATALYST || ANDROID || WINDOWS)
			Console.WriteLine($"HYBRIDWEBAPP_PICKER phase=after-await selected={file is not null} cancellation={cancellation.IsCancellationRequested} disposed={_disposed}");
#endif
			cancellation.ThrowIfCancellationRequested();
			var result = file is null ? null : AddFiles([file]).Single();
#if DEBUG && (IOS || MACCATALYST || ANDROID || WINDOWS)
			Console.WriteLine($"HYBRIDWEBAPP_PICKER phase=return selected={result is not null} cancellation={cancellation.IsCancellationRequested}");
#endif
			return result;
		}, cancellation);
	}

	[HybridAction("filePicker.pickMultiple", MainThread = true, Interactive = true)]
	internal Task<PickMultipleResult> PickMultiple(CancellationToken cancellation, string? pickerTitle = null, Dictionary<string, string[]>? fileTypes = null)
	{
		var options = Options(pickerTitle, fileTypes);
		return _context.InteractAsync(async () =>
		{
			var files = await _picker.PickMultipleAsync(options);
			cancellation.ThrowIfCancellationRequested();
			return new PickMultipleResult(files is null ? [] : AddFiles(files.ToArray()));
		}, cancellation);
	}

	[HybridAction("file.openRead", MainThread = true)]
	internal async Task<ReaderMetadata> OpenRead(string id, CancellationToken cancellation)
	{
		FileResult file;
		lock (_gate)
		{
			if (_disposed || !_files.TryGetValue(id, out file!)) throw InvalidFile();
			if (_readers.Count >= _limits.Readers) throw new CommandException("resource_limit", "The host's open file-reader budget was reached.");
		}
		var stream = await file.OpenReadAsync();
		try
		{
			cancellation.ThrowIfCancellationRequested();
			lock (_gate)
			{
				if (_disposed || !_files.ContainsKey(id)) throw InvalidFile();
				if (_readers.Count >= _limits.Readers) throw new CommandException("resource_limit", "The host's open file-reader budget was reached.");
				var readerId = Guid.NewGuid().ToString("D");
				_readers.Add(readerId, new Reader(id, stream));
				return new ReaderMetadata(readerId, stream.CanSeek, _limits.ChunkBytes);
			}
		}
		catch { stream.Dispose(); throw; }
	}

	[HybridAction("file.seek")]
	internal async Task<FileAccepted> Seek(string id, long position, CancellationToken cancellation)
	{
		if (position < 0 || position > 9007199254740991)
			throw new CommandException("invalid_arguments", "Use a nonnegative safe-integer byte position.");
		var reader = GetReader(id);
		using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, reader.Lifetime.Token);
		await reader.Lock.WaitAsync(linked.Token);
		try
		{
			linked.Token.ThrowIfCancellationRequested();
			if (!reader.Stream.CanSeek) throw new NotSupportedException("This selected native stream does not support seeking.");
			reader.Stream.Seek(position, SeekOrigin.Begin);
			linked.Token.ThrowIfCancellationRequested();
			return new FileAccepted(true);
		}
		catch (ObjectDisposedException) when (reader.Lifetime.IsCancellationRequested) { throw new OperationCanceledException(linked.Token); }
		finally { reader.Lock.Release(); }
	}

	[HybridAction("file.closeReader")]
	internal FileAccepted CloseReader(string id)
	{
		Reader? reader;
		lock (_gate)
			if (!_readers.Remove(id, out reader)) throw InvalidFile();
		reader.Dispose();
		return new(true);
	}

	[HybridAction("file.release")]
	internal FileAccepted Release(string id)
	{
		lock (_gate)
		{
			if (!_files.Remove(id)) throw InvalidFile();
			foreach (var (readerId, reader) in _readers.Where(pair => pair.Value.FileId == id).ToArray())
			{
				_readers.Remove(readerId); reader.Dispose();
			}
		}
		return new(true);
	}
	static CommandException InvalidFile() => new("invalid_file", "This selected file/reader does not belong to the calling live document.");
	Reader GetReader(string id)
	{
		lock (_gate)
			return !_disposed && _readers.TryGetValue(id, out var reader) ? reader : throw InvalidFile();
	}
	[HybridAction("file.readBytes")]
	internal async Task<byte[]> ReadAsync(string id, int count, CancellationToken cancellation)
	{
		if (count <= 0 || count > _limits.ChunkBytes)
			throw new CommandException("invalid_arguments", $"Read count must be between 1 and the {_limits.ChunkBytes}-byte transport chunk budget.");
		var reader = GetReader(id);
		using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, reader.Lifetime.Token);
		await reader.Lock.WaitAsync(linked.Token);
		try
		{
			var result = await ReadChunkAsync(reader.Stream, count, linked.Token);
			// One bounded discriminator byte preserves exact EOF, including partial/nonseekable reads.
			var response = new byte[result.Bytes.Length + 1];
			response[0] = result.Eof ? (byte)1 : (byte)0;
			result.Bytes.CopyTo(response, 1);
			return response;
		}
		catch (ObjectDisposedException) when (reader.Lifetime.IsCancellationRequested) { throw new OperationCanceledException(linked.Token); }
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
	static PickOptions Options(string? pickerTitle, Dictionary<string, string[]>? fileTypes)
	{
		if (fileTypes?.Count > 4 || fileTypes?.Any(pair => pair.Value.Length > 64 ||
			pair.Value.Any(value => string.IsNullOrWhiteSpace(value) || value.Length > 256)) == true)
			throw new CommandException("invalid_arguments", "Use bounded native platform file-type filters.");
		var options = new PickOptions { PickerTitle = pickerTitle };
		if (fileTypes is not null)
		{
			var types = new Dictionary<DevicePlatform, IEnumerable<string>>();
			foreach (var (platform, values) in fileTypes)
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
sealed record FileMetadata(string Id, string Name, string? ContentType);
sealed record PickMultipleResult(FileMetadata[] Files);
sealed record ReaderMetadata(string Id, bool CanSeek, int MaximumChunkBytes);
sealed record FileAccepted(bool Accepted);
