using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public class MauiClipperView : SKClipperView
	{
		IShape? _clip;
		SkiaCanvas _canvas;
		ScalingCanvas _scalingCanvas;

		public MauiClipperView()
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
			DrawClippingArea += OnDrawClippingArea;
		}

		public IShape? Clip
		{
			get => _clip;
			set
			{
				_clip = value;
				Invalidate();
			}
		}

		void OnDrawClippingArea(object? sender, SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs e)
		{

			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			if (Clip == null)
			{
				skiaCanvas.Clear(SkiaSharp.SKColors.White);
				return;
			}

			var width = (float)(e.Info.Width / DeviceInfo.ScalingFactor);
			var height = (float)(e.Info.Height / DeviceInfo.ScalingFactor);

			_canvas.Canvas = skiaCanvas;
			_scalingCanvas.SaveState();
			_scalingCanvas.Scale((float)DeviceInfo.ScalingFactor, (float)DeviceInfo.ScalingFactor);

			_scalingCanvas.FillColor = Colors.White;
			var clipPath = Clip.PathForBounds(new Rect(0, 0, width, height));
			_scalingCanvas.FillPath(clipPath);
			_scalingCanvas.RestoreState();
		}
	}
}