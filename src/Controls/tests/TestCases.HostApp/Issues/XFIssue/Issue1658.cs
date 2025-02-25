namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1658, "[macOS] GestureRecognizer on ListView Item not working", PlatformAffected.macOS)]

public class Issue1658 : TestNavigationPage
{
	protected override void Init()
	{
		var page = new ContentPage();

		PushAsync(page);

		page.Content = new ListView()
		{
			ItemsSource = new[] { "1" },
			ItemTemplate = new DataTemplate(() =>
			{
				ViewCell cells = new ViewCell();

				cells.ContextActions.Add(new MenuItem()
				{
					IconImageSource = "coffee.png",
					AutomationId = "coffee.png"
				});

				var label = new Label
				{
					Text = "Tap label",
					AutomationId = "labelId"
				};

				var gr = new TapGestureRecognizer();
				gr.Command = new Command(() =>
				{
					label.Text = label.Text == "Tap label" ? "Success" : "Tap label";
				});
				label.GestureRecognizers.Add(gr);
				cells.View = new StackLayout()
				{
					Orientation = StackOrientation.Horizontal,
					Children =
					{
						new Label()
						{
							Text = "Right click on any item within viewcell (including this label) should trigger context action on this row and you should see a coffee cup. Tap on colored box should change box color",
							AutomationId = "ListViewItem",
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.FillAndExpand
						},
						label
					}
				};

				return cells;
			})
		};
	}
}
