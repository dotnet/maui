namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16910, "IsRefreshing binding works", PlatformAffected.All)]
	public partial class Issue16910 : ContentPage
	{
		Label _isRefreshingLabel = new Label() { Text = "Is Refreshing", AutomationId = "IsRefreshing" };
		Label _isNotRefreshingLabel = new Label() { Text = "Is Not Refreshing", AutomationId = "IsNotRefreshing" };

		bool _isRefreshing;

		public bool IsRefreshing
		{
			get => _isRefreshing;
			set
			{
				_isRefreshing = value;
				OnPropertyChanged(nameof(IsRefreshing));
				UpdateRefreshingLabels();
			}
		}

		void UpdateRefreshingLabels()
		{
			if (IsRefreshing)
			{
				_isNotRefreshingLabel.IsVisible = false;
				_isRefreshingLabel.IsVisible = true;
				StopRefreshingButton.IsVisible = true;
			}
			else
			{
				_isRefreshingLabel.IsVisible = false;
				_isNotRefreshingLabel.IsVisible = true;
				StopRefreshingButton.IsVisible = false;
			}
		}

		public Issue16910()
		{
			InitializeComponent();
			BindingContext = this;

			// Insert status labels at top (before TestResult)
			var layout = (VerticalStackLayout)Content;
			layout.Insert(0, _isRefreshingLabel);
			layout.Insert(0, _isNotRefreshingLabel);
			UpdateRefreshingLabels();
		}

		void OnRunTestClicked(object sender, EventArgs e)
		{
			// Test programmatic refresh start — binding should propagate to ViewModel
			refreshView.IsRefreshing = true;
			if (!IsRefreshing)
			{
				TestResultLabel.Text = "FAIL: IsRefreshing did not propagate to true";
				return;
			}

			// Test programmatic refresh stop — binding should propagate to ViewModel
			refreshView.IsRefreshing = false;
			if (IsRefreshing)
			{
				TestResultLabel.Text = "FAIL: IsRefreshing did not propagate to false";
				return;
			}

			TestResultLabel.Text = "SUCCESS";
		}

		void OnStopRefreshClicked(object sender, EventArgs e)
		{
			refreshView.IsRefreshing = false;
		}
	}
}