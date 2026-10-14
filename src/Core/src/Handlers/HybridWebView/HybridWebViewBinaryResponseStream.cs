using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	// Android asks for the response stream synchronously, then reads it on its network thread.
	internal sealed class HybridWebViewBinaryResponseStream : Stream
	{
		readonly object _gate = new();
		readonly Task<byte[]> _response;
		readonly CancellationTokenSource _cancellation;
		readonly Func<bool> _isCurrentDocument;
		int _position;
		bool _disposed;

		internal HybridWebViewBinaryResponseStream(Task<byte[]> response, CancellationTokenSource cancellation, Func<bool> isCurrentDocument)
		{
			_response = response;
			_cancellation = cancellation;
			_isCurrentDocument = isCurrentDocument;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer is null || offset < 0 || count < 0 || offset > buffer.Length - count)
				throw new ArgumentException("Invalid response buffer range.");
			var bytes = _response.GetAwaiter().GetResult();
			lock (_gate)
			{
				if (_disposed || !_isCurrentDocument())
				{
					Array.Clear(bytes, 0, bytes.Length);
					throw new OperationCanceledException("Binary response belongs to an interrupted document.");
				}
				_cancellation.Token.ThrowIfCancellationRequested();
				var read = Math.Min(count, bytes.Length - _position);
				Buffer.BlockCopy(bytes, _position, buffer, offset, read);
				_position += read;
				return read;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (_gate)
				{
					if (_disposed)
						return;
					_disposed = true;
				}
				try
				{ _cancellation.Cancel(); }
				catch (AggregateException) { }
				_ = _response.ContinueWith(task =>
				{
					if (task.Status == TaskStatus.RanToCompletion)
						Array.Clear(task.Result, 0, task.Result.Length);
					_cancellation.Dispose();
				}, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			}
			base.Dispose(disposing);
		}

		public override bool CanRead => !_disposed;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override void Flush() => throw new NotSupportedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
