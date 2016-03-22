using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class SwitchGallery : ContentPage
	{
		public SwitchGallery ()
		{
			var testLabel = new Label {
				Text = "Test Label"
			};

			var normal = new Switch { IsToggled = true };
			var disabled = new Switch ();
			var transparent = new Switch ();
			var stepper = new Stepper ();

			normal.Toggled += (sender, e) => {
				testLabel.Text = "Toggled normal switch";
			};

			disabled.Toggled += (sender, e) => {
				testLabel.Text = "Toggled disabled switch (magic)";
			};

			transparent.Toggled += (sender, e) => {
				testLabel.Text = "Toggled transparent switch";
			};

			stepper.ValueChanged += (sender, e) => {
				testLabel.Text = stepper.Value.ToString ();
			};

			disabled.IsEnabled = false;
			transparent.Opacity = 0.5;

			Content = new StackLayout {
				Padding = new Thickness (20),
				Children = {
					testLabel, 
					normal,
					disabled,
					transparent,
					stepper,
				}
			};
		}
	}
}
