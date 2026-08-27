using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Widget;
using Bumptech.Glide;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StreamImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new StreamImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetDrawableAsync(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex).ToPlatform();

			var service = new StreamImageSourceService();

			var stream = CreateBitmapStream(100, 100, expectedColor);

			var imageSource = new StreamImageSourceStub(stream);

			using var result = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(result.Value);

			var bitmap = bitmapDrawable.Bitmap;

			await bitmap.AssertContainsColor(expectedColor).ConfigureAwait(false);
		}

		[Fact]
		public async Task LoadDrawableAsyncSurvivesGlideRestart()
		{
			var expectedColor = Colors.Red.ToPlatform();
			using var bitmapStream = Assert.IsType<MemoryStream>(CreateBitmapStream(100, 100, expectedColor));
			using var blockingStream = new BlockingReadStream(bitmapStream.ToArray());
			var imageSource = new StreamImageSourceStub(blockingStream);
			var service = new StreamImageSourceService();
			using var imageView = new ImageView(MauiProgram.DefaultContext);
			var unhandledException = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);

			void OnUnhandledException(object sender, RaiseThrowableEventArgs args)
			{
				if (args.Exception?.ToString().Contains(nameof(BlockingReadStream), StringComparison.Ordinal) == true)
				{
					unhandledException.TrySetResult(args.Exception);
					args.Handled = true;
				}
			}

			AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;

			try
			{
				await InvokeOnMainThreadAsync(() => imageView.AttachAndRun(async () =>
				{
					var requestManager = Glide.With(imageView);
					var loadTask = service.LoadDrawableAsync(imageSource, imageView);

					await blockingStream.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
					requestManager.OnStop();

					blockingStream.ReleaseRead();
					await blockingStream.ReadCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5));

					requestManager.OnStart();
					var completedTask = await Task.WhenAny(loadTask, unhandledException.Task).WaitAsync(TimeSpan.FromSeconds(5));

					if (completedTask == unhandledException.Task)
						Assert.Null(await unhandledException.Task);

					using var result = await loadTask;
					Assert.NotNull(result);

					var bitmapDrawable = Assert.IsType<BitmapDrawable>(imageView.Drawable);
					await bitmapDrawable.Bitmap.AssertContainsColor(expectedColor);
				}));
			}
			finally
			{
				blockingStream.ReleaseRead();
				AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledException;
			}
		}

		sealed class BlockingReadStream : MemoryStream
		{
			readonly TaskCompletionSource _releaseRead = new(TaskCreationOptions.RunContinuationsAsynchronously);
			int _blockNextRead = 1;

			public BlockingReadStream(byte[] buffer)
				: base(buffer)
			{
			}

			public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
			public TaskCompletionSource ReadCompleted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

			public void ReleaseRead() => _releaseRead.TrySetResult();

			public override int Read(byte[] buffer, int offset, int count)
			{
				BlockRead();

				try
				{
					return base.Read(buffer, offset, count);
				}
				finally
				{
					ReadCompleted.TrySetResult();
				}
			}

			public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
			{
				await BlockReadAsync(cancellationToken);

				try
				{
					await base.CopyToAsync(destination, bufferSize, cancellationToken);
				}
				finally
				{
					ReadCompleted.TrySetResult();
				}
			}

			void BlockRead()
			{
				if (Interlocked.Exchange(ref _blockNextRead, 0) == 0)
					return;

				ReadStarted.TrySetResult();
				_releaseRead.Task.GetAwaiter().GetResult();
			}

			async Task BlockReadAsync(CancellationToken cancellationToken)
			{
				if (Interlocked.Exchange(ref _blockNextRead, 0) == 0)
					return;

				ReadStarted.TrySetResult();
				await _releaseRead.Task.WaitAsync(cancellationToken);
			}
		}
	}
}
