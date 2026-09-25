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
			using var imageView = new RequestTrackingImageView(MauiProgram.DefaultContext);

			await InvokeOnMainThreadAsync(() => imageView.AttachAndRun(async () =>
			{
				var requestManager = Glide.With(imageView);
				var requestManagerStopped = false;

				try
				{
					var loadTask = service.LoadDrawableAsync(imageSource, imageView);

					await trackingStream.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
					var submission = await imageView.RequestSubmitted.Task.WaitAsync(TimeSpan.FromSeconds(5));

					Assert.True(submission.IsRunning);

					requestManager.OnStop();
					requestManagerStopped = true;
					Assert.False(submission.IsRunning);

					trackingStream.AllowRead();
					requestManager.OnStart();
					requestManagerStopped = false;

					using var result = await loadTask.WaitAsync(TimeSpan.FromSeconds(5));
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
			public RequestTrackingImageView(global::Android.Content.Context context)
				: base(context)
			{
			}

			public TaskCompletionSource<IRequest> RequestSubmitted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public override void SetTag(int key, Java.Lang.Object tag)
			{
				base.SetTag(key, tag);

				if (tag is IRequest request)
					RequestSubmitted.TrySetResult(request);
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

		}
	}
}
