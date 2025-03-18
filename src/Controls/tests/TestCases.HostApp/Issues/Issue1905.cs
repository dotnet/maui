using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using ListView = Microsoft.Maui.Controls.ListView;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1905, "Pull to refresh doesn't work if iOS 11 large titles is enabled", PlatformAffected.iOS)]
	public class Issue1905LargeTitles : TestNavigationPage
	{
		protected override void Init()
		{
			On<iOS>().SetPrefersLargeTitles(true);
			var items = new List<string>();
			for (int i = 0; i < 1000; i++)
			{
				items.Add($"pull to {DateTime.Now.Ticks}");
			}
			var page = new ContentPage
			{
				Title = "Pull Large Titles"
			};

			var lst = new ListView();
			lst.IsPullToRefreshEnabled = true;
			lst.ItemsSource = items;
			lst.RefreshCommand = new Command(async () =>
			{
				var newitems = new List<string>();
				newitems.Add("data refreshed");
				await Task.Delay(5000);
				for (int i = 0; i < 1000; i++)
				{
					newitems.Add($"data {DateTime.Now.Ticks} refreshed");
				}
				lst.ItemsSource = newitems;
				lst.EndRefresh();
			});
			page.Content = new StackLayout { Children = { lst } };
			page.Appearing += async (sender, e) =>
			{
				await Task.Delay(500);
				lst.BeginRefresh();
			};
			page.ToolbarItems.Add(new ToolbarItem { Text = "Refresh", Command = new Command((obj) => lst.BeginRefresh()), AutomationId = "btnRefresh" });
			Navigation.PushAsync(page);

		}
	}
}