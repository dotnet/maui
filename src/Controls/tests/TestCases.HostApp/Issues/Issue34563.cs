namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34563, "[iOS] Vertical SafeAreaEdge isn't respected when Top and Bottom constraints mismatch", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34563 : ContentPage
{
	readonly ContentView _parentSafeAreaContainer;
	readonly Grid _childSafeAreaContainer;
	readonly Label _statusLabel;
	bool _parentHandlesTop;
	bool _childHandlesTop = true;

	public Issue34563()
	{
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None);
		BackgroundColor = Colors.Red;

		var topMarker = new Label
		{
			AutomationId = "TopMarker",
			BackgroundColor = Colors.LimeGreen,
			HeightRequest = 30,
			HorizontalOptions = LayoutOptions.Fill,
			HorizontalTextAlignment = TextAlignment.Center,
			Text = "Top safe-area marker",
			VerticalOptions = LayoutOptions.Start,
		};

		_statusLabel = new Label
		{
			AutomationId = "SafeAreaStatusLabel",
			HorizontalTextAlignment = TextAlignment.Center,
		};

		var toggleChildTopButton = new Button
		{
			AutomationId = "ToggleChildTopButton",
			Text = "Toggle child top safe area",
		};
		toggleChildTopButton.Clicked += (_, _) =>
		{
			_childHandlesTop = !_childHandlesTop;
			UpdateSafeAreaEdges();
		};

		var toggleParentTopButton = new Button
		{
			AutomationId = "ToggleParentTopButton",
			Text = "Toggle parent top safe area",
		};
		toggleParentTopButton.Clicked += (_, _) =>
		{
			_parentHandlesTop = !_parentHandlesTop;
			UpdateSafeAreaEdges();
		};

		var controls = new VerticalStackLayout
		{
			Spacing = 12,
			Margin = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "The green marker must receive exactly one top safe-area inset.",
					HorizontalTextAlignment = TextAlignment.Center,
				},
				toggleChildTopButton,
				toggleParentTopButton,
				_statusLabel,
			}
		};

		_childSafeAreaContainer = new Grid
		{
			AutomationId = "ChildSafeAreaContainer",
		};
		_childSafeAreaContainer.Add(topMarker);
		_childSafeAreaContainer.Add(controls);

		_parentSafeAreaContainer = new ContentView
		{
			AutomationId = "ParentSafeAreaContainer",
			Content = _childSafeAreaContainer,
		};

		var rootGrid = new Grid { AutomationId = "RootGrid" };
		rootGrid.Add(_parentSafeAreaContainer);

		UpdateSafeAreaEdges();
		Content = rootGrid;
	}

	void UpdateSafeAreaEdges()
	{
		_parentSafeAreaContainer.SafeAreaEdges = new SafeAreaEdges(
			SafeAreaRegions.None,
			_parentHandlesTop ? SafeAreaRegions.Container : SafeAreaRegions.None,
			SafeAreaRegions.None,
			SafeAreaRegions.Container);

		_childSafeAreaContainer.SafeAreaEdges = new SafeAreaEdges(
			SafeAreaRegions.None,
			_childHandlesTop ? SafeAreaRegions.Container : SafeAreaRegions.None,
			SafeAreaRegions.None,
			SafeAreaRegions.None);

		_statusLabel.Text =
			$"Parent: Top={(_parentHandlesTop ? "Container" : "None")}, Bottom=Container | " +
			$"Child: Top={(_childHandlesTop ? "Container" : "None")}";
	}
}
