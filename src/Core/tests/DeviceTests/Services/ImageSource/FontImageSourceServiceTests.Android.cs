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

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetDrawableAsync(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureFonts()
				.ConfigureImageSources()
				.Build();

			var images = mauiApp.Services.GetRequiredService<IImageSourceServiceProvider>();
			var service = images.GetRequiredImageSourceService<FontImageSourceStub>();

			var imageSource = new FontImageSourceStub
			{
				Glyph = "X",
				Font = Font.Default,
				Color = expectedColor,
			};

			using var result = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(result.Value);

			var bitmap = bitmapDrawable.Bitmap;

			await bitmap.AssertContainsColor(expectedColor.ToPlatform()).ConfigureAwait(false);
		}

		[Theory]
		[InlineData("25d0", "\u25d0")]
		[InlineData("25d025d1", "\u25d0\u25d1")]
		[InlineData(null, null)]
		public void GetGlyphHexSerializesCorrectly(string expected, string input)
		{
			Assert.Equal(expected, PlatformInterop.GetGlyphHex(input));
		}

		[Fact]
		public async Task GetDrawableAsyncWithCustomFont()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
				})
				.ConfigureImageSources()
				.Build();

			var images = mauiApp.Services.GetRequiredService<IImageSourceServiceProvider>();
			var service = images.GetRequiredImageSourceService<FontImageSourceStub>();

			var imageSource = new FontImageSourceStub
			{
				Glyph = "X",
				Font = Font.OfSize("Dokdo", 24),
				Color = Colors.Red,
			};

			using var result = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(result.Value);

			var bitmap = bitmapDrawable.Bitmap;

			await bitmap.AssertContainsColor(Colors.Red.ToPlatform()).ConfigureAwait(false);
		}
	}
}