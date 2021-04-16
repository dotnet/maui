using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AImageView = Android.Widget.ImageView;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests
	{
		private const int ColorPrecision = 1;

		[Theory]
		[InlineData("red.png", "#FF0000")]
		[InlineData("green.png", "#00FF00")]
		[InlineData("black.png", "#000000")]
		public async Task SourceInitializesCorrectly(string filename, string colorHex)
		{
			var image = new ImageStub
			{
				BackgroundColor = Colors.Black,
				Source = new FileImageSourceStub(filename),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);

				await image.Wait();

				var expectedColor = Color.FromHex(colorHex);

				await handler.NativeView.AssertContainsColor(expectedColor, ColorPrecision);
			});
		}

		[Fact]
		public async Task InitializingNullSourceOnlyUpdatesTransparent()
		{
			var image = new ImageStub
			{
				BackgroundColor = Colors.Black,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				Assert.Single(handler.ImageEvents);
				Assert.Equal("SetImageResource", handler.ImageEvents[0].Member);
				Assert.Equal(Android.Resource.Color.Transparent, handler.ImageEvents[0].Value);

				await handler.NativeView.AssertContainsColor(Colors.Black);
			});
		}

		[Fact]
		public async Task InitializingSourceOnlyUpdatesDrawableOnce()
		{
			var image = new ImageStub
			{
				BackgroundColor = Colors.Black,
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

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
				BackgroundColor = Colors.Black,
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				await handler.NativeView.AssertContainsColor(Colors.Red, ColorPrecision);

				handler.ImageEvents.Clear();

				image.Source = new FileImageSourceStub("blue.png");
				handler.UpdateValue(nameof(IImage.Source));

				await image.Wait();

				await handler.NativeView.AssertContainsColor(Colors.Blue, ColorPrecision);

				Assert.Equal(2, handler.ImageEvents.Count);
				Assert.Equal("SetImageResource", handler.ImageEvents[0].Member);
				Assert.Equal(Android.Resource.Color.Transparent, handler.ImageEvents[0].Value);
				Assert.Equal("SetImageDrawable", handler.ImageEvents[1].Member);
				Assert.IsType<BitmapDrawable>(handler.ImageEvents[1].Value);
			});
		}

		class CountedImageHandler : ImageHandler
		{
			protected override AImageView CreateNativeView() => new CountedImageView(Context);

			public List<(string Member, object Value)> ImageEvents => ((CountedImageView)NativeView).ImageEvents;
		}

		class CountedImageView : AImageView
		{
			public CountedImageView(Context context)
				: base(context)
			{
			}

			public List<(string, object)> ImageEvents { get; } = new List<(string, object)>();

			public override void SetImageBitmap(Bitmap bm)
			{
				base.SetImageBitmap(bm);
				Log(bm);
			}

			public override void SetImageDrawable(Drawable drawable)
			{
				base.SetImageDrawable(drawable);
				Log(drawable);
			}

			public override void SetImageIcon(Icon icon)
			{
				base.SetImageIcon(icon);
				Log(icon);
			}

			public override void SetImageResource(int resId)
			{
				base.SetImageResource(resId);
				Log(resId);
			}

			public override void SetImageURI(Uri uri)
			{
				base.SetImageURI(uri);
				Log(uri);
			}

			private void Log(object value, [CallerMemberName] string member = null)
			{
				ImageEvents.Add((member, value));
			}
		}
	}
}