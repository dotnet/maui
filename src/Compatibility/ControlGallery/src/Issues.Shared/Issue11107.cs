using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
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
	[Issue(IssueTracker.Github, 11107, "[Bug][iOS] Shell Navigation implicitly adds Tabbar",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue11107 : TestShell
	{
		bool _tabBarIsVisible = false;
		Label _tabBarLabel;
		public Issue11107() : this(false)
		{
		}

		public Issue11107(bool tabBarIsVisible)
		{
			_tabBarIsVisible = tabBarIsVisible;
#if !UITEST
			Shell.SetTabBarIsVisible(this, _tabBarIsVisible);
			Shell.SetNavBarHasShadow(this, false);
			_tabBarLabel.Text = $"TabBarIsVisible: {_tabBarIsVisible}";
#endif
		}

		protected override void Init()
		{
			_tabBarLabel = new Label();
			ContentPage firstPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						_tabBarLabel,
						new Label()
						{
							Text = "If this page has a tab bar the test has failed",
							AutomationId = "Page1Loaded"
						},
						new Button()
						{
							Text = "Run test again with TabBarIsVisible Toggled",
							Command = new Command(() =>
							{
								Application.Current.MainPage = new Issue11107(!Shell.GetTabBarIsVisible(this));
							}),
							AutomationId = "RunTestTabBarIsVisible"
						},
						new Button()
						{
							Text = "Run with Two Tabs and TabBarIsVisible False",
							Command = new Command(() =>
							{
								var shell = new Issue11107(false);
								shell.AddBottomTab("Second Tab");
								Application.Current.MainPage = shell;
							}),
							AutomationId = "RunTestTwoTabs"
						}
					}
				}
			};

			ContentPage secondPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Hold Please!! Or fail the test if nothing happens."
						}
					}
				}
			};

			var item1 = AddFlyoutItem(firstPage, "Page1");
			item1.Items[0].Title = "Tab 1";
			item1.Items[0].AutomationId = "Tab1AutomationId";
			var item2 = AddFlyoutItem(secondPage, "Page2");

			item1.Route = "FirstPage";
			Routing.RegisterRoute("Issue11107HeaderPage", typeof(Issue11107HeaderPage));

			CurrentItem = item2;

			Device.BeginInvokeOnMainThread(async () =>
			{
				await Task.Delay(1000);
				await GoToAsync("//FirstPage/Issue11107HeaderPage");
			});
		}

		[Preserve(AllMembers = true)]
		public class Issue11107HeaderPage : ContentPage
		{
			public Issue11107HeaderPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "If this page has a tab bar the test has failed",
							AutomationId = "SecondPageLoaded"
						},
						new Label()
						{
							Text = "Click the Back Button"
						}
					}
				};
			}
		}


#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TabShouldntBeVisibleWhenThereIsOnlyOnePage()
		{
			RunTests();
			RunningApp.Tap("RunTestTabBarIsVisible");
			RunTests();
			RunningApp.Tap("RunTestTwoTabs");
			RunTests();

			void RunTests()
			{
				RunningApp.WaitForElement("SecondPageLoaded");
				RunningApp.WaitForNoElement("Tab1AutomationId");
				TapBackArrow();
				RunningApp.WaitForElement("Page1Loaded");
				RunningApp.WaitForNoElement("Tab1AutomationId");
			}
		}
#endif
	}
}
