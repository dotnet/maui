using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class MaterialActivityIndicatorGallery : ContentPage
	{
		public MaterialActivityIndicatorGallery()
		{
			Visual = VisualMarker.Material;

			var activityIndicator = new ActivityIndicator()
			{
				IsRunning = false,
				BackgroundColor = Colors.Red,
				HeightRequest = 50
			};

			var IsRunGrid = new Grid
			{
				Padding = 0,
				ColumnSpacing = 6,
				RowSpacing = 6,
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = 50 }
				}
			};

			IsRunGrid.AddChild(new Label { Text = "Is Running" }, 0, 0);
			var isRunSwitch = new Switch
			{
				IsToggled = activityIndicator.IsRunning,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			isRunSwitch.Toggled += (_, e) => activityIndicator.IsRunning = e.Value;
			IsRunGrid.AddChild(isRunSwitch, 1, 0);

			var primaryPicker = new ColorPicker { Title = "Primary Color", Color = activityIndicator.Color };
			primaryPicker.ColorPicked += (_, e) =>
			{
				activityIndicator.Color = e.Color;
			};
			var backgroundPicker = new ColorPicker { Title = "Background Color", Color = activityIndicator.BackgroundColor };
			backgroundPicker.ColorPicked += (_, e) => activityIndicator.BackgroundColor = e.Color;

			var heightPicker = MaterialProgressBarGallery.CreateValuePicker("Height", value => activityIndicator.HeightRequest = value);

			var content = new StackLayout
			{
				Children = { activityIndicator },
				BackgroundColor = Colors.Blue
			};

			var backgroundPanelPicker = new ColorPicker { Title = "Back panel Color", Color = content.BackgroundColor };
			backgroundPanelPicker.ColorPicked += (_, e) => content.BackgroundColor = e.Color;

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
								IsRunGrid,
								primaryPicker,
								backgroundPicker,
								backgroundPanelPicker,
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

					content
				}
			};
		}
	}
}
