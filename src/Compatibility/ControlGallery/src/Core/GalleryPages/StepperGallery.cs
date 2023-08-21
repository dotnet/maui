//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class StepperGallery : ContentPage
	{
		public StepperGallery()
		{
			var stepper = new Stepper
			{
				Minimum = 0,
				Maximum = 100,
				Increment = 10
			};

			var label = new Label
			{
				Text = stepper.Value.ToString()
			};

			stepper.ValueChanged += (s, e) =>
			{
				label.Text = e.NewValue.ToString();
			};

			var stepperTwo = new Stepper
			{
				Minimum = 0.0,
				Maximum = 1.0,
				Increment = 0.05
			};

			var labelTwo = new Label
			{
				Text = stepperTwo.Value.ToString()
			};

			stepperTwo.ValueChanged += (s, e) =>
			{
				labelTwo.Text = e.NewValue.ToString();
			};

			Content = new StackLayout
			{
				Padding = new Thickness(20),
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

