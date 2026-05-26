namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29491, "[CV2][CollectionView] Changing CollectionView's ItemsSource in runtime removes elements' parent seemingly random", PlatformAffected.iOS)]
public class Issue29491 : ContentPage
{
	List<string> m_items =
	[
		"Item 1", "Item 2", "Item 3", "Item 4", "Item 5",
		"Item 6", "Item 7", "Item 8", "Item 9", "Item 10"
	];

	public List<string> Items
	{
		get => m_items;
		set
		{
			if (Equals(value, m_items))
				return;
			m_items = value;
			OnPropertyChanged(nameof(Items));
		}
	}

	public Issue29491()
	{
		BindingContext = this;

		var reverseButton = new Button
		{
			Text = "Reverse",
			AutomationId = "Button"
		};
		reverseButton.Clicked += Button_OnClicked;

		var collectionView = new CollectionView
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var stack = new VerticalStackLayout();

				var label = new Label
				{
					FontSize = 30
				};
				label.SetBinding(Label.TextProperty, ".");

				var parentLabel = new Label { AutomationId = "Label" };
				parentLabel.SetBinding(Label.TextProperty, new Binding
				{
					Source = stack,
					Path = "Parent",
					TargetNullValue = "`null`"
				});

				var grid = new Grid
				{
					ColumnDefinitions = new ColumnDefinitionCollection
					{
						new ColumnDefinition { Width = GridLength.Auto },
						new ColumnDefinition { Width = GridLength.Star }
					}
				};
				grid.Add(new Label { Text = "Parent: " });
				grid.Add(parentLabel, 1, 0);

				stack.Add(label);
				stack.Add(grid);

				return stack;
			})
		};
		collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Items));

		var mainLayout = new StackLayout
		{
			Children =
			{
				reverseButton,
				collectionView
			}
		};

		Content = mainLayout;
	}

	void Button_OnClicked(object sender, EventArgs e)
	{
		var reversed = new List<string>(Items);
		reversed.Reverse();
		Items = reversed;
	}
}

