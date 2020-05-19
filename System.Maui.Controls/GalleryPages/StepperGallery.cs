using System;

namespace Xamarin.Forms.Controls
{
	public class StepperGallery : ContentPage
	{
		public StepperGallery ()
		{
			var stepper = new Stepper {
				Minimum = 0,
				Maximum = 100,
				Increment = 10
			};

			var label = new Label {
				Text = stepper.Value.ToString()
			};

			stepper.ValueChanged += (s, e) => {
				label.Text = e.NewValue.ToString();
			};

			var stepperTwo = new Stepper {
				Minimum = 0.0,
				Maximum = 1.0,
				Increment = 0.05
			};

			var labelTwo = new Label {
				Text = stepperTwo.Value.ToString ()
			};

			stepperTwo.ValueChanged += (s, e) => {
				labelTwo.Text = e.NewValue.ToString ();
			};

			Content = new StackLayout {
				Padding = new Thickness (20),
				Children = {
					stepper,
					label,
					stepperTwo,
					labelTwo
				}
			};
		}
	}
}

