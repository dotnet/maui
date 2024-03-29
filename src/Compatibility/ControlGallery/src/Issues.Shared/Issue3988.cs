﻿using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3988, "Windows Platform Specific - IsDynamicOverflowEnabled", PlatformAffected.UWP)]
	public class Issue3988 : TestContentPage
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout();
			var buttonNav = new Button() { Text = "Test navigation page" };
			buttonNav.Clicked += (s, e) => Application.Current.MainPage = new Issue3988NavigationPageTest();
			var buttonMaster = new Button() { Text = "Test master detail page" };
			buttonMaster.Clicked += (s, e) => Application.Current.MainPage = new Issue3988FlyoutPageTest();
			var buttonTabbed = new Button() { Text = "Test tabbed page" };
			buttonTabbed.Clicked += (s, e) => Application.Current.MainPage = new Issue3988TabbedPageTest();
			var text = new Label()
			{
				Text =
					"Click buttons to test dynamic toolbar overflow on different types of pages",
				LineBreakMode = LineBreakMode.WordWrap
			};
			stackLayout.Children.Add(text);
			stackLayout.Children.Add(buttonNav);
			stackLayout.Children.Add(buttonMaster);
			stackLayout.Children.Add(buttonTabbed);
			Content = stackLayout;
		}
	}

	public class Issue3988NavigationPageTest : NavigationPage
	{
		public Issue3988NavigationPageTest()
		{
			var page = Issue3988PageSetup.CreateTogglePage(this);
			PushAsync(page);
			Issue3988PageSetup.AddToolbarItems(this);
		}
	}

	public class Issue3988FlyoutPageTest : FlyoutPage
	{
		public Issue3988FlyoutPageTest()
		{
			var page = Issue3988PageSetup.CreateTogglePage(this);
			page.Title = "Test page";
			Flyout = new ContentPage() { Title = "master" };
			Detail = new NavigationPage(page);
			Issue3988PageSetup.AddToolbarItems(this);
		}
	}

	public class Issue3988TabbedPageTest : TabbedPage
	{
		public Issue3988TabbedPageTest()
		{
			var page = Issue3988PageSetup.CreateTogglePage(this);
			page.Title = "Test tab";
			page.Appearing += (s, e) => Issue3988PageSetup.AddToolbarItems(page);
			Children.Add(new NavigationPage(page));

		}
	}

	public static class Issue3988PageSetup
	{
		public static ContentPage CreateTogglePage(Page parent)
		{
			StackLayout stackLayout = new StackLayout();
			var button = new Button() { Text = "Toggle IsDynamicOverflowEnabled" };
			button.Clicked += (s, e) =>
			{
				var overflowEnabled = parent.On<WindowsOS>().GetToolbarDynamicOverflowEnabled();
				parent.On<WindowsOS>().SetToolbarDynamicOverflowEnabled(!overflowEnabled);
			};
			var text = new Label()
			{
				Text =
					"Shrink the app window size so that not all toolbar items are visible. By default, items should overflow to secondary menu, but this can be disabled by clicking the toggle button",
				LineBreakMode = LineBreakMode.WordWrap
			};
			stackLayout.Children.Add(text);
			stackLayout.Children.Add(button);
			return new ContentPage
			{
				Content = stackLayout
			};
		}

		public static void AddToolbarItems(Page parent)
		{
			for (int i = 0; i < 10; i++)
			{
				parent.ToolbarItems.Add(new ToolbarItem("Test", "coffee.png", () => { }));
			}
		}
	}
}