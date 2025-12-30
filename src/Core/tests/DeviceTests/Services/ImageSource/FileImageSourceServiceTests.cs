using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ImageSource)]
	public partial class FileImageSourceServiceTests : BaseImageSourceServiceTests
	{
#if ANDROID || IOS || MACCATALYST
		[Theory]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes([DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			var service = new FileImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetImageAsync(imageSource));
		}

		[Theory]
		[InlineData("red.png", "#FF0000")]
		[InlineData("green.png", "#00FF00")]
		[InlineData("black.png", "#000000")]
		public async Task GetImageAsyncWithResource(string filename, string colorHex)
		{
#if IOS || MACCATALYST
			await MainThread.InvokeOnMainThreadAsync(async () =>
		   {
#endif
			   var expectedColor = Color.FromArgb(colorHex);

			   var service = new FileImageSourceService();

			   var imageSource = new FileImageSourceStub(filename);

			   using var result = await service.GetImageAsync(imageSource);
			   var image = result.Value;

			   image.AssertColorAtCenter(expectedColor.ToPlatform());
#if IOS || MACCATALYST
		   });
#endif
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetImageAsyncWithFile(string colorHex)
		{
#if IOS || MACCATALYST
			await MainThread.InvokeOnMainThreadAsync(async () =>
		   {
#endif
			   var expectedColor = Color.FromArgb(colorHex);

			   var service = new FileImageSourceService();

			   var filename = CreateBitmapFile(100, 100, expectedColor);
			   var imageSource = new FileImageSourceStub(filename);

			   using var result = await service.GetImageAsync(imageSource);
			   var image = result.Value;

			   image.AssertColorAtCenter(expectedColor.ToPlatform());
#if IOS || MACCATALYST
		   });
#endif

		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetImageAsyncWithFileLoadsFileInsteadOfResource(string colorHex)
		{
#if IOS || MACCATALYST
			await MainThread.InvokeOnMainThreadAsync(async () =>
		   {
#endif
			   var expectedColor = Color.FromArgb(colorHex);

			   var service = new FileImageSourceService();

			   var filename = CreateBitmapFile(100, 100, expectedColor, "blue.png");
			   var imageSource = new FileImageSourceStub(filename);

			   using var result = await service.GetImageAsync(imageSource);
			   var image = result.Value;

			   image.AssertColorAtCenter(expectedColor.ToPlatform());
#if IOS || MACCATALYST
		   });
#endif
		}
#endif
	}
}