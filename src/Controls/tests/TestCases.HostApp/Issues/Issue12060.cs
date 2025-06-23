namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 12060, "Bug] DragGestureRecognizer shows 'Copy' tag when dragging in UWP",
		PlatformAffected.UWP)]
	public class Issue12060 : TestContentPage
	{
		protected override void Init()
		{
			Label testResult = new Label()
			{
				AutomationId = "Result",
				Text = "Running"
			};

			var drag = new DragGestureRecognizer();

			drag.DropCompleted += (_, __) =>
			{
				if (testResult.Text == "Running")
					testResult.Text = "Success";
			};

			BoxView boxView = new BoxView()
			{
				HeightRequest = 200,
				WidthRequest = 1000,
				BackgroundColor = Colors.Purple,
				GestureRecognizers =
				{
					drag
				},
				AutomationId = "DragBox"
			};

			var dropGestureRecognizer = new DropGestureRecognizer();

			dropGestureRecognizer.DragOver += (_, args) =>
			{
				args.AcceptedOperation = DataPackageOperation.None;
			};

			dropGestureRecognizer.Drop += (_, args) =>
			{
				testResult.Text = "Fail";
			};

			BoxView boxView2 = new BoxView()
			{
				HeightRequest = 200,
				WidthRequest = 1000,
				BackgroundColor = Colors.Pink,
				AutomationId = "DropBox"
			};

			boxView2.GestureRecognizers.Add(dropGestureRecognizer);

			Content = new StackLayout()
			{
				Children =
				{
					boxView,
					boxView2,
					new Label()
					{
						Text = "Drag the top box to the bottom one. The drop operation for the bottom box should be disabled.",
						AutomationId = "TestLoaded"
					},
					testResult
				}
			};
		}
	}
}
