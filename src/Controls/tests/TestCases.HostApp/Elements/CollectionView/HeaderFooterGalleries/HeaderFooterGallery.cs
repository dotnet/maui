using Controls.Sample.UITests;

namespace Maui.Controls.Sample.CollectionViewGalleries.HeaderFooterGalleries
{
	internal class HeaderFooterGallery : ContentPage
	{
		public HeaderFooterGallery()
		{
			var descriptionLabel =
				new Label { Text = "Header/Footer Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Header/Footer Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						TestBuilder.NavButton("Header/Footer (String)", () => new HeaderFooterString(), Navigation),
						TestBuilder.NavButton("Header/Footer (Forms View)", () => new HeaderFooterView(), Navigation),
						TestBuilder.NavButton("Header/Footer (Horizontal Forms View)", () => new HeaderFooterViewHorizontal(), Navigation),
						TestBuilder.NavButton("Header/Footer (Template)", () => new HeaderFooterTemplate(), Navigation),
						TestBuilder.NavButton("Header/Footer (Grid)", () => new HeaderFooterGrid(), Navigation),
						TestBuilder.NavButton("Footer Only (String)", () => new FooterOnlyString(), Navigation),
						TestBuilder.NavButton("Header/Footer (Grid Horizontal)", () => new HeaderFooterGridHorizontal(), Navigation),
					}
				}
			};
		}
	}
}
