using System;
using System.Runtime.InteropServices;
using ElmSharp;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui.Graphics.Skia.Views;
using SkiaSharp.Views.Tizen;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using SkiaGraphicsView = Microsoft.Maui.Platform.Tizen.SkiaGraphicsView;

namespace Microsoft.Maui
{
	public interface IBackgroundCanvas
	{
		public SkiaGraphicsView BackgroundCanvas { get; }
	}

	public partial class WrapperView : Canvas, IBackgroundCanvas
	{
		Lazy<SkiaGraphicsView> _backgroundCanvas;
		Lazy<SKClipperView> _clipperView;
		EvasObject? _content;

		public WrapperView(EvasObject parent) : base(parent)
		{
			_backgroundCanvas = new Lazy<SkiaGraphicsView>(() =>
			{
				var view = new SkiaGraphicsView(parent)
				{
					DeviceScalingFactor = (float)DeviceInfo.ScalingFactor
				};
				view.Show();
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

		partial void ClipChanged()
		{
			_clipperView.Value.Invalidate();
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
			if (_backgroundCanvas.IsValueCreated)
			{
				BackgroundCanvas.SetClipperCanvas(_clipperView.Value);
			}
		}

		void OnLayout(object? sender, Tizen.UIExtensions.Common.LayoutEventArgs e)
		{
			if (Content != null)
			{
				Content.Geometry = Geometry;
			}

			if (_backgroundCanvas.IsValueCreated)
			{
				_backgroundCanvas.Value.Geometry = Geometry;
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

		public SkiaGraphicsView BackgroundCanvas => _backgroundCanvas.Value;
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
