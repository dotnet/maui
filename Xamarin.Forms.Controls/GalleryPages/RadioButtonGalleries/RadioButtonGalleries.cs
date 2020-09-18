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
							new RadioButtonGroupGalleryPage(), Navigation),
						GalleryBuilder.NavButton("RadioButton Group (Attached Property)", () =>
							new RadioButtonGroupGallery(), Navigation),
						GalleryBuilder.NavButton("RadioButton Group (Attached Property, Binding)", () =>
							new RadioButtonGroupBindingGallery(), Navigation),
						GalleryBuilder.NavButton("RadioButton Group (Across Multiple Containers)", () =>
							new ScatteredRadioButtonGallery(), Navigation),
						GalleryBuilder.NavButton("RadioButton Content", () =>
							new RadioButtonContentGallery(), Navigation),
						GalleryBuilder.NavButton("RadioButton Content Properties", () =>
							new ContentProperties(), Navigation),
							GalleryBuilder.NavButton("RadioButton Template from Style", () =>
							new TemplateFromStyle(), Navigation),
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