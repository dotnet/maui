using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Maui.Controls.Sample.Pages.Fluent;

namespace Maui.Controls.Sample.Pages
{
	public class FluentExamplePage : ContentPage
	{
		public FluentExamplePage()
		{
			Resources = ExampleStyleResources.Default;
			Content = new ScrollView
			{
				new VerticalStackLayout
				{
					new Label()
						.Text("Fluent API Page")
						.FontSize(50)
						.TextColor(ExampleStyleResources.PrimaryColor)
						.HorizontalOptions(LayoutOptions.Center),

					new CardView()
						.SizeRequest(300, 500)
						.Margin(new Thickness(20)),

					new ShapesView()
						.SizeRequest(300, 500)
						.Margin(new Thickness(20))
				}
				.VerticalOptions(LayoutOptions.Center)
				.Margin(new Thickness(40))
			};
		}
	}
}