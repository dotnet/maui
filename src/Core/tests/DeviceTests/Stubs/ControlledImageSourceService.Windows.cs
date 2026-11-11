using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	internal sealed class ControlledImageSourceService : IImageSourceServiceProvider, IImageSourceService
	{
		public Task<IImageSourceServiceResult<ImageSource>> Result { get; set; }

		public IServiceProvider HostServiceProvider => this;

		public object GetService(Type serviceType) => this;

		public IImageSourceService GetImageSourceService(Type imageSource) => this;

		public Task<IImageSourceServiceResult<ImageSource>> GetImageSourceAsync(
			IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) => Result;
	}
}
