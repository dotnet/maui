using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6384, "content page in tabbed page not showing inside shell tab", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Github6384 : TestShell
	{
		protected override void Init()
		{
			var tabOneButton = new Button
			{
				AutomationId = "NavigationButton",
				Text = "Push me!"
			};

			tabOneButton.Clicked += TabOneButton_Clicked;

			var tabOnePage = new ContentPage { Content = tabOneButton };

			var tabTwoPage = new ContentPage { Content = new Label { Text = "Go to TabOne" } };
			var tabOne = new Tab { Title = "TabOne" };
			var tabTwo = new Tab { Title = "TabTwo" };
			tabOne.Items.Add(tabOnePage);
			tabTwo.Items.Add(tabTwoPage);

			Items.Add(
					new TabBar
					{
						Items = { tabOne, tabTwo }
					}
			);
		}

		private void TabOneButton_Clicked(object sender, System.EventArgs e)
		{
			var subTabPageOne = new ContentPage
			{
				Content = new Label
				{
					Text = "SubPage One",
					AutomationId = "SubTabLabel1",
					VerticalTextAlignment = TextAlignment.Center,
				}
			};
			var subTabPageTwo = new ContentPage
			{
				Content = new Label
				{
					Text = "SubPage Two",
					AutomationId = "SubTabLabel2",
					VerticalTextAlignment = TextAlignment.Center,
				}
			};

			var tabbedPage = new TabbedPage { Title = "TabbedPage" };
			tabbedPage.Children.Add(subTabPageOne);
			tabbedPage.Children.Add(subTabPageTwo);
			Shell.SetTabBarIsVisible(tabbedPage, false);
			this.Navigation.PushAsync(tabbedPage);
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void Github6384Test()
		{
			RunningApp.Screenshot("I am at Github6384");
			RunningApp.WaitForElement(q => q.Marked("NavigationButton"));
			RunningApp.Tap("NavigationButton");
			RunningApp.WaitForElement(q => q.Marked("SubTabLabel1"));
			// The label is visible!
			// Note: This check only catches the bug on iOS. Android will pass also without the fix.
			RunningApp.Screenshot("The new page is visible!");
		}
#endif
	}
}
