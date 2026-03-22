using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28509, "Dynamically Setting Header and Footer in CV2 Does Not Update Properly", PlatformAffected.iOS)]
public class Issue28509NavigationPage : TestNavigationPage
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

		Button collectionViewGroupButton = new Button
		{
			Text = "CollectionView Group Header/Footer Toggle",
			AutomationId = "GroupHeaderFooterButton"
		};
		collectionViewGroupButton.Clicked += (s, e) => Navigation.PushAsync(new Issue28509GroupHeaderFooter());

		Button collectionViewTemplatedButton = new Button
		{
			Text = "CollectionView Templated Header/Footer Toggle",
			AutomationId = "ItemsViewTemplatedHeaderFooterButton"
		};
		collectionViewTemplatedButton.Clicked += (s, e) => Navigation.PushAsync(new Issue28509TemplatedHeaderFooter());

		Button collectionViewItemsViewButton = new Button
		{
			Text = "CollectionView ItemsView Header/Footer Toggle",
			AutomationId = "ItemsViewHeaderFooterButton"
		};
		collectionViewItemsViewButton.Clicked += (s, e) => Navigation.PushAsync(new Issue28509ItemsViewHeaderFooter());

		rootLayout.Add(collectionViewGroupButton);
		rootLayout.Add(collectionViewTemplatedButton);
		rootLayout.Add(collectionViewItemsViewButton);

		ContentPage.Content = rootLayout;
		return ContentPage;
	}
}

public class Issue28509GroupHeaderFooter : TestContentPage
{
	ObservableCollection<Issue28509Group> _source;
	CollectionView2 _collectionView;

	protected override void Init()
	{
		_source = new ObservableCollection<Issue28509Group>();
		_collectionView = new CollectionView2 { AutomationId = "CollectionView", IsGrouped = true };

		Grid layoutGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		Button toggleHeaderButton = new Button
		{
			AutomationId = "ToggleHeaderButton",
			Text = "Toggle Header"
		};
		toggleHeaderButton.Clicked += ToggleHeader;

		Button toggleFooterButton = new Button
		{
			AutomationId = "ToggleFooterButton",
			Text = "Toggle Footer"
		};
		toggleFooterButton.Clicked += ToggleFooter;

		HorizontalStackLayout buttonsLayout = new HorizontalStackLayout
		{
			Padding = 20,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children = { toggleHeaderButton, toggleFooterButton }
		};

		layoutGrid.Add(buttonsLayout, 0, 0);

		_collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				BackgroundColor = Colors.HotPink,
				FontSize = 22
			};
			label.SetBinding(Label.TextProperty, "Header");
			return label;
		});

		_collectionView.GroupFooterTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				BackgroundColor = Colors.HotPink,
				FontSize = 22
			};
			label.SetBinding(Label.TextProperty, "Footer");
			return label;
		});

		_collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, "Name");
			return label;
		});

		_collectionView.ItemsSource = _source;

		layoutGrid.Add(_collectionView, 0, 1);
		Content = layoutGrid;

		Appearing += (sender, args) => AddItems();
	}

	void ToggleHeader(object sender, EventArgs e)
	{
		_collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				BackgroundColor = Colors.HotPink,
				FontSize = 22,
				Text = "GroupHeaderTemplate Changed"
			};
		});
	}

	void ToggleFooter(object sender, EventArgs e)
	{
		_collectionView.GroupFooterTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				BackgroundColor = Colors.HotPink,
				FontSize = 22,
				Text = "GroupFooterTemplate Changed"
			};
		});
	}

	void AddItems()
	{
		var groupIndex = _source.Count + 1;
		if (groupIndex > 2)
			return;

		var group = new Issue28509Group
		{
			Header = $"{groupIndex} Header (added)",
			Footer = $"{groupIndex} Footer (added)"
		};

		for (int itemIndex = 0; itemIndex < 3; itemIndex++)
		{
			var item = new Issue28509ViewItem { Name = $"{groupIndex}.{itemIndex} Item (added)" };
			group.Add(item);
		}

		_source.Add(group);
	}

	class Issue28509ViewItem
	{
		public string Name { get; set; }
	}

	class Issue28509Group : ObservableCollection<Issue28509ViewItem>
	{
		public string Header { get; set; }
		public string Footer { get; set; }
	}
}

public class Issue28509TemplatedHeaderFooter : TestContentPage
{
	CollectionView collectionView;
	ObservableCollection<string> Items { get; set; }

	protected override void Init()
	{
		Grid layoutGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		Button toggleHeaderButton = new Button
		{
			AutomationId = "ToggleHeaderButton",
			Text = "Toggle Header"
		};
		toggleHeaderButton.Clicked += ToggleHeader;

		Button toggleFooterButton = new Button
		{
			AutomationId = "ToggleFooterButton",
			Text = "Toggle Footer"
		};
		toggleFooterButton.Clicked += ToggleFooter;

		HorizontalStackLayout buttonsLayout = new HorizontalStackLayout
		{
			Padding = 20,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children = { toggleHeaderButton, toggleFooterButton }
		};

		layoutGrid.Add(buttonsLayout, 0, 0);
		Items = new ObservableCollection<string>
		{
			"Test 1", "Test 2", "Test 3", "Test 4", "Test 5",
			"Test 6", "Test 7", "Test 8", "Test 9", "Test 10",
		};

		collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			ItemsSource = Items,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					TextColor = Colors.Black,
				};

				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
			HeaderTemplate = new DataTemplate(() =>
			{
				return new Label
				{
					Text = "Header(added)",
					HeightRequest = 100,
					BackgroundColor = Colors.Red
				};
			}),
			FooterTemplate = new DataTemplate(() =>
			{
				return new Label
				{
					Text = "Footer(added)",
					HeightRequest = 100,
					BackgroundColor = Colors.Green
				};
			})
		};

		layoutGrid.Add(collectionView, 0, 1);
		Content = layoutGrid;
	}

	private void ToggleFooter(object sender, EventArgs e)
	{
		collectionView.FooterTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "FooterTemplate Changed",
				HeightRequest = 100,
				BackgroundColor = Colors.Green
			};
		});
	}

	private void ToggleHeader(object sender, EventArgs e)
	{
		collectionView.HeaderTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "HeaderTemplate Changed",
				HeightRequest = 100,
				BackgroundColor = Colors.Red
			};
		});
	}
}

public class Issue28509ItemsViewHeaderFooter : TestContentPage
{
	CollectionView collectionView;
	ObservableCollection<string> Items { get; set; }

	protected override void Init()
	{
		Grid layoutGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		Button toggleHeaderButton = new Button
		{
			AutomationId = "ToggleHeaderButton",
			Text = "Toggle Header"
		};
		toggleHeaderButton.Clicked += ToggleHeader;

		Button toggleFooterButton = new Button
		{
			AutomationId = "ToggleFooterButton",
			Text = "Toggle Footer"
		};
		toggleFooterButton.Clicked += ToggleFooter;

		HorizontalStackLayout buttonsLayout = new HorizontalStackLayout
		{
			Padding = 20,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children = { toggleHeaderButton, toggleFooterButton }
		};

		Items = new ObservableCollection<string>
		{
			"Test 1", "Test 2", "Test 3", "Test 4", "Test 5",
			"Test 6", "Test 7", "Test 8", "Test 9", "Test 10",
		};

		collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			ItemsSource = Items,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					TextColor = Colors.Black,
				};

				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),

			Header =
				new Label
				{
					Text = "Header (added)",
					HeightRequest = 100,
					BackgroundColor = Colors.Red
				},

			Footer = new Label
			{
				Text = "Footer (added)",
				HeightRequest = 100,
				BackgroundColor = Colors.Red
			},

		};

		layoutGrid.Add(buttonsLayout, 0, 0);
		layoutGrid.Add(collectionView, 0, 1);
		Content = layoutGrid;
	}

	private void ToggleFooter(object sender, EventArgs e)
	{
		collectionView.Footer = new Label
		{
			Text = "Footer Changed",
			HeightRequest = 100,
			BackgroundColor = Colors.Red
		};
	}

	private void ToggleHeader(object sender, EventArgs e)
	{
		collectionView.Header = new Label
		{
			Text = "Header Changed",
			HeightRequest = 100,
			BackgroundColor = Colors.Red
		};
	}
}