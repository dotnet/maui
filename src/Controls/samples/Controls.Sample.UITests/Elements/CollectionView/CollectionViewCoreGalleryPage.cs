using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests.Elements
{
	public class CollectionViewCoreGalleryPage : ContentPage
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
						new Label { Text = "Layouts" },
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
						new Label { Text= "Add Remove Items" },
						TestBuilder.NavButton("Add Remove Items (List)", () =>
							new ObservableCodeCollectionViewGallery(grid: false), Navigation),
						TestBuilder.NavButton("Add Remove Items (Grid)", () =>
							new ObservableCodeCollectionViewGallery(), Navigation),
						new Label { Text= "EmptyView" },
						TestBuilder.NavButton("EmptyView (String)", () =>
							new EmptyViewStringGallery(), Navigation),
						TestBuilder.NavButton("EmptyView (View)", () =>
							new EmptyViewViewGallery(), Navigation),
						TestBuilder.NavButton("EmptyView (Template View)", () =>
							new EmptyViewTemplateGallery(), Navigation),
						new Label { Text= "Selection" },
						TestBuilder.NavButton("Preselected Item", () =>
							new PreselectedItemGallery(), Navigation),
						TestBuilder.NavButton("Preselected Items", () =>
							new PreselectedItemsGallery(), Navigation),   
						new Label { Text= "Grouping" },
						TestBuilder.NavButton("List Grouping", () =>
							new ListGrouping(), Navigation),
						TestBuilder.NavButton("Grid Grouping", () =>
							new GridGrouping(), Navigation),
						new Label { Text= "Header Footer" },
						TestBuilder.NavButton("Header Footer (String)", () => 
							new HeaderFooterString(), Navigation),
						TestBuilder.NavButton("Header Footer (View)", () => 
							new HeaderFooterView(), Navigation),
						TestBuilder.NavButton("Header Footer (Template)", () => 
							new HeaderFooterTemplate(), Navigation),
					}
				}
			};
		}
	}
}