using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages
{
	[Preserve(AllMembers = true)]
	public class BorderPage : ContentPage
	{
		public BorderPage()
		{
			var descriptionLabel =
				new Label { Text = "Border Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Border Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Border Playground", () =>
							new BorderPlayground(), Navigation),
						GalleryBuilder.NavButton("Border using styles", () =>
							new BorderStyles(), Navigation),
					}
				}
			};
		}
	}
}