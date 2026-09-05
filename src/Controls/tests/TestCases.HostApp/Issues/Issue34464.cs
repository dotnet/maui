using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34464, "FlexLayout with BindableLayout and Label text display", PlatformAffected.All)]
public class Issue34464 : ContentPage
{
	public Issue34464()
	{
		var items = new List<string>
		{
			"Some Medium Text",
			"Shorter Text",
			"Slightly More Text",
			"One - Two",
			"Two - Four",
			"Two - Three",
			"One - Eleven",
		};

		var flexLayout = new FlexLayout
		{
			Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
			AutomationId = "TestFlexLayout"
		};

		BindableLayout.SetItemsSource(flexLayout, items);
		BindableLayout.SetItemTemplate(flexLayout, new DataTemplate(() =>
		{
			var border = new Border
			{
				BackgroundColor = Color.FromArgb("#ffcccccc"),
				Stroke = Color.FromArgb("#ffb8b8b8"),
				Padding = new Thickness(12)
			};

			FlexLayout.SetGrow(border, 1);
			FlexLayout.SetShrink(border, 0);

			var backgroundBorder = new Border
			{
				BackgroundColor = Color.FromArgb("#44ff0000")
			};

			var label = new Label
			{
				LineBreakMode = LineBreakMode.NoWrap,
				FontSize = 20,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromArgb("#ff000000")
			};

			label.SetBinding(Label.TextProperty, ".");

			var grid = new Grid
			{
				Children = { backgroundBorder, label }
			};

			border.Content = grid;

			return border;
		}));

		var headerLabel = new Label
		{
			Text = "FlexLayout with BindableLayout Items:",
			FontSize = 16,
			Margin = new Thickness(10),
			AutomationId = "HeaderLabel"
		};

		Content = new VerticalStackLayout
		{
			Children = { headerLabel, flexLayout }
		};
	}
}
