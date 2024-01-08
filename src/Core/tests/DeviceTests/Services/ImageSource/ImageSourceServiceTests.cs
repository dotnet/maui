using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageSourceServiceTests : BaseImageSourceServiceTests
	{
		[Fact]
		public void CanResolveCorrectService()
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddKeyedSingleton<IImageSourceService, FileImageSourceService>(typeof(FileImageSourceStub));

			var provider = (IKeyedServiceProvider)builder.Build().Services;

			var imageSource = new FileImageSourceStub();

			var service = provider.GetRequiredKeyedService<IImageSourceService>(imageSource.GetType());

			Assert.IsType<FileImageSourceService>(service);
		}

		[Fact]
		public void CanResolveCorrectServiceWhenMultiple()
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.Services.AddKeyedSingleton<IImageSourceService, FileImageSourceService>(typeof(FileImageSourceStub));
			builder.Services.AddKeyedSingleton<IImageSourceService, UriImageSourceService>(typeof(UriImageSourceStub));

			var provider = (IKeyedServiceProvider)builder.Build().Services;

			var fileImageSource = new FileImageSourceStub();
			var service = provider.GetRequiredKeyedService<IImageSourceService>(fileImageSource.GetType());
			Assert.IsType<FileImageSourceService>(service);

			var uriImageSource = new UriImageSourceStub();
			service = provider.GetRequiredKeyedService<IImageSourceService>(uriImageSource.GetType());
			Assert.IsType<UriImageSourceService>(service);
		}

		[Fact]
		public void ThrowsWhenMissingService()
		{
			var provider = (IKeyedServiceProvider)MauiApp.CreateBuilder(useDefaults: false).Build().Services;

			var ex = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredKeyedService<IImageSourceService>(typeof(FileImageSourceStub)));

			Assert.Contains(nameof(IImageSourceService), ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public async Task ResultsDisposeCorrectlyAndOnce()
		{
			var dispose = 0;

			var cache = new CustomImageCacheStub();
			var image = await InvokeOnMainThreadAsync(() => cache.Get(Colors.Red));

			var result = new ImageSourceServiceResult(image, () => dispose++);

			Assert.False(result.IsDisposed);
			Assert.Equal(0, dispose);

			result.Dispose();

			Assert.True(result.IsDisposed);
			Assert.Equal(1, dispose);

			result.Dispose();

			Assert.True(result.IsDisposed);
			Assert.Equal(1, dispose);

			cache.Return(Colors.Red);
		}
	}
}