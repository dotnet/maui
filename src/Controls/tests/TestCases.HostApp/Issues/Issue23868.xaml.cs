#nullable enable
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23868, "CollectionView with RefreshView Throws Exception During Pull-to-Refresh Actions", PlatformAffected.iOS)]
	public partial class Issue23868 : ContentPage
	{
		private ObservableCollection<string> _items = new ObservableCollection<string>();

		public ObservableCollection<string> Items
		{
			get => _items;
			set
			{
				_items = value;
				collectionView.ItemsSource = _items;
			}
		}

		public Command PullToRefreshCommand { get; }

		public Issue23868()
		{
			InitializeComponent();
			Items = new ObservableCollection<string>();
			BindingContext = this;

			PullToRefreshCommand = new Command(async () => await SimulateHttpRequest());
		}

		private async Task SimulateHttpRequest()
		{
			refreshView.IsRefreshing = true;

			// Simulate delay for data fetching
			await Task.Delay(200);

			// Simulated local JSON data
			string jsonData = "[\"Local Item 1\", \"Local Item 2\", \"Local Item 3\"]";

			// Parse the local data as a response
			var items = JsonSerializer.Deserialize<string[]>(jsonData);

			// Update ObservableCollection with fetched data
			if (items != null)
			{
				Items.Clear();
				foreach (var item in items)
				{
					Items.Add(item);
				}
			}

			refreshView.IsRefreshing = false;
		}

		private async void OnUpdateDataClicked(object sender, EventArgs e)
		{
			await SimulateHttpRequest();
		}

		private void OnClearDataClicked(object sender, EventArgs e)
		{
			Items.Clear();
		}

		private void OnRefreshing(object sender, EventArgs e)
		{
			PullToRefreshCommand.Execute(null);
		}
	}
}