using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests
	{
		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task InitializingNullSourceOnlyUpdatesTransparent(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var image = new ImageStub
			{
				Background = new SolidPaintStub(expectedColor)
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				Assert.Single(handler.ImageEvents);
				Assert.Equal("SetImageResource", handler.ImageEvents[0].Member);
				Assert.Equal(Android.Resource.Color.Transparent, handler.ImageEvents[0].Value);

				await handler.NativeView.AssertContainsColor(expectedColor);
			});
		}

		[Fact]
		public async Task InitializingSourceOnlyUpdatesDrawableOnce()
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
				Assert.Equal("SetImageResource", handler.ImageEvents[0].Member);
				Assert.Equal(Android.Resource.Color.Transparent, handler.ImageEvents[0].Value);
				Assert.Equal("SetImageDrawable", handler.ImageEvents[1].Member);
				Assert.IsType<BitmapDrawable>(handler.ImageEvents[1].Value);
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
				Assert.Equal("SetImageResource", handler.ImageEvents[0].Member);
				Assert.Equal(Android.Resource.Color.Transparent, handler.ImageEvents[0].Value);
				Assert.Equal("SetImageDrawable", handler.ImageEvents[1].Member);
				Assert.IsType<BitmapDrawable>(handler.ImageEvents[1].Value);
			});
		}

		[Fact]
		public async Task ImageLoadSequenceIsCorrectWithChecks()
		{
			var events = await ImageLoadSequenceIsCorrect();

			Assert.Equal(2, events.Count);
			Assert.Equal("SetImageResource", events[0].Member);
			Assert.Equal(Android.Resource.Color.Transparent, events[0].Value);
			Assert.Equal("SetImageDrawable", events[1].Member);
			var drawable = Assert.IsType<ColorDrawable>(events[1].Value);
			drawable.Color.IsEquivalent(Colors.Blue.ToNative());
		}

		[Fact]
		public async Task InterruptingLoadCancelsAndStartsOverWithChecks()
		{
			var events = await InterruptingLoadCancelsAndStartsOver();

			Assert.Equal(3, events.Count);
			Assert.Equal("SetImageResource", events[0].Member);
			Assert.Equal(Android.Resource.Color.Transparent, events[0].Value);
			Assert.Equal("SetImageResource", events[1].Member);
			Assert.Equal(Android.Resource.Color.Transparent, events[1].Value);
			Assert.Equal("SetImageDrawable", events[2].Member);
			var drawable = Assert.IsType<ColorDrawable>(events[2].Value);
			drawable.Color.IsEquivalent(Colors.Red.ToNative());
		}

		ImageView GetNativeImageView(ImageHandler imageHandler) =>
			imageHandler.NativeView;

		bool GetNativeIsAnimationPlaying(ImageHandler imageHandler) =>
			GetNativeImageView(imageHandler).Drawable is IAnimatable animatable && animatable.IsRunning;

		Aspect GetNativeAspect(ImageHandler imageHandler)
		{
			var scaleType = GetNativeImageView(imageHandler).GetScaleType();
			if (scaleType == ImageView.ScaleType.Center)
				return Aspect.Center;
			if (scaleType == ImageView.ScaleType.CenterCrop)
				return Aspect.AspectFill;
			if (scaleType == ImageView.ScaleType.FitCenter)
				return Aspect.AspectFit;
			if (scaleType == ImageView.ScaleType.FitXy)
				return Aspect.Fill;

			throw new ArgumentOutOfRangeException("Aspect");
		}
	}
}