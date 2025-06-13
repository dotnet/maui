namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29634, "iOS CV: Empty view not resizing when bounds change", PlatformAffected.iOS)]
public class Issue29634 : ContentPage
{
	public Issue29634()
	{
		var cv = new CollectionView
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 10 },
			ItemTemplate = new DataTemplate(),
			EmptyView = new Button
			{
				Margin=new Thickness(0,0,0,5),
				BackgroundColor=Colors.LightSeaGreen,
				FontSize = 12,
				TextColor = Colors.DarkSlateGray,
				Text="Button text"
			}
		};
		Grid.SetColumn(cv, 1);

		Content = new Grid
		{
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)
			],
			Children = { cv }
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Task.Delay(300);
		((Grid)Content).Add(new Label { Text = "StubLabel", AutomationId = "StubLabel" });
	}
}