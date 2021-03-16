namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.GradientGalleries
{
	public partial class VisualGradientViewsGallery : ContentPage
	{
		public VisualGradientViewsGallery()
		{
			InitializeComponent();
			BackgroundPicker.SelectedIndex = 1;

			Button.Clicked += (sender, args) =>
			{
				DisplayAlert("Events", "Button Clicked", "Ok");
			};
		}

		void OnBackgroundSelectedIndexChanged(object sender, System.EventArgs e)
		{
			Color? backgroundColor = null;
			Brush background = null;

			var selectedIndex = ((Picker)sender).SelectedIndex;

			switch (selectedIndex)
			{
				case 0:
					backgroundColor = null;
					background = null;
					break;
				case 1:
					backgroundColor = Color.Red;
					background = null;
					break;
				case 2:
					background = Resources["SolidColor"] as Brush;
					break;
				case 3:
					background = Resources["HorizontalLinearGradient"] as Brush;
					break;
				case 4:
					background = Resources["VerticalLinearGradient"] as Brush;
					break;
				case 5:
					background = Resources["RadialGradient"] as Brush;
					break;
			}

			UpdateBackgroundColor(backgroundColor);
			UpdateBackground(background);
		}

		void UpdateBackgroundColor(Color? color)
		{
			var backgroundColor = color ?? Color.Default;

			ActivityIndicator.BackgroundColor = backgroundColor;
			Button.BackgroundColor = backgroundColor;
			CheckBox.BackgroundColor = backgroundColor;
			DatePicker.BackgroundColor = backgroundColor;
			Editor.BackgroundColor = backgroundColor;
			Entry.BackgroundColor = backgroundColor;
			Frame.BackgroundColor = backgroundColor;
			Picker.BackgroundColor = backgroundColor;
			ProgressBar.BackgroundColor = backgroundColor;
			Slider.BackgroundColor = backgroundColor;
			Stepper.BackgroundColor = backgroundColor;
			TimePicker.BackgroundColor = backgroundColor;
		}

		void UpdateBackground(Brush background)
		{
			ActivityIndicator.Background = background;
			Button.Background = background;
			CheckBox.Background = background;
			DatePicker.Background = background;
			Editor.Background = background;
			Entry.Background = background;
			Frame.Background = background;
			Picker.Background = background;
			ProgressBar.Background = background;
			Slider.Background = background;
			Stepper.Background = background;
			TimePicker.Background = background;
		}
	}
}