using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Microsoft.Maui
{
	public static class VisualDiagnosticsWindowsExtensions
	{
		internal static async Task<byte[]?> RenderAsPng(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return null;

			if (nativeView is not UIElement uiElement)
				return null;

			try
			{
				var bitmap = new RenderTargetBitmap();
				await bitmap.RenderAsync(uiElement);
				var pixelBuffer = await bitmap.GetPixelsAsync();
				using var memoryStream = new InMemoryRandomAccessStream();
				BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
				enc.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 1, 1, pixelBuffer.ToArray());
				await enc.FlushAsync();
				await memoryStream.FlushAsync();
				var stream = memoryStream.AsStream();
				byte[] result = new byte[stream.Length];
				stream.Read(result, 0, result.Length);
				return result;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
