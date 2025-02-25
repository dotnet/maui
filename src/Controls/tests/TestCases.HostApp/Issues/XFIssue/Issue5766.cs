namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5766, "Frame size gets corrupted when ListView is scrolled", PlatformAffected.Android)]
public class Issue5766 : TestContentPage
{
	const string StartText1 = "start1";
	const string BigText1 = "big string > big frame1";
	const string SmallText1 = "s1";
	const string EndText1 = "end1";
	const string List1 = "lst1";

	const string StartText2 = "start2";
	const string BigText2 = "big string > big frame2";
	const string SmallText2 = "s2";
	const string EndText2 = "end2";
	const string List2 = "lst2";

	protected override void Init()
	{
		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition(),
			},
			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star }
			}
		};
		grid.AddChild(new Label
		{
			Text = "Scroll up and down several times and make sure Frame size is accurate when using Fast Renderers.",
			VerticalTextAlignment = TextAlignment.Center
		}, 0, 0, 2);

		var template = new DataTemplate(() =>
		{
			var text = new Label
			{
				VerticalOptions = LayoutOptions.Fill,
				TextColor = Colors.White,
			};

			text.SetBinding(Label.TextProperty, ".");
			var view = new Grid
			{
				HeightRequest = 80,
				Margin = new Thickness(0, 10, 0, 0),
				BackgroundColor = Color.FromArgb("#F1F1F1")
			};
			view.AddChild(new Frame
			{
				Padding = new Thickness(5),
				Margin = new Thickness(0, 0, 10, 0),
				BorderColor = Colors.Blue,
				BackgroundColor = Colors.Gray,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.End,
				CornerRadius = 3,
				Content = text
			}, 0, 0);
			return new ViewCell
			{
				View = view
			};
		});

		grid.AddChild(new ListView
		{
			AutomationId = List1,
			HasUnevenRows = true,
			ItemsSource = (new[] { StartText1 }).Concat(Enumerable.Range(0, 99).Select(i => i % 2 != 0 ? SmallText1 : BigText1)).Concat(new[] { EndText1 }),
			ItemTemplate = template
		}, 0, 1);
		grid.AddChild(new ListView(ListViewCachingStrategy.RecycleElement)
		{
			AutomationId = List2,
			HasUnevenRows = true,
			ItemsSource = (new[] { StartText2 }).Concat(Enumerable.Range(0, 99).Select(i => i % 2 != 0 ? SmallText2 : BigText2)).Concat(new[] { EndText2 }),
			ItemTemplate = template
		}, 1, 1);
		Content = grid;
	}
}
