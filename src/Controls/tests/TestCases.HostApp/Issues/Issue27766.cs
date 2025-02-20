using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27766, "The bottom content inset value does not need to be updated for CollectionView items when the editor is an inner element", PlatformAffected.iOS)]
public class Issue27766 : ContentPage
{
	public ObservableCollection<string> Items { get; set; }
	public Issue27766()
	{
		Items = new ObservableCollection<string>
		{
			"Test 1", "Test 2", "Test 3", "Test 4", "Test 5",
			"Test 6", "Test 7", "Test 8", "Test 9", "Test 10",
		};

		CollectionView collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			ItemsSource = Items,
			ItemTemplate = new DataTemplate(() =>
			{
				var editor = new UITestEditor
				{
					TextColor = Colors.Black,
					IsCursorVisible = false
				};
				editor.SetBinding(Editor.TextProperty, ".");
				editor.SetBinding(AutomationIdProperty, ".");
				return editor;
			}),
			FooterTemplate = new DataTemplate(() =>
			{
				return new BoxView
				{
					HeightRequest = 100,
					BackgroundColor = Colors.Green
				};
			})
		};

		Content = new Grid
		{
			Children = { collectionView }
		};
	}
}