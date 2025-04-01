using Controls.Sample.UITests;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class PropagationGallery : ContentPage
	{
		public PropagationGallery()
		{
			var descriptionLabel =
				new Label { Text = "Property Propagation Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Property Propagation Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						TestBuilder.NavButton("Propagate FlowDirection", () =>
							new PropagateCodeGallery(LinearItemsLayout.Vertical), Navigation),

						TestBuilder.NavButton("Propagate FlowDirection in EmptyView", () =>
							new PropagateCodeGallery(LinearItemsLayout.Vertical, 0), Navigation),
					}
				}
			};
		}
	}
}