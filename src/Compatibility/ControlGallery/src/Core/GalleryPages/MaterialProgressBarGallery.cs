using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class MaterialProgressBarGallery : ContentPage
	{
		public MaterialProgressBarGallery()
		{
			Visual = VisualMarker.Material;

			var progressBar = new ProgressBar { Progress = 0.5 };
			var slider = new Slider { Value = 0.5 };

			var primaryPicker = new ColorPicker { Title = "Primary Color" };
			primaryPicker.ColorPicked += (_, e) =>
			{
				progressBar.ProgressColor = e.Color;
				slider.MinimumTrackColor = e.Color;
			};
			var backgroundPicker = new ColorPicker { Title = "Background Color" };
			backgroundPicker.ColorPicked += (_, e) =>
			{
				progressBar.BackgroundColor = e.Color;
				slider.MaximumTrackColor = e.Color;
			};
			var thumbPicker = new ColorPicker { Title = "Thumb Color" };
			thumbPicker.ColorPicked += (_, e) =>
			{
				slider.ThumbColor = e.Color;
			};

			var valuePicker = CreateValuePicker("Value / Progress", value =>
			{
				progressBar.Progress = value / 100.0;
				slider.Value = value / 100.0;
			});
			var heightPicker = CreateValuePicker("Height", value =>
			{
				progressBar.HeightRequest = value;
				slider.HeightRequest = value;
			});

			Content = new StackLayout
			{
				Padding = 10,
				Spacing = 10,
				Children =
				{
					new ScrollView
					{
						Margin = new Thickness(-10, 0),
						Content = new StackLayout
						{
							Padding = 10,
							Spacing = 10,
							Children =
							{
								primaryPicker,
								backgroundPicker,
								thumbPicker,
								valuePicker,
								heightPicker,
							}
						}
					},

					new BoxView
					{
						HeightRequest = 1,
						Margin = new Thickness(-10, 0),
						Color = Colors.Black
					},

					progressBar,
					slider
				}
			};
		}

		internal static Grid CreateValuePicker(string title, Action<double> changed)
		{
			// 50%
			Slider slider = new Slider(0, 100, 50);

			var actions = new Grid
			{
				Padding = 0,
				ColumnSpacing = 6,
				RowSpacing = 6,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = 10 },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = 30 }
				}
			};

			actions.AddChild(new Label { Text = title }, 0, 0, 3);

			var valueLabel = new Label
			{
				Text = slider.Value.ToString(),
				HorizontalOptions = LayoutOptions.End
			};

			slider.ValueChanged += (_, e) =>
			{
				changed?.Invoke(slider.Value);
				valueLabel.Text = e.NewValue.ToString("0");
			};
			actions.AddChild(new Label { Text = "V" }, 0, 1);
			actions.AddChild(slider, 1, 1);
			actions.AddChild(valueLabel, 2, 1);

			return actions;
		}
	}
}
