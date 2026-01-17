namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20404, "Dynamic Grid Row/Column changes don't trigger layout update on Windows until window resize", PlatformAffected.UWP)]
public class Issue20404 : TestContentPage
{
	Grid dynamicGrid;
	Button toggleRowButton;
	Button toggleColumnButton;

	protected override void Init()
	{
		Content = CreateMainGrid();
	}

	Grid CreateMainGrid()
	{
		Grid mainGrid = new Grid
		{
			RowSpacing = 8,
			ColumnSpacing = 8,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			},
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star }
			}
		};

		AddStaticGrids(mainGrid);
		AddDynamicGrid(mainGrid);
		AddControlButtons(mainGrid);
		return mainGrid;
	}

	void AddStaticGrids(Grid parent)
	{
		Grid topLeft = CreateStaticGrid("Static\n(0,0)", Colors.LightBlue);
		Grid.SetRow(topLeft, 0);
		Grid.SetColumn(topLeft, 0);
		parent.Children.Add(topLeft);

		Grid topRight = CreateStaticGrid("Static\n(0,1)", Colors.LightCoral);
		Grid.SetRow(topRight, 0);
		Grid.SetColumn(topRight, 1);
		parent.Children.Add(topRight);

		Grid bottomLeft = CreateStaticGrid("Static\n(1,0)", Colors.LightGreen);
		Grid.SetRow(bottomLeft, 1);
		Grid.SetColumn(bottomLeft, 0);
		parent.Children.Add(bottomLeft);
	}

	Grid CreateStaticGrid(string labelText, Color backgroundColor)
	{
		Grid grid = new Grid { BackgroundColor = backgroundColor };
		Label label = new Label
		{
			Text = labelText,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			TextColor = Colors.Black
		};

		grid.Children.Add(label);
		return grid;
	}

	void AddDynamicGrid(Grid parent)
	{
		dynamicGrid = new Grid
		{
			BackgroundColor = Colors.DarkBlue,
			AutomationId = "DynamicGrid"
		};

		Label label = new Label
		{
			Text = "DYNAMIC\nGRID",
			TextColor = Colors.White,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "DynamicLabel"
		};

		dynamicGrid.Children.Add(label);
		Grid.SetRow(dynamicGrid, 1);
		Grid.SetColumn(dynamicGrid, 1);

		parent.Children.Add(dynamicGrid);
	}

	void AddControlButtons(Grid parent)
	{
		toggleRowButton = new Button
		{
			Text = "Toggle Row (Current: 1)",
			AutomationId = "ToggleRowButton"
		};
		toggleRowButton.Clicked += OnToggleRowClicked;

		toggleColumnButton = new Button
		{
			Text = "Toggle Column (Current: 1)",
			AutomationId = "ToggleColumnButton"
		};
		toggleColumnButton.Clicked += OnToggleColumnClicked;

		StackLayout buttonStack = new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			HorizontalOptions = LayoutOptions.Center,
			Children = { toggleRowButton, toggleColumnButton }
		};

		Grid.SetRow(buttonStack, 3);
		Grid.SetColumnSpan(buttonStack, 2);
		parent.Children.Add(buttonStack);
	}

	void OnToggleRowClicked(object sender, EventArgs e)
	{
		Grid.SetRow(dynamicGrid, 0);
		toggleRowButton.Text = $"Toggle Row (Current: 0)";
	}

	void OnToggleColumnClicked(object sender, EventArgs e)
	{
		Grid.SetColumn(dynamicGrid, 0);
		toggleColumnButton.Text = $"Toggle Column (Current: 0)";
	}
}