using System;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23377, "Horizontal Item spacing in collectionView", PlatformAffected.UWP)]
public class Issue23377 : TestContentPage
{
	public List<string> Items { get; set; }
	private Button button;
	private CollectionView collectionView;

	protected override void Init()
	{
		Items = new List<string>
		{
			"Item 1", "Item 2", "Item 3", "Item 4",
			"Item 5", "Item 6", "Item 7", "Item 8", "Item 9"
		};
		BindingContext = this;

		var VerticalStackLayout = new VerticalStackLayout();

		button = new Button
		{
			Text = "Change Item Space",
			AutomationId = "ChangeItemSpace"
		};

		button.Clicked += Button_Clicked;

		collectionView = new CollectionView
		{
			ItemsSource = Items,
			Margin = new Thickness(100),
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			HeightRequest = 200,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never
		};

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			return label;
		});

		VerticalStackLayout.Children.Add(collectionView);
		VerticalStackLayout.Children.Add(button);

		Content = VerticalStackLayout;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		if (collectionView.ItemsLayout is LinearItemsLayout linearItemsLayout)
		{
			linearItemsLayout.ItemSpacing = 80;
		}
	}
}