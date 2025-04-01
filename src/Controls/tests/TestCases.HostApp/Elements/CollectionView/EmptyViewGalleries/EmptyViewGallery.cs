using Controls.Sample.UITests;

namespace Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries
{
	internal class EmptyViewGallery : ContentPage
	{
		public EmptyViewGallery()
		{
			var descriptionLabel =
				new Label { Text = "EmptyView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "EmptyView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						TestBuilder.NavButton("EmptyView (null ItemsSource)", () =>
							new EmptyViewNullGallery(), Navigation),
						TestBuilder.NavButton("EmptyView (null ItemsSource) View", () =>
							new EmptyViewNullGallery(false), Navigation),
						TestBuilder.NavButton("EmptyView (String)", () =>
							new EmptyViewStringGallery(), Navigation),
						TestBuilder.NavButton("EmptyView (View)", () =>
							new EmptyViewViewGallery(), Navigation),
						TestBuilder.NavButton("EmptyView (Template View)", () =>
							new EmptyViewTemplateGallery(), Navigation),
						TestBuilder.NavButton("EmptyView (Swap EmptyView)", () =>
							new EmptyViewSwapGallery(), Navigation),
						TestBuilder.NavButton("EmptyView (load simulation)", () =>
							new EmptyViewLoadSimulateGallery(), Navigation),
						TestBuilder.NavButton("EmptyView RTL", () =>
							new EmptyViewRTLGallery(), Navigation),
					}
				}
			};
		}
	}
}