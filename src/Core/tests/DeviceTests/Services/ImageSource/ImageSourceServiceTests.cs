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
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<IFileImageSource, FileImageSourceService>();
			});

			var imageSource = new FileImageSourceStub();

			var service = provider.GetRequiredImageSourceService(imageSource);

			Assert.IsType<FileImageSourceService>(service);
		}

		[Fact]
		public void CanResolveCorrectServiceWhenMultiple()
		{
			var provider = CreateImageSourceServiceProvider(services =>
			{
				services.AddService<IFileImageSource, FileImageSourceService>();
				services.AddService<IUriImageSource, UriImageSourceService>();
			});

			var service = provider.GetRequiredImageSourceService(new FileImageSourceStub());
			Assert.IsType<FileImageSourceService>(service);

			service = provider.GetRequiredImageSourceService(new UriImageSourceStub());
			Assert.IsType<UriImageSourceService>(service);
		}

		[Fact]
		public void ThrowsWhenMissingService()
		{
			var provider = CreateImageSourceServiceProvider(services => { });

			var ex = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredImageSourceService(new FileImageSourceStub()));

			Assert.Contains(nameof(FileImageSourceStub), ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void ThrowsWhenNotASpecificImageSource()
		{
			var provider = CreateImageSourceServiceProvider(services => { });

			var ex = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredImageSourceService(new InvalidImageSourceStub()));

			Assert.Contains(nameof(InvalidImageSourceStub), ex.Message, StringComparison.Ordinal);
			Assert.Contains(nameof(IImageSource), ex.Message, StringComparison.Ordinal);
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

		private IImageSourceServiceProvider CreateImageSourceServiceProvider(Action<IImageSourceServiceCollection> configure)
		{
			var mauiApp = MauiApp.CreateBuilder(useDefaults: false)
				.ConfigureImageSources(configure)
				.Build();

			var provider = mauiApp.Services.GetRequiredService<IImageSourceServiceProvider>();

			return provider;
		}
	}
}