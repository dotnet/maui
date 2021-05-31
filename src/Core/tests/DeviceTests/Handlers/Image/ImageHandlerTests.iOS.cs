using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests
	{
		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task InitializingNullSourceOnlyUpdatesNull(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var image = new ImageStub
			{
				Background = new SolidPaintStub(expectedColor),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				Assert.Single(handler.ImageEvents);
				Assert.Equal("Image", handler.ImageEvents[0].Member);
				Assert.Null(handler.ImageEvents[0].Value);

				await handler.NativeView.AssertContainsColor(expectedColor);
			});
		}

		[Fact]
		public async Task InitializingSourceOnlyUpdatesImageOnce()
		{
			var image = new ImageStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				await handler.NativeView.AssertContainsColor(Colors.Red);

				Assert.Equal(2, handler.ImageEvents.Count);
				Assert.Equal("Image", handler.ImageEvents[0].Member);
				Assert.Null(handler.ImageEvents[0].Value);
				Assert.Equal("Image", handler.ImageEvents[1].Member);
				Assert.IsType<UIImage>(handler.ImageEvents[1].Value);
			});
		}

		[Fact]
		public async Task UpdatingSourceOnlyUpdatesDrawableTwice()
		{
			var image = new ImageStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				await handler.NativeView.AssertContainsColor(Colors.Red);

				handler.ImageEvents.Clear();

				image.Source = new FileImageSourceStub("blue.png");
				handler.UpdateValue(nameof(IImage.Source));

				await image.Wait();

				await handler.NativeView.AssertContainsColor(Colors.Blue);

				Assert.Equal(2, handler.ImageEvents.Count);
				Assert.Equal("Image", handler.ImageEvents[0].Member);
				Assert.Null(handler.ImageEvents[0].Value);
				Assert.Equal("Image", handler.ImageEvents[1].Member);
				Assert.IsType<UIImage>(handler.ImageEvents[1].Value);
			});
		}

		[Fact]
		public async Task ImageLoadSequenceIsCorrectWithChecks()
		{
			var events = await ImageLoadSequenceIsCorrect();

			Assert.Equal(2, events.Count);
			Assert.Equal("Image", events[0].Member);
			Assert.Null(events[0].Value);
			Assert.Equal("Image", events[1].Member);
			var image = Assert.IsType<UIImage>(events[1].Value);
			image.AssertContainsColor(Colors.Blue.ToNative());
		}

		[Fact]
		public async Task InterruptingLoadCancelsAndStartsOverWithChecks()
		{
			var events = await InterruptingLoadCancelsAndStartsOver();

			Assert.Equal(3, events.Count);
			Assert.Equal("Image", events[0].Member);
			Assert.Null(events[0].Value);
			Assert.Equal("Image", events[1].Member);
			Assert.Null(events[1].Value);
			Assert.Equal("Image", events[2].Member);
			var image = Assert.IsType<UIImage>(events[2].Value);
			image.AssertContainsColor(Colors.Red.ToNative());
		}

		UIImageView GetNativeImageView(ImageHandler imageHandler) =>
			imageHandler.NativeView;

		bool GetNativeIsAnimationPlaying(ImageHandler imageHandler) =>
			GetNativeImageView(imageHandler).IsAnimating;

		Aspect GetNativeAspect(ImageHandler imageHandler) =>
			GetNativeImageView(imageHandler).ContentMode switch
			{
				UIViewContentMode.ScaleAspectFit => Aspect.AspectFit,
				UIViewContentMode.ScaleAspectFill => Aspect.AspectFill,
				UIViewContentMode.ScaleToFill => Aspect.Fill,
				UIViewContentMode.Center => Aspect.Center,
				_ => throw new ArgumentOutOfRangeException("Aspect")
			};
	}
}