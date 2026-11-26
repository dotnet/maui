using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Runtime;
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

					var requestSubmittedTask = imageView.RequestSubmitted.Task;
					var completedTask = await WaitForStage(
						Task.WhenAny(requestSubmittedTask, loadTask),
						"Glide to submit the buffered image request");

					if (completedTask == loadTask)
					{
						using var unexpectedResult = await loadTask;
						throw new InvalidOperationException("The image load completed before Glide submitted its request.");
					}

					var submission = await requestSubmittedTask;
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

		[Fact]
		public async Task LoadDrawableAsyncCancelledAfterBufferingDoesNotReplaceImage()
		{
			using var cancellation = new CancellationTokenSource();
			using var redBitmapStream = Assert.IsType<MemoryStream>(CreateBitmapStream(100, 100, Colors.Red.ToPlatform()));
			using var redStream = new TrackingStream(redBitmapStream.ToArray(), cancellation);
			redStream.AllowRead();
			using var blueStream = CreateBitmapStream(100, 100, Colors.Blue.ToPlatform());
			var service = new StreamImageSourceService();
			using var imageView = new RequestTrackingImageView(MauiProgram.DefaultContext, redStream);

			await InvokeOnMainThreadAsync(() => imageView.AttachAndRun(async () =>
			{
				try
				{
					using var blueResult = await WaitForStage(
						service.LoadDrawableAsync(new StreamImageSourceStub(blueStream), imageView),
						"the initial blue image to load");
					Assert.NotNull(blueResult);
					var blueDrawable = Assert.IsType<BitmapDrawable>(imageView.Drawable);
					await blueDrawable.Bitmap.AssertContainsColor(Colors.Blue.ToPlatform());
					var submissionCount = imageView.RequestSubmissionCount;
					Assert.True(submissionCount > 0);

					// Disposal occurs after buffering succeeds, immediately before submission.
					await Assert.ThrowsAnyAsync<OperationCanceledException>(() => WaitForStage(
						service.LoadDrawableAsync(new StreamImageSourceStub(redStream), imageView, cancellation.Token),
						"the buffered red image load to observe cancellation"));

					Assert.True(redStream.IsDisposed);
					Assert.True(redStream.ReadStarted.Task.IsCompletedSuccessfully);
					Assert.Equal(submissionCount, imageView.RequestSubmissionCount);
					Assert.Same(blueDrawable, imageView.Drawable);
					await blueDrawable.Bitmap.AssertContainsColor(Colors.Blue.ToPlatform());
				}
				finally
				{
					Glide.With(imageView).Clear(imageView);
				}
			}));
		}

		sealed class RequestTrackingImageView : ImageView
		{
			readonly TrackingStream _sourceStream;
			readonly int _requestTagId;

			public RequestTrackingImageView(global::Android.Content.Context context, TrackingStream sourceStream)
				: base(context)
			{
				_sourceStream = sourceStream;
				_requestTagId = context.Resources.GetIdentifier("glide_custom_view_target_tag", "id", context.PackageName);
				Assert.NotEqual(0, _requestTagId);
			}

			public TaskCompletionSource<(IRequest Request, bool SourceDisposed)> RequestSubmitted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public int RequestSubmissionCount { get; private set; }

			public override void SetTag(int key, Java.Lang.Object tag)
			{
				base.SetTag(key, tag);

				if (key == _requestTagId && tag is not null)
				{
					using var javaClass = tag.Class;
					Console.WriteLine($"Glide request tag: Java={javaClass.Name}, managed={tag.GetType().FullName}, sourceDisposed={_sourceStream.IsDisposed}");
					var request = tag.JavaCast<IRequest>();
					Assert.NotNull(request);
					RequestSubmissionCount++;
					RequestSubmitted.TrySetResult((request, _sourceStream.IsDisposed));
				}
			}
		}

		sealed class TrackingStream : MemoryStream
		{
			readonly CancellationTokenSource _cancelOnDispose;

			public TrackingStream(byte[] buffer, CancellationTokenSource cancelOnDispose = null)
				: base(buffer)
			{
				_cancelOnDispose = cancelOnDispose;
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
				if (disposing && !IsDisposed)
					_cancelOnDispose?.Cancel();

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
