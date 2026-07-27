using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27959, "Dynamically toggling the Header/Footer between a null and a non-null value in CollectionView is not working", PlatformAffected.UWP | PlatformAffected.Android)]
public class Issue27959_NavigationPage : TestNavigationPage
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
			Text = "EmptyView Header/Footer Toggle",
			AutomationId = "EmptyViewButton"
		};
		collectionViewButton.Clicked += (s, e) => Navigation.PushAsync(new Issue27959_EmptyViewHeaderFooter());

		Button collectionViewTemplatedButton = new Button
		{
			Text = "EmptyViewTemplated  Header/Footer Toggle",
			AutomationId = "EmptyViewViewTemplatedButton"
		};
		collectionViewTemplatedButton.Clicked += (s, e) => Navigation.PushAsync(new Issue27959_EmptyViewTemplatedHeaderFooterTemplated());

		Button collectionItemsViewButton = new Button
		{
			Text = "ItemsView Header/Footer Toggle",
			AutomationId = "ItemsViewButton"
		};
		collectionItemsViewButton.Clicked += (s, e) => Navigation.PushAsync(new Issue27959_ItemsHeaderFooterView());

		Button collectionTemplatedItemsViewButton = new Button
		{
			Text = "ItemsViewTemplated Header/Footer Toggle",
			AutomationId = "ItemsViewTemplatedButton"
		};
		collectionTemplatedItemsViewButton.Clicked += (s, e) => Navigation.PushAsync(new Issue27959_ItemsHeaderFooterTemplatedView());

		rootLayout.Add(collectionViewButton);
		rootLayout.Add(collectionViewTemplatedButton);
		rootLayout.Add(collectionItemsViewButton);
		rootLayout.Add(collectionTemplatedItemsViewButton);

		ContentPage.Content = rootLayout;
		return ContentPage;
	}
}

public class Issue27959_EmptyViewHeaderFooter : TestContentPage
{
	CollectionView _collectionView;
	object _storedHeader;
	object _storedFooter;

	protected override void Init()
	{
		Title = "CollectionView Header/Footer Toggle";

		Grid layoutGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		Button _toggleHeaderButton = new Button
		{
			AutomationId = "ToggleHeaderButton",
			Text = "Toggle Header"
		};
		_toggleHeaderButton.Clicked += ToggleHeader;

		Button _toggleFooterButton = new Button
		{
			AutomationId = "ToggleFooterButton",
			Text = "Toggle Footer"
		};
		_toggleFooterButton.Clicked += ToggleFooter;

		HorizontalStackLayout buttonsLayout = new HorizontalStackLayout
		{
			Padding = 20,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children =
			{
				_toggleHeaderButton,
				_toggleFooterButton
			}
		};

		layoutGrid.Add(buttonsLayout, 0, 0);

		_collectionView = new CollectionView
		{
			AutomationId = "EmptyViewHeaderFooter",
			Header = new Label
			{
				Padding = 10,
				FontAttributes = FontAttributes.Bold,
				FontSize = 24,
				Text = "This Is A Header",
				AutomationId = "Header"
			},
			EmptyView = new Label
			{
				Padding = new Thickness(20, 5, 5, 5),
				Text = "Empty"
			},
			Footer = new Label
			{
				Padding = 10,
				FontAttributes = FontAttributes.Bold,
				FontSize = 24,
				Text = "This Is A Footer",
				AutomationId = "Footer"
			},
			ItemsSource = new ObservableCollection<string>()
		};

		layoutGrid.Add(_collectionView, 0, 1);
		Content = layoutGrid;
	}

	void ToggleHeader(object sender, System.EventArgs e)
	{
		if (_storedHeader == null)
		{
			_storedHeader = _collectionView.Header;
		}
		_collectionView.Header = _collectionView.Header == null ? _storedHeader : null;
	}

	void ToggleFooter(object sender, System.EventArgs e)
	{
		if (_storedFooter == null)
		{
			_storedFooter = _collectionView.Footer;
		}
		_collectionView.Footer = _collectionView.Footer == null ? _storedFooter : null;
	}
}

public class Issue27959_EmptyViewTemplatedHeaderFooterTemplated : TestContentPage
{
	CollectionView _templatedCollectionView;
	DataTemplate _storedHeaderTemplate;
	DataTemplate _storedFooterTemplate;

	protected override void Init()
	{
		Title = "CollectionViewTemplated Header/Footer Toggle";

		var layoutGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		Button _toggleHeaderButton = new Button
		{
			AutomationId = "ToggleHeaderTemplateButton",
			Text = "Toggle HeaderTemplate"
		};
		_toggleHeaderButton.Clicked += ToggleHeader;

		Button _toggleFooterButton = new Button
		{
			AutomationId = "ToggleFooterTemplateButton",
			Text = "Toggle FooterTemplate"
		};
		_toggleFooterButton.Clicked += ToggleFooter;

		HorizontalStackLayout buttonsLayout = new HorizontalStackLayout
		{
			Padding = 20,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children =
			{
				_toggleHeaderButton,
				_toggleFooterButton
			}
		};

		layoutGrid.Add(buttonsLayout, 0, 0);

		_templatedCollectionView = new CollectionView
		{
			AutomationId = "EmptyViewHeaderFooterTemplate",
			HeaderTemplate = new DataTemplate(() =>
				new Label
				{
					Padding = 10,
					FontAttributes = FontAttributes.Bold,
					FontSize = 24,
					Text = "This Is A HeaderTemplate",
					AutomationId = "HeaderTemplate"
				}),
			EmptyViewTemplate = new DataTemplate(() =>
				new Label
				{
					Padding = new Thickness(20, 5, 5, 5),
					Text = "Empty Template"
				}),
			FooterTemplate = new DataTemplate(() =>
				new Label
				{
					Padding = 10,
					FontAttributes = FontAttributes.Bold,
					FontSize = 24,
					Text = "This Is A FooterTemplate",
					AutomationId = "FooterTemplate"
				})
		};

		layoutGrid.Add(_templatedCollectionView, 0, 1);
		Content = layoutGrid;
	}

	void ToggleHeader(object sender, System.EventArgs e)
	{
		if (_storedHeaderTemplate == null)
		{
			_storedHeaderTemplate = _templatedCollectionView.HeaderTemplate;
		}
		_templatedCollectionView.HeaderTemplate = _templatedCollectionView.HeaderTemplate == null
			? _storedHeaderTemplate
			: null;
	}

	void ToggleFooter(object sender, System.EventArgs e)
	{
		if (_storedFooterTemplate == null)
		{
			_storedFooterTemplate = _templatedCollectionView.FooterTemplate;
		}
		_templatedCollectionView.FooterTemplate = _templatedCollectionView.FooterTemplate == null
			? _storedFooterTemplate
			: null;
	}
}

public class Issue27959_ItemsHeaderFooterView : TestContentPage
{
	CollectionView _collectionView;
	object _storedHeader;
	object _storedFooter;

	protected override void Init()
	{
		Title = "CollectionView Header/Footer Toggle";

		Grid layoutGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		Button _toggleHeaderButton = new Button
		{
			AutomationId = "ItemsViewToggleHeaderButton",
			Text = "Toggle Header"
		};
		_toggleHeaderButton.Clicked += ToggleHeader;

		Button _toggleFooterButton = new Button
		{
			AutomationId = "ItemsViewToggleFooterButton",
			Text = "Toggle Footer"
		};
		_toggleFooterButton.Clicked += ToggleFooter;

		HorizontalStackLayout buttonsLayout = new HorizontalStackLayout
		{
			Padding = 20,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children =
			{
				_toggleHeaderButton,
				_toggleFooterButton
			}
		};

		layoutGrid.Add(buttonsLayout, 0, 0);

		_collectionView = new CollectionView
		{
			AutomationId = "ItemsViewHeaderFooter",
			Header = new Label
			{
				Padding = 10,
				FontAttributes = FontAttributes.Bold,
				FontSize = 24,
				Text = "This Is A Header",
				AutomationId = "ItemsViewHeader"
			},
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
			Footer = new Label
			{
				Padding = 10,
				FontAttributes = FontAttributes.Bold,
				FontSize = 24,
				Text = "This Is A Footer",
				AutomationId = "ItemsViewFooter"
			},
		};

		layoutGrid.Add(_collectionView, 0, 1);
		Content = layoutGrid;
	}

	void ToggleHeader(object sender, System.EventArgs e)
	{
		if (_storedHeader == null)
		{
			_storedHeader = _collectionView.Header;
		}
		_collectionView.Header = _collectionView.Header == null ? _storedHeader : null;
	}

	void ToggleFooter(object sender, System.EventArgs e)
	{
		if (_storedFooter == null)
		{
			_storedFooter = _collectionView.Footer;
		}
		_collectionView.Footer = _collectionView.Footer == null ? _storedFooter : null;
	}
}

public class Issue27959_ItemsHeaderFooterTemplatedView : TestContentPage
{
	CollectionView _collectionView;
	DataTemplate _savedHeaderTemplate;
	DataTemplate _savedFooterTemplate;

	protected override void Init()
	{
		Title = "Items Header/Footer Templated View";

		Grid layoutGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		Button headerButton = new Button
		{
			AutomationId = "ToggleHeaderTemplateButton",
			Text = "Toggle HeaderTemplate"
		};
		headerButton.Clicked += ToggleHeader;

		Button footerButton = new Button
		{
			AutomationId = "ToggleFooterTemplateButton",
			Text = "Toggle FooterTemplate"
		};
		footerButton.Clicked += ToggleFooter;

		HorizontalStackLayout buttonsLayout = new HorizontalStackLayout
		{
			Padding = 20,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children = { headerButton, footerButton }
		};

		layoutGrid.Add(buttonsLayout, 0, 0);

		_collectionView = new CollectionView
		{
			AutomationId = "ItemsViewHeaderFooterTemplate",
			HeaderTemplate = new DataTemplate(() => new Label
			{
				Padding = 10,
				FontAttributes = FontAttributes.Bold,
				FontSize = 24,
				Text = "This Is A HeaderTemplate",
				AutomationId = "ItemsHeaderTemplate"
			}),
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
			FooterTemplate = new DataTemplate(() => new Label
			{
				Padding = 10,
				FontAttributes = FontAttributes.Bold,
				FontSize = 24,
				Text = "This Is A FooterTemplate",
				AutomationId = "ItemsFooterTemplate"
			})
		};

		layoutGrid.Add(_collectionView, 0, 1);
		Content = layoutGrid;
	}

	void ToggleHeader(object sender, EventArgs e)
	{
		if (_savedHeaderTemplate == null)
		{
			_savedHeaderTemplate = _collectionView.HeaderTemplate;
		}
		_collectionView.HeaderTemplate = _collectionView.HeaderTemplate == null
			? _savedHeaderTemplate
			: null;
	}

	void ToggleFooter(object sender, EventArgs e)
	{
		if (_savedFooterTemplate == null)
		{
			_savedFooterTemplate = _collectionView.FooterTemplate;
		}
		_collectionView.FooterTemplate = _collectionView.FooterTemplate == null
			? _savedFooterTemplate
			: null;
	}
}
