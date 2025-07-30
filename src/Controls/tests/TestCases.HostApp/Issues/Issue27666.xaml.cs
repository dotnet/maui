using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27666, "Vertical list and Vertical grid pages have different abnormal behaviors when clicking Update after changing the spacing value", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue27666_NavigationPage : TestNavigationPage
{
	protected override void Init()
	{
		var root = CreateRootContentPage();
		PushAsync(root);
	}

	ContentPage CreateRootContentPage()
	{
		ContentPage ContentPage = new ContentPage();

		VerticalStackLayout rootLayout = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = new Thickness(10),
		};

		Button collectionViewButton = new Button
		{
			Text = "Navigate to VerticalList CollectionView",
			AutomationId = "NavigationButton"
		};
		collectionViewButton.Clicked += (s, e) => Navigation.PushAsync(new Issue27666());

		rootLayout.Add(collectionViewButton);
		ContentPage.Content = rootLayout;
		return ContentPage;
	}
}

public partial class Issue27666 : ContentPage
{
	ObservableCollection<string> items;

	public Issue27666()
	{
		InitializeComponent();
		items = new ObservableCollection<string>(Enumerable.Range(1, 30).Select(i => $"Item {i}"));
		collectionView.ItemsSource = items;
	}

	private void OnItemSpacingButtonClicked(object sender, EventArgs e)
	{
		if (collectionView.ItemsLayout is LinearItemsLayout layout)
		{
			layout.ItemSpacing = layout.ItemSpacing == 0 ? 50 : 20;
		}
	}
}