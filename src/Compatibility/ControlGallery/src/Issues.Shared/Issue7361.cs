using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7361, "[UWP] Stepper display error when using Slider", PlatformAffected.UWP)]
	public class Issue7361 : TestContentPage
	{
		Slider TheSlider;
		Stepper TheStepper;
		Label ValueLabel;

		public Issue7361()
		{
			Title = "Issue 7361";
		}

		protected override void Init()
		{
			StackLayout layout = new StackLayout();
			layout.Orientation = StackOrientation.Vertical;

			var instructions = new Label
			{
				Margin = new Thickness(6),
				Text = "Slide slider to the extreme Right and Left, check that the Stepper to the right's +/- buttons work as expected, " +
					   "becoming disabled at the Max and Min positions of the Slider, and then enabled again as the Slider moves towards the center."
			};

			StackLayout controlsLayout = new StackLayout();
			controlsLayout.Orientation = StackOrientation.Horizontal;
			controlsLayout.HorizontalOptions = LayoutOptions.FillAndExpand;

			TheSlider = new Slider
			{
				Maximum = 100,
				Minimum = 1,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				MinimumTrackColor = Color.LightPink,
				MaximumTrackColor = Color.LightPink
			};
			controlsLayout.Children.Add(TheSlider);

			TheStepper = new Stepper
			{
				Maximum = 100,
				Minimum = 1,
				Increment = 1
			};
			controlsLayout.Children.Add(TheStepper);

			StackLayout labelLayout = new StackLayout();
			labelLayout.Orientation = StackOrientation.Horizontal;
			labelLayout.HorizontalOptions = LayoutOptions.FillAndExpand;

			Label valueHeaderLabel = new Label
			{
				Text = "Value:",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
			};
			labelLayout.Children.Add(valueHeaderLabel);

			ValueLabel = new Label
			{
				Text = "",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
			};
			labelLayout.Children.Add(ValueLabel);

			layout.Children.Add(instructions);
			layout.Children.Add(controlsLayout);
			layout.Children.Add(labelLayout);

			Content = layout;

			TheSlider.Value = 50;
			TheStepper.Value = 50;
			TheSlider.ValueChanged += SliderChanged;
			TheStepper.ValueChanged += StepperChanged;
		}

		private void SliderChanged(object sender, ValueChangedEventArgs e)
		{
			TheStepper.Value = e.NewValue;
			ValueLabel.Text = e.NewValue.ToString();
		}

		private void StepperChanged(object sender, ValueChangedEventArgs e)
		{
			TheSlider.Value = e.NewValue;
			ValueLabel.Text = e.NewValue.ToString();
		}
	}

}
