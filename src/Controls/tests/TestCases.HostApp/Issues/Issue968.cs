namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 968, "StackLayout does not relayout on device rotation", PlatformAffected.iOS)]
	public class Issue968 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Nav Bar";

			var layout = new StackLayout
			{
				Padding = new Thickness(20),
				BackgroundColor = Colors.Gray,
				AutomationId = "TestReady"
			};

#pragma warning disable CS0618 // Type or member is obsolete
			layout.Children.Add(new BoxView
			{
				BackgroundColor = Colors.Red,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
			});
#pragma warning restore CS0618 // Type or member is obsolete

			layout.Children.Add(new Label
			{
				Text = "You should see me after rotating"
			});

			Content = layout;
		}
	}
}
