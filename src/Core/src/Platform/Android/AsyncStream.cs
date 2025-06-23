using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Represents a stream that reads data asynchronously from a task that
/// produces either a byte array or a stream.
/// This class is useful for scenarios where the data source is not immediately
/// available and needs to be fetched asynchronously.
/// 
/// Specifically, the Android WebView requires that you provide a stream
/// immediately, but the data may not be available until later.
/// This class allows you to wrap a task that fetches the data and provides
/// an asynchronous stream interface to read from it.
/// </summary>
class AsyncStream : Stream
{
	readonly Task<Stream?> _streamTask;
	readonly ILogger? _logger;
	Stream? _stream;
	bool _isDisposed;

	public AsyncStream(Task<byte[]?> byteArrayTask, ILogger? logger)
		: this(AsStreamTask(byteArrayTask), logger)
	{
	}

	public AsyncStream(Task<Stream?> streamTask, ILogger? logger)
	{
		_streamTask = streamTask ?? throw new ArgumentNullException(nameof(streamTask));
		_logger = logger;
	}

	static async Task<Stream?> AsStreamTask(Task<byte[]?> task)
	{
		var bytes = await task;
		if (bytes is null)
			return Stream.Null;
		return new MemoryStream(bytes);
	}

	async Task<Stream?> GetStreamAsync(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, nameof(AsyncStream));

		if (_stream != null)
			return _stream;

		cancellationToken.ThrowIfCancellationRequested();

		_stream = await _streamTask.ConfigureAwait(false);
		return _stream;
	}

	public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
	{
		try
		{
			var stream = await GetStreamAsync(cancellationToken).ConfigureAwait(false);
			if (stream is null)
				return 0;
			return await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Error reading from asynchronous stream: {ErrorMessage}", ex.Message);
			throw;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		try
		{
			var stream = GetStreamAsync().GetAwaiter().GetResult();
			if (stream is null)
				return 0;
			return stream.Read(buffer, offset, count);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Error reading from asynchronous stream: {ErrorMessage}", ex.Message);
			throw;
		}
	}

	public override void Flush() => throw new NotSupportedException();

	public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

	public override bool CanRead => !_isDisposed;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => throw new NotSupportedException();

	public override long Position
	{
		get => throw new NotSupportedException();
		set => throw new NotSupportedException();
	}

	public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

	public override void SetLength(long value) => throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	protected override void Dispose(bool disposing)
	{
		if (_isDisposed)
			return;

		if (disposing)
			_stream?.Dispose();

		_isDisposed = true;
		base.Dispose(disposing);
	}

	public override async ValueTask DisposeAsync()
	{
		if (_isDisposed)
			return;

		if (_stream is not null)
			await _stream.DisposeAsync().ConfigureAwait(false);

		_isDisposed = true;
		await base.DisposeAsync().ConfigureAwait(false);
	}
}
