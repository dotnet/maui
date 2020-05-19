using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class SliderGallery : ContentPage
	{
		public SliderGallery ()
		{
			var normal = new Slider (20, 100, 20);
			var disabled = new Slider (0, 1, 0);
			var transparent = new Slider (0, 1, 0);
			var valueLabel = new Label { Text = normal.Value.ToString () };

			disabled.IsEnabled = false;
			transparent.Opacity = .5;
			normal.ValueChanged += (sender, e) => { valueLabel.Text = normal.Value.ToString (); };

			Content = new StackLayout {
				Padding = new Thickness (40),
				Children = {
					normal,
					disabled,
					transparent,
					valueLabel
				}
			};
		}
	}
}
