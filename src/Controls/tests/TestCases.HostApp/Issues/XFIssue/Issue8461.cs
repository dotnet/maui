namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8461, "[Bug] [iOS] [Shell] Nav Stack consistency error", PlatformAffected.iOS)]
public class Issue8461 : TestShell
{
	const string ButtonId = "PageButtonId";
	const string LayoutId = "LayoutId";

	protected override void Init()
	{
		var page1 = CreateContentPage("page 1");
		var page2 = new ContentPage() { Title = "page 2" };

		var pushPageBtn = new Button();
		pushPageBtn.Text = "Push Page";
		pushPageBtn.AutomationId = ButtonId;
		pushPageBtn.Clicked += (sender, args) =>
		{
			Navigation.PushAsync(page2);
		};

		page1.Content = new StackLayout()
		{
			pushPageBtn
		};

		var instructions = new StackLayout()
		{
			new Label()
			{
				AutomationId = "InstructionsLabel",
				Text = "1. Swipe left to dismiss this page, but cancel the gesture before it completes"
			},
			new Label()
			{
				Text = "2. Swipe left to dismiss this page again, crashes immediately"
			}
		};

		Grid.SetColumn(instructions, 1);

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		var grid = new Grid()
		{
			HorizontalOptions = LayoutOptions.FillAndExpand,
			VerticalOptions = LayoutOptions.FillAndExpand,

			ColumnDefinitions = new ColumnDefinitionCollection()
			{
				new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) },
				new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) },
			},
		};

		// Use this BoxView to anchor our swipe to left of the screen
		grid.Children.Add(new BoxView()
		{
			AutomationId = LayoutId,
			HorizontalOptions = LayoutOptions.FillAndExpand,
			VerticalOptions = LayoutOptions.FillAndExpand,
			BackgroundColor = Colors.Red
		});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		grid.Children.Add(instructions);

		page2.Content = grid;
	}
}