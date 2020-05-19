using System;
using System.Collections.Generic;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(Core.UITests.UITestCategories.MasterDetailPage)]
	[Category(Core.UITests.UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5412, "5412 - (NavigationBar disappears on MasterDetailPage)", PlatformAffected.UWP)]
	public class Issue5412 : TestContentPage
	{
		protected override async void Init()
		{
			await Navigation.PushModalAsync(new Issue5412MainPage());
		}

#if UITEST && __WINDOWS__
		[Test]
		public void Issue5412Test()
		{
			var hamburgerText = "\uE700";
			var settings = "Settings";
			var back = "Back";
		
			RunningApp.WaitForElement(hamburgerText);
			RunningApp.Tap(hamburgerText);

			RunningApp.WaitForElement(settings);
			RunningApp.Tap(settings);

			RunningApp.WaitForElement(back);
			RunningApp.Tap(back);

			// This fails if the menu isn't displayed (original error behavior)
			RunningApp.WaitForElement(hamburgerText);
		}
#endif
	}

	public class Issue5412MainPage : MasterDetailPage
	{
		public Issue5412MainPage()
		{
			var menuBtn = new Button
			{
				Text = "Settings"
			};
			menuBtn.Clicked += (sender, e) =>
			{
				var mdp = ((sender as Button).Parent.Parent as MasterDetailPage);
				mdp.Detail.Navigation.PushAsync(new Issue5412SettingPage());
				mdp.IsPresented = false;
			};

			MasterBehavior = MasterBehavior.Popover;
			Master = new ContentPage
			{
				Content = menuBtn,
				Title = "Menu title"
			};
			Detail = new NavigationPage(new Issue5412IndexPage());
		}
	}

	public class Issue5412SettingPage : ContentPage
	{
		public Issue5412SettingPage()
		{
			Content = new StackLayout
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Children = {
					new Label
					{
						Text = "Settings Page",
						FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label))
					},
					new Label
					{
						Text = "Navigate back and check the navbar & menu are still visible.",
						FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
					},
				}
			};
		}
	};

	public class Issue5412IndexPage : ContentPage
	{
		public Issue5412IndexPage()
		{
			Content = new StackLayout
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Children = {
					new Label
					{
						Text = "Index Page",
						FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label))
					},
					new Label
					{
						Text = "Open the hamburger menu and navigate to settings page",
						FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
					},
				}
			};
		}
	}
}
