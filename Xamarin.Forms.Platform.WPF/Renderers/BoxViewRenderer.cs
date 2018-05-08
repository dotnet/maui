using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using WRectangle = System.Windows.Shapes.Rectangle;

namespace Xamarin.Forms.Platform.WPF
{
	public class BoxViewRenderer : ViewRenderer<BoxView, WRectangle>
	{
		Border _border;

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // Construct and SetNativeControl and suscribe control event
				{
					WRectangle rectangle = new WRectangle();

					_border = new Border();

					VisualBrush visualBrush = new VisualBrush
					{
						Visual = _border
					};

					rectangle.Fill = visualBrush;

					SetNativeControl(rectangle);
				}

				UpdateColor();
				UpdateCornerRadius();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
		}

		protected override void UpdateNativeWidget()
		{
			base.UpdateNativeWidget();

			UpdateSize();
		}

		void UpdateColor()
		{
			Color color = Element.Color != Color.Default ? Element.Color : Element.BackgroundColor;
			_border.UpdateDependencyColor(Border.BackgroundProperty, color);
		}

		void UpdateCornerRadius()
		{
			var cornerRadius = Element.CornerRadius;
			_border.CornerRadius = new System.Windows.CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomRight, cornerRadius.BottomLeft);
		}

		void UpdateSize()
		{
			_border.Height = Element.Height > 0 ? Element.Height : Double.NaN;
			_border.Width = Element.Width > 0 ? Element.Width : Double.NaN;
		}
	}
}