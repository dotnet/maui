namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 46458, "Grid.IsEnabled property is not working", PlatformAffected.Android)]
	public class Bugzilla46458 : TestContentPage
	{
		protected override void Init()
		{
			var parentGrid = new Grid
			{
				BackgroundColor = Colors.Yellow
			};
			parentGrid.RowDefinitions.Add(new RowDefinition());
			parentGrid.RowDefinitions.Add(new RowDefinition());

			var grid = new Grid
			{
				IsEnabled = false,
				BackgroundColor = Colors.Red
			};

			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());

			var label = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				Text = "Success"
			};
			Grid.SetRow(label, 0);
			grid.Children.Add(label);

			var entry = new Entry
			{
				HorizontalOptions = LayoutOptions.Center,
				HeightRequest = 50,
				WidthRequest = 250,
				Placeholder = "Placeholder",
				AutomationId = "entry"
			};
			Grid.SetRow(entry, 1);
			entry.Focused += (sender, args) => { label.Text = "Fail"; };
			grid.Children.Add(entry);

			var button = new Button
			{
				WidthRequest = 250,
				HeightRequest = 50,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Click",
				HorizontalOptions = LayoutOptions.Center,
				Command = new Command(() => { label.Text = "Fail"; }),
				AutomationId = "button"
			};
			Grid.SetRow(button, 2);
			grid.Children.Add(button);

			parentGrid.Children.Add(grid);
			Grid.SetRow(grid, 1);

			var button1 = new Button
			{
				WidthRequest = 250,
				HeightRequest = 50,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Test transparency",
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = "button1"
			};
			button1.Command = new Command((sender) =>
			{
				grid.IsEnabled = true;
				grid.InputTransparent = true;
				button1.Text = "Clicked";
			});
			Grid.SetRow(button1, 0);
			parentGrid.Children.Add(button1);

			Content = parentGrid;
		}
	}
}