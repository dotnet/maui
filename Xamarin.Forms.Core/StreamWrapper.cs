using System;
using System.IO;

namespace Xamarin.Forms
{
	internal class StreamWrapper : Stream
	{
		readonly Stream _wrapped;

		public StreamWrapper(Stream wrapped)
		{
			if (wrapped == null)
				throw new ArgumentNullException("wrapped");

			_wrapped = wrapped;
		}

		public override bool CanRead
		{
			get { return _wrapped.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _wrapped.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _wrapped.CanWrite; }
		}

		public override long Length
		{
			get { return _wrapped.Length; }
		}

		public override long Position
		{
			get { return _wrapped.Position; }
			set { _wrapped.Position = value; }
		}

		public event EventHandler Disposed;

		public override void Flush()
		{
			_wrapped.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _wrapped.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _wrapped.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_wrapped.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_wrapped.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			_wrapped.Dispose();
			EventHandler eh = Disposed;
			if (eh != null)
				eh(this, EventArgs.Empty);

			base.Dispose(disposing);
		}
	}
}