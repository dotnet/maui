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

	// ContentButton content is hidden from accessibility, so UI tests cannot query these labels.
	// The page reports their window positions in a visible label instead.
	readonly Dictionary<string, Label> _labels = new();
	readonly Label _result = new() { AutomationId = "MeasureResult", HorizontalOptions = LayoutOptions.Center };

	public Issue8191Page()
	{
		SafeAreaEdges = SafeAreaEdges.None;
		Background = Colors.LightGray;

		var measure = new Button { Text = "Measure", AutomationId = "MeasureButton", HorizontalOptions = LayoutOptions.Center };
		measure.Clicked += (_, _) => _result.Text = string.Join(";", _labels.Select(pair => $"{pair.Key}={WindowX(pair.Value):0}"));

		var root = new Grid
		{
			SafeAreaEdges = SafeAreaEdges.None,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Star),
			},
		};

		root.Add(new HorizontalStackLayout { HorizontalOptions = LayoutOptions.Center, Spacing = 12, Children = { measure, _result } }, 0, 0);
		root.Add(CreateCollectionView(() => new Border
		{
			SafeAreaEdges = ItemSafeAreaEdges,
			Background = Colors.LightGreen,
			StrokeThickness = 0,
			Padding = 12,
			Content = CreateLabel("reference", "Border"),
		}), 0, 1);
		root.Add(CreateCollectionView(() => CreateButton("direct", hasShadow: false)), 0, 2);
		root.Add(CreateCollectionView(() => CreateButton("wrapped", hasShadow: true)), 0, 3);

		Content = root;
	}

	static CollectionView2 CreateCollectionView(Func<View> createItem) => new()
	{
		ItemsSource = new[] { "Item" },
		ItemTemplate = new DataTemplate(createItem),
	};

	ContentButton CreateButton(string name, bool hasShadow) => new()
	{
		SafeAreaEdges = ItemSafeAreaEdges,
		Background = Colors.LightPink,
		BorderWidth = 0,
		Padding = 12,
		Shadow = hasShadow
			? new Shadow { Brush = Colors.Black, Opacity = 0.4f, Radius = 8, Offset = new Point(0, 2) }
			: null,
		Content = CreateLabel(name, "ContentButton"),
	};

	Label CreateLabel(string name, string text)
	{
		var label = new Label { Text = text, FontSize = 24, TextColor = Colors.Black };
		_labels[name] = label;
		return label;
	}

	static double WindowX(Label label) =>
#if IOS || MACCATALYST
		label.Handler?.PlatformView is UIKit.UIView view
			? view.ConvertPointToView(CoreGraphics.CGPoint.Empty, null).X
			: double.NaN;
#else
		double.NaN;
#endif
}
