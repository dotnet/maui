namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36816, "Android: Clicks being passed through to controls under other Views", PlatformAffected.Android)]
public class Issue36816 : ContentPage
{
	const string ButtonClickedText = "Button Clicked";
	const string InitialText = "Not Clicked";
	readonly Grid _overlappingGrid;
	readonly Label _resultLabel;
	readonly Label _parentTapLabel;
	readonly Label _parentPanLabel;
	readonly Label _childResultLabel;
	View _overlay;
	int _parentTapCount;

	public Issue36816()
	{
		_resultLabel = new Label
		{
			Text = InitialText,
			AutomationId = "ResultLabel"
		};
		_parentTapLabel = new Label { Text = "0", AutomationId = "ParentTapCount" };
		_parentPanLabel = new Label { Text = "None", AutomationId = "ParentPanStatus" };
		_childResultLabel = new Label { Text = InitialText, AutomationId = "ChildResultLabel" };

		var button = new Button
		{
			Text = "Click me",
			AutomationId = "CounterBtn",
			HorizontalOptions = LayoutOptions.Fill
		};
		button.Clicked += (sender, e) => _resultLabel.Text = ButtonClickedText;

		_overlappingGrid = new Grid
		{
			AutomationId = "OverlappingGrid",
			HeightRequest = 120
		};
		_overlappingGrid.Children.Add(button);
		_overlay = CreateOverlay("ContentView");
		_overlappingGrid.Children.Add(_overlay);

		var selectors = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition()
			},
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)
			}
		};
		var types = new[] { "ContentView", "Grid", "StackLayout", "BoxView", "Border" };
		for (int i = 0; i < types.Length; i++)
		{
			var type = types[i];
			var selector = CreateButton(type, "Select" + type, () => SelectOverlay(type));
			selectors.Add(selector, i % 3, i / 3);
		}

		Content = new VerticalStackLayout
		{
			Spacing = 8,
			Padding = 12,
			Children =
			{
				selectors,
				new HorizontalStackLayout
				{
					Children =
					{
						CreateButton("Toggle transparency", "ToggleTransparency",
							() => _overlay.InputTransparent = !_overlay.InputTransparent),
						CreateButton("Reset results", "ResetResults", ResetResults)
					}
				},
				new HorizontalStackLayout
				{
					Children =
					{
						CreateButton("Shadow", "ToggleShadow",
							() => _overlay.Shadow = _overlay.Shadow is null ? new Shadow() : null),
						CreateButton("Clip", "ToggleClip", () =>
							_overlay.Clip = _overlay.Clip is null
								? new Microsoft.Maui.Controls.Shapes.RectangleGeometry
								{
									Rect = new Rect(0, 0, _overlay.Width, _overlay.Height)
								}
								: null),
						CreateButton("Cascade", "ToggleCascade", () =>
						{
							if (_overlay is Layout layout)
								layout.CascadeInputTransparent = !layout.CascadeInputTransparent;
						})
					}
				},
				new HorizontalStackLayout
				{
					Children =
					{
						CreateButton("Parent tap", "EnableParentTap", EnableParentTap),
						CreateButton("Parent pan", "EnableParentPan", EnableParentPan),
						CreateButton("Child", "ShowChildButton", ShowChildButton)
					}
				},
				_overlappingGrid,
				new HorizontalStackLayout
				{
					Spacing = 12,
					Children = { _resultLabel, _parentTapLabel, _parentPanLabel, _childResultLabel }
				}
			}
		};
	}

	void SelectOverlay(string type)
	{
		_overlappingGrid.Children.Remove(_overlay);
		_overlappingGrid.GestureRecognizers.Clear();
		_overlay = CreateOverlay(type);
		_overlappingGrid.Children.Add(_overlay);
		ResetResults();
	}

	View CreateOverlay(string type)
	{
		View overlay = type switch
		{
			"ContentView" => new ContentView(),
			"Grid" => new Grid(),
			"StackLayout" => new StackLayout(),
			"BoxView" => new BoxView { Color = Colors.Green },
			"Border" => new Border(),
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown overlay type.")
		};
		if (overlay is not BoxView)
			overlay.Background = Colors.Green;
		overlay.AutomationId = "OverlayView";
		overlay.HeightRequest = 120;
		SetOverlayContent(overlay, new Label
		{
			Text = "Tap anywhere on this card",
			TextColor = Colors.White,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "CardLabel"
		});
		return overlay;
	}

	static void SetOverlayContent(View overlay, View content)
	{
		switch (overlay)
		{
			case ContentView contentView:
				contentView.Content = content;
				break;
			case Border border:
				border.Content = content;
				break;
			case Layout layout:
				layout.Children.Clear();
				layout.Children.Add(content);
				break;
		}
	}

	void EnableParentTap()
	{
		_overlappingGrid.GestureRecognizers.Clear();
		var tap = new TapGestureRecognizer();
		tap.Tapped += (sender, e) => _parentTapLabel.Text = (++_parentTapCount).ToString();
		_overlappingGrid.GestureRecognizers.Add(tap);
	}

	void EnableParentPan()
	{
		_overlappingGrid.GestureRecognizers.Clear();
		var pan = new PanGestureRecognizer();
		pan.PanUpdated += (sender, e) => _parentPanLabel.Text = e.StatusType.ToString();
		_overlappingGrid.GestureRecognizers.Add(pan);
	}

	void ShowChildButton()
	{
		var child = new Button { Text = "Child button", AutomationId = "ChildButton" };
		child.Clicked += (sender, e) => _childResultLabel.Text = ButtonClickedText;
		SetOverlayContent(_overlay, child);
	}

	void ResetResults()
	{
		_resultLabel.Text = InitialText;
		_parentTapCount = 0;
		_parentTapLabel.Text = "0";
		_parentPanLabel.Text = "None";
		_childResultLabel.Text = InitialText;
	}

	static Button CreateButton(string text, string automationId, Action action)
	{
		var button = new Button { Text = text, AutomationId = automationId, FontSize = 12 };
		button.Clicked += (sender, e) => action();
		return button;
	}
}
