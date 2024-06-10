using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Controls.Sample.UITests;

namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	class GroupingGallery : ContentPage
	{
		public GroupingGallery()
		{
			var descriptionLabel =
				new Label { Text = "Grouping Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Grouping Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						TestBuilder.NavButton("Basic Grouping", () =>
							new BasicGrouping(), Navigation),
						TestBuilder.NavButton("Grouping, some empty groups", () =>
							new SomeEmptyGroups(), Navigation),
						TestBuilder.NavButton("Grouping, no templates", () =>
							new GroupingNoTemplates(), Navigation),
						TestBuilder.NavButton("Grouping, with selection", () =>
							new GroupingPlusSelection(), Navigation),
						TestBuilder.NavButton("Grouping, switchable", () =>
							new SwitchGrouping(), Navigation),
						TestBuilder.NavButton("Grouping, Observable", () =>
							new ObservableGrouping(), Navigation),
						TestBuilder.NavButton("Grouping, Grid", () =>
							new GridGrouping(), Navigation), 
					}
				}
			};
		}
	}
}