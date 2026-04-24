namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19542, "Flyout item didnt take full width", PlatformAffected.UWP)]
public class Issue19542 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Locked;

		// Set custom item template
		ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				},
				BackgroundColor = Colors.LightGreen
			};

			var leftImage = new Image
			{
				HeightRequest = 30,
				BackgroundColor = Colors.Salmon
			};
			leftImage.SetBinding(Image.SourceProperty, "Icon");

			var titleLabel = new Label
			{
				FontAttributes = FontAttributes.Italic,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center,
				BackgroundColor = Colors.Cyan
			};
			titleLabel.SetBinding(Label.TextProperty, "Title");

			var rightImage = new Image
			{
				HeightRequest = 30,
				BackgroundColor = Colors.Teal
			};
			rightImage.SetBinding(Image.SourceProperty, "Icon");

			grid.Add(leftImage, 0, 0);
			grid.Add(titleLabel, 1, 0);
			grid.Add(rightImage, 2, 0);

			return grid;
		});

		// Add only one FlyoutItem
		var singleItem = new FlyoutItem
		{
			Title = "Flyout Item",
			Icon = "dotnet_bot.png",
		};

		singleItem.Items.Add(new ShellContent
		{
			ContentTemplate = new DataTemplate(() =>
			{
				return new ContentPage
				{
					Content = new Label
					{
						AutomationId = "Label19542",
						Text = "Test passes if flyout item takes full width",
					}
				};
			})
		});

		Items.Add(singleItem);
	}
}
