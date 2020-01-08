using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9006, "[Bug] Unable to open a new Page for the second time in Xamarin.Forms Shell Tabbar",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue9006 : TestShell
	{
		protected override void Init()
		{
			Routing.RegisterRoute("Issue9006_ContentPage", typeof(ContentPage));
			Routing.RegisterRoute("Issue9006_FinalPage", typeof(ContentPage));

			var contentPage = AddBottomTab("Tab 1");
			Items[0].CurrentItem.AutomationId = "Tab1AutomationId";
			AddBottomTab("Ignore Me");

			Label label = new Label()
			{
				Text = "Clicking on the first tab should pop you back to the root",
				AutomationId = "FinalLabel"
			};

			Button button = null;
			bool navigated = false;
			button = new Button()
			{
				Text = "Click Me",
				AutomationId = "Click Me",
				Command = new Command(async () =>
				{
					await GoToAsync("Issue9006_ContentPage");
					await GoToAsync("Issue9006_FinalPage");
					
					button.Text = "Click me again. If pages get pushed again then test has passed.";
					DisplayedPage.Content = new StackLayout()
					{
						Children =
						{
							label
						}
					};
					if (navigated)
						label.Text = "Success";

					navigated = true;
				})
			};

			contentPage.Content = new StackLayout()
			{
				Children =
				{
					button
				}
			};
		}


#if UITEST && __IOS__
		[Test]
		public void ClickingOnTabToPopToRootDoesntBreakNavigation()
		{
			RunningApp.Tap("Click Me");
			RunningApp.WaitForElement("FinalLabel");
			RunningApp.Tap("Tab1AutomationId");
			RunningApp.Tap("Click Me");
			RunningApp.Tap("Success");
		}
#endif
	}
}
