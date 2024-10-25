namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 11132, "[Bug] [iOS] UpdateClip throws NullReferenceException when the Name of the Mask of the Layer is null", PlatformAffected.iOS)]
	public class Issue11132 : TestContentPage
	{
		const string InstructionsId = "instructions";

		public Issue11132()
		{

		}

		protected override void Init()
		{
			Title = "Issue 11132";

			var layout = new StackLayout();

			var instructions = new Label
			{
				AutomationId = InstructionsId,
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "If the test works without exceptions (an orange rectangle is rendered), the test has passed."
			};

			var issue11132Control = new Issue11132Control
			{
				HeightRequest = 100
			};

			layout.Children.Add(instructions);
			layout.Children.Add(issue11132Control);

			Content = layout;
		}
	}


	public class Issue11132Control : View
	{

	}
}