using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

#if ANDROID
using Android.Graphics.Drawables;
using PlatformImageType = System.Int32;
#elif IOS || MACCATALYST
using UIKit;
using PlatformImageType = UIKit.UIImage;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Image)]
	public partial class ImageHandlerTests : ImageHandlerTests<ImageHandler, ImageStub>
	{
	}

	public abstract partial class ImageHandlerTests<TImageHandler, TStub> : CoreHandlerTestBase<TImageHandler, TStub>
		where TImageHandler : class, IImageHandler, new()
		where TStub : StubBase, IImageStub, new()
	{
#if ANDROID
		const string ImageEventAppResourceMemberName = "SetImageResource";
		const string ImageEventCustomMemberName = "SetImageDrawable";
#elif IOS || MACCATALYST
		const string ImageEventAppResourceMemberName = "Image";
		const string ImageEventCustomMemberName = "Image";
#endif

		[Theory(
#if IOS || MACCATALYST
			Skip = "Test failing on iOS"
#endif
			)]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task UpdatingSourceUpdatesImageCorrectly(string colorHex)
		{
			// create files
			var expectedColor = Color.FromArgb(colorHex);
			var firstPath = BaseImageSourceServiceTests.CreateBitmapFile(100, 100, Colors.Blue);
			var secondPath = BaseImageSourceServiceTests.CreateBitmapFile(100, 100, expectedColor);

			await InvokeOnMainThreadAsync(async () =>
			{
				var image = new TStub { Width = 100, Height = 100 };
				var handler = CreateHandler(image);
				var platformView = GetPlatformImageView(handler);

				await platformView.AttachAndRun(async () =>
				{
					// the first one works
					image.Source = new FileImageSourceStub(firstPath);
					handler.UpdateValue(nameof(IImage.Source));
					await image.Wait();

					await platformView.AssertContainsColor(Colors.Blue.ToPlatform(), MauiContext);

					// the second one does not
					image.Source = new FileImageSourceStub(secondPath);
					handler.UpdateValue(nameof(IImage.Source));
					await image.Wait();

					await platformView.AssertContainsColor(expectedColor.ToPlatform(), MauiContext);
				});
			});
		}

		[Theory(
#if _ANDROID__
			Skip = "Test failing on ANDROID"
#endif
			)]
		[InlineData("red.png", "#FF0000")]
		[InlineData("green.png", "#00FF00")]
		[InlineData("black.png", "#000000")]
		public async Task SourceInitializesCorrectly(string filename, string colorHex)
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub(filename),
			};

			var order = new List<string>();

			image.LoadingStarted += () => order.Add($"LoadingStarted");
			image.LoadingCompleted += successful => order.Add($"LoadingCompleted({successful})");
			image.LoadingFailed += exception => order.Add($"LoadingFailed({exception})");

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);

				await image.Wait();

				var expectedColor = Color.FromArgb(colorHex);

				await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
			});

			Assert.Equal(new[] { "LoadingStarted", "LoadingCompleted(True)" }, order);
		}

		[Theory(
#if __IOS__ || __ANDROID__
			Skip = "Animated GIFs are not yet supported on iOS. Test failing on ANDROID"
#endif
		)]
		[InlineData("animated_heart.gif", true)]
		[InlineData("animated_heart.gif", false)]
		public async virtual Task AnimatedSourceInitializesCorrectly(string filename, bool isAnimating)
		{
			var image = new TStub
			{
				Source = new FileImageSourceStub(filename),
				IsAnimationPlaying = isAnimating,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);

				await image.Wait();

				await GetPlatformImageView(handler).AttachAndRun(() =>
				{
					Assert.Equal(isAnimating, GetNativeIsAnimationPlaying(handler));
				});
			});
		}

		[Theory]
		[InlineData(Aspect.AspectFill)]
		[InlineData(Aspect.AspectFit)]
		[InlineData(Aspect.Center)]
		[InlineData(Aspect.Fill)]
		public async Task AspectInitializesCorrectly(Aspect aspect)
		{
			var image = new TStub()
			{
				Aspect = aspect
			};

			await ValidatePropertyInitValue(image, () => image.Aspect, (h) => GetNativeAspect(h), aspect);
		}

		[Theory(Skip = "See: https://github.com/dotnet/maui/issues/6415")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task InvalidSourceFailsToLoad(string colorHex)
		{
			var color = Color.FromArgb(colorHex);

			var image = new TStub
			{
				Background = new SolidPaintStub(color),
				Source = new FileImageSourceStub("bad path"),
			};

			var order = new List<string>();
			Exception exception = null;

			image.LoadingStarted += () => order.Add($"LoadingStarted");
			image.LoadingCompleted += successful => order.Add($"LoadingCompleted({successful})");
			image.LoadingFailed += ex =>
			{
				order.Add($"LoadingFailed");
				exception = ex;
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = (IPlatformViewHandler)CreateHandler(image);

				await image.Wait(timeout: 5000);

#if __ANDROID__
				handler.PlatformView.SetMinimumHeight(1);
				handler.PlatformView.SetMinimumWidth(1);
#endif

				await handler.PlatformView.AssertContainsColor(color, MauiContext);
			});

			Assert.Equal(new List<string> { "LoadingStarted", "LoadingFailed" }, order);
			Assert.NotNull(exception);
		}

		[Fact]
		public async Task ImageLoadSequenceIsCorrect()
		{
			await ImageLoadSequenceIsCorrectImplementation();
		}

		async Task<List<(string Member, object Value)>> ImageLoadSequenceIsCorrectImplementation()
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black)
			};

			var order = new ConcurrentQueue<string>();

			return await InvokeOnMainThreadAsync(async () =>
			{
				// get the handler and reset things we don't care about
				var handler = CreateHandler<CountedImageHandler>(image);
				await image.Wait();
				handler.ImageEvents.Clear();

				// get the service we are going to test with
				var provider = handler.Services.GetRequiredService<IImageSourceServiceProvider>();
				var imageService = provider.GetRequiredImageSourceService<CountedImageSourceStub>();
				var countedService = Assert.IsType<CountedImageSourceServiceStub>(imageService);

				// hook up the listeners
				order.Enqueue("Before Starting");
				var startingTask = Task.Run(async () =>
				{
					countedService.Starting.WaitOne();
					order.Enqueue("Starting");

					await Task.Delay(1);

					order.Enqueue("DoWork");
					countedService.DoWork.Set();
				});
				var finishingTask = Task.Run(() =>
				{
					countedService.Finishing.WaitOne();
					order.Enqueue("Finishing");
				});

				// set & apply the image source
				image.Source = new CountedImageSourceStub(Colors.Blue, true);
				handler.UpdateValue(nameof(IImage.Source));

				// wait until everything is complete
				await Task.WhenAll(image.Wait(), startingTask, finishingTask);

				// verify that it all happened in the order as expected
				order.Enqueue("After Finishing");
				Assert.Equal(new[] { "Before Starting", "Starting", "DoWork", "Finishing", "After Finishing" }, order.ToArray());

				// make sure it did actually work
				await handler.PlatformView.AssertContainsColor(Colors.Blue, MauiContext);

				return handler.ImageEvents;
			});
		}

		[Fact]
		public async Task InterruptingLoadCancelsAndStartsOver()
		{
			await InterruptingLoadCancelsAndStartsOverImplementation();
		}

		async Task<List<(string Member, object Value)>> InterruptingLoadCancelsAndStartsOverImplementation()
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black)
			};

			var order = new List<string>();

			image.LoadingStarted += () => order.Add($"LoadingStarted");
			image.LoadingCompleted += successful => order.Add($"LoadingCompleted({successful})");
			image.LoadingFailed += exception => order.Add($"LoadingFailed({exception})");

			var events = await InvokeOnMainThreadAsync(async () =>
			{
				// get the handler and reset things we don't care about
				var handler = CreateHandler<CountedImageHandler>(image);
				await image.Wait();
				handler.ImageEvents.Clear();

				// get the service we are going to test with
				var provider = handler.Services.GetRequiredService<IImageSourceServiceProvider>();
				var imageService = provider.GetRequiredImageSourceService<CountedImageSourceStub>();
				var countedService = Assert.IsType<CountedImageSourceServiceStub>(imageService);

				// hook up the listeners
				var startingTask = Task.Run(async () =>
				{
					countedService.Starting.WaitOne();

					// set & apply the SECOND image source
					await InvokeOnMainThreadAsync(async () =>
					{
						image.Source = new CountedImageSourceStub(Colors.Red);
						handler.UpdateValue(nameof(IImage.Source));
						await image.Wait();
					});

					// let the FIRST one continue
					countedService.DoWork.Set();
				});

				// set & apply the FIRST image source
				image.Source = new CountedImageSourceStub(Colors.Blue, true);
				handler.UpdateValue(nameof(IImage.Source));
				await image.Wait();
				await startingTask;
				await image.Wait();

				// make sure it did actually work
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);

				return handler.ImageEvents;
			});

			Assert.Equal(new[] { "LoadingStarted", "LoadingStarted", "LoadingCompleted(True)", "LoadingCompleted(False)" }, order);

			return events;
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task InitializingNullSourceOnlyUpdatesNull(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var image = new TStub
			{
				Background = new SolidPaintStub(expectedColor),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				// We expect that if the Image is created with no Source set, the platform image view
				// will get a `null` image set
				Assert.NotEmpty(handler.ImageEvents);
				Assert.Null(handler.ImageEvents[0].Value);

				await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		[Fact]
		public async Task InitializingSourceOnlyUpdatesImageOnce()
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);

				Assert.Single(handler.ImageEvents);
				Assert.Equal(ImageEventAppResourceMemberName, handler.ImageEvents[0].Member);
				var platformImage = Assert.IsType<PlatformImageType>(handler.ImageEvents[0].Value);

#if ANDROID
				Assert.Equal(GetDrawableId("red"), platformImage);
#elif IOS || MACCATALYST
				platformImage.AssertContainsColor(Colors.Red.ToPlatform());
#endif
			});
		}

		[Fact]
		public async Task UpdatingSourceOnlyUpdatesImageOnce()
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);

				handler.ImageEvents.Clear();

				image.Source = new FileImageSourceStub("blue.png");
				handler.UpdateValue(nameof(IImage.Source));

				await image.Wait();

				await handler.PlatformView.AssertContainsColor(Colors.Blue, MauiContext);

				Assert.Single(handler.ImageEvents);
				Assert.Equal(ImageEventAppResourceMemberName, handler.ImageEvents[0].Member);
				var platformImage = Assert.IsType<PlatformImageType>(handler.ImageEvents[0].Value);

#if ANDROID
				Assert.Equal(GetDrawableId("blue"), platformImage);
#elif IOS || MACCATALYST
				platformImage.AssertContainsColor(Colors.Blue.ToPlatform());
#endif
			});
		}

		[Fact]
		public async Task ImageLoadSequenceIsCorrectWithChecks()
		{
			var events = await ImageLoadSequenceIsCorrectImplementation();

			Assert.Single(events);
			Assert.Equal(ImageEventCustomMemberName, events[0].Member);

#if ANDROID
			var platformImage = Assert.IsType<ColorDrawable>(events[0].Value);
			platformImage.Color.IsEquivalent(Colors.Blue.ToPlatform());
#elif IOS || MACCATALYST
			var platformImage = Assert.IsType<UIImage>(events[0].Value);
			platformImage.AssertContainsColor(Colors.Blue.ToPlatform());
#endif
		}

		[Fact]
		public async Task InterruptingLoadCancelsAndStartsOverWithChecks()
		{
			var events = await InterruptingLoadCancelsAndStartsOverImplementation();

			Assert.Single(events);
			Assert.Equal(ImageEventCustomMemberName, events[0].Member);

#if ANDROID
			var platformImage = Assert.IsType<ColorDrawable>(events[0].Value);
			platformImage.Color.IsEquivalent(Colors.Red.ToPlatform());
#elif IOS || MACCATALYST
			var platformImage = Assert.IsType<UIImage>(events[0].Value);
			platformImage.AssertContainsColor(Colors.Red.ToPlatform());
#endif
		}

		protected TCustomHandler CreateHandler<TCustomHandler>(IView view)
			where TCustomHandler : IImageHandler, new()
		{
			var handler = new TCustomHandler();
			InitializeViewHandler(view, handler);
			handler.SetMauiContext(MauiContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			view.Arrange(new Rect(0, 0, view.Width, view.Height));
			handler.PlatformArrange(view.Frame);

			return handler;
		}

#if ANDROID
		static int GetDrawableId(string image) =>
			MauiProgram.DefaultContext.Resources.GetDrawableId(MauiProgram.DefaultContext.PackageName, image);
#endif

		[Fact]
		public async Task UpdatingSourceToNullClearsImage()
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);

				await image.Wait();

				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);

				handler.ImageEvents.Clear();

				image.Source = null;
				handler.UpdateValue(nameof(IImage.Source));

				await image.Wait();

				await handler.PlatformView.AssertDoesNotContainColor(Colors.Red, MauiContext);
			});
		}

		[Fact]
		public async Task UpdatingSourceToNonexistentSourceClearsImage()
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<ImageHandler>(image);
				await image.Wait();
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);

				image.Source = new FileImageSourceStub("fail.png");
				handler.UpdateValue(nameof(IImage.Source));
				await handler.PlatformView.AttachAndRun(() => { });

				await image.Wait(5000);
				await handler.PlatformView.AssertDoesNotContainColor(Colors.Red, MauiContext);
			});
		}
	}
}