﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9092, "[Bug] GTK: reenabling button doesn't work if page is a not-initial NavigationPage", PlatformAffected.Gtk)]
	public class Issue9092 : TestNavigationPage
	{
		protected override void Init()
		{
			var page = new ContentPage9092();
			NavigationPage.SetHasNavigationBar(page, false);

			var navigationPage = new NavigationPage(page);
			NavigationPage.SetHasNavigationBar(navigationPage, false);

			Navigation.PushAsync(navigationPage);
		}

#if UITEST
		[Test]
		public void Issue9092Test()
		{
			RunningApp.Screenshot("I see button");
			RunningApp.Tap(q => q.Marked("AddButton"));
			RunningApp.Screenshot("I see button with added text");
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ContentPage9092 : ContentPage
	{
		public ContentPage9092()
		{
			var button = new Button()
			{
				Text = "Add +",
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = "AddButton"
			};

			button.Command = new Command(o =>
			{
				button.Text += " +";
			});

			var stackLayoutWithButton = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					button
				}
			};

			var grid = new Grid()
			{
				RowSpacing = 0,
				RowDefinitions =
				{
					new RowDefinition() { Height = 30 },
					new RowDefinition() { Height = GridLength.Star },
					new RowDefinition() { Height = 30 }
				}
			};

			grid.AddChild(new Grid { BackgroundColor = Colors.Green }, 0, 0);
			grid.AddChild(stackLayoutWithButton, 0, 1);
			grid.AddChild(new Grid { BackgroundColor = Colors.Green }, 0, 2);

			Content = grid;

			Title = "Page Title";
		}
	}
}
