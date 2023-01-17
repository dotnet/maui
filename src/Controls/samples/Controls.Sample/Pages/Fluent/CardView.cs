using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.Fluent
{
	public class CardView : ContentView
	{
		int count = 0;

		public CardView()
		{
			Content = new Border
			{
				new Grid
				{
					new Label()
						.Text("I'm .NET Bot")
						.TextColor(Colors.LightGray)
						.FontSize(40)
						.HorizontalOptions(LayoutOptions.Center)
						.VerticalOptions(LayoutOptions.Center),

					new Image()
						.Row(1)
						.Source("dotnet_bot.png"),

					new Label()
						.Text("Hello, World!")
						.Row(2)
						.TextColor(Colors.LightGray)
						.FontSize(30)
						.Margin(new Thickness(10))
						.HorizontalOptions(LayoutOptions.Center),

					new Button()
						.Text("Click me")
						.Row(3)
						.WidthRequest(220)
						.HorizontalOptions(LayoutOptions.Center)
						.Margin(new Thickness(40))
						.OnClicked(button =>
						{
							count++;
							button.Text = $"Clicked {count} ";
							button.Text += count == 1 ? "time" : "times";
						})

				}
				.RowDefinitions(e => e.Star().Star(2).Auto().Auto())
			}
			.StrokeShape(new RoundRectangle().CornerRadius(30))
			.BackgroundColor(Colors.DarkSlateGrey);
		}
	}
}