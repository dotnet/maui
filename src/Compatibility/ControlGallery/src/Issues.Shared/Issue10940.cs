﻿using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10940,
		"[Android]CustomRenderer object was created twice for single control when add custom control in SwipeView item",
		PlatformAffected.Android)]
	public class Issue10940 : TestContentPage
	{
		public Issue10940()
		{
		}

		protected override void Init()
		{
			Title = "Issue 10940";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Swipe to the left and verify that opening the SwipeView the CustomEntry is created only one time."
			};

			var customSwipeView = new CustomSwipeView
			{
				BackgroundColor = Colors.LightGray,
				HeightRequest = 200
			};

			var swipeItemView = new SwipeItemView();

			var swipeItemViewContent = new Grid
			{
				BackgroundColor = Colors.White,
				WidthRequest = 150
			};

			var customEntry = new CustomEntry
			{
				VerticalOptions = LayoutOptions.Center
			};

			swipeItemViewContent.Children.Add(customEntry);

			swipeItemView.Content = swipeItemViewContent;

			customSwipeView.RightItems = new SwipeItems
			{
				swipeItemView
			};

			layout.Children.Add(instructions);
			layout.Children.Add(customSwipeView);

			Content = layout;
		}
	}

	[Preserve(AllMembers = true)]
	public class CustomEntry : Entry
	{

	}

	[Preserve(AllMembers = true)]
	public class CustomSwipeView : SwipeView
	{

	}
}