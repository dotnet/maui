using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1557, "Setting source crashes if view was detached from visual tree", PlatformAffected.iOS)]
public class Issue1557 : NavigationPage
{
	public Issue1557()
	{

		var button = new Button { Text = "Bug Repro" };
		var contentPage = new ContentPage();
		contentPage.Content = button;
		button.Clicked += async (sender, e) => await Navigation.PushAsync(new Issue1557Page());
		Navigation.PushAsync(contentPage);
	}
}
public class Issue1557Page : ContentPage
{
	const int Delay = 3000;

	ObservableCollection<string> _items = new ObservableCollection<string> { "foo", "bar" };

	public Issue1557Page()
	{
		var listView = new ListView
		{
			ItemsSource = _items
		};

		Content = listView;

		Task.Delay(Delay).ContinueWith(async t =>
		{
			var list = (ListView)Content;

			await Navigation.PopAsync();

			list.ItemsSource = new List<string> { "test" };
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}
}
