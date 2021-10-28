using System;
using System.Runtime.InteropServices;
using ElmSharp;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui.Graphics.Skia.Views;
using SkiaSharp.Views.Tizen;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public partial class WrapperView : Canvas, IWrapperViewCanvas
	{
		Lazy<SkiaGraphicsView> _drawableCanvas;
		Lazy<SKClipperView> _clipperView;
		EvasObject? _content;
		IShape? _shape;

		public WrapperView(EvasObject parent) : base(parent)
		{
			_drawableCanvas = new Lazy<SkiaGraphicsView>(() =>
			{
				var view = new SkiaGraphicsView(parent)
				{
					IgnorePixelScaling = true
				};
				var _drawables = new WrapperViewDrawables();
				_drawables.Invalidated += (s, e) =>
				{
					view.Invalidate();
				};
				view.Drawable = _drawables;
				view.Show();
				view.PassEvents = true;
				Children.Add(view);
				view.Lower();
				Content?.RaiseTop();
				return view;
			});

			_clipperView = new Lazy<SKClipperView>(() =>
			{
				var clipper = new SKClipperView(parent);
				clipper.PassEvents = true;
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
			if (paint == null)
			{
				Drawables.BackgroundDrawable = null;
			}
			else
			{
				if (Drawables.BackgroundDrawable == null)
				{
					Drawables.BackgroundDrawable = paint.ToDrawable(GetBoundaryPath());
				}
				else
				{
					(Drawables.BackgroundDrawable as BackgroundDrawable)!.UpdatePaint(paint);
				}
			}
			_drawableCanvas.Value.Invalidate();
		}

		public void UpdateShape(IShape? shape)
		{
			_shape = shape;
			UpdateDrawableCanvas(false);
		}

		partial void ShadowChanged()
		{
			if (Shadow == null)
			{
				Drawables.ShadowDrawable = null;
				return;
			}

			if (Drawables.ShadowDrawable == null)
			{
				Drawables.ShadowDrawable = new ShadowDrawable(Shadow, GetBoundaryPath());
				_drawableCanvas.Value.SetClip(null);
			}
			UpdateDrawableCanvas(true);
		}

		partial void ClipChanged()
		{
			_clipperView.Value.Invalidate();
			UpdateDrawableCanvas(false);
		}

		void UpdateDrawableCanvas(bool isShadowUpdated)
		{
			if (isShadowUpdated)
			{
				UpdateDrawableCanvasGeometry();
			}
			UpdateDrawables();
			_drawableCanvas.Value.Invalidate();
		}

		void UpdateDrawables()
		{
			var path = GetBoundaryPath();
			if (Shadow != null)
			{
				(Drawables.ShadowDrawable as ShadowDrawable)?.UpdateShadow(Shadow, path);
			}
			(Drawables.BackgroundDrawable as BackgroundDrawable)?.UpdatePath(path);
		}

		void OnClipPaint(object? sender, DrawClipEventArgs e)
		{
			var canvas = e.Canvas;
			var width = e.DirtyRect.Width;
			var height = e.DirtyRect.Height;

			canvas.FillColor = Colors.Transparent;
			canvas.FillRectangle(e.DirtyRect);

			canvas.FillColor = Colors.White;

			var clipPath = Clip?.PathForBounds(new Graphics.Rectangle(0, 0, width, height)) ?? null;
			if (clipPath == null)
			{
				canvas.FillRectangle(e.DirtyRect);
				return;
			}

			canvas.FillPath(clipPath);
			Content?.SetClipperCanvas(_clipperView.Value);
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

		public IWrapperViewDrawables Drawables => (IWrapperViewDrawables)_drawableCanvas.Value.Drawable;

		void UpdateDrawableCanvasGeometry()
		{
			if (_drawableCanvas.IsValueCreated)
			{
				_drawableCanvas.Value.Geometry = Geometry.ExpandTo(Shadow);
			}
		}

		PathF GetBoundaryPath()
		{
			var drawableGeometry = _drawableCanvas.Value.Geometry;
			var left = Geometry.Left - drawableGeometry.Left;
			var top = Geometry.Top - drawableGeometry.Top;
			var width = Geometry.Width;
			var height = Geometry.Height;
			var bounds = new Tizen.UIExtensions.Common.Rect(left, top, width, height).ToDP();

			if (Clip != null)
			{
				var clipPath = Clip.PathForBounds(bounds);
				clipPath.Move((float)bounds.Left, (float)bounds.Top);
				return clipPath;
			}

			if (_shape != null)
			{
				return _shape.PathForBounds(bounds);
			}

			var path = new PathF();
			path.AppendRectangle(bounds);
			return path;
		}
	}

	public class DrawClipEventArgs : EventArgs
	{
		public DrawClipEventArgs(ICanvas canvas, RectangleF dirtyRect)
		{
			Canvas = canvas;
			DirtyRect = dirtyRect;
		}

		public ICanvas Canvas { get; set; }

		public RectangleF DirtyRect { get; set; }
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
			DrawClip?.Invoke(this, new DrawClipEventArgs(_scalingCanvas, new RectangleF(0, 0, width, height)));
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
