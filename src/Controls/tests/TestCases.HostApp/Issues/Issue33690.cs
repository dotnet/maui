namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33690, "PointerGestureRecognizer does not fire off PointerMove event on Android", PlatformAffected.Android)]
public class Issue33690 : ContentPage
{
	Label pointerPressedLabel;
	Label pointerMovedLabel;
	Label pointerReleasedLabel;
	int pressedCount = 0;
	int movedCount = 0;
	int releasedCount = 0;

	public Issue33690()
	{
		InitializeUI();
	}

	private void InitializeUI()
	{
		var stackLayout = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 10
		};

		var instructionLabel = new Label
		{
			Text = "Tap and drag on the image below. All three event counters should increment.",
			AutomationId = "InstructionLabel"
		};

		pointerPressedLabel = new Label
		{
			Text = $"Pointer Pressed: {pressedCount}",
			AutomationId = "PointerPressedLabel"
		};

		pointerMovedLabel = new Label
		{
			Text = $"Pointer Moved: {movedCount}",
			AutomationId = "PointerMovedLabel"
		};

		pointerReleasedLabel = new Label
		{
			Text = $"Pointer Released: {releasedCount}",
			AutomationId = "PointerReleasedLabel"
		};

		var testImage = new Image
		{
			Source = "dotnet_bot.png",
			WidthRequest = 300,
			HeightRequest = 300,
			BackgroundColor = Colors.LightGray,
			AutomationId = "TestImage"
		};

		var pointerGestureRecognizer = new PointerGestureRecognizer();
		pointerGestureRecognizer.PointerPressed += OnPointerPressed;
		pointerGestureRecognizer.PointerMoved += OnPointerMoved;
		pointerGestureRecognizer.PointerReleased += OnPointerReleased;
		testImage.GestureRecognizers.Add(pointerGestureRecognizer);

		stackLayout.Children.Add(instructionLabel);
		stackLayout.Children.Add(pointerPressedLabel);
		stackLayout.Children.Add(pointerMovedLabel);
		stackLayout.Children.Add(pointerReleasedLabel);
		stackLayout.Children.Add(testImage);

		Content = stackLayout;
	}

	private void OnPointerPressed(object sender, PointerEventArgs e)
	{
		pressedCount++;
		pointerPressedLabel.Text = $"Pointer Pressed: {pressedCount}";
	}

	private void OnPointerMoved(object sender, PointerEventArgs e)
	{
		movedCount++;
		pointerMovedLabel.Text = $"Pointer Moved: {movedCount}";
	}

	private void OnPointerReleased(object sender, PointerEventArgs e)
	{
		releasedCount++;
		pointerReleasedLabel.Text = $"Pointer Released: {releasedCount}";
	}
}
