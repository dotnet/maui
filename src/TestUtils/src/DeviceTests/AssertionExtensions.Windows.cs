using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.DirectX;
using Windows.Storage.Streams;
using Xunit;
using WColor = Windows.UI.Color;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
	{
		public static Task<string> CreateColorAtPointError(this CanvasBitmap bitmap, WColor expectedColor, int x, int y) =>
			CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in renderered view.");

		public static async Task<string> CreateColorError(this CanvasBitmap bitmap, string message)
		{
			using var ms = new InMemoryRandomAccessStream();
			await bitmap.SaveAsync(ms, CanvasBitmapFileFormat.Png);

			using var ms2 = new MemoryStream();
			await ms.AsStreamForRead().CopyToAsync(ms2);

			var imageAsString = Convert.ToBase64String(ms2.ToArray());

			return $"{message}. This is what it looked like:<img>{imageAsString}</img>";
		}

		public static WColor ColorAtPoint(this CanvasBitmap bitmap, int x, int y, bool includeAlpha = false)
		{
			var pixel = bitmap.GetPixelColors(x, y, 1, 1)[0];
			return includeAlpha
				? pixel
				: WColor.FromArgb(255, pixel.R, pixel.G, pixel.B);
		}

		public static Task AttachAndRun(this FrameworkElement view, Action action) =>
			view.AttachAndRun(() =>
			{
				action();
				return Task.FromResult(true);
			});

		public static Task AttachAndRun(this FrameworkElement view, Func<Task> action) =>
			view.AttachAndRun(async () =>
			{
				await action();
				return true;
			});

		public static async Task<T> AttachAndRun<T>(this FrameworkElement view, Func<Task<T>> action)
		{
			if (view.Parent is Border wrapper)
				view = wrapper;

			// TODO

			//var layout = new FrameLayout(view.Context)
			//{
			//	LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
			//};
			//view.LayoutParameters = new FrameLayout.LayoutParams(view.Width, view.Height)
			//{
			//	Gravity = GravityFlags.Center
			//};

			//var act = view.Context.GetActivity();
			//var rootView = act.FindViewById<FrameLayout>(Android.Resource.Id.Content);

			//layout.AddView(view);
			//rootView.AddView(layout);

			//await Task.Delay(100);

			try
			{
				var result = await action();
				return result;
			}
			finally
			{
				//rootView.RemoveView(layout);
				//layout.RemoveView(view);
			}
		}

		public static Task<CanvasBitmap> ToBitmap(this FrameworkElement view) =>
			view.AttachAndRun(async () =>
			{
				if (view.Parent is Border wrapper)
					view = wrapper;

				var bmp = new RenderTargetBitmap();
				await bmp.RenderAsync(view);
				var pixels = await bmp.GetPixelsAsync();
				var width = bmp.PixelWidth;
				var height = bmp.PixelHeight;

				var device = CanvasDevice.GetSharedDevice();

				return CanvasBitmap.CreateFromBytes(device, pixels, width, height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
			});

		public static CanvasBitmap AssertColorAtPoint(this CanvasBitmap bitmap, WColor expectedColor, uint x, uint y) =>
			bitmap.AssertColorAtPoint(expectedColor, (int)x, (int)y);

		public static CanvasBitmap AssertColorAtPoint(this CanvasBitmap bitmap, WColor expectedColor, int x, int y)
		{
			var actualColor = bitmap.ColorAtPoint(x, y);

			if (!actualColor.IsEquivalent(expectedColor))
				Assert.Equal(expectedColor, actualColor);

			return bitmap;
		}

		public static CanvasBitmap AssertColorAtCenter(this CanvasBitmap bitmap, WColor expectedColor) =>
			bitmap.AssertColorAtPoint(expectedColor, bitmap.SizeInPixels.Width / 2, bitmap.SizeInPixels.Height / 2);

		public static CanvasBitmap AssertColorAtBottomLeft(this CanvasBitmap bitmap, WColor expectedColor) =>
			bitmap.AssertColorAtPoint(expectedColor, 0, 0);

		public static CanvasBitmap AssertColorAtBottomRight(this CanvasBitmap bitmap, WColor expectedColor)
			=> bitmap.AssertColorAtPoint(expectedColor, bitmap.SizeInPixels.Width - 1, 0);

		public static CanvasBitmap AssertColorAtTopLeft(this CanvasBitmap bitmap, WColor expectedColor)
			=> bitmap.AssertColorAtPoint(expectedColor, 0, bitmap.SizeInPixels.Height - 1);

		public static CanvasBitmap AssertColorAtTopRight(this CanvasBitmap bitmap, WColor expectedColor)
			=> bitmap.AssertColorAtPoint(expectedColor, bitmap.SizeInPixels.Width - 1, bitmap.SizeInPixels.Height - 1);

		public static async Task<CanvasBitmap> AssertContainsColor(this CanvasBitmap bitmap, WColor expectedColor)
		{
			var colors = bitmap.GetPixelColors();

			foreach (var c in colors)
			{
				if (c.IsEquivalent(expectedColor))
				{
					return bitmap;
				}
			}

			Assert.True(false, await CreateColorError(bitmap, $"Color {expectedColor} not found."));
			return bitmap;
		}

		public static Task<CanvasBitmap> AssertContainsColor(this FrameworkElement view, Maui.Graphics.Color expectedColor) =>
			AssertContainsColor(view, expectedColor.ToWindowsColor());

		public static async Task<CanvasBitmap> AssertContainsColor(this FrameworkElement view, WColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return await AssertContainsColor(bitmap, expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtPoint(this FrameworkElement view, WColor expectedColor, int x, int y)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static async Task<CanvasBitmap> AssertColorAtCenter(this FrameworkElement view, WColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtBottomLeft(this FrameworkElement view, WColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtBottomRight(this FrameworkElement view, WColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtTopLeft(this FrameworkElement view, WColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtTopRight(this FrameworkElement view, WColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtTopRight(expectedColor);
		}

		//public static TextUtils.TruncateAt ToPlatform(this LineBreakMode mode) =>
		//	mode switch
		//	{
		//		LineBreakMode.NoWrap => null,
		//		LineBreakMode.WordWrap => null,
		//		LineBreakMode.CharacterWrap => null,
		//		LineBreakMode.HeadTruncation => TextUtils.TruncateAt.Start,
		//		LineBreakMode.TailTruncation => TextUtils.TruncateAt.End,
		//		LineBreakMode.MiddleTruncation => TextUtils.TruncateAt.Middle,
		//		_ => throw new ArgumentOutOfRangeException(nameof(mode))
		//	};

		//public static FontWeight GetFontWeight(this Typeface typeface) =>
		//	PlatformVersion.IsAtLeast(28)
		//		? (FontWeight)typeface.Weight
		//		: typeface.IsBold ? FontWeight.Bold : FontWeight.Regular;
	}
}