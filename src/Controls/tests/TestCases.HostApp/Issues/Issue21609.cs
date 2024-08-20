using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21609, "Changing the dimensions of the CarouselView doesn't update Item Dimensions")]
public class Issue21609 : TestContentPage
{
	protected override void Init()
	{
		if (OperatingSystem.IsWindows())
		{
			Content = new Label() { Text = "This Test currently doesn't work on windows" };
			return;
		}

		var carV = new CarouselView()
		{
			HeightRequest = 200,
			WidthRequest = 200,
			ItemTemplate = new DataTemplate(() =>
			{
				return new Image() { Source = "dotnet_bot.png", AutomationId = "DotnetBot" };
			})
		};

		carV.ItemsSource = new[] { 1 };

		var changeSize = new Button()
		{
			Text = "Click me and the dimensions of the CarouselView Should Change",
			AutomationId = "ChangeCarouselViewDimensions",
			Command = new Command(() =>
			{
				if (carV.HeightRequest == 200)
				{
					carV.WidthRequest = carV.HeightRequest = 100;
				}
				else
				{
					carV.WidthRequest = carV.HeightRequest = 200;
				}
			})
		};

		var innerVSL = new VerticalStackLayout()
		{
			carV
		};

		innerVSL.HeightRequest = 200;
		Content = new VerticalStackLayout()
		{
			innerVSL,
			changeSize
		};
	}
}
