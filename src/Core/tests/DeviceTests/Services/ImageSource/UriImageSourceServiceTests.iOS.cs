using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class UriImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new UriImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetImageAsync(imageSource));
		}

		[Theory]
		[InlineData("https://test.com/file", "{hash}")]
		[InlineData("https://test.com/file#test", "{hash}")]
		[InlineData("https://test.com/file#test=123", "{hash}")]
		[InlineData("https://test.com/file?test", "{hash}")]
		[InlineData("https://test.com/file?test=123", "{hash}")]
		[InlineData("https://test.com/file.png", "{hash}.png")]
		[InlineData("https://test.com/file.jpg", "{hash}.jpg")]
		[InlineData("https://test.com/file.gif", "{hash}.gif")]
		[InlineData("https://test.com/file.jpg?ids", "{hash}.jpg")]
		[InlineData("https://test.com/file.jpg?id=123", "{hash}.jpg")]
		[InlineData("https://test.com/file.gif#id=123", "{hash}.gif")]
		[InlineData("https://test.com/file.gif#ids", "{hash}.gif")]
		public void CachedFilenameIsCorrectAndValid(string uri, string expected)
		{
			using var algorithm = new Crc64HashAlgorithm();
			var hashed = algorithm.ComputeHashString(uri);
			expected = expected.Replace("{hash}", hashed, StringComparison.OrdinalIgnoreCase);

			var service = new UriImageSourceService();

			var filename = service.GetCachedFileName(new UriImageSourceStub { Uri = new Uri(uri) });

			Assert.Equal(expected, filename);
		}
	}
}