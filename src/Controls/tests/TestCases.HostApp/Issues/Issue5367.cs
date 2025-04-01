namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 5367, "[Bug] Editor with MaxLength", PlatformAffected.Android)]
	public class Issue5367 : TestContentPage
	{
		const string MaxLengthEditor = "MaxLengthEditor";
		const string ForceBigStringButton = "StringButton";

		protected override void Init()
		{
			var maxLength = 14;
			var editor = new Editor()
			{
				AutomationId = MaxLengthEditor,
				Text = $"MaxLength = {maxLength}",
				MaxLength = maxLength,
			};

			Content = new ScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						editor,
						new Button()
						{
							AutomationId = ForceBigStringButton,
							Text = "Force editor text greater than maxlength",
							Command = new Command(()=> editor.Text += $"This should not appear on editor")
						}
					}
				}
			};
		}
	}
}
