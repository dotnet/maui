using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public Task<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageSourceAsync((ICountedImageSourceStub)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>> GetImageSourceAsync(ICountedImageSourceStub imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}