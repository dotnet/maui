using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public class SwipeItemMenuItemHandlerTests : CoreHandlerTestBase
	{
		[Fact]
		public async Task IconTintCanBeClearedWithoutMutatingSharedDrawable()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var tintedItem = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Blue,
					Source = new FileImageSourceStub("red.png")
				};
				var tintedHandler = CreateHandler<SwipeItemMenuItemHandler>(tintedItem);
				var tintedButton = Assert.IsAssignableFrom<TextView>(tintedHandler.PlatformView);

				await AssertEventually(() => GetTopDrawable(tintedButton)?.ColorFilter is not null);

				var untintedItem = new SwipeItemMenuItemStub
				{
					Source = new FileImageSourceStub("red.png")
				};
				var untintedHandler = CreateHandler<SwipeItemMenuItemHandler>(untintedItem);
				var untintedButton = Assert.IsAssignableFrom<TextView>(untintedHandler.PlatformView);

				await AssertEventually(() => GetTopDrawable(untintedButton) is not null);
				Assert.Null(GetTopDrawable(untintedButton).ColorFilter);

				tintedItem.IconColor = null;
				tintedHandler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				await AssertEventually(() => GetTopDrawable(tintedButton) is not null &&
					GetTopDrawable(tintedButton).ColorFilter is null);
			});
		}

		[Fact]
		public async Task IconTintCanBeClearedWhenDrawableDoesNotReportColorFilter()
		{
			var imageService = new NonReportingFileImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Blue
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				item.Source = new FileImageSourceStub("custom.png");

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				var drawable = GetTopDrawable(Assert.IsAssignableFrom<TextView>(handler.PlatformView));
				Assert.Same(imageService.Drawable, drawable);
				Assert.Null(drawable.ColorFilter);
				Assert.NotNull(imageService.Drawable.AppliedColorFilter);

				item.IconColor = null;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				Assert.Null(imageService.Drawable.AppliedColorFilter);
			});
		}

		[Fact]
		public async Task IconColorLoadsSourceWhenPlatformImageIsMissing()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				var button = Assert.IsAssignableFrom<TextView>(handler.PlatformView);

				item.Source = new FileImageSourceStub("red.png");
				item.IconColor = Colors.Red;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				await AssertEventually(() => GetTopDrawable(button)?.ColorFilter is not null);
			});
		}

		[Fact]
		public async Task IconColorChangeMutatesAttachedDrawableWithoutReloading()
		{
			var imageService = new MutatingFileImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				item.Source = new FileImageSourceStub("custom.png");

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				var drawable = GetTopDrawable(Assert.IsAssignableFrom<TextView>(handler.PlatformView));
				Assert.Same(imageService.SourceDrawable, drawable);
				Assert.Equal(0, imageService.SourceDrawable.MutateCount);
				Assert.Equal(1, imageService.LoadCount);

				item.IconColor = Colors.Red;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				drawable = GetTopDrawable(Assert.IsAssignableFrom<TextView>(handler.PlatformView));
				Assert.Same(imageService.MutatedDrawable, drawable);
				Assert.True(imageService.MutatedDrawable.BoundsSet);
				Assert.Equal(1, imageService.SourceDrawable.MutateCount);
				Assert.Equal(1, imageService.LoadCount);
			});
		}

		static global::Android.Graphics.Drawables.Drawable GetTopDrawable(TextView textView)
		{
			var drawables = textView.GetCompoundDrawables();
			return drawables.Length > 1 ? drawables[1] : null;
		}

		sealed class MutatingFileImageSourceService : IImageSourceService<IFileImageSource>
		{
			readonly MutatingDrawable _sourceDrawable = new();

			public MutatingDrawable SourceDrawable => _sourceDrawable;

			public TrackingDrawable MutatedDrawable => _sourceDrawable.MutatedDrawable;

			public int LoadCount { get; private set; }

			public Task<IImageSourceServiceResult> LoadDrawableAsync(
				IImageSource imageSource,
				global::Android.Widget.ImageView imageView,
				CancellationToken cancellationToken = default)
			{
				LoadCount++;
				imageView.SetImageDrawable(_sourceDrawable);
				return Task.FromResult<IImageSourceServiceResult>(
					new ImageSourceServiceResult(_sourceDrawable));
			}

			public Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(
				IImageSource imageSource,
				global::Android.Content.Context context,
				CancellationToken cancellationToken = default)
			{
				LoadCount++;
				return Task.FromResult<IImageSourceServiceResult<Drawable>>(
					new ImageSourceServiceResult(_sourceDrawable));
			}
		}

		sealed class NonReportingFileImageSourceService : IImageSourceService<IFileImageSource>
		{
			public NonReportingColorFilterDrawable Drawable { get; } = new();

			public Task<IImageSourceServiceResult> LoadDrawableAsync(
				IImageSource imageSource,
				global::Android.Widget.ImageView imageView,
				CancellationToken cancellationToken = default)
			{
				imageView.SetImageDrawable(Drawable);
				return Task.FromResult<IImageSourceServiceResult>(
					new ImageSourceServiceResult(Drawable));
			}

			public Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(
				IImageSource imageSource,
				global::Android.Content.Context context,
				CancellationToken cancellationToken = default)
			{
				return Task.FromResult<IImageSourceServiceResult<Drawable>>(
					new ImageSourceServiceResult(Drawable));
			}
		}

		sealed class NonReportingColorFilterDrawable : Drawable
		{
			public ColorFilter? AppliedColorFilter { get; private set; }

			public override int Opacity => (int)Format.Translucent;

			public override void Draw(Canvas canvas)
			{
			}

			public override void SetAlpha(int alpha)
			{
			}

			public override void SetColorFilter(ColorFilter? colorFilter)
			{
				AppliedColorFilter = colorFilter;
			}
		}

		sealed class MutatingDrawable : ColorDrawable
		{
			public TrackingDrawable MutatedDrawable { get; } = new();

			public int MutateCount { get; private set; }

			public override Drawable Mutate()
			{
				MutateCount++;
				return MutatedDrawable;
			}
		}

		sealed class TrackingDrawable : ColorDrawable
		{
			public bool BoundsSet { get; private set; }

			public override void SetBounds(int left, int top, int right, int bottom)
			{
				BoundsSet = true;
				base.SetBounds(left, top, right, bottom);
			}
		}
	}
}
