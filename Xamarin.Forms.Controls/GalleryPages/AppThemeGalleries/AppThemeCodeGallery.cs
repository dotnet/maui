namespace Xamarin.Forms.Controls.GalleryPages.AppThemeGalleries
{
	public class AppThemeCodeGallery : ContentPage
	{
		AppThemeColor color = new AppThemeColor { Light = Color.Green, Dark = Color.Red };
		public Color TheColor
		{
			get => color.Value;
		}

		public AppThemeCodeGallery()
		{
			var currentThemeLabel = new Label
			{
				Text = Application.Current.RequestedTheme.ToString()
			};

			Application.Current.RequestedThemeChanged += (s, a) =>
			{
				currentThemeLabel.Text = Application.Current.RequestedTheme.ToString();
				OnPropertyChanged(nameof(TheColor));
			};

			var onThemeLabel = new Label
			{
				Text = "This text is green or red depending on Light (or default) or Dark"
			};

			onThemeLabel.SetBinding(Label.TextColorProperty, new Binding(nameof(TheColor)));
			BindingContext = this;

			var stackLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children = { currentThemeLabel, onThemeLabel }
			};

			Content = stackLayout;
		}
	}
}