namespace Maui.Controls.Sample.Issues;

class Issue33407CategoryItem
{
	public string Id { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public string AutomationId { get; set; } = string.Empty;
}

class Issue33407TestItem
{
	public string Id { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public string AutomationId { get; set; } = string.Empty;
}

[Issue(IssueTracker.Github, 33407, "Focusing and entering texts on entry control causes a gap at the top after rotating simulator.", PlatformAffected.iOS)]
public class Issue33407 : Shell
{
	public Issue33407()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		// Match ManualTests/Sandbox Shell styling so iOS safe area is handled correctly
		Shell.SetBackgroundColor(this, Color.FromArgb("#512BD4"));
		Shell.SetForegroundColor(this, Colors.White);
		Shell.SetTitleColor(this, Colors.White);
		Shell.SetNavBarHasShadow(this, false);

		Items.Add(new ShellContent
		{
			Title = "Home",
			Content = new Issue33407CategoriesPage()
		});
	}
}

class Issue33407CategoriesPage : ContentPage
{
	public Issue33407CategoriesPage()
	{
		Title = "Categories";

		var collection = new CollectionView { SelectionMode = SelectionMode.Single };
		collection.ItemsSource = new[]
		{
			new Issue33407CategoryItem { Id = "E", Title = "Entry", AutomationId = "CategoryE" }
		};
		collection.ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid { Padding = new Thickness(10) };
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

			var idLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 32, VerticalOptions = LayoutOptions.Center };
			idLabel.SetBinding(Label.TextProperty, nameof(Issue33407CategoryItem.Id));
			idLabel.SetBinding(Label.AutomationIdProperty, nameof(Issue33407CategoryItem.AutomationId));

			var titleLabel = new Label { Margin = new Thickness(20, 0, 0, 0), FontSize = 28, VerticalOptions = LayoutOptions.Center };
			titleLabel.SetBinding(Label.TextProperty, nameof(Issue33407CategoryItem.Title));

			grid.Add(idLabel, 0, 0);
			grid.Add(titleLabel, 1, 0);
			return grid;
		});
		collection.SelectionChanged += (s, e) =>
		{
			if (e.CurrentSelection.FirstOrDefault() is Issue33407CategoryItem item && item.Id == "E")
				Navigation.PushAsync(new Issue33407EntryListPage());
			((CollectionView)s).SelectedItem = null;
		};

		Content = collection;
	}
}

class Issue33407EntryListPage : ContentPage
{
	public Issue33407EntryListPage()
	{
		Title = "Entry";

		var collection = new CollectionView { SelectionMode = SelectionMode.Single };
		collection.ItemsSource = new[]
		{
			new Issue33407TestItem { Id = "E1", Title = "No gap at top after rotation", AutomationId = "TestE1" }
		};
		collection.ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid { Padding = new Thickness(10) };
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

			var idLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 32, VerticalOptions = LayoutOptions.Center };
			idLabel.SetBinding(Label.TextProperty, nameof(Issue33407TestItem.Id));
			idLabel.SetBinding(Label.AutomationIdProperty, nameof(Issue33407TestItem.AutomationId));

			var titleLabel = new Label { Margin = new Thickness(20, 0, 0, 0), FontSize = 12, VerticalOptions = LayoutOptions.Center };
			titleLabel.SetBinding(Label.TextProperty, nameof(Issue33407TestItem.Title));

			grid.Add(idLabel, 0, 0);
			grid.Add(titleLabel, 1, 0);
			return grid;
		});
		collection.SelectionChanged += (s, e) =>
		{
			if (e.CurrentSelection.FirstOrDefault() is Issue33407TestItem item && item.Id == "E1")
				Navigation.PushAsync(new Issue33407E1Page());
			((CollectionView)s).SelectedItem = null;
		};

		Content = collection;
	}
}

class Issue33407E1Page : ContentPage
{
	public Issue33407E1Page()
	{
		Title = "E1";

		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "1. Rotate the device between portrait and landscape with the keyboard hidden. The test passes if no extra gap appears at the top of the page above the entries."
				},
				new UITestEntry
				{
					IsPassword = false,
					IsCursorVisible = false,
					Placeholder = "Top gap check (normal entry)",
					AutomationId = "Entry1"
				},
				new Label
				{
					Text = "2. Tap into each Entry, then rotate the device again. The test passes if no additional top gap appears and both entries remain aligned directly under this text."
				},
				new UITestEntry
				{
					IsPassword = true,
					IsCursorVisible = false,
					Placeholder = "Top gap check (password entry)",
					AutomationId = "Entry2"
				}
			}
		};
	}
}
