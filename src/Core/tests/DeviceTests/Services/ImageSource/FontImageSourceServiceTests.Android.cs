using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new FontImageSourceService(null);

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetDrawableAsync(imageSource, Platform.DefaultContext));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetDrawableAsync(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var host = new AppHostBuilder()
				.ConfigureFonts()
				.ConfigureImageSources()
				.Build();

			var images = host.Services.GetRequiredService<IImageSourceServiceProvider>();
			var service = images.GetRequiredImageSourceService<FontImageSourceStub>();

			var imageSource = new FontImageSourceStub
			{
				Glyph = "X",
				Font = Font.Default,
				Color = expectedColor,
			};

			using var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(drawable.Value);

			var bitmap = bitmapDrawable.Bitmap;

			bitmap.AssertContainsColor(expectedColor.ToNative());
		}

		[Fact]
		public async Task GetDrawableAsyncWithCustomFont()
		{
			var host = new AppHostBuilder()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
				})
				.ConfigureImageSources()
				.Build();

			var images = host.Services.GetRequiredService<IImageSourceServiceProvider>();
			var service = images.GetRequiredImageSourceService<FontImageSourceStub>();

			var imageSource = new FontImageSourceStub
			{
				Glyph = "X",
				Font = Font.OfSize("Dokdo", 24),
				Color = Colors.Red,
			};

			using var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(drawable.Value);

			var bitmap = bitmapDrawable.Bitmap;

			bitmap.AssertContainsColor(Colors.Red.ToNative());
		}
	}
}