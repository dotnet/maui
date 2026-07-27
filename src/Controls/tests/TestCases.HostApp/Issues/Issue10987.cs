namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 10987, "Editor HorizontalTextAlignment Does not Works.", PlatformAffected.All)]
public class Issue10987 : TestContentPage
{
	Editor lTReditor, rTLeditor;
	Button updateAlignmentButton, resetAlignmentButton;
	protected override void Init()
	{

		updateAlignmentButton = CreateButton("UpdateAlignmentButton", "Update alignment");
		resetAlignmentButton = CreateButton("ResetAlignmentButton", "Reset alignment");

		updateAlignmentButton.Clicked += OnUpdateAlignmentButtonClicked;
		resetAlignmentButton.Clicked += OnResetAlignmentButtonClicked;

		lTReditor = CreateEditor("LTREditor", "placeholder text", FlowDirection.LeftToRight);
		rTLeditor = CreateEditor("RTLEditor", "placeholder text", FlowDirection.RightToLeft);

		Content = new VerticalStackLayout
		{
			Spacing = 25,
			VerticalOptions = LayoutOptions.Center,
			Children =
				{
					updateAlignmentButton,
					resetAlignmentButton,
					lTReditor,
					rTLeditor
				}
		};
	}

	Button CreateButton(string automationId, string text)
	{
		Button button = new Button();
		button.AutomationId = automationId;
		button.Text = text;
		return button;
	}

	Editor CreateEditor(string automationId, string placeholderText, FlowDirection flowDirection)
	{
		Editor editor = new Editor();
		editor.AutomationId = automationId;
		editor.BackgroundColor = Colors.LightBlue;
		editor.Placeholder = placeholderText;
		editor.HeightRequest = 250;
		editor.WidthRequest = 220;
		editor.FlowDirection = flowDirection;
		return editor;
	}

	private void OnResetAlignmentButtonClicked(object sender, EventArgs e)
	{
		lTReditor.VerticalTextAlignment = TextAlignment.Start;
		lTReditor.HorizontalTextAlignment = TextAlignment.Start;
		lTReditor.Unfocus();

		rTLeditor.VerticalTextAlignment = TextAlignment.Start;
		rTLeditor.HorizontalTextAlignment = TextAlignment.Start;
		rTLeditor.Unfocus();
	}

	private void OnUpdateAlignmentButtonClicked(object sender, EventArgs e)
	{
		lTReditor.VerticalTextAlignment = TextAlignment.End;
		lTReditor.HorizontalTextAlignment = TextAlignment.End;

		rTLeditor.VerticalTextAlignment = TextAlignment.End;
		rTLeditor.HorizontalTextAlignment = TextAlignment.End;
	}
}