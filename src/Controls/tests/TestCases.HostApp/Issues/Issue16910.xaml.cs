using System.Collections;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16910, "IsRefreshing binding works", PlatformAffected.All)]
	public partial class Issue16910 : ContentPage
	{
		Label _isRefreshingLabel = new Label() { Text = "Is Refreshing", AutomationId = "IsRefreshing" };
		Label _isNotRefreshingLabel = new Label() { Text = "Is Not Refreshing", AutomationId = "IsNotRefreshing" };

		bool _isRefreshing;

		public IEnumerable ItemSource { get; set; }

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
				grid.Remove(_isNotRefreshingLabel);
				grid.Insert(0, _isRefreshingLabel);
				StartRefreshing.IsVisible = false;
				StopRefreshing.IsVisible = true;
			}
			else
			{
				grid.Remove(_isRefreshingLabel);
				grid.Insert(0, _isNotRefreshingLabel);
				StartRefreshing.IsVisible = true;
				StopRefreshing.IsVisible = false;
			}
		}

		public Issue16910()
		{
			InitializeComponent();
			UpdateRefreshingLabels();
			ItemSource =
				Enumerable.Range(0, 100)
					.Select(x => new { Text = $"Item {x}", AutomationId = $"Item{x}" })
					.ToList();

			this.BindingContext = this;
		}


		void OnStopRefreshClicked(object sender, EventArgs e)
		{
			refreshView.IsRefreshing = false;
		}

		void OnRefreshClicked(object sender, EventArgs e)
		{
			refreshView.IsRefreshing = true;
		}
	}
}