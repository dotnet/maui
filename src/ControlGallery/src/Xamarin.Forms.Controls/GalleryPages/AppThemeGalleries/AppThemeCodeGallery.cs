using System;

namespace Xamarin.Forms.Controls.GalleryPages.AppThemeGalleries
{
	public class AppThemeCodeGallery : ContentPage
	{
		Label _currentThemeLabel;

		public AppThemeCodeGallery()
		{
			_currentThemeLabel = new Label
			{
				Text = Application.Current.RequestedTheme.ToString()
			};

			Application.Current.RequestedThemeChanged += Current_RequestedThemeChanged;

			var onThemeLabel = new Label
			{
				Text = "TextColor through SetBinding"
			};

			var onThemeLabel1 = new Label
			{
				Text = "TextColor through SetAppTheme"
			};

			var onThemeLabel2 = new Label
			{
				Text = "TextColor through SetAppThemeColor"
			};

			onThemeLabel.SetBinding(Label.TextColorProperty, new AppThemeBinding() { Light = Color.Green, Dark = Color.Red });

			onThemeLabel1.SetOnAppTheme(Label.TextColorProperty, Color.Green, Color.Red);

			onThemeLabel2.SetAppThemeColor(Label.TextColorProperty, Color.Green, Color.Red);

			var stackLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children = { _currentThemeLabel, onThemeLabel, onThemeLabel1, onThemeLabel2 }
			};

			Content = stackLayout;
		}

		private void Current_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			_currentThemeLabel.Text = Application.Current.RequestedTheme.ToString();
		}
	}
}