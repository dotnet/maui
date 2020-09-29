namespace Xamarin.Forms.Controls.GalleryPages.GradientGalleries
{
	public partial class GradientViewsGallery : ContentPage
	{
		public GradientViewsGallery()
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

			Button.BackgroundColor = backgroundColor;
			BoxView.BackgroundColor = backgroundColor;
			CornerRadiusBoxView.BackgroundColor = backgroundColor;
			CheckBox.BackgroundColor = backgroundColor;
			CarouselView.BackgroundColor = backgroundColor;
			CollectionView.BackgroundColor = backgroundColor;
			DatePicker.BackgroundColor = backgroundColor;
			Editor.BackgroundColor = backgroundColor;
			Entry.BackgroundColor = backgroundColor;
			Frame.BackgroundColor = backgroundColor;
			Grid.BackgroundColor = backgroundColor;
			ImageButton.BackgroundColor = backgroundColor;
			Label.BackgroundColor = backgroundColor;
			ListView.BackgroundColor = backgroundColor;
			Picker.BackgroundColor = backgroundColor;
			ScrollView.BackgroundColor = backgroundColor;
			SearchBar.BackgroundColor = backgroundColor;
			Slider.BackgroundColor = backgroundColor;
			Stepper.BackgroundColor = backgroundColor;
			SwipeView.BackgroundColor = backgroundColor;
			SwipeViewContent.BackgroundColor = backgroundColor;
			TableView.BackgroundColor = backgroundColor;
			TimePicker.BackgroundColor = backgroundColor;
		}

		void UpdateBackground(Brush background)
		{
			Button.Background = background;
			BoxView.Background = background;
			CornerRadiusBoxView.Background = background;
			CheckBox.Background = background;
			CarouselView.Background = background;
			CollectionView.Background = background;
			DatePicker.Background = background;
			Editor.Background = background;
			Entry.Background = background;
			Frame.Background = background;
			Grid.Background = background;
			ImageButton.Background = background;
			Label.Background = background;
			ListView.Background = background;
			Picker.Background = background;
			ScrollView.Background = background;
			SearchBar.Background = background;
			Slider.Background = background;
			Stepper.Background = background;
			SwipeView.Background = background;
			SwipeViewContent.Background = background;
			TableView.Background = background;
			TimePicker.Background = background;
		}
	}
}