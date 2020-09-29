using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using Xamarin.Forms.Platform.WPF.Extensions;
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

				UpdateBackground();
				UpdateCornerRadius();
				UpdateSize();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName ||
				e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
				e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
		}

		protected override void UpdateNativeWidget()
		{
			base.UpdateNativeWidget();

			UpdateSize();
		}

		protected override void UpdateBackground()
		{
			Brush background = Element.Background;

			if (Brush.IsNullOrEmpty(background))
			{
				Color color = Element.Color != Color.Default ? Element.Color : Element.BackgroundColor;
				_border.UpdateDependencyColor(Border.BackgroundProperty, color);
			}
			else
				_border.Background = background.ToBrush();
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