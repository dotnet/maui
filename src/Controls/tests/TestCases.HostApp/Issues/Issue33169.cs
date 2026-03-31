using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33169, "[iOS] RefreshView with CollectionView shows graphical glitches when Large Page Titles are enabled", PlatformAffected.iOS)]
public class Issue33169 : TestNavigationPage
{
	protected override void Init()
	{
		On<iOS>().SetPrefersLargeTitles(true);

		var statusLabel = new Label
		{
			Text = "None",
			AutomationId = "StatusLabel",
			FontSize = 20,
		};

		var items = Enumerable.Range(0, 30).Select(i => $"Item {i}").ToList();

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Margin = new Thickness(10) };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
		};

		var refreshView = new RefreshView
		{
			AutomationId = "TestRefreshView",
			Content = collectionView,
		};

		refreshView.Command = new Command(async () =>
		{
			statusLabel.Text = "Refreshing";
			await Task.Delay(500);
			refreshView.IsRefreshing = false;
			statusLabel.Text = "SUCCESS";
		});

		var page = new ContentPage
		{
			Title = "RefreshView Large Titles",
			Content = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star),
				},
			}
		};

		Grid.SetRow(statusLabel, 0);
		Grid.SetRow(refreshView, 1);
		((Grid)page.Content).Children.Add(statusLabel);
		((Grid)page.Content).Children.Add(refreshView);

		page.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);
		Navigation.PushAsync(page);
	}
}
