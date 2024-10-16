namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2767, "ArgumentException: NaN not valid for height", PlatformAffected.All)]
public class Issue2767 : TestContentPage
{
	protected override void Init()
	{
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(0, GridUnitType.Star) },
				new RowDefinition { Height = new GridLength(60, GridUnitType.Star) },
			},
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) },
				new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) },
			}
		};
		grid.AddChild(new Label { Text = "Collapsed" }, 0, 0);
		grid.AddChild(new Label { Text = "Collapsed" }, 0, 1);
		grid.AddChild(new Label { Text = "Collapsed" }, 1, 0);
		grid.AddChild(new Label { Text = "Label 1:1" }, 1, 1);

#pragma warning disable CS0618 // Type or member is obsolete
		Content = new Frame
		{
			HorizontalOptions = LayoutOptions.CenterAndExpand,
			Content = grid
		};
#pragma warning restore CS0618 // Type or member is obsolete
	}
}