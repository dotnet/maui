using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes([DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			var service = new FontImageSourceService(null);

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetImageAsync(imageSource));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetImageAsync(string colorHex)
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
				Font = Font.Default.WithSize(30),
				Color = expectedColor,
			};

			using var drawable = await InvokeOnMainThreadAsync(() => service.GetImageAsync(imageSource));

			var uiimage = Assert.IsType<UIImage>(drawable.Value);

			await uiimage.AssertContainsColor(expectedColor.ToPlatform()).ConfigureAwait(false);
		}

		[Fact]
		public async Task GetImageAsyncWithCustomFont()
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

			using var drawable = await InvokeOnMainThreadAsync(() => service.GetImageAsync(imageSource));

			var uiimage = Assert.IsType<UIImage>(drawable.Value);

			await uiimage.AssertContainsColor(Colors.Red.ToPlatform()).ConfigureAwait(false);
		}
	}
}