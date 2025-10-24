using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31825, "[iOS, macOS]CollectionView KeepLastItemInView not updating correctly when items are added", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31825 : ContentPage
{
	public Issue31825()
	{
		Issue31825ViewModel viewModel = new Issue31825ViewModel();
		CollectionView2 collectionView = new CollectionView2
		{
			VerticalOptions = LayoutOptions.FillAndExpand,
			HorizontalOptions = LayoutOptions.FillAndExpand,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
				};

				var nameLabel = new Label
				{
					FontAttributes = FontAttributes.Bold,
					FontSize = 18,
				};

				nameLabel.SetBinding(Label.TextProperty, "Name");
				nameLabel.SetBinding(Label.AutomationIdProperty, "Name");

				grid.Add(nameLabel, 0, 0);
				return grid;
			})
		};
		collectionView.ItemsSource = viewModel.Items;
		this.BindingContext = viewModel;
		collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
		{
			ItemSpacing = 10,
		};

		Button itemsUpdatingScrollModeButton = new Button
		{
			Text = "Update ItemScroll mode",
			AutomationId = "ItemsUpdatingScrollModeButton",
			Command = new Command(() =>
			{
				collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
			})
		};

		Button addButton = new Button
		{
			Text = "Add Item",
			AutomationId = "AddItemButton",
			Command = new Command(() =>
			{
				for (int i = 0; i < 10; i++)
				{
					viewModel.Items.Add(new Issue31825Model
					{
						Name = "New Item" + viewModel.Items.Count,
					});
				}

				viewModel.Items.Add(new Issue31825Model
				{
					Name = "Gelada",
				});
			})
		};

		Grid grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
		grid.Add(itemsUpdatingScrollModeButton, 0, 0);
		grid.Add(addButton, 0, 1);
		grid.Add(collectionView, 0, 2);
		Content = grid;

	}
}

public class Issue31825Model
{
	public string Name { get; set; }
}

public class Issue31825ViewModel
{
	public ObservableCollection<Issue31825Model> Items { get; private set; } = new ObservableCollection<Issue31825Model>();

	public Issue31825ViewModel()
	{
		_ = CreateMonkeyCollection();
	}

	async Task CreateMonkeyCollection()
	{
		for (int i = 0; i < 18; i++)
		{
			Items.Add(new Issue31825Model
			{
				Name = "New Item" + i,
			});
		}
	}
}
