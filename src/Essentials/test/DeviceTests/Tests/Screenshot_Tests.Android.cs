#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Xunit;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using MauiPlatform = Microsoft.Maui.ApplicationModel.Platform;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Category("Screenshot")]
public class Screenshot_Android_Tests
{
	const int Size = 200;

	// https://github.com/dotnet/maui/issues/40152
	// Capturing a single view must render that view, not whatever happens
	// to be drawn on screen at the view's location.
	[Fact]
	public Task CaptureViewDoesNotIncludeOverlappingSiblings()
	{
		return Utils.OnMainThread(async () =>
		{
			var (activity, content) = GetHost();

			var container = new FrameLayout(activity);
			var target = new AView(activity);
			target.SetBackgroundColor(AColor.Red);
			var overlay = new AView(activity);
			overlay.SetBackgroundColor(AColor.Blue);

			container.AddView(target, new FrameLayout.LayoutParams(Size, Size));
			container.AddView(overlay, new FrameLayout.LayoutParams(Size, Size));
			content.AddView(container, new FrameLayout.LayoutParams(Size, Size));

			try
			{
				await WaitForLayoutAsync(target);

				var result = await Screenshot.Default.CaptureAsync(target);
				Assert.NotNull(result);
				Assert.Equal(Size, result!.Width);
				Assert.Equal(Size, result.Height);

				using var bitmap = await DecodeAsync(result);
				AssertPixel(AColor.Red, bitmap, Size / 2, Size / 2);
				AssertPixel(AColor.Red, bitmap, 1, 1);
				AssertPixel(AColor.Red, bitmap, Size - 2, Size - 2);
			}
			finally
			{
				content.RemoveView(container);
			}
		});
	}

	// https://github.com/dotnet/maui/issues/40152
	// A view taller than its scroll viewport must be captured in full, including
	// the part that is scrolled out of sight, not just what the window shows.
	[Fact]
	public Task CaptureViewIncludesContentClippedByScrollViewport()
	{
		return Utils.OnMainThread(async () =>
		{
			var (activity, content) = GetHost();

			// The target is two bands: red (visible in the viewport) on top of
			// green (below the viewport). On screen, the area under the viewport is
			// blue, so a capture of the window region would show blue there.
			var target = new LinearLayout(activity) { Orientation = Orientation.Vertical };
			var visibleBand = new AView(activity);
			visibleBand.SetBackgroundColor(AColor.Red);
			var clippedBand = new AView(activity);
			clippedBand.SetBackgroundColor(AColor.Green);
			target.AddView(visibleBand, new LinearLayout.LayoutParams(Size, Size));
			target.AddView(clippedBand, new LinearLayout.LayoutParams(Size, Size));

			var viewport = new ScrollView(activity);
			viewport.AddView(target, new ViewGroup.LayoutParams(Size, Size * 2));

			var container = new FrameLayout(activity);
			container.SetBackgroundColor(AColor.Blue);
			container.AddView(viewport, new FrameLayout.LayoutParams(Size, Size));
			content.AddView(container, new FrameLayout.LayoutParams(Size, Size * 2));

			try
			{
				await WaitForLayoutAsync(target);
				Assert.Equal(Size, viewport.Height);
				Assert.Equal(Size * 2, target.Height);

				var result = await Screenshot.Default.CaptureAsync(target);
				Assert.NotNull(result);
				Assert.Equal(Size, result!.Width);
				Assert.Equal(Size * 2, result.Height);

				using var bitmap = await DecodeAsync(result);
				AssertPixel(AColor.Red, bitmap, Size / 2, Size / 2);
				AssertPixel(AColor.Green, bitmap, Size / 2, Size + Size / 2);
				AssertPixel(AColor.Green, bitmap, Size - 2, Size * 2 - 2);
			}
			finally
			{
				content.RemoveView(container);
			}
		});
	}

	// https://github.com/dotnet/maui/issues/30010
	// Capturing the whole window must keep using PixelCopy: it is the only path that
	// includes hardware-composited content. A TextureView is used because its pixels
	// exist only on the GPU, so drawing the view hierarchy into a bitmap leaves it empty.
	[Fact]
	public Task CaptureRootViewIncludesHardwareRenderedContent()
	{
		return Utils.OnMainThread(async () =>
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(26))
				return;

			var (activity, content) = GetHost();

			var listener = new SurfaceListener();
			var textureView = new TextureView(activity)
			{
				SurfaceTextureListener = listener
			};
			content.AddView(textureView, new FrameLayout.LayoutParams(Size, Size));

			try
			{
				await listener.Available.Task.WaitAsync(TimeSpan.FromSeconds(10));

				var canvas = textureView.LockCanvas()
					?? throw new InvalidOperationException("Unable to lock the TextureView canvas.");
				canvas.DrawColor(AColor.Green);
				textureView.UnlockCanvasAndPost(canvas);

				// The TextureView shows its new frame only after it has consumed it.
				await listener.Updated.Task.WaitAsync(TimeSpan.FromSeconds(10));
				await WaitForLayoutAsync(textureView);

				var root = activity.Window?.DecorView?.RootView
					?? throw new InvalidOperationException("The activity has no root view.");

				var result = await Screenshot.Default.CaptureAsync(root);
				Assert.NotNull(result);
				Assert.Equal(root.Width, result!.Width);
				Assert.Equal(root.Height, result.Height);

				var location = new int[2];
				textureView.GetLocationInWindow(location);

				using var bitmap = await DecodeAsync(result);
				AssertPixel(AColor.Green, bitmap, location[0] + Size / 2, location[1] + Size / 2);
			}
			finally
			{
				content.RemoveView(textureView);
			}
		});
	}

	static (global::Android.App.Activity Activity, ViewGroup Content) GetHost()
	{
		var activity = MauiPlatform.CurrentActivity
			?? throw new InvalidOperationException("The device-test host activity is unavailable.");
		var content = activity.FindViewById<ViewGroup>(global::Android.Resource.Id.Content)
			?? throw new InvalidOperationException("The activity has no content view.");
		return (activity, content);
	}

	static async Task WaitForLayoutAsync(AView view)
	{
		for (var i = 0; i < 50 && (!view.IsAttachedToWindow || view.Width <= 0 || view.Height <= 0); i++)
			await Task.Delay(50);

		// Give the render thread a chance to draw the new views to the window surface.
		await Task.Delay(250);
	}

	static async Task<Bitmap> DecodeAsync(IScreenshotResult result)
	{
		using var stream = await result.OpenReadAsync(ScreenshotFormat.Png);
		using var ms = new MemoryStream();
		await stream.CopyToAsync(ms);
		var bytes = ms.ToArray();
		return BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length)
			?? throw new InvalidOperationException("Unable to decode the screenshot.");
	}

	static void AssertPixel(AColor expected, Bitmap bitmap, int x, int y)
	{
		var actual = new AColor(bitmap.GetPixel(x, y));
		Assert.True(
			actual.R == expected.R && actual.G == expected.G && actual.B == expected.B,
			$"Expected pixel ({x},{y}) to be #{expected.R:X2}{expected.G:X2}{expected.B:X2} but was #{actual.R:X2}{actual.G:X2}{actual.B:X2}.");
	}

	sealed class SurfaceListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
	{
		public TaskCompletionSource Available { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource Updated { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) =>
			Available.TrySetResult();

		public bool OnSurfaceTextureDestroyed(SurfaceTexture surface) => true;

		public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
		{
		}

		public void OnSurfaceTextureUpdated(SurfaceTexture surface) =>
			Updated.TrySetResult();
	}
}
