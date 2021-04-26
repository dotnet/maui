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
		public async Task GetDrawableAsyncReturnsNullForIncorrectTypes(Type type)
		{
			var service = new FontImageSourceService(null);

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			using var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			Assert.Null(drawable);
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetDrawableAsync(string colorHex)
		{
			var expectedColor = Color.FromHex(colorHex);

			var host = new AppHostBuilder()
				.ConfigureFonts()
				.ConfigureImageSourceServices()
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
				.ConfigureImageSourceServices()
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