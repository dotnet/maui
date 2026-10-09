using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
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
			using var trackingStream = new TrackingStream(bitmapStream.ToArray());
			var imageSource = new StreamImageSourceStub(trackingStream);
			var service = new StreamImageSourceService();
			using var imageView = new RequestTrackingImageView(MauiProgram.DefaultContext, trackingStream);

			await InvokeOnMainThreadAsync(() => imageView.AttachAndRun(async () =>
			{
				var requestManager = Glide.With(imageView);
				var requestManagerStopped = false;

				try
				{
					var loadTask = service.LoadDrawableAsync(imageSource, imageView);

					await WaitForStage(trackingStream.ReadStarted.Task, "the source stream read to start");

					requestManager.OnStop();
					requestManagerStopped = true;

					trackingStream.AllowRead();

					var submission = await WaitForStage(
						imageView.RequestSubmitted.Task,
						"Glide to submit the buffered image request");

					Assert.True(submission.SourceDisposed);
					Assert.False(submission.Request.IsRunning);

					requestManager.OnStart();
					requestManagerStopped = false;

					using var result = await WaitForStage(
						loadTask,
						"the buffered image request to complete after Glide restarts");
					Assert.NotNull(result);

					var bitmapDrawable = Assert.IsType<BitmapDrawable>(imageView.Drawable);
					await bitmapDrawable.Bitmap.AssertContainsColor(expectedColor);
				}
				finally
				{
					trackingStream.AllowRead();

					if (requestManagerStopped)
						requestManager.OnStart();

					requestManager.Clear(imageView);
				}
			}));
		}

		sealed class RequestTrackingImageView : ImageView
		{
			readonly TrackingStream _sourceStream;

			public RequestTrackingImageView(global::Android.Content.Context context, TrackingStream sourceStream)
				: base(context)
			{
				_sourceStream = sourceStream;
			}

			public TaskCompletionSource<(IRequest Request, bool SourceDisposed)> RequestSubmitted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public override void SetTag(int key, Java.Lang.Object tag)
			{
				base.SetTag(key, tag);

				if (tag is IRequest request)
					RequestSubmitted.TrySetResult((request, _sourceStream.IsDisposed));
			}
		}

		sealed class TrackingStream : MemoryStream
		{
			public TrackingStream(byte[] buffer)
				: base(buffer)
			{
			}

			public TaskCompletionSource<bool> ReadStarted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			readonly TaskCompletionSource<bool> _allowRead =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public bool IsDisposed { get; private set; }

			public void AllowRead() => _allowRead.TrySetResult(true);

			public override int Read(byte[] buffer, int offset, int count)
			{
				ReadStarted.TrySetResult(true);
				_allowRead.Task.GetAwaiter().GetResult();
				return base.Read(buffer, offset, count);
			}

			public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
			{
				ReadStarted.TrySetResult(true);
				await _allowRead.Task.WaitAsync(cancellationToken);
				return await base.ReadAsync(buffer, cancellationToken);
			}

			public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				ReadStarted.TrySetResult(true);
				await _allowRead.Task.WaitAsync(cancellationToken);
				return await base.ReadAsync(buffer, offset, count, cancellationToken);
			}

			protected override void Dispose(bool disposing)
			{
				IsDisposed = true;
				base.Dispose(disposing);
			}
		}

		static async Task WaitForStage(Task task, string stage)
		{
			try
			{
				await task.WaitAsync(TimeSpan.FromSeconds(30));
			}
			catch (TimeoutException exception)
			{
				throw new TimeoutException($"Timed out waiting for {stage}.", exception);
			}
		}

		static async Task<T> WaitForStage<T>(Task<T> task, string stage)
		{
			try
			{
				return await task.WaitAsync(TimeSpan.FromSeconds(30));
			}
			catch (TimeoutException exception)
			{
				throw new TimeoutException($"Timed out waiting for {stage}.", exception);
			}
		}
	}
}
