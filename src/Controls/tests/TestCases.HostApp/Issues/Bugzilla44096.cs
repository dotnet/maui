﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44096, "Grid, StackLayout, and ContentView still participate in hit testing on "
		+ "Android after IsEnabled is set to false", PlatformAffected.Android)]
	public class Bugzilla44096 : TestContentPage
	{
		bool _flag;
		const string Child = "Child";
		const string Original = "Original";
		const string ToggleColor = "color";
		const string ToggleIsEnabled = "disabled";

		const string StackLayout = "stackLayout";
		const string ContentView = "contentView";
		const string Grid = "grid";
		const string RelativeLayout = "relativeLayout";

		protected override void Init()
		{
			var result = new Label
			{
				Text = Original
			};

			var grid = new Grid
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = Grid
			};
			AddTapGesture(result, grid);

			var contentView = new ContentView
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = ContentView
			};
			AddTapGesture(result, contentView);

			var stackLayout = new StackLayout
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = StackLayout
			};
			AddTapGesture(result, stackLayout);

			var relativeLayout = new Microsoft.Maui.Controls.Compatibility.RelativeLayout
			{
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = RelativeLayout
			};
			AddTapGesture(result, relativeLayout);

			var color = new Button
			{
				Text = "Toggle colors",
				Command = new Command(() =>
				{
					if (!_flag)
					{
						grid.BackgroundColor = Colors.Red;
						contentView.BackgroundColor = Colors.Blue;
						stackLayout.BackgroundColor = Colors.Yellow;
						relativeLayout.BackgroundColor = Colors.Green;
					}
					else
					{
						grid.BackgroundColor = null;
						contentView.BackgroundColor = null;
						stackLayout.BackgroundColor = null;
						relativeLayout.BackgroundColor = null;
					}

					_flag = !_flag;
				}),
				AutomationId = ToggleColor
			};

			var disabled = new Button
			{
				Text = "Toggle IsEnabled",
				Command = new Command(() =>
				{
					grid.IsEnabled = false;
					contentView.IsEnabled = false;
					stackLayout.IsEnabled = false;
					relativeLayout.IsEnabled = false;

					result.Text = Original;
				}),
				AutomationId = ToggleIsEnabled
			};

			var parent = new StackLayout
			{
				Spacing = 10,
				Orientation = StackOrientation.Vertical,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					color,
					disabled,
					result,
					grid,
					contentView,
					stackLayout,
					relativeLayout
				}
			};

			Content = parent;
		}

		void AddTapGesture(Label result, View view)
		{
			var tapGestureRecognizer = new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					result.Text = Child;
				})
			};
			view.GestureRecognizers.Add(tapGestureRecognizer);
		}
	}
}