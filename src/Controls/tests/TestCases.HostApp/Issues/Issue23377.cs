namespace Maui.Controls.Sample.Issues;
[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 23377, "Item Spacing misbehaviour for horizontal list", PlatformAffected.UWP)]
public partial class Issue23377 : ContentPage
{
	public List<string> Items { get; set; }
	private Button button;
	private Entry _entry;
	private CollectionView collectionView;

	public Issue23377()
	{
		Items = new List<string>
		{
			"Item 1", "Item 2", "Item 3", "Item 4",
			"Item 5", "Item 6", "Item 7", "Item 8", "Item 9"
		};
		BindingContext = this;

		var grid = new Grid();
		var stackLayout = new StackLayout();

		_entry = new Entry
		{
			AutomationId = "EntryControl",
			Keyboard = Keyboard.Numeric
		};

		button = new Button
		{
			Text = "Change Item Space",
			AutomationId = "ChangeItemSpace"
		};

		button.Clicked += Button_Clicked;

		stackLayout.Children.Add(_entry);
		stackLayout.Children.Add(button);

		collectionView = new CollectionView
		{
			ItemsSource = Items,
			Margin = new Thickness(100),
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
		};

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			return label;
		});

		grid.Children.Add(stackLayout);
		grid.Children.Add(collectionView);


		Content = grid;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		if (double.TryParse(_entry.Text, out double spacingValue))
		{
			if (collectionView.ItemsLayout is LinearItemsLayout linearItemsLayout)
			{
				linearItemsLayout.ItemSpacing = spacingValue;
			}
		}
	}
}

