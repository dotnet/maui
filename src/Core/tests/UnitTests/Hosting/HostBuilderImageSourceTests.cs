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
			var builder = MauiApp
				.CreateBuilder()
				.ConfigureImageSources();
			builder.Services.AddKeyedSingleton<IImageSourceService>(typeof(FileImageSourceStub), (ksvcs, _) => ((IKeyedServiceProvider)ksvcs).GetRequiredKeyedService<IImageSourceService>(typeof(IFileImageSource)));
			var mauiApp = builder.Build();

			var images = (IKeyedServiceProvider)mauiApp.Services;
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredKeyedService<IImageSourceService>(type);
			Assert.NotNull(imageSourceService);
			Assert.IsType<FileImageSourceService>(imageSourceService);
		}

		[Fact]
		public void CanRetrieveFontUsingInterfaceImageSource()
		{
			var builder = MauiApp
				.CreateBuilder()
				.ConfigureFonts()
				.ConfigureImageSources();
			var mauiApp = builder.Build();

			var manager = mauiApp.Services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var images = (IKeyedServiceProvider)mauiApp.Services;
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredKeyedService<IImageSourceService>(typeof(IFontImageSource));
			Assert.NotNull(imageSourceService);
			var fontService = Assert.IsType<FontImageSourceService>(imageSourceService);

			Assert.Equal(manager, fontService.FontManager);
		}

		[Fact]
		public void CanRetrieveFontUsingConcreteImageSource()
		{
			var builder = MauiApp
				.CreateBuilder()
				.ConfigureFonts()
				.ConfigureImageSources();
			builder.Services.AddKeyedSingleton<IImageSourceService>(typeof(FontImageSourceStub), (ksvcs, _) => ((IKeyedServiceProvider)ksvcs).GetRequiredKeyedService<IImageSourceService>(typeof(IFontImageSource)));
			var mauiApp = builder.Build();

			var manager = mauiApp.Services.GetRequiredService<IFontManager>();
			Assert.NotNull(manager);

			var images = (IKeyedServiceProvider)mauiApp.Services;
			Assert.NotNull(images);

			var imageSourceService = images.GetRequiredKeyedService<IImageSourceService>(typeof(FontImageSourceStub));
			Assert.NotNull(imageSourceService);
			var fontService = Assert.IsType<FontImageSourceService>(imageSourceService);

			Assert.Equal(manager, fontService.FontManager);
		}
	}
}