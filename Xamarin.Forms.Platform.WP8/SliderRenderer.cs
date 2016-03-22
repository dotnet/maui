using System.ComponentModel;
using System.Windows;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class SliderRenderer : ViewRenderer<Slider, System.Windows.Controls.Slider>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			base.OnElementChanged(e);

			var wSlider = new System.Windows.Controls.Slider { Minimum = Element.Minimum, Maximum = Element.Maximum, Value = Element.Value };

			SetNativeControl(wSlider);

			wSlider.ValueChanged += HandleValueChanged;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Minimum":
					Control.Minimum = Element.Minimum;
					break;
				case "Maximum":
					Control.Maximum = Element.Maximum;
					break;
				case "Value":
					if (Control.Value != Element.Value)
						Control.Value = Element.Value;
					break;
			}
		}

		void HandleValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Slider.ValueProperty, Control.Value);
		}
	}
}