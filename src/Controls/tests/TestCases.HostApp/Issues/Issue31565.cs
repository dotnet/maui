using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31565, "FlexLayout alignment issue when Wrap is set to Reverse and AlignContent is set to SpaceAround, SpaceBetween or SpaceEvenly", PlatformAffected.All)]
public class Issue31565 : ContentPage
{
	private FlexLayout flexLayout;
	public Issue31565()
	{
		// Root Grid
		var rootGrid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto }
				},
			BackgroundColor = Colors.LightGreen
		};

		// FlexLayout (Row 0)
		flexLayout = new FlexLayout
		{
			AutomationId = "FlexLayoutControl",
			Wrap = FlexWrap.Reverse,
			BackgroundColor = Colors.LightGray
		};

		// Add Borders (Children)
		flexLayout.Children.Add(CreateChild("Child1", Color.FromArgb("#4e79a7")));
		flexLayout.Children.Add(CreateChild("Child2", Color.FromArgb("#59a14f")));
		flexLayout.Children.Add(CreateChild("Child3", Color.FromArgb("#e15759")));
		flexLayout.Children.Add(CreateChild("Child4", Color.FromArgb("#FFDA8D00")));
		flexLayout.Children.Add(CreateChild("Child5", Color.FromArgb("#FF009DC0")));
		flexLayout.Children.Add(CreateChild("Child6", Color.FromArgb("#FF009DC0")));
		flexLayout.Children.Add(CreateChild("Child7", Color.FromArgb("#FF009DC0")));

		rootGrid.Add(flexLayout, 0, 0);

		// Bottom Grid (Row 1)
		var bottomGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				}
		};

		var label = new Label
		{
			Text = "AlignContent:",
			FontSize = 16,
			VerticalOptions = LayoutOptions.Center
		};
		bottomGrid.Add(label, 0, 0);

		var buttonLayout = new VerticalStackLayout();
		buttonLayout.Children.Add(CreateButton("SpaceAround"));
		buttonLayout.Children.Add(CreateButton("SpaceBetween"));
		buttonLayout.Children.Add(CreateButton("SpaceEvenly"));

		bottomGrid.Add(buttonLayout, 1, 0);
		rootGrid.Add(bottomGrid, 0, 1);

		Content = rootGrid;
	}

	private View CreateChild(string text, Color color)
	{
		return new Border
		{
			BackgroundColor = color,
			WidthRequest = 70,
			HeightRequest = 70,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 8 },
			Content = new Label
			{
				Text = text,
				TextColor = Colors.White,
				FontSize = 12,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
	}

	private Button CreateButton(string text)
	{
		var button = new Button
		{
			Text = text,
			AutomationId = text + "Button"
		};
		button.Clicked += OnAlignContentClicked;
		return button;
	}

	private void OnAlignContentClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			switch (button.Text)
			{
				case "SpaceAround":
					flexLayout.AlignContent = FlexAlignContent.SpaceAround;
					break;
				case "SpaceBetween":
					flexLayout.AlignContent = FlexAlignContent.SpaceBetween;
					break;
				case "SpaceEvenly":
					flexLayout.AlignContent = FlexAlignContent.SpaceEvenly;
					break;
			}
		}
	}

}
