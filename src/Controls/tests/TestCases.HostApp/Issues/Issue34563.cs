namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34563, "[iOS] Vertical SafeAreaEdge isn't respected when Top and Bottom constraints mismatch", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34563 : ContentPage
{
	readonly ContentView _parentSafeAreaContainer;
	readonly Grid _childSafeAreaContainer;
	readonly Label _statusLabel;
	bool _parentHandlesTop;

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

		var bottomMarker = new Label
		{
			AutomationId = "BottomMarker",
			BackgroundColor = Colors.Blue,
			HeightRequest = 30,
			HorizontalOptions = LayoutOptions.Fill,
			HorizontalTextAlignment = TextAlignment.Center,
			Text = "Bottom safe-area marker",
			TextColor = Colors.White,
			VerticalOptions = LayoutOptions.End,
		};

		_statusLabel = new Label
		{
			AutomationId = "SafeAreaStatusLabel",
			HorizontalTextAlignment = TextAlignment.Center,
		};

		var toggleParentEdgeButton = new Button
		{
			AutomationId = "ToggleParentEdgeButton",
			Text = "Swap parent safe-area edge",
		};
		toggleParentEdgeButton.Clicked += (_, _) =>
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
					Text = "The child handles Top and Bottom while its parent swaps between handling exactly one of those edges.",
					HorizontalTextAlignment = TextAlignment.Center,
				},
				toggleParentEdgeButton,
				_statusLabel,
			}
		};

		_childSafeAreaContainer = new Grid
		{
			AutomationId = "ChildSafeAreaContainer",
		};
		_childSafeAreaContainer.Add(topMarker);
		_childSafeAreaContainer.Add(bottomMarker);
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
			_parentHandlesTop ? SafeAreaRegions.None : SafeAreaRegions.Container);

		_childSafeAreaContainer.SafeAreaEdges = new SafeAreaEdges(
			SafeAreaRegions.None,
			SafeAreaRegions.Container,
			SafeAreaRegions.None,
			SafeAreaRegions.Container);

		_statusLabel.Text =
			$"Parent: Top={(_parentHandlesTop ? "Container" : "None")}, " +
			$"Bottom={(_parentHandlesTop ? "None" : "Container")} | " +
			"Child: Top=Container, Bottom=Container";
	}
}
