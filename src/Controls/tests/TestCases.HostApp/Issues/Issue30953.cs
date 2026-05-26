using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30953, "CollectionView does not update layout correctly when ItemsSource changes", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue30953 : ContentPage
{
	private ObservableCollection<string> Countries = new();

	public Issue30953()
	{
		LoadCountries();
		var collectionView = new CollectionView2();
		var button = new Button
		{
			Text = "Set ItemsSource",
			AutomationId = "Issue30953Button",
			HorizontalOptions = LayoutOptions.Center
		};

		button.Clicked += (sender, args) =>
		{
			collectionView.ItemsSource = Countries;
		};

		var label = new Label
		{
			Text = "The test passed if the CollectionView ItemsSource is set and the items are displayed correctly in runtime.",
			HorizontalOptions = LayoutOptions.Center
		};

		var stack = new VerticalStackLayout
		{
			Children = {
				label,
				button,
				collectionView
			}
		};

		Content = stack;
	}

	void LoadCountries()
	{
		Countries = new ObservableCollection<string>
		{
			"United States",
			"Canada",
			"United Kingdom",
			"Germany",
			"France",
			"Italy",
			"Spain",
			"Japan",
			"Australia",
			"Brazil",
			"India",
			"China",
			"Russia",
			"Mexico",
			"Argentina",
			"South Africa",
			"Egypt",
			"Turkey",
			"Netherlands",
			"Sweden"
		};
	}
}