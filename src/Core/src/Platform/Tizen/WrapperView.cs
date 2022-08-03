using System;
using System.Runtime.InteropServices;
using ElmSharp;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui.Graphics.Skia.Views;
using SkiaSharp.Views.Tizen;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : Canvas
	{
		Lazy<SkiaGraphicsView> _drawableCanvas;
		Lazy<SKClipperView> _clipperView;
		EvasObject? _content;

		public WrapperView(EvasObject parent) : base(parent)
		{
			_drawableCanvas = new Lazy<SkiaGraphicsView>(() =>
			{
				var view = new SkiaGraphicsView(parent)
				{
					IgnorePixelScaling = true,
					Drawable = new MauiDrawable(),
					PassEvents = true
				};
				view.Show();
				Children.Add(view);
				view.Lower();
				Content?.RaiseTop();
				return view;
			});

			_clipperView = new Lazy<SKClipperView>(() =>
			{
				var clipper = new SKClipperView(parent)
				{
					PassEvents = true
				};
				clipper.DrawClip += OnClipPaint;
				clipper.Show();
				clipper.DeviceScalingFactor = (float)DeviceInfo.ScalingFactor;
				Children.Add(clipper);
				clipper.Lower();
				Content?.RaiseTop();
				return clipper;
			});

			LayoutUpdated += OnLayout;
		}

		public void UpdateBackground(Paint? paint)
		{
			UpdateDrawableCanvas(paint);
		}

		public void UpdateShape(IShape? shape)
		{
			UpdateDrawableCanvas(shape);
		}

		public void UpdateBorder(IBorderStroke border)
		{
			((MauiDrawable)_drawableCanvas.Value.Drawable).Border = border;
			UpdateShape(border.Shape);
		}

		partial void ShadowChanged()
		{
			if (!_drawableCanvas.IsValueCreated && Shadow is null)
				return;

			((MauiDrawable)_drawableCanvas.Value.Drawable).Shadow = Shadow;

			if (Shadow != null)
			{
				_drawableCanvas.Value.SetClip(null);
			}
			UpdateDrawableCanvas(true);
		}

		partial void ClipChanged()
		{
			if (_drawableCanvas.IsValueCreated || Clip is not null)
			{
				((MauiDrawable)_drawableCanvas.Value.Drawable).Clip = Clip;
				UpdateDrawableCanvas(false);
			}

			if (_clipperView.IsValueCreated || Clip is not null)
				_clipperView.Value.Invalidate();
		}

		void UpdateDrawableCanvas(Paint? paint)
		{
			if (_drawableCanvas.IsValueCreated || paint is not null)
			{
				((MauiDrawable)_drawableCanvas.Value.Drawable).Background = paint;
				_drawableCanvas.Value.Invalidate();
			}
		}

		void UpdateDrawableCanvas(IShape? shape)
		{
			if (_drawableCanvas.IsValueCreated || shape is not null)
			{
				((MauiDrawable)_drawableCanvas.Value.Drawable).Shape = shape;
				_drawableCanvas.Value.Invalidate();
			}
		}

		void UpdateDrawableCanvas(bool isShadowUpdated)
		{
			if (isShadowUpdated)
			{
				UpdateDrawableCanvasGeometry();
			}
			_drawableCanvas.Value.Invalidate();
		}

		void OnClipPaint(object? sender, DrawClipEventArgs e)
		{
			var canvas = e.Canvas;
			var width = e.DirtyRect.Width;
			var height = e.DirtyRect.Height;

			canvas.FillColor = Colors.Transparent;
			canvas.FillRectangle(e.DirtyRect);

			canvas.FillColor = Colors.White;
			var clipPath = Clip?.PathForBounds(new Rect(0, 0, width, height)) ?? null;
			if (clipPath == null)
			{
				return;
			}
			canvas.FillPath(clipPath);
			Content?.SetClipperCanvas(_clipperView.Value);
			if (_drawableCanvas.IsValueCreated)
			{
				_drawableCanvas.Value.SetClipperCanvas(_clipperView.Value);
			}
		}

		void OnLayout(object? sender, LayoutEventArgs e)
		{
			if (Content != null)
			{
				Content.Geometry = Geometry;
			}

			if (_drawableCanvas.IsValueCreated)
			{
				UpdateDrawableCanvas(true);
			}

			if (_clipperView.IsValueCreated)
			{
				_clipperView.Value.Geometry = Geometry;
				_clipperView.Value.Invalidate();
				if (Shadow != null)
				{
					_drawableCanvas.Value.SetClip(null);
				}
			}
		}

		public EvasObject? Content
		{
			get => _content;
			set
			{
				if (_content != value)
				{
					if (_content != null)
					{
						Children.Remove(_content);
						_content = null;
					}
					_content = value;
					if (_content != null)
					{
						Children.Add(_content);
						_content.RaiseTop();
					}
				}
			}
		}

		void UpdateDrawableCanvasGeometry()
		{
			if (_drawableCanvas.IsValueCreated)
			{
				var shadowMargin = GetShadowMargin(Shadow);
				_drawableCanvas.Value.UpdateBounds(Geometry.ToDP().ExpandTo(shadowMargin).ToPixel());
				((MauiDrawable)_drawableCanvas.Value.Drawable).ShadowThickness = shadowMargin;
			}
		}

		Thickness GetShadowMargin(IShadow? shadow)
		{
			double left = 0;
			double top = 0;
			double right = 0;
			double bottom = 0;

			var offsetX = shadow == null ? 0 : shadow.Offset.X;
			var offsetY = shadow == null ? 0 : shadow.Offset.Y;
			var blurRadius = shadow == null ? 0 : ((double)shadow.Radius);
			var spreadSize = blurRadius * 3;
			var spreadLeft = offsetX - spreadSize;
			var spreadRight = offsetX + spreadSize;
			var spreadTop = offsetY - spreadSize;
			var spreadBottom = offsetY + spreadSize;
			if (left > spreadLeft)
				left = spreadLeft;
			if (top > spreadTop)
				top = spreadTop;
			if (right < spreadRight)
				right = spreadRight;
			if (bottom < spreadBottom)
				bottom = spreadBottom;

			return new Thickness(Math.Abs(left), Math.Abs(top), Math.Abs(right), Math.Abs(bottom));
		}
	}

	public class DrawClipEventArgs : EventArgs
	{
		public DrawClipEventArgs(ICanvas canvas, RectF dirtyRect)
		{
			Canvas = canvas;
			DirtyRect = dirtyRect;
		}

		public ICanvas Canvas { get; set; }

		public RectF DirtyRect { get; set; }
	}

	public class SKClipperView : SKCanvasView
	{
		private SkiaCanvas _canvas;
		private ScalingCanvas _scalingCanvas;

		public SKClipperView(EvasObject parent) : base(parent)
		{
			_canvas = new SkiaCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);
			PaintSurface += OnPaintSurface;
		}

		public float DeviceScalingFactor { get; set; }
		public bool ClippingRequired { get; set; }
		public event EventHandler<DrawClipEventArgs>? DrawClip;

		public new void Invalidate()
		{
			ClippingRequired = true;
			OnDrawFrame();
			ClippingRequired = false;
		}

		protected virtual void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
		{
			var skiaCanvas = e.Surface.Canvas;
			skiaCanvas.Clear();

			_canvas.Canvas = skiaCanvas;
			_scalingCanvas.ResetState();

			float width = e.Info.Width;
			float height = e.Info.Height;
			if (DeviceScalingFactor > 0)
			{
				width = width / DeviceScalingFactor;
				height = height / DeviceScalingFactor;
			}

			_scalingCanvas.SaveState();

			if (DeviceScalingFactor > 0)
				_scalingCanvas.Scale(DeviceScalingFactor, DeviceScalingFactor);
			DrawClip?.Invoke(this, new DrawClipEventArgs(_scalingCanvas, new RectF(0, 0, width, height)));
			_scalingCanvas.RestoreState();
		}
	}

	public static class ClipperExtension
	{
		public static void SetClipperCanvas(this EvasObject target, SKClipperView clipper)
		{
			if (target != null && clipper.ClippingRequired)
			{
				var realHandle = elm_object_part_content_get(clipper, "elm.swallow.content");

				target.SetClip(null); // To restore original image
				evas_object_clip_set(target, realHandle);
			}
		}

		[DllImport("libevas.so.1")]
		internal static extern void evas_object_clip_set(IntPtr obj, IntPtr clip);

		[DllImport("libelementary.so.1")]
		internal static extern IntPtr elm_object_part_content_get(IntPtr obj, string part);
	}
}
