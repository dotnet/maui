using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11869, "[Bug] ShellContent.IsVisible issue on Android",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue11869 : TestShell
	{
		protected override void Init()
		{
			ContentPage contentPage = new ContentPage();
			var tabbar = AddContentPage<TabBar, Tab>(contentPage, title: "Tab 1");
			AddBottomTab("Tab 2");
			AddBottomTab("Tab 3");
			AddTopTab("TopTab2");
			AddTopTab("TopTab3");

			contentPage.Content =
				new StackLayout()
				{
					Children =
					{
						new Button
						{
							Text = "Hide Bottom Tab 2",
							Command = new Command(() =>
							{
								Items[0].Items[1].Items[0].IsVisible = false;
							}),
							AutomationId = "HideBottom2"
						},
						new Button
						{
							Text = "Hide Bottom Tab 3",
							Command = new Command(() =>
							{
								Items[0].Items[2].Items[0].IsVisible = false;
							}),
							AutomationId = "HideBottom3"
						},
						new Button
						{
							Text = "Hide Top Tab 2",
							Command = new Command(() =>
							{
								Items[0].Items[0].Items[1].IsVisible = false;
							}),
							AutomationId = "HideTop2"
						},
						new Button
						{
							Text = "Hide Top Tab 3",
							Command = new Command(() =>
							{
								Items[0].Items[0].Items[2].IsVisible = false;
							}),
							AutomationId = "HideTop3"
						},
						new Button
						{
							Text = "Show All Tabs",
							Command = new Command(() =>
							{
								Items[0].Items[1].Items[0].IsVisible = true;
								Items[0].Items[2].Items[0].IsVisible = true;
								Items[0].Items[0].Items[1].IsVisible = true;
								Items[0].Items[0].Items[2].IsVisible = true;
							}),
							AutomationId = "ShowAllTabs"
						}
					}
				};
		}


#if UITEST && __SHELL__
		[Test]
		public void IsVisibleWorksForShowingHidingTabs()
		{
			RunningApp.WaitForElement("TopTab2");
			RunningApp.Tap("HideTop2");
			RunningApp.WaitForNoElement("TopTab2");

			RunningApp.WaitForElement("TopTab3");
			RunningApp.Tap("HideTop3");
			RunningApp.WaitForNoElement("TopTab3");

			RunningApp.WaitForElement("Tab 2");
			RunningApp.Tap("HideBottom2");
			RunningApp.WaitForNoElement("Tab 2");

			RunningApp.WaitForElement("Tab 3");
			RunningApp.Tap("HideBottom3");
			RunningApp.WaitForNoElement("Tab 3");

			RunningApp.Tap("ShowAllTabs");
			RunningApp.WaitForElement("TopTab2");
			RunningApp.WaitForElement("TopTab3");
			RunningApp.WaitForElement("Tab 2");
			RunningApp.WaitForElement("Tab 3");
		}
#endif
	}
}
