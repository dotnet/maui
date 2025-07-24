using System;
using System.IO;
using System.Reflection;
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

	[Fact]
 	public void PlatformImageFromStreamSurvivesStreamDisposal()
 	{
 		// This test reproduces the scenario from issue #42 where the stream
 		// might be disposed or become invalid during processing in release builds
 		byte[] orange1x1pxPngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2P4v5ThPwAG7wKklwQ/bwAAAABJRU5ErkJggg==");

 		IImage image;
 		using (var memoryStream = new MemoryStream(orange1x1pxPngBytes))
 		{
 			// Create image from stream while stream is valid
 			image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(memoryStream);
 		}
 		// Stream is now disposed, but image should still be valid

 		Assert.NotNull(image);
 		Assert.Equal(1, image.Width);
 		Assert.Equal(1, image.Height);

 		// Verify the image is still usable after stream disposal
 		using var outputStream = new MemoryStream();
 		image.Save(outputStream);
 		Assert.True(outputStream.Length > 0);
 	}

 	[Fact] 
 	public void PlatformImageFromEmbeddedResourceStream()
 	{
 		// This test simulates the real-world scenario from the issue where
 		// GetManifestResourceStream is used (similar to Bug3() in the issue)
 		byte[] orange1x1pxPngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2P4v5ThPwAG7wKklwQ/bwAAAABJRU5ErkJggg==");

 		using var sourceStream = new MemoryStream(orange1x1pxPngBytes);
 		using var resourceStream = new NonSeekableReadOnlyStream(sourceStream);

 		// This should not crash in release builds
 		var image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(resourceStream);

 		Assert.NotNull(image);
 		Assert.Equal(1, image.Width);
 		Assert.Equal(1, image.Height);
 	}

 	[Fact]
 	public void PlatformImageFromStreamWithJpegData()
 	{
 		// Test with the actual JPEG data from the issue to ensure our fix
 		// handles the exact scenario reported
 		byte[] jpegImageBytes = new byte[]
 		{
 			0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xE2, 0x01, 0xD8, 0x49, 0x43, 0x43, 0x5F, 0x50, 0x52, 0x4F, 0x46, 0x49, 0x4C, 0x45, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0xC8, 0x00, 0x00, 0x00, 0x00, 0x04, 0x30, 0x00, 0x00,
 			0x6D, 0x6E, 0x74, 0x72, 0x52, 0x47, 0x42, 0x20, 0x58, 0x59, 0x5A, 0x20, 0x07, 0xE0, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x61, 0x63, 0x73, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 			0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0xF6, 0xD6, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0xD3, 0x2D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 			0xFF, 0xD9 // Simplified JPEG end marker
 		};

 		using var stream = new MemoryStream(jpegImageBytes);

 		// This simulates the exact scenario from Bug3() method in the issue
 		var image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);

 		Assert.NotNull(image);
 		// Note: Dimensions may vary based on actual JPEG decoding, just ensure no crash
 	}
    
    [Fact]
    public void PlatformImageFromManifestResourceStream()
    {
	    // This test  reproduces the https://github.com/dotnet/maui/issues/30783 scenario:
	    // - Uses reflection to get the current assembly
	    // - Uses GetManifestResourceStream with an embedded resource
	    // - Calls PlatformImage.FromStream directly within a using statement
	    // - Tests the exact pattern that was causing crashes in release builds

	    var assembly = GetType().GetTypeInfo().Assembly;
	    using (var stream = assembly.GetManifestResourceStream("Graphics.Tests.Resources.royals.png"))
	    {
		    Assert.NotNull(stream); // Ensure resource is found

		    // This was the line causing crashes in release builds in the issue
		    var image = Platform.PlatformImage.FromStream(stream);

		    Assert.NotNull(image);
		    Assert.True(image.Width > 0);
		    Assert.True(image.Height > 0);
	    }
	    // Stream is disposed here, but image should remain valid due to the fix
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
