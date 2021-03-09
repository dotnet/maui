using System;
using System.ComponentModel;
using SkiaSharp.Views.Tizen;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class FastLayoutRenderer : ViewRenderer<Layout, Native.EvasFormsCanvas>, SkiaSharp.IBackgroundCanvas, ILayoutRenderer
	{
		bool _layoutUpdatedRegistered = false;

		Lazy<SKCanvasView> _backgroundCanvas;

		public SKCanvasView BackgroundCanvas => _backgroundCanvas.Value;

		public void RegisterOnLayoutUpdated()
		{
			if (!_layoutUpdatedRegistered)
			{
				Control.LayoutUpdated += OnLayoutUpdated;
				_layoutUpdatedRegistered = true;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			if (null == Control)
			{
				SetNativeControl(new Native.EvasFormsCanvas(Forms.NativeParent));
			}

			if (Forms.UseSkiaSharp)
			{
				Control.LayoutUpdated += OnBackgroundLayoutUpdated;
				_backgroundCanvas = new Lazy<SKCanvasView>(() =>
				{
					var canvas = new SKCanvasView(Forms.NativeParent);
					canvas.PassEvents = true;
					canvas.PaintSurface += OnBackgroundPaint;
					canvas.Show();
					Control.Children.Add(canvas);
					canvas.Lower();
					return canvas;
				});
			}
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Layout.CascadeInputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent(false);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_layoutUpdatedRegistered)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
					_layoutUpdatedRegistered = false;
				}

				if (Forms.UseSkiaSharp)
				{
					Control.LayoutUpdated -= OnBackgroundLayoutUpdated;

					if (_backgroundCanvas.IsValueCreated)
					{
						BackgroundCanvas.PaintSurface -= OnBackgroundPaint;
						BackgroundCanvas.Unrealize();
						_backgroundCanvas = null;
					}
				}
			}
			base.Dispose(disposing);
		}

		protected override void UpdateInputTransparent(bool initialize)
		{
			if (initialize && Element.InputTransparent == default(bool))
			{
				return;
			}

			if (Element.InputTransparent)
			{
				if (Element.CascadeInputTransparent)
				{
					//Ignore all events of both layout and it's chidren
					NativeView.PassEvents = true;
				}
				else
				{
					//Ignore Layout's event only. Children's events should be allowded.
					NativeView.PassEvents = false;
					NativeView.RepeatEvents = true;
				}
			}
			else
			{
				//Allow layout's events and children's events would be determined by CascadeInputParent.
				NativeView.PassEvents = false;
				NativeView.RepeatEvents = false;
			}

			if (GestureDetector != null)
			{
				GestureDetector.InputTransparent = Element.InputTransparent;
			}
		}

		protected override void UpdateLayout()
		{
			if (!_layoutUpdatedRegistered)
			{
				base.UpdateLayout();
			}
			else
			{
				ApplyTransformation();
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
				using (var path = bounds.ToPath())
				{
					canvas.DrawPath(path, paint);
				}
			}
		}

		protected virtual void OnBackgroundLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			if (_backgroundCanvas.IsValueCreated)
			{
				BackgroundCanvas.Geometry = Control.Geometry;
			}
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			Element.Layout(e.Geometry.ToDP());
		}
	}
}
