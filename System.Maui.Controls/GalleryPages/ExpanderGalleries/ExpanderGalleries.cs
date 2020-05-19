namespace System.Maui.Controls.GalleryPages.ExpanderGalleries
{
	public class ExpanderGalleries : ContentPage
	{
		public ExpanderGalleries()
		{
			var descriptionLabel =
				   new Label { Text = "Expander Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Expander Galleries";

			var button = new Button
			{
				Text = "Enable Expander",
				AutomationId = "EnableExpander"
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
						GalleryBuilder.NavButton("Expander Gallery", () =>
							new ExpanderGallery(), Navigation)
					}
				}
			};
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "Expander Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.ExpanderExperimental });
		}
	}
}