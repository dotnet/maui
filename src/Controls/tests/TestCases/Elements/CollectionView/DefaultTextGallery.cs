using Controls.Sample.UITests;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class DefaultTextGallery : ContentPage
	{
		public DefaultTextGallery()
		{
			var descriptionLabel = new Label
			{
				Text = "No DataTemplates; just using the ToString() of the objects in the source.",
				Margin = new Thickness(2, 2, 2, 2)
			};

			Title = "Default Text Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						// TODO hartez 2018-06-05 10:43 AM Need a gallery page which allows layout selection
						// so we can demonstrate switching between them
						descriptionLabel,
						TestBuilder.NavButton("Vertical List (Code)", () =>
							new TextCodeCollectionViewGallery(LinearItemsLayout.Vertical), Navigation),
						TestBuilder.NavButton("Horizontal List (Code)", () =>
							new TextCodeCollectionViewGallery(LinearItemsLayout.Horizontal), Navigation),
						TestBuilder.NavButton("Vertical Grid (Code)", () =>
							new TextCodeCollectionViewGridGallery(), Navigation),
						TestBuilder.NavButton("Horizontal Grid (Code)", () =>
							new TextCodeCollectionViewGridGallery(ItemsLayoutOrientation.Horizontal), Navigation),
					}
				}
			};
		}
	}
}