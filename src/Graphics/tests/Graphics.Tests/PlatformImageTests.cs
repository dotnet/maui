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
		byte[] orange2x3pxPngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAIAAAADCAIAAAA2iEnWAAAACXBIWXMAAAABAAAAAQBPJcTWAAAAE0lEQVR4nGP8u5SBgYGBBUTAKQAggQGsd/BKkQAAAABJRU5ErkJggg==");
		using MemoryStream memoryStream = new(orange2x3pxPngBytes);
		Stream stream = seekable ? memoryStream : new NonSeekableReadOnlyStream(memoryStream);

		var image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);

		Assert.NotNull(image);
		Assert.Equal(2f, image.Width);
		Assert.Equal(3f, image.Height);
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
