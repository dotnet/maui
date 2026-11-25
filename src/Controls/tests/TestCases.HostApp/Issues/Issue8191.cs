namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8191, "ContentButton item template roots apply CollectionView cell safe areas", PlatformAffected.iOS)]
public class Issue8191 : Shell
{
	public Issue8191()
	{
		Shell.SetNavBarIsVisible(this, false);
		Items.Add(new ShellContent
		{
			ContentTemplate = new DataTemplate(typeof(Issue8191Page)),
		});
	}
}

public class Issue8191Page : ContentPage
{
	// Horizontal edges respect the cell's unsafe region while vertical edges do not, so the
	// cell must inject a per-cell override rather than relying on the uniform parent inset.
	static readonly SafeAreaEdges ItemSafeAreaEdges = new(
		SafeAreaRegions.Container,
		SafeAreaRegions.None,
		SafeAreaRegions.Container,
		SafeAreaRegions.None);

	public Issue8191Page()
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
				new RowDefinition(GridLength.Star),
			},
		};

		root.Add(CreateCollectionView(() => new Border
		{
			SafeAreaEdges = ItemSafeAreaEdges,
			Background = Colors.LightGreen,
			StrokeThickness = 0,
			Padding = 12,
			Content = CreateLabel("ReferenceLabel", "Border"),
		}), 0, 0);
		root.Add(CreateCollectionView(() => CreateButton("ContentButtonLabel", hasShadow: false)), 0, 1);
		root.Add(CreateCollectionView(() => CreateButton("ShadowContentButtonLabel", hasShadow: true)), 0, 2);

		Content = root;
	}

	static CollectionView2 CreateCollectionView(Func<View> createItem) => new()
	{
		ItemsSource = new[] { "Item" },
		ItemTemplate = new DataTemplate(createItem),
	};

	static ContentButton CreateButton(string labelAutomationId, bool hasShadow) => new()
	{
		SafeAreaEdges = ItemSafeAreaEdges,
		Background = Colors.LightPink,
		BorderWidth = 0,
		Padding = 12,
		Shadow = hasShadow
			? new Shadow { Brush = Colors.Black, Opacity = 0.4f, Radius = 8, Offset = new Point(0, 2) }
			: null,
		Content = CreateLabel(labelAutomationId, "ContentButton"),
	};

	static Label CreateLabel(string automationId, string text) => new()
	{
		AutomationId = automationId,
		Text = text,
		FontSize = 24,
		TextColor = Colors.Black,
	};
}
