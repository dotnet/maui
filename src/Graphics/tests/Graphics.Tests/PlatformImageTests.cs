using System;
using System.IO;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests;

public class PlatformImageTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void PlatformImageFromStreamTest(bool seekable)
	{
		byte[] orange1x1pxPngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2P4v5ThPwAG7wKklwQ/bwAAAABJRU5ErkJggg==");
		using MemoryStream memoryStream = new(orange1x1pxPngBytes);
		Stream stream = seekable ? memoryStream : new NonSeekableReadOnlyStream(memoryStream);

		var image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);

		Assert.NotNull(image);
	}

	private class NonSeekableReadOnlyStream(Stream stream) : Stream
	{
		public override bool CanRead => stream.CanRead;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get { return stream.Position; }
			set { throw new NotSupportedException(); }
		}

		public override void Flush() => throw new NotSupportedException();

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

		public override void SetLength(long value) => throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
