namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33604, "CollectionView does not respect content SafeAreaEdges choices", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue33604 : Shell
{
	public Issue33604()
	{
		Shell.SetNavBarIsVisible(this, false);
		Items.Add(new ShellContent
		{
			Route = "MainPage",
			ContentTemplate = new DataTemplate(typeof(Issue33604Page)),
		});
	}
}

public class Issue33604Page : ContentPage
{
	public Issue33604Page()
	{
		BackgroundColor = Colors.Blue;

		var labelStyle = new Style(typeof(Label));
		labelStyle.Setters.Add(new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Fill });
		labelStyle.Setters.Add(new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Aquamarine });
		labelStyle.Setters.Add(new Setter { Property = Label.PaddingProperty, Value = new Thickness(4, 0) });
		labelStyle.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 32.0 });
		Resources = new ResourceDictionary { labelStyle };

		var containerNone = new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.None);

		var topLabel = new Label
		{
			AutomationId = "TopLabel",
			Text = "Hello, World! Left Side Test",
			HorizontalTextAlignment = TextAlignment.Start,
		};
		var topView = new ContentView { BackgroundColor = Colors.Plum, Content = topLabel };
		topView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

		var collectionView = new CollectionView2
		{
			AutomationId = "TestCollectionView",
			BackgroundColor = Colors.Red,
			IsGrouped = true,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
					BackgroundColor = Color.FromArgb("#2F4F4F"),
					Margin = new Thickness(0, 2),
					RowSpacing = 4,
					SafeAreaEdges = containerNone,
				};

				var leftLabel = new Label { FontSize = 22.0, HorizontalTextAlignment = TextAlignment.Start };
				leftLabel.SetBinding(Label.TextProperty, "LeftTest");

				var rightLabel = new Label { FontSize = 22.0, HorizontalTextAlignment = TextAlignment.End };
				rightLabel.SetBinding(Label.TextProperty, "RightTest");

				grid.Add(leftLabel, 0, 0);
				grid.Add(rightLabel, 0, 1);
				return grid;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
					BackgroundColor = Colors.LightGray,
					Margin = new Thickness(0, 2),
					RowSpacing = 4,
					SafeAreaEdges = containerNone,
				};

				var leftLabel = new Label { FontSize = 12.0, HorizontalTextAlignment = TextAlignment.Start };
				leftLabel.SetBinding(Label.TextProperty, "LeftTest");

				var rightLabel = new Label { FontSize = 12.0, HorizontalTextAlignment = TextAlignment.End };
				rightLabel.SetBinding(Label.TextProperty, "RightTest");

				grid.Add(leftLabel, 0, 0);
				grid.Add(rightLabel, 0, 1);
				return grid;
			}),
		};

		var bottomLabel = new Label
		{
			AutomationId = "BottomLabel",
			Text = "Testing Right Side Here",
			HorizontalTextAlignment = TextAlignment.End,
		};
		var bottomView = new ContentView { BackgroundColor = Colors.Plum, Content = bottomLabel };
		bottomView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

		var rootGrid = new Grid
		{
			RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
			BackgroundColor = Colors.Yellow,
			SafeAreaEdges = SafeAreaEdges.None,
		};

		rootGrid.Add(topView, 0, 0);
		rootGrid.Add(collectionView, 0, 1);
		rootGrid.Add(bottomView, 0, 2);

		Content = rootGrid;

		var groups = new List<Issue33604ModelGroup>();
		for (var i = 1; i <= 10; i++)
		{
			var group = new Issue33604ModelGroup
			{
				LeftTest = $"Title Left {i}",
				RightTest = $"Title Right {i}",
			};
			for (var j = 1; j <= i; j++)
			{
				group.Add(new Issue33604Model
				{
					LeftTest = $"Content Test for Left Side {i}",
					RightTest = $"Content Test for Right Side {i}",
				});
			}
			groups.Add(group);
		}
		collectionView.ItemsSource = groups;
	}
}

public class Issue33604ModelGroup : List<Issue33604Model>
{
	public string LeftTest { get; set; }
	public string RightTest { get; set; }
}

public class Issue33604Model
{
	public string LeftTest { get; set; }
	public string RightTest { get; set; }
}
