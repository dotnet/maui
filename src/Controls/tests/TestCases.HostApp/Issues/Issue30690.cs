using System.Linq;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 30690, "RefreshView IsRefreshEnabled Property and Consistent IsEnabled Behavior", PlatformAffected.All)]
	public class Issue30690 : TestContentPage
	{
		RefreshView _refreshView;
		Label _statusLabel;
		Entry _testEntry;

		public Issue30690()
		{
		}

		protected override void Init()
		{
			Title = "RefreshView IsRefreshEnabled Tests";

			// Create status label to show current states
			_statusLabel = new Label 
			{ 
				AutomationId = "StatusLabel",
				Text = "Ready to test",
				BackgroundColor = Colors.LightGray,
				Padding = 10
			};

			// Create a test entry to verify child control interaction
			_testEntry = new Entry
			{
				AutomationId = "TestEntry",
				Placeholder = "Type here to test child interaction",
				BackgroundColor = Colors.LightBlue,
				Margin = 10
			};

			// Create scrollable content
			var scrollViewContent = new StackLayout();
			scrollViewContent.Children.Add(_testEntry);
			
			Enumerable
				.Range(0, 5)
				.Select(i => new Label() 
				{ 
					HeightRequest = 100, 
					Text = $"Item {i + 1} - Pull down to refresh",
					BackgroundColor = i % 2 == 0 ? Colors.LightGreen : Colors.LightYellow,
					VerticalTextAlignment = TextAlignment.Center,
					HorizontalTextAlignment = TextAlignment.Center
				})
				.ToList()
				.ForEach(scrollViewContent.Children.Add);

			// Create refresh view
			_refreshView = new RefreshView()
			{
				AutomationId = "TestRefreshView",
				Content = new ScrollView()
				{
					Content = scrollViewContent,
					AutomationId = "ScrollViewContent"
				},
				Command = new Command(async () =>
				{
					UpdateStatusLabel("Refreshing...");
					await Task.Delay(1000);
					_refreshView.IsRefreshing = false;
					UpdateStatusLabel("Refresh completed");
				})
			};

			// Update status when refresh state changes
			_refreshView.Refreshing += (sender, args) => UpdateStatusLabel("Refresh started");

			Content = new StackLayout()
			{
				Padding = 10,
				Children =
				{
					_statusLabel,
					
					// Control buttons
					new Button()
					{
						AutomationId = "ToggleIsRefreshEnabled",
						Text = "Toggle IsRefreshEnabled",
						Command = new Command(() =>
						{
							_refreshView.IsRefreshEnabled = !_refreshView.IsRefreshEnabled;
							UpdateStatusLabel($"IsRefreshEnabled: {_refreshView.IsRefreshEnabled}");
						})
					},
					
					new Button()
					{
						AutomationId = "ToggleIsEnabled",
						Text = "Toggle IsEnabled",
						Command = new Command(() =>
						{
							_refreshView.IsEnabled = !_refreshView.IsEnabled;
							UpdateStatusLabel($"IsEnabled: {_refreshView.IsEnabled}");
						})
					},
					
					new Button()
					{
						AutomationId = "StartRefresh",
						Text = "Start Refresh Programmatically",
						Command = new Command(() =>
						{
							_refreshView.IsRefreshing = true;
						})
					},
					
					new Button()
					{
						AutomationId = "CheckStates",
						Text = "Check Current States",
						Command = new Command(() =>
						{
							UpdateStatusLabel($"IsEnabled: {_refreshView.IsEnabled}, IsRefreshEnabled: {_refreshView.IsRefreshEnabled}, IsRefreshing: {_refreshView.IsRefreshing}");
						})
					},

					new Button()
					{
						AutomationId = "TestEntryFocus",
						Text = "Focus Entry",
						Command = new Command(() =>
						{
							_testEntry.Focus();
							UpdateStatusLabel("Entry focus attempted");
						})
					},
					
					_refreshView
				}
			};

			// Initial status
			UpdateStatusLabel($"Initial - IsEnabled: {_refreshView.IsEnabled}, IsRefreshEnabled: {_refreshView.IsRefreshEnabled}");
		}

		private void UpdateStatusLabel(string message)
		{
			_statusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
		}
	}
}