namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2963, "Disabling Editor in iOS does not disable entry of text")]
	public class Issue2963 : TestContentPage
	{
		readonly string _editorId = "DisabledEditor";
		readonly string _focusedLabelId = "FocusedLabel";

		protected override void Init()
		{

			var disabledEditor = new Editor
			{
				AutomationId = _editorId,
				Text = "You should not be able to edit me",
				IsEnabled = false
			};

			BindingContext = disabledEditor;
			var focusedLabel = new Label
			{
				AutomationId = _focusedLabelId
			};
			focusedLabel.SetBinding(Label.TextProperty, "IsFocused");

			Content = new StackLayout
			{
				Children = {
					disabledEditor,
					focusedLabel,
				}
			};
		}
	}
}
