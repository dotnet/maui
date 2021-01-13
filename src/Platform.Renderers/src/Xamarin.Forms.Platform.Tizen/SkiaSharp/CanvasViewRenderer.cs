using System;
using System.Diagnostics;
using ElmSharp;
using SkiaSharp;
using SkiaSharp.Views.Tizen;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public abstract class CanvasViewRenderer<TView, TNativeView> : ViewRenderer<TView, Native.Canvas>, IBackgroundCanvas, IClipperCanvas
		where TView : View
		where TNativeView : EvasObject
	{
		public TNativeView RealControl
		{
			get
			{
				return (TNativeView)RealNativeView;
			}
		}

		Lazy<SKCanvasView> _backgroundCanvas;

		Lazy<SKClipperView> _clipper;

		public SKCanvasView BackgroundCanvas => _backgroundCanvas.Value;

		public SKClipperView ClipperCanvas => _clipper.Value;

		public EvasObject RealNativeView { get; private set; }

		public CornerRadius CornerRadius { get; set; }

		public Geometry ClippingGeometry { get; set; }

		protected override void OnElementChanged(ElementChangedEventArgs<TView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.Canvas(Forms.NativeParent));
				Control.Show();
				Control.LayoutUpdated += OnLayout;
				Control.Children.Add(RealNativeView);
			}

			_backgroundCanvas = new Lazy<SKCanvasView>(() =>
			{
				var canvas = new SKCanvasView(Forms.NativeParent);
				canvas.PassEvents = true;
				canvas.PaintSurface += OnBackgroundPaint;
				canvas.Show();
				Control.Children.Add(canvas);
				canvas.Lower();
				RealNativeView?.RaiseTop();
				return canvas;
			});

			_clipper = new Lazy<SKClipperView>(() =>
			{
				var clipper = new SKClipperView(Forms.NativeParent);
				clipper.PassEvents = true;
				clipper.PaintSurface += OnClipperPaint;
				clipper.Show();
				Control.Children.Add(clipper);
				clipper.Lower();
				RealNativeView?.RaiseTop();
				return clipper;
			});

			base.OnElementChanged(e);
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();
			if (_backgroundCanvas.IsValueCreated)
			{
				BackgroundCanvas.Geometry = Control.Geometry;
			}
			if (_clipper.IsValueCreated)
			{
				ClipperCanvas.Geometry = Control.Geometry;
				ClipperCanvas.Invalidate();
			}

		}

		protected void SetRealNativeControl(TNativeView control)
		{
			Debug.Assert(control != null);
			RealNativeView = control;
			RealNativeView.Show();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.LayoutUpdated -= OnLayout;
				}
				if (_backgroundCanvas.IsValueCreated)
				{
					BackgroundCanvas.PaintSurface -= OnBackgroundPaint;
					BackgroundCanvas.Unrealize();
					_backgroundCanvas = null;
				}

				if (_clipper.IsValueCreated)
				{
					ClipperCanvas.PaintSurface -= OnClipperPaint;
					ClipperCanvas.Unrealize();
					_clipper = null;
				}
			}
			base.Dispose(disposing);
		}

		protected virtual void OnLayout(object sender, Native.LayoutEventArgs e)
		{
			RealControl.Geometry = Control.Geometry;
			if (_backgroundCanvas.IsValueCreated)
			{
				BackgroundCanvas.Geometry = Control.Geometry;
			}
			if (_clipper.IsValueCreated)
			{
				ClipperCanvas.Geometry = Control.Geometry;
				ClipperCanvas.Invalidate();
			}
		}

		protected virtual void OnBackgroundPaint(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear();

			var bounds = e.Info.Rect;
			var paint = Element.GetBackgroundPaint(bounds);

			if (paint != null)
			{
				using (paint)
				using (var path = bounds.ToRoundedRectPath(CornerRadius))
				{
					canvas.DrawPath(path, paint);
				}
			}
		}

		protected virtual void OnClipperPaint(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear();

			ClippingGeometry = Element.Clip;
			if (ClippingGeometry == null)
				return;

			using (var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.White
			})
			{
				canvas.DrawPath(ClippingGeometry.ToSKPath(), paint);
				RealControl?.SetClipperCanvas(ClipperCanvas);

				if (_backgroundCanvas.IsValueCreated)
				{
					BackgroundCanvas.SetClipperCanvas(ClipperCanvas);
				}
			}
		}
	}
}
