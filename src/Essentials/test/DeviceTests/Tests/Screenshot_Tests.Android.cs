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
			var activity = MauiPlatform.CurrentActivity
				?? throw new InvalidOperationException("The device-test host activity is unavailable.");
			var content = activity.FindViewById<ViewGroup>(global::Android.Resource.Id.Content)
				?? throw new InvalidOperationException("The activity has no content view.");

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
}
