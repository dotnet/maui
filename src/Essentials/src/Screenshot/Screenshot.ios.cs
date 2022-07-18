#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
		public bool IsCaptureSupported =>
			true;

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.0")]
		public Task<IScreenshotResult> CaptureAsync()
		{
			var scenes = UIApplication.SharedApplication.ConnectedScenes;
			//#pragma warning disable CA1416 // Known false positive with Lambda expression
			var currentScene = scenes.ToArray().Where(n => n.ActivationState == UISceneActivationState.ForegroundActive).FirstOrDefault();
			//#pragma warning restore CA1416
			if (currentScene == null)
				throw new InvalidOperationException("Unable to find current scene.");

			var uiWindowScene = currentScene as UIWindowScene;
			if (uiWindowScene == null)
				throw new InvalidOperationException("Unable to find current window scene.");

			var currentWindow = uiWindowScene.Windows.FirstOrDefault(n => n.IsKeyWindow);
			if (currentWindow == null)
				throw new InvalidOperationException("Unable to find current window.");

			return CaptureAsync(currentWindow.Layer, true);
		}

		public Task<IScreenshotResult> CaptureAsync(UIWindow window)
		{
			_ = window ?? throw new ArgumentNullException(nameof(window));

			// NOTE: We rely on the window frame having been set to the correct size when this method is invoked.
			UIGraphics.BeginImageContextWithOptions(window.Bounds.Size, false, window.Screen.Scale);
			var ctx = UIGraphics.GetCurrentContext();

			if (!TryRender(window, out var error))
			{
				// FIXME: test/handle this case
			}

			// Render the status bar with the correct frame size
			try
			{
				TryHideStatusClockView(UIApplication.SharedApplication);
				var statusbarWindow = GetStatusBarWindow(UIApplication.SharedApplication);
				if (statusbarWindow != null/* && metrics.StatusBar != null*/)
				{
					statusbarWindow.Frame = window.Frame;
					statusbarWindow.Layer.RenderInContext(ctx);
				}
			}
			catch
			{
				// FIXME: test/handle this case
			}

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			var result = new ScreenshotResult(image);

			return Task.FromResult<IScreenshotResult>(result);
		}

		public Task<IScreenshotResult> CaptureAsync(UIView view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			// NOTE: We rely on the view frame having been set to the correct size when this method is invoked.
			UIGraphics.BeginImageContextWithOptions(view.Bounds.Size, false, view.Window.Screen.Scale);
			var ctx = UIGraphics.GetCurrentContext();

			// ctx will be null if the width/height of the view is zero
			if (ctx != null)
			{
				if (!TryRender(view, out var error))
				{
					// FIXME: test/handle this case
				}
			}

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			var result = new ScreenshotResult(image);

			return Task.FromResult<IScreenshotResult>(result);
		}

		public Task<IScreenshotResult> CaptureAsync(CALayer layer, bool skipChildren)
		{
			_ = layer ?? throw new ArgumentNullException(nameof(layer));

			// NOTE: We rely on the layer frame having been set to the correct size when this method is invoked.
			UIGraphics.BeginImageContextWithOptions(layer.Bounds.Size, false, layer.RasterizationScale);
			var ctx = UIGraphics.GetCurrentContext();

			// ctx will be null if the width/height of the view is zero
			if (ctx != null)
			{
				if (!TryRender(layer, ctx, skipChildren, out var error))
				{
					// FIXME: test/handle this case
				}
			}

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			var result = new ScreenshotResult(image);

			return Task.FromResult<IScreenshotResult>(result);
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
			if (layer.Sublayers == null)
				return;

			foreach (var sublayer in layer.Sublayers)
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

		static void TryHideStatusClockView(UIApplication app)
		{
			var statusBarWindow = GetStatusBarWindow(app);
			if (statusBarWindow == null)
				return;

			var clockView = GetClockView(statusBarWindow);
			if (clockView != null)
				clockView.Hidden = true;
		}

		static UIView? GetClockView(UIWindow window)
		{
			var classNames = new[] {
				"UIStatusBar",
				"UIStatusBarForegroundView",
				"UIStatusBarTimeItemView"
			};

			return FindSubview(window, ((IEnumerable<string>)classNames).GetEnumerator());

			static UIView? FindSubview(UIView view, IEnumerator<string> classNames)
			{
				if (!classNames.MoveNext())
					return view;

				foreach (var subview in view.Subviews)
				{
					if (subview.ToString().StartsWith($"<{classNames.Current}:", StringComparison.Ordinal))
						return FindSubview(subview, classNames);
				}

				return null;
			}
		}

		static UIWindow? GetStatusBarWindow(UIApplication app)
		{
			if (!app.RespondsToSelector(statusBarWindowSelector))
				return null;

			var ptr = IntPtr_objc_msgSend(app.Handle, statusBarWindowSelector.Handle);
			return ptr != IntPtr.Zero ? Runtime.GetNSObject(ptr) as UIWindow : null;
		}

		static readonly Selector statusBarWindowSelector = new Selector("statusBarWindow");

		[DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
		static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);
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

			var result = data.AsStream();

			return Task.FromResult(result);
		}

		Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
		{
			using var data = format switch
			{
				ScreenshotFormat.Png => bmp.AsPNG(),
				ScreenshotFormat.Jpeg => bmp.AsJPEG(quality / 100.0f),
				_ => throw new ArgumentOutOfRangeException(nameof(format))
			};

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
