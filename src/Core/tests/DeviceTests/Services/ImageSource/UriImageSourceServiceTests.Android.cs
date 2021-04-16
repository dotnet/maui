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
		public async Task GetDrawableAsyncReturnsNullForIncorrectTypes(Type type)
		{
			var service = new FileImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			Assert.Null(drawable);
		}
	}
}