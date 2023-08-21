using System;
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
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6738, "Flyout Navigation fails when coupled with tabs that have a stack", PlatformAffected.Android)]
	public class Issue6738 : TestShell
	{
		const string pushAutomationId = "PushPageButton";
		const string insertAutomationId = "InsertPageButton";
		const string returnAutomationId = "ReturnPageButton";
		const string flyoutMainTitle = "Main";
		const string flyoutOtherTitle = "Other Page";

		Tab flyoutContent = new Tab();
		Button pushPageButton = new Button { Text = "Tap to push new page to stack", AutomationId = pushAutomationId };
		Button insertPageButton = new Button { Text = "Push another page to the stack, then go to tab two", AutomationId = insertAutomationId };
		ContentPage pushedPage;
		Tab tabOne = new Tab { Title = "TabOne" };
		Tab tabTwo = new Tab { Title = "TabTwo " };


		void OnReturnTapped(object sender, EventArgs e)
		{
			ForceTabSwitch();
		}

		async void OnPushTapped(object sender, EventArgs e)
		{
			pushedPage = new ContentPage { Content = insertPageButton };
			await Navigation.PushAsync(pushedPage);
		}

		void OnInsertTapped(object sender, EventArgs e)
		{
			Navigation.InsertPageBefore(new ContentPage { Content = new Label { Text = "This is an extra page" } }, pushedPage);
			ForceTabSwitch();
		}

		void ForceTabSwitch()
		{
			if (CurrentItem != null)
			{
				if (CurrentItem.CurrentItem == tabOne)
				{
					CurrentItem.CurrentItem = tabTwo;
				}
				else
					CurrentItem.CurrentItem = tabOne;
			}
		}

		protected override void Init()
		{
			var tabOnePage = new ContentPage { Content = pushPageButton };
			var stackLayout = new StackLayout();
			stackLayout.Children.Add(new Label { Text = "If you've been here already, go to tab one now. Otherwise, go to Other Page in the flyout." });
			var returnButton = new Button { Text = "Go back to tab 1", AutomationId = returnAutomationId };
			returnButton.Clicked += OnReturnTapped;
			stackLayout.Children.Add(returnButton);

			var tabTwoPage = new ContentPage { Content = stackLayout };
			tabOne.Items.Add(tabOnePage);
			tabTwo.Items.Add(tabTwoPage);

			pushPageButton.Clicked += OnPushTapped;
			insertPageButton.Clicked += OnInsertTapped;
			flyoutContent.Items.Add(new ContentPage { Content = new Label { Text = "Go back to main page via the flyout" } });

			Items.Add(
					new FlyoutItem
					{
						Title = flyoutMainTitle,
						Items = { tabOne, tabTwo }
					}
			);
			Items.Add(new FlyoutItem
			{
				Title = flyoutOtherTitle,
				Items = { flyoutContent }
			});
		}

#if UITEST && __SHELL__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void FlyoutNavigationBetweenItemsWithNavigationStacks()
		{
			RunningApp.WaitForElement(pushAutomationId);
			RunningApp.Tap(pushAutomationId);
			RunningApp.WaitForElement(insertAutomationId);
			RunningApp.Tap(insertAutomationId);

			TapInFlyout(flyoutOtherTitle, timeoutMessage: flyoutOtherTitle);
			TapInFlyout(flyoutMainTitle, timeoutMessage: flyoutMainTitle);

			RunningApp.WaitForElement(returnAutomationId);
			RunningApp.Tap(returnAutomationId);
			RunningApp.NavigateBack();
			RunningApp.NavigateBack();
		}
#endif
	}
}
