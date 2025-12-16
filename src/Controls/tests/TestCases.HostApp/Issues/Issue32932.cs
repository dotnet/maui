namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32932, "[Android] EmptyView doesn’t display when CollectionView is placed inside a VerticalStackLayout", PlatformAffected.Android)]

public class Issue32932 : TestShell
{
	protected override void Init()
	{
		var shellContent = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = new Issue32932CollectionViewPage() { Title = "Home" }
		};

		Items.Add(shellContent);
	}


	class Issue32932CollectionViewPage : ContentPage
	{
		public Issue32932CollectionViewPage()
		{
			Title = "Issue 32932 CollectionView Page";

			var collectionView = new CollectionView2
			{
				AutomationId = "EmptyView",
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding(".")); // bind to the string item
					return label;
				}),

				// EmptyView: ContentView -> VerticalStackLayout -> Label "No values found..."
				EmptyView = new ContentView
				{
					Content = new VerticalStackLayout
					{
						Children =
						{
							new Label { Text = "No values found..." , AutomationId= "EmptyViewLabel"}
						}
					}
				}
			};

			collectionView.ItemsSource = null; // Null collection to trigger EmptyView

			// inner stack that contains the title and the CollectionView
			var innerStack = new VerticalStackLayout
			{
				Children = { collectionView }
			};

			Content = innerStack;
		}
	}
}

