using Controls.Sample.UITests;
using Maui.Controls.Sample.CollectionViewGalleries.DataTemplateSelectorGalleries;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class DataTemplateGallery : ContentPage
	{
		public DataTemplateGallery()
		{
			var descriptionLabel =
				new Label { Text = "Simple DataTemplate Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Simple DataTemplate Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						TestBuilder.NavButton("Vertical List (Code)", () =>
							new TemplateCodeCollectionViewGallery(LinearItemsLayout.Vertical), Navigation),
						TestBuilder.NavButton("Horizontal List (Code)", () =>
							new TemplateCodeCollectionViewGallery(LinearItemsLayout.Horizontal), Navigation),
						TestBuilder.NavButton("Vertical Grid (Code)", () =>
							new TemplateCodeCollectionViewGridGallery (), Navigation),
						TestBuilder.NavButton("Horizontal Grid (Code)", () =>
							new TemplateCodeCollectionViewGridGallery (ItemsLayoutOrientation.Horizontal), Navigation),
						TestBuilder.NavButton("DataTemplateSelector", () =>
							new DataTemplateSelectorGallery(), Navigation),
						TestBuilder.NavButton("Varied Size Data Templates", () =>
							new VariedSizeDataTemplateSelectorGallery(), Navigation),
					}
				}
			};
		}
	}
}