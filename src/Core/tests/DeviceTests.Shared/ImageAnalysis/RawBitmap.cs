using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
#elif IOS
using CoreGraphics;
using CoreImage;
using UIKit;
using Microsoft.Maui.ApplicationModel;
#endif

namespace Microsoft.Maui.DeviceTests.ImageAnalysis
{
	public class RawBitmap
	{
		public int PixelHeight { get; internal set; }
		public int PixelWidth { get; internal set; }
		public double Height => PixelHeight / Density;
		public double Width => PixelWidth / Density;
		public byte[] PixelBuffer { get; internal set; }
		public double Density { get; internal set; }
	}

	public static partial class RawBitmapExtensions
	{
		public static async Task<RawBitmap> AsRawBitmapAsync(this VisualElement element)
		{
			TaskCompletionSource load = new();
			element.Loaded += (s, e) => load.TrySetResult();
			if (element.IsLoaded)
				load.TrySetResult();
			await load.Task;
			TaskCompletionSource<IViewHandler> tcs = new();
			element.HandlerChanged += (s, e) => { if (element.Handler != null) tcs.TrySetResult(element.Handler); };
			if (element.Handler is not null)
				tcs.TrySetResult(element.Handler);
			var getHandler = await tcs.Task;
			var platformView = getHandler.PlatformView;
#if WINDOWS
			return await CaptureView(platformView as UIElement);
#elif ANDROID
			return await CaptureView(platformView as Android.Views.View);
#elif IOS
			return await CaptureView(platformView as UIKit.UIView);
#else
			throw new PlatformNotSupportedException("TODO");
#endif
		}

#if WINDOWS
		private static async Task<RawBitmap> CaptureView(UIElement view)
		{
			var renderTargetBitmap = new RenderTargetBitmap();
			await renderTargetBitmap.RenderAsync(view);
			var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
			var pixels = pixelBuffer.ToArray();
			var width = (int)renderTargetBitmap.PixelWidth;
			var height = (int)renderTargetBitmap.PixelHeight;

			return new RawBitmap
			{
				PixelBuffer = pixels,
				PixelHeight = height,
				PixelWidth = width,
				Density = view.XamlRoot.RasterizationScale
			};
		}
#elif IOS
		private static async Task<RawBitmap> CaptureView(UIView view)
		{
			await Task.Delay(1000); // TODO: Wait for UI to render
			var rect = view.Bounds;
			var scale = UIScreen.MainScreen.Scale;
			int width = (int)(rect.Width * scale);
			int height = (int)(rect.Height * scale);

			return await MainThread.InvokeOnMainThreadAsync(() =>
			{
				var renderer = new UIGraphicsImageRenderer(rect.Size, new UIGraphicsImageRendererFormat()
				{
					Opaque = false,
					Scale = UIScreen.MainScreen.Scale,
				});

				using var image = renderer.CreateImage((context) =>
				{
					using var colorSpace = CGColorSpace.CreateDeviceRGB();
					view.Layer.RenderInContext(context.CGContext);
				});
				var cgimage = image.CGImage;
				var buffer = cgimage.DataProvider.CopyData();
				var pixelBuffer = buffer.ToArray();
				var bytesPerPixel = cgimage.BitsPerPixel / 8;
				if (cgimage.ByteOrderInfo != CGImageByteOrderInfo.ByteOrder32Little)
					throw new NotImplementedException($"ByteOrderInfo CGImageByteOrderInfo.{cgimage.ByteOrderInfo} not implemented");

				byte[] rawData = new byte[(int)(cgimage.Width * cgimage.Height * 4)];
				int index = 0;
				for (int i = 0; i < cgimage.Height; i++)
				{
					var a = i * cgimage.BytesPerRow;
					for (int j = 0; j < cgimage.Width; j++)
					{
						rawData[index++] = pixelBuffer[a + j * bytesPerPixel + 0]; //B
						rawData[index++] = pixelBuffer[a + j * bytesPerPixel + 1]; //G
						rawData[index++] = pixelBuffer[a + j * bytesPerPixel + 2]; //R
						rawData[index++] = pixelBuffer[a + j * bytesPerPixel + 3]; //A
					}
				}
				return new RawBitmap()
				{
					PixelBuffer = rawData,
					PixelWidth = width,
					PixelHeight = height,
					Density = scale
				};
			});
		}
#elif ANDROID
		private static async Task<RawBitmap> CaptureView(Android.Views.View view)
		{
#pragma warning disable CS0618 // Obsolete			
			while (!AndroidX.Core.View.ViewCompat.IsLaidOut(view))
				await Task.Delay(10); // Wait for Android to render the view
#pragma warning restore CS0618 // Obsolete				
			var bitmap = Android.Graphics.Bitmap.CreateBitmap(view.Width, view.Height, Android.Graphics.Bitmap.Config.Argb8888);
			Android.Graphics.Canvas canvas = new(bitmap);
			view.Draw(canvas);
			int[] pixels = new int[bitmap.Width * bitmap.Height];
			bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
			return new RawBitmap
			{
				PixelBuffer = pixels.SelectMany(p => BitConverter.GetBytes(p)).ToArray(),
				PixelHeight = bitmap.Height,
				PixelWidth = bitmap.Width,
				Density = bitmap.Density / 160d
			};
		}
		/*
		[System.Runtime.Versioning.SupportedOSPlatform("android26.0")]
		private static Task<BitmapSource> CaptureView2(Android.Views.View view)
		{
			var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
			var loc = new int[2];
			view.GetLocationInWindow(loc);
			var bitmap = Android.Graphics.Bitmap.CreateBitmap(view.Width, view.Height, Android.Graphics.Bitmap.Config.Argb8888);
			OnPixelCopyFinishedListener listener = new OnPixelCopyFinishedListener(view);
			Android.Views.PixelCopy.Request(activity.Window, new Android.Graphics.Rect(loc[0], loc[1], view.Width + loc[0], view.Height + loc[1]), bitmap, listener, view.Handler);
			return listener.Task;
		}
		private class OnPixelCopyFinishedListener : Java.Lang.Object, Android.Views.PixelCopy.IOnPixelCopyFinishedListener
		{
			private readonly TaskCompletionSource<BitmapSource> _tcs = new();
			private readonly Android.Views.View _view;
			public OnPixelCopyFinishedListener(Android.Views.View view)
			{
				_view = view;
			}
			public Task<BitmapSource> Task => _tcs.Task;
			void Android.Views.PixelCopy.IOnPixelCopyFinishedListener.OnPixelCopyFinished(int copyResult)
			{
				if (copyResult == 0)
				{
					var bitmap = Android.Graphics.Bitmap.CreateBitmap(_view.Width, _view.Height, Android.Graphics.Bitmap.Config.Argb8888);
					int[] pixels = new int[bitmap.Width * bitmap.Height];
					bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
					_tcs.TrySetResult(new BitmapSource
					{
						PixelBuffer = pixels.SelectMany(p => BitConverter.GetBytes(p)).ToArray(),
						PixelHeight = bitmap.Height,
						PixelWidth = bitmap.Width,
						Density = bitmap.Density / 160d
					});
				}
				else
				{
					_tcs.TrySetException(new Exception($"Failed to copy pixels: {copyResult}"));
				}
			}
		}*/
#endif
	}
}