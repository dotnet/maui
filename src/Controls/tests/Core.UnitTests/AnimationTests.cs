using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class AnimationTests : BaseTestFixture
	{
		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=51424
		public async Task AnimationRepeats()
		{
			var box = AnimationReadyHandler.Prepare(new BoxView());
			Assert.Equal(0d, box.Rotation);
			var sb = new Animation();
			var animcount = 0;
			var rot45 = new Animation(d =>
			{
				box.Rotation = d;
				if (d > 44)
					animcount++;
			}, box.Rotation, box.Rotation + 45);
			sb.Add(0, .5, rot45);
			Assert.Equal(0d, box.Rotation);

			var i = 0;
			sb.Commit(box, "foo", length: 100, repeat: () => ++i < 2);

			await Task.Delay(1000);
			Assert.Equal(2, animcount);
		}

		[Fact]
		public async Task FadeToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Opacity = 1;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.FadeToAsync(0, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// Opacity should not have changed
			Assert.Equal(1d, box.Opacity);
		}

		[Fact]
		public async Task RotateToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Rotation = 0;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.RotateToAsync(360, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// Rotation should not have changed
			Assert.Equal(0d, box.Rotation);
		}

		[Fact]
		public async Task TranslateToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.TranslationX = 0;
			box.TranslationY = 0;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.TranslateToAsync(100, 100, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// Translation should not have changed
			Assert.Equal(0d, box.TranslationX);
			Assert.Equal(0d, box.TranslationY);
		}

		[Fact]
		public async Task ScaleToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Scale = 1;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.ScaleToAsync(5, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// Scale should not have changed
			Assert.Equal(1d, box.Scale);
		}

		[Fact]
		public async Task FadeToAsyncWithoutCancellationCompletesNormally()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Opacity = 1;

			// Run animation without cancellation
			var wasCanceled = await box.FadeToAsync(0, 100);

			// Should report not canceled (completed normally)
			Assert.False(wasCanceled);

			// Opacity should have reached the target value
			Assert.Equal(0d, box.Opacity);
		}

		[Fact]
		public async Task RotateToAsyncWithDefaultCancellationTokenCompletesNormally()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Rotation = 0;

			// Run animation with default (none) cancellation token
			var wasCanceled = await box.RotateToAsync(90, 100, null, default);

			// Should report not canceled
			Assert.False(wasCanceled);

			// Rotation should have reached the target value
			Assert.Equal(90d, box.Rotation);
		}

		[Fact]
		public async Task ScaleToAsyncCompletesNormally()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Scale = 1;

			// Run animation to completion
			var wasCanceled = await box.ScaleToAsync(2, 100);

			// Should report not canceled
			Assert.False(wasCanceled);

			// Scale should have reached the target value
			Assert.Equal(2d, box.Scale);
		}

		[Fact]
		public async Task TranslateToAsyncCompletesNormally()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.TranslationX = 0;
			box.TranslationY = 0;

			// Run animation to completion
			var wasCanceled = await box.TranslateToAsync(50, 50, 100);

			// Should report not canceled
			Assert.False(wasCanceled);

			// Translation should have reached the target values
			Assert.Equal(50d, box.TranslationX);
			Assert.Equal(50d, box.TranslationY);
		}

		[Fact]
		public async Task RotateXToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.RotationX = 0;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.RotateXToAsync(45, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// RotationX should not have changed
			Assert.Equal(0d, box.RotationX);
		}

		[Fact]
		public async Task RotateYToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.RotationY = 0;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.RotateYToAsync(45, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// RotationY should not have changed
			Assert.Equal(0d, box.RotationY);
		}

		[Fact]
		public async Task ScaleXToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.ScaleX = 1;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.ScaleXToAsync(3, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// ScaleX should not have changed
			Assert.Equal(1d, box.ScaleX);
		}

		[Fact]
		public async Task ScaleYToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.ScaleY = 1;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.ScaleYToAsync(3, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// ScaleY should not have changed
			Assert.Equal(1d, box.ScaleY);
		}

		[Fact]
		public async Task RelRotateToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Rotation = 45;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.RelRotateToAsync(90, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// Rotation should not have changed (should still be 45, not 135)
			Assert.Equal(45d, box.Rotation);
		}

		[Fact]
		public async Task RelScaleToAsyncWithPreCanceledTokenReturnsImmediately()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Scale = 2;

			// Create an already canceled token
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			// Animation should return immediately
			var wasCanceled = await box.RelScaleToAsync(1, 2000, null, cts.Token);

			// Should report canceled
			Assert.True(wasCanceled);

			// Scale should not have changed (should still be 2, not 3)
			Assert.Equal(2d, box.Scale);
		}

		[Fact]
		public async Task FadeToAsyncCanceledMidFlightReturnsCanceled()
		{
			var box = AnimationReadyHandler<AsyncTicker>.Prepare(new BoxView());
			box.Opacity = 1;

			using var cts = new CancellationTokenSource();

			// Start a long animation (10 seconds) and cancel it after a short delay
			var animationTask = box.FadeToAsync(0, 10_000, null, cts.Token);

			// Let a few frames run so the animation is mid-flight
			// Poll until opacity actually changes (avoids fixed-delay flakiness on loaded CI machines)
			var sw = System.Diagnostics.Stopwatch.StartNew();
			while (box.Opacity >= 1d && sw.ElapsedMilliseconds < 5000)
				await Task.Delay(32);

			// Verify it hasn't completed yet
			Assert.False(animationTask.IsCompleted, "Animation should still be running");

			// Opacity should have moved somewhat (animation is running)
			Assert.True(box.Opacity < 1d, $"Expected opacity to have decreased from 1, but was {box.Opacity}");

			// Cancel mid-flight
			cts.Cancel();

			var wasCanceled = await animationTask;

			// Should report canceled
			Assert.True(wasCanceled);

			// Opacity should be between 0 and 1 (stopped mid-flight, not reached target)
			Assert.True(box.Opacity < 1d, "Opacity should not have reset to start value");
			Assert.True(box.Opacity > 0d, "Opacity should not have reached target value 0");
		}
	}
}