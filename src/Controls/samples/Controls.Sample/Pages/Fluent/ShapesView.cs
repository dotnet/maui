using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.Fluent
{
	public class ShapesView : ContentView
	{
		public ShapesView()
		{
			Content = new Border
			{
				new Grid
				{
					new Polygon
					{
						new Point(0,48),
						new Point(0,144),
						new Point(96,150),
						new Point(100, 0),
						new Point(192, 0),
						new Point(192,96),
						new Point(50,96),
						new Point(48,192),
						new Point(150,200),
						new Point(144,48)
					}
					.Fill(Colors.DarkSlateGrey)
					.Stroke(Colors.DarkRed)
					.StrokeThickness(4)
					.HorizontalOptions(LayoutOptions.Center)
					.VerticalOptions(LayoutOptions.Center),

					new Label()
						.Row(1)
						.HorizontalOptions(LayoutOptions.Center)
						.VerticalOptions(LayoutOptions.Center)
						.Text("Shapes test")
						.FontSize(30)
						.TextColor(Colors.DarkSlateGray),

					new Polyline
					{
						new Point(0,0),
						new Point(20,60),
						new Point(30,0),
						new Point(36,120),
						new Point(46,60),
						new Point(70,60),
						new Point(80,0),
						new Point(86,120),
						new Point(96,60),
						new Point(200,60)
					}
					.Row(2)
					.Stroke(Colors.Red)
					.HorizontalOptions(LayoutOptions.Center)
					.VerticalOptions(LayoutOptions.Center)
				}
				.RowDefinitions(e => e.Star(1.3).Auto().Star())
			}
			.StrokeShape(new RoundRectangle().CornerRadius(30))
			.BackgroundColor(Colors.LightGrey);
		}
	}
}

