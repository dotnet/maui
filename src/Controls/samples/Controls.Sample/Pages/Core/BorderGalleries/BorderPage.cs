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
						GalleryBuilder.NavButton("Border Clipping Playground", () =>
							new BorderClipPlayground(), Navigation),
						GalleryBuilder.NavButton("Border using styles", () =>
							new BorderStyles(), Navigation),
						GalleryBuilder.NavButton("Border using Content Layout", () =>
							new BorderLayout(), Navigation),
						GalleryBuilder.NavButton("Border Stroke options", () =>
							new BorderStroke(), Navigation),
						GalleryBuilder.NavButton("Border without Stroke", () =>
							new Borderless(), Navigation),
						GalleryBuilder.NavButton("Border resize Content", () =>
							new BorderResizeContent(), Navigation),
						GalleryBuilder.NavButton("Border Alignment", () =>
							new BorderAlignment(), Navigation),
					}
				}
			};
		}
	}
}