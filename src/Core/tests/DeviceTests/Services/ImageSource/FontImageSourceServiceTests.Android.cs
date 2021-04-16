using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task GetDrawableAsyncReturnsNullForIncorrectTypes(Type type)
		{
			var service = new FileImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			Assert.Null(drawable);
		}
	}
}