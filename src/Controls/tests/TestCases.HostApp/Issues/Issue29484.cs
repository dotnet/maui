namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29484, "CollectionView Selected state does not work on the selected item when combined with PointerOver", PlatformAffected.UWP | PlatformAffected.macOS)]
public class Issue29484 : TestContentPage
{
	protected override void Init()
	{
		Style pointerOverSelectedStyle = CreatePointerOverSelectedItemStyle();
		DataTemplate pointerOverSelectedItemTemplate = CreateItemTemplate(pointerOverSelectedStyle);

		Grid grid = new Grid
		{
			HorizontalOptions = LayoutOptions.Center,
			RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star },
				}
		};

		grid.Add(new Label { Text = "PointerOverAndSelectedState" }, 0, 0);
		grid.Add(InitializeCollectionView(pointerOverSelectedItemTemplate), 0, 1);
		Content = grid;
	}

	CollectionView InitializeCollectionView(DataTemplate itemTemplate)
	{
		return new CollectionView
		{
			AutomationId = "CollectionView",
			ItemTemplate = itemTemplate,
			SelectionMode = SelectionMode.Single,
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3", "Item 4" }
		};
	}

	DataTemplate CreateItemTemplate(Style style)
	{
		return new DataTemplate(() =>
		{
			Label label = new Label { Style = style };
			label.SetBinding(Label.TextProperty, ".");
			return label;
		});
	}

	Style CreatePointerOverSelectedItemStyle()
	{
		return new Style(typeof(Label))
		{
			Setters =
				{
					new Setter { Property = BackgroundColorProperty, Value = Colors.Transparent },
					new Setter
					{
						Property = VisualStateManager.VisualStateGroupsProperty,
						Value = CreateVisualState()
					}
				}
		};
	}

	VisualStateGroupList CreateVisualState()
	{
		VisualStateGroupList groupList = new VisualStateGroupList();
		VisualStateGroup commonStates = new VisualStateGroup { Name = "CommonStates" };

		VisualState normalState = new VisualState { Name = "Normal" };
		VisualState pointerOverState = new VisualState { Name = "PointerOver" };
		VisualState selectedState = new VisualState { Name = "Selected" };

		pointerOverState.Setters.Add(new Setter { Property = BackgroundColorProperty, Value = Colors.DarkTurquoise });
		selectedState.Setters.Add(new Setter { Property = BackgroundColorProperty, Value = Colors.DarkBlue });

		commonStates.States.Add(pointerOverState);
		commonStates.States.Add(selectedState);
		commonStates.States.Add(normalState);

		groupList.Add(commonStates);
		return groupList;
	}
}