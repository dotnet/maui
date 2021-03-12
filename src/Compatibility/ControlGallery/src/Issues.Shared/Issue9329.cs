using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9329, "Xamarin.Forms SwipeView IsEnabled not working", PlatformAffected.All)]
	public class Issue9329 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 9329";

			var layout = new StackLayout();

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Check/uncheck the CheckBox to enable or disable the SwipeView."
			};

			var swipeItem = new SwipeItem { BackgroundColor = Color.Red, Text = "Test", IconImageSource = "coffee.png" };

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				BackgroundColor = Color.LightGray,
				LeftItems = new SwipeItems(new List<SwipeItem> { swipeItem })
				{
					Mode = SwipeMode.Execute
				},
				RightItems = new SwipeItems(new List<SwipeItem> { swipeItem })
			};

			var content = new Grid
			{
				BackgroundColor = Color.LightGoldenrodYellow
			};

			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe"
			};

			content.Children.Add(info);

			swipeView.Content = content;

			var checkLayout = new StackLayout();
			checkLayout.Orientation = StackOrientation.Horizontal;

			var checkBox = new CheckBox
			{
				VerticalOptions = LayoutOptions.Center
			};

			var checkInfoLabel = new Label
			{
				Text = "SwipeView Enabled",
				VerticalOptions = LayoutOptions.Center
			};

			checkBox.CheckedChanged += (sender, args) =>
			{
				swipeView.IsEnabled = !swipeView.IsEnabled;
				checkInfoLabel.Text = swipeView.IsEnabled ? "SwipeView Enabled" : "SwipeView Disabled";
			};

			checkLayout.Children.Add(checkBox);
			checkLayout.Children.Add(checkInfoLabel);

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);
			layout.Children.Add(checkLayout);

			Content = layout;
		}
	}
}
