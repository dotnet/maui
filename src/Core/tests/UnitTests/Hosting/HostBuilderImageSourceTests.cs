using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderImageSourceTests
	{
		[Theory]
		[InlineData(typeof(IFileImageSource))]
		[InlineData(typeof(FileImageSourceStub))]
		public void CanRetrieveFileUsingInterfaceImageSource(Type type)
		{
			var host = new AppHostBuilder()
				.ConfigureImageSources()
				.Build();

			var images = host.Services.GetRequiredService<IImageSourceServiceProvider>();
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredImageSourceService(type);
			Assert.NotNull(imageSourceService);
			Assert.IsType<FileImageSourceService>(imageSourceService);
		}

		[Fact]
		public void CanRetrieveFontUsingInterfaceImageSource()
		{
			var host = new AppHostBuilder()
				.ConfigureFonts()
				.ConfigureImageSources()
				.Build();

			var manager = host.Services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var images = host.Services.GetRequiredService<IImageSourceServiceProvider>();
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredImageSourceService<IFontImageSource>();
			Assert.NotNull(imageSourceService);
			var fontService = Assert.IsType<FontImageSourceService>(imageSourceService);

			Assert.Equal(manager, fontService.FontManager);
		}

		[Fact]
		public void CanRetrieveFontUsingConcreteImageSource()
		{
			var host = new AppHostBuilder()
				.ConfigureFonts()
				.ConfigureImageSources()
				.Build();

			var manager = host.Services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var images = host.Services.GetRequiredService<IImageSourceServiceProvider>();
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredImageSourceService<FontImageSourceStub>();
			Assert.NotNull(imageSourceService);
			var fontService = Assert.IsType<FontImageSourceService>(imageSourceService);

			Assert.Equal(manager, fontService.FontManager);
		}
	}
}