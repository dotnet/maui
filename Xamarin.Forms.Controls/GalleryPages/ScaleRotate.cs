using System;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	internal class ScaleRotate : ContentPage
	{
		public ScaleRotate()
		{
			Label label = new Label
			{
				Text = "SCALE AND\nROTATE",
				HorizontalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			// Label and Slider for Scale property.
			Label scaleSliderValue = new Label
			{
				VerticalTextAlignment = TextAlignment.Center
			};
			Grid.SetRow(scaleSliderValue, 0);
			Grid.SetColumn(scaleSliderValue, 0);

			Slider scaleSlider = new Slider
			{
				Maximum = 10
			};
			Grid.SetRow(scaleSlider, 0);
			Grid.SetColumn(scaleSlider, 1);

			// Set Bindings.
			scaleSliderValue.BindingContext = scaleSlider;
			scaleSliderValue.SetBinding(Label.TextProperty, 
				new Binding("Value", BindingMode.OneWay, null, null, "Scale = {0:F1}"));

			scaleSlider.BindingContext = label;
			scaleSlider.SetBinding(Slider.ValueProperty,
				new Binding("Scale", BindingMode.TwoWay));

			// Label and Slider for ScaleX property.
			Label scaleXSliderValue = new Label {
				VerticalTextAlignment = TextAlignment.Center
			};
			Grid.SetRow(scaleXSliderValue, 1);
			Grid.SetColumn(scaleXSliderValue, 0);

			Slider scaleXSlider = new Slider {
				Maximum = 10
			};
			Grid.SetRow(scaleXSlider, 1);
			Grid.SetColumn(scaleXSlider, 1);

			// Set Bindings.
			scaleXSliderValue.BindingContext = scaleXSlider;
			scaleXSliderValue.SetBinding(Label.TextProperty,
				new Binding("Value", BindingMode.OneWay, null, null, "ScaleX = {0:F1}"));

			scaleXSlider.BindingContext = label;
			scaleXSlider.SetBinding(Slider.ValueProperty,
				new Binding("ScaleX", BindingMode.TwoWay));

			// Label and Slider for Rotation property.
			Label rotationSliderValue = new Label
			{
				VerticalTextAlignment = TextAlignment.Center
			};
			Grid.SetRow(rotationSliderValue, 2);
			Grid.SetColumn(rotationSliderValue, 0);

			Slider rotationSlider = new Slider
			{
				Maximum = 360
			};
			Grid.SetRow(rotationSlider, 2);
			Grid.SetColumn(rotationSlider, 1);

			// Set Bindings.
			rotationSliderValue.BindingContext = rotationSlider;
			rotationSliderValue.SetBinding(Label.TextProperty,
				new Binding("Value", BindingMode.OneWay, null, null, "Rotation = {0:F0}"));

			rotationSlider.BindingContext = label;
			rotationSlider.SetBinding(Slider.ValueProperty,
				new Binding("Rotation", BindingMode.TwoWay));

			// Label and Slider for AnchorX property.
			Label anchorxStepperValue = new Label
			{
				VerticalTextAlignment = TextAlignment.Center
			};
			Grid.SetRow(anchorxStepperValue, 3);
			Grid.SetColumn(anchorxStepperValue, 0);

			Stepper anchorxStepper = new Stepper
			{
				Maximum = 2,
				Minimum = -1,
				Increment = 0.5
			};
			Grid.SetRow(anchorxStepper, 3);
			Grid.SetColumn(anchorxStepper, 1);

			// Set bindings.
			anchorxStepperValue.BindingContext = anchorxStepper;
			anchorxStepperValue.SetBinding(Label.TextProperty,
				new Binding("Value", BindingMode.OneWay, null, null, "AnchorX = {0:F1}"));

			anchorxStepper.BindingContext = label;
			anchorxStepper.SetBinding(Stepper.ValueProperty, 
				new Binding("AnchorX", BindingMode.TwoWay));

			// Label and Slider for AnchorY property.
			Label anchoryStepperValue = new Label
			{
				VerticalTextAlignment = TextAlignment.Center
			};
			Grid.SetRow(anchoryStepperValue, 4);
			Grid.SetColumn(anchoryStepperValue, 0);

			Stepper anchoryStepper = new Stepper
			{
				Maximum = 2,
				Minimum = -1,
				Increment = 0.5
			};
			Grid.SetRow(anchoryStepper, 4);
			Grid.SetColumn(anchoryStepper, 1);

			// Set bindings.
			anchoryStepperValue.BindingContext = anchoryStepper;
			anchoryStepperValue.SetBinding(Label.TextProperty,
				new Binding("Value", BindingMode.OneWay, null, null, "AnchorY = {0:F1}"));

			anchoryStepper.BindingContext = label;
			anchoryStepper.SetBinding(Stepper.ValueProperty, 
				new Binding("AnchorY", BindingMode.TwoWay));

			// Assemble the page.
			Content = new StackLayout
			{
				Children =
				{
					label,
					new Grid
					{
						Padding = 10,
						RowDefinitions = 
						{
							new RowDefinition { Height = GridLength.Auto },
							new RowDefinition { Height = GridLength.Auto },
							new RowDefinition { Height = GridLength.Auto },
							new RowDefinition { Height = GridLength.Auto },
							new RowDefinition { Height = GridLength.Auto },
						},
						ColumnDefinitions = 
						{
							new ColumnDefinition { Width = GridLength.Auto },
							new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)}
						},
						Children = 
						{
							scaleSliderValue,
							scaleSlider,
							scaleXSliderValue,
							scaleXSlider,
							rotationSliderValue,
							rotationSlider,
							anchorxStepperValue, 
							anchorxStepper,
							anchoryStepperValue,
							anchoryStepper
						}
					}
				}
			};
		}
	}
}


