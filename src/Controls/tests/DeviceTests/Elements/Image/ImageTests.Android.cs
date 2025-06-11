using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageTests
	{
		const string coffeeBase64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAA4CAYAAAC7UXvqAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3gUJADAhwicxqAAAAB1pVFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAABTklEQVRo3u2ZvUoDQRSFv2g6FZ/A0p/G9PZpJCCWFr6BoAGfxsY3sTZgbAKRYCtBCwkh6aIQmyFGEJPZnd25Y86B6XaX882cvXOHAemHpm6UprXUZ2zlAPJGJHjEFCEBCEAAAlhtAEmSpLiqzLW5SfpPfh+oJpKQ3w5GaiUEIAABCEAAAhDAf++FpsuuwCT1CI0Nent13ehfYwbQNwjQ91mBnkGAJx+AlkGAts/DNb6vf6yMA1/iriHznSwb2a2h+NxkeWkTeDcw+2/ARlbypgGAi7ytRTui+XtgPW/+9oFRBPNDYDfUT3QCfJZofgI0QleC85IgPoCzosrZqWv0ijI/Ao6Lrsl7wGMB5h/ct0s7+FwBgwDGB8BlrMPUNnDtOkVf4123z2yFNFTJ8e4hUAeOXBR25syNgRfg2dX2O5/+JguA1RuahROsm/rY+gI8XGfJmDMlSQAAAABJRU5ErkJggg==";
		const string booksBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMjHxIGmVAAAB5UlEQVR4Xu2XsW4UQRBEzxkiITIidGgSZIgsMKk/gj/ylzgkceiIFDmFiMwiIkQkIKAab0mtUa00Y/qsG1096en2ptatmdoNzhtjjDHGmBXewmv4A/4p9BM8heQMfob5HpLXlKRdjz3H3mP2vTiHP2E7uMpbSL7CNifteitRWRhniLMM0z6RbUhGsyxRGY03boinUA2qloxmWaKy7CHs5giqIdWS0SxLVJaNM3XjAqAaUi0ZzbJEZVkXALtxAVANqZaMZlmisqwLgN3sagG/FvMayWvK6Qv4AuN/kzCu1+5bc9oC4rf8BXz879sdcR1rkRE1IzttASfLZ/BukeRMzchOW0DwDL6HzOI61jL5b5XTFhBP/Bts81jLb0Obt05XQPvU1+TboLLsdAWop75mz73TFVCtC4DduACohlRLVFatC4DduACohlRLVFatC4DduACohlRLVFatC4DduACohlRLVFatC4DduACohlRLVFatC4DduACohlRLVFatC4Dd7H0BT+BvqAZVSlRWaZwlzjTEB6iGVUpUVmmcZZhX8DtUA6skKqsyzvAS3osX8Apuq4jLRZX9r7Hn2HucYad4BN/AG8jNfoSvYWR7w3PIAo5jYd84gCwgrvcSFmCMMcYY88BsNn8BZTc8uHKN4gQAAAAASUVORK5CYII=";

		[Fact]
		public async Task CachingDisabledForImagesLoadViaInputStreams()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var image1 = ImageSource.FromStream(() => new TestMemoryStream(Convert.FromBase64String(coffeeBase64)));
				var image2 = ImageSource.FromStream(() => new TestMemoryStream(Convert.FromBase64String(booksBase64)));

				var platformImage1 = await image1.GetPlatformImageAsync(MauiContext);
				var platformImage2 = await image2.GetPlatformImageAsync(MauiContext);

				var bitmapDrawable1 = Assert.IsType<BitmapDrawable>(platformImage1.Value);
				var bitmapDrawable2 = Assert.IsType<BitmapDrawable>(platformImage2.Value);

				// If Glide is using caching for images loaded from streams, then the second image will
				// match the first image (because the second image load attempt will return the cached image)

				// So we assert that the images are _not_ equal to ensure that caching is _not_ turned on for
				// image streams.
				await bitmapDrawable1.Bitmap.AssertNotEqualAsync(bitmapDrawable2.Bitmap);
			});
		}

		[Fact]
		public async Task ImageSetFromStreamRenders()
		{
			SetupBuilder();
			var layout = new VerticalStackLayout();

			using var stream = GetType().Assembly.GetManifestResourceStream("red-embedded.png");

			var image = new Image
			{
				Source = ImageSource.FromStream(() => stream)
			};

			layout.Add(image);

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);
				var rootView = handler.ToPlatform();

				await rootView.AttachAndRun(async () =>
				{
					await image.WaitUntilLoaded();
					await rootView.AssertContainsColor(Colors.Red, MauiContext);
				});
			});
		}

		[Fact]
		[Description("The Opacity property of a image should match with native Opacity")]
		public async Task VerifyImageOpacityProperty()
		{
			var image = new Image
			{
				Opacity = 0.35f
			};
			var expectedValue = image.Opacity;

			var handler = await CreateHandlerAsync<ImageHandler>(image);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}
		ImageView GetPlatformImage(ImageHandler imageHandler) =>
			imageHandler.PlatformView;

		Task<float> GetPlatformOpacity(ImageHandler imageHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformImage(imageHandler);
				return (float)nativeView.Alpha;
			});
		}

		[Fact]
		[Description("The ScaleX property of a Image should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var image = new Image() { ScaleX = 0.45f };
			var expected = image.ScaleX;
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a Image should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var image = new Image() { ScaleY = 1.23f };
			var expected = image.ScaleY;
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a Image should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var image = new Image() { Scale = 2.0f };
			var expected = image.Scale;
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a Image should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var image = new Image() { RotationX = 33.0 };
			var expected = image.RotationX;
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			var platformRotationX = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a Image should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var image = new Image() { RotationY = 87.0 };
			var expected = image.RotationY;
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			var platformRotationY = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a Image should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var image = new Image() { Rotation = 23.0 };
			var expected = image.Rotation;
			var handler = await CreateHandlerAsync<ImageHandler>(image);
			var platformRotation = await InvokeOnMainThreadAsync(() => handler.PlatformView.Rotation);
			Assert.Equal(expected, platformRotation);
		}
	}

	// This subclass of memory stream is deliberately set up to trick Glide into using the cached image
	// after the first image is loaded. See https://github.com/dotnet/maui/issues/8676#issuecomment-1416183584 and
	// https://github.com/dotnet/maui/pull/13111#issuecomment-1416214847 for context.
	class TestMemoryStream : MemoryStream
	{
		public TestMemoryStream(byte[] data) : base(data) { }

		public override string ToString()
		{
			return "TestMemoryStream";
		}

		public override int GetHashCode()
		{
			return 42;
		}

		public override bool Equals(object obj)
		{
			return true;
		}
	}
}
