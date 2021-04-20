using System.IO;
using d2 = SharpDX.Direct2D1;
using wic = SharpDX.WIC;

namespace Microsoft.Maui.Graphics.SharpDX
{
	public static class RenderTargetExtensions
	{
		public static d2.Bitmap LoadBitmap(this d2.RenderTarget renderTarget, Stream stream)
		{
			var bitmapDecoder = new wic.BitmapDecoder(DXGraphicsService.FactoryImaging, stream,
				wic.DecodeOptions.CacheOnDemand);

			wic.BitmapFrameDecode bitmapFrameDecode = bitmapDecoder.GetFrame(0);
			var bitmapSource = new wic.BitmapSource(bitmapFrameDecode.NativePointer);

			var formatConverter = new wic.FormatConverter(DXGraphicsService.FactoryImaging);
			formatConverter.Initialize(bitmapSource, wic.PixelFormat.Format32bppPBGRA);

			d2.Bitmap bitmap = d2.Bitmap.FromWicBitmap(renderTarget, formatConverter);

			formatConverter.Dispose();
			/* todo: check to see if I need to register to dispose of this later...  Can't comment this out because server side rendering will crash */
			//bitmapSource.Dispose();
			bitmapFrameDecode.Dispose();
			bitmapDecoder.Dispose();

			return bitmap;
		}
	}
}
