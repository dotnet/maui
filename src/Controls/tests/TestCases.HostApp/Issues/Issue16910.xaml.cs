namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16910, "IsRefreshing binding works", PlatformAffected.All)]
	public partial class Issue16910 : ContentPage
	{
		bool _isRefreshing;

		public bool IsRefreshing
		{
			get => _isRefreshing;
			set
			{
				_isRefreshing = value;
				OnPropertyChanged(nameof(IsRefreshing));
			}
		}

		public Issue16910()
		{
			InitializeComponent();
			BindingContext = this;
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
	}
}