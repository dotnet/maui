using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 35756, "OnNavigatedTo does not fire after PopModalAsync when tab was changed from inside the modal", PlatformAffected.All)]
	public class Issue35756TabbedPage : TabbedPage
	{
		public Issue35756TabbedPage()
		{
			Title = "Issue35756";
			Children.Add(new Issue35756Tab1Page());
			Children.Add(new Issue35756Tab2Page(this));
		}
	}

	public class Issue35756Tab1Page : ContentPage
	{
		readonly Label _navigatedToCountLabel;
		int _navigatedToCount;

		public Issue35756Tab1Page()
		{
			Title = "Tab 1";
			_navigatedToCountLabel = new Label
			{
				AutomationId = "Tab1NavigatedToCount",
				Text = "NavigatedTo count: 0"
			};
			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label { Text = "Tab 1", FontSize = 18, AutomationId = "Tab1Content" },
					_navigatedToCountLabel
				}
			};
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);
			_navigatedToCount++;
			_navigatedToCountLabel.Text = $"NavigatedTo count: {_navigatedToCount}";
		}
	}

	public class Issue35756Tab2Page : ContentPage
	{
		readonly TabbedPage _tabbedPage;

		public Issue35756Tab2Page(TabbedPage tabbedPage)
		{
			Title = "Tab 2";
			_tabbedPage = tabbedPage;

			var pushModalButton = new Button
			{
				Text = "Push Modal",
				AutomationId = "PushModalButton"
			};
			pushModalButton.Clicked += OnPushModalClicked;

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label { Text = "Tab 2", FontSize = 18, AutomationId = "Tab2Content" },
					pushModalButton
				}
			};
		}

		async void OnPushModalClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new Issue35756ModalPage(_tabbedPage));
		}
	}

	public class Issue35756ModalPage : ContentPage
	{
		readonly TabbedPage _tabbedPage;

		public Issue35756ModalPage(TabbedPage tabbedPage)
		{
			Title = "Modal";
			_tabbedPage = tabbedPage;

			var switchTabButton = new Button
			{
				Text = "Switch to Tab 1",
				AutomationId = "SwitchToTab1Button"
			};
			switchTabButton.Clicked += OnSwitchToTab1Clicked;

			var closeModalButton = new Button
			{
				Text = "Close Modal",
				AutomationId = "CloseModalButton"
			};
			closeModalButton.Clicked += OnCloseModalClicked;

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label { Text = "Modal Page", FontSize = 18, AutomationId = "ModalContent" },
					switchTabButton,
					closeModalButton
				}
			};
		}

		void OnSwitchToTab1Clicked(object sender, EventArgs e)
		{
			_tabbedPage.CurrentPage = _tabbedPage.Children[0];
		}

		async void OnCloseModalClicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}
