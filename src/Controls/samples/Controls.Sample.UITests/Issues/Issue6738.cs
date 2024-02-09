using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	// src/Compatibility/ControlGallery/src/Issues.Shared/Issue6738.cs
	[Issue(IssueTracker.None, 6738, "Flyout Navigation fails when coupled with tabs that have a stack", PlatformAffected.Android)]
	public class Issue6738 : TestShell
	{
		const string PushAutomationId = "PushPageButton";
		const string InsertAutomationId = "InsertPageButton";
		const string ReturnAutomationId = "ReturnPageButton"; 
		const string FlyoutMainId = "Main";
		const string FlyoutMainTitle = "Main"; 
		const string FlyoutOtherId = "OtherPage";
		const string FlyoutOtherTitle = "Other Page";

		readonly Tab _flyoutContent = new Tab();
		readonly Button _pushPageButton = new Button { Text = "Tap to push new page to stack", AutomationId = PushAutomationId };
		readonly Button _insertPageButton = new Button { Text = "Push another page to the stack, then go to tab two", AutomationId = InsertAutomationId };
		ContentPage _pushedPage;
		readonly Tab _tabOne = new Tab { Title = "TabOne" };
		readonly Tab _tabTwo = new Tab { Title = "TabTwo " };

		protected override void Init()
		{
			var tabOnePage = new ContentPage { Content = _pushPageButton };
			var stackLayout = new StackLayout();
			stackLayout.Children.Add(new Label { Text = "If you've been here already, go to tab one now. Otherwise, go to Other Page in the flyout." });
			var returnButton = new Button { Text = "Go back to tab 1", AutomationId = ReturnAutomationId };
			returnButton.Clicked += OnReturnTapped;
			stackLayout.Children.Add(returnButton);

			var tabTwoPage = new ContentPage { Content = stackLayout };
			_tabOne.Items.Add(tabOnePage);
			_tabTwo.Items.Add(tabTwoPage);

			_pushPageButton.Clicked += OnPushTapped;
			_insertPageButton.Clicked += OnInsertTapped;
			_flyoutContent.Items.Add(new ContentPage { Content = new Label { Text = "Go back to main page via the flyout" } });

			Items.Add(new FlyoutItem
			{
				AutomationId = FlyoutMainId,
				Title = FlyoutMainTitle,
				Items = { _tabOne, _tabTwo }
			});

			Items.Add(new FlyoutItem
			{
				AutomationId = FlyoutOtherId,
				Title = FlyoutOtherTitle,
				Items = { _flyoutContent }
			});
		}

		void OnReturnTapped(object sender, EventArgs e)
		{
			ForceTabSwitch();
		}

		async void OnPushTapped(object sender, EventArgs e)
		{
			_pushedPage = new ContentPage { Content = _insertPageButton };
			await Navigation.PushAsync(_pushedPage);
		}

		void OnInsertTapped(object sender, EventArgs e)
		{
			Navigation.InsertPageBefore(new ContentPage { Content = new Label { Text = "This is an extra page" } }, _pushedPage);
			ForceTabSwitch();
		}

		void ForceTabSwitch()
		{
			if (CurrentItem != null)
			{
				if (CurrentItem.CurrentItem == _tabOne)
				{
					CurrentItem.CurrentItem = _tabTwo;
				}
				else
					CurrentItem.CurrentItem = _tabOne;
			}
		}
	}
}