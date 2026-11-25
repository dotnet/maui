using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "21814_TabbedPage", "Add better parameters for navigation args (TabbedPage)", PlatformAffected.All)]
	public class Issue21814TabbedPage : TabbedPage
	{
		public Issue21814TabbedPage()
		{
			Title = "Issue21814 TabbedPage";

			// Add the tabs
			Children.Add(new Issue21814TabItem1());
			Children.Add(new Issue21814TabItem2());
			Children.Add(new Issue21814TabItem3());
			Children.Add(new ContentPage { Title = "Tab 4" });
			Children.Add(new ContentPage { Title = "Tab 5" });
			var morePage = new Issue21814MoreTab();
			Children.Add(morePage);
			CurrentPageChanged += (_, _) => morePage.UpdateSelection(CurrentPage == morePage);
		}
	}

	// Issue 16175: selecting a child from iOS More must raise OnNavigatedTo.
	public class Issue21814MoreTab : ContentPage
	{
		readonly Label _navigationStatus = new Label { AutomationId = "MoreTabNavigationStatus", Text = "Not navigated" };
		readonly Label _selectionStatus = new Label { AutomationId = "MoreTabSelectionStatus", Text = "Not selected" };

		public Issue21814MoreTab()
		{
			Title = "Tab 6";
			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label { AutomationId = "MoreTabContent", Text = "Tab 6 content" },
					_navigationStatus,
					_selectionStatus
				}
			};
		}

		public void UpdateSelection(bool selected) => _selectionStatus.Text = selected ? "Selected" : "Not selected";

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);
			_navigationStatus.Text = "Navigated";
		}
	}

	public class Issue21814TabItem1 : ContentPage
	{
		readonly Label _onNavigatedToLabel;
		readonly Label _onNavigatingFromLabel;
		readonly Label _onNavigatedFromLabel;

		public Issue21814TabItem1()
		{
			Title = "Tab 1";

			_onNavigatedToLabel = new Label { AutomationId = "Tab1OnNavigatedToLabel", Text = "-" };
			_onNavigatingFromLabel = new Label { AutomationId = "Tab1OnNavigatingFromLabel", Text = "-" };
			_onNavigatedFromLabel = new Label { AutomationId = "Tab1OnNavigatedFromLabel", Text = "-" };

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label { AutomationId = "Tab1Content", Text = "Tab 1 Content", FontSize = 18 },
					new Label { Text = "OnNavigated", FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 12)},
					_onNavigatedToLabel,
					new Label { Text = "OnNavigatingFrom", FontAttributes = FontAttributes.Bold },
					_onNavigatingFromLabel,
					new Label { Text = "OnNavigatedFrom", FontAttributes = FontAttributes.Bold },
					_onNavigatedFromLabel
				}
			};
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);

			var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
			_onNavigatedToLabel.Text = $"PreviousPage: {previousPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
		{
			base.OnNavigatingFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			_onNavigatingFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			base.OnNavigatedFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			_onNavigatedFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}
	}

	public class Issue21814TabItem2 : ContentPage
	{
		readonly Label _onNavigatedToLabel;
		readonly Label _onNavigatingFromLabel;
		readonly Label _onNavigatedFromLabel;

		public Issue21814TabItem2()
		{
			Title = "Tab 2";

			_onNavigatedToLabel = new Label { AutomationId = "Tab2OnNavigatedToLabel", Text = "-" };
			_onNavigatingFromLabel = new Label { AutomationId = "Tab2OnNavigatingFromLabel", Text = "-" };
			_onNavigatedFromLabel = new Label { AutomationId = "Tab2OnNavigatedFromLabel", Text = "-" };

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label { AutomationId = "Tab2Content", Text = "Tab 2 Content", FontSize = 18 },
					new Label { Text = "OnNavigated", FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 12)},
					_onNavigatedToLabel,
					new Label { Text = "OnNavigatingFrom", FontAttributes = FontAttributes.Bold },
					_onNavigatingFromLabel,
					new Label { Text = "OnNavigatedFrom", FontAttributes = FontAttributes.Bold },
					_onNavigatedFromLabel
				}
			};
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);

			var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
			_onNavigatedToLabel.Text = $"PreviousPage: {previousPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
		{
			base.OnNavigatingFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			_onNavigatingFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			base.OnNavigatedFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			_onNavigatedFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}
	}

	public class Issue21814TabItem3 : ContentPage
	{
		readonly Label _onNavigatedToLabel;
		readonly Label _onNavigatingFromLabel;
		readonly Label _onNavigatedFromLabel;

		public Issue21814TabItem3()
		{
			Title = "Tab 3";

			_onNavigatedToLabel = new Label { AutomationId = "Tab3OnNavigatedToLabel", Text = "-" };
			_onNavigatingFromLabel = new Label { AutomationId = "Tab3OnNavigatingFromLabel", Text = "-" };
			_onNavigatedFromLabel = new Label { AutomationId = "Tab3OnNavigatedFromLabel", Text = "-" };

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					new Label { AutomationId = "Tab3Content", Text = "Tab 3 Content", FontSize = 18 },
					new Label { Text = "OnNavigated", FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 12)},
					_onNavigatedToLabel,
					new Label { Text = "OnNavigatingFrom", FontAttributes = FontAttributes.Bold },
					_onNavigatingFromLabel,
					new Label { Text = "OnNavigatedFrom", FontAttributes = FontAttributes.Bold },
					_onNavigatedFromLabel
				}
			};
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);

			var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
			_onNavigatedToLabel.Text = $"PreviousPage: {previousPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
		{
			base.OnNavigatingFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			_onNavigatingFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}

		protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			base.OnNavigatedFrom(args);

			var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
			_onNavigatedFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
		}
	}
}