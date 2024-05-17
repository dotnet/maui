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
	[Issue(IssueTracker.Github, 12320, "[iOS] TabBarIsVisible = True/False doesn't work on Back Navigation When using BackButtonBehavior",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue12320 : TestShell
	{
		bool firstNavigated = true;
		protected override void Init()
		{
			var page1 = new ContentPage();
			page1.Content = new Label()
			{
				Text = "If you don't see any bottom tabs the test has failed"
			};

			AddFlyoutItem(page1, "Tab 1");
			AddBottomTab("Tab 2");
			Shell.SetTabBarIsVisible(page1, true);
		}

		protected override async void OnNavigated(ShellNavigatedEventArgs args)
		{
			base.OnNavigated(args);

			if (firstNavigated)
			{
				firstNavigated = false;
				ContentPage contentPage = new ContentPage();
				contentPage.Content = new Label()
				{
					Text = "Click the Coffee Cup in the Nav Bar",
					AutomationId = "TestReady"
				};

				Shell.SetTabBarIsVisible(contentPage, false);
				Shell.SetBackButtonBehavior(contentPage, new BackButtonBehavior() { IconOverride = "coffee.png" });
				await Navigation.PushAsync(contentPage);
			}
		}


#if UITEST && __SHELL__
		[Test]
		public void PopLogicExecutesWhenUsingBackButtonBehavior()
		{
			RunningApp.WaitForElement("TestReady");
			base.TapBackArrow();
			RunningApp.WaitForElement("Tab 1");
		}
#endif
	}
}
