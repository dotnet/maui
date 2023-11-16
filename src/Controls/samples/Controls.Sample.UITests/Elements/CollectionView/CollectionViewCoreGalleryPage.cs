using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests.Elements
{ 
	class CollectionViewCoreGalleryPage : ContentPage
	{
		public CollectionViewCoreGalleryPage()
		{
			var descriptionLabel =
				   new Label { AutomationId = "WaitForStubControl", Text = "CollectionView Galleries", Margin = new Thickness(2) };

			Title = "CollectionView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Padding = new Thickness(2),
					Children =
					{
						descriptionLabel,
						new Label{ Text = "Layouts" },
						TestBuilder.NavButton("Vertical List", () =>
							new TemplateCodeCollectionViewGallery(LinearItemsLayout.Vertical), Navigation),
						TestBuilder.NavButton("Horizontal List", () =>
							new TemplateCodeCollectionViewGallery(LinearItemsLayout.Horizontal), Navigation),
						TestBuilder.NavButton("Vertical Grid", () =>
							new TemplateCodeCollectionViewGridGallery (), Navigation),
						TestBuilder.NavButton("Horizontal Grid", () =>
							new TemplateCodeCollectionViewGridGallery (ItemsLayoutOrientation.Horizontal), Navigation),
						TestBuilder.NavButton("DataTemplateSelector", () =>
							new DataTemplateSelectorGallery(), Navigation),
					}
				}
			};
		}
	}
}