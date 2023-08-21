// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp.Views.iOS;

namespace Microsoft.Maui.Graphics.Skia.Views
{
	public class SkiaGraphicsView : SKCanvasView
	{
		private IDrawable _drawable;
		private SkiaCanvas _canvas;
		private ScalingCanvas _scalingCanvas;

		public SkiaGraphicsView(IDrawable drawable = null)
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
			Drawable = drawable;
		}

		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				Invalidate();
			}
		}

		public void Invalidate()
		{
			if (Handle == IntPtr.Zero)
				return;

			SetNeedsDisplay();
		}

		protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			if (_drawable == null)
				return;

			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			var scale = (float)Window.Screen.Scale;
			_canvas.Canvas = skiaCanvas;

			_scalingCanvas.ResetState();
			_scalingCanvas.Scale(scale, scale);
			_drawable.Draw(_scalingCanvas, Bounds.AsRectangleF());
		}
	}
}
