using ElmSharp;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class FrameRenderer : ViewRenderer<Frame, Native.Canvas>
	{
		const int _thickness = 2;
		const int _shadow_shift = 2;
		const int _shadow_thickness = _thickness + 2;

		static readonly EColor s_DefaultColor = EColor.Black;
		static readonly EColor s_ShadowColor = EColor.FromRgba(80, 80, 80, 50);

		Polygon _shadow = null;
		Polygon _frame = null;

		public FrameRenderer()
		{
			RegisterPropertyHandler(Frame.BorderColorProperty, UpdateColor);
			RegisterPropertyHandler(Frame.HasShadowProperty, UpdateShadowVisibility);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.Canvas(Forms.NativeParent));

				_shadow = new Polygon(NativeView);
				_shadow.Color = s_ShadowColor;
				Control.Children.Add(_shadow);

				_frame = new Polygon(NativeView);
				_frame.Show();
				Control.Children.Add(_frame);
				Control.LayoutUpdated += OnLayoutUpdated;
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
				}
			}
			base.Dispose(disposing);
		}

		static void DrawFrame(Polygon frame, int left, int top, int right, int bottom, int thickness)
		{
			frame.ClearPoints();
			if (left + thickness >= right || top + thickness >= bottom)
			{
				if (left >= right || top >= bottom)
					return;
				// shape reduces to a rectangle
				frame.AddPoint(left, top);
				frame.AddPoint(right, top);
				frame.AddPoint(right, bottom);
				frame.AddPoint(left, bottom);
				return;
			}
			// outside edge
			frame.AddPoint(left, top);
			frame.AddPoint(right, top);
			frame.AddPoint(right, bottom);
			frame.AddPoint(left, bottom);
			frame.AddPoint(left, top + thickness);
			// and inside edge
			frame.AddPoint(left + thickness, top + thickness);
			frame.AddPoint(left + thickness, bottom - thickness);
			frame.AddPoint(right - thickness, bottom - thickness);
			frame.AddPoint(right - thickness, top + thickness);
			frame.AddPoint(left, top + thickness);
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			UpdateGeometry();
		}

		void UpdateGeometry()
		{
			var geometry = NativeView.Geometry;
			DrawFrame(_frame,
				geometry.X,
				geometry.Y,
				geometry.Right,
				geometry.Bottom,
				_thickness
			);
			DrawFrame(_shadow,
				geometry.X + _shadow_shift,
				geometry.Y + _shadow_shift,
				geometry.Right - _thickness + _shadow_shift + _shadow_thickness,
				geometry.Bottom - _thickness + _shadow_shift + _shadow_thickness,
				_shadow_thickness
			);
		}

		void UpdateColor()
		{
			if (Element.BorderColor.IsDefault)
				_frame.Color = s_DefaultColor;
			else
				_frame.Color = Element.BorderColor.ToNative();
		}

		void UpdateShadowVisibility()
		{
			if (Element.HasShadow)
				_shadow.Show();
			else
				_shadow.Hide();
		}
	}
}
