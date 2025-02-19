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
	[Issue(IssueTracker.Github, 12126, "[iOS] TabBarIsVisible = True/False breaking for multiple nested pages",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue12126 : TestShell
	{
		bool firstNavigated = true;
		protected override void Init()
		{
			var page1 = AddFlyoutItem("Tab 1");
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
					Text = "If you don't see any bottom tabs the test has failed"
				};
				Shell.SetTabBarIsVisible(contentPage, true);

				ContentPage contentPage2 = new ContentPage();
				contentPage2.Content =
					new StackLayout()
					{
						Children =
						{
							new Label()
							{
								Text = "Click The Back Arrow",
								AutomationId = "TestReady"
							}
						}
					};

				Shell.SetTabBarIsVisible(contentPage2, false);
				await Navigation.PushAsync(contentPage);
				await Navigation.PushAsync(contentPage2);
			}
		}


#if UITEST && __SHELL__
		[Test]
		public void NavigatingBackFromMultiplePushPagesChangesTabVisibilityCorrectly()
		{
			RunningApp.WaitForElement("TestReady");
			TapBackArrow();
			RunningApp.WaitForElement("Tab 1");
		}
#endif
	}
}
