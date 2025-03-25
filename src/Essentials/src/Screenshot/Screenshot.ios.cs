#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.ApplicationModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
		public bool IsCaptureSupported =>
			true;

		public Task<IScreenshotResult> CaptureAsync()
		{
			var currentWindow = WindowStateManager.Default.GetCurrentUIWindow();
			if (currentWindow == null)
				throw new InvalidOperationException("Unable to find current window.");

			return CaptureAsync(currentWindow);
		}

		public Task<IScreenshotResult> CaptureAsync(UIWindow window)
		{
			_ = window ?? throw new ArgumentNullException(nameof(window));

			// NOTE: We rely on the window frame having been set to the correct size when this method is invoked.
			UIGraphics.BeginImageContextWithOptions(window.Bounds.Size, false, window.Screen.Scale);
			var ctx = UIGraphics.GetCurrentContext();

			// ctx will be null if the width/height of the view is zero
			if (ctx is not null && !TryRender(window, out _))
			{
				// TODO: test/handle this case
			}

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			var result = new ScreenshotResult(image);

			return Task.FromResult<IScreenshotResult>(result);
		}

		public Task<IScreenshotResult?> CaptureAsync(UIView view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			// NOTE: We rely on the view frame having been set to the correct size when this method is invoked.
			UIGraphics.BeginImageContextWithOptions(view.Bounds.Size, false, view.Window.Screen.Scale);
			var ctx = UIGraphics.GetCurrentContext();

			// ctx will be null if the width/height of the view is zero
			if (ctx is not null && !TryRender(view, out _))
			{
				// TODO: test/handle this case
			}

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			var result = image is null ? null : new ScreenshotResult(image);

			return Task.FromResult<IScreenshotResult?>(result);
		}

		public Task<IScreenshotResult?> CaptureAsync(CALayer layer, bool skipChildren)
		{
			_ = layer ?? throw new ArgumentNullException(nameof(layer));

			// NOTE: We rely on the layer frame having been set to the correct size when this method is invoked.
			UIGraphics.BeginImageContextWithOptions(layer.Bounds.Size, false, layer.RasterizationScale);
			var ctx = UIGraphics.GetCurrentContext();

			// ctx will be null if the width/height of the view is zero
			if (ctx is not null && !TryRender(layer, ctx, skipChildren, out _))
			{
				// TODO: test/handle this case
			}

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			var result = image is null ? null : new ScreenshotResult(image);

			return Task.FromResult<IScreenshotResult?>(result);
		}

		static bool TryRender(UIView view, out Exception? error)
		{
			try
			{
				view.DrawViewHierarchy(view.Bounds, afterScreenUpdates: true);

				error = null;
				return true;
			}
			catch (Exception e)
			{
				error = e;
				return false;
			}
		}

		static bool TryRender(CALayer layer, CGContext ctx, bool skipChildren, out Exception? error)
		{
			var visibilitySnapshot = new Dictionary<CALayer, bool>();

			try
			{
				if (skipChildren)
					HideSublayers(layer, visibilitySnapshot);

				layer.RenderInContext(ctx);

				error = null;
				return true;
			}
			catch (Exception e)
			{
				error = e;
				return false;
			}
			finally
			{
				if (skipChildren)
					RestoreSublayers(layer, visibilitySnapshot);
			}
		}

		static void HideSublayers(CALayer layer, Dictionary<CALayer, bool> visibilitySnapshot)
		{
			var sublayers = layer?.Sublayers;
			if (sublayers is null)
				return;

			foreach (var sublayer in sublayers)
			{
				HideSublayers(sublayer, visibilitySnapshot);

				visibilitySnapshot.Add(sublayer, sublayer.Hidden);
				sublayer.Hidden = true;
			}
		}

		static void RestoreSublayers(CALayer layer, Dictionary<CALayer, bool> visibilitySnapshot)
		{
			if (layer.Sublayers == null)
				return;

			foreach (var sublayer in visibilitySnapshot)
			{
				sublayer.Key.Hidden = sublayer.Value;
			}
		}
	}

	partial class ScreenshotResult
	{
		readonly UIImage bmp;

		internal ScreenshotResult(UIImage bmp)
			: base()
		{
			this.bmp = bmp;

			Width = (int)bmp.Size.Width;
			Height = (int)bmp.Size.Height;
		}

		Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality)
		{
			var data = format switch
			{
				ScreenshotFormat.Png => bmp.AsPNG(),
				ScreenshotFormat.Jpeg => bmp.AsJPEG(quality / 100.0f),
				_ => throw new ArgumentOutOfRangeException(nameof(format))
			};

			ArgumentNullException.ThrowIfNull(data);

			return Task.FromResult(data.AsStream());
		}

		Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
		{
			using var data = format switch
			{
				ScreenshotFormat.Png => bmp.AsPNG(),
				ScreenshotFormat.Jpeg => bmp.AsJPEG(quality / 100.0f),
				_ => throw new ArgumentOutOfRangeException(nameof(format))
			};

			ArgumentNullException.ThrowIfNull(data);

			using var result = data.AsStream();

			result.CopyTo(destination);

			return Task.CompletedTask;
		}

		Task<byte[]> PlatformToPixelBufferAsync()
		{
			var cgimage = bmp.CGImage!;
			var width = cgimage.Width;
			var height = cgimage.Height;

			var pixelData = new byte[height * width * 4];
			var gchandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
			var data = gchandle.AddrOfPinnedObject();
			try
			{
				var colorSpace = CGColorSpace.CreateDeviceRGB();
				var context = new CGBitmapContext(data, width, height, 8, 4 * width, colorSpace, CGImageAlphaInfo.PremultipliedLast);
				context.DrawImage(new CGRect(0, 0, width, height), cgimage);
			}
			finally
			{
				gchandle.Free();
			}

			return Task.FromResult(pixelData);
		}
	}
}
