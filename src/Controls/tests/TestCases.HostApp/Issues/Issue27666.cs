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

public partial class Issue27666 : TestContentPage
{
	ObservableCollection<string> items;
	CollectionView collectionView;
	protected override void Init()
	{
		items = new ObservableCollection<string>(Enumerable.Range(1, 30).Select(i => $"Item {i}"));
		Button itemSpacingButton = CreateButton("Update ItemSpacing", "UpdateItemSpacingButton", OnItemSpacingButtonClicked);

		collectionView = new CollectionView
		{
			AutomationId = "ItemsLayoutCollectionView",
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return new Border
				{
					Content = label,
					Padding = 10,
					Margin = new Thickness(5),
					BackgroundColor = Colors.LightGray,
				};
			})
		};

		Grid grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},
			RowSpacing = 5
		};
		grid.Add(itemSpacingButton, 0, 0);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}

	Button CreateButton(string text, string automationId, EventHandler onClick)
	{
		return new Button
		{
			Text = text,
			AutomationId = automationId,
			Command = new Command(_ => onClick(this, EventArgs.Empty))
		};
	}

	void OnItemSpacingButtonClicked(object sender, EventArgs e)
	{
		if (collectionView.ItemsLayout is LinearItemsLayout layout)
		{
			layout.ItemSpacing = layout.ItemSpacing == 0 ? 50 : 20;
		}
	}
}