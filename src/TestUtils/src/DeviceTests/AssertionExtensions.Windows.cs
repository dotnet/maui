using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
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
		public static Task<string> CreateColorAtPointErrorAsync(this CanvasBitmap bitmap, WColor expectedColor, int x, int y) =>
			CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in renderered view.");

		public static Task WaitForKeyboardToShow(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task WaitForKeyboardToHide(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task SendValueToKeyboard(this FrameworkElement view, char value, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task SendKeyboardReturnType(this FrameworkElement view, ReturnType returnType, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task WaitForFocused(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task WaitForUnFocused(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task FocusView(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task ShowKeyboardForView(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task HideKeyboardForView(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task<string> CreateColorAtPointError(this CanvasBitmap bitmap, WColor expectedColor, int x, int y) =>
			CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in renderered view.");

		public static async Task<string> CreateColorError(this CanvasBitmap bitmap, string message) =>
			$"{message} This is what it looked like:<img>{await bitmap.ToBase64StringAsync()}</img>";

		public static async Task<string> CreateEqualError(this CanvasBitmap bitmap, CanvasBitmap other, string message) =>
			$"{message} This is what it looked like: <img>{await bitmap.ToBase64StringAsync()}</img> and <img>{await other.ToBase64StringAsync()}</img>";

		public static async Task<string> ToBase64StringAsync(this CanvasBitmap bitmap)
		{
			using var ms = new InMemoryRandomAccessStream();
			await bitmap.SaveAsync(ms, CanvasBitmapFileFormat.Png);

			using var ms2 = new MemoryStream();
			await ms.AsStreamForRead().CopyToAsync(ms2);

			return Convert.ToBase64String(ms2.ToArray());
		}

		public static WColor ColorAtPoint(this CanvasBitmap bitmap, int x, int y, bool includeAlpha = false)
		{
			var pixel = bitmap.GetPixelColors(x, y, 1, 1)[0];
			return includeAlpha
				? pixel
				: WColor.FromArgb(255, pixel.R, pixel.G, pixel.B);
		}

		public static Task AttachAndRun(this FrameworkElement view, Action action, IMauiContext? mauiContext = null) =>
			view.AttachAndRun(window => action(), mauiContext);

		public static Task AttachAndRun(this FrameworkElement view, Action<Window> action, IMauiContext? mauiContext = null) =>
			view.AttachAndRun((window) =>
			{
				action(window);
				return Task.FromResult(true);
			}, mauiContext);

		public static Task<T> AttachAndRun<T>(this FrameworkElement view, Func<T> action, IMauiContext? mauiContext = null) =>
			view.AttachAndRun(window => action(), mauiContext);

		public static Task<T> AttachAndRun<T>(this FrameworkElement view, Func<Window, T> action, IMauiContext? mauiContext = null) =>
			view.AttachAndRun((window) =>
			{
				var result = action(window);
				return Task.FromResult(result);
			}, mauiContext);

		public static Task AttachAndRun(this FrameworkElement view, Func<Task> action) =>
			view.AttachAndRun(window => action());

		public static Task AttachAndRun(this FrameworkElement view, Func<Window, Task> action) =>
			view.AttachAndRun(async (window) =>
			{
				await action(window);
				return true;
			});

		// Windows does ok running these tests in parallel but there's definitely
		// a limit where it'll eventually be too many windows.
		// So, for now we're limiting this to 10 parallel windows which seems 
		// to work fine.
		static SemaphoreSlim _attachAndRunSemaphore = new SemaphoreSlim(10);

		public static Task<T> AttachAndRun<T>(this FrameworkElement view, Func<Task<T>> action) =>
			view.AttachAndRun(window => action());

		public static async Task<T> AttachAndRun<T>(this FrameworkElement view, Func<Window, Task<T>> action, IMauiContext? mauiContext = null)
		{
			if (view.Parent is Border wrapper)
				view = wrapper;

			TaskCompletionSource? tcs = null;
			TaskCompletionSource? unloadedTcs = null;

			if (view.Parent == null)
			{
				T result;

				try
				{
					await _attachAndRunSemaphore.WaitAsync();

					// prepare to wait for element to be in the UI
					tcs = new TaskCompletionSource();
					unloadedTcs = new TaskCompletionSource();

					view.Loaded += OnViewLoaded;

					// attach to the UI
					Grid grid;
					var window = (mauiContext?.Services != null) ?
						Extensions.DependencyInjection.ActivatorUtilities.GetServiceOrCreateInstance<Window>(mauiContext.Services) : new Window();

					window.Content = new Grid
					{
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						Children =
						{
							(grid = new Grid
							{
								Width = view.Width,
								Height = view.Height,
								Children =
								{
									view
								}
							})
						}
					};

					window.Activate();

					// wait for element to be loaded
					await tcs.Task;
					view.Unloaded += OnViewUnloaded;

					try
					{
						result = await Run(() => action(window));
					}
					finally
					{
						grid.Children.Clear();
						await unloadedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
						await Task.Delay(10);
						window.Close();
					}
				}
				finally
				{
					_attachAndRunSemaphore.Release();
				}

				return result;
			}
			else
			{
				var window = view.GetParentOfType<Window>() ?? throw new InvalidOperationException("View was attached to a window but there was no window.");
				return await Run(() => action(window));
			}

			static async Task<T> Run(Func<Task<T>> action)
			{
				return await action();
			}

			void OnViewLoaded(object sender, RoutedEventArgs e)
			{
				view.Loaded -= OnViewLoaded;
				tcs?.SetResult();
			}

			void OnViewUnloaded(object sender, RoutedEventArgs e)
			{
				view.Unloaded -= OnViewUnloaded;
				unloadedTcs?.SetResult();
			}
		}

		public static Task<CanvasBitmap> ToBitmap(this FrameworkElement view) =>
			view.AttachAndRun(async (window) =>
			{
				if (view.Parent is Border wrapper)
					view = wrapper;

				var device = CanvasDevice.GetSharedDevice();

				// HELP?
				// The simple act of doing a window capture results in the next render method
				// working on DirectX controls (such as Win2D).
				// We could use this window bitmap directly, but that is extra effort to crop
				// to the view bounds... so until this breaks...
				using var windowBitmap = await CaptureHelper.RenderAsync(window, device);

				var bmp = new RenderTargetBitmap();
				await bmp.RenderAsync(view);
				var pixels = await bmp.GetPixelsAsync();
				var width = bmp.PixelWidth;
				var height = bmp.PixelHeight;

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

		public static Task<CanvasBitmap> AssertContainsColor(this CanvasBitmap bitmap, Graphics.Color expectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null)
			=> bitmap.AssertContainsColor(expectedColor.ToWindowsColor());

		public static async Task<CanvasBitmap> AssertContainsColor(this CanvasBitmap bitmap, WColor expectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null)
		{
			var imageRect = new Graphics.RectF(0, 0, bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height);

			if (withinRectModifier is not null)
				imageRect = withinRectModifier.Invoke(imageRect);

			var colors = bitmap.GetPixelColors((int)imageRect.X, (int)imageRect.Y, (int)imageRect.Width, (int)imageRect.Height);

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

		public static async Task<CanvasBitmap> AssertColorAtPointAsync(this FrameworkElement view, WColor expectedColor, int x, int y)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static async Task<CanvasBitmap> AssertColorAtCenterAsync(this FrameworkElement view, WColor expectedColor)
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

		public static async Task AssertEqualAsync(this CanvasBitmap bitmap, CanvasBitmap other)
		{
			Assert.NotNull(bitmap);
			Assert.NotNull(other);

			Assert.Equal(bitmap.SizeInPixels, other.SizeInPixels);

			Assert.True(IsMatching(), await CreateEqualError(bitmap, other, $"Images did not match."));

			bool IsMatching()
			{
				var first = bitmap.GetPixelColors();
				var second = other.GetPixelColors();
				for (int i = 0; i < first.Length; i++)
				{
					if (first[i] != second[i])
						return false;
				}
				return true;
			}
		}

		public static TextTrimming ToPlatform(this LineBreakMode mode) =>
			mode switch
			{
				LineBreakMode.NoWrap => TextTrimming.Clip,
				LineBreakMode.WordWrap => TextTrimming.None,
				LineBreakMode.CharacterWrap => TextTrimming.WordEllipsis,
				LineBreakMode.HeadTruncation => TextTrimming.WordEllipsis,
				LineBreakMode.TailTruncation => TextTrimming.CharacterEllipsis,
				LineBreakMode.MiddleTruncation => TextTrimming.WordEllipsis,
				_ => throw new ArgumentOutOfRangeException(nameof(mode))
			};

		public static bool IsAccessibilityElement(this FrameworkElement platformView)
		{
			return AutomationProperties.GetAccessibilityView(platformView) == UI.Xaml.Automation.Peers.AccessibilityView.Content;
		}

		public static bool IsExcludedWithChildren(this FrameworkElement platformView)
		{
			throw new NotImplementedException();
		}
	}
}
