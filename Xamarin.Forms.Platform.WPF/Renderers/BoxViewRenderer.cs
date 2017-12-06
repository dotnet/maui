using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRectangle = System.Windows.Shapes.Rectangle;

namespace Xamarin.Forms.Platform.WPF
{
	public class BoxViewRenderer : ViewRenderer<BoxView, WRectangle>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new WRectangle());
				}

				UpdateColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				UpdateColor();
		}

		void UpdateColor()
		{
			Color color = Element.Color != Color.Default ? Element.Color : Element.BackgroundColor;
			Control.UpdateDependencyColor(WRectangle.FillProperty, color);
		}
	}
}
