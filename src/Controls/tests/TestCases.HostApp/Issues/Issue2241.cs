namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2241, "ScrollView content can become stuck on orientation change (iOS)", PlatformAffected.iOS)]
	public class Issue2241 : TestContentPage
	{
		protected override void Init()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var grid = new Grid
			{
				BackgroundColor = Colors.Red,
				HeightRequest = 400,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				AutomationId = "MainGrid"
			};
#pragma warning restore CS0618 // Type or member is obsolete
			grid.RowDefinitions.Add(new RowDefinition { Height = 10 });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = 10 });

#pragma warning disable CS0618 // Type or member is obsolete
			var boxView = new BoxView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Color = Colors.Yellow
			};
#pragma warning restore CS0618 // Type or member is obsolete
			Grid.SetRow(boxView, 0);

			var label = new Label
			{
				Text = "If the view is scrollable, scroll down to see the yellow line at the bottom of the red box. Scroll back to top. Rotate the device to landscape mode."
				+ " Scroll down to see the yellow line again. Rotate the device back to portrait while you are still looking at the yellow line at the bottom of the red box."
				+ " If the view was originally scrollable, it should still be scrollable. You should still be able to see both of the yellow lines.",
				LineBreakMode = LineBreakMode.WordWrap,
				Margin = new Thickness(10, 0),
				VerticalTextAlignment = TextAlignment.Center
			};
			Grid.SetRow(label, 1);

#pragma warning disable CS0618 // Type or member is obsolete
			var boxView2 = new BoxView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Color = Colors.Yellow
			};
#pragma warning restore CS0618 // Type or member is obsolete
			Grid.SetRow(boxView2, 2);

			grid.Children.Add(boxView);
			grid.Children.Add(label);
			grid.Children.Add(boxView2);

#pragma warning disable CS0618 // Type or member is obsolete
			var scrollView = new ScrollView
			{
				AutomationId = "TestScrollView",
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Padding = new Thickness(20),
				Content = grid
			};
#pragma warning restore CS0618 // Type or member is obsolete

			Content = scrollView;
		}
	}
}

