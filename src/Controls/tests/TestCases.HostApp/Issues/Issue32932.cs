namespace Maui.Controls.Sample.Issues;

using System.Collections.ObjectModel;

[Issue(IssueTracker.Github, 32932, "[Android] EmptyView doesn’t display when CollectionView is placed inside a VerticalStackLayout", PlatformAffected.Android)]

public class Issue32932 : TestContentPage
{
	protected override void Init()
	{
		var collectionView = new CollectionView2();

		collectionView.AutomationId = "EmptyCollectionView";
		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding(".")); // bind to the string item
			return label;
		});

		// EmptyView: ContentView -> VerticalStackLayout -> Label "No values found..."
		collectionView.EmptyView = new ContentView
		{
			Content = new VerticalStackLayout
			{
				Children =
						{
							new Label { Text = "No values found..." , AutomationId= "EmptyViewLabel"}
						}
			}
		};
		collectionView.ItemsSource = new ObservableCollection<string>();

		// inner stack that contains the title and the CollectionView
		var innerStack = new VerticalStackLayout
		{
			Children = { collectionView }
		};

		Content = innerStack;
	}
}
