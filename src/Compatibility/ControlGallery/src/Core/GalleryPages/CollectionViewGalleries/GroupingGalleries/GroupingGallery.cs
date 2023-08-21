//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.GroupingGalleries
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
						GalleryBuilder.NavButton("Basic Grouping", () =>
							new BasicGrouping(), Navigation),
						GalleryBuilder.NavButton("Grouping, some empty groups", () =>
							new SomeEmptyGroups(), Navigation),
						GalleryBuilder.NavButton("Grouping, no templates", () =>
							new GroupingNoTemplates(), Navigation),
						GalleryBuilder.NavButton("Grouping, with selection", () =>
							new GroupingPlusSelection(), Navigation),
						GalleryBuilder.NavButton("Grouping, switchable", () =>
							new SwitchGrouping(), Navigation),
						GalleryBuilder.NavButton("Grouping, Observable", () =>
							new ObservableGrouping(), Navigation),
						GalleryBuilder.NavButton("Grouping, Grid", () =>
							new GridGrouping(), Navigation),
					}
				}
			};
		}
	}
}
