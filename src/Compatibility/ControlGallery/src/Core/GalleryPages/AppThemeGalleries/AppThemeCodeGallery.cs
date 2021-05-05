using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.AppThemeGalleries
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

			onThemeLabel.SetBinding(Label.TextColorProperty, new AppThemeBinding() { Light = Colors.Green, Dark = Colors.Red });

			onThemeLabel1.SetOnAppTheme(Label.TextColorProperty, Colors.Green, Colors.Red);

			onThemeLabel2.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);

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