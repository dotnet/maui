namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 13203, "[Bug] [iOS] CollectionView does not bind to items if IsVisible=False", PlatformAffected.iOS)]
public class Issue13203 : TestContentPage
{
	const string Success = "Success";

	protected override void Init()
	{
		var cv = new CollectionView
		{
			IsVisible = false,

			BackgroundColor = Colors.PaleGreen,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { FontSize = 40, BackgroundColor = Colors.SeaGreen };
				label.SetBinding(Label.TextProperty, new Binding(nameof(Item.Text)));
				label.SetBinding(Label.AutomationIdProperty, new Binding(nameof(Item.Text)));
				return label;
			})
		};

		var source = new List<Item> { new Item { Text = Success } };
		cv.ItemsSource = source;
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		Content = cv;

		Appearing += (sender, args) => { cv.IsVisible = true; };
	}

	class Item
	{
		public string Text { get; set; }
	}
}
