namespace Xamarin.Forms.Controls.GalleryPages.RadioButtonGalleries
{
	public class RadioButtonGalleries : ContentPage
	{
		public RadioButtonGalleries()
		{
			var descriptionLabel =
				   new Label { Text = "RadioButton Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "RadioButton Galleries";

			var button = new Button
			{
				Text = "Enable RadioButton",
				AutomationId = "EnableRadioButton"
			};
			button.Clicked += ButtonClicked;

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						button,
						GalleryBuilder.NavButton("RadioButton Group Gallery", () =>
							new RadioButtonGroupGalleryPage(), Navigation)
					}
				}
			};
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "RadioButton Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.RadioButtonExperimental });
		}
	}
}