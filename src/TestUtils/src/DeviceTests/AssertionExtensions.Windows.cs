using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.DirectX;
using Windows.Storage.Streams;
using Xunit;
using Xunit.Sdk;
using WColor = Windows.UI.Color;
using WHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using WVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
	{
		public static Task<string> CreateColorAtPointErrorAsync(this CanvasBitmap bitmap, WColor expectedColor, int x, int y) =>
			CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in rendered view.");

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

		public static async Task WaitForFocused(this FrameworkElement view, int timeout = 1000)
		{
			TaskCompletionSource focusSource = new TaskCompletionSource();

			FocusManager.GotFocus += OnFocused;

			try
			{
				await focusSource.Task.WaitAsync(TimeSpan.FromMilliseconds(timeout));
			}
			finally
			{
				FocusManager.GotFocus -= OnFocused;
			}

			void OnFocused(object? sender, FocusManagerGotFocusEventArgs e)
			{
				if (e.NewFocusedElement == view)
				{
					FocusManager.GotFocus -= OnFocused;
					focusSource.SetResult();
				}
			}
		}

		public static async Task WaitForUnFocused(this FrameworkElement view, int timeout = 1000)
		{
			TaskCompletionSource focusSource = new TaskCompletionSource();

			FocusManager.LostFocus += OnUnFocused;

			try
			{
				await focusSource.Task.WaitAsync(TimeSpan.FromMilliseconds(timeout));
			}
			finally
			{
				FocusManager.LostFocus -= OnUnFocused;
			}

			void OnUnFocused(object? sender, FocusManagerLostFocusEventArgs e)
			{
				if (e.OldFocusedElement == view)
				{
					FocusManager.LostFocus -= OnUnFocused;
					focusSource.SetResult();
				}
			}
		}

		public static Task FocusView(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task ShowKeyboardForView(this FrameworkElement view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task HideKeyboardForView(this FrameworkElement view, int timeout = 1000, string? message = null)
		{
			throw new NotImplementedException();
		}

		public static Task<string> CreateColorAtPointError(this CanvasBitmap bitmap, WColor expectedColor, int x, int y) =>
			CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in rendered view.");

		public static async Task<string> CreateColorError(this CanvasBitmap bitmap, string message) =>
			$"{message} This is what it looked like:<img>{await bitmap.ToBase64StringAsync()}</img>";

		public static async Task<string> CreateEqualError(this CanvasBitmap bitmap, CanvasBitmap other, string message) =>
			$"{message} This is what it looked like: <img>{await bitmap.ToBase64StringAsync()}</img> and <img>{await other.ToBase64StringAsync()}</img>";

		public static async Task<string> CreateScreenshotError(this CanvasBitmap bitmap, string message) =>
			$"{message} This is what it looked like:<img>{await bitmap.ToBase64StringAsync()}</img>";

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

		public static Task AttachAndRun(this FrameworkElement view, Action action, IMauiContext mauiContext) =>
			view.AttachAndRun(window => action(), mauiContext);

		public static Task AttachAndRun(this FrameworkElement view, Action<Window> action, IMauiContext mauiContext) =>
			view.AttachAndRun((window) =>
			{
				action(window);
				return Task.FromResult(true);
			}, mauiContext);

		public static Task<T> AttachAndRun<T>(this FrameworkElement view, Func<T> action, IMauiContext mauiContext) =>
			view.AttachAndRun(window => action(), mauiContext);

		public static Task AttachAndRun(this FrameworkElement view, Func<Task> action, IMauiContext mauiContext) =>
			view.AttachAndRun(async window =>
			{
				await action();
				return true;
			}, mauiContext);

		public static Task<T> AttachAndRun<T>(this FrameworkElement view, Func<Window, T> action, IMauiContext mauiContext) =>
			view.AttachAndRun((window) =>
			{
				var result = action(window);
				return Task.FromResult(result);
			}, mauiContext);

		// Windows does ok running these tests in parallel but there's definitely
		// a limit where it'll eventually be too many windows.
		// So, for now we're limiting this to 10 parallel windows which seems 
		// to work fine.
		static SemaphoreSlim _attachAndRunSemaphore = new SemaphoreSlim(10);

		public static async Task<T> AttachAndRun<T>(this FrameworkElement view, Func<Window, Task<T>> action, IMauiContext mauiContext)
		{
			if (view.Parent is Border wrapper)
				view = wrapper;

			// If the view has a parent, it's already attached to the UI
			if (view.Parent != null)
			{
				// Window is not a XAML type so is never on the hierarchy
				var window = (Window)mauiContext!.Services!.GetService(typeof(Window))!;

				Assert.True(window?.Content is not null,
					"Content on Window has not been set. Most likely the window under test isn't being registered against the test service being used. Check if you're passing the right MauiContext in");

				if (window.Content.XamlRoot != view.XamlRoot)
					throw new Exception("The window retrieved from the service is different than the window this view is attached to");

				return await Run(() => action(window));
			}

			// If the view has no parent, we need to attach it to the UI by creating a new window and using that as the host
			try
			{
				await _attachAndRunSemaphore.WaitAsync();

				// prepare to wait for element to be in the UI
				var loadedTcs = new TaskCompletionSource();
				var unloadedTcs = new TaskCompletionSource();

				view.Loaded += OnViewLoaded;

				// attach to the UI
				Grid viewContainer;
				var window = (Window)mauiContext!.Services!.GetService(typeof(Window))!;

				if (window.Content is not null)
					throw new Exception("The window retrieved from the service is already attached to existing content");

				window.Content = new Grid
				{
					HorizontalAlignment = WHorizontalAlignment.Center,
					VerticalAlignment = WVerticalAlignment.Center,
					Children =
					{
						(viewContainer = new Grid
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
				await loadedTcs.Task;
				view.Unloaded += OnViewUnloaded;

				try
				{
					return await Run(() => action(window));
				}
				finally
				{
					// release all views
					window.Content = null;
					viewContainer.Children.Clear();

					// wait for an unload
					await unloadedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
					await Task.Delay(10);

					// close the window
					window.Close();
				}

				void OnViewLoaded(object sender, RoutedEventArgs e)
				{
					view.Loaded -= OnViewLoaded;
					loadedTcs?.SetResult();
				}

				void OnViewUnloaded(object sender, RoutedEventArgs e)
				{
					view.Unloaded -= OnViewUnloaded;
					unloadedTcs?.SetResult();
				}
			}
			finally
			{
				_attachAndRunSemaphore.Release();
			}

			static async Task<T> Run(Func<Task<T>> action)
			{
				return await action();
			}
		}

		public static Task<CanvasBitmap> ToBitmap(this FrameworkElement view, IMauiContext mauiContext) =>
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
			}, mauiContext);

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

		public static Task<CanvasBitmap> AssertContainsColor(this CanvasBitmap bitmap, Graphics.Color expectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null, double? tolerance = null)
			=> bitmap.AssertContainsColor(expectedColor.ToWindowsColor(), withinRectModifier, tolerance: tolerance);

		public static async Task<CanvasBitmap> AssertContainsColor(this CanvasBitmap bitmap, WColor expectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null, double? tolerance = null)
		{
			var imageRect = new Graphics.RectF(0, 0, bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height);

			if (withinRectModifier is not null)
				imageRect = withinRectModifier.Invoke(imageRect);

			if (imageRect.Width == 0 || imageRect.Height == 0)
			{
				// Detect this case and give a better message instead of letting GetPixelColors throw an IndexOutOfRangeException
				Assert.Fail($"Bitmap must have non-zero width and height.  Width = {(int)imageRect.Width} Height = {(int)imageRect.Height}.");
				return bitmap;
			}

			var colors = bitmap.GetPixelColors((int)imageRect.X, (int)imageRect.Y, (int)imageRect.Width, (int)imageRect.Height);

			foreach (var c in colors)
			{
				if (c.IsEquivalent(expectedColor))
				{
					return bitmap;
				}
			}

			Assert.Fail(await CreateColorError(bitmap, $"Color {expectedColor} not found."));
			return bitmap;
		}

		public static Task<CanvasBitmap> AssertContainsColor(this FrameworkElement view, Maui.Graphics.Color expectedColor, IMauiContext mauiContext, double? tolerance = null) =>
			AssertContainsColor(view, expectedColor.ToWindowsColor(), mauiContext, tolerance: tolerance);

		public static async Task<CanvasBitmap> AssertContainsColor(this FrameworkElement view, WColor expectedColor, IMauiContext mauiContext, double? tolerance = null)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return await AssertContainsColor(bitmap, expectedColor, tolerance: tolerance);
		}

		public static Task<CanvasBitmap> AssertDoesNotContainColor(this CanvasBitmap bitmap, Graphics.Color unexpectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null)
			=> bitmap.AssertDoesNotContainColor(unexpectedColor.ToWindowsColor(), withinRectModifier);

		public static async Task<CanvasBitmap> AssertDoesNotContainColor(this CanvasBitmap bitmap, WColor unexpectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null)
		{
			var imageRect = new Graphics.RectF(0, 0, bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height);

			if (withinRectModifier is not null)
				imageRect = withinRectModifier.Invoke(imageRect);

			if (imageRect.Width == 0 || imageRect.Height == 0)
			{
				// Detect this case and give a better message instead of letting GetPixelColors throw an IndexOutOfRangeException
				Assert.Fail($"Bitmap must have non-zero width and height.  Width = {(int)imageRect.Width} Height = {(int)imageRect.Height}.");
				return bitmap;
			}

			var colors = bitmap.GetPixelColors((int)imageRect.X, (int)imageRect.Y, (int)imageRect.Width, (int)imageRect.Height);

			foreach (var c in colors)
			{
				if (c.IsEquivalent(unexpectedColor))
				{
					Assert.Fail(await CreateColorError(bitmap, $"Color {unexpectedColor} was found."));
				}
			}

			return bitmap;
		}

		public static Task<CanvasBitmap> AssertDoesNotContainColor(this FrameworkElement view, Maui.Graphics.Color unexpectedColor, IMauiContext mauiContext) =>
			AssertDoesNotContainColor(view, unexpectedColor.ToWindowsColor(), mauiContext);

		public static async Task<CanvasBitmap> AssertDoesNotContainColor(this FrameworkElement view, WColor unexpectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return await AssertDoesNotContainColor(bitmap, unexpectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtPointAsync(this FrameworkElement view, WColor expectedColor, int x, int y, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static async Task<CanvasBitmap> AssertColorsAtPointsAsync(this FrameworkElement view, Graphics.Color[] colors, Graphics.Point[] points, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);

			for (int i = 0; i < points.Length; i++)
			{
				bitmap.AssertColorAtPoint(colors[i].ToWindowsColor(), (int)points[i].X, (int)points[i].Y);
			}

			return bitmap;
		}

		public static async Task<CanvasBitmap> AssertColorAtCenterAsync(this FrameworkElement view, WColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtBottomLeft(this FrameworkElement view, WColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtBottomRight(this FrameworkElement view, WColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtTopLeft(this FrameworkElement view, WColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static async Task<CanvasBitmap> AssertColorAtTopRight(this FrameworkElement view, WColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
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

		public static async Task ThrowScreenshot(this FrameworkElement view, IMauiContext mauiContext, string? message = null, Exception? ex = null)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			if (ex is null)
				throw new XunitException(await CreateScreenshotError(bitmap, message ?? "There was an error."));
			else
				throw new XunitException(await CreateScreenshotError(bitmap, message ?? "There was an error: " + ex.Message), ex);
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

		static List<NavigationViewItem?> GetTabBarItems(this NavigationView navigationView)
		{
			StackPanel? topNavArea;
			if (navigationView is MauiNavigationView mauiNavView)
				topNavArea = mauiNavView.TopNavArea;
			else
				topNavArea = navigationView.FindName("TopNavArea") as StackPanel;

			if (topNavArea is null)
				throw new Exception("Unable to find Top Nav Area");

			return topNavArea.GetChildren<NavigationViewItem>().ToList();
		}

		static public Task AssertTabItemTextDoesNotContainColor(
			this NavigationView navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemTextColor(navigationView, tabText, expectedColor, false, mauiContext);

		static public Task AssertTabItemTextContainsColor(
			this NavigationView navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemTextColor(navigationView, tabText, expectedColor, true, mauiContext);

		static async Task AssertTabItemTextColor(
			this NavigationView navigationView,
			string tabText,
			Color expectedColor,
			bool hasColor,
			IMauiContext mauiContext)
		{
			var items = navigationView.GetTabBarItems();
			var platformItem =
				items.FirstOrDefault(x => x?.Content?.ToString()?.Equals(tabText, StringComparison.OrdinalIgnoreCase) == true)
				?.GetDescendantByName<UI.Xaml.Controls.Grid>("ContentGrid")
				?.GetDescendantByName<UI.Xaml.Controls.ContentPresenter>("ContentPresenter");

			if (platformItem is null)
				throw new Exception("Unable to locate Tab Item Text Container");

			if (hasColor)
				await AssertContainsColor(platformItem, expectedColor, mauiContext);
			else
				await AssertDoesNotContainColor(platformItem, expectedColor, mauiContext);
		}

		static async Task AssertTabItemIconColor(
			this NavigationView navigationView, string tabText, Color expectedColor, bool hasColor,
			IMauiContext mauiContext)
		{
			var items = navigationView.GetTabBarItems();
			var platformItem =
				items.FirstOrDefault(x => x?.Content?.ToString()?.Equals(tabText, StringComparison.OrdinalIgnoreCase) == true)
				?.GetDescendantByName<UI.Xaml.Controls.ContentPresenter>("Icon");

			if (platformItem is null)
				throw new Exception("Unable to locate Tab Item Icon Container");

			if (hasColor)
				await AssertContainsColor(platformItem, expectedColor, mauiContext);
			else
				await AssertDoesNotContainColor(platformItem, expectedColor, mauiContext);
		}

		static public Task AssertTabItemIconDoesNotContainColor(
			this NavigationView navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemIconColor(navigationView, tabText, expectedColor, false, mauiContext);

		static public Task AssertTabItemIconContainsColor(
			this NavigationView navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemIconColor(navigationView, tabText, expectedColor, true, mauiContext);
	}
}
