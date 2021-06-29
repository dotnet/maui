using System;
using System.Runtime.InteropServices;
using ElmSharp;
using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui.Graphics.Skia.Views;
using SkiaSharp;
using SkiaSharp.Views.Tizen;
using Tizen.UIExtensions.ElmSharp;

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
				var view = new SkiaGraphicsView(parent);
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
				clipper.PaintSurface += OnClipPaint;
				clipper.Show();
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

		void OnClipPaint(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear();

			var width = e.Info.Width;
			var height = e.Info.Height;

			var clipPath = Clip?.PathForBounds(new Graphics.Rectangle(0, 0, width, height)) ?? null;
			if (clipPath == null)
			{
				canvas.Clear(SKColors.White);
				return;
			}

			using (var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				Color = SKColors.White
			})
			{
				canvas.DrawPath(clipPath.AsSkiaPath(), paint);
				Content?.SetClipperCanvas(_clipperView.Value);
				if (_backgroundCanvas.IsValueCreated)
				{
					BackgroundCanvas.SetClipperCanvas(_clipperView.Value);
				}
			}
		}

		void OnLayout(object sender, Tizen.UIExtensions.Common.LayoutEventArgs e)
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

	/// <summary>
	/// A clipper area drawing view, it used for clipping
	/// </summary>
	public class SKClipperView : SKCanvasView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SKClipperView"/> class.
		/// </summary>
		/// <param name="parent">Parent of this instance.</param>
		public SKClipperView(EvasObject parent) : base(parent) { }

		public bool ClippingRequired { get; set; }

		/// <summary>
		/// Invalidate clipping area
		/// </summary>
		public new void Invalidate()
		{
			ClippingRequired = true;
			OnDrawFrame();
			ClippingRequired = false;
		}
	}

	public static class ClipperExtension
	{
		/// <summary>
		/// Set Clipper canvas
		/// </summary>
		/// <param name="target">A target view to clip</param>
		/// <param name="clipper">A clip area</param>
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
