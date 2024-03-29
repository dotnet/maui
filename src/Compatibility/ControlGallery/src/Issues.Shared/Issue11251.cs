﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
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
	[Issue(IssueTracker.Github, 11251,
		"[Bug] SwipeView on UWP Executes Command Twice",
		PlatformAffected.UWP)]
	public class Issue11251 : TestContentPage
	{
		public Issue11251()
		{
		}

		public ICommand ExecuteMe = new Command(() =>
		{
			Debug.WriteLine("Executing Command...");
		});

		protected override void Init()
		{
			Title = "Issue 11251";

			BindingContext = this;

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Swipe to the right and tap the SwipeItem. Verify that the command has only been executed once."
			};

			var swipeView = new SwipeView();

			var swipeContent = new Grid
			{
				BackgroundColor = Colors.LightGray,
				HeightRequest = 80
			};

			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe To Right"
			};

			swipeContent.Children.Add(info);

			swipeView.Content = swipeContent;

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Execute Command",
				Command = ExecuteMe
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);

			Content = layout;

			swipeItem.Invoked += (sender, args) =>
			{
				Debug.WriteLine("SwipeItem Invoked...");
			};
		}
	}
}