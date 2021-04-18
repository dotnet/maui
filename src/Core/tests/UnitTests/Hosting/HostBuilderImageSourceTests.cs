using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderImageSourceTests
	{
		[Fact]
		public void ConfigureFontsRegistersTheCorrectServices()
		{
			var host = new AppHostBuilder()
				.ConfigureFonts()
				.ConfigureImageSourceServices()
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
	}
}