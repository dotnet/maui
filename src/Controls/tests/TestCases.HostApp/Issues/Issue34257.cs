using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34257, "CollectionView vertical grid item spacing updates all rows and columns", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34257 : ContentPage
{
	readonly GridItemsLayout _itemsLayout;
	readonly Label _statusLabel;

	public Issue34257()
	{
		_itemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
		{
			HorizontalItemSpacing = 0,
			VerticalItemSpacing = 0
		};

		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Spacing=0,0"
		};

		var applySpacingButton = new Button
		{
			AutomationId = "ApplyHorizontalSpacingButton",
			Text = "Apply horizontal spacing"
		};
		applySpacingButton.Clicked += OnApplyHorizontalSpacingClicked;

		var applyVerticalSpacingButton = new Button
		{
			AutomationId = "ApplyVerticalSpacingButton",
			Text = "Apply vertical spacing"
		};
		applyVerticalSpacingButton.Clicked += OnApplyVerticalSpacingClicked;

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			HeightRequest = 260,
			HorizontalOptions = LayoutOptions.Center,
			ItemsLayout = _itemsLayout,
			ItemsSource = CreateItems(),
			ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
			SelectionMode = SelectionMode.None,
			WidthRequest = 340
		};
		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var titleLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				LineBreakMode = LineBreakMode.TailTruncation
			};
			titleLabel.SetBinding(Label.TextProperty, nameof(SpacingIssueItem.Name));

			var locationLabel = new Label
			{
				FontAttributes = FontAttributes.Italic,
				LineBreakMode = LineBreakMode.TailTruncation,
				VerticalOptions = LayoutOptions.End
			};
			locationLabel.SetBinding(Label.TextProperty, nameof(SpacingIssueItem.Location));

			var textLayout = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto }
				}
			};
			textLayout.Add(titleLabel);
			textLayout.Add(locationLabel, 0, 1);

			var root = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = 70 },
					new ColumnDefinition { Width = GridLength.Star }
				},
				Padding = 10
			};
			root.SetBinding(AutomationIdProperty, nameof(SpacingIssueItem.AutomationId));
			root.SetBinding(BackgroundColorProperty, nameof(SpacingIssueItem.BackgroundColor));

			var imagePlaceholder = new Border
			{
				Background = Colors.DarkSlateBlue,
				HeightRequest = 60,
				StrokeThickness = 0,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 60
			};

			root.Add(imagePlaceholder);
			root.Add(textLayout, 1, 0);

			return root;
		});

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 12,
				Children =
				{
					new Label { Text = "Issue 34257 reproduces a spacing update bug in a two-column vertical CollectionView grid." },
					new Label { Text = "Apply horizontal or vertical spacing and verify both columns and rows resize consistently." },
					new HorizontalStackLayout
					{
						Spacing = 12,
						Children =
						{
							applySpacingButton,
							applyVerticalSpacingButton
						}
					},
					_statusLabel,
					collectionView
				}
			}
		};
	}

	void OnApplyHorizontalSpacingClicked(object sender, EventArgs e)
	{
		_itemsLayout.VerticalItemSpacing = 0;
		_itemsLayout.HorizontalItemSpacing = 80;
		_statusLabel.Text = "Spacing=0,80";
	}

	void OnApplyVerticalSpacingClicked(object sender, EventArgs e)
	{
		_itemsLayout.VerticalItemSpacing = 40;
		_itemsLayout.HorizontalItemSpacing = 0;
		_statusLabel.Text = "Spacing=40,0";
	}

	static ObservableCollection<SpacingIssueItem> CreateItems()
	{
		return
		[
			new SpacingIssueItem("FirstColumnTopItem", "Capuchin", "Central America", Colors.LightSkyBlue),
			new SpacingIssueItem("SecondColumnTopItem", "Spider", "South America", Colors.LightSalmon),
			new SpacingIssueItem("FirstColumnBottomItem", "Howler", "South America", Colors.PaleGreen),
			new SpacingIssueItem("SecondColumnBottomItem", "Baboon", "Africa", Colors.Khaki)
		];
	}

	class SpacingIssueItem(string automationId, string name, string location, Color backgroundColor)
	{
		public string AutomationId { get; } = automationId;

		public Color BackgroundColor { get; } = backgroundColor;

		public string Location { get; } = location;

		public string Name { get; } = name;
	}
}
