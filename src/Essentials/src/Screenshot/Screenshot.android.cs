#nullable enable

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Java.Nio;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
		static IWindowManager? WindowManager =>
			Application.Context.GetSystemService(Context.WindowService) as IWindowManager;

		public bool IsCaptureSupported => true;

		public Task<IScreenshotResult> CaptureAsync()
		{
			if (WindowManager?.DefaultDisplay?.Flags.HasFlag(DisplayFlags.Secure) == true)
				throw new UnauthorizedAccessException("Unable to take a screenshot of a secure window.");

			var activity = ActivityStateManager.Default.GetCurrentActivity(true)
				?? throw new InvalidOperationException("Unable to find the current activity.");

			return CaptureAsync(activity);
		}

		public async Task<IScreenshotResult> CaptureAsync(Activity activity)
		{
			var window = activity?.Window;
			var view = window?.DecorView?.RootView;
			if (view == null)
				throw new InvalidOperationException("Unable to find the main window.");

			var result = await CaptureAsync(view, window).ConfigureAwait(false);
			return result ?? throw new InvalidOperationException("Unable to capture screenshot.");
		}

		public async Task<IScreenshotResult?> CaptureAsync(View view)
		{
			return await CaptureAsync(view, GetActivity(view.Context)?.Window).ConfigureAwait(false);
		}

		async Task<IScreenshotResult?> CaptureAsync(View view, Window? window)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			var bitmap = await RenderAsync(view, window).ConfigureAwait(false);
			return bitmap is null ? null : new ScreenshotResult(bitmap);
		}

		static async Task<Bitmap?> RenderAsync(View view, Window? window)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				var bitmap = await RenderUsingPixelCopyAsync(view, window);
				if (bitmap is not null)
					return bitmap;
			}

			return RenderUsingCanvasDrawing(view) ?? RenderUsingDrawingCache(view);
		}

		static async Task<Bitmap?> RenderUsingPixelCopyAsync(View view, Window? window)
		{
			if (view.Width <= 0 || view.Height <= 0 || !view.IsAttachedToWindow)
				return null;

			if (window is null)
				return null;

			var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888!);
			if (bitmap is null)
				return null;

			var location = new int[2];
			view.GetLocationInWindow(location);
			var rect = new Rect(
				location[0],
				location[1],
				location[0] + view.Width,
				location[1] + view.Height);

			var tcs = new TaskCompletionSource<Bitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);

			try
			{
				var listener = new PixelCopyFinishedListener(tcs, bitmap);
				PixelCopy.Request(window, rect, bitmap,
					listener,
					new Handler(Looper.MainLooper!));

				try
				{
					return await tcs.Task.ConfigureAwait(true);
				}
				finally
				{
					GC.KeepAlive(listener);
				}
			}
			catch (Exception)
			{
				bitmap.Dispose();
				return null;
			}
		}

		static Activity? GetActivity(Context? context)
		{
			while (context is ContextWrapper wrapper)
			{
				if (context is Activity activity)
					return activity;
				context = wrapper.BaseContext;
			}
			return context as Activity;
		}

		sealed class PixelCopyFinishedListener : Java.Lang.Object, PixelCopy.IOnPixelCopyFinishedListener
		{
			readonly TaskCompletionSource<Bitmap?> _tcs;
			readonly Bitmap _bitmap;

			public PixelCopyFinishedListener(TaskCompletionSource<Bitmap?> tcs, Bitmap bitmap)
			{
				_tcs = tcs;
				_bitmap = bitmap;
			}

			public void OnPixelCopyFinished(int copyResult)
			{
				// PixelCopy.SUCCESS == 0
				if (copyResult == 0)
				{
					_tcs.TrySetResult(_bitmap);
				}
				else
				{
					_bitmap.Dispose();
					_tcs.TrySetResult(null);
				}
			}
		}

		static Bitmap? RenderUsingCanvasDrawing(View view)
		{
			try
			{
				if (view?.LayoutParameters == null || Bitmap.Config.Argb8888 == null)
					return null;
				var width = view.Width;
				var height = view.Height;

				var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
				if (bitmap is null)
					return null;

				using (var canvas = new Canvas(bitmap))
					view.Draw(canvas);

				return bitmap;
			}
			catch (Exception)
			{
				return null;
			}
		}

		static Bitmap? RenderUsingDrawingCache(View view)
		{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
			try
			{
				var enabled = view.DrawingCacheEnabled;
				view.DrawingCacheEnabled = true;
				view.BuildDrawingCache();
				var cachedBitmap = view.DrawingCache;
				if (cachedBitmap == null)
					return null;
				var bitmap = Bitmap.CreateBitmap(cachedBitmap);
				view.DrawingCacheEnabled = enabled;
				return bitmap;
			}
			catch (Exception)
			{
				return null;
			}
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}

	partial class ScreenshotResult
	{
		readonly Bitmap bmp;

		internal ScreenshotResult(Bitmap bmp)
			: base()
		{
			this.bmp = bmp;

			Width = bmp.Width;
			Height = bmp.Height;
		}

		Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality)
		{
			var result = new MemoryStream();
			PlatformCopyToAsync(result, format, quality);
			result.Position = 0;
			return Task.FromResult<Stream>(result);
		}

		Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
		{
			var f = ToCompressFormat(format);
			bmp.Compress(f, quality, destination);
			return Task.CompletedTask;
		}

		Task<byte[]> PlatformToPixelBufferAsync()
		{
			var byteBuffer = ByteBuffer.AllocateDirect(bmp.ByteCount);
			bmp.CopyPixelsToBuffer(byteBuffer);
			byte[] byt = new byte[bmp.ByteCount];
			Marshal.Copy(byteBuffer.GetDirectBufferAddress(), byt, 0, bmp.ByteCount);
			return Task.FromResult(byt);
		}

		static Bitmap.CompressFormat ToCompressFormat(ScreenshotFormat format) =>
			format switch
			{
				ScreenshotFormat.Png => Bitmap.CompressFormat.Png!,
				ScreenshotFormat.Jpeg => Bitmap.CompressFormat.Jpeg!,
				_ => throw new ArgumentOutOfRangeException(nameof(format)),
			};
	}
}
