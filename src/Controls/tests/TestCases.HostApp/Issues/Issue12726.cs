namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 12726, "Drag and Drop Gesture Fails on Runtime Changes of CanDrag and AllowDrop in Windows",
		PlatformAffected.UWP)]
	public class Issue12726 : TestContentPage
	{
		protected override void Init()
		{
			Label dragResult = new Label();
			Label dropResult = new Label();

			DragGestureRecognizer dragGestureRecognizer = new DragGestureRecognizer { CanDrag = false };
			Label dragBox = new Label
			{
				HeightRequest = 200,
				WidthRequest = 200,
				BackgroundColor = Colors.Purple,
				GestureRecognizers = { dragGestureRecognizer },
				AutomationId = "DragElement"
			};
			dragGestureRecognizer.DragStarting += (_, __) => dragResult.Text = "DragEventTriggered";

			DropGestureRecognizer dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = false };
			Label dropBox = new Label
			{
				HeightRequest = 200,
				WidthRequest = 200,
				BackgroundColor = Colors.Pink,
				GestureRecognizers = { dropGestureRecognizer },
				AutomationId = "DropTarget"
			};
			dropGestureRecognizer.Drop += (_, __) => dropResult.Text = "DropEventTriggered";

			Button button = new Button
			{
				Text = "Enable Drag and Drop",
				AutomationId = "EnableDragAndDrop"
			};

			button.Clicked += (_, __) =>
			{
				dragGestureRecognizer.CanDrag = true;
				dropGestureRecognizer.AllowDrop = true;
			};

			Content = new StackLayout
			{
				Spacing = 5,
				Children =
				{
					dragBox,
					dropBox,
					button,
					dragResult,
					dropResult
				}
			};
		}
	}
}
