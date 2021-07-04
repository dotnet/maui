using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Image)]
	public partial class ImageHandlerTests : HandlerTestBase<ImageHandler, ImageStub>
	{
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
			var image = new ImageStub
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

				await handler.NativeView.AssertContainsColor(expectedColor);
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
		public async Task AnimatedSourceInitializesCorrectly(string filename, bool isAnimating)
		{
			var image = new ImageStub
			{
				Source = new FileImageSourceStub(filename),
				IsAnimationPlaying = isAnimating,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);

				await image.Wait();

				await GetNativeImageView(handler).AttachAndRun(() =>
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
			var image = new ImageStub()
			{
				Aspect = aspect
			};

			await ValidatePropertyInitValue(image, () => image.Aspect, GetNativeAspect, aspect);
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task InvalidSourceFailsToLoad(string colorHex)
		{
			var color = Color.FromArgb(colorHex);

			var image = new ImageStub
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
				var handler = CreateHandler(image);

				await image.Wait();

#if __ANDROID__
				handler.NativeView.SetMinimumHeight(1);
				handler.NativeView.SetMinimumWidth(1);
#endif

				await handler.NativeView.AssertContainsColor(color);
			});

			Assert.Equal(new[] { "LoadingStarted", "LoadingFailed" }, order);
			Assert.NotNull(exception);
		}

		[Fact]
		public async Task<List<(string Member, object Value)>> ImageLoadSequenceIsCorrect()
		{
			var image = new ImageStub
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
				await handler.NativeView.AssertContainsColor(Colors.Blue);

				return handler.ImageEvents;
			});
		}

		[Fact]
		public async Task<List<(string Member, object Value)>> InterruptingLoadCancelsAndStartsOver()
		{
			var image = new ImageStub
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
				await handler.NativeView.AssertContainsColor(Colors.Red);

				return handler.ImageEvents;
			});

			Assert.Equal(new[] { "LoadingStarted", "LoadingStarted", "LoadingCompleted(True)", "LoadingCompleted(False)" }, order);

			return events;
		}
	}
}