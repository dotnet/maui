﻿namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 3413, "[iOS] Searchbar in Horizontal Stacklayout doesn't render", PlatformAffected.iOS)]
	public class Issue3413 : TestContentPage
	{
		protected override void Init()
		{
			Padding = new Thickness(20);

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Vertical
			};

			var searchBar = new SearchBar
			{
				BackgroundColor = Colors.Yellow,
				Text = "i m on a vertical stacklayout",
				AutomationId = "srb_vertical"
			};
			layout.Children.Add(new Label { Text = "Vertical" });
			layout.Children.Add(searchBar);

			var layout1 = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var searchBar1 = new SearchBar
			{
				BackgroundColor = Colors.Yellow,
				Text = "i m on a horizontal stacklayout",
				AutomationId = "srb_horizontal"
			};

			layout1.Children.Add(new Label { Text = "Horizontal" });
			layout1.Children.Add(searchBar1);

#pragma warning disable CS0618 // Type or member is obsolete
			var searchBar2 = new SearchBar
			{
				BackgroundColor = Colors.Blue,
				Text = "i m with expand",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				AutomationId = "srb_grid"
			};
#pragma warning restore CS0618 // Type or member is obsolete

			var grid = new Grid();
			grid.Children.Add(layout);
			Grid.SetRow(layout, 0);
			grid.Children.Add(layout1);
			Grid.SetRow(layout1, 1);
			grid.Children.Add(searchBar2);
			Grid.SetRow(searchBar2, 2);
			Content = grid;
		}
	}
}