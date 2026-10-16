namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39202, "SafeAreaEdges on a CollectionView item template root is ignored when the root has a Shadow", PlatformAffected.iOS)]
public class Issue39202 : Shell
{
	public Issue39202()
	{
		Shell.SetNavBarIsVisible(this, false);
		Items.Add(new ShellContent
		{
			ContentTemplate = new DataTemplate(typeof(Issue39202Page)),
		});
	}
}

public class Issue39202Page : ContentPage
{
	static readonly SafeAreaEdges ItemSafeAreaEdges = new(
		SafeAreaRegions.Container,
		SafeAreaRegions.None,
		SafeAreaRegions.Container,
		SafeAreaRegions.None);

	public Issue39202Page()
	{
		SafeAreaEdges = SafeAreaEdges.None;
		Background = Colors.LightGray;

		var root = new Grid
		{
			SafeAreaEdges = SafeAreaEdges.None,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Star),
			},
		};

		root.Add(CreateCollectionView("Without Shadow", "NoShadowLabel", Colors.LightGreen, false), 0, 0);
		root.Add(CreateCollectionView("With Shadow", "ShadowLabel", Colors.LightPink, true), 0, 1);

		Content = root;
	}

	static CollectionView2 CreateCollectionView(string text, string labelAutomationId, Color background, bool hasShadow)
	{
		return new CollectionView2
		{
			ItemsSource = new[] { text },
			ItemTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					SafeAreaEdges = ItemSafeAreaEdges,
					Background = background,
					StrokeThickness = 0,
					Padding = 12,
					Margin = new Thickness(0, 8),
					Content = new Label
					{
						AutomationId = labelAutomationId,
						Text = text,
						FontSize = 24,
						TextColor = Colors.Black,
					},
				};

				if (hasShadow)
				{
					border.Shadow = new Shadow
					{
						Brush = Colors.Black,
						Opacity = 0.4f,
						Radius = 8,
						Offset = new Point(0, 2),
					};
				}

				return border;
			}),
		};
	}
}
