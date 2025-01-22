namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27169, "[iOS] ScrollView content was being clipped", PlatformAffected.iOS)]
public partial class Issue27169 : ContentPage
{
	public Issue27169()
	{
		SetupLayout();
	}

	private void SetupLayout()
	{
		var mainGrid = new Grid
		{
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Start,
			Padding = 0,
			BackgroundColor = Colors.Green,
		};

		mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
		var contentSection = CreateContentSection();
		mainGrid.Children.Add(contentSection);
		Grid.SetRow(contentSection, 0);

		mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

		var frame = new Border
		{
			Stroke = Colors.Green,
			Content = mainGrid,
			HorizontalOptions = LayoutOptions.Fill,
			MaximumWidthRequest = 800
		};

		var scrollView = new ScrollView
		{
			Content = frame,
			Padding = 10,
			HorizontalOptions = LayoutOptions.Fill
		};

		Content = new Grid
		{
			Children = { scrollView },
			AutomationId = "gridView",
		};
	}

	private View CreateContentSection()
	{
		var grid = new Grid
		{
			Padding = 10,
			ColumnSpacing = 10,
		};

		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
		var columnContent = CreateColumnContent();
		grid.Children.Add(columnContent);
		Grid.SetColumn(columnContent, 0);

		return new Border
		{
			Stroke = Colors.Red,
			Content = grid,
			VerticalOptions = LayoutOptions.Start,
			Margin = new Thickness(10),
		};
	}

	private View CreateColumnContent()
	{
		var stack = new Grid
		{
			Padding = 5,
			RowSpacing = 5,
		};

		for (int i = 0; i < 10; i++)
		{
			var button = new Button
			{
				HeightRequest = 50,
				BackgroundColor = Colors.Blue,
				TextColor = Colors.White,
				Text = $"Button {i}",
			};

			stack.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			stack.Children.Add(button);
			Grid.SetRow(button, i);
		}

		return new Border
		{
			Stroke = Colors.Purple,
			Content = stack,
			VerticalOptions = LayoutOptions.Start,
		};
	}
}