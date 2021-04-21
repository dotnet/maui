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

			var order = new List<string>();

			image.LoadingStarted += () => order.Add($"LoadingStarted");
			image.LoadingCompleted += successful => order.Add($"LoadingCompleted({successful})");
			image.LoadingFailed += exception => order.Add($"LoadingFailed({exception})");

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);

				await image.Wait();

				var expectedColor = Color.FromHex(colorHex);

				await handler.NativeView.AssertContainsColor(expectedColor);
			});

			Assert.Equal(new[] { "LoadingStarted", "LoadingCompleted(True)" }, order);
		}

		[Fact]
		public async Task InvalidSourceFailsToLoad()
		{
			var image = new ImageStub
			{
				BackgroundColor = Colors.Black,
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

				await handler.NativeView.AssertContainsColor(Colors.Black);
			});

			Assert.Equal(new[] { "LoadingStarted", "LoadingFailed" }, order);
			Assert.NotNull(exception);
		}

		[Fact]
		public async Task<List<(string Member, object Value)>> CountedImageSourceServiceStubWorks()
		{
			var image = new ImageStub
			{
				BackgroundColor = Colors.Black,
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
				BackgroundColor = Colors.Black,
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

				// make sure it did actually work
				await handler.NativeView.AssertContainsColor(Colors.Red);

				return handler.ImageEvents;
			});

			Assert.Equal(new[] { "LoadingStarted", "LoadingStarted", "LoadingCompleted(True)", "LoadingCompleted(False)" }, order);

			return events;
		}
	}
}